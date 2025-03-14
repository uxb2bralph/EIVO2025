using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Microsoft.AspNetCore.Mvc;


using CommonLib.DataAccess;

using WebHome.Properties;
using MessagingToolkit.QRCode.Codec;
using ModelCore.DataEntity;
using ModelCore.Locale;

using CommonLib.Utility;
using WebHome.Controllers;
using CommonLib.Core.Utility;
using System.Data;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModelCore.Helper;

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

    }
}