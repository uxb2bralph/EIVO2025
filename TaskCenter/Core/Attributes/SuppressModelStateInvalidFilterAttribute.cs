using Microsoft.AspNetCore.Mvc.Filters;

namespace TaskCenter.Core.Attributes
{
    /// <summary>
    /// 停用套用對象的 [ApiController] 自動模型驗證（<c>ModelStateInvalidFilter</c>，Order = -2000）。
    /// <para>
    /// 對外收單 API 必須維持 HTTP 200 + <c>Root</c> 的回應格式，不能被自動驗證換成
    /// <c>ValidationProblemDetails</c>，也不能在 action 執行前就被短路。因此本過濾器以更前面的
    /// Order 執行，先把繫結錯誤的中文訊息存入 <see cref="Microsoft.AspNetCore.Http.HttpContext.Items"/>，
    /// 再清空 ModelState 讓自動驗證放行，由 action 自行以該訊息回報。
    /// </para>
    /// <para>
    /// 只影響掛上本屬性的控制器／動作，其餘 SPA API 仍維持自動驗證
    /// （回應內容由 <c>ApiBehaviorOptions.InvalidModelStateResponseFactory</c> 在地化）。
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class SuppressModelStateInvalidFilterAttribute : ActionFilterAttribute
    {
        /// <summary>暫存繫結錯誤訊息的 <c>HttpContext.Items</c> 索引鍵。</summary>
        public const String ModelStateErrorKey = "TaskCenter.ModelStateError";

        public SuppressModelStateInvalidFilterAttribute()
        {
            //必須早於 ModelStateInvalidFilter（Order = -2000）執行
            Order = -2100;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.HttpContext.Items[ModelStateErrorKey] = ApiValidationResponse.DescribeMessage(context.ModelState);
                context.ModelState.Clear();
            }
        }

        /// <summary>取出本過濾器暫存的繫結錯誤訊息；沒有錯誤時回傳 null。</summary>
        public static String? GetModelStateError(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            return httpContext.Items.TryGetValue(ModelStateErrorKey, out var message)
                ? message as String
                : null;
        }
    }
}
