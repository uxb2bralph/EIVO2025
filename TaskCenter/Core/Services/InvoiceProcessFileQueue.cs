using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using CommonLib.Utility;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TaskCenter.Core.Interfaces;
using TaskCenter.Properties;

namespace TaskCenter.Core.Services
{
    /// <summary>
    /// <see cref="IInvoiceProcessQueue"/> 的檔案系統實作：作業內容以 JSON 檔案落地，
    /// 行程中斷（強制結束、IIS 回收、主機重開）後重新啟動時可接續處理未完成的作業。
    /// <para>
    /// 佇列根目錄取自 <c>AppSettings.InvoiceProcessQueue.StoragePath</c>，其下分為數個子目錄：
    /// <list type="table">
    /// <item><term>pending</term><description>待處理；收單端點寫入，Worker 由此取件（依檔名時間序）。</description></item>
    /// <item><term>processing</term><description>已取件、正在處理；完成後刪除（或歸檔），失敗則移到 failed。</description></item>
    /// <item><term>failed</term><description>處理過程拋出例外的作業，另存 .error.txt 記錄例外內容。</description></item>
    /// <item><term>interrupted</term><description>行程中斷時仍在 processing 的作業（可能已存證部分發票）。</description></item>
    /// <item><term>archive</term><description><c>ArchiveCompletedJobs</c> 為 true 時保留已完成的作業內容。</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// 啟動時的接續原則：<c>pending</c> 內的作業一律重新排入（尚未開始處理，重跑安全）；
    /// <c>processing</c> 內的作業預設移到 <c>interrupted</c> 等人工確認，因為存證可能已寫入部分發票，
    /// 自動重跑會造成重複開立。確認可安全重跑者，可將設定 <c>ResumeInterruptedJobs</c> 設為 true
    /// 改為自動重新排入，或人工把檔案搬回 <c>pending</c> 後重啟服務。
    /// </para>
    /// <para>
    /// 記憶體中僅保留待處理的「檔名」（不含內容），佇列深度不影響記憶體用量；檔案內容於取件時才反序列化。
    /// 單一行程使用，未做跨行程檔案鎖（多台/多站台請各自使用不同的 StoragePath）。
    /// </para>
    /// </summary>
    public class InvoiceProcessFileQueue : IInvoiceProcessQueue, IDisposable
    {
        private const String PendingFolderName = "pending";
        private const String ProcessingFolderName = "processing";
        private const String FailedFolderName = "failed";
        private const String InterruptedFolderName = "interrupted";
        private const String ArchiveFolderName = "archive";
        private const String JobFileExtension = ".json";
        private const String TempFileExtension = ".tmp";

        private readonly ILogger<InvoiceProcessFileQueue> _logger;

