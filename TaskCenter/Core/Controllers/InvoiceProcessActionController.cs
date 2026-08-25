using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelCore.DTOs;
using TaskCenter.Core;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Interfaces;

namespace TaskCenter.Core.Controllers
{
    /// <summary>
    /// 發票逐列作業 API（遷移自 WebHome InvoiceProcessController 之 CommitAction 系列）。
    /// 目前提供線上作廢（對應選單「線上作廢發票」/InvoiceProcess/InquireToCancel）。
    /// 查詢由 InvoiceProcessQueryController 負責；此控制器僅處理寫入動作，作業對象依角色範圍限縮。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class InvoiceProcessActionController : ApiBaseController
    {
        private readonly IInvoiceProcessActionService _service;

        public InvoiceProcessActionController(
            IInvoiceProcessActionService service,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
            _service = service;
        }

        /// <summary>作廢選取的發票（對應舊版 CancelInvoice CommitAction）。</summary>
        [HttpPost("Cancel")]
        [ProducesResponseType(typeof(ResponseDto<CancelInvoiceResultDto>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 401)]
        [ProducesResponseType(typeof(BaseResponseDto), 500)]
        public async Task<IActionResult> Cancel([FromBody] CancelInvoiceRequestDto request)
        {
            var uid = User.GetUserId();
            if (!uid.HasValue) return CreateUnauthorizedResponse("登入資訊無效!!");

            if (request?.KeyIds == null || request.KeyIds.Count == 0)
            {
                return CreateBadRequestResponse("請選擇作廢資料!!");
            }

            try
            {
                var result = await _service.CancelAsync(request.KeyIds, uid.Value);
                return CreateSuccessResponse(result, "Common.Updated");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error cancelling invoices");
                return CreateErrorResponse(500, "Common.UpdateError");
            }
        }
    }
}
