using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement.ErrorHandle;
using ModelCore.Locale;
using ModelCore.Resource;
using ModelCore.Schema.EIVO;
using ModelCore.Models.ViewModel;
using CommonLib.Utility;
using System.Data;
using ModelCore.InvoiceManagement.InvoiceProcess;
using CommonLib.DataAccess;
using CommonLib.Core.Utility;

namespace ModelCore.InvoiceManagement.Validator
{
    public partial class InvoiceDataSetValidator : InvoiceRootInvoiceValidator
    {
        protected new DataRow _invItem;
        protected IEnumerable<DataRow> _details;

        public Organization ExpectedSeller { get; set; }

        internal InvoiceFieldIndex InvoiceField = new InvoiceFieldIndex { };
        internal class InvoiceFieldIndex
        {
            public int Invoice_No { get; internal set; } = 0;
            public int Invoice_Date { get; internal set; } = 1;
            public int Data_ID { get; internal set; } = 2;
            public int Data_Date { get; internal set; } = 3;
            public int Seller_ID { get; internal set; } = 4;
            public int Buyer_Name { get; internal set; } = 5;
            public int Buyer_ID { get; internal set; } = 6;
            public int Buyer_Mark { get; internal set; } = 7;
            public int Customs_Clearance_Mark { get; internal set; } = 8;
            public int Customer_ID { get; internal set; } = 9;
            public int Contact_Name { get; internal set; } = 10;
            public int EMail { get; internal set; } = 11;
            public int Address { get; internal set; } = 12;
            public int Phone { get; internal set; } = 13;
            public int Sales_Amount { get; internal set; } = 14;
            public int Free_Tax_Sales_Amount { get; internal set; } = 15;
            public int Zero_Tax_Sales_Amount { get; internal set; } = 16;
            public int Invoice_Type { get; internal set; } = 17;
            public int Tax_Type { get; internal set; } = 18;
            public int Tax_Rate { get; internal set; } = 19;
            public int Tax_Amount { get; internal set; } = 20;
            public int Total_Amount { get; internal set; } = 21;
            public int Currency { get; internal set; } = 22;
            public int Print_Mark { get; internal set; } = 23;
            public int Carrier_Type { get; internal set; } = 24;
            public int Carrier_Id1 { get; internal set; } = 25;
            public int Carrier_Id2 { get; internal set; } = 26;
            public int Donate_Mark { get; internal set; } = 27;
            public int NPOBAN { get; internal set; } = 28;
            public int Random_Number { get; internal set; } = 29;
            public int Main_Remark { get; internal set; } = 30;

        }

        internal DetailsFieldIndex DetailsField = new DetailsFieldIndex { };
        internal class DetailsFieldIndex
        {
            public int Invoice_No { get; internal set; } = 0;
            public int Data_ID { get; internal set; } = 1;
            public int Description { get; internal set; } = 2;
            public int Quantity { get; internal set; } = 3;
            public int Unit { get; internal set; } = 4;
            public int Unit_Price { get; internal set; } = 5;
            public int Amount { get; internal set; } = 6;
            public int Item_Tax_Type { get; internal set; } = 7;
            public int Remark { get; internal set; } = 8;

        }

        public InvoiceDataSetValidator(GenericManager<EIVOEntityDataContext> mgr, Organization owner, Naming.InvoiceProcessType processType) : base(mgr, owner)
        {
            this.processType = processType;
            resetFieldIndex();
            initializeDeliveryCheck();
        }

