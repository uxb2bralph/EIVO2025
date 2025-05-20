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
    public class TurnkeyExchangeCSVTransferManager : TurnkeyCSVTransferManager
    {

        public TurnkeyExchangeCSVTransferManager()
        {
            _Settings = AppSettings.Default.TurnkeyExchangeCSVSettings;
            if (_Settings == null)
            {
                AppSettings.Default.TurnkeyExchangeCSVSettings = _Settings = new LocalSettings()
                {
                    Invoice = "Invoice_exchange_csv",
                    InvoiceCancellation = "CancelInvoice_exchange_csv",
                    Allowance = "Allowance_exchange_csv",
                    AllowanceCancellation = "CancelAllowance_exchange_csv"
                };
                AppSettings.Default.Save();
            }
        }

        public override void EnableAll(String fullPath)
        {
            _InvoiceWatcher = new ERPInvoiceWatcher(Path.Combine(fullPath, _Settings!.Invoice!))
            {
                PreferredProcessType = Naming.InvoiceProcessType.A0101
            };
            _InvoiceWatcher.StartUp();

            _CancellationWatcher = new ERPInvoiceCancellationWatcher(Path.Combine(fullPath, _Settings!.InvoiceCancellation!))
            {
                PreferredProcessType = Naming.InvoiceProcessType.A0201
            };
            _CancellationWatcher.StartUp();

            _AllowanceWatcher = new ERPAllowanceWatcher(Path.Combine(fullPath, _Settings!.Allowance!))
            {
                PreferredProcessType = Naming.InvoiceProcessType.B0101
            };
            _AllowanceWatcher.StartUp();

            _AllowanceCancellationWatcher = new ERPAllowanceCancellationWatcher(Path.Combine(fullPath, _Settings!.AllowanceCancellation!))
            {
                PreferredProcessType = Naming.InvoiceProcessType.B0201
            };
            _AllowanceCancellationWatcher.StartUp();

        }
    }
}
