using CommonLib.Core.DataWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ModelCore.DataEntity;
using System.Globalization;

namespace TaskCenter.Core
{
    public class SampleController : CommonLib.Core.Controllers.SampleController
    {
        protected internal ModelSource? _dataSource;

        protected internal bool _dbInstance;
        protected internal GenericDbContext<ApplicationDbContext>? models;
        protected ApplicationDbContext? db;


        public SampleController(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider)
        {
            Logger = loggerFactory.CreateLogger(this.GetType());
            db = serviceProvider.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;
        }

        public ILogger Logger
        {
            get;
            private set;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_dbInstance && models != null)
            {
                models.Dispose();
            }
        }

        public ModelSource DataSource => _dataSource!;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            models = HttpContext.Items["__DB_Instance"] as GenericDbContext<ApplicationDbContext>;
            if (models == null)
            {
                models = new GenericDbContext<ApplicationDbContext>(db);
                _dbInstance = true;
                HttpContext.Items["__DB_Instance"] = models;
            }

            _dataSource = new ModelSource(models);
            HttpContext.Items["Models"] = DataSource;
            HttpContext.Items["Controller"] = this;

            var lang = Request.Cookies["cLang"];
            if (lang != null)
            {
                var cultureInfo = new CultureInfo(lang);
                Thread.CurrentThread.CurrentUICulture = cultureInfo;
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(cultureInfo.Name);
                ViewBag.Lang = lang;
            }
        }
    }

}
