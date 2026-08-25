using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using ModelCore.DTOs;
using ModelCore.Helper;

namespace TaskCenter.Core.Attributes
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 在動作執行前進行模型驗證
        /// </summary>
        /// <param name="context">動作執行上下文</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = ApiValidationResponse.Create(context);

                //var message = context.ModelState.ErrorMessage();
                //var errors = context.ModelState.Values
                //    .SelectMany(v => v.Errors.Select(e =>
                //    {
                //        return e.ErrorMessage;
                //    }));
                //context.Result = new BadRequestObjectResult(new BaseResponseDto
                //{
                //    Success = false,
                //    Result = false,
                //    Message = message,
                //    Errors = errors
                //});
            }
        }
    }

}
