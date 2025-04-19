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
using InvoiceClient.Helper;

namespace InvoiceClient.TransferManagement
{
    public class XmlInvoiceTransferManagerForCBE : ITransferManager
    {
        private InvoiceWatcher _InvoiceWatcher;
        private InvoiceWatcher _CancellationWatcher;
        private InvoiceWatcher _AllowanceWatcher;
        private InvoiceWatcher _AllowanceCancellationWatcher;

        LocalSettings _Settings;
        class LocalSettings
        {
            public String InvoiceRequestPath { get; set; } = "PreInvoice";
            public String VoidInvoiceRequestPath { get; set; } = "CancelInvoice";
            public String AllowanceRequestPath { get; set; } = "Allowance";
            public String VoidAllowanceRequestPath { get; set; } = "CancelAllowance";
        }
        public ITabWorkItem WorkItem { get; set; }

        public XmlInvoiceTransferManagerForCBE()
        {
            String filePath = Path.Combine(Logger.LogPath, "XmlInvoiceTransferManagerForCBE.json");
            if (File.Exists(filePath))
            {
                _Settings = JsonConvert.DeserializeObject<LocalSettings>(File.ReadAllText(filePath));
            }
            else
            {
                _Settings = new LocalSettings { };
                File.WriteAllText(filePath, JsonConvert.SerializeObject(_Settings));
            }
        }

        public void EnableAll(String fullPath)
        {
            _InvoiceWatcher = new XmlProcessRequestWatcher(Path.Combine(fullPath, _Settings.InvoiceRequestPath)) 
            {
                ResponsibleProcessType = Naming.InvoiceProcessType.C0401_Xml_CBE
            };
            _InvoiceWatcher.StartUp();

            _CancellationWatcher = new XmlProcessRequestWatcher(Path.Combine(fullPath, _Settings.VoidInvoiceRequestPath))
            {
                ResponsibleProcessType = Naming.InvoiceProcessType.C0501
            };
            _CancellationWatcher.StartUp();

            _AllowanceWatcher = new XmlProcessRequestWatcher(Path.Combine(fullPath, _Settings.AllowanceRequestPath))
            {
                ResponsibleProcessType = Naming.InvoiceProcessType.D0401
            };
            _AllowanceWatcher.StartUp();

            _AllowanceCancellationWatcher = new XmlProcessRequestWatcher(Path.Combine(fullPath, _Settings.VoidAllowanceRequestPath))
            {
                ResponsibleProcessType = Naming.InvoiceProcessType.D0501
            };
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
            get { return typeof(InvoiceClient.MainContent.CBEXmlInvoiceCenterConfig); }
        }
    }
}
