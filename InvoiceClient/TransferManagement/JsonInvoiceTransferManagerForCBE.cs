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
using ModelCore.Locale;
using InvoiceClient.MainContent;
using InvoiceClient.Helper;

namespace InvoiceClient.TransferManagement
{
    public class JsonInvoiceTransferManagerForCBE : ITransferManager
    {
        private InvoiceWatcher _InvoiceWatcher;
        private InvoiceWatcher _CancellationWatcher;
        private InvoiceWatcher _AllowanceWatcher;
        private InvoiceWatcher _AllowanceCancellationWatcher;
        private JsonInvoiceTransferManagerForCBE.LocalSettings _Settings;
        public ITabWorkItem WorkItem { get; set; }

        public JsonInvoiceTransferManagerForCBE()
        {
            string path = Path.Combine(Logger.LogPath, "JsonInvoiceTransferManagerForCBE.json");
            if (File.Exists(path))
            {
                this._Settings = JsonConvert.DeserializeObject<JsonInvoiceTransferManagerForCBE.LocalSettings>(File.ReadAllText(path));
            }
            else
            {
                this._Settings = new JsonInvoiceTransferManagerForCBE.LocalSettings();
                File.WriteAllText(path, JsonConvert.SerializeObject((object)this._Settings));
            }
        }

        public void EnableAll(string fullPath)
        {
            this._InvoiceWatcher = (InvoiceWatcher)new ProcessRequestWatcher(Path.Combine(fullPath, this._Settings.InvoiceRequestPath))
            {
                ResponsibleProcessType = new Naming.InvoiceProcessType?(Naming.InvoiceProcessType.C0401_Json_CBE)
            };
            this._InvoiceWatcher.StartUp();
            this._CancellationWatcher = (InvoiceWatcher)new ProcessRequestWatcher(Path.Combine(fullPath, this._Settings.VoidInvoiceRequestPath))
            {
                ResponsibleProcessType = new Naming.InvoiceProcessType?(Naming.InvoiceProcessType.C0501_Json)
            };
            this._CancellationWatcher.StartUp();
            this._AllowanceWatcher = (InvoiceWatcher)new ProcessRequestWatcher(Path.Combine(fullPath, this._Settings.AllowanceRequestPath))
            {
                ResponsibleProcessType = new Naming.InvoiceProcessType?(Naming.InvoiceProcessType.D0401_Json)
            };
            this._AllowanceWatcher.StartUp();
            this._AllowanceCancellationWatcher = (InvoiceWatcher)new ProcessRequestWatcher(Path.Combine(fullPath, this._Settings.VoidAllowanceRequestPath))
            {
                ResponsibleProcessType = new Naming.InvoiceProcessType?(Naming.InvoiceProcessType.D0501_Json)
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
