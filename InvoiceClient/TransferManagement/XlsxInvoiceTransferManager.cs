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
using ModelCore.Locale;

namespace InvoiceClient.TransferManagement
{
    public class XlsxInvoiceTransferManager : ITransferManager
    {
        private InvoiceWatcher _InvoiceWatcher;
        private InvoiceWatcher _DelayedInvoiceWatcher;
        private InvoiceWatcher _CancellationWatcher;
        private InvoiceWatcher _AllowanceWatcher;
        private InvoiceWatcher _AllowanceCancellationWatcher;
        private InvoiceWatcher _FullAllowanceWatcher;

        static XlsxInvoiceTransferManager _Default;

        LocalSettings _Settings;
        internal class LocalSettings
        {
            public String InvoiceRequestPath { get; set; } = "Invoice_Xlsx";
            public String VoidInvoiceRequestPath { get; set; } = "CancelInvoice_Xlsx";
            public String AllowanceRequestPath { get; set; } = "Allowance_Xlsx";
            public String VoidAllowanceRequestPath { get; set; } = "CancelAllowance_Xlsx";
            public String FullAllowanceRequestPath { get; set; } = "FullAllowance_Xlsx";
            public String DelayedInvoiceRequestPath { get; set; } = "Delayed_Invoice_Xlsx";
            public Naming.InvoiceProcessType? DefaultProcessType { get; set; }

        }
        public ITabWorkItem WorkItem { get; set; }

        public XlsxInvoiceTransferManager()
        {
            String filePath = Path.Combine(Logger.LogPath, "XlsxInvoiceTransferManager.json");
            if (File.Exists(filePath))
            {
                _Settings = JsonConvert.DeserializeObject<LocalSettings>(File.ReadAllText(filePath));
            }
            else
            {
                _Settings = new LocalSettings { };
                File.WriteAllText(filePath, JsonConvert.SerializeObject(_Settings));
            }

            _Default = this;
        }

        internal static XlsxInvoiceTransferManager Default => _Default;

        internal LocalSettings Settings => _Settings;

        public void EnableAll(String fullPath)
        {
            _InvoiceWatcher = new XlsxInvoiceWatcher(Path.Combine(fullPath, _Settings.InvoiceRequestPath))
            {
                TxnPath = fullPath
            };
            _InvoiceWatcher.StartUp();

            _DelayedInvoiceWatcher = new XlsxDelayedInvoiceWatcher(Path.Combine(fullPath, _Settings.DelayedInvoiceRequestPath)) { TxnPath = fullPath };
            _DelayedInvoiceWatcher.StartUp();

            _CancellationWatcher = new XlsxVoidInvoiceWatcher(Path.Combine(fullPath, _Settings.VoidInvoiceRequestPath))
            {
                TxnPath = fullPath
            };
            _CancellationWatcher.StartUp();

            _AllowanceWatcher = new XlsxAllowanceWatcher(Path.Combine(fullPath, _Settings.AllowanceRequestPath)) { TxnPath = fullPath };
            _AllowanceWatcher.StartUp();

            _AllowanceCancellationWatcher = new XlsxVoidAllowanceWatcher(Path.Combine(fullPath, _Settings.VoidAllowanceRequestPath)) { TxnPath = fullPath };
            _AllowanceCancellationWatcher.StartUp();

            _FullAllowanceWatcher = new XlsxFullAllowanceWatcher(Path.Combine(fullPath, _Settings.FullAllowanceRequestPath)) { TxnPath = fullPath };
            _FullAllowanceWatcher.StartUp();

        }

        public void PauseAll()
        {
            if (_InvoiceWatcher != null)
            {
                _InvoiceWatcher.Dispose();
            }

            if (_DelayedInvoiceWatcher != null)
            {
                _DelayedInvoiceWatcher.Dispose();
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
            if (_FullAllowanceWatcher != null)
            {
                _FullAllowanceWatcher.Dispose();
            }

        }

        public String ReportError()
        {
            StringBuilder sb = new StringBuilder();
            if (_InvoiceWatcher != null)
                sb.Append(_InvoiceWatcher.ReportError());
            if (_DelayedInvoiceWatcher != null)
                sb.Append(_DelayedInvoiceWatcher.ReportError());
            if (_CancellationWatcher != null)
                sb.Append(_CancellationWatcher.ReportError());
            if (_AllowanceWatcher != null)
                sb.Append(_AllowanceWatcher.ReportError());
            if (_AllowanceCancellationWatcher != null)
                sb.Append(_AllowanceCancellationWatcher.ReportError());
            if (_FullAllowanceWatcher != null)
                sb.Append(_FullAllowanceWatcher.ReportError());

            return sb.ToString();

        }

        public void SetRetry()
        {
            _InvoiceWatcher.Retry();
            _DelayedInvoiceWatcher.Retry();
            _CancellationWatcher.Retry();
            _AllowanceWatcher.Retry();
            _AllowanceCancellationWatcher.Retry();
            _FullAllowanceWatcher.Retry();
        }



        public Type UIConfigType
        {
            get { return typeof(InvoiceClient.MainContent.XlsxInvoiceCenterConfig); }
        }
    }
}
