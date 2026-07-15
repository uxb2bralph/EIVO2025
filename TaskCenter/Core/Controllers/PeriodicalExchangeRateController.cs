using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommonLib.Core.Utility;
using CommonLib.DataAccess;
using CommonLib.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelCore.DataEntity;
using ModelCore.DTOs;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Interfaces;

namespace TaskCenter.Core.Controllers
{
    /// <summary>
    /// 期別匯率維護 API（遷移自 WebHome PeriodicalExchangeRateController 之 Index / Inquire、列管理動作，
    /// 以及匯率資料範本下載 / Excel 匯入）。
    /// 匯率以複合鍵（PeriodID + CurrencyID）識別；PeriodID = 年度*100 + 期別（期別 1~6 為雙月）。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class PeriodicalExchangeRateController : ApiBaseController
    {
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private readonly IPeriodicalExchangeRateService _exchangeRateService;

        public PeriodicalExchangeRateController(
            IPeriodicalExchangeRateService exchangeRateService,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
            _exchangeRateService = exchangeRateService;
        }

        /// <summary>
        /// 查詢期別匯率（分頁）。對應舊版 PeriodicalExchangeRateController.Inquire。
        /// </summary>
        /// <param name="queryDto">查詢條件（Year 必填）</param>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseDto<PagedResultDto<ExchangeRateDatatableDto>>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 500)]
        public async Task<IActionResult> GetList([FromQuery] ExchangeRateQueryDto queryDto)
        {
            if (queryDto == null || !queryDto.Year.HasValue)
            {
                return CreateBadRequestResponse("請選擇年份!!");
            }

            try
            {
                var result = await _exchangeRateService.GetPagedAsync(queryDto);
                return CreateSuccessResponse(result, "Common.Retrieved");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving exchange rates");
                return CreateErrorResponse(500, "Common.RetrieveError");
            }
        }

        /// <summary>
        /// 新增 / 修改期別匯率（遷移自 PeriodicalExchangeRateController.CommitItem）。
        /// 沿用舊版驗證：匯率須大於 0、期別（PeriodID）與幣別皆須有效；
        /// 修改時若幣別 / 期別變更（OrigPeriodId / OrigCurrencyId 與新值不同），則將資料列移動至新複合鍵。
        /// </summary>
        [HttpPost("CommitItem")]
        [ProducesResponseType(typeof(ResponseDto<ExchangeRateDatatableDto>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public IActionResult CommitItem([FromBody] ExchangeRateEditDto dto)
        {
            if (dto == null)
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            var errors = new List<string>();
            var model = CommitCore(dto, errors);

            if (errors.Count > 0)
            {
                return CreateBadRequestResponse("Common.SaveError", errors);
            }

            return CreateSuccessResponse(ToDatatableDto(model!), "Common.Saved");
        }

        /// <summary>
        /// 刪除期別匯率（遷移自 PeriodicalExchangeRateController.DeleteItem）。
        /// 以複合鍵（PeriodID + CurrencyID）刪除。
        /// </summary>
        /// <param name="periodId">期別識別碼（InvoicePeriodExchangeRate.PeriodID）</param>
        /// <param name="currencyId">幣別識別碼（InvoicePeriodExchangeRate.CurrencyID）</param>
        [HttpPost("DeleteItem")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public IActionResult DeleteItem([FromQuery] int periodId, [FromQuery] int currencyId)
        {
            // 沿用舊版原生 SQL 刪除。
            var count = models!.ExecuteCommand(
                "delete InvoicePeriodExchangeRate where PeriodID = {0} and CurrencyID = {1}",
                periodId, currencyId);

            if (count == 0)
            {
                return CreateBadRequestResponse("資料錯誤");
            }

            return CreateSuccessResponse("Common.Saved");
        }

        /// <summary>
        /// 下載匯率資料範本（遷移自舊版 PeriodicalExchangeRateController.GetExchangeRateSampleAsync）。
        /// 產生含「匯率」工作表（年度 / 期別 / 幣別代碼 / 匯率 + 一筆範例）與「幣別」工作表（幣別代碼 / 幣名）的 Excel。
        /// </summary>
        [HttpGet("GetExchangeRateSample")]
        [Produces(ExcelContentType)]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        public IActionResult GetExchangeRateSample()
        {
            // 「匯率」工作表 + 一筆範例資料（沿用舊版 2020 / 1 / USD / 27.2）。
            var rateTable = new DataTable("匯率");
            rateTable.Columns.Add(new DataColumn("年度", typeof(int)));
            rateTable.Columns.Add(new DataColumn("期別", typeof(int)));
            rateTable.Columns.Add(new DataColumn("幣別代碼", typeof(string)));
            rateTable.Columns.Add(new DataColumn("匯率", typeof(decimal)));

            var sample = rateTable.NewRow();
            sample[0] = 2020;
            sample[1] = 1;
            sample[2] = "USD";
            sample[3] = 27.2M;
            rateTable.Rows.Add(sample);

            // 「幣別」工作表：所有幣別代碼與幣名（沿用舊版）。
            var currencyTable = new DataTable("幣別");
            currencyTable.Columns.Add(new DataColumn("幣別代碼", typeof(string)));
            currencyTable.Columns.Add(new DataColumn("幣名", typeof(string)));

            var currencies = models!.GetTable<CurrencyType>()
                .Select(d => new { d.AbbrevName, d.CurrencyName })
                .ToList();
            foreach (var c in currencies)
            {
                var row = currencyTable.NewRow();
                row[0] = c.AbbrevName ?? string.Empty;
                row[1] = c.CurrencyName ?? string.Empty;
                currencyTable.Rows.Add(row);
            }

            using var ds = new DataSet();
            ds.Tables.Add(rateTable);
            ds.Tables.Add(currencyTable);

            using var xls = ds.ConvertToExcel();
            using var ms = new MemoryStream();
            xls.SaveAs(ms);

            return File(ms.ToArray(), ExcelContentType, "ExchangeRateSample.xlsx");
        }

        /// <summary>
        /// 匯入匯率 Excel（遷移自舊版 PeriodicalExchangeRateController.UploadExchangeRate）。
        /// 讀取「匯率」工作表，逐列新增 / 更新期別匯率，於原資料附加「處理狀態」欄後同步回傳結果檔。
        /// 成功時直接回傳結果 Excel（application/octet-stream）；未選檔 / 無匯率工作表則回傳 JSON 錯誤。
        /// </summary>
        /// <param name="excelFile">匯率資料 Excel 檔（單一檔案，須包含「匯率」工作表）</param>
        [HttpPost("UploadExchangeRate")]
        [Produces(ExcelContentType, "application/json")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 500)]
        public IActionResult UploadExchangeRate(IFormFile? excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                return CreateBadRequestResponse("未選取檔案或檔案上傳失敗");
            }

            try
            {
                // 儲存上傳檔至當日記錄目錄（沿用舊版以 Ticks 前綴避免檔名衝突）。
                var fileName = Path.Combine(
                    CommonLib.Core.Utility.Logger.LogDailyPath,
                    $"{DateTime.Now.Ticks}_{Path.GetFileName(excelFile.FileName)}");
                using (var fs = new FileStream(fileName, FileMode.Create))
                {
                    excelFile.CopyTo(fs);
                }

                using var ds = fileName.ImportExcelXLS();
                var table = ds.Tables.Count == 0
                    ? null
                    : ds.Tables.Cast<DataTable>().FirstOrDefault(t => t.TableName.Contains("匯率"));
                if (table == null)
                {
                    return CreateBadRequestResponse("Excel檔未包含【匯率】資料表");
                }

                table.Columns.Add(new DataColumn("處理狀態", typeof(string)));
                var statusIdx = table.Columns.Count - 1;

                foreach (DataRow row in table.Rows)
                {
                    try
                    {
                        var dto = new ExchangeRateEditDto
                        {
                            Year = row.GetData<int>(0),
                            PeriodNo = row.GetData<int>(1),
                            Currency = row.GetString(2),
                            ExchangeRate = row.GetData<decimal>(3),
                        };

                        var errors = new List<string>();
                        CommitCore(dto, errors);
                        if (errors.Count > 0)
                        {
                            row[statusIdx] = string.Join("、", errors);
                        }
                    }
                    catch (Exception ex)
                    {
                        row[statusIdx] = ex.Message;
                    }
                }

                using var xls = ds.ConvertToExcel();
                using var ms = new MemoryStream();
                xls.SaveAs(ms);

                return File(ms.ToArray(), ExcelContentType, "匯率資料(回應).xlsx");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error uploading exchange rate excel");
                return CreateErrorResponse(500, ex.Message);
            }
        }

        /// <summary>
        /// 新增 / 修改期別匯率的核心邏輯（遷移自舊版 CommitItem，供 API 端點與 Excel 匯入共用）。
        /// 驗證失敗時將訊息加入 <paramref name="errors"/> 並回傳 null；成功時回傳存檔後的實體。
        /// </summary>
        private InvoicePeriodExchangeRate? CommitCore(ExchangeRateEditDto dto, List<string> errors)
        {
            // 期別（PeriodID）：優先採用 PeriodId，否則由 Year*100 + PeriodNo 組成（沿用舊版）。
            int? periodId = dto.PeriodId;
            if (!periodId.HasValue && dto.Year.HasValue && dto.PeriodNo.HasValue)
            {
                periodId = dto.Year.Value * 100 + dto.PeriodNo.Value;
            }

            // 幣別：AbbrevName 對應 CurrencyType（NTD 視同 TWD，沿用舊版）。
            CurrencyType? currency = null;
            var abbrev = dto.Currency.GetEfficientString();
            if (abbrev != null)
            {
                if (abbrev == "NTD")
                {
                    abbrev = "TWD";
                }
                currency = models!.GetTable<CurrencyType>().FirstOrDefault(c => c.AbbrevName == abbrev);
            }

            if (!dto.ExchangeRate.HasValue || dto.ExchangeRate <= 0)
            {
                errors.Add("請輸入匯率");
            }
            if (!periodId.HasValue)
            {
                errors.Add("請輸入期別");
            }
            if (currency == null)
            {
                errors.Add("幣別錯誤");
            }

            if (errors.Count > 0)
            {
                return null;
            }

            // 修改且複合鍵變更時，先刪除原資料列，再以新複合鍵 upsert（等同舊版將資料列搬移至新期別 / 幣別）。
            if (dto.OrigPeriodId.HasValue && dto.OrigCurrencyId.HasValue
                && (dto.OrigPeriodId != periodId || dto.OrigCurrencyId != currency!.CurrencyID))
            {
                models!.ExecuteCommand(
                    "delete InvoicePeriodExchangeRate where PeriodID = {0} and CurrencyID = {1}",
                    dto.OrigPeriodId.Value, dto.OrigCurrencyId.Value);
            }

            // upsert：以（PeriodID, CurrencyID）查找既有資料列，無則新增（並確保 InvoicePeriod 存在）。
            var item = models!.GetTable<InvoicePeriodExchangeRate>()
                .FirstOrDefault(p => p.PeriodID == periodId && p.CurrencyID == currency!.CurrencyID);

            if (item == null)
            {
                if (!models.GetTable<InvoicePeriod>().Any(p => p.PeriodID == periodId))
                {
                    models.GetTable<InvoicePeriod>().Add(new InvoicePeriod { PeriodID = periodId!.Value });
                }

                item = new InvoicePeriodExchangeRate
                {
                    PeriodID = periodId!.Value,
                    CurrencyID = currency!.CurrencyID,
                };
                models.GetTable<InvoicePeriodExchangeRate>().Add(item);
            }

            item.ExchangeRate = dto.ExchangeRate!.Value;
            models.SubmitChanges();

            // 沿用舊版：將該年度 / 期別的字軌對應至此 InvoicePeriod。
            models.ExecuteCommand(
                "update InvoiceTrackCode set PeriodID = {0} where Year = {1} and PeriodNo = {2}",
                item.PeriodID, item.PeriodID / 100, item.PeriodID % 100);

            return item;
        }

        private ExchangeRateDatatableDto ToDatatableDto(InvoicePeriodExchangeRate model)
        {
            // 幣別代碼 / 名稱以 CurrencyID 取得（新增列的導覽屬性可能尚未載入）。
            var currency = models!.GetTable<CurrencyType>()
                .FirstOrDefault(c => c.CurrencyID == model.CurrencyID);

            return new ExchangeRateDatatableDto
            {
                PeriodId = model.PeriodID,
                Year = model.PeriodID / 100,
                PeriodNo = model.PeriodID % 100,
                CurrencyId = model.CurrencyID,
                Currency = currency?.AbbrevName,
                CurrencyName = currency?.CurrencyName,
                ExchangeRate = model.ExchangeRate,
            };
        }
    }
}
