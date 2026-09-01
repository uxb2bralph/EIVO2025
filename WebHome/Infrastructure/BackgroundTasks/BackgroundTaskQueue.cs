using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace WebHome.Infrastructure.BackgroundTasks
{
    /// <summary>
    /// <see cref="IBackgroundTaskQueue"/> 的記憶體實作，以有界 <see cref="Channel"/> 承載。
    /// 以 Singleton 註冊。
    /// </summary>
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        public const int DefaultCapacity = 1024;

        private readonly Channel<BackgroundWorkItem> _queue;
        private int _count;

        public BackgroundTaskQueue()
            : this(DefaultCapacity)
        {
        }

        public BackgroundTaskQueue(int capacity)
        {
            // FullMode = DropWrite：佇列滿時 TryWrite 直接回 false，不阻塞請求執行緒。
            _queue = Channel.CreateBounded<BackgroundWorkItem>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.DropWrite,
                SingleReader = true,
                SingleWriter = false,
            });
        }

        public int Count => Volatile.Read(ref _count);

        public bool TryEnqueue(string name, Func<CancellationToken, ValueTask> workItem)
        {
            ArgumentNullException.ThrowIfNull(workItem);

            if (!_queue.Writer.TryWrite(new BackgroundWorkItem(name ?? "(unnamed)", workItem)))
            {
                return false;
            }

            Interlocked.Increment(ref _count);
            return true;
        }

        public async ValueTask<BackgroundWorkItem> DequeueAsync(CancellationToken cancellationToken)
        {
            BackgroundWorkItem item = await _queue.Reader.ReadAsync(cancellationToken);
            Interlocked.Decrement(ref _count);
            return item;
        }

        /// <summary>
        /// 關閉寫入端，讓 <see cref="BackgroundTaskService"/> 能把剩餘工作排空後結束。
        /// </summary>
        public void Complete()
        {
            _queue.Writer.TryComplete();
        }
    }
}