        void resetFieldIndex()
        {
            switch (processType)
            {
                case Naming.InvoiceProcessType.C0401_Xlsx_Allocation_ByVAC:
                    InvoiceField.Data_ID = 0;
                    InvoiceField.Data_Date = 1;
                    InvoiceField.Seller_ID = 2;
                    InvoiceField.Buyer_Name = 3;
                    InvoiceField.Buyer_ID = 4;
                    InvoiceField.Buyer_Mark = 5;
                    InvoiceField.Customs_Clearance_Mark = 6;
                    InvoiceField.Customer_ID = 7;
                    InvoiceField.Contact_Name = 8;
                    InvoiceField.EMail = 9;
                    InvoiceField.Address = 10;
                    InvoiceField.Phone = 11;
                    InvoiceField.Sales_Amount = 12;
                    InvoiceField.Free_Tax_Sales_Amount = 13;
                    InvoiceField.Zero_Tax_Sales_Amount = 14;
                    InvoiceField.Invoice_Type = 15;
                    InvoiceField.Tax_Type = 16;
                    InvoiceField.Tax_Rate = 17;
                    InvoiceField.Tax_Amount = 18;
                    InvoiceField.Total_Amount = 19;
                    InvoiceField.Currency = 20;
                    InvoiceField.Print_Mark = 21;
                    InvoiceField.Carrier_Type = 22;
                    InvoiceField.Carrier_Id1 = 23;
                    InvoiceField.Carrier_Id2 = 24;
                    InvoiceField.Donate_Mark = 25;
                    InvoiceField.NPOBAN = 26;
                    InvoiceField.Random_Number = 27;
                    InvoiceField.Main_Remark = 28;

                    DetailsField.Data_ID = 0;
                    DetailsField.Description = 1;
                    DetailsField.Quantity = 2;
                    DetailsField.Unit = 3;
                    DetailsField.Unit_Price = 4;
                    DetailsField.Amount = 5;
                    DetailsField.Item_Tax_Type = 6;
                    DetailsField.Remark = 7;

                    break;
                case Naming.InvoiceProcessType.C0401_Xlsx_CBE:
                    InvoiceField.Data_ID = 0;
                    InvoiceField.Data_Date = 1;
                    InvoiceField.Seller_ID = 2;
                    InvoiceField.Customer_ID = 3;
                    InvoiceField.Sales_Amount = 4;
                    InvoiceField.Tax_Amount = 5;
                    InvoiceField.Total_Amount = 6;
                    InvoiceField.Currency = 7;
                    InvoiceField.Carrier_Id1 = 8;
                    InvoiceField.Main_Remark = 9;

                    DetailsField.Data_ID = 0;
                    DetailsField.Description = 1;
                    DetailsField.Quantity = 2;
                    DetailsField.Unit = 3;
                    DetailsField.Unit_Price = 4;
                    DetailsField.Amount = 5;
                    DetailsField.Remark = 6;

                    break;

                case Naming.InvoiceProcessType.A0401_Xlsx_Allocation_ByIssuer:
                case Naming.InvoiceProcessType.C0401_Xlsx_Allocation_ByIssuer:
                    InvoiceField.Invoice_No = 0;
                    InvoiceField.Invoice_Date = 1;
                    InvoiceField.Data_ID = 2;
                    InvoiceField.Seller_ID = 3;
                    InvoiceField.Buyer_Name = 4;
                    InvoiceField.Buyer_ID = 5;
                    InvoiceField.Buyer_Mark = 6;
                    InvoiceField.Customs_Clearance_Mark = 7;
                    InvoiceField.Customer_ID = 8;
                    InvoiceField.Contact_Name = 9;
                    InvoiceField.EMail = 10;
                    InvoiceField.Address = 11;
                    InvoiceField.Phone = 12;
                    InvoiceField.Sales_Amount = 13;
                    InvoiceField.Free_Tax_Sales_Amount = 14;
                    InvoiceField.Zero_Tax_Sales_Amount = 15;
                    InvoiceField.Invoice_Type = 16;
                    InvoiceField.Tax_Type = 17;
                    InvoiceField.Tax_Rate = 18;
                    InvoiceField.Tax_Amount = 19;
                    InvoiceField.Total_Amount = 20;
                    InvoiceField.Currency = 21;
                    InvoiceField.Print_Mark = 22;
                    InvoiceField.Carrier_Type = 23;
                    InvoiceField.Carrier_Id1 = 24;
                    InvoiceField.Carrier_Id2 = 25;
                    InvoiceField.Donate_Mark = 26;
                    InvoiceField.NPOBAN = 27;
                    InvoiceField.Random_Number = 28;
                    InvoiceField.Main_Remark = 29;

                    DetailsField.Invoice_No = 0;
                    DetailsField.Description = 1;
                    DetailsField.Quantity = 2;
                    DetailsField.Unit = 3;
                    DetailsField.Unit_Price = 4;
                    DetailsField.Amount = 5;
                    DetailsField.Item_Tax_Type = 6;
                    DetailsField.Remark = 7;

                    break;
            }
        }

        String NPOBAN()
        {
            return processType == Naming.InvoiceProcessType.C0401_Xlsx_CBE ? null : GetString(InvoiceField.NPOBAN);
        }

        String RandomNumber()
        {
            return GetString(InvoiceField.Random_Number);
        }

        byte? CustomsClearanceMark()
        {
            return processType == Naming.InvoiceProcessType.C0401_Xlsx_CBE ? null : GetData<byte>(InvoiceField.Customs_Clearance_Mark);
        }

        byte? TaxType()
        {
            return processType == Naming.InvoiceProcessType.C0401_Xlsx_CBE ? (byte)Naming.TaxTypeDefinition.應稅 : GetData<byte>(InvoiceField.Tax_Type);
        }
        byte? InvoiceType()
        {
            return processType == Naming.InvoiceProcessType.C0401_Xlsx_CBE ? (byte)Naming.InvoiceTypeDefinition.一般稅額計算之電子發票 : GetData<byte>(InvoiceField.Invoice_Type);
        }

        decimal? TaxAmount()
        {
            return GetData<decimal>(InvoiceField.Tax_Amount);
        }

