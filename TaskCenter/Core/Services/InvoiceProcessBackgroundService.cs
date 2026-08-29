using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Business.Helper.InvoiceProcessor;
using CommonLib.Utility;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelCore.DataEntity;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;
using ModelCore.Schema.TXN;
using TaskCenter.Core.Interfaces;
using TaskCenter.Properties;

namespace TaskCenter.Core.Services
{
    /// <summary>
    /// 發票處理背景服務：執行 InvoiceService 各 Apply* 端點收下的作業——發票存證（帶號／自動配號）、
    /// 發票作廢、折讓單存證、折讓單作廢（見 <see cref="InvoiceProcessJobKind"/>）。
    /// 原本收單端點以 <c>Task.Run</c> 直接把 InvoiceManager 丟到執行緒集區處理，改為
    /// 統一由本服務自佇列（<see cref="IInvoiceProcessQueue"/>）取件執行：
    /// <list type="bullet">
    /// <item>作業不再綁在請求生命週期上（請求 DbContext 釋放後不會影響背景處理）。</item>
    /// <item>可控制併發數（AppSettings.InvoiceProcessQueue.WorkerCount，預設 1，避免自動配號互搶號碼）。</item>
    /// <item>例外集中在此記錄，不會變成無人接手的 unobserved task exception。</item>
    /// <item>服務停止時先關閉佇列寫入端，讓已收下的作業在主機關閉逾時內排空。</item>
    /// </list>
    /// 每筆作業處理結束後回報佇列狀態（<c>Acknowledge</c> / <c>Abandon</c>）：檔案佇列
    /// （<c>InvoiceProcessFileQueue</c>）據此清除或保留落地的 JSON 檔案，行程中斷重啟後才能
    /// 接續處理未完成的作業；使用記憶體佇列（AppSettings.InvoiceProcessQueue.Persistent = false）
    /// 時，行程被強制中止未處理完的作業會遺失（與原 Task.Run 行為相同）。
    /// </summary>
    public class InvoiceProcessBackgroundService : BackgroundService
    {
        private readonly IInvoiceProcessQueue _queue;
        private readonly ILogger<InvoiceProcessBackgroundService> _logger;

        public InvoiceProcessBackgroundService(IInvoiceProcessQueue queue, ILogger<InvoiceProcessBackgroundService> logger)
        {
            _queue = queue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int workerCount = Math.Max(1, AppSettings.Default.InvoiceProcessQueue.WorkerCount);
            _logger.LogInformation("發票處理背景服務啟動，Worker 數量 = {WorkerCount}。", workerCount);

            // 以 Task.Run 起 Worker：取件後的存證處理是同步（阻塞）作業，
            // 不可佔用 Host 啟動時呼叫 ExecuteAsync 的執行緒。
            var workers = Enumerable.Range(0, workerCount)
                .Select(id => Task.Run(() => RunWorkerAsync(id, stoppingToken), CancellationToken.None))
                .ToArray();

            await Task.WhenAll(workers);

            _logger.LogInformation("發票處理背景服務結束，佇列剩餘 {Count} 筆。", _queue.Count);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            // 先關閉寫入端，Worker 才能在讀完剩餘作業後自然結束（排空）。
            _queue.Complete();

            int pending = _queue.Count;
            if (pending > 0)
            {
                _logger.LogWarning("發票處理背景服務停止中，尚有 {Count} 筆作業待處理。", pending);
            }

            await base.StopAsync(cancellationToken);
        }

        /// <summary>
        /// 取件迴圈。讀取刻意不套用 stoppingToken：關閉時由 <see cref="StopAsync"/> 關閉寫入端，
        /// 讓佇列在主機關閉逾時（預設 30 秒）內排空；逾時後由主機強制結束。
        /// </summary>
        private async Task RunWorkerAsync(int workerId, CancellationToken stoppingToken)
        {
            try
            {
                await foreach (var job in _queue.ReadAllAsync(CancellationToken.None))
                {
                    ProcessJob(workerId, job);
                }
            }
            catch (OperationCanceledException)
            {
                // 正常關閉
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發票處理背景服務 Worker {WorkerId} 異常結束。", workerId);
            }
        }

