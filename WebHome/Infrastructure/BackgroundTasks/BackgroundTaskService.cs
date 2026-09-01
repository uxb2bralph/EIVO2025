using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebHome.Infrastructure.BackgroundTasks
{
    /// <summary>
    /// 取件執行 <see cref="IBackgroundTaskQueue"/> 中的工作。
    /// 一次執行一件，避免同時開太多 DbContext／連線；工作內的例外一律攔下記錄，
    /// 不讓單一作業失敗把整個 host 拖掉。
    /// </summary>
    public class BackgroundTaskService : BackgroundService
    {
        private readonly IBackgroundTaskQueue _queue;
        private readonly ILogger<BackgroundTaskService> _logger;

        public BackgroundTaskService(IBackgroundTaskQueue queue, ILogger<BackgroundTaskService> logger)
        {
            _queue = queue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BackgroundTaskService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                BackgroundWorkItem item;
                try
                {
                    item = await _queue.DequeueAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ChannelClosedException)
                {
                    break;
                }

                try
                {
                    await item.WorkItem(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Background work item '{Name}' was cancelled by host shutdown.", item.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Background work item '{Name}' failed.", item.Name);
                    CommonLib.Core.Utility.Logger.Error(ex);
                }
            }

            _logger.LogInformation("BackgroundTaskService stopping, {Count} work item(s) still queued.", _queue.Count);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            (_queue as BackgroundTaskQueue)?.Complete();
            await base.StopAsync(cancellationToken);
        }
    }
}
