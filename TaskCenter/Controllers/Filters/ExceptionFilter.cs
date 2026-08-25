using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using ModelCore.DTOs;
using TaskCenter.Models;

namespace TaskCenter.Controllers.Filters
{
    public class ExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ExceptionFilter> _logger;

        public ExceptionFilter(ILogger<ExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext filterContext)
        {
            var actionContext = filterContext.HttpContext.RequestServices.GetRequiredService<IActionContextAccessor>().ActionContext;
            if (actionContext != null)
            {
                var urlHelper = new UrlHelper(actionContext);
                //IUrlHelper urlHelper = new UrlHelper(new ActionContext(filterContext.HttpContext, filterContext.RouteData, filterContext.ActionDescriptor));
                //var urlHelper = filterContext.HttpContext.RequestServices.GetRequiredService<IUrlHelper>();
            }

            if (filterContext.Exception != null)
            {
                _logger.LogError(filterContext.Exception, filterContext.Exception.ToString());

                //ViewDataDictionary viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                //{
                //    Model = new ErrorViewModel
                //    {
                //        Exception = filterContext.Exception,
                //        RequestId = Activity.Current?.Id ?? filterContext.HttpContext.TraceIdentifier
                //    }
                //};
                //filterContext.Result = new ViewResult
                //{
                //    ViewName = "~/Views/Shared/Error.cshtml",
                //    ViewData = viewData,
                //};
                filterContext.ExceptionHandled = true;
                filterContext.Result = new JsonResult
                    (new BaseResponseDto
                    {
                        Success = false,
                        Result = false,
                        Message = filterContext.Exception?.Message ?? "An unexpected error occurred.",
                    })
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    };
            }
        }
    }

}
