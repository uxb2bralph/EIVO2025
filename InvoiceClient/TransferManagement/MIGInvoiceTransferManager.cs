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
using InvoiceClient.Helper;

namespace InvoiceClient.TransferManagement
{
    public class MIGInvoiceTransferManager : ITransferManager
    {
        private InvoiceWatcher _InvoiceWatcher;
        private InvoiceWatcher _CancellationWatcher;
        private AllowanceWatcher _AllowanceWatcher;
        private AllowanceCancellationWatcher _AllowanceCancellationWatcher;
        private MIGInvoiceTransferManager.LocalSettings _Settings;
        public ITabWorkItem WorkItem { get; set; }

        public MIGInvoiceTransferManager()
        {
            string path = Path.Combine(Logger.LogPath, "MIGInvoiceTransferManager.json");
            if (File.Exists(path))
            {
                this._Settings = JsonConvert.DeserializeObject<MIGInvoiceTransferManager.LocalSettings>(File.ReadAllText(path));
            }
            else
            {
                this._Settings = new MIGInvoiceTransferManager.LocalSettings();
                File.WriteAllText(path, JsonConvert.SerializeObject((object)this._Settings));
            }
        }
        public void EnableAll(String fullPath)
        {
            _InvoiceWatcher = new MIGInvoiceWatcher(Path.Combine(fullPath, _Settings.InvoiceRequestPath));
            _InvoiceWatcher.StartUp();

            _CancellationWatcher = new MIGInvoiceCancellationWatcher(Path.Combine(fullPath, _Settings.VoidInvoiceRequestPath));
            _CancellationWatcher.StartUp();

            _AllowanceWatcher = new D0401Watcher(Path.Combine(fullPath, _Settings.AllowanceRequestPath));
            _AllowanceWatcher.StartUp();

            _AllowanceCancellationWatcher = new D0501Watcher(Path.Combine(fullPath, _Settings.VoidAllowanceRequestPath));
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
            public string InvoiceRequestPath { get; set; } = "SellerInvoice_C0401";

            public string VoidInvoiceRequestPath { get; set; } = "CancelInvoice_C0501";

            public string AllowanceRequestPath { get; set; } = "Allowance_D0401";

            public string VoidAllowanceRequestPath { get; set; } = "CancelAllowance_D0501";
        }
    }
}
