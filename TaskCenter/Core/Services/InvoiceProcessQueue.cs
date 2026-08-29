using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using TaskCenter.Core.Interfaces;
using TaskCenter.Properties;

namespace TaskCenter.Core.Services
{
    /// <summary>
    /// <see cref="IInvoiceProcessQueue"/> 的記憶體實作，以 <see cref="Channel"/> 建立
    /// 有界佇列（容量取自 AppSettings.InvoiceProcessQueue.Capacity）。
    /// 以 Singleton 註冊：收單端點寫入、<see cref="InvoiceProcessBackgroundService"/> 讀取。
    /// 寫入採不阻塞（TryWrite），佇列已滿時直接回報失敗，避免拖慢對外收單 API。
    /// </summary>
    public class InvoiceProcessQueue : IInvoiceProcessQueue
    {
        private readonly Channel<InvoiceProcessJob> _channel;

        public InvoiceProcessQueue()
        {
            int capacity = Math.Max(1, AppSettings.Default.InvoiceProcessQueue.Capacity);
            _channel = Channel.CreateBounded<InvoiceProcessJob>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false,
            });
        }

        public int Count => _channel.Reader.Count;

        public bool TryEnqueue(InvoiceProcessJob job)
        {
            ArgumentNullException.ThrowIfNull(job);
            return _channel.Writer.TryWrite(job);
        }

        public IAsyncEnumerable<InvoiceProcessJob> ReadAllAsync(CancellationToken cancellationToken)
        {
            return _channel.Reader.ReadAllAsync(cancellationToken);
        }

        public void Complete()
        {
            _channel.Writer.TryComplete();
        }

        /// <summary>記憶體佇列取件後即不再保存作業內容，無須處理。</summary>
        public void Acknowledge(InvoiceProcessJob job)
        {
        }

        /// <summary>記憶體佇列取件後即不再保存作業內容，失敗的作業無法重送（由呼叫端記錄）。</summary>
        public void Abandon(InvoiceProcessJob job, Exception? error = null)
        {
        }
    }
}
