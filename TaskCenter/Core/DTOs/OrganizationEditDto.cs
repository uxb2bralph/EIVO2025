using System;
using System.Text.Json.Serialization;

namespace TaskCenter.Core.DTOs
{
    /// <summary>
    /// 營業人編輯資料（遷移自 WebHome Organization/EditItem + ItemForm 表單欄位）。
    /// 對應舊版 OrganizationViewModel，以 ApplyFromModel 載入、CommitOrganizationViewModel 儲存。
    /// 全域序列化為 PascalCase，故逐欄位以 [JsonPropertyName] 指定 camelCase；
    /// 同一組名稱亦供 [FromBody] 反序列化（前端送 camelCase）。
    /// </summary>
    public class OrganizationEditDto
    {
        /// <summary>加密後的 CompanyID（沿用舊版 KeyID 做法）；儲存時後端會解密還原 CompanyID。</summary>
        [JsonPropertyName("keyId")]
        public string? KeyId { get; set; }

        [JsonPropertyName("companyId")]
        public int? CompanyId { get; set; }

        // ── 基本資料 ──────────────────────────────────────────────
        [JsonPropertyName("receiptNo")]
        public string? ReceiptNo { get; set; }

        [JsonPropertyName("companyName")]
        public string? CompanyName { get; set; }

        [JsonPropertyName("addr")]
        public string? Addr { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("fax")]
        public string? Fax { get; set; }

        [JsonPropertyName("undertakerName")]
        public string? UndertakerName { get; set; }

        /// <summary>類別（CategoryDefinition.CategoryEnum 值）</summary>
        [JsonPropertyName("categoryId")]
        public int? CategoryId { get; set; }

        [JsonPropertyName("customerNo")]
        public string? CustomerNo { get; set; }

        // ── 聯絡方式 ──────────────────────────────────────────────
        [JsonPropertyName("contactName")]
        public string? ContactName { get; set; }

        [JsonPropertyName("contactTitle")]
        public string? ContactTitle { get; set; }

        [JsonPropertyName("contactPhone")]
        public string? ContactPhone { get; set; }

        [JsonPropertyName("contactMobilePhone")]
        public string? ContactMobilePhone { get; set; }

        [JsonPropertyName("contactEmail")]
        public string? ContactEmail { get; set; }

        // ── 日期 ──────────────────────────────────────────────────
        [JsonPropertyName("creationDate")]
        public DateTime? CreationDate { get; set; }

        [JsonPropertyName("expirationDate")]
        public DateTime? ExpirationDate { get; set; }

        [JsonPropertyName("authorizationNotBefore")]
        public DateTime? AuthorizationNotBefore { get; set; }

        [JsonPropertyName("authorizationNotAfter")]
        public DateTime? AuthorizationNotAfter { get; set; }

        [JsonPropertyName("invoiceRequestNotBefore")]
        public DateTime? InvoiceRequestNotBefore { get; set; }

        [JsonPropertyName("invoiceRequestNotAfter")]
        public DateTime? InvoiceRequestNotAfter { get; set; }

        [JsonPropertyName("taxNo")]
        public string? TaxNo { get; set; }

        // ── 設定項目（直接對應 OrganizationViewModel 布林屬性） ─────
        [JsonPropertyName("setToPrintInvoice")]
        public bool? SetToPrintInvoice { get; set; }

        [JsonPropertyName("invoicePrintView")]
        public string? InvoicePrintView { get; set; }

        [JsonPropertyName("c0401POSView")]
        public string? C0401POSView { get; set; }

        [JsonPropertyName("allowancePrintView")]
        public string? AllowancePrintView { get; set; }

        [JsonPropertyName("authorizationNo")]
        public string? AuthorizationNo { get; set; }

        [JsonPropertyName("entrustToPrint")]
        public bool? EntrustToPrint { get; set; }

        [JsonPropertyName("downloadDataNumber")]
        public bool? DownloadDataNumber { get; set; }

        [JsonPropertyName("uploadBranchTrackBlank")]
        public bool? UploadBranchTrackBlank { get; set; }

        [JsonPropertyName("autoBlankTrack")]
        public bool? AutoBlankTrack { get; set; }

        [JsonPropertyName("autoBlankTrackEmittance")]
        public bool? AutoBlankTrackEmittance { get; set; }

        [JsonPropertyName("printAll")]
        public bool? PrintAll { get; set; }

        /// <summary>發票類別（Naming.InvoiceTypeDefinition 值）</summary>
        [JsonPropertyName("settingInvoiceType")]
        public int? SettingInvoiceType { get; set; }

        [JsonPropertyName("subscribeB2BInvoicePDF")]
        public bool? SubscribeB2BInvoicePDF { get; set; }

        [JsonPropertyName("enableTrackCodeInvoiceNoValidation")]
        public bool? EnableTrackCodeInvoiceNoValidation { get; set; }

        [JsonPropertyName("setToOutsourcingCS")]
        public bool? SetToOutsourcingCS { get; set; }

        [JsonPropertyName("downloadDispatch")]
        public bool? DownloadDispatch { get; set; }

        [JsonPropertyName("setToNotifyCounterpartBySMS")]
        public bool? SetToNotifyCounterpartBySMS { get; set; }

        [JsonPropertyName("useB2BStandalone")]
        public bool? UseB2BStandalone { get; set; }

        /// <summary>
        /// 字串型設定旗標（對應舊版表單 name="Settings" 複選），
        /// 例如 DisableC0401Template、ForcedAuditNo、IgnoreDuplicatedNo、InvoiceExchange、
        /// AllB2B、SendAllowanceMIGManually、InvoiceNotUploadedAlert、HybridB2B。
        /// </summary>
        [JsonPropertyName("settings")]
        public string[]? Settings { get; set; }

        // ── 訊息通知 ──────────────────────────────────────────────
        /// <summary>
        /// 已啟用的通知旗標位元值清單（對應舊版表單 name="NoticeStatus" 複選，
        /// 後端 CommitOrganizationViewModel 會加總為 InvoiceNoticeSetting 位元遮罩）。
        /// </summary>
        [JsonPropertyName("noticeStatus")]
        public int[]? NoticeStatus { get; set; }

        [JsonPropertyName("businessContactPhone")]
        public string? BusinessContactPhone { get; set; }

        [JsonPropertyName("customNotificationView")]
        public string? CustomNotificationView { get; set; }

        [JsonPropertyName("customNotification")]
        public string? CustomNotification { get; set; }

        /// <summary>公司識別標章相對路徑（唯讀，僅供顯示）。</summary>
        [JsonPropertyName("logoUrl")]
        public string? LogoUrl { get; set; }
    }
}
