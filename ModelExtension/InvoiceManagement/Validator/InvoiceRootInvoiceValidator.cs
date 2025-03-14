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
using ModelCore.InvoiceManagement.InvoiceProcess;
using CommonLib.DataAccess;

namespace ModelCore.InvoiceManagement.Validator
{
    public partial class InvoiceRootInvoiceValidator
    {
        public static string __DECIMAL_AMOUNT_PATTERN = "^-?\\d{1,12}(.[0-9]{0,4})?$";
        public static string __CELLPHONE_BARCODE = "3J0002";
        public static String __自然人憑證 = "CQ0001";
        public static String __CROSS_BORDER_MURCHANT = "5G0001";
        public static readonly Regex __MatchCellPhoneBarcode = new Regex("^/((?<!,)[A-Z0-9+-\\.](?!,)){7}$");
        public static readonly String[] __InvoiceDateTimeFormat =
            {
                "yyyy/MM/dd HH:mm:ss",
                "yyyy-MM-dd HH:mm:ss",
                "yyyyMMdd HH:mm:ss",
                "yyyy/M/d H:m:s",
                "yyyy-M-d H:m:s",
            };

        protected GenericManager<EIVOEntityDataContext> _models;
        protected Organization _owner;

        protected InvoiceRootInvoice _invItem;

        protected InvoiceItem _newItem;
        protected InvoiceItem _container;
        protected Organization _seller;
        protected bool _isCrossBorderMerchant;
        protected InvoicePurchaseOrder _order;
        protected InvoicePurchaseOrderAudit _orderAudit;
        protected InvoiceBuyer _buyer;
        protected InvoiceCarrier _carrier;
        protected InvoiceDonation _donation;
        protected CurrencyType _currency;
        protected IEnumerable<InvoiceProductItem> _productItems;

        protected bool _isAutoTrackNo;
        protected DateTime? _autoTrackNoInvoiceDate;
        protected Dictionary<int, TrackNoManager> _trackNoManagerList;
        protected Func<Exception>[,,,] _deliveryCheck;
        protected Naming.InvoiceProcessType? processType;


        public InvoiceRootInvoiceValidator(GenericManager<EIVOEntityDataContext> mgr, Organization owner)
        {
            _models = mgr;
            _owner = owner;

            initializeDeliveryCheck();
        }

        public GenericManager<EIVOEntityDataContext> DataSource => _models;

        public bool UseDefaultCrossBorderMerchantCarrier { get; set; } = false;

        public Naming.InvoiceProcessType? ProcessType
        {
            get => processType;
            set => processType = value;
        }

