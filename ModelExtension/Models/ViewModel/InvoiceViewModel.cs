using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ModelCore.Locale;
using CommonLib.Utility;

namespace ModelCore.Models.ViewModel
{
    public class InvoiceViewModel : QueryViewModel
    {
        public InvoiceViewModel()
        {
            InvoiceType = 7;
            InvoiceDate = DateTime.Now;
            TaxAmount = 1;
            No = "00000000";
            RandomNo = String.Format("{0:0000}",(DateTime.Now.Ticks % 10000));
            TaxRate = 0.05m;
            DonateMark = "0";
        }
        public int? SellerID { get; set; }
        public String SellerName { get; set; }
        public String SellerReceiptNo { get; set; }
        public String? BuyerReceiptNo { get; set; }  //11405
        public String No { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public String Remark { get; set; }
        public String BuyerRemark { get; set; }
        public byte? CustomsClearanceMark { get; set; }
        public byte? InvoiceType { get; set; }
        public byte? TaxType { get; set; }
        public decimal? TaxRate { get; set; }
        public String? RandomNo { get; set; }
        public string DonateMark { get; set; }
        public string CarrierType { get; set; }
        public string CarrierId1 { get; set; }
        public string CarrierId2 { get; set; }
        public string NPOBAN { get; set; }
        public string? CustomerID { get; set; }
        public string? BuyerName { get; set; }
        public String TrackCode { get; set; }
        public String DataNumber { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? SalesAmount { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public String[] Brief { get; set; }
        public String[] ItemNo { get; set; }
        public String[] ItemRemark { get; set; }
        public decimal?[] UnitCost { get; set; }
        public decimal?[] CostAmount { get; set; }
        public int?[] Piece { get; set; }
        public bool? Counterpart { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? EMail { get; set; }
        public byte? BuyerMark { get; set; }
        public string CheckNo { get; set; }
        public string RelateNumber { get; set; }
        public string Category { get; set; }
        public Naming.InvoiceProcessType? InvoiceProcessType { get; set; }
        public bool? ForPreview { get; set; }
        public bool? B2BRelation { get; set; }
    }

    public class InvoiceBuyerViewModel : QueryViewModel
    {
        public int? InvoiceID { get; set; }
        public String ReceiptNo { get; set; }
        public String PostCode { get; set; }
        public String Address { get; set; }
        public String Name { get; set; }
        public int? BuyerID { get; set; }
        public String CustomerID { get; set; }
        public String ContactName { get; set; }
        public String Phone { get; set; }
        public String EMail { get; set; }
        public String CustomerName { get; set; }
        public String Fax { get; set; }
        public String PersonInCharge { get; set; }
        public String RoleRemark { get; set; }
        public String CustomerNumber { get; set; }
        public int? BuyerMark { get; set; }
    }

    public class AllowanceViewModel : QueryViewModel
    {
        public AllowanceViewModel()
        {
            AllowanceType = Naming.AllowanceTypeDefinition.賣方開立;
        }

        public int? AllowanceID { get; set; }
        public String AllowanceNumber { get; set; }
        public Naming.AllowanceTypeDefinition? AllowanceType { get; set; }
        public DateTime? AllowanceDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TaxAmount { get; set; }
        public int? SellerID { get; set; }
        public String BuyerReceiptNo { get; set; }
        public String BuyerName { get; set; }
        public int? InvoiceID { get; set; }
        public Naming.InvoiceProcessType? ProcessType { get; set; }

        public int?[] ItemID { get; set; }
        public short[] No { get; set; }
        public String[] InvoiceNo { get; set; }
        public decimal?[] Piece { get; set; }
        public decimal?[] Amount { get; set; }
        public decimal[] Tax { get; set; }
        public DateTime[] InvoiceDate { get; set; }
        public short?[] OriginalSequenceNo { get; set; }
        public String[] PieceUnit { get; set; }
        public String[] OriginalDescription { get; set; }
        public Naming.TaxTypeDefinition[] TaxType { get; set; }
        public decimal?[] UnitCost { get; set; }
    }
}