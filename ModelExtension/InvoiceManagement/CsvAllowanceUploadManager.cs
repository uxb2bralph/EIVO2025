using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;


using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.Locale;
using ModelCore.Security.MembershipManagement;
using ModelCore.UploadManagement;
using CommonLib.Utility;
using CommonLib.DataAccess;

namespace ModelCore.InvoiceManagement
{
    public class CsvAllowanceUploadManager : CsvUploadManager<EIVOEntityDataContext, InvoiceAllowance, ItemUpload<InvoiceAllowance>>
    {
        public const String __Fields = "序號,日期,客戶ID,折讓單號碼,欲折讓發票之原發票開立日期,欲折讓發票號碼,欲折讓原品名,欲折讓單價未稅,欲折讓數量,欲折讓金額未稅,對方統編,欲折讓稅額,欲折讓含稅金額,原發票名稱,原發票聯絡人,原發票地址,原發票聯絡電話,原發票email";

        public enum FieldIndex
        {
            序號 = 0,
            日期,
            客戶ID,
            折讓單號碼,
            欲折讓發票之原發票開立日期,
            欲折讓發票號碼,
            欲折讓原品名,
            欲折讓單價未稅,
            欲折讓數量,
            欲折讓金額未稅,
            對方統編,
            欲折讓稅額,
            欲折讓含稅金額,
            原發票名稱,
            原發票聯絡人,
            原發票地址,
            原發票聯絡電話,
            原發票email

        }

        protected int _sellerID;
        protected Organization _seller;

        //private bool bDisposed = false;

        public CsvAllowanceUploadManager(GenericManager<EIVOEntityDataContext> manager, int sellerID)
            : base(manager)
        {
            _sellerID = sellerID;
        }

        public CsvAllowanceUploadManager(int sellerID)
            : this(null, sellerID)
        {
        }

        protected override void initialize()
        {
            __COLUMN_COUNT = 18;   
        }

        public override void ParseData(UserProfile userProfile, string fileName, Encoding encoding)
        {
            _seller = this.GetTable<Organization>().Where(o => o.CompanyID == _sellerID).First();
            base.ParseData(userProfile, fileName, encoding);
        }

        protected override void doSave()
        {
            var items = _items.Where(i => i.Entity != null).Select(i => i.Entity);
            this.EntityList.InsertAllOnSubmit(items);
            this.SubmitChanges();

            //items.Select(i => i.AllowanceID).SendIssuingNotification();
        }

        protected bool isB2C(String[] values)
        {
            return String.IsNullOrEmpty(values[(int)FieldIndex.對方統編]);
        }

        public Organization Seller
        {
            get
            {
                return _seller;
            }
        }

        protected override bool validate(ItemUpload<InvoiceAllowance> item)
        {
            string[] column = item.Columns;
            item.Entity = new InvoiceAllowance
            {
                CDS_Document = new CDS_Document
                {
                    DocDate = DateTime.Now,
                    DocType = (int)Naming.DocumentTypeDefinition.E_Allowance,
                    DocumentOwner = new DocumentOwner
                    {
                        OwnerID = _seller.CompanyID
                    }
                },
                AllowanceDate = DateTime.Now,
                IssueDate = DateTime.Now,
                InvoiceAllowanceSeller = new InvoiceAllowanceSeller
                {
                    SellerID = _seller.CompanyID,
                    Name = _seller.CompanyName,
                    Address = _seller.Addr,
                    EMail = _seller.ContactEmail,
                    Fax = _seller.Fax,
                    ReceiptNo = _seller.ReceiptNo,
                    PersonInCharge = _seller.UndertakerName,
                    RoleRemark = _seller.ContactTitle,
                    Phone = _seller.Phone,
                    CustomerName = _seller.CompanyName,
                    ContactName = _seller.ContactName

                },
                SellerId = _seller.ReceiptNo
            };

            item.Entity.InvoiceAllowanceDetails.Add(new InvoiceAllowanceDetail
            {
                InvoiceAllowanceItem = new InvoiceAllowanceItem
                {
                    TaxType = 1,
                    No = 0
                }
            });

            checkInputFields(item);
            checkDateValue(item);
            var invItem = checkInvoiceNo(item);
            if (invItem != null)
                checkAmountValue(item, invItem);

            return _bResult;
        }

