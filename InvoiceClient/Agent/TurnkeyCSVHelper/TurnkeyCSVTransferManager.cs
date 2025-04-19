using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;

using InvoiceClient.Properties;
using ModelCore.Schema.EIVO;
using InvoiceClient.Agent;
using CommonLib.Core.Utility;
using CommonLib.Utility;
using Newtonsoft.Json;
using InvoiceClient.TransferManagement;
using InvoiceClient.Helper;

namespace InvoiceClient.Agent.TurnkeyCSVHelper
{
    public class TurnkeyCSVTransferManager : ITransferManager
    {
        private InvoiceWatcher _InvoiceWatcher;
        private InvoiceWatcher _CancellationWatcher;
        private InvoiceWatcher _AllowanceWatcher;
        private InvoiceWatcher _AllowanceCancellationWatcher;
        private LocalSettings _Settings;

        public ITabWorkItem WorkItem { get; set; }

        public TurnkeyCSVTransferManager()
        {
            string path = Path.Combine(Logger.LogPath, "TurnkeyCSVTransferManager.json");
            if (File.Exists(path))
            {
                this._Settings = JsonConvert.DeserializeObject<LocalSettings>(File.ReadAllText(path));
            }
            else
            {
                this._Settings = new LocalSettings();
            }
            File.WriteAllText(path, JsonConvert.SerializeObject((object)this._Settings));
        }
        public void EnableAll(String fullPath)
        {
            _InvoiceWatcher = new ERPInvoiceWatcher(Path.Combine(fullPath, _Settings.Invoice));
            _InvoiceWatcher.StartUp();


            _CancellationWatcher = new ERPInvoiceCancellationWatcher(Path.Combine(fullPath, _Settings.InvoiceCancellation));
            _CancellationWatcher.StartUp();

            _AllowanceWatcher = new ERPAllowanceWatcher(Path.Combine(fullPath, _Settings.Allowance));
            _AllowanceWatcher.StartUp();

            _AllowanceCancellationWatcher = new ERPAllowanceCancellationWatcher(Path.Combine(fullPath, _Settings.AllowanceCancellation));
            _AllowanceCancellationWatcher.StartUp();

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
            _CancellationWatcher.Retry();
            _AllowanceWatcher.Retry();
            _AllowanceCancellationWatcher.Retry();
        }



        public Type UIConfigType
        {
            get { return typeof(InvoiceClient.MainContent.MIGInvoiceConfig); }
        }

        private class LocalSettings
        {
            public string Invoice { get; set; } = "Invoice_csv";
            public string InvoiceCancellation { get; set; } = "CancelInvoice_csv";
            public string Allowance { get; set; } = "Allowance_csv";
            public string AllowanceCancellation { get; set; } = "CancelAllowance_csv";
        }
    }
}
