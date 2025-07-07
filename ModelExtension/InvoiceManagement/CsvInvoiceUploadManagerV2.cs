using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Security.MembershipManagement;
using ModelCore.Helper;
using CommonLib.Utility;
using ModelCore.UploadManagement;

using ModelCore.Resource;
using ModelCore.InvoiceManagement.InvoiceProcess;
using CommonLib.DataAccess;

namespace ModelCore.InvoiceManagement
{
    public class CsvInvoiceUploadManagerV2 : CsvInvoiceUploadManager
    {

        public CsvInvoiceUploadManagerV2(GenericManager<EIVOEntityDataContext> manager, int sellerID)
            : base(manager,sellerID)
        {

        }

        public CsvInvoiceUploadManagerV2(int sellerID)
            : base(sellerID)
        {
        }

        protected override void initialize()
        {
            __COLUMN_COUNT = 21;   
            /* 
             *   序號   ,  日期  ,    客戶ID   ,    品名    ,    單價    ,     數量     ,金額,
             * 對方統編 , 銷售額 ,     稅額    ,  含稅金額  ,    名稱    ,    聯絡人    ,地址,
             * 連絡電話 , E-MAIL , 載具類別號碼, 載具顯碼ID , 載具隱碼ID , 發票捐贈對象
             */
            
            _uploadInvoiceDate = DateTime.Now;
        }

        protected virtual bool checkCarrierDataIsComplete(ItemUpload<InvoiceItem> item)
        {
            bool c_type = String.IsNullOrEmpty(item.Columns[(int)FieldIndex.載具類別號碼]);
            bool c_id_1 = String.IsNullOrEmpty(item.Columns[(int)FieldIndex.載具顯碼ID]);
            bool c_id_2 = String.IsNullOrEmpty(item.Columns[(int)FieldIndex.載具隱碼ID]);
            if (c_type)
            {
                if ((!c_id_1 && c_id_2) || (c_id_1 && !c_id_2) || (!c_id_1 && !c_id_2))
                {
                    item.Status = String.Join("、", item.Status, MessageResources.AlertInvoiceCarrierComplete);
                    _bResult = false;
                }
            }
            else
            {
                if (c_id_1 && c_id_2)
                {
                    item.Status = String.Join("、", item.Status, MessageResources.AlertInvoiceCarrierComplete);                    
                    _bResult = false;
                }
            }

            if ((!c_type && item.Columns[(int)FieldIndex.載具類別號碼].Length > 6) || (!c_id_1 && item.Columns[(int)FieldIndex.載具顯碼ID].Length > 64) || (!c_id_2 && item.Columns[(int)FieldIndex.載具隱碼ID].Length > 64))
            {
                item.Status = String.Join("、", item.Status, String.Format(MessageResources.AlertInvoiceCarrierLength, item.Columns[(int)FieldIndex.載具類別號碼].Length, item.Columns[(int)FieldIndex.載具顯碼ID].Length, item.Columns[(int)FieldIndex.載具隱碼ID].Length));
                _bResult = false;
            }

            return _bResult;
        }


