using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Security.MembershipManagement;
using CommonLib.Utility;
using CommonLib.DataAccess;
using ModelCore.Helper;
using CommonLib.Core.Utility;


namespace ModelCore.InvoiceManagement
{
    public class TrackNoManager : EIVOEntityManager<InvoiceItem>
    {
        private InvoiceNoInterval _currentInterval;
        private int? _currentNo;
        private DateTime _uploadInvoiceDate;
        private String _deviceName;
        private int _sellerID;
        private bool _closed = false;
        private Naming.InvoiceTypeDefinition _typeIndication = Naming.InvoiceTypeDefinition.一般稅額計算之電子發票;

        //private static List<int> __lockManager = new List<int>();

        public TrackNoManager(int sellerID)
            : this(null, sellerID, null)
        {

        }

        public TrackNoManager(GenericManager<EIVOEntityDataContext> mgr, int sellerID,String deviceName)
            : base(mgr)
        {
            _sellerID = sellerID;
            _deviceName = deviceName;
            initialize();
        }

        public TrackNoManager(GenericManager<EIVOEntityDataContext> mgr, int sellerID)
            : this(mgr, sellerID, null)
        {
        }

        public void ApplyInvoiceTypeIndication(Naming.InvoiceTypeDefinition indication)
        {
            _typeIndication = indication;
            resetInterval();
            initialize();
            _currentInterval = getNextInterval(null);
        }

        public Naming.InvoiceTypeDefinition? InvoiceTypeIndication => _typeIndication;

        public InvoiceNoInterval InitializeInvoiceNoInterval()
        {
            if (_currentInterval == null)
            {
                _currentInterval = getNextInterval(null);
            }
            return _currentInterval;
        }

        private void initialize()
        {
            //lock (__lockManager)
            //{
            //    if (__lockManager.Contains(_sellerID))
            //    {
            //        throw new Exception("發票配號程序已被佔用!!");
            //    }
            //    else
            //    {
            //        __lockManager.Add(_sellerID);
            //    }
            //}

            _uploadInvoiceDate = DateTime.Now;
            //_currentInterval = getNextInterval(null);

        }

