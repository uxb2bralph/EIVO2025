using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;

namespace WebHome.Infrastructure.ModelBinding
{
    /// <summary>
    /// 把 JSON Request Body 攤平成「欄位名稱 → 字串值」的值提供者，
    /// 讓 <see cref="FromJsonOrFormAttribute"/> 的 JSON 請求可以走一般 MVC 值提供者繫結。
    /// </summary>
    /// <remarks>
    /// 舊版頁面的 JSON 都是 <c>serializeObject()</c> 由 DOM 蒐集而來，所有值本質上都是字串
    /// （<c>"ProcessType":"41"</c>、<c>"InvoiceDateFrom":"2026/08/01"</c>、<c>"Cancelled":""</c>），
    /// 直接交給 System.Text.Json 反序列化會因型別過嚴而整個 viewModel 變成 null。
    /// 改走值提供者後，轉型交由 <c>TypeConverter</c>／<c>TryParse</c> 處理，
    /// 與同一個 action 的 form post 完全同一套語意。
    /// <para>
    /// BindingSource 刻意用 <see cref="BindingSource.Form"/>：
    /// <see cref="FromJsonOrFormAttribute.JsonOrFormBindingSource"/> 是 Path／Query／Form 的組合，
    /// 巢狀屬性若標了 <c>[FromForm]</c> 之類的來源而觸發過濾時，本提供者才不會被濾掉。
    /// </para>
    /// </remarks>
    public sealed class JsonBodyValueProvider : BindingSourceValueProvider
    {
        private readonly IReadOnlyDictionary<string, StringValues> _values;
        private readonly HashSet<string> _prefixes;

        private JsonBodyValueProvider(IReadOnlyDictionary<string, StringValues> values, HashSet<string> prefixes)
            : base(BindingSource.Form)
        {
            _values = values;
            _prefixes = prefixes;
        }

        /// <summary>
        /// 由 JSON 根項目建立值提供者；根項目不是物件或陣列時回 null（交由呼叫端改走輸入格式器）。
        /// </summary>
        public static JsonBodyValueProvider? TryCreate(JsonElement root)
        {
            if (root.ValueKind is not (JsonValueKind.Object or JsonValueKind.Array))
            {
                return null;
            }

            var values = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
            Flatten(root, string.Empty, values);

            var prefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { string.Empty };
            foreach (var key in values.Keys)
            {
                AddPrefixes(prefixes, key);
            }

            return new JsonBodyValueProvider(values, prefixes);
        }

        public override bool ContainsPrefix(string prefix)
        {
            ArgumentNullException.ThrowIfNull(prefix);
            return _prefixes.Contains(prefix);
        }

        public override ValueProviderResult GetValue(string key)
        {
            ArgumentNullException.ThrowIfNull(key);

            if (key.Length > 0 && _values.TryGetValue(key, out var value))
            {
                // 與 FormValueProvider 一致採用 CurrentCulture，日期等格式才會和 form post 解讀相同。
                return new ValueProviderResult(value, CultureInfo.CurrentCulture);
            }

            return ValueProviderResult.None;
        }

        /// <summary>
        /// 物件用 <c>a.b</c>、陣列用 <c>a[0]</c> 攤平成 MVC 值提供者認得的鍵。
        /// </summary>
        private static void Flatten(JsonElement element, string prefix, IDictionary<string, StringValues> values)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        Flatten(
                            property.Value,
                            prefix.Length == 0 ? property.Name : $"{prefix}.{property.Name}",
                            values);
                    }
                    break;

                case JsonValueKind.Array:
                    var index = 0;
                    foreach (var item in element.EnumerateArray())
                    {
                        Flatten(item, $"{prefix}[{index++}]", values);
                        // 陣列元素若是物件／陣列會再往下攤平，純量則直接落在 a[0]。
                    }

                    // 純量陣列另以 a 本身存一份多值，比照 form post 的 chkItem=1&chkItem=2。
                    // CollectionModelBinder 取得 a 的值時走 BindSimpleCollection，
                    // 否則改走 a[0]、a[1]… 的索引繫結，超過 MvcOptions.MaxModelBindingCollectionSize（1024）會丟例外
                    // （例如列印時勾選逾千筆發票）。
                    if (prefix.Length > 0 && TryGetScalarValues(element, out var scalars))
                    {
                        values[prefix] = scalars;
                    }
                    break;

                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    // 視同未提供，讓可為 null 的屬性維持預設值，非可為 null 的屬性也不會多出驗證錯誤。
                    break;

                case JsonValueKind.String:
                    if (prefix.Length > 0)
                    {
                        values[prefix] = element.GetString();
                    }
                    break;

                default:
                    // 數字與 true／false 取原始文字，bool.TryParse／數值 TryParse 都吃得下。
                    if (prefix.Length > 0)
                    {
                        values[prefix] = element.GetRawText();
                    }
                    break;
            }
        }

        /// <summary>
        /// 陣列元素全為純量（null 略過）時回傳其字串值；含物件／陣列或為空陣列時回 false。
        /// </summary>
        private static bool TryGetScalarValues(JsonElement array, out StringValues values)
        {
            var list = new List<string?>(array.GetArrayLength());
            foreach (var item in array.EnumerateArray())
            {
                switch (item.ValueKind)
                {
                    case JsonValueKind.Object:
                    case JsonValueKind.Array:
                        values = StringValues.Empty;
                        return false;

                    case JsonValueKind.Null:
                    case JsonValueKind.Undefined:
                        break;

                    case JsonValueKind.String:
                        list.Add(item.GetString());
                        break;

                    default:
                        list.Add(item.GetRawText());
                        break;
                }
            }

            values = new StringValues(list.ToArray());
            return list.Count > 0;
        }

        /// <summary>
        /// 由 <c>a.b[0].c</c> 推出 <c>a</c>、<c>a.b</c>、<c>a.b[0]</c>、<c>a.b[0].c</c> 等可用前綴。
        /// </summary>
        private static void AddPrefixes(HashSet<string> prefixes, string key)
        {
            for (var i = key.Length - 1; i > 0; i--)
            {
                if (key[i] is '.' or '[')
                {
                    prefixes.Add(key[..i]);
                }
            }

            prefixes.Add(key);
        }
    }
}
