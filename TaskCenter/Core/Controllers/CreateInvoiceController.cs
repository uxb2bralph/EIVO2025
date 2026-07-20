using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelCore.DataEntity;
using ModelCore.DTOs;
using ModelCore.Helper;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Interfaces;
using TaskCenter.Core.Services;

namespace TaskCenter.Core.Controllers
{
    /// <summary>
    /// 線上開立發票 API（遷移自 WebHome InvoiceBusinessController.CreateInvoice /
    /// CommitInvoice（F0401 存證）/ CommitA0101（B2B 交換）），對應選單「線上開立發票」
    /// （/InvoiceBusiness/CreateInvoice）。提供開立人 / 相對營業人 / 產品快速查詢，以及開立與內容預覽。
    /// 開立對象（開立人）以加密後的 sellerKey 傳遞；寫入前以角色範圍（CanAccessSeller）守門。
    /// 延後：列印 / 立即檢視證明聯（需 QRCode 金鑰）。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class CreateInvoiceController : ApiBaseController
    {
        private readonly ICreateInvoiceService _service;

        public CreateInvoiceController(
            ICreateInvoiceService service,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
            _service = service;
        }

        /// <summary>開立人候選清單（依角色範圍限縮）。</summary>
        [HttpGet("Sellers")]
        [ProducesResponseType(typeof(ResponseDto<List<CreateInvoiceSellerOptionDto>>), 200)]
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

        /// <summary>相對營業人（買受人）查詢：供表單自動帶入 / autocomplete。需先選定開立人。</summary>
        [HttpGet("Counterparts")]
        [ProducesResponseType(typeof(ResponseDto<List<CounterpartOptionDto>>), 200)]
        public async Task<IActionResult> Counterparts([FromQuery] string? sellerKey, [FromQuery] string? term)
        {
            var sellerId = ResolveSellerId(sellerKey);
            if (sellerId == null) return CreateBadRequestResponse("請先選擇發票開立人!!");
            if (!CanAccessSeller(sellerId.Value)) return CreateErrorResponse(403, "無權存取此開立人資料!!");

            try
            {
                var result = await _service.SearchCounterpartsAsync(sellerId.Value, term);
                return CreateSuccessResponse(result, "Common.Retrieved");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving counterparts");
                return CreateErrorResponse(500, "Common.RetrieveError");
            }
        }

        /// <summary>產品快速查詢（依登入者角色與所選開立人限縮）。</summary>
        [HttpGet("Products")]
        [ProducesResponseType(typeof(ResponseDto<List<ProductOptionDto>>), 200)]
        public async Task<IActionResult> Products([FromQuery] string? sellerKey, [FromQuery] string? productName)
        {
            var uid = User.GetUserId();
            if (!uid.HasValue) return CreateUnauthorizedResponse("登入資訊無效!!");

            var sellerId = ResolveSellerId(sellerKey) ?? 0;
            try
            {
                var result = await _service.SearchProductsAsync(uid.Value, sellerId, productName);
                return CreateSuccessResponse(result, "Common.Retrieved");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving products");
                return CreateErrorResponse(500, "Common.RetrieveError");
            }
        }

        /// <summary>
        /// 開立發票（F0401 存證或 A0101 交換，由 body.ProcessType 決定）。
        /// ForPreview 為 true 時僅回傳內容預覽（不寫入）。
        /// </summary>
        [HttpPost("Commit")]
        [ProducesResponseType(typeof(ResponseDto<CreateInvoiceResultDto>), 200)]
        [ProducesResponseType(typeof(ResponseDto<InvoicePreviewDto>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public async Task<IActionResult> Commit([FromBody] CreateInvoiceRequestDto dto)
        {
            var sellerId = ResolveSellerId(dto?.SellerKey);
            if (dto == null || sellerId == null) return CreateBadRequestResponse("發票開立人錯誤!!");
            if (!CanAccessSeller(sellerId.Value)) return CreateErrorResponse(403, "無權為此開立人開立發票!!");

            try
            {
                var result = await _service.CommitAsync(dto, sellerId.Value);
                return result.Outcome switch
                {
                    CreateInvoiceOutcome.Preview => CreateSuccessResponse(result.Preview!, "Common.Retrieved"),
                    CreateInvoiceOutcome.Created => CreateSuccessResponse(result.Created!, "發票已開立"),
                    _ => CreateBadRequestResponse(result.Message ?? "開立失敗"),
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error committing invoice");
                return CreateErrorResponse(500, "開立失敗，請稍後再試!!");
            }
        }

        // ── helpers ─────────────────────────────────────────────────

        /// <summary>解析加密後的 sellerKey → CompanyID（失敗回 null）。</summary>
        private static int? ResolveSellerId(string? sellerKey)
        {
            if (string.IsNullOrEmpty(sellerKey)) return null;
            try
            {
                var id = sellerKey.DecryptKeyValue();
                return id > 0 ? id : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>登入者是否為系統管理。</summary>
        private bool IsAdmin() => OrganizationScope.IsSystemAdmin(User.GetRoleId(), User.GetCategoryId());

        /// <summary>登入者是否有權為指定開立人操作（admin 一律可）。</summary>
        private bool CanAccessSeller(int sellerId)
            => OrganizationScope.CanAccessSeller(models!, User.GetRoleId(), User.GetCategoryId(), User.GetCompanyId(), sellerId);
    }
}
