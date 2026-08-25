using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommonLib.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelCore.DTOs;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Interfaces;

namespace TaskCenter.Core.Controllers
{
    /// <summary>
    /// 發票月報表 API（遷移自 WebHome InvoiceQueryController.MonthlyReport / InquireMonthlyReport）。
    /// 對應選單「下載發票月報表」（/InvoiceQuery/MonthlyReport）。
    /// 依所選開立人 / 代理業者與日期區間逐月統計發票、作廢發票、折讓、作廢折讓筆數與計費，
    /// 直接回傳 Excel（舊版為背景產檔 + 輪詢下載）。資料範圍依登入者角色過濾。
    /// 開立人 / 代理業者候選清單沿用 /api/InvoiceProcessQuery 的 Sellers、Agents 端點。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class MonthlyReportController : ApiBaseController
    {
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private readonly IMonthlyReportService _service;

        public MonthlyReportController(
            IMonthlyReportService service,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
            _service = service;
        }

        /// <summary>製作並下載發票月報表 Excel。</summary>
        [HttpPost("Export")]
        [Produces("application/octet-stream")]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 401)]
        [ProducesResponseType(typeof(BaseResponseDto), 500)]
        public async Task<IActionResult> Export([FromBody] MonthlyReportQueryDto queryDto)
        {
            var uid = User.GetUserId();
            if (!uid.HasValue) return CreateUnauthorizedResponse("登入資訊無效!!");

            var errors = Validate(queryDto);
            if (errors.Count > 0) return CreateBadRequestResponse("查詢條件不完整!!", errors);

            try
            {
                using var ds = await _service.BuildReportAsync(queryDto, uid.Value);
                using var xls = ds.ConvertToExcel();
                using var ms = new MemoryStream();
                xls.SaveAs(ms);
                return File(ms.ToArray(), ExcelContentType, "月報表資料明細.xlsx");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating monthly report");
                return CreateErrorResponse(500, "月報表製作失敗!!");
            }
        }

        // ── helpers ─────────────────────────────────────────────────

        /// <summary>查詢條件檢核（對應舊版 InquireMonthlyReport 的 ModelState 驗證）。</summary>
        private static List<string> Validate(MonthlyReportQueryDto dto)
        {
            var errors = new List<string>();
            if (!dto.DateFrom.HasValue) errors.Add("請輸入查詢起日");
            if (!dto.DateTo.HasValue) errors.Add("請輸入查詢迄日");
            if (string.IsNullOrEmpty(dto.SellerKey) && string.IsNullOrEmpty(dto.AgentKey))
            {
                errors.Add("請選擇代理人或開立人");
            }
            return errors;
        }
    }
}
