using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using TaskCenter.Models;
using System.Diagnostics;

namespace TaskCenter.Controllers.Filters
{
    public class ExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ExceptionFilter> _logger;

        public ExceptionFilter()
        {
            _logger = LoggerFactory
                .Create(config => { })
                .CreateLogger<ExceptionFilter>();
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
                _logger.LogError(filterContext.Exception, filterContext.Exception.Message);

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
                    (new
                    {
                        result = false,
                        message = filterContext.Exception?.Message,
                    });
            }
        }
    }

}
