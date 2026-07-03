using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using CommonLib.Core.Utility;
using CommonLib.Utility;
using InvoiceClient.Agent;
using InvoiceClient.Agent.JsonHelper;
using InvoiceClient.Helper;
using InvoiceClient.MainContent;
using InvoiceClient.Properties;
using ModelCore.Locale;
using ModelCore.Schema.EIVO;
using Newtonsoft.Json;

namespace InvoiceClient.TransferManagement
{
    public class JsonInvoiceTransferManagerForCBE : ITransferManager
    {
        private InvoiceWatcher _InvoiceWatcher = null!;
        private InvoiceWatcher _CancellationWatcher = null!;
        private InvoiceWatcher _AllowanceWatcher = null!;
        private InvoiceWatcher _AllowanceCancellationWatcher = null!;
        private JsonInvoiceTransferManagerForCBE.LocalSettings _Settings = null!;
        public ITabWorkItem? WorkItem { get; set; }

        public JsonInvoiceTransferManagerForCBE()
        {
            string path = Path.Combine(Logger.LogPath, "JsonInvoiceTransferManagerForCBE.json");
            if (File.Exists(path))
            {
                _Settings = JsonConvert.DeserializeObject<JsonInvoiceTransferManagerForCBE.LocalSettings>(File.ReadAllText(path)) ?? null!;
            }

            if (_Settings == null)
            {
                _Settings = new JsonInvoiceTransferManagerForCBE.LocalSettings();
                File.WriteAllText(path, _Settings.JsonStringify());
            }
        }

        public void EnableAll(string fullPath)
        {
            this._InvoiceWatcher = (InvoiceWatcher)new CBEInvoiceJsonDataWatcher(Path.Combine(fullPath, _Settings.InvoiceRequestPath))
            {
                ResponsibleProcessType = new Naming.InvoiceProcessType?(Naming.InvoiceProcessType.F0401_Json_CBE)
            };
            this._InvoiceWatcher.StartUp();
            this._CancellationWatcher = (InvoiceWatcher)new InvoiceCancellationJsonDataWatcher(Path.Combine(fullPath, _Settings.VoidInvoiceRequestPath))
            {
                ResponsibleProcessType = new Naming.InvoiceProcessType?(Naming.InvoiceProcessType.F0501_Json)
            };
            this._CancellationWatcher.StartUp();
            this._AllowanceWatcher = (InvoiceWatcher)new AllowanceJsonDataWatcher(Path.Combine(fullPath, _Settings.AllowanceRequestPath))
            {
                ResponsibleProcessType = new Naming.InvoiceProcessType?(Naming.InvoiceProcessType.G0401_Json)
            };
            this._AllowanceWatcher.StartUp();
            this._AllowanceCancellationWatcher = (InvoiceWatcher)new AllowanceCancellationJsonDataWatcher(Path.Combine(fullPath, _Settings.VoidAllowanceRequestPath))
            {
                ResponsibleProcessType = new Naming.InvoiceProcessType?(Naming.InvoiceProcessType.G0501_Json)
            };
            this._AllowanceCancellationWatcher.StartUp();
        }

        public void PauseAll()
        {
            if (this._InvoiceWatcher != null)
                this._InvoiceWatcher.Dispose();
            if (this._CancellationWatcher != null)
                this._CancellationWatcher.Dispose();
            if (this._AllowanceWatcher != null)
                this._AllowanceWatcher.Dispose();
            if (this._AllowanceCancellationWatcher != null)
                this._AllowanceCancellationWatcher.Dispose();
        }

        public string ReportError()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (this._InvoiceWatcher != null)
                stringBuilder.Append(this._InvoiceWatcher.ReportError());
            if (this._CancellationWatcher != null)
                stringBuilder.Append(this._CancellationWatcher.ReportError());
            if (this._AllowanceWatcher != null)
                stringBuilder.Append(this._AllowanceWatcher.ReportError());
            if (this._AllowanceCancellationWatcher != null)
                stringBuilder.Append(this._AllowanceCancellationWatcher.ReportError());
            return stringBuilder.ToString();
        }

        public void SetRetry()
        {
            this._InvoiceWatcher.Retry();
            this._CancellationWatcher.Retry();
            this._AllowanceWatcher.Retry();
            this._AllowanceCancellationWatcher.Retry();
        }

        public Type UIConfigType
        {
            get
            {
                return typeof(JsonInvoiceCenterConfig);
            }
        }

        private class LocalSettings
        {
            public string InvoiceRequestPath { get; set; } = "Invoice_Json";

            public string VoidInvoiceRequestPath { get; set; } = "CancelInvoice_Json";

            public string AllowanceRequestPath { get; set; } = "Allowance_Json";

            public string VoidAllowanceRequestPath { get; set; } = "CancelAllowance_Json";
        }
    }
}
