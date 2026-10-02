using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebHome.Infrastructure.ModelBinding
{
    /// <summary>
    /// 請求是 JSON 就從 Body 讀，否則走 form／query／route 的值提供者。
    /// </summary>
    public sealed class JsonOrFormModelBinder : IModelBinder
    {
        private readonly IModelBinder _bodyBinder;
        private readonly IModelBinder _valueBinder;

        public JsonOrFormModelBinder(IModelBinder bodyBinder, IModelBinder valueBinder)
        {
            _bodyBinder = bodyBinder;
            _valueBinder = valueBinder;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            ArgumentNullException.ThrowIfNull(bindingContext);

            var request = bindingContext.HttpContext.Request;
            // ContentLength 為 0 時代表沒有 body，交給值提供者避免輸入格式器報錯。
            if (!request.HasJsonContentType() || request.ContentLength == 0)
            {
                return _valueBinder.BindModelAsync(bindingContext);
            }

            // 簡單型別（String、int…）不能直接把整份 JSON 物件反序列化成值，
            // 例如 $.postJSON(url, { 'term': x }) 對應的是 String term 參數，
            // 因此改成從 JSON 物件取同名屬性。
            return bindingContext.ModelMetadata.IsComplexType
                ? BindComplexValueFromJsonBodyAsync(bindingContext)
                : BindSimpleValueFromJsonBodyAsync(bindingContext);
        }

        /// <summary>
        /// 把 JSON body 攤平成值提供者後走一般 MVC 繫結，而非交給 System.Text.Json 反序列化。
        /// </summary>
        /// <remarks>
        /// 舊版頁面的 JSON 由 <c>serializeObject()</c> 從 DOM 蒐集，所有值都是字串：
        /// <c>"ProcessType":"41"</c>（enum）、<c>"InvoiceDateFrom":"2026/08/01"</c>（非 ISO 8601 日期）、
        /// <c>"Cancelled":""</c>（可為 null 的 bool／int）。
        /// 輸入格式器對這些一律丟 JsonException，整個 viewModel 會變 null 並回報
        /// 「The viewModel field is required.」；改走值提供者後轉型由 TypeConverter 處理，
        /// 與同一個 action 的 form post 語意完全一致。
        /// </remarks>
        private async Task BindComplexValueFromJsonBodyAsync(ModelBindingContext bindingContext)
        {
            var json = await ReadBodyAsync(bindingContext.HttpContext.Request);
            if (string.IsNullOrWhiteSpace(json))
            {
                await _valueBinder.BindModelAsync(bindingContext);
                return;
            }

            JsonElement root;
            try
            {
                using var document = JsonDocument.Parse(json);
                root = document.RootElement.Clone();
            }
            catch (JsonException)
            {
                // JSON 本身就壞掉時交回輸入格式器，沿用它的錯誤訊息。
                await _bodyBinder.BindModelAsync(bindingContext);
                return;
            }

            var jsonValueProvider = JsonBodyValueProvider.TryCreate(root);
            if (jsonValueProvider == null)
            {
                await _bodyBinder.BindModelAsync(bindingContext);
                return;
            }

            // 保留原本的 query／route 值提供者：舊版有 $.postJSON(url + '?id=5', {...}) 這種混用寫法，
            // body 的值優先。
            var originalValueProvider = bindingContext.ValueProvider;
            var originalModelName = bindingContext.ModelName;
            bindingContext.ValueProvider = new CompositeValueProvider
            {
                jsonValueProvider,
                originalValueProvider,
            };

            // ParameterBinder 決定前綴時只看得到 form／query／route，JSON 請求下一律判成空前綴。
            // 物件型別無妨（屬性本來就不帶前綴），但 int[] chkItem 這類集合會去找 "[0]" 而非
            // "chkItem"／"chkItem[0]"，結果繫結成空陣列；因此比照 ParameterBinder 以 JSON 內容補判一次。
            // 補判後 CollectionModelBinder 對 "chkItem":"1" 與 "chkItem":["1","2"] 都能繫結。
            if (bindingContext.IsTopLevelObject
                && string.IsNullOrEmpty(originalModelName)
                && !string.IsNullOrEmpty(bindingContext.FieldName)
                && jsonValueProvider.ContainsPrefix(bindingContext.FieldName))
            {
                bindingContext.ModelName = bindingContext.FieldName;
            }

            try
            {
                await _valueBinder.BindModelAsync(bindingContext);
            }
            finally
            {
                bindingContext.ValueProvider = originalValueProvider;
                bindingContext.ModelName = originalModelName;
            }
        }

        private async Task BindSimpleValueFromJsonBodyAsync(ModelBindingContext bindingContext)
        {
            var json = await ReadBodyAsync(bindingContext.HttpContext.Request);

            if (string.IsNullOrWhiteSpace(json))
            {
                await _valueBinder.BindModelAsync(bindingContext);
                return;
            }

            JsonElement element;
            try
            {
                using var document = JsonDocument.Parse(json);
                element = document.RootElement.Clone();
            }
            catch (JsonException)
            {
                await _valueBinder.BindModelAsync(bindingContext);
                return;
            }

            if (element.ValueKind == JsonValueKind.Object
                && !TryGetProperty(element, bindingContext.FieldName, out element))
            {
                // JSON 裡沒有同名屬性時退回值提供者，
                // 例如 $.postJSON(url + '?id=5', {...}) 這種 body 與 query 混用的呼叫。
                await _valueBinder.BindModelAsync(bindingContext);
                return;
            }

            if (element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return;
            }

            var raw = element.ValueKind == JsonValueKind.String
                ? element.GetString()
                : element.GetRawText();

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, raw, raw);

            try
            {
                bindingContext.Result = ModelBindingResult.Success(ConvertTo(raw, bindingContext.ModelType));
            }
            catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException or NotSupportedException)
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, ex.Message);
            }
        }

        private static async Task<string> ReadBodyAsync(HttpRequest request)
        {
            if (!request.Body.CanSeek)
            {
                request.EnableBuffering();
            }

            var position = request.Body.Position;
            request.Body.Position = 0;
            string json;
            using (var reader = new StreamReader(request.Body, leaveOpen: true))
            {
                json = await reader.ReadToEndAsync();
            }
            request.Body.Position = position;

            return json;
        }

        private static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
        {
            if (element.TryGetProperty(name, out value))
            {
                return true;
            }

            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        private static object? ConvertTo(string? raw, Type modelType)
        {
            var targetType = Nullable.GetUnderlyingType(modelType) ?? modelType;

            if (raw == null)
            {
                return null;
            }

            if (targetType == typeof(string))
            {
                return raw;
            }

            if (targetType == typeof(byte[]))
            {
                return Convert.FromBase64String(raw);
            }

            var converter = TypeDescriptor.GetConverter(targetType);
            return converter.ConvertFromString(null, CultureInfo.InvariantCulture, raw);
        }
    }
}
