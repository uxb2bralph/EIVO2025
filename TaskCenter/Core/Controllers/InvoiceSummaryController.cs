using System;
using System.Collections.Generic;
using System.Data;
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
    /// 發票統計表 API（遷移自 WebHome InvoiceQueryController.InvoiceSummary / InquireSummary /
    /// CreateMonthlyReportXlsx）。對應選單「發票統計表」（/InvoiceQuery/InvoiceSummary）。
    /// 以查詢條件過濾發票後，依「開立發票營業人」彙總筆數；另可下載開立發票月報表 Excel。
    /// 資料範圍依登入者角色過濾（系統管理看全部、開立人看自家、代理看旗下）。
    /// 開立人 / 代理業者候選清單沿用 /api/InvoiceProcessQuery 的 Sellers、Agents 端點。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class InvoiceSummaryController : ApiBaseController
    {
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private readonly IInvoiceSummaryService _service;

        public InvoiceSummaryController(
            IInvoiceSummaryService service,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
            _service = service;
        }

        /// <summary>依開立發票營業人彙總查詢結果（分頁）。發票日期起迄為必填（同舊版 InquireSummary）。</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseDto<PagedResultDto<InvoiceSummaryRowDto>>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 401)]
        [ProducesResponseType(typeof(BaseResponseDto), 500)]
        public async Task<IActionResult> GetList([FromQuery] InvoiceSummaryQueryDto queryDto)
        {
            var uid = User.GetUserId();
            if (!uid.HasValue) return CreateUnauthorizedResponse("登入資訊無效!!");

            var errors = ValidateDateRange(queryDto);
            if (errors.Count > 0) return CreateBadRequestResponse("查詢條件不完整!!", errors);

            try
            {
                var result = await _service.GetPagedAsync(queryDto, uid.Value);
                return CreateSuccessResponse(result, "Common.Retrieved");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving invoice summary");
                return CreateErrorResponse(500, "Common.RetrieveError");
            }
        }

        /// <summary>下載開立發票月報表 Excel（營業人統計 + 各年月日別統計）。</summary>
        [HttpPost("MonthlyReport")]
        [Produces("application/octet-stream")]
        public async Task<IActionResult> MonthlyReport([FromBody] InvoiceSummaryQueryDto queryDto)
        {
            var uid = User.GetUserId();
            if (!uid.HasValue) return CreateUnauthorizedResponse("登入資訊無效!!");

            var errors = ValidateDateRange(queryDto);
            if (errors.Count > 0) return CreateBadRequestResponse("查詢條件不完整!!", errors);

            using var ds = await _service.BuildMonthlyReportAsync(queryDto, uid.Value);
            using var xls = ds.ConvertToExcel();
            using var ms = new MemoryStream();
            xls.SaveAs(ms);
            return File(ms.ToArray(), ExcelContentType, "開立發票月報表.xlsx");
        }

        // ── helpers ─────────────────────────────────────────────────

        /// <summary>發票日期起迄檢核（對應舊版 InquireSummary 的 ModelState 驗證）。</summary>
        private static List<string> ValidateDateRange(InvoiceSummaryQueryDto dto)
        {
            var errors = new List<string>();
            if (!dto.DateFrom.HasValue) errors.Add("請輸入查詢起日");
            if (!dto.DateTo.HasValue) errors.Add("請輸入查詢迄日");
            return errors;
        }
    }
}
