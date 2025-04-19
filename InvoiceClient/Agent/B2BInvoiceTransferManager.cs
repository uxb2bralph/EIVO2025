using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;

using InvoiceClient.Properties;
using ModelCore.Schema.EIVO;

namespace InvoiceClient.Agent
{
    public static class B2BInvoiceTransferManager
    {
        private static B2BInvoiceWatcher _InvoiceWatcher;
        private static B2BBuyerInvoiceWatcher _BuyerInvoiceWatcher;
        private static B2BAllowanceWatcher _AllowanceWatcher;
        private static B2BInvoiceCancellationWatcher _CancellationWatcher;
        private static B2BAllowanceCancellationWatcher _AllowanceCancellationWatcher;
        private static B2BCounterpartBusinessWatcher _CounterpartBusinessWatcher;
        //private static BranchTrackBlankWatcher _BranchTrackBlankWatcher;


        public static void EnableAll(String fullPath)
        {

            _InvoiceWatcher = new B2BInvoiceWatcher(Path.Combine(fullPath, Settings.Default.B2BUploadInvoiceFolder));
            _InvoiceWatcher.StartUp();

            _BuyerInvoiceWatcher = new B2BBuyerInvoiceWatcher(Path.Combine(fullPath, Settings.Default.B2BUploadBuyerInvoiceFolder));
            _BuyerInvoiceWatcher.StartUp();

            _AllowanceWatcher = new B2BAllowanceWatcher(Path.Combine(fullPath, Settings.Default.B2BUploadAllowanceFolder));
            _AllowanceWatcher.StartUp();

            _CancellationWatcher = new B2BInvoiceCancellationWatcher(Path.Combine(fullPath, Settings.Default.B2BUploadInvoiceCancellationFolder));
            _CancellationWatcher.StartUp();

            _AllowanceCancellationWatcher = new B2BAllowanceCancellationWatcher(Path.Combine(fullPath, Settings.Default.B2BUploadAllowanceCancellationFolder));
            _AllowanceCancellationWatcher.StartUp();

            _CounterpartBusinessWatcher = new B2BCounterpartBusinessWatcher(Path.Combine(fullPath, Settings.Default.B2BCounterpartBusinessFolder));
            _CounterpartBusinessWatcher.StartUp();

            //_BranchTrackBlankWatcher = new BranchTrackBlankWatcher(Path.Combine(fullPath, Settings.Default.BranchTrackBlankFolder));
            //_BranchTrackBlankWatcher.StartUp();
        }

        public static void PauseAll()
        {
            if (_InvoiceWatcher != null)
            {
                _InvoiceWatcher.Dispose();
            }

            if (_BuyerInvoiceWatcher != null)
            {
                _BuyerInvoiceWatcher.Dispose();
            }

            if (_AllowanceWatcher != null)
            {
                _AllowanceWatcher.Dispose();
            }
            if (_CancellationWatcher != null)
            {
                _CancellationWatcher.Dispose();
            }
            if (_AllowanceCancellationWatcher != null)
            {
                _AllowanceCancellationWatcher.Dispose();
            }

            if (_CounterpartBusinessWatcher != null)
            {
                _CounterpartBusinessWatcher.Dispose();
            }

            if (_CounterpartBusinessWatcher != null)
            {
                _CounterpartBusinessWatcher.Dispose();
            }

            //if (_BranchTrackBlankWatcher != null)
            //{
            //    _BranchTrackBlankWatcher.Dispose();
            //}
        }

        public static String ReportError()
        {
            StringBuilder sb = new StringBuilder();
            if (_InvoiceWatcher != null)
                sb.Append(_InvoiceWatcher.ReportError());
            if (_BuyerInvoiceWatcher != null)
                sb.Append(_BuyerInvoiceWatcher.ReportError());
            if (_CancellationWatcher != null)
                sb.Append(_CancellationWatcher.ReportError());
            if (_AllowanceWatcher != null)
                sb.Append(_AllowanceWatcher.ReportError());
            if (_AllowanceCancellationWatcher != null)
                sb.Append(_AllowanceCancellationWatcher.ReportError());
            if (_CounterpartBusinessWatcher != null)
                sb.Append(_CounterpartBusinessWatcher.ReportError());
            //if (_BranchTrackBlankWatcher != null)
            //    sb.Append(_BranchTrackBlankWatcher.ReportError());
            return sb.ToString();

        }

        public static void SetRetry()
        {
            _InvoiceWatcher.Retry();
            _CancellationWatcher.Retry();
            _BuyerInvoiceWatcher.Retry();
            _AllowanceWatcher.Retry();
            _AllowanceCancellationWatcher.Retry();
            _CounterpartBusinessWatcher.Retry();
            //_BranchTrackBlankWatcher.Retry();
        }

    }
}
