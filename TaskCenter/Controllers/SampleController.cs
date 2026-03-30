using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
//using System.Web.Security;

using TaskCenter.Properties;
using ModelCore.DataEntity;
using ModelCore.Models.ViewModel;
using ModelCore.Helper;
using CommonLib.Utility;
using CommonLib.Core.DataWork;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Globalization;
using CommonLib.Core.Utility;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace TaskCenter.Controllers
{
    public class SampleController : CommonLib.Core.Controllers.SampleController
    {
        // GET: Sample
        protected internal ModelSource? _dataSource;

        protected internal bool _dbInstance;
        protected internal GenericDbContext<ApplicationDbContext>? models;

        public SampleController(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider)
        {
            Logger = loggerFactory.CreateLogger(this.GetType());
        }

        public ILogger Logger
        {
            get;
            private set;
        }

        public ModelSource DataSource => _dataSource!;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            models = HttpContext.Items["__DB_Instance"] as GenericDbContext<ApplicationDbContext>;
            if (models == null)
            {
                models = new GenericDbContext<ApplicationDbContext>();
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


        [NonAction]
        public int DecryptKeyValue(QueryViewModel viewModel, out bool expired)
        {
            int keyID = viewModel.DecryptKeyValue(out long ticks);
            expired = (DateTime.Now.Ticks - ticks) > Settings.Default.TimeoutTicks;
            return keyID;
        }

    }
}