        /// <summary>
        /// 執行單筆作業（原 InvoiceServiceController 各 Apply* 端點內 Task.Run 的內容）。
        /// </summary>
        private void ProcessJob(int workerId, InvoiceProcessJob job)
        {
            String content = DescribeContent(job);
            var waiting = DateTime.Now - job.SubmitDate;

            try
            {
                Exception? invalid = CheckJob(job);
                if (invalid != null)
                {
                    _logger.LogError("{Kind} 作業取消：{Message}（AgentID = {AgentID}）。",
                        job.Kind, invalid.Message, job.AgentID);

                    _queue.Abandon(job, invalid);
                    return;
                }

                Root result = CreateMessageToken();

                using InvoiceManagerV2 manager = CreateManager(job);

                // 憑證以本服務的 DbContext 重新載入（不可沿用請求的追蹤實體）。
                OrganizationToken? token = manager.GetTable<OrganizationToken>()
                    .Where(t => t.CompanyID == job.AgentID)
                    .FirstOrDefault();

                if (token == null)
                {
                    _logger.LogError("{Kind} 作業取消：找不到營業人憑證資料，AgentID = {AgentID}，{Content}。",
                        job.Kind, job.AgentID, content);

                    // 保留作業內容（檔案佇列移至 failed）：憑證補齊後可人工重送。
                    _queue.Abandon(job, new InvalidOperationException($"找不到營業人憑證資料，AgentID = {job.AgentID}。"));
                    return;
                }

                switch (job.Kind)
                {
                    case InvoiceProcessJobKind.UploadInvoice:
                        manager.UploadInvoice(job.Invoice!, result, token);
                        break;

                    case InvoiceProcessJobKind.UploadInvoiceAutoTrackNo:
                        manager.ApplyInvoiceDate = job.ApplyInvoiceDate;
                        manager.UploadInvoiceAutoTrackNo(job.Invoice!, result, token);
                        break;

                    case InvoiceProcessJobKind.UploadInvoiceCancellation:
                        manager.UploadInvoiceCancellation(result, job.CancelInvoice!, token);
                        break;

                    case InvoiceProcessJobKind.UploadAllowance:
                        manager.UploadAllowance(result, job.Allowance!, token);
                        break;

                    case InvoiceProcessJobKind.UploadAllowanceCancellation:
                        manager.UploadAllowanceCancellation(result, job.CancelAllowance!, token);
                        break;
                }

                LogResult(workerId, job, content, waiting, result);

                // 作業已執行完畢（含業務層面的部分失敗，該類錯誤由 ExceptionNotification 通知營業人），
                // 回報佇列釋放作業內容；不可重跑，避免重複開立／重複作廢。
                _queue.Acknowledge(job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Kind} 作業失敗，AgentID = {AgentID}，{Content}。", job.Kind, job.AgentID, content);

                // 保留作業內容供事後檢視；因可能已處理部分資料，不自動重跑。
                _queue.Abandon(job, ex);
            }
        }

        /// <summary>
        /// 建立作業所需的 InvoiceManager。發票存證沿用收單端點的判斷（F0401_Json_CBE 使用
        /// <c>InvoiceManagerForCBE</c>）；作廢／折讓的作業不帶 <c>ClientID</c>、<c>ProcessType</c>
        /// （原收單端點即以 <c>new InvoiceManagerV3 { }</c> 處理），故一律落在 V3 分支。
        /// </summary>
        private static InvoiceManagerV2 CreateManager(InvoiceProcessJob job)
        {
            return job.ProcessType == Naming.InvoiceProcessType.F0401_Json_CBE
                ? new InvoiceManagerForCBE { InvoiceClientID = job.ClientID, ProcessType = job.ProcessType }
                : new InvoiceManagerV3 { InvoiceClientID = job.ClientID, ProcessType = job.ProcessType };
        }

        /// <summary>
        /// 檢查作業種類與內容是否相符。不相符者（含種類為 <see cref="InvoiceProcessJobKind.Unknown"/>
        /// 的舊格式作業檔）一律不處理，交由 <c>Abandon</c> 保留供人工檢視，避免誤用其他種類的處理方式。
        /// </summary>
        private static Exception? CheckJob(InvoiceProcessJob job)
        {
            if (job.Kind == InvoiceProcessJobKind.Unknown)
            {
                return new InvalidOperationException("作業種類未指定（作業內容格式不符）。");
            }

            bool hasContent = job.Kind switch
            {
                InvoiceProcessJobKind.UploadInvoice or InvoiceProcessJobKind.UploadInvoiceAutoTrackNo => job.Invoice != null,
                InvoiceProcessJobKind.UploadInvoiceCancellation => job.CancelInvoice != null,
                InvoiceProcessJobKind.UploadAllowance => job.Allowance != null,
                InvoiceProcessJobKind.UploadAllowanceCancellation => job.CancelAllowance != null,
                _ => false,
            };

            return hasContent ? null : new InvalidOperationException($"作業內容為空，種類 = {job.Kind}。");
        }

        /// <summary>依作業種類描述處理內容與筆數，供記錄使用。</summary>
        private static String DescribeContent(InvoiceProcessJob job)
        {
            return job.Kind switch
            {
                InvoiceProcessJobKind.UploadInvoice or InvoiceProcessJobKind.UploadInvoiceAutoTrackNo
                    => $"發票 {job.Invoice?.Invoice?.Length ?? 0} 筆",
                InvoiceProcessJobKind.UploadInvoiceCancellation
                    => $"作廢發票 {job.CancelInvoice?.CancelInvoice?.Length ?? 0} 筆",
                InvoiceProcessJobKind.UploadAllowance
                    => $"折讓單 {job.Allowance?.Allowance?.Length ?? 0} 筆",
                InvoiceProcessJobKind.UploadAllowanceCancellation
                    => $"作廢折讓單 {job.CancelAllowance?.CancelAllowance?.Length ?? 0} 筆",
                _ => "內容不明",
            };
        }

        private void LogResult(int workerId, InvoiceProcessJob job, String content, TimeSpan waiting, Root result)
        {
            if (result.Result.value == 1)
            {
                _logger.LogInformation(
                    "{Kind} 作業完成，Worker = {WorkerId}，AgentID = {AgentID}，{Content}，等待 {Waiting} 秒。",
                    job.Kind, workerId, job.AgentID, content, Math.Round(waiting.TotalSeconds, 1));
                return;
            }

            // 處理失敗（含部分失敗）時記錄回應內容，供事後對帳；細項錯誤原本即由
            // ExceptionNotification 通知營業人，此處僅補記錄。
            _logger.LogWarning(
                "{Kind} 作業未完全成功，Worker = {WorkerId}，AgentID = {AgentID}，{Content}，回應 = {Result}。",
                job.Kind, workerId, job.AgentID, content, result.JsonStringify());
        }

        private static Root CreateMessageToken()
        {
            return new Root
            {
                UXB2B = "電子發票系統",
                Result = new RootResult
                {
                    timeStamp = DateTime.Now,
                    value = 0
                }
            };
        }
    }
}