        decimal? TotalAmount()
        {
            return GetData<decimal>(InvoiceField.Total_Amount);
        }
        decimal? SalesAmount() { return GetData<decimal>(InvoiceField.Sales_Amount); }
        decimal? FreeTaxSalesAmount() { return processType == Naming.InvoiceProcessType.C0401_Xlsx_CBE ? null : GetData<decimal>(InvoiceField.Free_Tax_Sales_Amount); }
        decimal? ZeroTaxSalesAmount() { return processType == Naming.InvoiceProcessType.C0401_Xlsx_CBE ? null : GetData<decimal>(InvoiceField.Zero_Tax_Sales_Amount); }
        decimal? TaxRate() { return processType == Naming.InvoiceProcessType.C0401_Xlsx_CBE ? 0.05m : GetData<decimal>(InvoiceField.Tax_Rate); }
        string PrintMark() { return processType == Naming.InvoiceProcessType.C0401_Xlsx_CBE ? "N" : GetString(InvoiceField.Print_Mark)?.ToUpper(); }
        string MainRemark() { return GetString(InvoiceField.Main_Remark); }
        DateTime? InvoiceDate() { return processType == Naming.InvoiceProcessType.C0401_Xlsx_CBE ? null : GetData<DateTime>(InvoiceField.Invoice_Date); }
        String InvoiceNo()
        {
            return processType == Naming.InvoiceProcessType.C0401_Xlsx_CBE || processType == Naming.InvoiceProcessType.C0401_Xlsx_Allocation_ByVAC ? null : GetString(InvoiceField.Invoice_No);
        }
        String DataID() { return GetString(InvoiceField.Data_ID); }
        DateTime? DataDate() { return GetData<DateTime>(InvoiceField.Data_Date); }
        String SellerID() { return GetString(InvoiceField.Seller_ID); }
        String BuyerID() { return processType == Naming.InvoiceProcessType.C0401_Xlsx_CBE ? "0000000000" : GetString(InvoiceField.Buyer_ID).GetEfficientString() ?? "0000000000"; }
        String BuyerName() { return processType == Naming.InvoiceProcessType.C0401_Xlsx_CBE ? null : GetString(InvoiceField.Buyer_Name); }
        String CarrierType() { return processType == Naming.InvoiceProcessType.C0401_Xlsx_CBE ? __CROSS_BORDER_MURCHANT : GetString(InvoiceField.Carrier_Type); }
        String CarrierId1() { return GetString(InvoiceField.Carrier_Id1); }
        String CarrierId2() { return processType == Naming.InvoiceProcessType.C0401_Xlsx_CBE ? null : GetString(InvoiceField.Carrier_Id2); }
        String EMail() { return processType == Naming.InvoiceProcessType.C0401_Xlsx_CBE ? GetString(InvoiceField.Carrier_Id1) : GetString(InvoiceField.EMail); }
        String Address() { return processType == Naming.InvoiceProcessType.C0401_Xlsx_CBE ? null : GetString(InvoiceField.Address); }
        String Phone() { return processType == Naming.InvoiceProcessType.C0401_Xlsx_CBE ? null : GetString(InvoiceField.Phone); }
        int? BuyerMark()
        {
            return processType == Naming.InvoiceProcessType.C0401_Xlsx_CBE ? null : GetData<int>(InvoiceField.Buyer_Mark);
        }
        String CustomerID() { return GetString(InvoiceField.Customer_ID); }
        String ContactName()
        {
            return processType == Naming.InvoiceProcessType.C0401_Xlsx_CBE ? null : GetString(InvoiceField.Contact_Name);
        }
        String DonateMark() { return processType == Naming.InvoiceProcessType.C0401_Xlsx_CBE ? "0" : GetString(InvoiceField.Donate_Mark); }
        String Currency() { return GetString(InvoiceField.Currency); }


        Nullable<T> GetData<T>(int index)
            where T : struct
        {
            return _invItem.GetData<T>((int)index);
        }

        Nullable<T> GetDetails<T>(DataRow row, int index)
            where T : struct
        {
            return row.GetData<T>((int)index);
        }

        String GetString(int index)
        {
            return _invItem.GetString((int)index);
        }

        String GetString(DataRow row, int index)
        {
            return row.GetString((int)index);
        }


        private void initializeDeliveryCheck()
        {
            #region 列印Y

            _deliveryCheck[(int)PrintedMark.Yes, (int)CarrierIntent.Yes, (int)IsB2C.Yes, (int)DonationIntent.Yes] = () =>
            {
                return new Exception(String.Format(MessageResources.AlertPrintedInvoiceCarrierType, CarrierType()));
            };

            _deliveryCheck[(int)PrintedMark.Yes, (int)CarrierIntent.Yes, (int)IsB2C.Yes, (int)DonationIntent.No] =
                _deliveryCheck[(int)PrintedMark.Yes, (int)CarrierIntent.Yes, (int)IsB2C.Yes, (int)DonationIntent.Yes];

            _deliveryCheck[(int)PrintedMark.Yes, (int)CarrierIntent.Yes, (int)IsB2C.No, (int)DonationIntent.Yes] = () =>
            {
                return new Exception(String.Format(MessageResources.AlertDonationInvoiceCarryType, CarrierType()));
            };

            _deliveryCheck[(int)PrintedMark.Yes, (int)CarrierIntent.Yes, (int)IsB2C.No, (int)DonationIntent.No] = checkPublicCarrier;

            _deliveryCheck[(int)PrintedMark.Yes, (int)CarrierIntent.No, (int)IsB2C.Yes, (int)DonationIntent.Yes] = () =>
            {
                return new Exception(String.Format(MessageResources.AlertPrintedInvoiceDonation, PrintMark()));
            };

            _deliveryCheck[(int)PrintedMark.Yes, (int)CarrierIntent.No, (int)IsB2C.No, (int)DonationIntent.Yes] = () =>
            {
                return new Exception(String.Format(MessageResources.AlertDonationInvoiceCarryType, CarrierType()));
            };

            _deliveryCheck[(int)PrintedMark.Yes, (int)CarrierIntent.No, (int)IsB2C.Yes, (int)DonationIntent.No] =

            _deliveryCheck[(int)PrintedMark.Yes, (int)CarrierIntent.No, (int)IsB2C.No, (int)DonationIntent.No] = () => { return null; };

            #endregion 


            #region 列印N

            _deliveryCheck[(int)PrintedMark.No, (int)CarrierIntent.Yes, (int)IsB2C.Yes, (int)DonationIntent.Yes] = () =>
                {
                    Exception ex = checkCarrierDataIsComplete();
                    if (ex != null)
                        return ex;

                    if (String.IsNullOrEmpty(NPOBAN()))
                    {
                        return new Exception(String.Format(MessageResources.InvalidDonationTaker, NPOBAN()));
                    }
                    else
                    {
                        if (NPOBAN().Length < 3 || NPOBAN().Length > 7)
                            return new Exception(String.Format(MessageResources.InvalidDonationTaker, NPOBAN()));
                    }

                    _donation = new InvoiceDonation
                    {
                        AgencyCode = NPOBAN()
                    };

                    return null;
                };

            _deliveryCheck[(int)PrintedMark.No, (int)CarrierIntent.No, (int)IsB2C.Yes, (int)DonationIntent.Yes] = () =>
                {
                    if (String.IsNullOrEmpty(NPOBAN()))
                    {
                        return new Exception(String.Format(MessageResources.InvalidDonationTaker, NPOBAN()));
                    }
                    else
                    {
                        if (NPOBAN().Length < 3 || NPOBAN().Length > 7)
                            return new Exception(String.Format(MessageResources.InvalidDonationTaker, NPOBAN()));


                    }
                    _donation = new InvoiceDonation
                    {
                        AgencyCode = NPOBAN()
                    };

                    return null;

                };

            #endregion

        }

