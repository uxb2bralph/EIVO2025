using System;
using System.Collections.Generic;
using System.Threading;
using ModelCore.Locale;
using ModelCore.Schema.EIVO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TaskCenter.Core.Interfaces
{
    /// <summary>
    /// 背景作業種類，對應 <c>InvoiceManagerExtensions</c> 內各存證方法。
    /// 以名稱（字串）序列化到佇列檔案，日後增修成員不會影響既有檔案的判讀。
    /// </summary>
    public enum InvoiceProcessJobKind
    {
        /// <summary>未指定（作業檔案內容不完整或格式過舊），一律視為失敗不予處理。</summary>
        Unknown = 0,

        /// <summary>發票存證（帶號）：<c>UploadInvoice</c>。</summary>
        UploadInvoice = 1,

        /// <summary>發票存證（自動配號）：<c>UploadInvoiceAutoTrackNo</c>。</summary>
        UploadInvoiceAutoTrackNo = 2,

        /// <summary>發票作廢：<c>UploadInvoiceCancellation</c>。</summary>
        UploadInvoiceCancellation = 3,

        /// <summary>折讓單存證：<c>UploadAllowance</c>。</summary>
        UploadAllowance = 4,

        /// <summary>折讓單作廢：<c>UploadAllowanceCancellation</c>。</summary>
        UploadAllowanceCancellation = 5,
    }

    /// <summary>
    /// 發票處理背景作業項目（對應 InvoiceService 各 Apply* 端點收下的一次請求）。
    /// 只攜帶「值」型別的資料：<see cref="InvoiceRoot"/> 等 Root 物件由請求內容反序列化而來，
    /// 與資料庫無關；營業人憑證（OrganizationToken）則只帶 <see cref="AgentID"/>，由背景服務以
    /// 自己的 DbContext 重新載入。EF Core 已開啟 Lazy Loading Proxy，若直接把請求 DbContext 追蹤的
    /// OrganizationToken 傳到背景執行，請求結束後該 DbContext 已釋放，存取 Company 等
    /// 導覽屬性會拋出 ObjectDisposedException。
    /// <para>
    /// 依 <see cref="Kind"/> 只會有一個 Root 屬性有值，其餘為 null（不序列化）。
    /// </para>
    /// </summary>
    public class InvoiceProcessJob
    {
        /// <summary>作業種類，決定背景服務呼叫哪一個存證方法、讀取哪一個 Root 屬性。</summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public InvoiceProcessJobKind Kind { get; init; }

        /// <summary>營業人（Organization.CompanyID），背景服務據此重新載入 OrganizationToken。</summary>
        public int AgentID { get; init; }

        /// <summary>待存證的發票資料（<see cref="InvoiceProcessJobKind.UploadInvoice"/> / <see cref="InvoiceProcessJobKind.UploadInvoiceAutoTrackNo"/>）。</summary>
        public InvoiceRoot? Invoice { get; init; }

        /// <summary>待作廢的發票資料（<see cref="InvoiceProcessJobKind.UploadInvoiceCancellation"/>）。</summary>
        public CancelInvoiceRoot? CancelInvoice { get; init; }

        /// <summary>待存證的折讓單資料（<see cref="InvoiceProcessJobKind.UploadAllowance"/>）。</summary>
        public AllowanceRoot? Allowance { get; init; }

        /// <summary>待作廢的折讓單資料（<see cref="InvoiceProcessJobKind.UploadAllowanceCancellation"/>）。</summary>
        public CancelAllowanceRoot? CancelAllowance { get; init; }

        /// <summary>
        /// 來源系統識別碼（InvoiceRequestViewModel.ClientID），僅發票存證作業使用；
        /// 作廢／折讓沿用原收單端點行為，不帶此值。
        /// </summary>
        public String? ClientID { get; init; }

        /// <summary>
        /// 處理類型，僅發票存證作業使用；F0401_Json_CBE 時改用 InvoiceManagerForCBE。
        /// 作廢／折讓沿用原收單端點行為，不帶此值。
        /// </summary>
        public Naming.InvoiceProcessType? ProcessType { get; init; }

        /// <summary>指定開立日期，僅自動配號（<see cref="InvoiceProcessJobKind.UploadInvoiceAutoTrackNo"/>）時有效。</summary>
        public DateTime? ApplyInvoiceDate { get; init; }

        /// <summary>進入佇列的時間，用於記錄等待時間。</summary>
        public DateTime SubmitDate { get; init; } = DateTime.Now;

        /// <summary>
        /// 佇列給予的作業識別碼；檔案佇列（<c>InvoiceProcessFileQueue</c>）以此對應落地的 JSON 檔名，
        /// 處理完成（<see cref="IInvoiceProcessQueue.Acknowledge"/>）或失敗
        /// （<see cref="IInvoiceProcessQueue.Abandon"/>）時據此清除或搬移檔案。
        /// 由佇列自行填入，不隨作業內容序列化。
        /// </summary>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public String? JobId { get; set; }
    }

    /// <summary>
    /// 發票處理作業佇列。由 InvoiceService 的 Apply* 端點寫入，<c>InvoiceProcessBackgroundService</c>
    /// 讀取執行。兩種實作：
    /// <list type="bullet">
    /// <item><c>InvoiceProcessQueue</c>：記憶體內佇列，行程結束未處理完的作業會遺失。</item>
    /// <item><c>InvoiceProcessFileQueue</c>：作業以 JSON 檔案落地，行程中斷重啟後可接續處理。</item>
    /// </list>
    /// 取件端須在每筆作業處理結束後呼叫 <see cref="Acknowledge"/> 或 <see cref="Abandon"/>，
    /// 否則檔案佇列會把該作業視為「處理中」，重啟時列為中斷作業。
    /// </summary>
    public interface IInvoiceProcessQueue
    {
        /// <summary>目前待處理的作業數（不含正在執行中的）。</summary>
        int Count { get; }

        /// <summary>
        /// 寫入作業。佇列已滿時回傳 false（呼叫端應回報忙碌，不阻塞收單請求）。
        /// </summary>
        bool TryEnqueue(InvoiceProcessJob job);

        /// <summary>依序讀取作業，直到佇列結束（<see cref="Complete"/>）且已排空。</summary>
        IAsyncEnumerable<InvoiceProcessJob> ReadAllAsync(CancellationToken cancellationToken);

        /// <summary>關閉寫入端，不再接受新作業（服務停止時呼叫，讓剩餘作業排空）。</summary>
        void Complete();

        /// <summary>
        /// 標記作業處理完畢（含業務層面失敗但已完成處理者），佇列可釋放其保存的內容。
        /// 記憶體佇列不需保存內容，實作為空。
        /// </summary>
        void Acknowledge(InvoiceProcessJob job);

        /// <summary>
        /// 標記作業處理過程異常中止（例外、找不到憑證等），佇列保留內容供事後檢視或人工重送。
        /// 記憶體佇列不需保存內容，實作為空。
        /// </summary>
        void Abandon(InvoiceProcessJob job, Exception? error = null);
    }
}
