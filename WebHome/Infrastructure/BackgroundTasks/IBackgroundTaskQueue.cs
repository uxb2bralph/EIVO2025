using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebHome.Infrastructure.BackgroundTasks
{
    /// <summary>
    /// 已排入佇列、等待背景執行的工作。
    /// </summary>
    /// <param name="Name">作業名稱，只用於記錄，方便從 log 追出是哪一支頁面送出的。</param>
    /// <param name="WorkItem">實際要執行的工作。</param>
    public record BackgroundWorkItem(string Name, Func<CancellationToken, ValueTask> WorkItem);

    /// <summary>
    /// 背景工作佇列，用來取代散落在 View / Controller 中的 <c>Task.Run</c> 與
    /// <c>ThreadPool.QueueUserWorkItem</c>：工作改由 <see cref="BackgroundTaskService"/>
    /// （IHostedService）統一取件執行，主機關閉時可被取消並等待收尾，例外也會集中記錄。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 排入的工作是在請求結束之後才執行，所以 <b>不可以</b> 捕捉任何屬於請求生命週期的資源：
    /// <c>HttpContext</c>、請求的 <c>ModelSource</c> / <c>DbContext</c>、或是被該 context 追蹤的實體。
    /// </para>
    /// <para>
    /// <c>ApplicationDbContext</c> 開啟了 lazy loading proxy，context 釋放後再存取實體的導覽屬性
    /// 會擲出 <c>ObjectDisposedException</c>；即使 context 還活著，在請求執行緒與背景執行緒同時使用
    /// 同一個連線也會出現「已經開啟與這個 Connection 建立關聯的 DataReader」。
    /// 正確做法是只把純資料（主鍵、字串、ViewModel 等值型別）傳進工作內，
    /// 工作自己 <c>new</c> 一個 <c>ModelSource</c> 重新查詢。
    /// </para>
    /// </remarks>
    public interface IBackgroundTaskQueue
    {
        /// <summary>
        /// 將工作排入佇列。佇列已滿時回傳 <c>false</c>（不會阻塞請求執行緒）。
        /// </summary>
        bool TryEnqueue(string name, Func<CancellationToken, ValueTask> workItem);

        /// <summary>
        /// 取出下一件工作；佇列為空時等待，直到有工作或 <paramref name="cancellationToken"/> 被取消。
        /// </summary>
        ValueTask<BackgroundWorkItem> DequeueAsync(CancellationToken cancellationToken);

        /// <summary>
        /// 目前排隊中（尚未取出）的工作數。
        /// </summary>
        int Count { get; }
    }
}
