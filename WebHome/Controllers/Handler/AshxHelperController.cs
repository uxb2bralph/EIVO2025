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

        /// <summary>
        /// 下載匯入用的 Excel 範本，取代舊版 ~/Helper/GetSample.ashx。
        /// </summary>
        /// <param name="data">範本種類：InvoiceBuyer、TrackCode、WinningNo</param>
        [HttpGet]
        [Route("Helper/GetSample.ashx")]
        [Route("AshxHelper/GetSample")]
        public async Task<ActionResult> GetSampleAsync(String? data)
        {
            switch (data)
            {
                case "InvoiceBuyer":
                    using (XLWorkbook xls = new InvoiceBuyerExchange().GetSample())
                    {
                        await SaveSampleAsync(xls, "修改買受人資料.xlsx");
                    }
                    break;

                case "TrackCode":
                    using (XLWorkbook xls = new TrackCodeExchange().GetSample())
                    {
                        await SaveSampleAsync(xls, "發票字軌資料.xlsx");
                    }
                    break;

                case "WinningNo":
                    using (DataSet ds = CreateWinningNoSample())
                    using (XLWorkbook xls = ds.ConvertToExcel())
                    {
                        await SaveSampleAsync(xls, "WinningSample.xlsx");
                    }
                    break;

                default:
                    return NotFound();
            }

            return new EmptyResult { };
        }

        private Task SaveSampleAsync(XLWorkbook xls, String fileName)
        {
            return xls.SaveAsExcelAsync(Response, $"attachment;filename={HttpUtility.UrlEncode(fileName)}");
        }

        private static DataSet CreateWinningNoSample()
        {
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("期別", typeof(String)));
            table.Columns.Add(new DataColumn("字軌", typeof(String)));
            table.Columns.Add(new DataColumn("號碼", typeof(String)));
            table.Columns.Add(new DataColumn("中獎獎別", typeof(String)));
            table.Columns.Add(new DataColumn("中獎獎金", typeof(int)));

            DateTime sampleDate = (new DateTime(DateTime.Today.Year, (DateTime.Today.Month + 1) / 2 * 2, 1)).AddMonths(-2);
            var row = table.NewRow();
            row[0] = $"{sampleDate.Year - 1911:000}{sampleDate.Month:00}";
            row[1] = "XX";
            row[2] = "01234567";
            row[3] = "D";
            row[4] = "500";

            table.Rows.Add(row);

            table.TableName = "中獎清冊";
            DataSet ds = new DataSet();
            ds.Tables.Add(table);
            return ds;
        }

    }
}