        public virtual Exception Validate(DataRow dataItem, IEnumerable<DataRow> details)
        {
            _invItem = dataItem;
            _details = details;

            Exception ex;

            _isAutoTrackNo = String.IsNullOrEmpty(InvoiceNo());

            //_seller = null;
            _newItem = null;

            if ((ex = checkBusiness()) != null)
            {
                return ex;
            }

            if (_isAutoTrackNo)
            {
                if ((ex = checkDataNumber()) != null)
                {
                    return ex;
                }
            }
            else
            {
                buildDataNumber();
            }

            if ((ex = checkAmount()) != null)
            {
                return ex;
            }

            if ((ex = checkInvoiceDelivery()) != null)
            {
                return ex;
            }

            if ((ex = checkMandatoryFields()) != null)
            {
                return ex;
            }

            if ((ex = checkInvoiceProductItems()) != null)
            {
                return ex;
            }

            if ((ex = checkInvoice()) != null)
            {
                return ex;
            }


            return null;
        }


        protected override Exception checkInvoice()
        {
            DuplicateProcess = false;
            _newItem = new InvoiceItem
            {
                CDS_Document = new CDS_Document
                {
                    DocDate = DateTime.Now,
                    DocType = (int)Naming.DocumentTypeDefinition.E_Invoice,
                    DocumentOwner = new DocumentOwner
                    {
                        OwnerID = _owner.CompanyID
                    },
                    ProcessType = processType == Naming.InvoiceProcessType.A0401_Xlsx_Allocation_ByIssuer
                        ? (int)Naming.InvoiceProcessType.A0401
                        : (int)Naming.InvoiceProcessType.C0401,
                },
                DonateMark = _donation == null ? "0" : "1",
                InvoiceType = InvoiceTypeIndication == Naming.InvoiceTypeDefinition.一般稅額計算之電子發票
                                ? (byte)Naming.InvoiceTypeDefinition.一般稅額計算之電子發票
                                : (byte)Naming.InvoiceTypeDefinition.特種稅額計算之電子發票,
                SellerID = _seller.CompanyID,
                CustomsClearanceMark = CustomsClearanceMark(),
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
                    SellerID = _seller.CompanyID,
                },
                InvoiceBuyer = _buyer,
                RandomNo = randomNo,
                InvoiceAmountType = new InvoiceAmountType
                {
                    SalesAmount = SalesAmount(),
                    ZeroTaxSalesAmount = ZeroTaxSalesAmount(),
                    FreeTaxSalesAmount = FreeTaxSalesAmount(),
                    TaxAmount = TaxAmount(),
                    TaxRate = TaxRate(),
                    TaxType = TaxType(),
                    TotalAmount = TotalAmount(),
                    //TotalAmountInChinese = ValidityAgent.MoneyShow(TotalAmount()),
                    CurrencyID = _currency?.CurrencyID,
                },
                InvoiceCarrier = _carrier,
                InvoiceDonation = _donation,
                PrintMark = printMark,
                Remark = MainRemark(),
            };

            if (_order != null)
            {
                _newItem.InvoicePurchaseOrder = _order;
            }

            if (_orderAudit != null)
            {
                _orderAudit.InvoiceItem = _newItem;
            }

            _newItem.InvoiceDetails.AddRange(_productItems.Select(p => new InvoiceDetail
            {
                InvoiceProduct = p.InvoiceProduct,
            }));

            if (_isAutoTrackNo)
            {
                try
                {
                    TrackNoManager trackNoMgr;
                    if (_trackNoManagerList.ContainsKey(_seller.CompanyID))
                    {
                        trackNoMgr = _trackNoManagerList[_seller.CompanyID];
                    }
                    else
                    {
                        trackNoMgr = new TrackNoManager(_models, _seller.CompanyID);
                        if (_autoTrackNoInvoiceDate.HasValue)
                        {
                            trackNoMgr.ApplyInvoiceDate(_autoTrackNoInvoiceDate.Value);
                        }
                        if (InvoiceTypeIndication != Naming.InvoiceTypeDefinition.一般稅額計算之電子發票)
                        {
                            trackNoMgr.ApplyInvoiceTypeIndication(InvoiceTypeIndication);
                        }
                        _trackNoManagerList[_seller.CompanyID] = trackNoMgr;
                    }

                    if (!trackNoMgr.CheckInvoiceNo(_newItem))
                    {
                        return new Exception(String.Format(MessageResources.AlertNullTrackNoInterval, _seller.ReceiptNo));
                    }
                    else
                    {
                        if (_autoTrackNoInvoiceDate.HasValue)
                        {
                            _newItem.InvoiceDate = _autoTrackNoInvoiceDate.Value.Add(DateTime.Now.TimeOfDay);
                        }
                        else
                        {
                            _newItem.InvoiceDate = DateTime.Now;
                        }
                    }

                }
                catch (Exception ex)
                {
                    return ex;
                }
            }
            else
            {
                DateTime? invoiceDate = InvoiceDate();

                if (!invoiceDate.HasValue)
                {
                    return new Exception(MessageResources.AlertInvoiceDate);
                }

                _newItem.InvoiceDate = invoiceDate;
                DateTime periodStart = new DateTime(invoiceDate.Value.Year, (invoiceDate.Value.Month - 1) / 2 * 2 + 1, 1);

                if (InvoiceNo() == null || !Regex.IsMatch(InvoiceNo(), "^[a-zA-Z]{2}[0-9]{8}$"))
                {
                    return new Exception(String.Format(MessageResources.AlertInvoiceNumber, InvoiceNo()));
                }

                var ex = checkInvoiceNo(_newItem, invoiceDate.Value, InvoiceNo());
                if (ex != null)
                {
                    return ex;
                }

                var currentItem = _models.GetTable<InvoiceItem>().Where(i => i.TrackCode == _newItem.TrackCode && i.No == _newItem.No
                            && i.InvoiceDate >= periodStart && i.InvoiceDate < periodStart.AddMonths(2)).FirstOrDefault();

                if (currentItem != null)
                {
                    //if (currentItem.SellerID == _seller.CompanyID && currentItem.RandomNo == randomNo)
                    //{
                    //    DuplicateProcess = true;
                    //    _newItem = currentItem;
                    //    return null;
                    //}
                    //else
                    {
                        return new Exception(MessageResources.AlertInvoiceDuplicated);
                    }
                }

                if (_seller.OrganizationStatus?.EnableTrackCodeInvoiceNoValidation == true)
                {
                    TrackNoManager trackMgr = new TrackNoManager(_models, _seller.CompanyID);
                    var item = trackMgr.GetAppliedInterval(invoiceDate.Value, _newItem.TrackCode, int.Parse(_newItem.No));

                    if (item == null)
                    {
                        return new Exception(String.Format("發票號碼錯誤:{0}，TAG:< InvoicNumber />", InvoiceNo()));
                    }
                }
            }

