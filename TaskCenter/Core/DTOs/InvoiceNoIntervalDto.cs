using System.Text.Json.Serialization;

namespace TaskCenter.Core.DTOs
{
    /// <summary>
    /// 電子發票配號區間查詢條件（遷移自舊版 InvoiceNoController.InquireInterval / InquireNoIntervalViewModel）。
    /// 以 [FromQuery] 繫結；SellerKey（加密後之 CompanyID）與 Year 為必要條件（沿用舊版一律指定開立人 + 年度）。
    /// </summary>
    public class InvoiceNoIntervalQueryDto
    {
        /// <summary>開立人（加密後的 Organization.CompanyID；對應舊版 EncSellerID）。</summary>
        public string? SellerKey { get; set; }

        /// <summary>發票年度（西元年；前端以民國年顯示 = Year - 1911）。</summary>
        public int? Year { get; set; }

        /// <summary>發票期別（1~6 對應雙月；null 表示全部）。</summary>
        public int? PeriodNo { get; set; }

        /// <summary>是否含分支機構之發票號碼區間（對應舊版 BranchRelation）。</summary>
        public bool? BranchRelation { get; set; }

        /// <summary>頁碼（1-based）。</summary>
        public int Page { get; set; } = 1;

        /// <summary>每頁筆數。</summary>
        public int PageSize { get; set; } = 10;

        /// <summary>略過筆數。</summary>
        [JsonIgnore]
        public int Skip => (Page - 1) * PageSize;
    }

    /// <summary>
    /// 配號區間列表項目（對應舊版 InvoiceNo/Module/DataItem.cshtml 之列欄位）。
    /// 全域序列化為 PascalCase，故逐欄位以 [JsonPropertyName] 指定 camelCase。
    /// </summary>
    public class InvoiceNoIntervalDatatableDto
    {
        /// <summary>配號區間識別碼（InvoiceNoInterval.IntervalID），供修改 / 刪除 / 鎖定傳遞。</summary>
        [JsonPropertyName("intervalId")]
        public int IntervalId { get; set; }

        /// <summary>開立人統一編號（Organization.ReceiptNo）。</summary>
        [JsonPropertyName("receiptNo")]
        public string? ReceiptNo { get; set; }

        /// <summary>發票年度（西元年）。</summary>
        [JsonPropertyName("year")]
        public int Year { get; set; }

        /// <summary>發票期別（1~6）。</summary>
        [JsonPropertyName("periodNo")]
        public int PeriodNo { get; set; }

        /// <summary>字軌（二位英文字母）。</summary>
        [JsonPropertyName("trackCode")]
        public string? TrackCode { get; set; }

        /// <summary>發票類別（7 一般 / 8 特種）。</summary>
        [JsonPropertyName("invoiceType")]
        public int? InvoiceType { get; set; }

        /// <summary>發票號碼起（8 位）。</summary>
        [JsonPropertyName("startNo")]
        public int StartNo { get; set; }

        /// <summary>發票號碼迄（8 位）。</summary>
        [JsonPropertyName("endNo")]
        public int EndNo { get; set; }

        /// <summary>指定 POS 機號碼（InvoiceNoSegment.DeviceName）。</summary>
        [JsonPropertyName("deviceName")]
        public string? DeviceName { get; set; }

        /// <summary>配號總數（EndNo - StartNo + 1）。</summary>
        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        /// <summary>配號本數（TotalCount / 50）。</summary>
        [JsonPropertyName("bookletCount")]
        public int BookletCount { get; set; }

        /// <summary>目前給號（CurrentAllocatingNo）。</summary>
        [JsonPropertyName("currentNo")]
        public int CurrentNo { get; set; }

        /// <summary>剩餘可配號（EndNo - CurrentNo + 1）。</summary>
        [JsonPropertyName("remaining")]
        public int Remaining { get; set; }

        /// <summary>是否已鎖定（LockID 有值）。</summary>
        [JsonPropertyName("locked")]
        public bool Locked { get; set; }

        /// <summary>是否可編輯 / 刪除（尚無任何號碼被配發或指派）。</summary>
        [JsonPropertyName("editable")]
        public bool Editable { get; set; }

        /// <summary>開立人是否為主機構（有下轄分支機構）；用於顯示主機構配號 / 指派分支機構功能。</summary>
        [JsonPropertyName("isMaster")]
        public bool IsMaster { get; set; }

        /// <summary>此區間是否已設定主機構配號（涵蓋本區間之 InvoiceNoMainAssignment 已存在）。</summary>
        [JsonPropertyName("hasMainAssignment")]
        public bool HasMainAssignment { get; set; }
    }