        private void checkAmountValue(ItemUpload<InvoiceAllowance> item, InvoiceItem invItem)
        {
            String[] column = item.Columns;
            bool isB2C = invItem.InvoiceBuyer.IsB2C();

            decimal totalAmt;
            decimal salesAmt;
            decimal taxAmt = 0;

            if (decimal.TryParse(column[(int)FieldIndex.欲折讓含稅金額], out totalAmt))
            {
                if (isB2C)
                {
                    salesAmt = decimal.Round(totalAmt / 1.05m, 0, MidpointRounding.AwayFromZero);
                    taxAmt = totalAmt - salesAmt;
                    item.Entity.TotalAmount = totalAmt;
                    item.Entity.TaxAmount = taxAmt;
                }
                else
                {
                    if (decimal.TryParse(column[(int)FieldIndex.欲折讓稅額], out taxAmt))
                    {
                        if (decimal.TryParse(column[(int)FieldIndex.欲折讓金額未稅], out salesAmt))
                        {
                            item.Entity.TotalAmount = totalAmt;
                            item.Entity.TaxAmount = taxAmt;
                        }
                        else
                        {
                            item.Status = String.Join("、", item.Status, "未稅金額錯誤");
                            _bResult = false;
                        }
                    }
                    else
                    {
                        item.Status = String.Join("、", item.Status, "稅額錯誤");
                        _bResult = false;
                    }
                }
            }
            else
            {
                item.Status = String.Join("、", item.Status, "含稅金額錯誤");
                _bResult = false;
            }


            decimal costAmt;
            if (decimal.TryParse(column[(int)FieldIndex.欲折讓單價未稅], out costAmt))
            {
                decimal piece;
                if (decimal.TryParse(column[(int)FieldIndex.欲折讓數量], out piece))
                {
                    item.Entity.InvoiceAllowanceDetails[0].InvoiceAllowanceItem.Amount = totalAmt;
                    item.Entity.InvoiceAllowanceDetails[0].InvoiceAllowanceItem.Piece = piece;
                    item.Entity.InvoiceAllowanceDetails[0].InvoiceAllowanceItem.Tax = taxAmt;
                    item.Entity.InvoiceAllowanceDetails[0].InvoiceAllowanceItem.UnitCost = costAmt;
                }
                else
                {
                    item.Status = String.Join("、", item.Status, "數量錯誤");
                    _bResult = false;
                }
            }
            else
            {
                item.Status = String.Join("、", item.Status, "單價(未稅)錯誤");
                _bResult = false;
            }
        }

        private void checkDateValue(ItemUpload<InvoiceAllowance> item)
        {
            string[] column = item.Columns;
            DateTime dateValue;
            if (DateTime.TryParseExact(column[(int)FieldIndex.日期], "yyyy/M/d", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateValue))
            {
                item.Entity.AllowanceDate = item.Entity.IssueDate = dateValue;
            }
            else
            {
                item.Status = String.Join("、", item.Status, "折讓單日期錯誤");
                _bResult = false;
            }

            DateTime invoiceDate;
            if (DateTime.TryParseExact(column[(int)FieldIndex.欲折讓發票之原發票開立日期], "yyyy/M/d", CultureInfo.CurrentCulture, DateTimeStyles.None, out invoiceDate))
            {
                item.Entity.InvoiceAllowanceDetails[0].InvoiceAllowanceItem.InvoiceDate = invoiceDate;
            }
            else
            {
                item.Status = String.Join("、", item.Status, "發票日期錯誤");
                _bResult = false;
            }
        }