            return null;
        }

        protected override Exception checkDataNumber()
        {
            _order = null;
            _orderAudit = null;
            if (String.IsNullOrEmpty(DataID()))
            {
                return new Exception(MessageResources.AlertDataNumber);
            }

            if (DataID()?.Length > 60)
            {
                return new Exception(String.Format(MessageResources.AlertDataNumberLimitedLength, DataID()));
            }

            if (_seller.ForcedAuditNo())
            {
                _orderAudit = _models.CreateInvoicePurchaseOrderAudit(_seller.CompanyID, DataID());
                if (_orderAudit == null)
                {
                    return new Exception(String.Format(MessageResources.AlertDataNumberDuplicated, DataID()));
                }
            }

            var po = _models.GetTable<InvoicePurchaseOrder>().Where(d => d.OrderNo == DataID()
                    && d.InvoiceItem.SellerID == _seller.CompanyID).FirstOrDefault();
            if (po != null)
            {
                return new DuplicateDataNumberException(String.Format(MessageResources.AlertDataNumberDuplicated, DataID()))
                {
                    CurrentPO = po
                };
            }

            DateTime? dataDate = DataDate();
            if (!dataDate.HasValue)
            {
                return new Exception(MessageResources.AlertDataDate);
            }

            _order = new InvoicePurchaseOrder
            {
                OrderNo = DataID(),
                PurchaseDate = dataDate
            };

            return null;
        }

        protected void buildDataNumber()
        {
            _order = null;
            _orderAudit = null;

            if (!String.IsNullOrEmpty(DataID()))
            {
                _order = new InvoicePurchaseOrder
                {
                    OrderNo = DataID(),
                };
            }
        }

        String randomNo = null;
        protected override Exception checkBusiness()
        {
            if (_seller == null || _seller.ReceiptNo != SellerID())
            {

                _seller = _models.GetTable<Organization>().Where(o => o.ReceiptNo == SellerID()).FirstOrDefault();
                if (_seller == null)
                {
                    return new Exception(String.Format(MessageResources.AlertInvalidSeller, SellerID()));
                }

                ExpectedSeller = _seller;

                if (_seller.CompanyID != _owner.CompanyID && !_models.GetTable<InvoiceIssuerAgent>().Any(a => a.AgentID == _owner.CompanyID && a.IssuerID == _seller.CompanyID))
                {
                    //return new Exception(String.Format(MessageResources.AlertSellerSignature, SellerID()));
                    return new Exception(String.Format(MessageResources.InvalidSellerOrAgent, SellerID(), _owner.ReceiptNo));
                }

                if (_seller.OrganizationStatus.CurrentLevel == (int)Naming.MemberStatusDefinition.Mark_To_Delete)
                {
                    return new Exception(String.Format("開立人已註記停用,開立人統一編號:{0}，TAG:<SellerId />", SellerID()));
                }

                _isCrossBorderMerchant = _models.GetTable<OrganizationCategory>().Any(c => c.CompanyID == _seller.CompanyID && c.CategoryID == (int)Naming.CategoryID.COMP_CROSS_BORDER_MURCHANT);

            }

            if (processType == Naming.InvoiceProcessType.C0401_Xlsx_CBE)
            {
            }
            else if (BuyerID().GetEfficientString() == null)
            {
                _invItem[(int)InvoiceField.Buyer_ID] = "0000000000";
            }
            else if (BuyerID() == "0000000000")
            {
                //if (BuyerName() == null || Encoding.GetEncoding(950).GetBytes(BuyerName()).Length != 4)
                //{
                //    return new Exception(String.Format(MessageResources.InvalidBuyerName, BuyerName()));
                //}
            }
            else if (!Regex.IsMatch(BuyerID(), "^[0-9]{8}$"))
            {
                return new Exception(String.Format(MessageResources.InvalidBuyerId, BuyerID()));
            }
            else if (_seller.OrganizationStatus.EnableBuyerIDValidation != false && !BuyerID().CheckRegno())
            {
                return new Exception(String.Format(MessageResources.InvalidReceiptNo, BuyerID()));
            }
            else if (/*String.IsNullOrEmpty(BuyerName()) ||*/ BuyerName().Length > 60)
            {
                return new Exception(String.Format(MessageResources.InvalidBuyerNameLengthLimit, BuyerName()));
            }

            if (processType == Naming.InvoiceProcessType.C0401_Xlsx_CBE)
            {
                randomNo = String.Format("{0:ffff}", DateTime.Now);
            }
            else if ((randomNo = RandomNumber().GetEfficientString()) == null)
            {
                randomNo = String.Format("{0:ffff}", DateTime.Now);
                //ValidityAgent.GenerateRandomCode(4)
            }
            else if (!Regex.IsMatch(randomNo, "^[0-9]{4}$"))
            {
                return new Exception(String.Format(MessageResources.InvalidRandomNumber, randomNo));
            }

            return checkBusinessDetails();
        }

