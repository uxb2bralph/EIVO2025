using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;

using InvoiceClient.Properties;
using ModelCore.Schema.EIVO;
using InvoiceClient.Agent;
using InvoiceClient.Helper;

namespace InvoiceClient.TransferManagement
{
    public class B2BInvoiceTransferManager : ITransferManager
    {
        private B2BInvoiceWatcher _InvoiceWatcher;
        private B2BBuyerInvoiceWatcher _BuyerInvoiceWatcher;
        private B2BAllowanceWatcher _AllowanceWatcher;
        private B2BInvoiceCancellationWatcher _CancellationWatcher;
        private B2BAllowanceCancellationWatcher _AllowanceCancellationWatcher;
        private B2BCounterpartBusinessWatcher _CounterpartBusinessWatcher;
        private ReceiptWatcher _receiptWatcher;
        private ReceiptCancellationWatcher _receiptCancellationWatcher;

        public ITabWorkItem WorkItem { get; set; }

        public void EnableAll(String fullPath)
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

            _receiptWatcher = new ReceiptWatcher(Path.Combine(fullPath, Settings.Default.B2BUploadReceiptFolder));
            _receiptWatcher.StartUp();

            _receiptCancellationWatcher = new ReceiptCancellationWatcher(Path.Combine(fullPath, Settings.Default.B2BUploadReceiptCancellationFolder));
            _receiptCancellationWatcher.StartUp();

        }

        public void PauseAll()
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

            if (_receiptWatcher != null)
            {
                _receiptWatcher.Dispose();
            }

            if (_receiptCancellationWatcher != null)
            {
                _receiptCancellationWatcher.Dispose();
            }
        }

        public String ReportError()
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
            if (_receiptWatcher != null)
                sb.Append(_receiptWatcher.ReportError());
            if (_receiptCancellationWatcher != null)
                sb.Append(_receiptCancellationWatcher.ReportError());
            return sb.ToString();

        }

        public void SetRetry()
        {
            _InvoiceWatcher.Retry();
            _CancellationWatcher.Retry();
            _BuyerInvoiceWatcher.Retry();
            _AllowanceWatcher.Retry();
            _AllowanceCancellationWatcher.Retry();
            _CounterpartBusinessWatcher.Retry();
            _receiptWatcher.Retry();
            _receiptCancellationWatcher.Retry();
        }



        public Type UIConfigType
        {
            get { return typeof(InvoiceClient.MainContent.B2BInvoiceCenterConfig); }
        }
    }
}
