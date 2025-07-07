using CommonLib.Core.Utility;
using CommonLib.DataAccess;
using CommonLib.Utility;
using MessagingToolkit.QRCode.Codec;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.Locale;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using WebHome.Controllers;
using WebHome.Properties;

namespace WebHome.Helper
{
    public static class MvcExtensions
    {
        public static GenericManager<EIVOEntityDataContext> DataSource(this ControllerBase controller)
        {
            return ((SampleController<EIVOEntityDataContext>)controller).DataSource;
        }

        //public static void RenderJsonResult(this ViewUserControl control, object data)
        //{
        //    JsonResult result = new JsonResult()
        //    {
        //        ContentType = "application/json",
        //        Data = data,
        //    };
        //    result.ExecuteResult(control.ViewContext.Controller.ControllerContext);
        //    control.
        //}

        public static async Task RenderJsonResultAsync(this HttpResponse response, object data)
        {
            response.Clear();
            response.ContentType = "application/json";
            await response.WriteAsync(data.JsonStringify());
        }

        public static async Task SaveAsExcelAsync(this DataSet ds, HttpResponse response, String disposition, String fileDownloadToken = null)
        {
            if (fileDownloadToken != null)
            {
                response.Cookies.Append("fileDownloadToken", fileDownloadToken);
            }
            response.Headers.Append("Cache-control", "max-age=1");
            response.ContentType = "application/vnd.ms-excel";
            response.Headers.Append("Content-Disposition", disposition);

            using (var xls = ds.ConvertToExcel())
            {
                String tmpPath = Path.Combine(FileLogger.Logger.LogDailyPath, $"{DateTime.Now.Ticks}.xlsx");
                using (FileStream tmp = System.IO.File.Create(tmpPath))
                {
                    xls.SaveAs(tmp);
                    tmp.Flush();
                    tmp.Position = 0;

                    await tmp.CopyToAsync(response.Body);
                }
                await response.Body.FlushAsync();

                System.IO.File.Delete(tmpPath);
            }

        }

        public static async Task SaveAsExcelAsync(this ClosedXML.Excel.XLWorkbook xls, HttpResponse response, String disposition, String fileDownloadToken = null)
        {
            if (fileDownloadToken != null)
            {
                response.Cookies.Append("fileDownloadToken", fileDownloadToken);
            }
            response.Headers.Append("Cache-control", "max-age=1");
            response.ContentType = "application/vnd.ms-excel";
            response.Headers.Append("Content-Disposition", disposition);

            String tmpPath = Path.Combine(FileLogger.Logger.LogDailyPath, $"{DateTime.Now.Ticks}.tmp");
            using (FileStream tmp = System.IO.File.Create(tmpPath))
            {
                xls.SaveAs(tmp);
                tmp.Flush();
                tmp.Position = 0;

                await tmp.CopyToAsync(response.Body);
            }
            await response.Body.FlushAsync();

            System.IO.File.Delete(tmpPath);
        }

        public static string RenderHtmlContentToString(this IHtmlContent htmlContent, HtmlEncoder encoder)
        {
            var builder = new System.Text.StringBuilder();
            using (var writer = new StringWriter(builder))
            {
                htmlContent.WriteTo(writer, encoder);
            }
            return builder.ToString();
        }
    }
}