        protected override Exception usePublicCarrier(bool forced)
        {
            if (CarrierType() == __CELLPHONE_BARCODE)
            {
                if (checkPublicCarrierId(CarrierId1()))
                {
                    _carrier = new InvoiceCarrier
                    {
                        CarrierType = CarrierType(),
                        CarrierNo = CarrierId1(),
                        CarrierNo2 = CarrierId1()
                    };

                    return null;
                }
                else if (checkPublicCarrierId(CarrierId2()))
                {
                    _carrier = new InvoiceCarrier
                    {
                        CarrierType = CarrierType(),
                        CarrierNo = CarrierId2(),
                        CarrierNo2 = CarrierId2()
                    };

                    return null;
                }
                else
                {
                    return new Exception(String.Format(MessageResources.InvalidPublicCarrierType, CarrierType(), CarrierId1(), CarrierId2()));
                }
            }
            else if (CarrierType() == __自然人憑證)
            {
                if (CarrierId1() != null && Regex.IsMatch(CarrierId1(), "^[A-Z]{2}[0-9]{14}$"))
                {
                    _carrier = new InvoiceCarrier
                    {
                        CarrierType = CarrierType(),
                        CarrierNo = CarrierId1(),
                        CarrierNo2 = CarrierId1()
                    };

                    return null;
                }
                else if (CarrierId2() != null && Regex.IsMatch(CarrierId2(), "^[A-Z]{2}[0-9]{14}$"))
                {
                    _carrier = new InvoiceCarrier
                    {
                        CarrierType = CarrierType(),
                        CarrierNo = CarrierId2(),
                        CarrierNo2 = CarrierId2()
                    };

                    return null;
                }
                else
                {
                    return new Exception(String.Format(MessageResources.InvalidPublicCarrierType, CarrierType(), CarrierId1(), CarrierId2()));
                }
            }
            else if (CarrierType() == __CROSS_BORDER_MURCHANT)
            {
                if (CarrierId1() != null)
                {
                    _carrier = new InvoiceCarrier
                    {
                        CarrierType = CarrierType(),
                        CarrierNo = CarrierId1(),
                        CarrierNo2 = CarrierId1()
                    };

                    return null;
                }
                else if (CarrierId2() != null)
                {
                    _carrier = new InvoiceCarrier
                    {
                        CarrierType = CarrierType(),
                        CarrierNo = CarrierId2(),
                        CarrierNo2 = CarrierId2()
                    };

                    return null;
                }
                else
                {
                    return new Exception(String.Format(MessageResources.InvalidPublicCarrierType, CarrierType(), CarrierId1(), CarrierId2()));
                }
            }
            else
            {
                if (forced)
                {
                    return new Exception(String.Format(MessageResources.InvalidPublicCarrierType, CarrierType(), CarrierId1(), CarrierId2()));
                }
                else
                {
                    return null;
                }
            }

        }

        protected override Exception checkPublicCarrier()
        {
            if (CarrierType() == __CELLPHONE_BARCODE)
            {
                if (checkPublicCarrierId(CarrierId1()))
                {
                    _carrier = new InvoiceCarrier
                    {
                        CarrierType = CarrierType(),
                        CarrierNo = CarrierId1(),
                        CarrierNo2 = CarrierId1()
                    };

                    return null;
                }
                else if (checkPublicCarrierId(CarrierId2()))
                {
                    _carrier = new InvoiceCarrier
                    {
                        CarrierType = CarrierType(),
                        CarrierNo = CarrierId2(),
                        CarrierNo2 = CarrierId2()
                    };

                    return null;
                }
            }
            else if (CarrierType() == __自然人憑證)
            {
                if (CarrierId1() != null && Regex.IsMatch(CarrierId1(), "^[A-Z]{2}[0-9]{14}$"))
                {
                    _carrier = new InvoiceCarrier
                    {
                        CarrierType = CarrierType(),
                        CarrierNo = CarrierId1(),
                        CarrierNo2 = CarrierId1()
                    };

                    return null;
                }
                else if (CarrierId2() != null && Regex.IsMatch(CarrierId2(), "^[A-Z]{2}[0-9]{14}$"))
                {
                    _carrier = new InvoiceCarrier
                    {
                        CarrierType = CarrierType(),
                        CarrierNo = CarrierId2(),
                        CarrierNo2 = CarrierId2()
                    };

                    return null;
                }
            }
            else if (CarrierType() == __CROSS_BORDER_MURCHANT)
            {
                if (CarrierId1() != null)
                {
                    _carrier = new InvoiceCarrier
                    {
                        CarrierType = CarrierType(),
                        CarrierNo = CarrierId1(),
                        CarrierNo2 = CarrierId1()
                    };

                    return null;
                }
                else if (CarrierId2() != null)
                {
                    _carrier = new InvoiceCarrier
                    {
                        CarrierType = CarrierType(),
                        CarrierNo = CarrierId2(),
                        CarrierNo2 = CarrierId2()
                    };

                    return null;
                }
            }

            return new Exception(String.Format(MessageResources.InvalidPublicCarrierType, CarrierType(), CarrierId1(), CarrierId2()));
        }