    /// <summary>
    /// 新增 / 修改配號區間（遷移自舊版 InvoiceNoController.CommitItem / InvoiceNoIntervalViewModel）。
    /// IntervalId 為 null 時新增（需 TrackId + SellerKey），否則修改（僅變更起迄與 POS 機號）。
    /// </summary>
    public class InvoiceNoIntervalEditDto
    {
        /// <summary>配號區間識別碼；新增時為 null。</summary>
        public int? IntervalId { get; set; }

        /// <summary>字軌識別碼（InvoiceTrackCode.TrackID）；新增時必填。</summary>
        public int? TrackId { get; set; }

        /// <summary>開立人（加密後的 CompanyID）；新增時必填。</summary>
        public string? SellerKey { get; set; }

        /// <summary>發票號碼起（8 位整數）。</summary>
        public int? StartNo { get; set; }

        /// <summary>發票號碼迄（8 位整數；迄 &gt; 起，且差距為 50 之倍數）。</summary>
        public int? EndNo { get; set; }

        /// <summary>指定 POS 機號碼；空白表示清除。</summary>
        public string? DeviceName { get; set; }
    }

    /// <summary>鎖定 / 解除鎖定配號區間（遷移自舊版 InvoiceNoController.LockInterval）。</summary>
    public class InvoiceNoIntervalLockDto
    {
        /// <summary>配號區間識別碼。</summary>
        public int IntervalId { get; set; }

        /// <summary>true 鎖定、false 解除鎖定。</summary>
        public bool Locked { get; set; }
    }

    /// <summary>本組數均分（遷移自舊版 InvoiceNoController.CommitAllotment）。將區間均分為每份 Parts*50 個號碼。</summary>
    public class InvoiceNoIntervalAllotDto
    {
        /// <summary>配號區間識別碼。</summary>
        public int IntervalId { get; set; }

        /// <summary>每份本數（每本 50 個號碼；須 &gt; 0）。</summary>
        public int Parts { get; set; }
    }

    /// <summary>指派分支機構（遷移自舊版 InvoiceNoController.CommitBranch）。將區間改配給指定分支機構開立人。</summary>
    public class InvoiceNoIntervalBranchDto
    {
        /// <summary>配號區間識別碼。</summary>
        public int IntervalId { get; set; }

        /// <summary>分支機構開立人（加密後的 CompanyID）。</summary>
        public string? SellerKey { get; set; }
    }

    /// <summary>設定可用配號存量警戒值（遷移自舊版 Organization.CommitInvoiceNoSafetyStock）。</summary>
    public class InvoiceNoSafetyStockDto
    {
        /// <summary>開立人（加密後的 CompanyID）。</summary>
        public string? SellerKey { get; set; }

        /// <summary>警戒值（可用配號存量低於此值時通知）；null 表示清除。</summary>
        public int? SafetyStock { get; set; }
    }

    /// <summary>可用配號存量警戒值查詢結果。</summary>
    public class InvoiceNoSafetyStockResultDto
    {
        /// <summary>目前警戒值（OrganizationExtension.InvoiceNoSafetyStock）。</summary>
        [JsonPropertyName("safetyStock")]
        public int? SafetyStock { get; set; }
    }

    /// <summary>開立人候選項目（供前端選擇開立人 autocomplete）。</summary>
    public class InvoiceNoSellerOptionDto
    {
        /// <summary>加密後的 CompanyID（供查詢 / 新增傳遞）。</summary>
        [JsonPropertyName("sellerKey")]
        public string? SellerKey { get; set; }

        /// <summary>統一編號。</summary>
        [JsonPropertyName("receiptNo")]
        public string? ReceiptNo { get; set; }

        /// <summary>營業人名稱。</summary>
        [JsonPropertyName("companyName")]
        public string? CompanyName { get; set; }
    }

    /// <summary>字軌候選項目（新增配號區間時選擇字軌；對應舊版 TrackCodeSelector）。</summary>
    public class InvoiceTrackCodeOptionDto
    {
        /// <summary>字軌識別碼（InvoiceTrackCode.TrackID）。</summary>
        [JsonPropertyName("trackId")]
        public int TrackId { get; set; }

        /// <summary>字軌（二位英文字母）。</summary>
        [JsonPropertyName("trackCode")]
        public string? TrackCode { get; set; }

        /// <summary>發票類別（7 一般 / 8 特種）。</summary>
        [JsonPropertyName("invoiceType")]
        public int? InvoiceType { get; set; }
    }
}
