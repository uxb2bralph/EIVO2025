using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

/// <summary>
/// 電子發票主檔
/// </summary>
public partial class InvoiceItem
{
    /// <summary>
    /// Primary Key
    /// </summary>
    public int InvoiceID { get; set; }

    /// <summary>
    /// 發票號碼
    /// </summary>
    public string? No { get; set; }

    /// <summary>
    /// 發票日期
    /// </summary>
    public DateTime? InvoiceDate { get; set; }

    /// <summary>
    /// 發票檢查碼
    /// </summary>
    public string? CheckNo { get; set; }

    /// <summary>
    /// 總備註
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 買受人註記欄
    /// 1：得抵扣之進貨及費用；
    /// 2：得抵扣之固定資產；
    /// 3：不得抵扣之進貨及費用；
    /// 4：不得抵扣之固定資產
    /// 
    /// </summary>
    public byte? BuyerRemark { get; set; }

    /// <summary>
    /// 通關方式註記
    /// 1：非經海關出口;
    /// 2：經海關出口(零稅率時，為必要欄位)
    /// 
    /// </summary>
    public byte? CustomsClearanceMark { get; set; }

    /// <summary>
    /// 稅捐稽徵處名稱
    /// </summary>
    public string? TaxCenter { get; set; }

    /// <summary>
    /// 核准日
    /// </summary>
    public DateTime? PermitDate { get; set; }

    /// <summary>
    /// 核准文
    /// </summary>
    public string? PermitWord { get; set; }

    /// <summary>
    /// 核准號
    /// </summary>
    public string? PermitNumber { get; set; }

    /// <summary>
    /// 沖帳別
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// 相關號碼
    /// </summary>
    public string? RelateNumber { get; set; }

    /// <summary>
    /// 發票類別
    /// 1: 三聯式;
    /// 2: 二聯式;
    /// 3: 二聯式收銀機;
    /// 4. 特種稅額;
    /// 5: 電子計算機;
    /// 6: 三聯式收銀機
    /// 
    /// </summary>
    public byte? InvoiceType { get; set; }

    /// <summary>
    /// 彙開註記
    /// 以”*”表示 彙開
    /// </summary>
    public string? GroupMark { get; set; }

    /// <summary>
    /// 捐贈註記
    /// 以”0”表示 非捐贈發票
    /// 以”1”表示 為捐贈發票
    /// 
    /// </summary>
    public string? DonateMark { get; set; }

    public int? SellerID { get; set; }

    public int? DonationID { get; set; }

    /// <summary>
    /// 發票防偽隨機碼
    /// 前端隨機產生
    /// </summary>
    public string? RandomNo { get; set; }

    public string? TrackCode { get; set; }

    public byte? BondedAreaConfirm { get; set; }

    public string? PrintMark { get; set; }

    public int? ProcessType { get; set; }

    public int? TrackID { get; set; }

    public virtual AuthorizeToVoid? AuthorizeToVoid { get; set; }

    public virtual ICollection<DocumentPostLog> DocumentPostLog { get; set; } = new List<DocumentPostLog>();

    public virtual Organization? Donation { get; set; }

    public virtual CDS_Document Invoice { get; set; } = null!;

    public virtual ICollection<InvoiceAllowance> InvoiceAllowance { get; set; } = new List<InvoiceAllowance>();

    public virtual InvoiceAmountType? InvoiceAmountType { get; set; }

    public virtual InvoiceBuyer? InvoiceBuyer { get; set; }

    public virtual InvoiceByHousehold? InvoiceByHousehold { get; set; }

    public virtual InvoiceCancellation? InvoiceCancellation { get; set; }

    public virtual InvoiceCarrier? InvoiceCarrier { get; set; }

    public virtual ICollection<InvoiceDeliveryTracking> InvoiceDeliveryTracking { get; set; } = new List<InvoiceDeliveryTracking>();

    public virtual InvoiceDonation? InvoiceDonation { get; set; }

    public virtual InvoiceItemExtension? InvoiceItemExtension { get; set; }

    public virtual InvoiceMail? InvoiceMail { get; set; }

    public virtual InvoiceNoAssignment? InvoiceNoAssignment { get; set; }

    public virtual InvoicePaperRequest? InvoicePaperRequest { get; set; }

    public virtual InvoicePrintAssertion? InvoicePrintAssertion { get; set; }

    public virtual InvoicePrintQueue? InvoicePrintQueue { get; set; }

    public virtual InvoicePurchaseOrder? InvoicePurchaseOrder { get; set; }

    public virtual ICollection<InvoicePurchaseOrderAudit> InvoicePurchaseOrderAudit { get; set; } = new List<InvoicePurchaseOrderAudit>();

    public virtual InvoiceSeller? InvoiceSeller { get; set; }

    public virtual InvoiceWinningNumber? InvoiceWinningNumber { get; set; }

    public virtual Organization? Seller { get; set; }

    public virtual InvoiceTrackCode? Track { get; set; }

    public virtual ICollection<InvoiceProduct> Product { get; set; } = new List<InvoiceProduct>();
}