        public void Close()
        {
            try
            {
                _closed = true;
                resetInterval();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void resetInterval()
        {
            if (_currentInterval != null)
            {
                this.ExecuteCommand("Update InvoiceNoInterval set LockID = null where IntervalID = {0}", _currentInterval.IntervalID);
                _currentInterval = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_closed)
                Close();
            base.Dispose(disposing);
            //lock (__lockManager)
            //{
            //    __lockManager.Remove(_sellerID);
            //}
        }

        public bool ApplyInvoiceDate(DateTime invoiceDate)
        {
            if (_currentInterval == null
                || _uploadInvoiceDate.Year != invoiceDate.Year
                || (_uploadInvoiceDate.Month / 2) != (invoiceDate.Month / 2))
            {
                _uploadInvoiceDate = invoiceDate;
                resetInterval();
                _currentInterval = getNextInterval(null);
            }
            return _currentInterval != null;
        }

        //public bool ApplyDeviceName(String deviceName)
        //{
        //    _deviceName = deviceName;
        //    resetInterval();
        //    _currentInterval = getNextInterval(null);

        //    return _currentInterval != null;
        //}

        public int? PeekInvoiceNo()
        {
            InitializeInvoiceNoInterval();
            return _currentNo;
        }

        public bool CheckInvoiceNo(InvoiceItem item)
        {
            if (InitializeInvoiceNoInterval() == null)
            {
                return false;
            }

            if (!_currentNo.HasValue)
                return false;
            
            item.InvoiceNoAssignment = new InvoiceNoAssignment
            {
                InvoiceNoInterval = _currentInterval,
                InvoiceNo = _currentNo
            };

            item.TrackCode = _currentInterval.InvoiceTrackCodeAssignment.InvoiceTrackCode.TrackCode;
            item.No = String.Format("{0:00000000}", _currentNo);
            item.TrackID = _currentInterval.TrackID;

            _currentNo++;
            if (_currentNo > _currentInterval.EndNo)
            {
                _currentInterval = getNextInterval(_currentInterval.IntervalID);
            }
            return true;
        }

        public InvoiceNoAllocation AllocateInvoiceNo()
        {
            if (InitializeInvoiceNoInterval() == null)
            {
                return null;
            }

            if (!_currentNo.HasValue)
                return null;

            InvoiceNoAllocation item = new InvoiceNoAllocation
            {
                InvoiceNoInterval = _currentInterval,
                InvoiceNo = _currentNo.Value,
                Status = (int)Naming.UploadStatusDefinition.等待匯入,
                AllocateDate = DateTime.Now
            };
            _currentInterval.InvoiceNoAllocation.Add(item);
            this.SubmitChanges();

            _currentNo++;
            if (_currentNo > _currentInterval.EndNo)
            {
                _currentInterval = getNextInterval(_currentInterval.IntervalID);
            }
            return item;

        }

        public InvoiceNoInterval GetAppliedInterval(DateTime invoiceDate,String trackCode,int invoiceNo)
        {
            int currentYear = invoiceDate.Year;
            int currentPeriodNo = (invoiceDate.Month + 1) / 2;
            var intervalItems = getAvailableInterval(currentYear, currentPeriodNo, false);
            //var intervalItems = this.GetTable<InvoiceNoInterval>().Where(n => n.InvoiceTrackCodeAssignment.SellerID == _sellerID
            //    && n.InvoiceTrackCodeAssignment.InvoiceTrackCode.Year == currentYear
            //    && n.InvoiceTrackCodeAssignment.InvoiceTrackCode.PeriodNo == currentPeriodNo
            //    && n.InvoiceTrackCodeAssignment.InvoiceTrackCode.TrackCode == trackCode);
            return intervalItems.Where(n => n.StartNo <= invoiceNo && n.EndNo >= invoiceNo).FirstOrDefault();
        }


        private InvoiceNoInterval getNextInterval(int? intervalID)
        {
            //lock (__lockManager)
            //lock(typeof(InvoiceNoInterval))
            {
                int currentYear = _uploadInvoiceDate.Year;
                int currentPeriodNo = (_uploadInvoiceDate.Month + 1) / 2;

                var intervalItems = getAvailableInterval(currentYear, currentPeriodNo);

                //var intervalItems = this.GetTable<InvoiceNoInterval>().Where(n => n.InvoiceTrackCodeAssignment.SellerID == _sellerID
                //    && n.InvoiceTrackCodeAssignment.InvoiceTrackCode.Year == currentYear
                //    && n.InvoiceTrackCodeAssignment.InvoiceTrackCode.PeriodNo == currentPeriodNo
                //    && !n.LockID.HasValue);

                intervalItems = intervalItems
                                .Where(n => !n.InvoiceNoAssignments.Any()
                                    || n.InvoiceNoAssignments.Max(a => a.InvoiceNo) < n.EndNo)
                                .Where(n => !n.InvoiceNoAllocation.Any()
                                    || n.InvoiceNoAllocation.Max(a => a.InvoiceNo) < n.EndNo);

                if (intervalID.HasValue)
                    intervalItems = intervalItems.Where(n => n.IntervalID > intervalID);

                _currentNo = null;
                InvoiceNoInterval item = null;

                item = null;
                foreach (var interval in intervalItems.OrderBy(n => n.IntervalID).ThenBy(n => n.StartNo).ToList())
                {
                    if (this.ExecuteCommand("Update InvoiceNoInterval set LockID = 1 where IntervalID = {0} And LockID is null", interval.IntervalID) > 0)
                    {
                        item = interval;
                        break;
                    }
                }

                if (item != null)
                {
                    _currentNo = item.CurrentAllocatingNo();
                }

                return item;
            }
        }

        private IQueryable<InvoiceNoInterval> getAvailableInterval(int currentYear, int currentPeriodNo, bool checkLock = true)
        {
            var trackCodeItems = this.GetTable<InvoiceTrackCode>()
                .Where(t => t.Year == currentYear)
                .Where(t => t.PeriodNo == currentPeriodNo)
                .Where(t => t.InvoiceType == (byte)_typeIndication);

            var trackCodeAssignment = this.GetTable<InvoiceTrackCodeAssignment>()
                .Where(c => c.SellerID == _sellerID)
                .Join(trackCodeItems, c => c.TrackID, t => t.TrackID, (c, t) => c);

            IQueryable<InvoiceNoInterval> items = this.GetTable<InvoiceNoInterval>();
            if (checkLock)
            {
                items = items
                    .Where(n => !n.LockID.HasValue);
            }

            if (_deviceName != null)
            {
                items = items.Where(i => i.InvoiceNoSegment.DeviceName == _deviceName);
            }
            else
            {
                items = items.Where(i => i.InvoiceNoSegment == null);
            }

            items = items.Join(trackCodeAssignment, n => new { n.TrackID, n.SellerID }, c => new { c.TrackID, c.SellerID }, (n, c) => n);

            return items;
        }
    }
}
