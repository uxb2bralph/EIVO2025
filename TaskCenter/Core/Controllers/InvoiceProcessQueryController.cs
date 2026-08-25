using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Core.Utility;
using CommonLib.DataAccess;
using CommonLib.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelCore.DataEntity;
using ModelCore.DTOs;
using ModelCore.Helper;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Interfaces;
using TaskCenter.Core.Services;

namespace TaskCenter.Core.Controllers
{
    /// <summary>
    /// 發票資料查詢／列印／匯出 API（遷移自 WebHome InvoiceProcessController.Index/Inquire）。
    /// 對應選單「資料查詢／列印／匯出」（/InvoiceProcess/Index）。查詢依登入者角色範圍過濾，
    /// 提供分頁清單、幣別統計、發票明細，以及 Excel／買受人／ERP 匯出。
    /// 註：與既有 InvoiceProcessController（api/InvoiceProcess，外部 agent 用途）為不同用途，故另立控制器。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class InvoiceProcessQueryController : ApiBaseController
    {
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        private const string ErpCustomerReceiptNo = "43460094";

        private readonly IInvoiceProcessQueryService _service;

        public InvoiceProcessQueryController(
            IInvoiceProcessQueryService service,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
            _service = service;
        }

        /// <summary>查詢發票（分頁）。依登入者角色範圍過濾。</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseDto<PagedResultDto<InvoiceItemDatatableDto>>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 401)]
        [ProducesResponseType(typeof(BaseResponseDto), 500)]
        public async Task<IActionResult> GetList([FromQuery] InvoiceProcessQueryDto queryDto)
        {
            var uid = User.GetUserId();
            if (!uid.HasValue) return CreateUnauthorizedResponse("登入資訊無效!!");

            try
            {
                var result = await _service.GetPagedAsync(queryDto, uid.Value);
                return CreateSuccessResponse(result, "Common.Retrieved");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving invoices");
                return CreateErrorResponse(500, "Common.RetrieveError");
            }
        }

        /// <summary>幣別統計（對應舊版 CurrencySummary footer）。</summary>
        [HttpGet("Summary")]
        [ProducesResponseType(typeof(ResponseDto<List<CurrencySummaryDto>>), 200)]
        public async Task<IActionResult> Summary([FromQuery] InvoiceProcessQueryDto queryDto)
        {
            var uid = User.GetUserId();
            if (!uid.HasValue) return CreateUnauthorizedResponse("登入資訊無效!!");

            try
            {
                var result = await _service.GetSummaryAsync(queryDto, uid.Value);
                return CreateSuccessResponse(result, "Common.Retrieved");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving currency summary");
                return CreateErrorResponse(500, "Common.RetrieveError");
            }
        }

        /// <summary>取得發票明細（供結果列點擊發票號碼開啟預覽）。</summary>
        [HttpGet("Detail")]
        [ProducesResponseType(typeof(ResponseDto<InvoiceDetailDto>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public async Task<IActionResult> Detail([FromQuery] string keyId)
        {
            var uid = User.GetUserId();
            if (!uid.HasValue) return CreateUnauthorizedResponse("登入資訊無效!!");
            if (string.IsNullOrEmpty(keyId)) return CreateBadRequestResponse("Common.InvalidParameter");

            try
            {
                var invoiceId = keyId.DecryptKeyValue();
                var detail = await _service.GetDetailAsync(invoiceId, uid.Value);
                if (detail == null) return CreateNotFoundResponse("查無發票資料或無權存取!!");
                return CreateSuccessResponse(detail, "Common.Retrieved");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving invoice detail");
                return CreateErrorResponse(500, "Common.RetrieveError");
            }
        }

        /// <summary>開立人候選清單（依角色範圍限縮）。</summary>
        [HttpGet("Sellers")]
        [ProducesResponseType(typeof(ResponseDto<List<InvoiceQuerySellerOptionDto>>), 200)]
        public async Task<IActionResult> Sellers([FromQuery] string? keyword)
        {
            try
            {
                var result = await _service.SearchSellersAsync(keyword, IsAdmin(), User.GetCategoryId(), User.GetCompanyId());
                return CreateSuccessResponse(result, "Common.Retrieved");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving sellers");
                return CreateErrorResponse(500, "Common.RetrieveError");
            }
        }

        /// <summary>代理業者候選清單（僅系統管理使用）。</summary>
        [HttpGet("Agents")]
        [ProducesResponseType(typeof(ResponseDto<List<InvoiceQueryAgentOptionDto>>), 200)]
        public async Task<IActionResult> Agents([FromQuery] string? keyword)
        {
            if (!IsAdmin()) return CreateSuccessResponse(new List<InvoiceQueryAgentOptionDto>(), "Common.Retrieved");
            try
            {
                var result = await _service.SearchAgentsAsync(keyword);
                return CreateSuccessResponse(result, "Common.Retrieved");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving agents");
                return CreateErrorResponse(500, "Common.RetrieveError");
            }
        }

        /// <summary>下載發票資料明細 Excel（admin 25 欄 / 非 admin 20 欄）。</summary>
        [HttpPost("ExportXlsx")]
        [Produces("application/octet-stream")]
        public async Task<IActionResult> ExportXlsx([FromBody] InvoiceProcessQueryDto queryDto)
        {
            var uid = User.GetUserId();
            if (!uid.HasValue) return CreateUnauthorizedResponse("登入資訊無效!!");

            var table = await _service.BuildXlsxTableAsync(queryDto, uid.Value, IsAdmin());
            return ExcelFile(table, "發票資料明細.xlsx");
        }

        /// <summary>下載發票買受人資料 Excel（6 欄；僅系統管理）。</summary>
        [HttpPost("ExportBuyer")]
        [Produces("application/octet-stream")]
        public async Task<IActionResult> ExportBuyer([FromBody] InvoiceProcessQueryDto queryDto)
        {
            var uid = User.GetUserId();
            if (!uid.HasValue) return CreateUnauthorizedResponse("登入資訊無效!!");
            if (!IsAdmin()) return CreateErrorResponse(403, "無權下載買受人資料!!");

            var table = await _service.BuildBuyerTableAsync(queryDto, uid.Value);
            return ExcelFile(table, "發票買受人資料.xlsx");
        }

        /// <summary>下載 ERP 匯出檔（固定寬度 POSINV.dat；系統管理或特定開立人統編）。</summary>
        [HttpPost("ExportErp")]
        [Produces("application/octet-stream")]
        public async Task<IActionResult> ExportErp([FromBody] InvoiceProcessQueryDto queryDto)
        {
            var uid = User.GetUserId();
            if (!uid.HasValue) return CreateUnauthorizedResponse("登入資訊無效!!");
            if (!IsAdmin() && !CurrentSellerIs(ErpCustomerReceiptNo))
            {
                return CreateErrorResponse(403, "無權下載 ERP 資料!!");
            }

            var text = await _service.BuildErpTextAsync(queryDto, uid.Value);
            var bytes = Encoding.UTF8.GetBytes(text);
            return File(bytes, "text/plain", "POSINV.dat");
        }

        /// <summary>
        /// 下載選取發票的 MIG XML 壓縮檔（對應選單「下載MIG檔案」/InvoiceProcess/InquireToMIG 之
        /// 下載F0401／下載F0701／下載F0501）。下載對象依登入者角色範圍限縮。
        /// </summary>
        [HttpPost("DownloadMig")]
        [Produces("application/octet-stream")]
        public async Task<IActionResult> DownloadMig([FromBody] MigDownloadRequestDto request)
        {
            var uid = User.GetUserId();
            if (!uid.HasValue) return CreateUnauthorizedResponse("登入資訊無效!!");

            var docType = request?.DocType?.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(docType) || !InvoiceProcessQueryService.MigDocTypes.Contains(docType))
            {
                return CreateBadRequestResponse("不支援的 MIG 格式!!");
            }
            if (request!.KeyIds == null || request.KeyIds.Count == 0)
            {
                return CreateBadRequestResponse("請選擇下載資料!!");
            }

            var zip = await _service.BuildMigZipAsync(docType, request.KeyIds, uid.Value);
            if (zip.Content == null || zip.Content.Length == 0)
            {
                return CreateBadRequestResponse(docType == "F0501"
                    ? "選取的發票均無 F0501 可下載（僅已作廢發票適用）!!"
                    : $"選取的發票均無 {docType} 可下載!!");
            }

            return File(zip.Content, "application/zip", $"{docType}.zip");
        }

        // ── helpers ─────────────────────────────────────────────────

        private IActionResult ExcelFile(DataTable table, string fileName)
        {
            using var ds = new DataSet();
            ds.Tables.Add(table);
            using var xls = ds.ConvertToExcel();
            using var ms = new MemoryStream();
            xls.SaveAs(ms);
            return File(ms.ToArray(), ExcelContentType, fileName);
        }

        /// <summary>登入者是否為系統管理。</summary>
        private bool IsAdmin() => OrganizationScope.IsSystemAdmin(User.GetRoleId(), User.GetCategoryId());

        /// <summary>登入者所屬開立人統編是否等於指定值（供 ERP 下載授權）。</summary>
        private bool CurrentSellerIs(string receiptNo)
        {
            var companyId = User.GetCompanyId();
            if (!companyId.HasValue) return false;
            return models!.GetTable<Organization>()
                .Any(o => o.CompanyID == companyId.Value && o.ReceiptNo == receiptNo);
        }
    }
}