        private void initializeDeliveryCheck()
        {
            _deliveryCheck = new Func<Exception>[2, 2, 2, 2];

            #region 列印Y

            _deliveryCheck[(int)PrintedMark.Yes, (int)CarrierIntent.Yes, (int)IsB2C.Yes, (int)DonationIntent.Yes] = () =>
                {
                    return new Exception(String.Format(MessageResources.AlertPrintedInvoiceCarrierType, _invItem.CarrierType));
                };

            _deliveryCheck[(int)PrintedMark.Yes, (int)CarrierIntent.Yes, (int)IsB2C.Yes, (int)DonationIntent.No] =
                _deliveryCheck[(int)PrintedMark.Yes, (int)CarrierIntent.Yes, (int)IsB2C.Yes, (int)DonationIntent.Yes];

            _deliveryCheck[(int)PrintedMark.Yes, (int)CarrierIntent.Yes, (int)IsB2C.No, (int)DonationIntent.Yes] = () =>
            {
                return new Exception(String.Format(MessageResources.AlertDonationInvoiceCarryType, _invItem.CarrierType));
            };

            _deliveryCheck[(int)PrintedMark.Yes, (int)CarrierIntent.Yes, (int)IsB2C.No, (int)DonationIntent.No] = checkPublicCarrier;

            _deliveryCheck[(int)PrintedMark.Yes, (int)CarrierIntent.No, (int)IsB2C.Yes, (int)DonationIntent.Yes] = () =>
            {
                return new Exception(String.Format(MessageResources.AlertPrintedInvoiceDonation, _invItem.PrintMark));
            };

            _deliveryCheck[(int)PrintedMark.Yes, (int)CarrierIntent.No, (int)IsB2C.No, (int)DonationIntent.Yes] = () =>
            {
                return new Exception(String.Format(MessageResources.AlertDonationInvoiceCarryType, _invItem.CarrierType));
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

                    if (String.IsNullOrEmpty(_invItem.NPOBAN))
                    {
                        return new Exception(String.Format(MessageResources.InvalidDonationTaker, _invItem.NPOBAN));
                    }
                    else
                    {
                        if (_invItem.NPOBAN.Length < 3 || _invItem.NPOBAN.Length > 7)
                            return new Exception(String.Format(MessageResources.InvalidDonationTaker, _invItem.NPOBAN));


                    }

                    _donation = new InvoiceDonation
                    {
                        AgencyCode = _invItem.NPOBAN
                    };

                    return null;
                };

            _deliveryCheck[(int)PrintedMark.No, (int)CarrierIntent.Yes, (int)IsB2C.Yes, (int)DonationIntent.No] = () =>
                {
                    Exception ex = usePublicCarrier(false);
                    if (ex != null)
                        return ex;

                    ex = checkCarrierDataIsComplete();
                    if (ex != null)
                        return ex;

                    //if (_invItem.CarrierType == __CELLPHONE_BARCODE)
                    //{
                    //    ex = checkPublicCarrier();
                    //    if (ex != null)
                    //        return ex;
                    //}

                    //_carrier = new InvoiceCarrier
                    //{
                    //    CarrierType = _invItem.CarrierType,
                    //    CarrierNo = String.IsNullOrEmpty(_invItem.CarrierId1) ? _invItem.CarrierId2 : _invItem.CarrierId1
                    //};

                    //if (String.IsNullOrEmpty(_invItem.CarrierId2))
                    //{
                    //    _carrier.CarrierNo2 = _carrier.CarrierNo;
                    //}
                    //else
                    //{
                    //    _carrier.CarrierNo2 = _invItem.CarrierId2;
                    //}

                    return null;
                };

            _deliveryCheck[(int)PrintedMark.No, (int)CarrierIntent.Yes, (int)IsB2C.No, (int)DonationIntent.Yes] = checkPublicCarrier;
            _deliveryCheck[(int)PrintedMark.No, (int)CarrierIntent.Yes, (int)IsB2C.No, (int)DonationIntent.No] = checkPublicCarrier;

            _deliveryCheck[(int)PrintedMark.No, (int)CarrierIntent.No, (int)IsB2C.Yes, (int)DonationIntent.Yes] = () =>
                {
                    if (String.IsNullOrEmpty(_invItem.NPOBAN))
                    {
                        return new Exception(String.Format(MessageResources.InvalidDonationTaker, _invItem.NPOBAN));
                    }
                    else
                    {
                        if (_invItem.NPOBAN.Length < 3 || _invItem.NPOBAN.Length > 7)
                            return new Exception(String.Format(MessageResources.InvalidDonationTaker, _invItem.NPOBAN));


                    }
                    _donation = new InvoiceDonation
                    {
                        AgencyCode = _invItem.NPOBAN
                    };

                    return null;

                };

            _deliveryCheck[(int)PrintedMark.No, (int)CarrierIntent.No, (int)IsB2C.Yes, (int)DonationIntent.No] = checkPrintAll;

            _deliveryCheck[(int)PrintedMark.No, (int)CarrierIntent.No, (int)IsB2C.No, (int)DonationIntent.Yes] = () => { return null; };
            _deliveryCheck[(int)PrintedMark.No, (int)CarrierIntent.No, (int)IsB2C.No, (int)DonationIntent.No] = checkPrintAll;

            #endregion

        }

        public Naming.InvoiceTypeDefinition InvoiceTypeIndication { get; set; } = Naming.InvoiceTypeDefinition.一般稅額計算之電子發票;

        public InvoiceItem InvoiceItem
        {
            get
            {
                return _newItem;
            }
        }

        public bool DuplicateProcess
        {
            get;
            protected set;
        }

        public virtual void StartAutoTrackNo(DateTime? autoTrackNoInvoiceDate = null)
        {
            _trackNoManagerList = new Dictionary<int, TrackNoManager>();
            _isAutoTrackNo = true;
            _autoTrackNoInvoiceDate = autoTrackNoInvoiceDate;
        }

        public virtual void EndAutoTrackNo()
        {
            _isAutoTrackNo = false;
            _autoTrackNoInvoiceDate = null;
            if (_trackNoManagerList != null)
            {
                foreach (var item in _trackNoManagerList)
                {
                    item.Value.Close();
                    item.Value.Dispose();
                }
                _trackNoManagerList.Clear();
            }
        }


