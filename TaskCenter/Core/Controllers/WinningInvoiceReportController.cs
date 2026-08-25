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
    /// 中獎統計表 API（遷移自 WebHome WinningInvoiceController.ReportIndex / InquireReport /
    /// ReportGridPage / CreateXlsx）。對應選單「中獎統計表」（/WinningInvoice/ReportIndex）。
    /// 以查詢條件過濾發票後，僅取中獎發票並依「開立人」彙總中獎 / 捐贈張數；另可下載統計表 Excel。
    /// 舊版限 ROLE_SYS 使用（[RoleAuthorize(ROLE_SYS)]），此處以 JWT roleId 比對維持相同限制；
    /// 資料範圍另由服務層依登入者角色過濾。
    /// 開立人 / 代理業者候選清單沿用 /api/InvoiceProcessQuery 的 Sellers、Agents 端點。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class WinningInvoiceReportController : ApiBaseController
    {
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private readonly IWinningInvoiceReportService _service;

        public WinningInvoiceReportController(
            IWinningInvoiceReportService service,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
            _service = service;
        }

        /// <summary>依開立人彙總中獎 / 捐贈張數（分頁）。發票日期起迄為必填。</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseDto<PagedResultDto<WinningInvoiceReportRowDto>>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 401)]
        [ProducesResponseType(typeof(BaseResponseDto), 403)]
        [ProducesResponseType(typeof(BaseResponseDto), 500)]
        public async Task<IActionResult> GetList([FromQuery] WinningInvoiceReportQueryDto queryDto)
        {
            var uid = User.GetUserId();
            if (!uid.HasValue) return CreateUnauthorizedResponse("登入資訊無效!!");
            if (!User.IsSystemAdmin()) return CreateErrorResponse(403, "權限不足!!");

            var errors = ValidateDateRange(queryDto);
            if (errors.Count > 0) return CreateBadRequestResponse("查詢條件不完整!!", errors);

            try
            {
                var result = await _service.GetPagedAsync(queryDto, uid.Value);
                return CreateSuccessResponse(result, "Common.Retrieved");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving winning invoice report");
                return CreateErrorResponse(500, "Common.RetrieveError");
            }
        }

        /// <summary>下載中獎統計表 Excel（單一工作表，不分頁）。</summary>
        [HttpPost("Export")]
        [Produces("application/octet-stream")]
        public async Task<IActionResult> Export([FromBody] WinningInvoiceReportQueryDto queryDto)
        {
            var uid = User.GetUserId();
            if (!uid.HasValue) return CreateUnauthorizedResponse("登入資訊無效!!");
            if (!User.IsSystemAdmin()) return CreateErrorResponse(403, "權限不足!!");

            var errors = ValidateDateRange(queryDto);
            if (errors.Count > 0) return CreateBadRequestResponse("查詢條件不完整!!", errors);

            using var ds = await _service.BuildReportAsync(queryDto, uid.Value);
            using var xls = ds.ConvertToExcel();
            using var ms = new MemoryStream();
            xls.SaveAs(ms);
            return File(ms.ToArray(), ExcelContentType, $"中獎統計表({DateTime.Today:yyyy-MM-dd}).xlsx");
        }

        // ── helpers ─────────────────────────────────────────────────

        /// <summary>發票日期起迄檢核（遷移時新增，避免無條件全表彙總）。</summary>
        private static List<string> ValidateDateRange(WinningInvoiceReportQueryDto dto)
        {
            var errors = new List<string>();
            if (!dto.DateFrom.HasValue) errors.Add("請輸入查詢起日");
            if (!dto.DateTo.HasValue) errors.Add("請輸入查詢迄日");
            return errors;
        }
    }
}