        protected override Exception checkPrintAll()
        {

            if (_seller.OrganizationStatus.PrintAll == true)
            {
                _invItem[(int)InvoiceField.Print_Mark] = "Y";
            }
            else
            {
                if (UseDefaultCrossBorderMerchantCarrier && _isCrossBorderMerchant)
                {
                    var carrierID = EMail();
                    if (carrierID != null && carrierID.Length < 64)
                    {
                        _carrier = new InvoiceCarrier
                        {
                            CarrierType = __CROSS_BORDER_MURCHANT,
                            CarrierNo = carrierID
                        };
                    }
                    else
                    {
                        return new Exception($"EMail as Carrier ID limits to 64 characters, \"{carrierID}\"");
                    }
                }
                else
                {
                    _carrier = new InvoiceCarrier
                    {
                        CarrierType = EIVOTurnkeyFactory.DefaultUserCarrierType,
                        CarrierNo = Guid.NewGuid().ToString()
                    };
                }

                _carrier.CarrierNo2 = _carrier.CarrierNo;
            }

            return null;

        }

        protected override Exception checkBusinessDetails()
        {
            Organization buyer = null;
            BusinessRelationship relationship = null;
            String buyerName = null;
            String receiptNo = BuyerID();
            if (receiptNo == "0000000000")
            {
                buyerName = BuyerName().CheckB2CMIGName();
            }
            else
            {
                buyer = _models.GetTable<Organization>().Where(o => o.ReceiptNo == receiptNo).FirstOrDefault();
                if (buyer != null)
                {
                    relationship = _models.GetTable<BusinessRelationship>().Where(r => r.MasterID == _seller.CompanyID && r.RelativeID == buyer.CompanyID).FirstOrDefault();
                }

                buyerName = BuyerName().GetEfficientString() ?? relationship?.CompanyName ?? buyer?.CompanyName ?? null;
            }

            _buyer = new InvoiceBuyer
            {
                BuyerMark = BuyerMark(),
                Name = buyerName,
                ReceiptNo = BuyerID(),
                CustomerID = CustomerID(),
                CustomerName = receiptNo != "0000000000" ? buyerName : null,
            };

            if (_isCrossBorderMerchant)
            {
                _buyer.CustomerNumber = _buyer.ReceiptNo;
                _buyer.ReceiptNo = "0000000000";
            }

            _buyer.ContactName = ContactName().GetEfficientString() ?? buyer?.ContactName;
            if (relationship == null)
            {
                _buyer.Address = Address().GetEfficientString() ?? buyer?.Addr;
                _buyer.Phone = Phone().GetEfficientString() ?? buyer?.Phone;
                _buyer.EMail = EMail().GetEfficientString()?.Replace(';', ',').Replace('、', ',').Replace(' ', ',') ?? buyer?.ContactEmail;
            }
            else
            {
                _buyer.Address = Address().GetEfficientString() ?? relationship.Addr;
                _buyer.Phone = Phone().GetEfficientString() ?? relationship.Phone;
                _buyer.EMail = EMail().GetEfficientString()?.Replace(';', ',').Replace('、', ',').Replace(' ', ',') ?? relationship.ContactEmail;
            }

            return null;
        }

        protected override Exception checkInvoiceDelivery()
        {

            _carrier = null;
            _donation = null;

            var checkFunc = _deliveryCheck[Convert.ToInt32(PrintMark() == "Y"),
                Convert.ToInt32(!String.IsNullOrEmpty(CarrierType())
                    && !(String.IsNullOrEmpty(CarrierId1()) && String.IsNullOrEmpty(CarrierId2()))),
                Convert.ToInt32(BuyerID() == "0000000000"),
                Convert.ToInt32(DonateMark() == "1")];

            return checkFunc();

        }


        protected override Exception checkAmount()
        {
            //應稅銷售額
            if (SalesAmount() < 0 /*|| decimal.Floor(SalesAmount()) != SalesAmount()*/)
            {
                return new Exception(String.Format(MessageResources.InvalidSellingPrice, SalesAmount()));
            }

            if (FreeTaxSalesAmount() < 0 /*|| decimal.Floor(FreeTaxSalesAmount()) != FreeTaxSalesAmount()*/)
            {
                return new Exception(String.Format(MessageResources.InvalidFreeTaxAmount, FreeTaxSalesAmount()));
            }

            if (ZeroTaxSalesAmount() < 0 /*|| decimal.Floor(ZeroTaxSalesAmount()) != ZeroTaxSalesAmount()*/)
            {
                return new Exception(String.Format(MessageResources.InvalidZeroTaxAmount, ZeroTaxSalesAmount()));
            }


            if (TaxAmount() < 0 /*|| decimal.Floor(TaxAmount()) != TaxAmount()*/)
            {
                return new Exception(String.Format(MessageResources.InvalidTaxAmount, TaxAmount()));
            }

            if (TotalAmount() < 0 /*|| decimal.Floor(TotalAmount()) != TotalAmount()*/)
            {
                return new Exception(String.Format(MessageResources.InvalidTotalAmount, TotalAmount()));
            }

            //課稅別
            if (!Enum.IsDefined(typeof(Naming.TaxTypeDefinition), (int)TaxType()))
            {
                return new Exception(String.Format(MessageResources.InvalidTaxType, TaxType()));
            }

            if (TaxRate() < 0m)
            {
                return new Exception(String.Format(MessageResources.InvalidTaxRate, TaxRate()));
            }

            if (TaxType() == (byte)Naming.TaxTypeDefinition.零稅率)
            {
                if (!CustomsClearanceMark().HasValue)
                {
                    return new Exception(String.Format(MessageResources.AlertClearanceMarkZeroTax, CustomsClearanceMark()));
                }
                else if (CustomsClearanceMark() != 1 && CustomsClearanceMark() != 2)
                {
                    return new Exception(String.Format(MessageResources.AlertClearanceMarkExport, CustomsClearanceMark()));
                }
            }
            else if (CustomsClearanceMark().HasValue)
            {
                if (CustomsClearanceMark() != 1 && CustomsClearanceMark() != 2)
                {
                    return new Exception(String.Format(MessageResources.AlertClearanceMarkExport, CustomsClearanceMark()));
                }
            }

            //Currency() = Currency().GetEfficientString();
            _currency = null;
            if (!String.IsNullOrEmpty(Currency()))
            {
                _currency = _models.GetTable<CurrencyType>().Where(c => c.AbbrevName == Currency()).FirstOrDefault();
                if (_currency == null)
                {
                    return new Exception($"Invalid currency code：{Currency()}，TAG：<Currency/>");
                }
            }

            return null;
        }