        public virtual Exception Validate(InvoiceRootInvoice dataItem)
        {
            _invItem = dataItem;

            Exception ex;

            //_seller = null;
            _newItem = null;
            _container = new InvoiceItem { };

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
                appendDataNumber();
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

        protected virtual Exception checkInvoiceNo(InvoiceItem item, DateTime invoiceDate, String invoiceNo)
        {
            item.TrackCode = invoiceNo.Substring(0, 2);
            item.No = invoiceNo.Substring(2);

            if (_seller.OrganizationStatus?.EnableTrackCodeInvoiceNoValidation == true)
            {
                int periodNo = (invoiceDate.Month + 1) / 2;

                var trackCode = _models.GetTable<InvoiceTrackCode>()
                    .Where(t => t.Year == invoiceDate.Year && t.PeriodNo == periodNo && t.TrackCode == item.TrackCode)
                    .FirstOrDefault();

                if (trackCode == null)
                {
                    return new Exception(String.Format(MessageResources.InvalidTrackCode, invoiceNo));
                }

                item.TrackID = trackCode.TrackID;

                int no = int.Parse(item.No);
                var interval = _models.GetTable<InvoiceNoInterval>()
                    .Where(t => t.SellerID == _seller.CompanyID && t.TrackID == trackCode.TrackID && no >= t.StartNo && no <= t.EndNo)
                    .FirstOrDefault();

                if (interval != null)
                {
                    item.InvoiceNoAssignment = new InvoiceNoAssignment
                    {
                        IntervalID = interval.IntervalID,
                        InvoiceNo = no,
                    };
                }
            }

            return null;
        }

        protected virtual Exception checkInvoice()
        {
            DuplicateProcess = false;
            _container.CDS_Document = new CDS_Document
            {
                DocDate = DateTime.Now,
                DocType = (int)Naming.DocumentTypeDefinition.E_Invoice,
                DocumentOwner = new DocumentOwner
                {
                    OwnerID = _owner.CompanyID
                },
                ProcessType = (int)(processType ?? Naming.InvoiceProcessType.C0401),
            };
            _container.DonateMark = _donation == null ? "0" : "1";
            _container.SellerID = _seller.CompanyID;
            _container.CustomsClearanceMark = _invItem.CustomsClearanceMark;
            _container.InvoiceSeller = new InvoiceSeller
            {
                Name = _seller.CompanyName,
                ReceiptNo = _seller.ReceiptNo,
                Address = _seller.Addr,
                ContactName = _seller.ContactName,
                //CustomerID = String.IsNullOrEmpty(_invItem.GoogleId) ? "" : _invItem.GoogleId,
                CustomerName = _seller.CompanyName,
                EMail = _seller.ContactEmail,
                Fax = _seller.Fax,
                Phone = _seller.Phone,
                PersonInCharge = _seller.UndertakerName,
                SellerID = _seller.CompanyID,
            };
            _container.InvoiceBuyer = _buyer;
            _container.RandomNo = _invItem.RandomNumber;
            _container.InvoiceAmountType = new InvoiceAmountType
            {
                DiscountAmount = _invItem.DiscountAmount,
                SalesAmount = _invItem.SalesAmount,
                FreeTaxSalesAmount = _invItem.FreeTaxSalesAmount,
                ZeroTaxSalesAmount = _invItem.ZeroTaxSalesAmount,
                TaxAmount = _invItem.TaxAmount,
                TaxRate = _invItem.TaxRate,
                TaxType = _invItem.TaxType,
                TotalAmount = _invItem.TotalAmount,
                TotalAmountInChinese = ValidityAgent.MoneyShow(_invItem.TotalAmount),
                CurrencyID = _currency?.CurrencyID,
                BondedAreaConfirm = _invItem.BondedAreaConfirm,
            };
            _container.InvoiceCarrier = _carrier;
            _container.InvoiceDonation = _donation;
            _container.PrintMark = _invItem.PrintMark;
            _container.Remark = _invItem.MainRemark;
            if (_invItem.CustomerDefined != null)
            {
                if (_container.CDS_Document.CustomerDefined == null)
                {
                    _container.CDS_Document.CustomerDefined = new CustomerDefined { };
                }
                _container.CDS_Document.CustomerDefined.DataContent = _invItem.CustomerDefined.ConvertToXml().OuterXml;
            }

            if (_order != null)
            {
                _container.InvoicePurchaseOrder = _order;
            }

            if (_orderAudit != null)
            {
                _orderAudit.InvoiceItem = _container;
            }

            _container.InvoiceDetails.AddRange(_productItems.Select(p => new InvoiceDetail
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

                    if (!trackNoMgr.CheckInvoiceNo(_container))
                    {
                        return new Exception(String.Format(MessageResources.AlertNullTrackNoInterval, _seller.ReceiptNo));
                    }
                    else
                    {
                        if (_autoTrackNoInvoiceDate.HasValue)
                        {
                            _container.InvoiceDate = _autoTrackNoInvoiceDate.Value.Add(DateTime.Now.TimeOfDay);
                        }
                        else
                        {
                            _container.InvoiceDate = DateTime.Now;
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
                DateTime invoiceDate = DateTime.Now;

                if (String.IsNullOrEmpty(_invItem.InvoiceDate))
                {
                    return new Exception(MessageResources.AlertInvoiceDate);
                }

                _invItem.InvoiceTime = _invItem.InvoiceTime.GetEfficientString();
                if (_invItem.InvoiceTime == null)
                {
                    _invItem.InvoiceTime = "12:00:00";
                }
                //if (String.IsNullOrEmpty(_invItem.InvoiceTime))
                //{
                //    return new Exception(MessageResources.AlertInvoiceTime);
                //}

                if (!DateTime.TryParseExact($"{_invItem.InvoiceDate} {_invItem.InvoiceTime}", __InvoiceDateTimeFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out invoiceDate)
                        || invoiceDate > DateTime.Today.AddDays(3))
                {
                    return new Exception(String.Format(MessageResources.AlertInvoiceDateTime, _invItem.InvoiceDate, _invItem.InvoiceTime));
                }

                _container.InvoiceDate = invoiceDate;
                DateTime periodStart = new DateTime(invoiceDate.Year, (invoiceDate.Month - 1) / 2 * 2 + 1, 1);


                if (_invItem.InvoiceNumber == null || !Regex.IsMatch(_invItem.InvoiceNumber, "^[a-zA-Z]{2}[0-9]{8}$"))
                {
                    return new Exception(String.Format(MessageResources.AlertInvoiceNumber, _invItem.InvoiceNumber));
                }

                var ex = checkInvoiceNo(_container, invoiceDate, _invItem.InvoiceNumber);
                if (ex != null)
                {
                    return ex;
                }

                var currentItem = _models.GetTable<InvoiceItem>().Where(i => i.TrackCode == _container.TrackCode && i.No == _container.No
                            && i.InvoiceDate >= periodStart && i.InvoiceDate < periodStart.AddMonths(2)).FirstOrDefault();

                if (currentItem != null)
                {
                    if (currentItem.SellerID == _seller.CompanyID && (currentItem.RandomNo == _invItem.RandomNumber || _seller.IgnoreDuplicatedNo()))
                    {
                        DuplicateProcess = true;
                        _newItem = currentItem;
                        return null;
                    }
                    else
                    {
                        return new Exception(MessageResources.AlertInvoiceDuplicated);
                    }
                }

                if (_seller.OrganizationStatus?.EnableTrackCodeInvoiceNoValidation == true)
                {
                    TrackNoManager trackMgr = new TrackNoManager(_models, _seller.CompanyID);
                    var item = trackMgr.GetAppliedInterval(invoiceDate, _container.TrackCode, int.Parse(_container.No));

                    if (item == null)
                    {
                        return new Exception(String.Format("發票號碼錯誤:{0}，TAG:< InvoicNumber />", _invItem.InvoiceNumber));
                    }
                }
            }

            if (_invItem.CustomerDefined != null)
            {
                _container.InvoiceItemExtension = new InvoiceItemExtension
                {
                    ProjectNo = _invItem.CustomerDefined.ProjectNo,
                    PurchaseNo = _invItem.CustomerDefined.PurchaseNo
                };

                if (_invItem.CustomerDefined.StampDutyFlagSpecified)
                {
                    _container.InvoiceItemExtension.StampDutyFlag = (byte?)_invItem.CustomerDefined.StampDutyFlag;
                }
            }

            _newItem = _container;

            return null;
        }

        protected virtual Exception checkDataNumber()
        {
            _order = null;
            _orderAudit = null;
            if (String.IsNullOrEmpty(_invItem.DataNumber))
            {
                return new Exception(MessageResources.AlertDataNumber);
            }

            if (_invItem.DataNumber?.Length > 60)
            {
                return new Exception(String.Format(MessageResources.AlertDataNumberLimitedLength, _invItem.DataNumber));
            }

            if (_seller.ForcedAuditNo())
            {
                _orderAudit = _models.CreateInvoicePurchaseOrderAudit(_seller.CompanyID, _invItem.DataNumber);
                if (_orderAudit == null)
                {
                    return new Exception(String.Format(MessageResources.AlertDataNumberDuplicated, _invItem.DataNumber));
                }
            }

            var po = _models.GetTable<InvoicePurchaseOrder>().Where(d => d.OrderNo == _invItem.DataNumber
                    && d.InvoiceItem.SellerID == _seller.CompanyID).FirstOrDefault();
            if (po != null)
            {
                return new DuplicateDataNumberException(String.Format(MessageResources.AlertDataNumberDuplicated, _invItem.DataNumber))
                {
                    CurrentPO = po
                };
            }

            if (String.IsNullOrEmpty(_invItem.DataDate))
            {
                return new Exception(MessageResources.AlertDataDate);
            }

            DateTime dataDate;
            if (!DateTime.TryParseExact(_invItem.DataDate, "yyyy/MM/dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out dataDate))
            {
                return new Exception(String.Format(MessageResources.AlertDataDateFormat, _invItem.DataDate));
            }

            _order = new InvoicePurchaseOrder
            {
                OrderNo = _invItem.DataNumber,
                PurchaseDate = dataDate
            };

            return null;
        }

        protected virtual void appendDataNumber()
        {
            _order = null;
            if (String.IsNullOrEmpty(_invItem.DataNumber))
            {
                return;
            }

            if (_invItem.DataNumber?.Length > 60)
            {
                return;
            }

            var po = _models.GetTable<InvoicePurchaseOrder>().Where(d => d.OrderNo == _invItem.DataNumber
                    && d.InvoiceItem.SellerID == _seller.CompanyID).FirstOrDefault();
            if (po != null)
            {
                return;
            }

            if (String.IsNullOrEmpty(_invItem.DataDate))
            {
                return;
            }

            DateTime dataDate;
            if (!DateTime.TryParse(_invItem.DataDate, out dataDate))
            {
                return;
            }

            _order = new InvoicePurchaseOrder
            {
                OrderNo = _invItem.DataNumber,
                PurchaseDate = dataDate
            };
        }



        protected virtual Exception checkBusiness()
        {
            if (_seller == null || _seller.ReceiptNo != _invItem.SellerId)
            {

                _seller = _models.GetTable<Organization>().Where(o => o.ReceiptNo == _invItem.SellerId).FirstOrDefault();
                if (_seller == null)
                {
                    return new Exception(String.Format(MessageResources.AlertInvalidSeller, _invItem.SellerId));
                }

                if (_seller.CompanyID != _owner.CompanyID && !_models.GetTable<InvoiceIssuerAgent>().Any(a => a.AgentID == _owner.CompanyID && a.IssuerID == _seller.CompanyID))
                {
                    //return new Exception(String.Format(MessageResources.AlertSellerSignature, _invItem.SellerId));
                    return new Exception(String.Format(MessageResources.InvalidSellerOrAgent, _invItem.SellerId, _owner.ReceiptNo));
                }

                if (_seller.OrganizationStatus.CurrentLevel == (int)Naming.MemberStatusDefinition.Mark_To_Delete)
                {
                    return new Exception(String.Format("開立人已註記停用,開立人統一編號:{0}，TAG:<SellerId />", _invItem.SellerId));
                }

                _isCrossBorderMerchant = _models.GetTable<OrganizationCategory>().Any(c => c.CompanyID == _seller.CompanyID && c.CategoryID == (int)Naming.CategoryID.COMP_CROSS_BORDER_MURCHANT);

            }

            _invItem.BuyerId = _invItem.BuyerId.GetEfficientString();
            if (_invItem.BuyerId == "0000000000")
            {
                //if (_invItem.BuyerName == null || Encoding.GetEncoding(950).GetBytes(_invItem.BuyerName).Length != 4)
                //{
                //    return new Exception(String.Format(MessageResources.InvalidBuyerName, _invItem.BuyerName));
                //}
            }
            else if (_invItem.BuyerId == null)
            {
                _invItem.BuyerId = "0000000000";

                //if (_isCrossBorderMerchant)
                //{
                //    _invItem.BuyerId = "0000000000";
                //}
                //else
                //{
                //    return new Exception(String.Format(MessageResources.InvalidBuyerId, _invItem.BuyerId));
                //}
            }
            else if (!Regex.IsMatch(_invItem.BuyerId, "^[0-9]{8}$"))
            {
                return new Exception(String.Format(MessageResources.InvalidBuyerId, _invItem.BuyerId));
            }
            else if (_seller.OrganizationStatus.EnableBuyerIDValidation != false && !_invItem.BuyerId.CheckRegno())
            {
                return new Exception(String.Format(MessageResources.InvalidReceiptNo, _invItem.BuyerId));
            }
            else if (_invItem.PrintMark != "Y" && (String.IsNullOrEmpty(_invItem.BuyerName) || _invItem.BuyerName.Length > 60))
            {
                return new Exception(String.Format(MessageResources.InvalidBuyerNameLengthLimit, _invItem.BuyerName));
            }

            if (processType == Naming.InvoiceProcessType.A0401 || processType == Naming.InvoiceProcessType.A0101)
            {
                if (_invItem.BuyerId == "0000000000")
                {
                    return new Exception(String.Format(MessageResources.InvalidBuyerId, _invItem.BuyerId));
                }
            }

            if (String.IsNullOrEmpty(_invItem.RandomNumber))
            {
                _invItem.RandomNumber = String.Format("{0:0000}", DateTime.Now.Ticks % 10000); //ValidityAgent.GenerateRandomCode(4)
            }
            else if (!Regex.IsMatch(_invItem.RandomNumber, "^[0-9]{4}$"))
            {
                return new Exception(String.Format(MessageResources.InvalidRandomNumber, _invItem.RandomNumber));
            }

            return checkBusinessDetails();
        }

        protected bool checkPublicCarrierId(String carrierId)
        {
            return carrierId?.Length == 8 && __MatchCellPhoneBarcode.IsMatch(carrierId);
        }

        protected virtual Exception checkPublicCarrier()
        {
            return usePublicCarrier(true);
        }

        protected virtual Exception usePublicCarrier(bool forced)
        {
            if (_invItem.CarrierType == __CELLPHONE_BARCODE)
            {
                if (checkPublicCarrierId(_invItem.CarrierId1))
                {
                    _carrier = new InvoiceCarrier
                    {
                        CarrierType = _invItem.CarrierType,
                        CarrierNo = _invItem.CarrierId1,
                        CarrierNo2 = _invItem.CarrierId1
                    };

                    return null;
                }
                else if (checkPublicCarrierId(_invItem.CarrierId2))
                {
                    _carrier = new InvoiceCarrier
                    {
                        CarrierType = _invItem.CarrierType,
                        CarrierNo = _invItem.CarrierId2,
                        CarrierNo2 = _invItem.CarrierId2
                    };

                    return null;
                }
                else
                {
                    return new Exception(String.Format(MessageResources.InvalidPublicCarrierType, _invItem.CarrierType, _invItem.CarrierId1, _invItem.CarrierId2));
                }
            }
            else if (_invItem.CarrierType == __自然人憑證)
            {
                if (_invItem.CarrierId1 != null && Regex.IsMatch(_invItem.CarrierId1, "^[A-Z]{2}[0-9]{14}$"))
                {
                    _carrier = new InvoiceCarrier
                    {
                        CarrierType = _invItem.CarrierType,
                        CarrierNo = _invItem.CarrierId1,
                        CarrierNo2 = _invItem.CarrierId1
                    };

                    return null;
                }
                else if (_invItem.CarrierId2 != null && Regex.IsMatch(_invItem.CarrierId2, "^[A-Z]{2}[0-9]{14}$"))
                {
                    _carrier = new InvoiceCarrier
                    {
                        CarrierType = _invItem.CarrierType,
                        CarrierNo = _invItem.CarrierId2,
                        CarrierNo2 = _invItem.CarrierId2
                    };

                    return null;
                }
                else
                {
                    return new Exception(String.Format(MessageResources.InvalidPublicCarrierType, _invItem.CarrierType, _invItem.CarrierId1, _invItem.CarrierId2));
                }
            }
            else if (_invItem.CarrierType == __CROSS_BORDER_MURCHANT)
            {
                if (_invItem.CarrierId1 != null)
                {
                    _carrier = new InvoiceCarrier
                    {
                        CarrierType = _invItem.CarrierType,
                        CarrierNo = _invItem.CarrierId1,
                        CarrierNo2 = _invItem.CarrierId1
                    };

                    return null;
                }
                else if (_invItem.CarrierId2 != null)
                {
                    _carrier = new InvoiceCarrier
                    {
                        CarrierType = _invItem.CarrierType,
                        CarrierNo = _invItem.CarrierId2,
                        CarrierNo2 = _invItem.CarrierId2
                    };

                    return null;
                }
                else
                {
                    return new Exception(String.Format(MessageResources.InvalidPublicCarrierType, _invItem.CarrierType, _invItem.CarrierId1, _invItem.CarrierId2));
                }
            }
            else
            {
                if (forced)
                {
                    return new Exception(String.Format(MessageResources.InvalidPublicCarrierType, _invItem.CarrierType, _invItem.CarrierId1, _invItem.CarrierId2));
                }
                else
                {
                    return null;
                }
            }

        }


        protected virtual Exception checkPrintAll()
        {

            if (_seller.OrganizationStatus.PrintAll == true)
            {
                //全列印
                //if (_invItem.Contact == null)
                //{
                //    return new Exception(MessageResources.AlertContactToPrintAll);
                //}

                _invItem.PrintMark = "Y";

            }
            else
            {
                if (UseDefaultCrossBorderMerchantCarrier && _isCrossBorderMerchant)
                {
                    var carrierID = _invItem.Contact?.Email?.GetEfficientString() ?? _invItem.CarrierId1?.GetEfficientString();
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

        protected virtual Exception checkBusinessDetails()
        {
            _buyer = new InvoiceBuyer
            {
                BuyerMark = _invItem.BuyerMark,
                Name = _invItem.BuyerId == "0000000000" ? _invItem.BuyerName.CheckB2CMIGName() : _invItem.BuyerName,
                ReceiptNo = _invItem.BuyerId,
                CustomerID = _invItem.GoogleId.GetEfficientString() ?? _invItem.CustomerID.GetEfficientString(),
                CustomerName = _invItem.BuyerName,
            };

            if (_isCrossBorderMerchant)
            {
                _buyer.CustomerNumber = _buyer.ReceiptNo;
                _buyer.ReceiptNo = "0000000000";
                _buyer.EMail = _invItem.CarrierId1 ?? _invItem.CarrierId2;
            }

            BusinessRelationship relationship = null;
            if (processType == Naming.InvoiceProcessType.A0401 || processType == Naming.InvoiceProcessType.A0101)
            {
                relationship = _models.GetTable<BusinessRelationship>()
                                    .Where(r => r.MasterID == _seller.CompanyID)
                                    .Join(_models.GetTable<Organization>().Where(b => b.ReceiptNo == _buyer.ReceiptNo),
                                        r => r.RelativeID, b => b.CompanyID, (r, b) => r).FirstOrDefault();
                if (relationship != null)
                {
                    _buyer.BuyerID = relationship.RelativeID;
                }
            }

            if (_invItem.Contact != null)
            {

                if (!String.IsNullOrEmpty(_invItem.Contact.Name) && _invItem.Contact.Name.Length > 64)
                {
                    return new Exception(String.Format(MessageResources.InvalidContactName, _invItem.Contact.Name));
                }

                if (!String.IsNullOrEmpty(_invItem.Contact.Address) && _invItem.Contact.Address.Length > 128)
                {
                    return new Exception(String.Format(MessageResources.InvalidContactAddress, _invItem.Contact.Address));
                }

                if (!String.IsNullOrEmpty(_invItem.Contact.Email) && _invItem.Contact.Email.Length > 512)
                {
                    return new Exception(String.Format(MessageResources.InvalidContactEMail, _invItem.Contact.Email));
                }

                if (!String.IsNullOrEmpty(_invItem.Contact.TEL) && _invItem.Contact.TEL.Length > 64)
                {
                    return new Exception(String.Format(MessageResources.InvalidContactPhone, _invItem.Contact.TEL));
                }

                _buyer.ContactName = _invItem.Contact.Name;
                _buyer.Address = _invItem.Contact.Address;
                _buyer.Phone = _invItem.Contact.TEL;
                _buyer.EMail = _invItem.Contact.Email != null ? _invItem.Contact.Email.Replace(';', ',').Replace('、', ',').Replace(' ', ',') : null;
            }
            else if (relationship != null)
            {
                _buyer.ContactName = relationship.CompanyName;
                _buyer.Address = relationship.Addr;
                _buyer.Phone = relationship.Phone;
                _buyer.EMail = relationship.ContactEmail;
            }

            return null;
        }

        protected virtual Exception checkInvoiceDelivery()
        {

            _carrier = null;
            _donation = null;

            var checkFunc = _deliveryCheck[Convert.ToInt32(_invItem.PrintMark == "Y"),
                Convert.ToInt32(!String.IsNullOrEmpty(_invItem.CarrierType)
                    && !(String.IsNullOrEmpty(_invItem.CarrierId1) && String.IsNullOrEmpty(_invItem.CarrierId2))),
                Convert.ToInt32(_invItem.BuyerId == "0000000000"),
                Convert.ToInt32(_invItem.DonateMark == "1")];

            return checkFunc();

        }


        protected virtual Exception checkAmount()
        {
            //應稅銷售額
            if (_invItem.SalesAmount < 0 /*|| decimal.Floor(_invItem.SalesAmount) != _invItem.SalesAmount*/)
            {
                return new Exception(String.Format(MessageResources.InvalidSellingPrice, _invItem.SalesAmount));
            }

            if (_invItem.FreeTaxSalesAmount < 0 /*|| decimal.Floor(_invItem.FreeTaxSalesAmount) != _invItem.FreeTaxSalesAmount*/)
            {
                return new Exception(String.Format(MessageResources.InvalidFreeTaxAmount, _invItem.FreeTaxSalesAmount));
            }

            if (_invItem.ZeroTaxSalesAmount < 0 /*|| decimal.Floor(_invItem.ZeroTaxSalesAmount) != _invItem.ZeroTaxSalesAmount*/)
            {
                return new Exception(String.Format(MessageResources.InvalidZeroTaxAmount, _invItem.ZeroTaxSalesAmount));
            }


            if (_invItem.TaxAmount < 0 /*|| decimal.Floor(_invItem.TaxAmount) != _invItem.TaxAmount*/)
            {
                return new Exception(String.Format(MessageResources.InvalidTaxAmount, _invItem.TaxAmount));
            }

            if (_invItem.TotalAmount < 0 /*|| decimal.Floor(_invItem.TotalAmount) != _invItem.TotalAmount*/)
            {
                return new Exception(String.Format(MessageResources.InvalidTotalAmount, _invItem.TotalAmount));
            }

            //課稅別
            if (!Enum.IsDefined(typeof(Naming.TaxTypeDefinition), (int)_invItem.TaxType))
            {
                return new Exception(String.Format(MessageResources.InvalidTaxType, _invItem.TaxType));
            }

            if (_invItem.TaxRate < 0m)
            {
                return new Exception(String.Format(MessageResources.InvalidTaxRate, _invItem.TaxRate));
            }

            if (_invItem.TaxType == (byte)Naming.TaxTypeDefinition.零稅率)
            {
                if (!_invItem.CustomsClearanceMark.HasValue)
                {
                    return new Exception(String.Format(MessageResources.AlertClearanceMarkZeroTax, _invItem.CustomsClearanceMark));
                }
                else if (_invItem.CustomsClearanceMark != 1 && _invItem.CustomsClearanceMark != 2)
                {
                    return new Exception(String.Format(MessageResources.AlertClearanceMarkExport, _invItem.CustomsClearanceMark));
                }
            }
            else if (_invItem.CustomsClearanceMark.HasValue)
            {
                if (_invItem.CustomsClearanceMark != 1 && _invItem.CustomsClearanceMark != 2)
                {
                    return new Exception(String.Format(MessageResources.AlertClearanceMarkExport, _invItem.CustomsClearanceMark));
                }
            }

            _invItem.Currency = _invItem.Currency.GetEfficientString();
            _currency = null;
            if (!String.IsNullOrEmpty(_invItem.Currency))
            {
                _currency = _models.GetTable<CurrencyType>().Where(c => c.AbbrevName == _invItem.Currency).FirstOrDefault();
                if (_currency == null)
                {
                    return new Exception($"Invalid currency code：{_invItem.Currency}，TAG：<Currency/>");
                }
            }

            return null;
        }


        protected virtual Exception checkInvoiceProductItems()
        {
            if (_invItem.InvoiceItem == null || _invItem.InvoiceItem.Length == 0)
            {
                return new Exception(MessageResources.InvalidInvoiceDetails);
            }

            short seqNo = 1;
            _productItems = _invItem.InvoiceItem.Select(i => new InvoiceProductItem
            {
                InvoiceProduct = new InvoiceProduct { Brief = i.Description },
                CostAmount = i.Amount,
                ItemNo = i.Item,
                Piece = i.Quantity,
                PieceUnit = i.Unit,
                UnitCost = i.UnitPrice,
                Remark = i.Remark,
                TaxType = i.TaxType,
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


                //if (!product.UnitCost.HasValue || product.UnitCost == 0)
                //{
                //    return new Exception(String.Format(MessageResources.InvalidUnitPrice, product.UnitCost));
                //}

                //if (!product.CostAmount.HasValue || product.CostAmount == 0)
                //{
                //    return new Exception(String.Format(MessageResources.InvalidCostAmount, product.CostAmount));
                //}

                //if (!product.Piece.HasValue || product.Piece == 0)
                //{
                //    return new Exception(String.Format(MessageResources.InvalidQuantity, product.Piece));
                //}

            }
            return null;
        }

        protected virtual Exception checkMandatoryFields()
        {

            if (_invItem.BuyerId == "0000000000" && _invItem.DonateMark != "0" && _invItem.DonateMark != "1")
            {
                return new Exception(String.Format(MessageResources.InvalidDonationMark, _invItem.DonateMark));
            }

            if (String.IsNullOrEmpty(_invItem.PrintMark))
            {
                //return new Exception(MessageResources.InvalidPrintMark);
                _invItem.PrintMark = "N";
            }
            else
            {
                _invItem.PrintMark = _invItem.PrintMark.ToUpper();
                if (_invItem.PrintMark != "Y" && _invItem.PrintMark != "N")
                {
                    return new Exception(MessageResources.InvalidPrintMark);
                }
            }

            if (_invItem.InvoiceType.IsValidInvoiceType(out byte data))
            {
                _container.InvoiceType = data;
            }
            else
            {
                return new Exception(String.Format(MessageResources.InvalidInvoiceType, _invItem.InvoiceType));
            }

            return null;
        }

        protected virtual Exception checkCarrierDataIsComplete()
        {

            if (String.IsNullOrEmpty(_invItem.CarrierType))
            {
                return new Exception(MessageResources.AlertInvoiceCarrierComplete);
            }
            else
            {
                if (_invItem.CarrierType.Length > 6 || (_invItem.CarrierId1 != null && _invItem.CarrierId1.Length > 64) || (_invItem.CarrierId2 != null && _invItem.CarrierId2.Length > 64))
                    return new Exception(String.Format(MessageResources.AlertInvoiceCarrierLength, _invItem.CarrierType, _invItem.CarrierId1, _invItem.CarrierId2));

                _carrier = new InvoiceCarrier
                {
                    CarrierType = _invItem.CarrierType
                };

                if (!String.IsNullOrEmpty(_invItem.CarrierId1))
                {
                    if (_invItem.CarrierId1.Length > 64)
                        return new Exception(String.Format(MessageResources.AlertInvoiceCarrierLength, _invItem.CarrierType, _invItem.CarrierId1, _invItem.CarrierId2));

                    _carrier.CarrierNo = _invItem.CarrierId1;
                }

                if (!String.IsNullOrEmpty(_invItem.CarrierId2))
                {
                    if (_invItem.CarrierId2.Length > 64)
                        return new Exception(String.Format(MessageResources.AlertInvoiceCarrierLength, _invItem.CarrierType, _invItem.CarrierId1, _invItem.CarrierId2));

                    _carrier.CarrierNo2 = _invItem.CarrierId2;
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

    public enum IsB2C
    {
        No = 0,
        Yes = 1
    }
    public enum PrintedMark
    {
        No = 0,
        Yes = 1
    }
    public enum CarrierIntent
    {
        No = 0,
        Yes = 1
    }
    public enum DonationIntent
    {
        No = 0,
        Yes = 1
    }

}