        /// <summary>待處理檔名的訊號佇列（無界；容量由 <see cref="_capacity"/> 於寫入時自行把關）。</summary>
        private readonly Channel<String> _signal = Channel.CreateUnbounded<String>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = false,
        });

        private readonly String _pendingPath;
        private readonly String _processingPath;
        private readonly String _failedPath;
        private readonly String _interruptedPath;
        private readonly String _archivePath;
        private readonly int _capacity;
        private readonly bool _archiveCompleted;

        /// <summary>檔名序號，避免同一毫秒內的多筆作業檔名相同。</summary>
        private long _sequence;

        /// <summary>
        /// 收單請求即以 Newtonsoft.Json 解析（見 <c>LegacyJsonInputFormatter</c>），佇列檔案沿用同一套
        /// 序列化行為，<c>InvoiceRoot</c>（xsd 產生的 public field）才能正確來回轉換。
        /// </summary>
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
            Formatting = Formatting.Indented,
        };

        public InvoiceProcessFileQueue(ILogger<InvoiceProcessFileQueue> logger)
        {
            _logger = logger;

            InvoiceProcessQueueSettings settings = AppSettings.Default.InvoiceProcessQueue;
            _capacity = Math.Max(1, settings.Capacity);
            _archiveCompleted = settings.ArchiveCompletedJobs;

            String root = settings.StoragePath.GetEfficientString()
                ?? Path.Combine(CommonLib.Core.Utility.Logger.LogPath, "InvoiceProcessQueue");

            _pendingPath = Path.Combine(root, PendingFolderName);
            _processingPath = Path.Combine(root, ProcessingFolderName);
            _failedPath = Path.Combine(root, FailedFolderName);
            _interruptedPath = Path.Combine(root, InterruptedFolderName);
            _archivePath = Path.Combine(root, ArchiveFolderName);

            foreach (String path in new[] { _pendingPath, _processingPath, _failedPath, _interruptedPath })
            {
                Directory.CreateDirectory(path);
            }

            if (_archiveCompleted)
            {
                Directory.CreateDirectory(_archivePath);
            }

            _logger.LogInformation("發票處理佇列使用檔案儲存，路徑 = {Path}，容量 = {Capacity}。", root, _capacity);

            RecoverInterruptedJobs(settings.ResumeInterruptedJobs);
            RecoverPendingJobs();
        }

        /// <summary>待處理的作業數（不含已取件、正在執行中的）。</summary>
        public int Count => _signal.Reader.Count;

        public bool TryEnqueue(InvoiceProcessJob job)
        {
            ArgumentNullException.ThrowIfNull(job);

            if (Count >= _capacity)
            {
                _logger.LogWarning("發票處理佇列已滿（{Count}/{Capacity}），拒絕收件。", Count, _capacity);
                return false;
            }

            String jobId = CreateJobId(job);
            String jobPath = Path.Combine(_pendingPath, jobId);
            String tempPath = jobPath + TempFileExtension;

            try
            {
                // 先寫暫存檔再改名：避免 Worker 讀到只寫了一半的 JSON。
                File.WriteAllText(tempPath, JsonConvert.SerializeObject(job, _jsonSettings));
                File.Move(tempPath, jobPath, overwrite: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發票處理佇列寫入檔案失敗，AgentID = {AgentID}，檔案 = {JobId}。", job.AgentID, jobId);
                TryDelete(tempPath);
                TryDelete(jobPath);
                return false;
            }

            if (!_signal.Writer.TryWrite(jobId))
            {
                // 佇列已關閉寫入端（服務停止中），移除剛落地的檔案，避免下次啟動被當成待處理作業。
                TryDelete(jobPath);
                return false;
            }

            job.JobId = jobId;
            return true;
        }

        public async IAsyncEnumerable<InvoiceProcessJob> ReadAllAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (String jobId in _signal.Reader.ReadAllAsync(cancellationToken))
            {
                InvoiceProcessJob? job = CheckOut(jobId);
                if (job != null)
                {
                    yield return job;
                }
            }
        }

        public void Complete()
        {
            _signal.Writer.TryComplete();
        }

        /// <summary>處理完成：刪除 processing 內的作業檔案（設定歸檔時改為搬到 archive）。</summary>
        public void Acknowledge(InvoiceProcessJob job)
        {
            ArgumentNullException.ThrowIfNull(job);

            String? jobId = job.JobId.GetEfficientString();
            if (jobId == null)
            {
                return;
            }

            String processingPath = Path.Combine(_processingPath, jobId);
            try
            {
                if (_archiveCompleted)
                {
                    File.Move(processingPath, Path.Combine(_archivePath, jobId), overwrite: true);
                }
                else
                {
                    //File.Delete(processingPath);
                    File.Move(processingPath, Path.Combine(CommonLib.Core.Utility.Logger.LogDailyPath, jobId), overwrite: true);

                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "發票處理佇列清除已完成作業檔案失敗，檔案 = {JobId}。", jobId);
            }
        }

        /// <summary>處理失敗：作業檔案移到 failed，並記錄例外內容供事後檢視或人工重送。</summary>
        public void Abandon(InvoiceProcessJob job, Exception? error)
        {
            ArgumentNullException.ThrowIfNull(job);

            String? jobId = job.JobId.GetEfficientString();
            if (jobId == null)
            {
                return;
            }

            String? failedPath = MoveTo(Path.Combine(_processingPath, jobId), _failedPath, jobId);
            if (failedPath != null && error != null)
            {
                try
                {
                    File.WriteAllText(failedPath + ".error.txt",
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}{Environment.NewLine}{error}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "發票處理佇列寫入失敗記錄失敗，檔案 = {JobId}。", jobId);
                }
            }
        }

        public void Dispose()
        {
            Complete();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 取件：把 pending 的檔案移到 processing 後才反序列化，讓「已取件」的狀態同樣落地；
        /// 中斷重啟時才能分辨作業是尚未開始（pending）或執行中被中斷（processing）。
        /// </summary>
        private InvoiceProcessJob? CheckOut(String jobId)
        {
            String pendingPath = Path.Combine(_pendingPath, jobId);
            String processingPath = Path.Combine(_processingPath, jobId);

            try
            {
                File.Move(pendingPath, processingPath, overwrite: true);
            }
            catch (FileNotFoundException)
            {
                // 檔案已被人工移走，或先前已取件，略過。
                _logger.LogWarning("發票處理佇列取件時找不到作業檔案，檔案 = {JobId}。", jobId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發票處理佇列取件失敗，檔案 = {JobId}。", jobId);
                return null;
            }

            try
            {
                InvoiceProcessJob? job = JsonConvert.DeserializeObject<InvoiceProcessJob>(
                    File.ReadAllText(processingPath), _jsonSettings);

                if (job == null)
                {
                    _logger.LogError("發票處理佇列作業檔案內容為空，檔案 = {JobId}，移至 failed。", jobId);
                    MoveTo(processingPath, _failedPath, jobId);
                    return null;
                }

                job.JobId = jobId;
                return job;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發票處理佇列作業檔案無法解析，檔案 = {JobId}，移至 failed。", jobId);
                MoveTo(processingPath, _failedPath, jobId);
                return null;
            }
        }

        /// <summary>啟動時把 pending 內既有的作業（含前次中斷未處理的）依檔名時間序重新排入。</summary>
        private void RecoverPendingJobs()
        {
            String[] files;
            try
            {
                files = Directory.GetFiles(_pendingPath, "*" + JobFileExtension);

                // 前次中斷留下的 .tmp（寫入未完成）沒有完整內容，直接清掉。
                foreach (String temp in Directory.GetFiles(_pendingPath, "*" + TempFileExtension))
                {
                    TryDelete(temp);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發票處理佇列讀取待處理作業目錄失敗，路徑 = {Path}。", _pendingPath);
                return;
            }

            // 檔名以「時間 + 序號」開頭，字串排序即為進入佇列的先後順序。
            foreach (String? name in files.Select(Path.GetFileName).OrderBy(name => name, StringComparer.Ordinal))
            {
                if (name != null)
                {
                    _signal.Writer.TryWrite(name);
                }
            }

            if (files.Length > 0)
            {
                _logger.LogInformation("發票處理佇列接續前次未處理的作業 {Count} 筆。", files.Length);
            }
        }

        /// <summary>
        /// 處理前次中斷時仍在 processing 的作業。預設移到 interrupted 等人工確認
        /// （可能已存證部分發票，自動重跑會重複開立）；<paramref name="resume"/> 為 true 時重新排入 pending。
        /// </summary>
        private void RecoverInterruptedJobs(bool resume)
        {
            String[] files;
            try
            {
                files = Directory.GetFiles(_processingPath, "*" + JobFileExtension);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發票處理佇列讀取處理中作業目錄失敗，路徑 = {Path}。", _processingPath);
                return;
            }

            if (files.Length == 0)
            {
                return;
            }

            foreach (String? name in files.Select(Path.GetFileName).OrderBy(name => name, StringComparer.Ordinal))
            {
                if (name != null)
                {
                    MoveTo(Path.Combine(_processingPath, name), resume ? _pendingPath : _interruptedPath, name);
                }
            }

            if (resume)
            {
                _logger.LogWarning(
                    "發票處理佇列將前次中斷處理中的作業 {Count} 筆重新排入（ResumeInterruptedJobs = true，請留意重複存證風險）。",
                    files.Length);
            }
            else
            {
                _logger.LogWarning(
                    "發票處理佇列有 {Count} 筆作業於前次中斷時正在處理，已移至 {Path} 等待人工確認（可能已存證部分發票）。",
                    files.Length, _interruptedPath);
            }
        }

        /// <summary>作業檔名：時間 + 序號（維持先進先出）+ 營業人 + 作業種類 + 隨機碼（避免同名覆蓋）。</summary>
        private String CreateJobId(InvoiceProcessJob job)
        {
            long sequence = Interlocked.Increment(ref _sequence);
            String random = Guid.NewGuid().ToString("N").Substring(0, 8);
            return $"{DateTime.Now:yyyyMMddHHmmssfff}_{sequence:D6}_{job.AgentID}_{job.Kind}_{random}{JobFileExtension}";
        }

        private String? MoveTo(String sourcePath, String targetFolder, String fileName)
        {
            try
            {
                Directory.CreateDirectory(targetFolder);
                String targetPath = Path.Combine(targetFolder, fileName);
                File.Move(sourcePath, targetPath, overwrite: true);
                return targetPath;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "發票處理佇列搬移作業檔案失敗，來源 = {Source}，目的 = {Target}。", sourcePath, targetFolder);
                return null;
            }
        }

        private void TryDelete(String path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "發票處理佇列刪除檔案失敗，檔案 = {Path}。", path);
            }
        }
    }
}
