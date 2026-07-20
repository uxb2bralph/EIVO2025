using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TaskCenter.Core.DTOs
{
    /// <summary>
    /// 線上開立發票（單張）請求（遷移自 WebHome InvoiceBusinessController.CommitInvoice / CommitA0101
    /// 所繫結的 InvoiceViewModel）。同時支援 F0401 存證（SimpleInvoice）與 A0101 B2B 交換（B2BInvoice）。
    /// 由 [FromBody] 繫結（繫結大小寫不敏感，故 request DTO 不加 [JsonPropertyName]）。
    /// 金額（銷售額 / 稅額 / 總計）沿用舊版：由前端「金額」按鈕計算後帶入。
    /// </summary>
    public class CreateInvoiceRequestDto
    {
        /// <summary>開立人（加密後的 Organization.CompanyID；對應舊版 EncSellerID）。</summary>
        public string? SellerKey { get; set; }

        /// <summary>電子發票型式（Naming.InvoiceProcessType；41 F0401 存證 / 21 A0101 交換）。</summary>
        public int ProcessType { get; set; } = 41;

        /// <summary>是否僅供內容預覽（true 時驗證後回傳預覽內容，不寫入）。</summary>
        public bool ForPreview { get; set; }

        // ── 買受人 ────────────────────────────────────────────
        public string? BuyerReceiptNo { get; set; }
        public string? BuyerName { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? EMail { get; set; }
        public string? CustomerID { get; set; }
        public byte? BuyerMark { get; set; }
        /// <summary>設為相對營業人（A0101：不存在時自動建立買受人與關係）。</summary>
        public bool? Counterpart { get; set; }
        /// <summary>以相對營業人關係自動帶入買受人（F0401）。</summary>
        public bool? B2BRelation { get; set; }

        // ── 發票主檔 ──────────────────────────────────────────
        public string? RandomNo { get; set; }
        public byte? InvoiceType { get; set; } = 7;
        public byte? TaxType { get; set; }
        public byte? CustomsClearanceMark { get; set; }
        public decimal? TaxRate { get; set; }
        public decimal? SalesAmount { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public string? CarrierType { get; set; }
        public string? CarrierId1 { get; set; }
        public string? CarrierId2 { get; set; }
        public string? NPOBAN { get; set; }
        public string? Remark { get; set; }
        public string? DataNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }

        // ── A0101 交換專屬欄位 ────────────────────────────────
        public string? CheckNo { get; set; }
        public string? BuyerRemark { get; set; }
        public string? RelateNumber { get; set; }
        public string? Category { get; set; }

        // ── 發票明細 ──────────────────────────────────────────
        public List<CreateInvoiceLineDto>? Lines { get; set; }
    }

    /// <summary>發票明細品項列（對應舊版 InvoiceViewModel 之平行陣列 Brief/ItemNo/Piece/…）。</summary>
    public class CreateInvoiceLineDto
    {
        public string? ItemNo { get; set; }
        public string? Brief { get; set; }
        public int? Piece { get; set; }
        public decimal? UnitCost { get; set; }
        public decimal? CostAmount { get; set; }
        public string? Remark { get; set; }
    }

    /// <summary>
    /// 開立成功結果（對應舊版 InvoiceCreated.cshtml / A0401Created.cshtml 之確認資訊）。
    /// 列印 / 立即檢視（需 QRCode 金鑰）延後，僅回傳號碼與識別鍵供前端顯示。
    /// </summary>
    public class CreateInvoiceResultDto
    {
        /// <summary>加密後的 InvoiceID（供後續列印 / 檢視動作）。</summary>
        [JsonPropertyName("keyId")] public string? KeyId { get; set; }
        [JsonPropertyName("invoiceNo")] public string? InvoiceNo { get; set; }
        [JsonPropertyName("trackCode")] public string? TrackCode { get; set; }
        [JsonPropertyName("no")] public string? No { get; set; }
        /// <summary>列印註記（Y = 需列印證明聯 / N = 使用載具或捐贈）。</summary>
        [JsonPropertyName("printMark")] public string? PrintMark { get; set; }
        /// <summary>是否使用載具（不列印證明聯）。</summary>
        [JsonPropertyName("hasCarrier")] public bool HasCarrier { get; set; }
        [JsonPropertyName("processType")] public int ProcessType { get; set; }
    }

    /// <summary>
    /// 發票內容預覽（對應舊版 CommitInvoice ForPreview 回傳的 InvoiceContent.cshtml）。
    /// 由驗證後（未寫入）的 InvoiceItem 組裝而成。
    /// </summary>
    public class InvoicePreviewDto
    {
        [JsonPropertyName("invoiceNo")] public string? InvoiceNo { get; set; }
        [JsonPropertyName("invoiceDate")] public DateTime? InvoiceDate { get; set; }
        [JsonPropertyName("randomNo")] public string? RandomNo { get; set; }
        [JsonPropertyName("sellerName")] public string? SellerName { get; set; }
        [JsonPropertyName("sellerReceiptNo")] public string? SellerReceiptNo { get; set; }
        [JsonPropertyName("buyerName")] public string? BuyerName { get; set; }
        [JsonPropertyName("buyerReceiptNo")] public string? BuyerReceiptNo { get; set; }
        [JsonPropertyName("buyerAddress")] public string? BuyerAddress { get; set; }
        [JsonPropertyName("buyerEmail")] public string? BuyerEmail { get; set; }
        /// <summary>是否為 B2C（買受人統編為 0000000000）。</summary>
        [JsonPropertyName("isB2C")] public bool IsB2C { get; set; }
        [JsonPropertyName("taxTypeLabel")] public string? TaxTypeLabel { get; set; }
        [JsonPropertyName("salesAmount")] public decimal? SalesAmount { get; set; }
        [JsonPropertyName("taxAmount")] public decimal? TaxAmount { get; set; }
        [JsonPropertyName("totalAmount")] public decimal? TotalAmount { get; set; }
        [JsonPropertyName("carrierType")] public string? CarrierType { get; set; }
        [JsonPropertyName("carrierNo")] public string? CarrierNo { get; set; }
        [JsonPropertyName("agencyCode")] public string? AgencyCode { get; set; }
        [JsonPropertyName("remark")] public string? Remark { get; set; }
        [JsonPropertyName("lines")] public List<InvoicePreviewLineDto> Lines { get; set; } = new();
    }

    /// <summary>發票內容預覽品項列。</summary>
    public class InvoicePreviewLineDto
    {
        [JsonPropertyName("seq")] public int Seq { get; set; }
        [JsonPropertyName("itemNo")] public string? ItemNo { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
        [JsonPropertyName("quantity")] public decimal? Quantity { get; set; }
        [JsonPropertyName("unitPrice")] public decimal? UnitPrice { get; set; }
        [JsonPropertyName("amount")] public decimal? Amount { get; set; }
        [JsonPropertyName("remark")] public string? Remark { get; set; }
    }

    /// <summary>開立人候選項目（供前端選擇開立人 autocomplete）。</summary>
    public class CreateInvoiceSellerOptionDto
    {
        [JsonPropertyName("sellerKey")] public string? SellerKey { get; set; }
        [JsonPropertyName("receiptNo")] public string? ReceiptNo { get; set; }
        [JsonPropertyName("companyName")] public string? CompanyName { get; set; }
    }

    /// <summary>
    /// 相對營業人（買受人）候選 / 帶入資料（對應舊版 Home/SearchCounterpart + DataEntity/Organization）。
    /// 以開立人的相對營業人關係（BusinessRelationship，銷項）為範圍。
    /// </summary>
    public class CounterpartOptionDto
    {
        [JsonPropertyName("receiptNo")] public string? ReceiptNo { get; set; }
        [JsonPropertyName("companyName")] public string? CompanyName { get; set; }
        [JsonPropertyName("address")] public string? Address { get; set; }
        [JsonPropertyName("phone")] public string? Phone { get; set; }
        [JsonPropertyName("email")] public string? Email { get; set; }
        [JsonPropertyName("customerId")] public string? CustomerId { get; set; }
    }

    /// <summary>產品快速查詢候選項目（對應舊版 ProductCatalog/QuickSearch）。</summary>
    public class ProductOptionDto
    {
        [JsonPropertyName("productId")] public int ProductId { get; set; }
        [JsonPropertyName("productName")] public string? ProductName { get; set; }
        [JsonPropertyName("salePrice")] public decimal SalePrice { get; set; }
        [JsonPropertyName("remark")] public string? Remark { get; set; }
        [JsonPropertyName("barcode")] public string? Barcode { get; set; }
        [JsonPropertyName("spec")] public string? Spec { get; set; }
        [JsonPropertyName("pieceUnit")] public string? PieceUnit { get; set; }
    }

    /// <summary>
    /// 開立作業結果（服務層內部載體）：依 Outcome 分為驗證錯誤、內容預覽、開立成功三種。
    /// </summary>
    public class CreateInvoiceCommitResult
    {
        public CreateInvoiceOutcome Outcome { get; set; }
        public string? Message { get; set; }
        public InvoicePreviewDto? Preview { get; set; }
        public CreateInvoiceResultDto? Created { get; set; }

        public static CreateInvoiceCommitResult Error(string message)
            => new() { Outcome = CreateInvoiceOutcome.Error, Message = message };
        public static CreateInvoiceCommitResult ForPreview(InvoicePreviewDto preview)
            => new() { Outcome = CreateInvoiceOutcome.Preview, Preview = preview };
        public static CreateInvoiceCommitResult Success(CreateInvoiceResultDto created)
            => new() { Outcome = CreateInvoiceOutcome.Created, Created = created };
    }

    public enum CreateInvoiceOutcome
    {
        Error,
        Preview,
        Created,
    }
}
