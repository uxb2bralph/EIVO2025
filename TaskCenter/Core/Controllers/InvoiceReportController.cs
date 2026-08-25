using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CommonLib.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelCore.DTOs;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Interfaces;
using TaskCenter.Core.Services;

namespace TaskCenter.Core.Controllers
{
    /// <summary>
    /// 發票明細查詢 API（遷移自 WebHome InvoiceQueryController.InvoiceReport 頁面
    /// Views/InvoiceQuery/Module/InvoiceReport.cshtml「發票報表匯出」）。
    /// 對應選單「發票明細查詢」（/InvoiceQuery/InvoiceReport）。
    /// 以查詢條件取得發票明細清單（分頁 + 排序），並提供明細 Excel／CSV 匯出與附件壓縮檔下載
    /// （對應原版 CreateXlsxAsync／DownloadCSV／DownloadAttachment／DownloadAll）。
    /// 資料範圍依登入者角色過濾；原版頁面為 [RoleAuthorize(ROLE_SYS)]，故此處以 [SysAdminOnly] 對應。
    /// 開立人 / 代理業者候選清單沿用 /api/InvoiceProcessQuery 的 Sellers、Agents 端點。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [SysAdminOnly]
    [Produces("application/json")]
    public class InvoiceReportController : ApiBaseController
    {
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private readonly IInvoiceReportService _service;

        public InvoiceReportController(
            IInvoiceReportService service,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
            _service = service;
        }

        /// <summary>查詢發票明細（分頁）。發票日期起迄為必填。</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseDto<PagedResultDto<InvoiceReportRowDto>>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 401)]
        [ProducesResponseType(typeof(BaseResponseDto), 500)]
        public async Task<IActionResult> GetList([FromQuery] InvoiceReportQueryDto queryDto)
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
                Logger.LogError(ex, "Error retrieving invoice report");
                return CreateErrorResponse(500, "Common.RetrieveError");
            }
        }

        /// <summary>下載發票資料明細 Excel（欄位同結果表格）。</summary>
        [HttpPost("ExportXlsx")]
        [Produces("application/octet-stream")]
        public async Task<IActionResult> ExportXlsx([FromBody] InvoiceReportQueryDto queryDto)
        {
            var uid = User.GetUserId();
            if (!uid.HasValue) return CreateUnauthorizedResponse("登入資訊無效!!");

            var errors = ValidateDateRange(queryDto);
            if (errors.Count > 0) return CreateBadRequestResponse("查詢條件不完整!!", errors);

            var table = await _service.BuildDetailTableAsync(queryDto, uid.Value);
            using var ds = new DataSet();
            ds.Tables.Add(table);
            using var xls = ds.ConvertToExcel();
            using var ms = new MemoryStream();
            xls.SaveAs(ms);
            return File(ms.ToArray(), ExcelContentType, "發票資料明細.xlsx");
        }

        /// <summary>下載發票資料明細 CSV（欄位同 Excel）。以 UTF-8 BOM 輸出，供 Excel 直接開啟。</summary>
        [HttpPost("ExportCsv")]
        [Produces("application/octet-stream")]
        public async Task<IActionResult> ExportCsv([FromBody] InvoiceReportQueryDto queryDto)
        {
            var uid = User.GetUserId();
            if (!uid.HasValue) return CreateUnauthorizedResponse("登入資訊無效!!");

            var errors = ValidateDateRange(queryDto);
            if (errors.Count > 0) return CreateBadRequestResponse("查詢條件不完整!!", errors);

            var csv = await _service.BuildDetailCsvAsync(queryDto, uid.Value);
            // 原版以 CP950(Big5) 輸出；改用 UTF-8 BOM 以免非 Big5 字元遺失，Excel 亦可正確辨識。
            var bytes = new UTF8Encoding(true).GetBytes(csv);
            return File(bytes, "text/csv", "發票資料明細.csv");
        }

        /// <summary>下載選取發票的附件壓縮檔（對應原版逐列勾選 + 下載附件檔）。</summary>
        [HttpPost("DownloadAttachments")]
        [Produces("application/octet-stream")]
        public async Task<IActionResult> DownloadAttachments([FromBody] InvoiceAttachmentZipRequestDto request)
        {
            var uid = User.GetUserId();
            if (!uid.HasValue) return CreateUnauthorizedResponse("登入資訊無效!!");

            if (request?.KeyIds == null || request.KeyIds.Count == 0)
            {
                return CreateBadRequestResponse("請選擇下載資料!!");
            }

            var zip = await _service.BuildSelectedAttachmentZipAsync(request.KeyIds, uid.Value);
            return AttachmentZipFile(zip);
        }

        /// <summary>下載查詢結果全部發票的附件壓縮檔（對應原版 DownloadAll）。</summary>
        [HttpPost("DownloadAllAttachments")]
        [Produces("application/octet-stream")]
        public async Task<IActionResult> DownloadAllAttachments([FromBody] InvoiceReportQueryDto queryDto)
        {
            var uid = User.GetUserId();
            if (!uid.HasValue) return CreateUnauthorizedResponse("登入資訊無效!!");

            var errors = ValidateDateRange(queryDto);
            if (errors.Count > 0) return CreateBadRequestResponse("查詢條件不完整!!", errors);

            var zip = await _service.BuildAllAttachmentZipAsync(queryDto, uid.Value);
            if (zip.ExceededLimit)
            {
                return CreateBadRequestResponse(
                    $"符合條件且有附件的發票共 {zip.MatchedInvoiceCount} 筆，超過單次下載上限 " +
                    $"{InvoiceReportService.AttachmentZipInvoiceLimit} 筆，請縮小查詢範圍!!");
            }
            return AttachmentZipFile(zip);
        }

        // ── helpers ─────────────────────────────────────────────────

        /// <summary>附件壓縮檔回應；無可下載內容時回 400 並說明原因。</summary>
        private IActionResult AttachmentZipFile(InvoiceAttachmentZipResultDto zip)
        {
            if (zip.Content == null || zip.Content.Length == 0)
            {
                return CreateBadRequestResponse("選取的發票均無附件檔可下載!!");
            }
            return File(zip.Content, "application/zip", "發票附件.zip");
        }

        /// <summary>
        /// 發票日期起迄檢核。原版本頁未驗證，但明細查詢／匯出若不限日期將掃全表，
        /// 故與同類報表頁（發票統計表／發票月報表）一致改為必填。
        /// </summary>
        private static List<string> ValidateDateRange(InvoiceReportQueryDto dto)
        {
            var errors = new List<string>();
            if (!dto.DateFrom.HasValue) errors.Add("請輸入查詢起日");
            if (!dto.DateTo.HasValue) errors.Add("請輸入查詢迄日");
            return errors;
        }
    }
}
