using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommonLib.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelCore.DataEntity;
using ModelCore.DTOs;
using ModelCore.Locale;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Interfaces;

namespace TaskCenter.Core.Controllers
{
    /// <summary>
    /// 電子發票字軌維護 API（遷移自 WebHome TrackCodeController 之 Index / Inquire 與列管理動作）。
    /// 由系統管理維護入口進入；字軌以原始 TrackID 傳遞（沿用舊版做法，TrackID 為非敏感之流水鍵）。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class TrackCodeController : ApiBaseController
    {
        private readonly ITrackCodeService _trackCodeService;

        public TrackCodeController(
            ITrackCodeService trackCodeService,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
            _trackCodeService = trackCodeService;
        }

        /// <summary>
        /// 查詢電子發票字軌（分頁）。
        /// 對應舊版 TrackCodeController.Inquire（依發票年度 + 期別篩選）。
        /// </summary>
        /// <param name="queryDto">查詢條件（Year 必填）</param>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseDto<PagedResultDto<TrackCodeDatatableDto>>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 500)]
        public async Task<IActionResult> GetList([FromQuery] TrackCodeQueryDto queryDto)
        {
            if (queryDto == null || !queryDto.Year.HasValue)
            {
                return CreateBadRequestResponse("請選擇年份!!");
            }

            try
            {
                var result = await _trackCodeService.GetPagedAsync(queryDto);
                return CreateSuccessResponse(result, "Common.Retrieved");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving track codes");
                return CreateErrorResponse(500, "Common.RetrieveError");
            }
        }

        /// <summary>
        /// 新增 / 修改電子發票字軌（遷移自 TrackCodeController.CommitItem）。
        /// 沿用舊版驗證：字軌須為二位英文字母；新增時須有期別（1~6）與年度，且同年度 / 期別 / 字軌不可重複。
        /// TrackId 為 null 時新增，否則修改（修改僅更新字軌與類別）。
        /// </summary>
        [HttpPost("CommitItem")]
        [ProducesResponseType(typeof(ResponseDto<TrackCodeDatatableDto>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public IActionResult CommitItem([FromBody] TrackCodeEditDto dto)
        {
            if (dto == null)
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            var errors = new List<string>();

            // 字軌須為二位英文字母（沿用舊版 Regex "^[A-Za-z]{2}$"）。
            var trackCode = dto.TrackCode.GetEfficientString();
            if (trackCode == null || !Regex.IsMatch(trackCode, "^[A-Za-z]{2}$"))
            {
                errors.Add("字軌應為二位英文字母!!");
            }

            // TrackId 有值時視為修改，否則新增。
            var model = dto.TrackId.HasValue
                ? models!.GetTable<InvoiceTrackCode>().FirstOrDefault(t => t.TrackID == dto.TrackId.Value)
                : null;

            // 新增（含 TrackId 指定但查無資料）時，驗證期別 / 年度並檢查字軌是否重複。
            if (model == null)
            {
                if (!dto.PeriodNo.HasValue || dto.PeriodNo > 6 || dto.PeriodNo < 1)
                {
                    errors.Add("請選擇期別!!");
                }
                else if (!dto.Year.HasValue)
                {
                    errors.Add("請選擇年份!!");
                }
                else if (trackCode != null)
                {
                    var year = (short)dto.Year.Value;
                    var periodNo = (short)dto.PeriodNo.Value;
                    var duplicated = models!.GetTable<InvoiceTrackCode>()
                        .Any(t => t.Year == year && t.TrackCode == trackCode && t.PeriodNo == periodNo);
                    if (duplicated)
                    {
                        errors.Add("字軌重複!!");
                    }
                }
            }

            if (errors.Count > 0)
            {
                return CreateBadRequestResponse("Common.SaveError", errors);
            }

            if (model == null)
            {
                model = new InvoiceTrackCode
                {
                    Year = (short)dto.Year!.Value,
                    PeriodNo = (short)dto.PeriodNo!.Value,
                };
                models!.GetTable<InvoiceTrackCode>().Add(model);
            }

            model.TrackCode = trackCode!;
            model.InvoiceType = dto.InvoiceType.HasValue
                ? (byte)dto.InvoiceType.Value
                : (byte)Naming.InvoiceTypeDefinition.一般稅額計算之電子發票;

            models!.SubmitChanges();

            var result = new TrackCodeDatatableDto
            {
                TrackId = model.TrackID,
                Year = model.Year,
                PeriodNo = model.PeriodNo,
                TrackCode = model.TrackCode,
                InvoiceType = model.InvoiceType,
            };
            return CreateSuccessResponse(result, "Common.Saved");
        }

        /// <summary>
        /// 刪除電子發票字軌（遷移自 TrackCodeController.DeleteItem）。
        /// 沿用舊版以原始 TrackID 傳遞。
        /// </summary>
        /// <param name="id">字軌識別碼（InvoiceTrackCode.TrackID）</param>
        [HttpPost("DeleteItem")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public IActionResult DeleteItem([FromQuery] int id)
        {
            var item = models!.DeleteAny<InvoiceTrackCode>(d => d.TrackID == id);
            if (item == null)
            {
                return CreateBadRequestResponse("發票字軌資料錯誤!!");
            }

            return CreateSuccessResponse("Common.Saved");
        }
    }
}
