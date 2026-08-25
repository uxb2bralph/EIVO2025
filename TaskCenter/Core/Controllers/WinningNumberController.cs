using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClosedXML.Excel;
using CommonLib.Core.DataWork;
using CommonLib.Core.Utility;
using CommonLib.DataAccess;
using CommonLib.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelCore.DataEntity;
using ModelCore.DTOs;
using ModelCore.Helper;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Interfaces;

namespace TaskCenter.Core.Controllers
{
    /// <summary>
    /// 中獎號碼維護 API（遷移自 WebHome WinningNumberController 之 Index / Inquire、列管理動作、
    /// 發票對獎 / 清除中獎發票，以及雲端發票中獎清冊範本下載 / Excel 上傳）。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class WinningNumberController : ApiBaseController
    {
        private readonly IWinningNumberService _winningNumberService;

        public WinningNumberController(
            IWinningNumberService winningNumberService,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
            _winningNumberService = winningNumberService;
        }

        /// <summary>
        /// 查詢中獎號碼（依發票年度 + 期別）。對應舊版 WinningNumberController.Inquire。
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseDto<List<WinningNumberDatatableDto>>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 500)]
        public async Task<IActionResult> GetList([FromQuery] WinningNumberQueryDto queryDto)
        {
            // 對應舊版 Inquire：Year / PeriodNo 皆為必填。
            if (queryDto == null || !queryDto.Year.HasValue)
            {
                return CreateBadRequestResponse("請選擇年份!!");
            }
            if (!queryDto.PeriodNo.HasValue)
            {
                return CreateBadRequestResponse("請選擇期別!!");
            }

            try
            {
                var result = await _winningNumberService.GetListAsync(queryDto);
                return CreateSuccessResponse(result, "Common.Retrieved");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving winning numbers");
                return CreateErrorResponse(500, "Common.RetrieveError");
            }
        }

        /// <summary>
        /// 新增 / 修改中獎號碼（遷移自 WinningNumberController.CommitItem）。
        /// 沿用舊版驗證：特別獎 / 特獎 / 頭獎為 8 碼數字，增開六獎為 3 碼數字；
        /// 新增時須有期別（1~6）與年度，且同年度 / 期別 / 號碼不可重複。
        /// 頭獎會自動衍生二~六獎（以號碼末碼），修改頭獎時先清除舊衍生再重建。
        /// </summary>
        [HttpPost("CommitItem")]
        [ProducesResponseType(typeof(ResponseDto<WinningNumberDatatableDto>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public IActionResult CommitItem([FromBody] WinningNumberEditDto dto)
        {
            if (dto == null)
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            var errors = new List<string>();
            var winningNo = dto.WinningNo.GetEfficientString();

            // 號碼格式驗證（依獎別；沿用舊版 CommitItem）。
            if (dto.Rank == (int)Naming.EditableWinningPrizeType.特別獎
                || dto.Rank == (int)Naming.EditableWinningPrizeType.特獎
                || dto.Rank == (int)Naming.EditableWinningPrizeType.頭獎)
            {
                if (winningNo == null || !Regex.IsMatch(winningNo, "^[0-9]{8}$"))
                {
                    errors.Add("中獎號碼為8碼數字!!");
                }
            }
            else if (dto.Rank == (int)Naming.EditableWinningPrizeType.增開六獎)
            {
                if (winningNo == null || !Regex.IsMatch(winningNo, "^[0-9]{3}$"))
                {
                    errors.Add("中獎號碼為3碼數字!!");
                }
            }
            else
            {
                errors.Add("獎別錯誤!!");
            }

            var table = models!.GetTable<UniformInvoiceWinningNumber>();
            var model = dto.WinningId.HasValue
                ? table.FirstOrDefault(t => t.WinningID == dto.WinningId.Value)
                : null;

            // 新增（含指定 WinningId 但查無資料）時，驗證期別 / 年度並檢查號碼是否重複。
            if (model == null)
            {
                if (!dto.Period.HasValue || dto.Period > 6 || dto.Period < 1)
                {
                    errors.Add("請選擇期別!!");
                }
                else if (!dto.Year.HasValue)
                {
                    errors.Add("請選擇年份!!");
                }
                else if (winningNo != null
                    && table.Any(t => t.Year == dto.Year && t.WinningNO == winningNo && t.Period == dto.Period))
                {
                    errors.Add("中獎號碼重複!!");
                }
            }

            if (errors.Count > 0)
            {
                return CreateBadRequestResponse("Common.SaveError", errors);
            }

            if (model == null)
            {
                model = new UniformInvoiceWinningNumber
                {
                    Year = dto.Year!.Value,
                    Period = dto.Period!.Value,
                };
                table.Add(model);
            }
            else if (model.Rank == (int)Naming.WinningPrizeType.頭獎)
            {
                // 修改既有頭獎前，先清除依舊號碼衍生的二~六獎（沿用舊版 DeleteAllOnSubmit）。
                RemoveDerivedPrizes(model.Year, model.Period, model.WinningNO);
            }

            model.WinningNO = winningNo!;
            model.Year = dto.Year!.Value;
            model.Period = dto.Period!.Value;
            model.Rank = dto.Rank!.Value;
            model.PrizeType = ((Naming.WinningPrizeType)dto.Rank.Value).ToString();
            model.Bonus = Naming.WinningBonus[dto.Rank.Value];

            if (model.Rank == (int)Naming.WinningPrizeType.頭獎)
            {
                // 頭獎自動衍生二~六獎（以號碼末碼；沿用舊版 createWinningNo）。
                CreateWinningNo(table, model.Year, model.Period, winningNo!.Substring(1), Naming.WinningPrizeType.二獎);
                CreateWinningNo(table, model.Year, model.Period, winningNo.Substring(2), Naming.WinningPrizeType.三獎);
                CreateWinningNo(table, model.Year, model.Period, winningNo.Substring(3), Naming.WinningPrizeType.四獎);
                CreateWinningNo(table, model.Year, model.Period, winningNo.Substring(4), Naming.WinningPrizeType.五獎);
                CreateWinningNo(table, model.Year, model.Period, winningNo.Substring(5), Naming.WinningPrizeType.六獎);
            }

            models!.SubmitChanges();

            var result = ToDatatableDto(model);
            return CreateSuccessResponse(result, "Common.Saved");
        }

        /// <summary>
        /// 刪除中獎號碼（遷移自 WinningNumberController.DeleteItem）。
        /// 刪除頭獎時一併刪除其衍生的二~六獎。
        /// </summary>
        /// <param name="id">中獎號碼識別碼（UniformInvoiceWinningNumber.WinningID）</param>
        [HttpPost("DeleteItem")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public IActionResult DeleteItem([FromQuery] int id)
        {
            var item = models!.DeleteAny<UniformInvoiceWinningNumber>(d => d.WinningID == id);
            if (item == null)
            {
                return CreateBadRequestResponse("中獎號碼資料錯誤!!");
            }

            if (item.Rank == (int)Naming.WinningPrizeType.頭獎)
            {
                // 頭獎的衍生二~六獎（以號碼末碼）一併刪除（沿用舊版 DeleteAll）。
                models.DeleteAll<UniformInvoiceWinningNumber>(u => u.Period == item.Period && u.Year == item.Year && u.WinningNO == item.WinningNO.Substring(1)); // 二獎
                models.DeleteAll<UniformInvoiceWinningNumber>(u => u.Period == item.Period && u.Year == item.Year && u.WinningNO == item.WinningNO.Substring(2)); // 三獎
                models.DeleteAll<UniformInvoiceWinningNumber>(u => u.Period == item.Period && u.Year == item.Year && u.WinningNO == item.WinningNO.Substring(3)); // 四獎
                models.DeleteAll<UniformInvoiceWinningNumber>(u => u.Period == item.Period && u.Year == item.Year && u.WinningNO == item.WinningNO.Substring(4)); // 五獎
                models.DeleteAll<UniformInvoiceWinningNumber>(u => u.Period == item.Period && u.Year == item.Year && u.WinningNO == item.WinningNO.Substring(5)); // 六獎
            }

            return CreateSuccessResponse("Common.Saved");
        }

        /// <summary>
        /// 執行發票對獎（遷移自 WinningNumberController.MatchWinningInvoiceNo）。
        /// 呼叫 dbo.MatchWinningInvoiceNo 儲存程序建立中獎發票對應，並對需通知者發送中獎通知。
        /// </summary>
        [HttpPost("MatchWinningInvoiceNo")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 500)]
        public async Task<IActionResult> MatchWinningInvoiceNo([FromBody] WinningActionDto dto)
        {
            var validation = await ValidateHasWinningNumbers(dto);
            if (validation != null)
            {
                return validation;
            }

            try
            {
                // dbo.MatchWinningInvoiceNo(@Year, @PeriodNo)（舊版為 L2S 之 DataContext.MatchWinningInvoiceNo）。
                models!.ExecuteCommand("EXEC dbo.MatchWinningInvoiceNo @Year = {0}, @PeriodNo = {1}", dto!.Year!.Value, dto.PeriodNo!.Value);

                var invoiceIDs = models.PromptWinningInvoiceForNotification(dto.Year!.Value, dto.PeriodNo!.Value)
                    .Select(i => i.InvoiceID)
                    .ToList();
                invoiceIDs.NotifyWinningInvoice(false);

                return CreateSuccessResponse("對獎作業完成!!");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error matching winning invoices for {Year}/{Period}", dto?.Year, dto?.PeriodNo);
                return CreateErrorResponse(500, "對獎作業失敗!!");
            }
        }

        /// <summary>
        /// 清除中獎發票（遷移自 WinningNumberController.ClearWinningInvoiceNo）。
        /// 刪除指定年度 / 期別已建立的中獎發票對應（InvoiceWinningNumber）。
        /// </summary>
        [HttpPost("ClearWinningInvoiceNo")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 500)]
        public async Task<IActionResult> ClearWinningInvoiceNo([FromBody] WinningActionDto dto)
        {
            var validation = await ValidateHasWinningNumbers(dto);
            if (validation != null)
            {
                return validation;
            }

            try
            {
                // 沿用舊版原生 SQL：以 UniformInvoiceWinningNumber 期別 / 年度為條件刪除 InvoiceWinningNumber。
                models!.ExecuteCommand(@"
                    DELETE FROM InvoiceWinningNumber
                    FROM     UniformInvoiceWinningNumber INNER JOIN
                                    InvoiceWinningNumber ON UniformInvoiceWinningNumber.WinningID = InvoiceWinningNumber.WinningID
                    WHERE   (UniformInvoiceWinningNumber.Period = {0}) AND (UniformInvoiceWinningNumber.Year = {1})", dto!.PeriodNo!.Value, dto.Year!.Value);

                return CreateSuccessResponse("中獎發票已清除完成!!");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error clearing winning invoices for {Year}/{Period}", dto?.Year, dto?.PeriodNo);
                return CreateErrorResponse(500, "清除中獎發票失敗!!");
            }
        }

        /// <summary>
        /// 下載雲端發票中獎清冊範本（遷移自舊版 AshxHelper.GetSample?data=WinningNo）。
        /// 產生含「期別 / 字軌 / 號碼 / 中獎獎別 / 中獎獎金」欄位與一筆範例資料的 Excel。
        /// </summary>
        [HttpGet("DownloadSample")]
        [Produces("application/octet-stream")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        public IActionResult DownloadSample()
        {
            // 欄位對應舊版 GetSample「WinningNo」分支。
            var table = new DataTable("中獎清冊");
            table.Columns.Add(new DataColumn("期別", typeof(string)));
            table.Columns.Add(new DataColumn("字軌", typeof(string)));
            table.Columns.Add(new DataColumn("號碼", typeof(string)));
            table.Columns.Add(new DataColumn("中獎獎別", typeof(string)));
            table.Columns.Add(new DataColumn("中獎獎金", typeof(int)));

            // 範例期別 = 上一個雙月期別（沿用舊版計算）。
            var sampleDate = new DateTime(DateTime.Today.Year, (DateTime.Today.Month + 1) / 2 * 2, 1).AddMonths(-2);
            var row = table.NewRow();
            row[0] = $"{sampleDate.Year - 1911:000}{sampleDate.Month:00}";
            row[1] = "XX";
            row[2] = "01234567";
            row[3] = "D";
            row[4] = 500;
            table.Rows.Add(row);

            using var ds = new DataSet();
            ds.Tables.Add(table);

            using var xls = ds.ConvertToExcel();
            using var ms = new MemoryStream();
            xls.SaveAs(ms);

            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "WinningSample.xlsx");
        }

        /// <summary>
        /// 上傳雲端發票中獎清冊 Excel（遷移自舊版 WinningNumberController.UploadWinningNo）。
        /// 儲存上傳檔並建立 proc.ProcessRequest 工作，於背景比對發票、寫入中獎資料並發送中獎通知，
        /// 產生含「處理狀態」欄位的結果檔。前端以回傳的 taskId 輪詢 CheckProcess，完成後由 DownloadResult 下載。
        /// </summary>
        /// <param name="excelFile">中獎清冊 Excel 檔（單一檔案）</param>
        [HttpPost("UploadWinningNo")]
        [ProducesResponseType(typeof(ResponseDto<WinningNoUploadResultDto>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 500)]
        public async Task<IActionResult> UploadWinningNo(IFormFile? excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                return CreateBadRequestResponse("未選取檔案或檔案上傳失敗!!");
            }

            try
            {
                // 儲存上傳檔至當日記錄目錄（沿用舊版以 Ticks 前綴避免檔名衝突）。
                var requestPath = Path.Combine(CommonLib.Core.Utility.Logger.LogDailyPath, $"{DateTime.Now.Ticks}_{Path.GetFileName(excelFile.FileName)}");
                using (var fs = new FileStream(requestPath, FileMode.Create))
                {
                    await excelFile.CopyToAsync(fs);
                }

                var responsePath = Path.Combine(CommonLib.Core.Utility.Logger.LogDailyPath, Guid.NewGuid().ToString() + ".xlsx");

                var processItem = new ProcessRequest
                {
                    Sender = GetCurrentUid(),
                    SubmitDate = DateTime.Now,
                    ProcessStart = DateTime.Now,
                    RequestPath = requestPath,
                    ResponsePath = responsePath,
                };
                models!.GetTable<ProcessRequest>().Add(processItem);
                models.SubmitChanges();

                // 背景處理（沿用舊版 Task.Run；使用獨立 DbContext 避免與請求範圍衝突）。
                ProcessWinningNoExcel(processItem.TaskID, responsePath, requestPath);

                return CreateSuccessResponse(
                    new WinningNoUploadResultDto
                    {
                        TaskId = processItem.TaskID,
                        FileDownloadName = "中獎發票回應.xlsx",
                    },
                    "檔案已上傳，處理中!!");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error uploading winning number excel");
                return CreateErrorResponse(500, "檔案上傳失敗!!");
            }
        }

        /// <summary>
        /// 查詢雲端發票中獎清冊處理狀態（對應舊版 DataExchange.CheckResource 之輪詢）。
        /// 僅可查詢自己送出的工作；結果檔已產生即視為完成。
        /// </summary>
        /// <param name="taskId">處理工作識別碼（來自 UploadWinningNo 回傳的 taskId）</param>
        [HttpGet("CheckProcess")]
        [ProducesResponseType(typeof(ResponseDto<WinningNoProcessStatusDto>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult CheckProcess([FromQuery] int taskId)
        {
            var taskItem = FindOwnedTask(taskId);
            if (taskItem == null)
            {
                return CreateNotFoundResponse("找不到處理工作或無權限存取!!");
            }

            // 結果檔存在即完成（沿用舊版 CheckResource 以 ResponsePath 檔案是否存在為完成訊號）。
            var completed = taskItem.ProcessComplete.HasValue
                && !string.IsNullOrEmpty(taskItem.ResponsePath)
                && System.IO.File.Exists(taskItem.ResponsePath);

            return CreateSuccessResponse(
                new WinningNoProcessStatusDto
                {
                    Completed = completed,
                    Failed = taskItem.Log != null,
                    Message = taskItem.Log?.DataContent,
                },
                "Common.Retrieved");
        }

        /// <summary>
        /// 下載雲端發票中獎清冊處理結果檔（對應舊版 DataExchange.DownloadResource）。
        /// 僅可下載自己送出且已完成的工作結果。
        /// </summary>
        /// <param name="taskId">處理工作識別碼（來自 UploadWinningNo 回傳的 taskId）</param>
        [HttpGet("DownloadResult")]
        [Produces("application/octet-stream")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult DownloadResult([FromQuery] int taskId)
        {
            var taskItem = FindOwnedTask(taskId);
            if (taskItem == null
                || string.IsNullOrEmpty(taskItem.ResponsePath)
                || !System.IO.File.Exists(taskItem.ResponsePath))
            {
                return CreateNotFoundResponse("結果檔尚未產生或無權限存取!!");
            }

            return PhysicalFile(taskItem.ResponsePath,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "中獎發票回應.xlsx");
        }

        /// <summary>
        /// 取當前登入者的 UID（來自 ClaimTypes.NameIdentifier；對應舊版 HttpContext.GetUser()?.UID）。
        /// </summary>
        private int? GetCurrentUid()
        {
            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : null;
        }

        /// <summary>
        /// 依 TaskID 取得屬於當前登入者（Sender）的處理工作；查無或非本人則回傳 null（避免越權存取）。
        /// </summary>
        private ProcessRequest? FindOwnedTask(int taskId)
        {
            var uid = GetCurrentUid();
            if (uid == null)
            {
                return null;
            }

            return models!.GetTable<ProcessRequest>()
                .FirstOrDefault(p => p.TaskID == taskId && p.Sender == uid);
        }

        /// <summary>
        /// 背景比對中獎清冊 Excel（遷移自舊版 WinningNumberController.ProcessWinningNoExcel）。
        /// 逐列以「字軌 + 號碼 + 期別」比對發票，建立 / 更新 InvoiceWinningNumber（獎別 / 獎金），
        /// 對成功列發送中獎通知，並在原資料附加「處理狀態」欄後輸出結果檔；完成時標記 ProcessComplete。
        /// 使用獨立 DbContext（背景執行，不可沿用請求範圍的 _models）。
        /// </summary>
        private static void ProcessWinningNoExcel(int taskId, string resultFile, string excelPath)
        {
            Task.Run(() =>
            {
                try
                {
                    using var db = new GenericDbContext<ApplicationDbContext>(new ApplicationDbContext());
                    using var ds = excelPath.ImportExcelXLS();

                    Exception? exception = null;
                    if (ds.Tables.Count > 0)
                    {
                        var winningIDs = new List<int>();
                        var table = ds.Tables[0];
                        table.Columns.Add(new DataColumn("處理狀態", typeof(string)));
                        var statusIdx = table.Columns.Count - 1;

                        // 前五欄（期別 / 字軌 / 號碼 / 中獎獎別 / 中獎獎金）皆需有值（沿用舊版篩選）。
                        var assumedRows = table.Rows.Cast<DataRow>()
                            .Where(r => !r.IsNull(0) && !r.IsNull(1) && !r.IsNull(2) && !r.IsNull(3) && !r.IsNull(4));

                        foreach (var row in assumedRows)
                        {
                            try
                            {
                                var periodNo = row.GetString(0).GetEfficientString();
                                var trackCode = row.GetString(1).GetEfficientString();
                                var no = row.GetString(2).GetEfficientString();

                                var items = db.GetTable<InvoiceItem>()
                                    .Where(i => i.TrackCode == trackCode)
                                    .Where(i => i.No == no)
                                    .ToList();

                                // 期別格式 = 民國年3碼 + 雙月期別2碼（沿用舊版計算）。
                                var invoice = items.FirstOrDefault(i =>
                                    $"{i.InvoiceDate!.Value.Year - 1911:000}{(i.InvoiceDate.Value.Month + 1) / 2 * 2:00}" == periodNo);

                                if (invoice == null)
                                {
                                    row[statusIdx] = "發票號碼不存在";
                                    continue;
                                }

                                var winningInvoice = invoice.InvoiceWinningNumber;
                                if (winningInvoice == null)
                                {
                                    winningInvoice = new InvoiceWinningNumber { InvoiceID = invoice.InvoiceID };
                                    db.GetTable<InvoiceWinningNumber>().Add(winningInvoice);
                                }

                                winningInvoice.PrizeType = row.GetString(3).GetEfficientString();
                                winningInvoice.Bonus = row.GetData<int>(4);

                                db.SubmitChanges();
                                winningIDs.Add(invoice.InvoiceID);
                            }
                            catch (Exception ex)
                            {
                                exception ??= ex;
                                CommonLib.Core.Utility.Logger.Error(ex);
                                row[statusIdx] = ex.Message;
                            }
                        }

                        if (winningIDs.Count > 0)
                        {
                            winningIDs.NotifyWinningInvoice(false);
                        }
                    }

                    using (var xls = ds.ConvertToExcel())
                    {
                        xls.SaveAs(resultFile);
                    }

                    var taskItem = db.GetTable<ProcessRequest>().FirstOrDefault(t => t.TaskID == taskId);
                    if (taskItem != null)
                    {
                        if (exception != null)
                        {
                            taskItem.Log = new ExceptionLog { DataContent = exception.Message };
                        }
                        taskItem.ProcessComplete = DateTime.Now;
                        db.SubmitChanges();
                    }
                }
                catch (Exception ex)
                {
                    CommonLib.Core.Utility.Logger.Error(ex);
                }
            });
        }

        /// <summary>
        /// 對獎 / 清除作業共用驗證：Year / PeriodNo 必填，且該年度 / 期別須已建立中獎號碼
        /// （對應舊版先呼叫 Inquire 並檢查 items.Count() > 0 的守門邏輯）。
        /// 通過驗證回傳 null，否則回傳對應的錯誤 IActionResult。
        /// </summary>
        private async Task<IActionResult?> ValidateHasWinningNumbers(WinningActionDto? dto)
        {
            if (dto == null || !dto.Year.HasValue)
            {
                return CreateBadRequestResponse("請選擇年份!!");
            }
            if (!dto.PeriodNo.HasValue)
            {
                return CreateBadRequestResponse("請選擇期別!!");
            }

            var hasWinning = await models!.GetTable<UniformInvoiceWinningNumber>()
                .AnyAsync(w => w.Year == dto.Year && w.Period == dto.PeriodNo);
            if (!hasWinning)
            {
                return CreateBadRequestResponse("查無中獎號碼資料，請先設定中獎號碼!!");
            }

            return null;
        }

        /// <summary>
        /// 刪除依頭獎號碼末碼衍生的二~六獎（延後提交，隨後 SubmitChanges 一併寫入）。
        /// 對應舊版 CommitItem 修改頭獎時的 DeleteAllOnSubmit 區塊。
        /// </summary>
        private void RemoveDerivedPrizes(int year, int period, string winningNo)
        {
            for (int start = 1; start <= 5; start++)
            {
                var derived = winningNo.Substring(start);
                models!.DeleteAllOnSubmit<UniformInvoiceWinningNumber>(
                    u => u.Period == period && u.Year == year && u.WinningNO == derived);
            }
        }

        /// <summary>
        /// 建立衍生獎項（沿用舊版 createWinningNo）。
        /// </summary>
        private void CreateWinningNo(DbSet<UniformInvoiceWinningNumber> table, int year, int period, string winningNo, Naming.WinningPrizeType prizeType)
        {
            table.Add(new UniformInvoiceWinningNumber
            {
                Year = year,
                Period = period,
                WinningNO = winningNo,
                PrizeType = prizeType.ToString(),
                Rank = (int)prizeType,
                Bonus = Naming.WinningBonus[(int)prizeType],
            });
        }

        private static WinningNumberDatatableDto ToDatatableDto(UniformInvoiceWinningNumber model)
        {
            return new WinningNumberDatatableDto
            {
                WinningId = model.WinningID,
                Year = model.Year,
                Period = model.Period,
                Rank = model.Rank,
                PrizeType = model.PrizeType,
                Bonus = model.Bonus,
                WinningNo = model.WinningNO,
                Editable = model.Rank == (int)Naming.EditableWinningPrizeType.特別獎
                    || model.Rank == (int)Naming.EditableWinningPrizeType.特獎
                    || model.Rank == (int)Naming.EditableWinningPrizeType.頭獎
                    || model.Rank == (int)Naming.EditableWinningPrizeType.增開六獎,
            };
        }
    }
}
