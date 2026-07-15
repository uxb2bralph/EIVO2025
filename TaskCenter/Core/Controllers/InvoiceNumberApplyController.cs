using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelCore.DataEntity;
using ModelCore.DTOs;
using ModelCore.Helper;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Interfaces;

namespace TaskCenter.Core.Controllers
{
    /// <summary>
    /// 電子發票字軌號碼申請查詢 API（遷移自 WebHome InvoiceNumberApplyController 之 QueryIndex / Query 頁）。
    /// 提供申請 JSON 檔查詢，以及列動作：歸檔（MoveFile）、轉營業人（TransferOrganization）、下載 Word（SetAll）。
    /// 申請資料以檔案系統上的 JSON 檔保存，非資料庫實體；轉營業人之寫入沿用 CommitOrganizationViewModel。
    /// 對應舊版 [AuthorizedSysAdmin]：以 [Authorize] + [SysAdminOnly] 限系統管理者使用。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [SysAdminOnly]
    [Produces("application/json")]
    public class InvoiceNumberApplyController : ApiBaseController
    {
        private readonly IInvoiceNumberApplyService _invoiceNumberApplyService;

        public InvoiceNumberApplyController(
            IInvoiceNumberApplyService invoiceNumberApplyService,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
            _invoiceNumberApplyService = invoiceNumberApplyService;
        }

        /// <summary>
        /// 查詢申請 JSON 檔列表（對應舊版 Query / GetApplyJsonFiles）。以統一編號部分比對。
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseDto<List<InvoiceNumberApplyItemDto>>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 500)]
        public IActionResult GetList([FromQuery] InvoiceNumberApplyQueryDto queryDto)
        {
            try
            {
                var items = _invoiceNumberApplyService.GetApplyFiles(queryDto?.BusinessId);
                return CreateSuccessResponse(items, "Common.Retrieved");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving invoice number apply files");
                return CreateErrorResponse(500, "Common.RetrieveError");
            }
        }

        /// <summary>
        /// 歸檔：將申請 JSON 檔搬移至備份資料夾（遷移自舊版 MoveFile）。
        /// </summary>
        [HttpPost("MoveFile")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public IActionResult MoveFile([FromBody] InvoiceNumberApplyActionDto dto)
        {
            var filePath = _invoiceNumberApplyService.ResolveApplyFilePath(dto?.KeyId);
            if (filePath == null)
            {
                return CreateBadRequestResponse("file not found!!");
            }

            try
            {
                _invoiceNumberApplyService.MoveJsonFile(filePath);
                return CreateSuccessResponse("歸檔完成.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error archiving invoice number apply file");
                return CreateErrorResponse(500, ex.Message);
            }
        }

        /// <summary>
        /// 轉營業人：將申請資料轉為營業人並提交（遷移自舊版 TransferOrganization）。
        /// 成功後將申請 JSON 檔歸檔。
        /// </summary>
        [HttpPost("TransferOrganization")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public IActionResult TransferOrganization([FromBody] InvoiceNumberApplyActionDto dto)
        {
            var filePath = _invoiceNumberApplyService.ResolveApplyFilePath(dto?.KeyId);
            if (filePath == null)
            {
                return CreateBadRequestResponse("file not found!!");
            }

            var apply = _invoiceNumberApplyService.LoadApply(filePath);
            if (apply == null)
            {
                return CreateBadRequestResponse("file not found!!");
            }

            try
            {
                var organizationViewModel = _invoiceNumberApplyService.ConvertToOrganization(apply);

                // 沿用舊版 CommitOrganizationViewModel 的驗證與寫入邏輯。
                Organization organization = organizationViewModel.CommitOrganizationViewModel(models!, ModelState);

                if (organization == null || !ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(kv => kv.Value != null && kv.Value.Errors.Count > 0)
                        .SelectMany(kv => kv.Value!.Errors.Select(e => e.ErrorMessage))
                        .Where(m => !string.IsNullOrWhiteSpace(m))
                        .ToList();
                    if (errors.Count == 0)
                    {
                        errors.Add("營業人轉換失敗!!");
                    }
                    return CreateBadRequestResponse("營業人轉換失敗!!", errors);
                }

                _invoiceNumberApplyService.MoveJsonFile(filePath);

                return CreateSuccessResponse("營業人轉檔完成.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error transferring invoice number apply to organization");
                return CreateErrorResponse(500, "營業人轉換失敗!!");
            }
        }

        /// <summary>
        /// 下載 Word：依統一編號產出各 Word 範本並打包為 zip（遷移自舊版 SetAll）。
        /// </summary>
        /// <param name="businessId">統一編號</param>
        [HttpGet("DownloadWord")]
        [Produces("application/octet-stream")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public IActionResult DownloadWord([FromQuery] string businessId)
        {
            if (string.IsNullOrWhiteSpace(businessId))
            {
                return CreateBadRequestResponse("businessID is null or empty.");
            }

            try
            {
                var result = _invoiceNumberApplyService.BuildWordZip(businessId);
                if (result == null)
                {
                    return CreateBadRequestResponse("找不到相關JSON檔,或JSON格式有誤.");
                }

                return File(result.Value.Content, "application/octet-stream", result.Value.FileName);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error building invoice number apply word zip");
                return CreateErrorResponse(500, "JSON轉Word失敗.");
            }
        }
    }
}
