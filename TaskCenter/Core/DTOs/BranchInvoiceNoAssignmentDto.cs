using System.Text.Json.Serialization;

namespace TaskCenter.Core.DTOs
{
    /// <summary>
    /// 主機構批次配號設定的單一分支機構配號
    /// （對應 OrganizationCustomSetting.Settings.BranchInvoiceNoAssignments 之 BranchInvoiceNoAssignmentModel）。
    /// 全域序列化為 PascalCase，故逐欄位以 [JsonPropertyName] 指定 camelCase。
    /// </summary>
    public class BranchInvoiceNoAssignmentDto
    {
        /// <summary>分支機構統一編號（對應 BranchInvoiceNoAssignmentModel.ReceiptNo）</summary>
        [JsonPropertyName("receiptNo")]
        public string? ReceiptNo { get; set; }

        /// <summary>分支機構名稱（顯示用；由統編對應 Organization 帶出，不存於設定 JSON）</summary>
        [JsonPropertyName("companyName")]
        public string? CompanyName { get; set; }

        /// <summary>配號本組數（每組 50 號；對應 BranchInvoiceNoAssignmentModel.Booklets）</summary>
        [JsonPropertyName("booklets")]
        public int? Booklets { get; set; }

        /// <summary>配號後預設鎖定字軌號碼區間（對應 BranchInvoiceNoAssignmentModel.InitialLock）</summary>
        [JsonPropertyName("initialLock")]
        public bool InitialLock { get; set; }
    }
}
