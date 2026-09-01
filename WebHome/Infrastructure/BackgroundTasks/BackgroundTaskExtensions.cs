using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace WebHome.Infrastructure.BackgroundTasks
{
    public static class BackgroundTaskExtensions
    {
        /// <summary>
        /// 註冊背景工作佇列與取件執行的 <see cref="BackgroundTaskService"/>。
        /// </summary>
        public static IServiceCollection AddBackgroundTaskQueue(this IServiceCollection services)
        {
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddHostedService<BackgroundTaskService>();
            return services;
        }

        /// <summary>
        /// 從 View / Controller 排入背景工作，取代 <c>Task.Run</c>。
        /// </summary>
        /// <remarks>
        /// <paramref name="workItem"/> 是在請求結束之後才執行，內部不可使用 <c>HttpContext</c>、
        /// 請求的 <c>ModelSource</c>／被它追蹤的實體；請自行 new 一個 <c>ModelSource</c> 重新查詢。
        /// 詳見 <see cref="IBackgroundTaskQueue"/>。
        /// </remarks>
        /// <returns>成功排入回傳 <c>true</c>；佇列已滿回傳 <c>false</c>。</returns>
        public static bool EnqueueBackgroundWork(this HttpContext context, string name, Func<CancellationToken, ValueTask> workItem)
        {
            IBackgroundTaskQueue? queue = context?.RequestServices?.GetService<IBackgroundTaskQueue>()
                ?? Startup.ServiceProvider?.GetService<IBackgroundTaskQueue>();

            if (queue == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(IBackgroundTaskQueue)} 尚未註冊，請在 Startup.ConfigureServices 呼叫 AddBackgroundTaskQueue()。");
            }

            return queue.TryEnqueue(name, workItem);
        }

        /// <summary>
        /// 同步版本的 <see cref="EnqueueBackgroundWork(HttpContext, string, Func{CancellationToken, ValueTask})"/>，
        /// 方便直接搬移原本 <c>Task.Run(() =&gt; { ... })</c> 的內容。
        /// </summary>
        public static bool EnqueueBackgroundWork(this HttpContext context, string name, Action<CancellationToken> workItem)
        {
            ArgumentNullException.ThrowIfNull(workItem);

            return context.EnqueueBackgroundWork(name, token =>
            {
                workItem(token);
                return ValueTask.CompletedTask;
            });
        }
    }
}
