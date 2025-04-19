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
    public class CsvInvoiceTransferManagerV2 : ITransferManager
    {
        private InvoiceWatcher _InvoiceWatcher;
        private InvoiceWatcher _CustomerInvoiceWatcher;
        private InvoiceWatcher _CancellationWatcher;
        private InvoiceWatcher _AllowanceWatcher;
        private InvoiceWatcher _AllowanceCancellationWatcher;

        public ITabWorkItem WorkItem { get; set; }

        public void EnableAll(String fullPath)
        {
            _InvoiceWatcher = new CsvInvoiceWatcherV2(Path.Combine(fullPath, Settings.Default.UploadCsvInvoiceFolder));
            _InvoiceWatcher.StartUp();

            _CustomerInvoiceWatcher = new CustomerCsvInvoiceWatcher(Path.Combine(fullPath, Settings.Default.UploadCustomerCsvInvoiceFolder));
            _CustomerInvoiceWatcher.StartUp();

            _CancellationWatcher = new CsvInvoiceCancellationWatcher(Path.Combine(fullPath, Settings.Default.UploadCsvInvoiceCancellationFolder));
            _CancellationWatcher.StartUp();

            _AllowanceWatcher = new CsvAllowanceWatcher(Path.Combine(fullPath, Settings.Default.UploadCsvAllowanceFolder));
            _AllowanceWatcher.StartUp();

            _AllowanceCancellationWatcher = new CsvAllowanceCancellationWatcher(Path.Combine(fullPath, Settings.Default.UploadCsvAllowanceCancellationFolder));
            _AllowanceCancellationWatcher.StartUp();

        }

        public void PauseAll()
        {
            if (_InvoiceWatcher != null)
            {
                _InvoiceWatcher.Dispose();
            }
            if(_CustomerInvoiceWatcher!=null)
            {
                _CustomerInvoiceWatcher.Dispose();
            }
            if (_CancellationWatcher != null)
            {
                _CancellationWatcher.Dispose();
            }
            if (_AllowanceWatcher != null)
            {
                _AllowanceWatcher.Dispose();
            }
            if (_AllowanceCancellationWatcher != null)
            {
                _AllowanceCancellationWatcher.Dispose();
            }
        }

        public String ReportError()
        {
            StringBuilder sb = new StringBuilder();
            if (_InvoiceWatcher != null)
                sb.Append(_InvoiceWatcher.ReportError());
            if (_CustomerInvoiceWatcher != null)
                sb.Append(_CustomerInvoiceWatcher.ReportError());
            if (_CancellationWatcher != null)
                sb.Append(_CancellationWatcher.ReportError());
            if (_AllowanceWatcher != null)
                sb.Append(_AllowanceWatcher.ReportError());
            if (_AllowanceCancellationWatcher != null)
                sb.Append(_AllowanceCancellationWatcher.ReportError());

            return sb.ToString();

        }

        public void SetRetry()
        {
            _InvoiceWatcher.Retry();
            _CustomerInvoiceWatcher.Retry();
            _CancellationWatcher.Retry();
            _AllowanceWatcher.Retry();
            _AllowanceCancellationWatcher.Retry();
        }



        public Type UIConfigType
        {
            get { return typeof(InvoiceClient.MainContent.CsvInvoiceCenterConfig); }
        }
    }
}
