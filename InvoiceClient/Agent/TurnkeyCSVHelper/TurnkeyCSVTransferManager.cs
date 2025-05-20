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
using ModelCore.Locale;

namespace InvoiceClient.Agent.TurnkeyCSVHelper
{
    public class TurnkeyCSVTransferManager : ITransferManager
    {
        protected InvoiceWatcher? _InvoiceWatcher;
        protected InvoiceWatcher? _InvoiceExchangeWatcher;
        protected InvoiceWatcher? _CancellationWatcher;
        protected InvoiceWatcher? _AllowanceWatcher;
        protected InvoiceWatcher? _AllowanceCancellationWatcher;
        protected LocalSettings? _Settings;

        public ITabWorkItem? WorkItem { get; set; }

        public TurnkeyCSVTransferManager()
        {
            _Settings = AppSettings.Default.TurnkeyCSVSettings;
            if(_Settings == null)
            {
                AppSettings.Default.TurnkeyCSVSettings = _Settings = new LocalSettings()
                {
                    Invoice = "Invoice_csv",
                    InvoiceExchange = "Invoice_exchagne_csv",
                    InvoiceCancellation = "CancelInvoice_csv",
                    Allowance = "Allowance_csv",
                    AllowanceCancellation = "CancelAllowance_csv"
                };
                AppSettings.Default.Save();
            }
        }
        public virtual void EnableAll(String fullPath)
        {
            _InvoiceWatcher = new ERPInvoiceWatcher(Path.Combine(fullPath, _Settings!.Invoice!));
            _InvoiceWatcher.StartUp();

            _InvoiceExchangeWatcher = new ERPInvoiceWatcher(Path.Combine(fullPath, _Settings!.InvoiceExchange!))
            {
                PreferredProcessType = Naming.InvoiceProcessType.A0101
            };
            _InvoiceExchangeWatcher.StartUp();

            _CancellationWatcher = new ERPInvoiceCancellationWatcher(Path.Combine(fullPath, _Settings!.InvoiceCancellation!));
            _CancellationWatcher.StartUp();

            _AllowanceWatcher = new ERPAllowanceWatcher(Path.Combine(fullPath, _Settings!.Allowance!));
            _AllowanceWatcher.StartUp();

            _AllowanceCancellationWatcher = new ERPAllowanceCancellationWatcher(Path.Combine(fullPath, _Settings!.AllowanceCancellation!));
            _AllowanceCancellationWatcher.StartUp();

        }

        public void PauseAll()
        {
            _InvoiceWatcher?.Dispose();
            _InvoiceExchangeWatcher?.Dispose();
            _CancellationWatcher?.Dispose();
            _AllowanceWatcher?.Dispose();
            _AllowanceCancellationWatcher?.Dispose();
        }

        public String ReportError()
        {
            StringBuilder sb = new StringBuilder();

            if (_InvoiceWatcher != null)
                sb.Append(_InvoiceWatcher.ReportError());

            if (_InvoiceExchangeWatcher != null)
                sb.Append(_InvoiceExchangeWatcher.ReportError());

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
            _InvoiceWatcher!.Retry();
            _InvoiceExchangeWatcher!.Retry();
            _CancellationWatcher!.Retry();
            _AllowanceWatcher!.Retry();
            _AllowanceCancellationWatcher!.Retry();
        }



        public Type UIConfigType
        {
            get { return typeof(InvoiceClient.MainContent.MIGInvoiceConfig); }
        }
    }
}
