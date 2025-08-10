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
using System.Text;
using System.Threading;
using System.Web;
using Microsoft.AspNetCore.Mvc;



using ClosedXML.Excel;
using WebHome.Helper;
using WebHome.Models;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;

using CommonLib.Utility;
using ModelCore.Helper;
using CommonLib.Core.Utility;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;

namespace WebHome.Controllers
{
    public class TestAllController : SampleController<InvoiceItem>
    {
        public TestAllController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // GET: TestAll
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Test()
        {
            CommonLib.Core.Utility.FileLogger.Logger.Info("test...");
            return new EmptyResult();
        }

        public ActionResult NotifyEIVOPlatform(bool? reset)
        {
            if (reset == true)
            {
                EIVOTurnkeyFactory.ResetBusyCount();
            }
            //EIVONotificationFactory.Notify();
            return Json(EIVOTurnkeyFactory.CurrentStatus);
        }

        public ActionResult MatchAttachment()
        {
            GoogleInvoiceExtensionMethods.MatchGoogleInvoiceAttachment();
            return Content(DateTime.Now.ToString());
        }

        public ActionResult QRCode(String data, int? width, int? height, int? margin)
        {
            //if (!String.IsNullOrEmpty(data))
            //{
            //    using (Bitmap qrcode = data.CreateQRCode(width ?? 160, height ?? 160, margin ?? 0))
            //    {
            //        Response.Clear();
            //        Response.ContentType = "image/jpeg";
            //        qrcode.Save(Response.OutputStream, ImageFormat.Jpeg);
            //    }
            //}

            if (!String.IsNullOrEmpty(data))
            {
                using (Bitmap qrcode = data.CreateQRCode(width ?? 160, height ?? 160, margin ?? 0))
                {
                    Response.Clear();
                    Response.ContentType = "image/jpeg";
                    qrcode.Save(Response.Body, ImageFormat.Jpeg);
                }
            }
            return new EmptyResult();

        }

        public ActionResult BarCode(String data, int? width, int? height, int? margin, int? wide, int? narrow, String format)
        {
            data = $"{data.GetEfficientString()}";
            {
                using (Bitmap qrcode = data.GetCode39(false,wide,narrow,height,margin) /* data.CreateBarCode(width ?? 160, height ?? 160, margin ?? 0)*/)
                {
                    Response.Clear();
                    format = format.GetEfficientString();
                    format = format == null ? "bmp" : format.ToLower();
                    switch (format)
                    {
                        case "gif":
                            Response.ContentType = "image/gif";
                            qrcode.Save(Response.Body, ImageFormat.Gif);
                            break;
                        case "tif":
                            Response.ContentType = "image/tiff";
                            qrcode.Save(Response.Body, ImageFormat.Tiff);
                            break;
                        case "jpg":
                            Response.ContentType = "image/jpg";
                            qrcode.Save(Response.Body, ImageFormat.Jpeg);
                            break;
                        case "png":
                            Response.ContentType = "image/png";
                            qrcode.Save(Response.Body, ImageFormat.Png);
                            break;
                        case "bmp":
                            Response.ContentType = "image/bmp";
                            qrcode.Save(Response.Body, ImageFormat.Bmp);
                            break;
                        default:
                            Response.ContentType = "application/octet-stream";
                            qrcode.Save(Response.Body, ImageFormat.Bmp);
                            break;
                    }
                }
            }
            return new EmptyResult();

        }

        public async Task<ActionResult> CheckInvoiceNoAsync(POSDeviceViewModel viewModel)
        {
            //if (String.IsNullOrEmpty(Request.ContentType) && String.IsNullOrEmpty(Request.Params["Query_String"]))
            //{
            //    using (StreamReader reader = new StreamReader(Request.InputStream, Request.ContentEncoding))
            //    {
            //        viewModel = JsonConvert.DeserializeObject<POSDeviceViewModel>(reader.ReadToEnd());
            //    }
            //}

            /**
                Http Header
                Seed: RANDOM[16]
                Authorization: Base64(ToHexString(SHA256([Vendor 統編] + [Activation Key] + [Seed])))

                {
	                "SellerID": "[商家統編]",
	                "Booklet": 1
                }
             */

            await Request.SaveAsAsync(System.IO.Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogDailyPath, String.Format("{0}.txt", DateTime.Now.Ticks)), true);

            if (viewModel.Booklet.HasValue)
            {
                viewModel.quantity = viewModel.Booklet * 50;
            }

            viewModel.Seed = Request.Headers["Seed"].FirstOrDefault().GetEfficientString();
            viewModel.Authorization = Request.Headers["Authorization"].FirstOrDefault().GetEfficientString();

            String reason;
            var result = models.CheckAvailableInterval(viewModel,out reason);

            return Json(new
            {
                result = result,
                message = reason,
            });
        }

        public ActionResult Echo(POSDeviceViewModel viewModel)
        {
            return Json(viewModel);
        }

        public ActionResult EditItem(DataTableQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            JObject json = JObject.Parse(RequestBody);
            Table<Organization> org = models.GetTable<Organization>();
            IQueryable<Organization> items = org;
            Expression<Func<Organization, String>> order = o => o.CompanyName;

            //string propertyName = "CompanyName"; // 要排序的屬性名稱
            //Type sourceType = typeof(Organization); // 源類型
            //Type keyType = typeof(string); // 排序鍵類型
            //var propertyExpression = CreateExpression(sourceType, keyType, propertyName);

            //items = items.OrderBy(propertyExpression);
            items = items.OrderBy(order);
            items = items.Skip(1000).Take(50);
            var sqlCmd = items.ToString();

            return Json(viewModel);
        }

        public static Expression<Func<TSource, TKey>> CreateExpression<TSource, TKey>(string propertyName)
        {
            var parameter = Expression.Parameter(typeof(TSource), "x");
            var property = Expression.Property(parameter, typeof(TSource).GetProperty(propertyName));
            var lambda = Expression.Lambda<Func<TSource, TKey>>(property, parameter);
            return lambda;
        }

        public static LambdaExpression CreateExpression(Type sourceType, Type keyType, string propertyName)
        {
            var parameter = Expression.Parameter(sourceType, "x");
            var property = Expression.Property(parameter, sourceType.GetProperty(propertyName));
            var keySelector = Expression.Lambda(property, parameter);

            var resultType = typeof(Func<,>).MakeGenericType(sourceType, keyType);
            var lambda = Expression.Lambda(resultType, keySelector.Body, keySelector.Parameters);

            return lambda;
        }

        public async Task<ActionResult> DumpAsync()
        {
            await Request.SaveAsAsync(System.IO.Path.Combine(Logger.LogDailyPath, $"{DateTime.Now.Ticks}.txt"), true);
            return Json(new { result = true });
        }

        public ActionResult ResetKey()
        {
            AppResource.Instance.InitializeKey(true);
            return Content("OK");
        }

    }
}