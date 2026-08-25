using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModelCore.DTOs;

namespace TaskCenter.Core
{
    /// <summary>
    /// 模型驗證（含 [FromBody] 請求內容反序列化）失敗時的中文訊息與回應格式。
    /// <para>
    /// System.Text.Json 反序列化失敗的訊息（例如 "The JSON value could not be converted to
    /// System.Nullable`1[System.Byte]. Path: ... | LineNumber: ..."）來自 <see cref="JsonException"/>，
    /// 不存在於任何資源檔中，無法以在地化資源翻譯，只能在此改寫。
    /// </para>
    /// </summary>
    public static class ApiValidationResponse
    {
        public const String DefaultMessage = "請求內容格式不正確!!";

        /// <summary>將 ModelState 內的錯誤轉為中文訊息；無錯誤時回傳空陣列。</summary>
        public static String[] Describe(ModelStateDictionary modelState)
        {
            return modelState
                .Where(e => e.Value != null && e.Value.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors.Select(error => Describe(e.Key, error)))
                .Distinct()
                .ToArray();
        }

        /// <summary>將 ModelState 內的錯誤合併為單一則中文訊息。</summary>
        public static String DescribeMessage(ModelStateDictionary modelState, String defaultMessage = DefaultMessage)
        {
            String[] errors = Describe(modelState);
            return errors.Length > 0 ? String.Join("、", errors) : defaultMessage;
        }

        /// <summary>
        /// 供 <c>ApiBehaviorOptions.InvalidModelStateResponseFactory</c> 使用，
        /// 取代預設英文的 <c>ValidationProblemDetails</c>，改回與
        /// <see cref="Attributes.ValidateModelAttribute"/> 一致的 <see cref="BaseResponseDto"/>。
        /// </summary>
        public static IActionResult Create(ActionContext context)
        {
            String[] errors = Describe(context.ModelState);
            return new BadRequestObjectResult(new BaseResponseDto
            {
                Success = false,
                Result = false,
                Message = errors.Length > 0 ? String.Join("、", errors) : DefaultMessage,
                Errors = errors,
            });
        }

        private static String Describe(String key, ModelError error)
        {
            String field = FieldName(key);

            //JSON 反序列化失敗：訊息夾帶 Path/LineNumber 等英文內容，改寫為欄位層級的中文說明，
            //並保留原始訊息供客戶端定位出錯的 JSON 路徑與行號
            if (error.Exception is JsonException || error.Exception is Newtonsoft.Json.JsonException)
            {
                return Combine(field.Length > 0
                    ? String.Format("欄位 {0} 的資料格式不正確!!", field)
                    : DefaultMessage, Detail(error));
            }

            //其他例外型錯誤，ErrorMessage 會是空字串（MVC 刻意不外洩例外內容）
            if (String.IsNullOrEmpty(error.ErrorMessage))
            {
                return Combine(field.Length > 0
                    ? String.Format("欄位 {0} 資料錯誤!!", field)
                    : "請求資料錯誤!!", Detail(error));
            }

            return error.ErrorMessage;
        }

        /// <summary>
        /// 取原始（未在地化）的錯誤內容。System.Text.Json 由輸入格式器寫入 ErrorMessage，
        /// Newtonsoft 則只帶例外本身（見 <see cref="LegacyJsonInputFormatter"/>）。
        /// </summary>
        private static String? Detail(ModelError error)
        {
            String? detail = String.IsNullOrWhiteSpace(error.ErrorMessage)
                ? error.Exception?.Message
                : error.ErrorMessage;
            return String.IsNullOrWhiteSpace(detail) ? null : detail!.Trim();
        }

        private static String Combine(String message, String? detail)
        {
            return detail == null ? message : String.Format("{0}（{1}）", message, detail);
        }

        /// <summary>ModelState 的 key 可能是 JSON 路徑（$.A.B[0].C）或參數／屬性名稱。</summary>
        private static String FieldName(String key)
        {
            return key.StartsWith("$.") ? key.Substring(2) : key;
        }
    }
}
