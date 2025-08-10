using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;
using System.Collections.Specialized;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

using ClosedXML.Excel;
using WebHome.Helper;
using WebHome.Models;
using WebHome.Models.ViewModel;
using ModelCore.Models.ViewModel;
using WebHome.Properties;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;

using CommonLib.Utility;
using CommonLib.DataAccess;
using Newtonsoft.Json;

namespace WebHome.Controllers.Handler
{
    public class _TestController : SampleController<InvoiceItem>
    {
        public _TestController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // GET: _Testing
        public string ConvertFormCollectionToMimeString(IEnumerable<KeyValuePair<String,StringValues>> formCollection)
        {
            var sb = new StringBuilder();

            foreach (var item in formCollection)
            {
                var values = item.Value;
                foreach (var value in values)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("&");
                    }
                    sb.Append(Uri.EscapeDataString(item.Key));
                    sb.Append("=");
                    sb.Append(Uri.EscapeDataString(value));
                }
            }

            return sb.ToString();
        }
        public ActionResult Test01()
        {
            var formValues = Request.Form.ToList();
            formValues.Add(new KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>("test01", "hello..."));
            return Content(ConvertFormCollectionToMimeString(formValues));
        }

        public ActionResult ReloadSettings()
        {
            AppSettings.Reload();
            ModelCore.Properties.AppSettings.Reload();
            ModelExtension.Properties.AppSettings.Reload();
            CommonLib.Core.Properties.AppSettings.Reload();
            CommonLib.Logger.Properties.AppSettings.Reload();
            ExternalPdfWrapper.AppSettings.Reload();

            return Content(AppSettings.AllSettings.JsonStringify(), "application/json");
        }

        public ActionResult SaveSettings()
        {
            //AppSettings.SaveAll();
            AppSettings.Default.Save();
            ModelCore.Properties.AppSettings.Default.Save();
            ModelExtension.Properties.AppSettings.Default.Save();
            CommonLib.Core.Properties.AppSettings.Default.Save();
            CommonLib.Logger.Properties.AppSettings.Default.Save();
            ExternalPdfWrapper.AppSettings.Default.Save();
            return Content(AppSettings.AllSettings?.ToString(), "application/json");
        }

        public ActionResult AllSettings()
        {
            return Content(AppSettings.AllSettings?.ToString(), "application/json");
        }

    }
}