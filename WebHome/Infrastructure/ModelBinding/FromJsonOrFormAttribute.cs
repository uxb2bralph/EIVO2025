using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebHome.Infrastructure.ModelBinding
{
    /// <summary>
    /// 依請求的 Content-Type 決定繫結來源：
    /// application/json 由 Request Body 反序列化，其餘（form-urlencoded、multipart、
    /// 或 GET 的 query string）走傳統 MVC 值提供者。
    /// </summary>
    /// <remarks>
    /// 舊版 ASP.NET MVC 的頁面同一個 action 可能同時被 <c>$.postJSON</c>／<c>doPost</c>（JSON）
    /// 與 <c>form.submit()</c>／<c>launchDownload</c>／選單連結（form-urlencoded 或 GET）呼叫，
    /// 單獨標 <c>[FromBody]</c> 會讓後者回 415，單獨移除又會讓前者收不到資料，
    /// 因此這類端點改用本屬性。
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class FromJsonOrFormAttribute : Attribute, IBindingSourceMetadata
    {
        /// <summary>
        /// 必須是 <see cref="CompositeBindingSource"/>：MVC 會依參數的 BindingSource
        /// 過濾值提供者（<c>DefaultModelBindingContext.CreateBindingContext</c>），
        /// 自訂的單一 BindingSource 不被 query／form／route 任何一個值提供者接受，
        /// 會讓非 JSON 的請求連 query string 都讀不到。
        /// </summary>
        public static readonly BindingSource JsonOrFormBindingSource =
            CompositeBindingSource.Create(
                new[] { BindingSource.Path, BindingSource.Query, BindingSource.Form },
                "JsonOrForm");

        public BindingSource BindingSource => JsonOrFormBindingSource;
    }
}