        protected override Exception checkInvoiceProductItems()
        {
            if (_details == null || _details.Count() == 0)
            {
                return new Exception(MessageResources.InvalidInvoiceDetails);
            }

            short seqNo = 1;
            _productItems = _details.Select(i => new InvoiceProductItem
            {
                InvoiceProduct = new InvoiceProduct { Brief = GetString(i, DetailsField.Description) },
                CostAmount = GetDetails<decimal>(i, DetailsField.Amount),
                Piece = GetDetails<decimal>(i, DetailsField.Quantity),
                PieceUnit = GetString(i, DetailsField.Unit),
                UnitCost = GetDetails<decimal>(i, DetailsField.Unit_Price),
                Remark = GetString(i, DetailsField.Remark),
                TaxType = processType == Naming.InvoiceProcessType.C0401_Xlsx_CBE
                            ? (byte)Naming.TaxTypeDefinition.應稅
                            : GetDetails<byte>(i, DetailsField.Item_Tax_Type),
                No = (seqNo++)
            }).ToList();


            foreach (var product in _productItems)
            {
                if (String.IsNullOrEmpty(product.InvoiceProduct.Brief) || product.InvoiceProduct.Brief.Length > 256)
                {
                    return new Exception(String.Format(MessageResources.InvalidProductDescription, product.InvoiceProduct.Brief));
                }


                if (!String.IsNullOrEmpty(product.PieceUnit) && product.PieceUnit.Length > 6)
                {
                    return new Exception(String.Format(MessageResources.InvalidPieceUnit, product.PieceUnit));
                }


                //if (!Regex.IsMatch(product.UnitCost.ToString(), __DECIMAL_AMOUNT_PATTERN))
                //{
                //    return new Exception(String.Format(MessageResources.InvalidUnitPrice, product.UnitCost));
                //}

                //if (!Regex.IsMatch(product.CostAmount.ToString(), __DECIMAL_AMOUNT_PATTERN))
                //{
                //    return new Exception(String.Format(MessageResources.InvalidCostAmount, product.CostAmount));
                //}

                //if (product.CostAmount.HasValue && product.UnitCost.HasValue && product.Piece.HasValue)
                //{
                //    if (product.CostAmount != product.UnitCost * product.Piece)
                //    {
                //        return new Exception(MessageResources.InvalidProductAmount);
                //    }
                //}
            }
            return null;
        }

        String printMark;
        protected override Exception checkMandatoryFields()
        {

            if (BuyerID() == "0000000000" && DonateMark() != "0" && DonateMark() != "1")
            {
                return new Exception(String.Format(MessageResources.InvalidDonationMark, DonateMark()));
            }

            if ((printMark = PrintMark().GetEfficientString()) == null)
            {
                //return new Exception(MessageResources.InvalidPrintMark);
                printMark = "N";
            }
            else
            {
                printMark = PrintMark().ToUpper();
                if (printMark != "Y" && printMark != "N")
                {
                    return new Exception(MessageResources.InvalidPrintMark);
                }
            }

            if (!InvoiceType().IsValidInvoiceType())
            {
                return new Exception(String.Format(MessageResources.InvalidInvoiceType, InvoiceType()));
            }


            return null;
        }

        protected override Exception checkCarrierDataIsComplete()
        {

            if (String.IsNullOrEmpty(CarrierType()))
            {
                return new Exception(MessageResources.AlertInvoiceCarrierComplete);
            }
            else
            {
                if (CarrierType().Length > 6 || (CarrierId1() != null && CarrierId1().Length > 64) || (CarrierId2() != null && CarrierId2().Length > 64))
                    return new Exception(String.Format(MessageResources.AlertInvoiceCarrierLength, CarrierType(), CarrierId1(), CarrierId2()));

                _carrier = new InvoiceCarrier
                {
                    CarrierType = CarrierType()
                };

                if (!String.IsNullOrEmpty(CarrierId1()))
                {
                    if (CarrierId1().Length > 64)
                        return new Exception(String.Format(MessageResources.AlertInvoiceCarrierLength, CarrierType(), CarrierId1(), CarrierId2()));

                    _carrier.CarrierNo = CarrierId1();
                }

                if (!String.IsNullOrEmpty(CarrierId2()))
                {
                    if (CarrierId2().Length > 64)
                        return new Exception(String.Format(MessageResources.AlertInvoiceCarrierLength, CarrierType(), CarrierId1(), CarrierId2()));

                    _carrier.CarrierNo2 = CarrierId2();
                }

                if (_carrier.CarrierNo == null)
                {
                    if (_carrier.CarrierNo2 == null)
                    {
                        return new Exception(MessageResources.AlertInvoiceCarrierComplete);
                    }
                    else
                    {
                        _carrier.CarrierNo = _carrier.CarrierNo2;
                    }
                }
                else
                {
                    if (_carrier.CarrierNo2 == null)
                        _carrier.CarrierNo2 = _carrier.CarrierNo;
                }
            }

            return null;
        }

    }
}
