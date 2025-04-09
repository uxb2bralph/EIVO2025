using CommonLib.Core.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace CommonLib.Core.Controllers
{
    public class SampleController : Controller
    {
        protected SampleController(IServiceProvider serviceProvider) : base()
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider
        {
            get;
            private set;
        }

        private IViewRenderService? _viewRenderService;

        public async Task<string> RenderViewToStringAsync(String viewName, object model)
        {
            if (_viewRenderService == null)
            {
                _viewRenderService = ServiceProvider.GetRequiredService<IViewRenderService>();
                _viewRenderService.HttpContext = this.HttpContext;
                //_viewRenderService.ActionContext = ServiceProvider.GetRequiredService<IActionContextAccessor>().ActionContext;
            }
            return await _viewRenderService.RenderToStringAsync(viewName, model);
        }

        protected async Task<string> DumpAsync(bool includeHeader = true)
        {
            String fileName = Path.Combine(FileLogger.Logger.LogDailyPath, $"request{DateTime.Now.Ticks}.txt");
            await Request.SaveAsAsync(fileName, includeHeader);
            return fileName;
        }

        String? _reqeustBody;
        public String RequestBody
        {
            get
            {
                if (_reqeustBody == null)
                {
                    var t = Request.GetRequestBodyAsync();
                    t.Wait();
                    _reqeustBody = t.Result;
                }
                return _reqeustBody;
            }
        }

        public async Task<T> PrepareViewModelAsync<T>()
            where T : class
        {
            T? viewModel;
            if (Request.ContentType?.Contains("application/json", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                viewModel = JsonConvert.DeserializeObject<T>(RequestBody)!;
            }
            else
            {
                viewModel = Activator.CreateInstance<T>();
                await this.TryUpdateModelAsync<T>(viewModel);
            }

            ViewBag.ViewModel = viewModel;
            return viewModel;
        }

        public T? FromJsonBody<T>()
            where T : class
        {
            return JsonConvert.DeserializeObject<T>(RequestBody);
        }

        public void BuildViewModel(object viewModel)
        {
            var t = this.TryUpdateModelAsync(viewModel, viewModel.GetType(), String.Empty);
            t.Wait();
        }

        protected ViewEngineResult CheckView(String actionName)
        {
            ICompositeViewEngine viewEngine = ServiceProvider.GetRequiredService<ICompositeViewEngine>();
            ViewEngineResult viewResult = viewEngine.GetView("~/", $"/Views/{RouteData.Values["controller"]}/{actionName}.cshtml", isMainPage: false);
            if (!viewResult.Success)
            {
                viewResult = viewEngine.GetView("~/", $"/Views/{RouteData.Values["controller"]}/Module/{actionName}.cshtml", isMainPage: false);
            }
            if (!viewResult.Success)
            {
                viewResult = viewEngine.GetView("~/", $"/Views/{RouteData.Values["controller"]}/Page/{actionName}.cshtml", isMainPage: false);
            }
            if (!viewResult.Success)
            {
                viewResult = viewEngine.GetView("~/", $"/Views/{RouteData.Values["controller"]}/Page/Module/{actionName}.cshtml", isMainPage: false);
            }
            return viewResult;
        }

        [AllowAnonymous]
        public ActionResult HandleUnknownAction(string actionName, IFormCollection forms)
        {
            ViewEngineResult viewResult = CheckView(actionName);

            if (viewResult.Success)
            {
                // 视图存在
                return View(viewResult.ViewName, forms);
            }
            else
            {
                return NotFound();
            }
            //this.View(actionName).ExecuteResult(this.ControllerContext);
        }
    }

}
