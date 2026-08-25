using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using ModelCore.Models.ViewModel;
using Newtonsoft.Json;

namespace TaskCenter.Core
{
    /// <summary>
    /// 對外收單 API（<c>InvoiceService</c>）沿用 Newtonsoft.Json 解析請求內容，維持與舊版
    /// <c>SampleController.FromJsonBody&lt;T&gt;()</c> 完全一致的寬鬆行為：
    /// 空字串視為 null、字串可轉為數值、屬性名稱不分大小寫、日期格式不限 ISO 8601。
    /// <para>
    /// System.Text.Json 對上述情形一律判為格式錯誤（例如 <c>"CustomsClearanceMark": ""</c> 轉
    /// <c>byte?</c>、<c>"SalesAmount": "107"</c> 轉 <c>decimal</c>），既有客戶端送出的資料會直接被擋下，
    /// 因此改由本格式器接手。僅對 <see cref="InvoiceRequestViewModel"/> 生效，其餘型別（SPA 專用的
    /// DTO）仍由 System.Text.Json 處理，不受影響。
    /// </para>
    /// </summary>
    public class LegacyJsonInputFormatter : TextInputFormatter
    {
        public LegacyJsonInputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/json"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/*+json"));
            SupportedEncodings.Add(UTF8EncodingWithoutBOM);
            SupportedEncodings.Add(UTF16EncodingLittleEndian);
        }

        protected override bool CanReadType(Type type)
        {
            return type == typeof(InvoiceRequestViewModel);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            var request = context.HttpContext.Request;

            //OnActionExecuted 的 request dump 會再讀一次 Body，讀取前後都需歸位（已由中介層 EnableBuffering）
            if (request.Body.CanSeek)
            {
                request.Body.Position = 0;
            }

            String body;
            using (StreamReader reader = new StreamReader(request.Body, encoding, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
            }

            if (request.Body.CanSeek)
            {
                request.Body.Position = 0;
            }

            if (String.IsNullOrWhiteSpace(body))
            {
                return await InputFormatterResult.NoValueAsync();
            }

            try
            {
                object? model = JsonConvert.DeserializeObject(body, context.ModelType);
                return model == null
                    ? await InputFormatterResult.NoValueAsync()
                    : await InputFormatterResult.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                context.ModelState.TryAddModelError(BuildErrorKey(context.ModelName, ex), ex, context.Metadata);
                return await InputFormatterResult.FailureAsync();
            }
        }

        /// <summary>
        /// 以出錯位置的 JSON 路徑作為 ModelState 索引鍵，格式比照 System.Text.Json 的
        /// <c>$.A.B[0].C</c>，讓錯誤訊息能指出是哪個欄位（Newtonsoft 的 Path 不含 <c>$.</c> 前綴）。
        /// </summary>
        private static String BuildErrorKey(String modelName, Exception ex)
        {
            String? path = (ex as JsonReaderException)?.Path ?? (ex as JsonSerializationException)?.Path;
            if (String.IsNullOrEmpty(path))
            {
                return modelName;
            }

            String jsonPath = String.Concat("$.", path);
            return String.IsNullOrEmpty(modelName) ? jsonPath : String.Concat(modelName, ".", jsonPath);
        }
    }
}