        private InvoiceItem checkInvoiceNo(ItemUpload<InvoiceAllowance> item)
        {
            InvoiceItem invItem = null;

            if (item.Columns[(int)FieldIndex.欲折讓發票號碼].Length != 10 || !ValidityAgent.ValidateString(item.Columns[(int)FieldIndex.欲折讓發票號碼], 14))
            {
                item.Status = String.Join("、", item.Status, "發票號碼格式錯誤");
                _bResult = false;
                return null;
            }
            else
            {
                String trackCode = item.Columns[(int)FieldIndex.欲折讓發票號碼].Substring(0, 2);
                String no = item.Columns[(int)FieldIndex.欲折讓發票號碼].Substring(2);

                invItem = this.GetTable<InvoiceItem>().Where(i => i.TrackCode == trackCode && i.No == no).FirstOrDefault();
            }

            if (invItem == null)
            {
                item.Status = String.Join("、", item.Status, "發票不存在");
                _bResult = false;
                return null;
            }

            if (invItem.InvoiceBuyer.CustomerID != item.Columns[(int)FieldIndex.客戶ID])
            {
                item.Status = String.Join("、", item.Status, "Google ID(Customer ID)不存在");
                _bResult = false;
            }

            item.Entity.InvoiceID = invItem.InvoiceID;
            item.Entity.InvoiceItem = invItem;
            if ((invItem.InvoiceBuyer.IsB2C() && (item.Columns[(int)FieldIndex.對方統編] == String.Empty || item.Columns[(int)FieldIndex.對方統編]=="0000000000")) || invItem.InvoiceBuyer.ReceiptNo == item.Columns[(int)FieldIndex.對方統編])
            {
                item.Entity.BuyerId = invItem.InvoiceBuyer.ReceiptNo;
                item.Entity.InvoiceAllowanceBuyer = new InvoiceAllowanceBuyer
                {
                    Address = invItem.InvoiceBuyer.Address,
                    ContactName = invItem.InvoiceBuyer.ContactName,
                    CustomerID = invItem.InvoiceBuyer.CustomerID,
                    CustomerName = invItem.InvoiceBuyer.CustomerName,
                    Name = invItem.InvoiceBuyer.Name,
                    EMail = invItem.InvoiceBuyer.EMail,
                    Phone = invItem.InvoiceBuyer.Phone,
                    PostCode = invItem.InvoiceBuyer.PostCode,
                    ReceiptNo = invItem.InvoiceBuyer.ReceiptNo
                };
            }
            else
            {
                item.Status = String.Join("、", item.Status, "買受人統編錯誤");
                _bResult = false;
            }

            //if (String.IsNullOrEmpty(item.Columns[(int)FieldIndex.原發票名稱]))
            //{
            //    item.Status = String.Join("、", item.Status, "買受人名稱不得為空白");
            //    _bResult = false;
            //}

            //if (String.IsNullOrEmpty(item.Columns[(int)FieldIndex.原發票聯絡人]))
            //{
            //    item.Status = String.Join("、", item.Status, "聯絡人不得為空白");
            //    _bResult = false;
            //}
            //if (String.IsNullOrEmpty(item.Columns[(int)FieldIndex.原發票地址]))
            //{
            //    item.Status = String.Join("、", item.Status, "買受人地址不得為空白");
            //    _bResult = false;
            //}
            //if (String.IsNullOrEmpty(item.Columns[(int)FieldIndex.原發票聯絡電話]))
            //{
            //    item.Status = String.Join("、", item.Status, "連絡電話不得為空白");
            //    _bResult = false;
            //}
            //if (String.IsNullOrEmpty(item.Columns[(int)FieldIndex.原發票email]) && !ValidityAgent.ValidateString(item.Columns[(int)FieldIndex.原發票email], 16))
            //{
            //    item.Status = String.Join("、", item.Status, "EMail錯誤");
            //    _bResult = false;
            //}

            item.Entity.AllowanceType = invItem.InvoiceBuyer.IsB2C() ? (byte)2 : (byte)1;
            item.Entity.InvoiceAllowanceDetails[0].InvoiceAllowanceItem.TaxType = invItem.InvoiceDetails[0].InvoiceProduct.InvoiceProductItem[0].TaxType ?? invItem.InvoiceAmountType.TaxType;
            item.Entity.InvoiceAllowanceDetails[0].InvoiceAllowanceItem.ProductItemID = invItem.InvoiceDetails[0].InvoiceProduct.InvoiceProductItem[0].ItemID;
            item.Entity.InvoiceAllowanceDetails[0].InvoiceAllowanceItem.InvoiceNo = invItem.TrackCode + invItem.No;
            if (item.Entity.InvoiceAllowanceDetails[0].InvoiceAllowanceItem.InvoiceDate.HasValue && invItem.InvoiceDate.Value.Date != item.Entity.InvoiceAllowanceDetails[0].InvoiceAllowanceItem.InvoiceDate.Value.Date)
            {
                item.Status = String.Join("、", item.Status, "發票日期錯誤");
                _bResult = false;
            }

            return invItem;
        }

        private void checkInputFields(ItemUpload<InvoiceAllowance> item)
        {
            String[] column = item.Columns;

            if (String.IsNullOrEmpty(column[(int)FieldIndex.折讓單號碼]))
            {
                item.Status = String.Join("、", item.Status, "未指定折讓號碼");
                _bResult = false;
            }
            else
            {
                item.Entity.AllowanceNumber = column[(int)FieldIndex.折讓單號碼];
                if (EntityList.Any(a => a.AllowanceNumber == column[(int)FieldIndex.折讓單號碼]))
                {
                    item.Status = String.Join("、", item.Status, "折讓單號碼重複");
                    _bResult = false;
                }
            }

            if (String.IsNullOrEmpty(column[(int)FieldIndex.欲折讓原品名]))
            {
                item.Status = String.Join("、", item.Status, "未指定品名");
                _bResult = false;
            }
            else
            {
                item.Entity.InvoiceAllowanceDetails[0].InvoiceAllowanceItem.OriginalDescription = column[(int)FieldIndex.欲折讓原品名];
            }

            if (String.IsNullOrEmpty(column[(int)FieldIndex.序號]) || !ValidityAgent.ValidateString(column[0], 20))
            {
                item.Status = String.Join("、", item.Status, "序號格式錯誤");
                _bResult = false;
            }

            //if (String.IsNullOrEmpty(column[(int)FieldIndex.客戶ID]) || !ValidityAgent.ValidateString(column[(int)FieldIndex.客戶ID], 20))
            //{
            //    item.Status = String.Join("、", item.Status, "客戶ID格式錯誤");
            //    _bResult = false;
            //}

            if (String.IsNullOrEmpty(column[(int)FieldIndex.折讓單號碼]) || !ValidityAgent.ValidateString(column[(int)FieldIndex.折讓單號碼], 14))
            {
                item.Status = String.Join("、", item.Status, "折讓單號碼格式錯誤");
                _bResult = false;
            }

            if (_items.Any(a => a.Columns[(int)FieldIndex.折讓單號碼] == column[(int)FieldIndex.折讓單號碼]))
            {
                item.Status = String.Join("、", item.Status, "折讓單號碼重複匯入");
                _bResult = false;
            }

        }
    }

}
