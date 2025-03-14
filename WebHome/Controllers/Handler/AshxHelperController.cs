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
using CommonLib.Core.Helper;
using ModelCore.DataExchange;

namespace WebHome.Controllers.Handler
{
    public class AshxHelperController : SampleController<InvoiceItem>
    {
        public AshxHelperController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public IActionResult Index()
        {
            return Content("Helper OK!");
        }

        //public async Task<ActionResult> GetSampleAsync(String data)
        //{
        //    Response.ContentType = "text/plain";

        //    switch (data)
        //    {
        //        case "InvoiceBuyer":
        //            var exchange = new InvoiceBuyerExchange();
        //            using (XLWorkbook xls = exchange.GetSample())
        //            {
        //                await xls.SaveAsExcelAsync(Response, $"attachment;filename={HttpUtility.UrlEncode("修改買受人資料")}.xlsx");
        //            }
        //            break;

        //        case "TrackCode":
        //            var tracodeSample = new TrackCodeExchange();
        //            using (XLWorkbook xls = tracodeSample.GetSample())
        //            {
        //                await xls.SaveAsExcelAsync(Response, $"attachment;filename={HttpUtility.UrlEncode("發票字軌資料")}.xlsx");
        //            }
        //            break;

        //        case "WinningNo":
        //            DataTable table = new DataTable();
        //            table.Columns.Add(new DataColumn("期別", typeof(String)));
        //            table.Columns.Add(new DataColumn("字軌", typeof(String)));
        //            table.Columns.Add(new DataColumn("號碼", typeof(String)));
        //            table.Columns.Add(new DataColumn("中獎獎別", typeof(String)));
        //            table.Columns.Add(new DataColumn("中獎獎金", typeof(int)));

        //            DateTime sampleDate = (new DateTime(DateTime.Today.Year, (DateTime.Today.Month + 1) / 2 * 2, 1)).AddMonths(-2);
        //            var row = table.NewRow();
        //            row[0] = $"{sampleDate.Year - 1911:000}{sampleDate.Month:00}";
        //            row[1] = "XX";
        //            row[2] = "01234567";
        //            row[3] = "D";
        //            row[4] = "500";

        //            table.Rows.Add(row);

        //            using (DataSet ds = new DataSet())
        //            {
        //                table.TableName = "中獎清冊";
        //                ds.Tables.Add(table);

        //                using var xls = ds.ConvertToExcel();
        //                await xls.SaveAsExcelAsync(Response, $"attachment;filename={HttpUtility.UrlEncode("WinningSample")}.xlsx");
        //            }
        //            break;
        //    }

        //    return new EmptyResult();

        //}

    }
}