        protected override bool validate(ItemUpload<InvoiceItem> item)
        {
            String[] column = item.Columns;
            BusinessRelationship relation = null;
            bool isB2C = this.isB2C(item.Columns);
            if (!isB2C)
            {
                if (column[(int)FieldIndex.對方統編].Length > 8 || !ValidityAgent.ValidateString(column[(int)FieldIndex.對方統編], 20))
                {
                    item.Status = String.Join("、", item.Status, "對方統編格式錯誤 => " + column[(int)FieldIndex.對方統編]);
                    _bResult = false;
                }
                else if (column[(int)FieldIndex.對方統編].Length < 8)
                {
                    column[(int)FieldIndex.對方統編] = column[(int)FieldIndex.對方統編].PadLeft(8, '0');
                }

                if (_seller.IsEnterpriseGroupMember())
                {
                    relation = _seller.MasterRelation.Where(b => b.Counterpart.ReceiptNo == column[(int)FieldIndex.對方統編])
                        .FirstOrDefault();
                    if (relation == null)
                    {
                        item.Status = String.Join("、", item.Status, "對方統編不為已設定的B2B相對營業人 => " + column[(int)FieldIndex.對方統編]);
                        _bResult = false;
                    }
                }
            }

            if (_seller.OrganizationStatus.CurrentLevel == (int)Naming.MemberStatusDefinition.Mark_To_Delete)
            {
                item.Status = String.Join("、", String.Format("開立人已註記停用,開立人統一編號:{0}，TAG:<SellerId />", _seller.ReceiptNo));
            }

            checkInputFields(item);

            ItemUpload<InvoiceItem> firstItem;
            if(AutoTrackNo)
            {
                firstItem = _items.Where(i => i.Columns[(int)FieldIndex.客戶ID_發票號碼] == item.Columns[(int)FieldIndex.客戶ID_發票號碼]
                   && i.Columns[(int)FieldIndex.序號] == item.Columns[(int)FieldIndex.序號]).FirstOrDefault();
            }
            else
            {
                firstItem = _items.Where(i => i.Columns[(int)FieldIndex.客戶ID_發票號碼] == item.Columns[(int)FieldIndex.客戶ID_發票號碼]).FirstOrDefault();
                _uploadInvoiceDate = DateTime.Parse(column[(int)FieldIndex.日期]);
            }

            if (firstItem == null)
            {
                InvoiceCarrier carrier = null;

                #region 載具、全列印相關

                //B2B
                if (!String.IsNullOrEmpty(item.Columns[(int)FieldIndex.對方統編]))
                {
                    if (!String.IsNullOrEmpty(item.Columns[(int)FieldIndex.載具顯碼ID]) && !String.IsNullOrEmpty(item.Columns[(int)FieldIndex.載具隱碼ID]) && !String.IsNullOrEmpty(item.Columns[(int)FieldIndex.載具類別號碼]))
                    {
                        carrier = new InvoiceCarrier
                        {
                            CarrierNo = column[(int)FieldIndex.載具顯碼ID],
                            CarrierNo2 = column[(int)FieldIndex.載具隱碼ID],
                            CarrierType = column[(int)FieldIndex.載具類別號碼]
                        };
                    }
                }
                else //B2C
                {
                    if (_seller.OrganizationStatus.PrintAll != true)
                    {
                        if (String.IsNullOrEmpty(item.Columns[(int)FieldIndex.發票捐贈對象]))
                        {
                            if (String.IsNullOrEmpty(item.Columns[(int)FieldIndex.載具顯碼ID]) && String.IsNullOrEmpty(item.Columns[(int)FieldIndex.載具隱碼ID]) && String.IsNullOrEmpty(item.Columns[(int)FieldIndex.載具類別號碼]))
                            {
                                carrier = new InvoiceCarrier
                                {
                                    CarrierType = EIVOTurnkeyFactory.DefaultUserCarrierType
                                };
                                carrier.CarrierNo = carrier.CarrierNo2 = Guid.NewGuid().ToString();
                            }
                            else
                            {
                                if (checkCarrierDataIsComplete(item))
                                {
                                    carrier = new InvoiceCarrier
                                    {
                                        CarrierNo = column[(int)FieldIndex.載具顯碼ID],
                                        CarrierNo2 = column[(int)FieldIndex.載具隱碼ID],
                                        CarrierType = column[(int)FieldIndex.載具類別號碼]
                                    };
                                }
                            }
                        }
                        else
                        {
                            if (checkCarrierDataIsComplete(item))
                            {
                                carrier = new InvoiceCarrier
                                {
                                    CarrierNo = column[(int)FieldIndex.載具顯碼ID],
                                    CarrierNo2 = column[(int)FieldIndex.載具隱碼ID],
                                    CarrierType = column[(int)FieldIndex.載具類別號碼]
                                };
                            }
                        }
                    }
                }

                #endregion

                item.Entity = new InvoiceItem
                {
                    CDS_Document = new CDS_Document
                    {
                        DocDate = DateTime.Now,
                        DocType = (int)Naming.DocumentTypeDefinition.E_Invoice,
                        DocumentOwner = new DocumentOwner
                        {
                            OwnerID = _sellerID
                        },
                        ProcessType = _seller.IsEnterpriseGroupMember() 
                                        ? (int)Naming.InvoiceProcessType.A0401
                                        : (int)Naming.InvoiceProcessType.C0401,
                    },
                    DonateMark = _seller.OrganizationStatus.PrintAll == true ? "0" : (!String.IsNullOrEmpty(column[(int)FieldIndex.對方統編]) ? "0" : (String.IsNullOrEmpty(column[(int)FieldIndex.發票捐贈對象]) ? "0" : "1")),
                    InvoiceDonation = _seller.OrganizationStatus.PrintAll == true ? null : (!String.IsNullOrEmpty(column[(int)FieldIndex.對方統編]) ? null : (String.IsNullOrEmpty(column[(int)FieldIndex.發票捐贈對象]) ? null : new InvoiceDonation
                    {
                        AgencyCode = column[(int)FieldIndex.發票捐贈對象],
                    })),
                    InvoiceDate = _uploadInvoiceDate,
                    InvoiceType = (byte?)_seller.OrganizationStatus.SettingInvoiceType?? (byte?)DefaultInvoiceType ?? (byte)Naming.InvoiceTypeDefinition.一般稅額計算之電子發票,
                    SellerID = _sellerID,
                    RandomNo = ValidityAgent.GenerateRandomCode(4),//"AAAA",ValidityAgent.GenerateRandomCode(4),

                    //DonationID = donatory != null ? donatory.CompanyID : (int?)null,
                    InvoicePurchaseOrder = new InvoicePurchaseOrder
                    {
                        InvoicePurchaseOrderUpload = _uploadItem,
                        OrderNo = column[(int)FieldIndex.序號] + column[(int)FieldIndex.客戶ID_發票號碼]
                    },
                    InvoiceSeller = new InvoiceSeller
                    {
                        Name = _seller.CompanyName,
                        ReceiptNo = _seller.ReceiptNo,
                        Address = _seller.Addr,
                        ContactName = _seller.ContactName,
                        CustomerName = _seller.CompanyName,
                        EMail = _seller.ContactEmail,
                        Fax = _seller.Fax,
                        Phone = _seller.Phone,
                        PersonInCharge = _seller.UndertakerName,
                        SellerID = _seller.CompanyID
                    },
                    InvoiceBuyer = new InvoiceBuyer
                    {
                        BuyerID = relation != null ? (int?)relation.RelativeID : null,
                        CustomerID = AutoTrackNo ? column[(int)FieldIndex.客戶ID_發票號碼] : null,
                        ReceiptNo = isB2C ? "0000000000" : column[(int)FieldIndex.對方統編],
                        Name = isB2C ? column[(int)FieldIndex.名稱].CheckB2CMIGName() : column[(int)FieldIndex.名稱],
                        CustomerName = column[(int)FieldIndex.名稱],
                        ContactName = column[(int)FieldIndex.聯絡人],
                        EMail = column[(int)FieldIndex.email],
                        Address = column[(int)FieldIndex.地址],
                        Phone = column[(int)FieldIndex.連絡電話]
                    },
                    Remark = column[(int)FieldIndex.備註].InsteadOfNullOrEmpty(null),
                    InvoiceCarrier = carrier,
                    PrintMark = _seller.OrganizationStatus.PrintAll == true ? "Y" : (!String.IsNullOrEmpty(column[(int)FieldIndex.對方統編]) ? "Y" : "N" ),
                };
                checkAmountValue(item);
                checkInvoiceNo(item);
                checkOrderNo(item);
                checkDetail(item, item);

                item.Entity.CDS_Document.PushStepQueueOnSubmit(this, Naming.InvoiceStepDefinition.已開立, Naming.InvoiceProcessType.F0401);
                item.Entity.CDS_Document.PushStepQueueOnSubmit(this, Naming.InvoiceStepDefinition.已接收資料待通知, Naming.InvoiceProcessType.F0401);

            }
            else
            {
                checkFirstItem(firstItem, item);
                checkDetail(firstItem, item);
            }

            return _bResult;
        }

    }
}
