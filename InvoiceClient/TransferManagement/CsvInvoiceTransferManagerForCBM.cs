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
    public class CsvInvoiceTransferManagerForCBM : ITransferManager
    {
        private InvoiceWatcher _InvoiceWatcher;
        private InvoiceWatcher _CancellationWatcher;

        public ITabWorkItem WorkItem { get; set; }

        public void EnableAll(String fullPath)
        {
            _InvoiceWatcher = new CsvInvoiceWatcherForCrossBorderMerchant(Path.Combine(fullPath, Settings.Default.UploadCsvInvoiceFolder))
            {
                ResponsePath = Path.Combine(fullPath, Settings.Default.UploadPreInvoiceFolder)
            };
            _InvoiceWatcher.StartUp();

            _CancellationWatcher = new CsvInvoiceCancellationWatcherForCrossBorderMerchant(Path.Combine(fullPath, Settings.Default.UploadCsvInvoiceCancellationFolder))
            {
                ResponsePath = Path.Combine(fullPath, Settings.Default.UploadInvoiceCancellationFolder)
            };
            _CancellationWatcher.StartUp();

        }

        public void PauseAll()
        {
            if (_InvoiceWatcher != null)
            {
                _InvoiceWatcher.Dispose();
            }

            if (_CancellationWatcher != null)
            {
                _CancellationWatcher.Dispose();
            }

        }

        public String ReportError()
        {
            StringBuilder sb = new StringBuilder();
            if (_InvoiceWatcher != null)
                sb.Append(_InvoiceWatcher.ReportError());
            if (_CancellationWatcher != null)
                sb.Append(_CancellationWatcher.ReportError());

            return sb.ToString();

        }

        public void SetRetry()
        {
            _InvoiceWatcher.Retry();
            _CancellationWatcher.Retry();
        }



        public Type UIConfigType
        {
            get { return typeof(InvoiceClient.MainContent.CsvInvoiceCenterConfig); }
        }
    }
}
