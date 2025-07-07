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
using InvoiceClient.Agent.MIGHelper;
using InvoiceClient.Helper;

namespace InvoiceClient.TransferManagement
{
    public class MIGInvoiceTransferManagerV2 : ITransferManager
    {
        private InvoiceWatcher _AttachmentWatcher;
        private InvoiceWatcher _F0401Watcher;
        private InvoiceWatcher _F0501Watcher;
        private InvoiceWatcher _G0401Watcher;
        private InvoiceWatcher _G0501Watcher;
        private InvoiceWatcher _A0101Watcher;
        private LocalSettings _Settings;
        public ITabWorkItem? WorkItem { get; set; }

        public MIGInvoiceTransferManagerV2()
        {
            string path = Path.Combine(Logger.LogPath, "MIG.json");
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
            _AttachmentWatcher = new AttachmentWatcher(Path.Combine(fullPath, _Settings.Attachment));
            _AttachmentWatcher.StartUp();

            _F0401Watcher = new F0401Watcher(Path.Combine(fullPath, _Settings.F0401));
            _F0401Watcher.StartUp();

            _A0101Watcher = new A0101Watcher(Path.Combine(fullPath, _Settings.A0101));
            _A0101Watcher.StartUp();

            _F0501Watcher = new F0501Watcher(Path.Combine(fullPath, _Settings.F0501));
            _F0501Watcher.StartUp();

            _G0401Watcher = new Agent.MIGHelper.G0401Watcher(Path.Combine(fullPath, _Settings.G0401));
            _G0401Watcher.StartUp();

            _G0501Watcher = new Agent.MIGHelper.G0501Watcher(Path.Combine(fullPath, _Settings.G0501));
            _G0501Watcher.StartUp();

        }

        public void PauseAll()
        {
            if (_AttachmentWatcher != null)
            {
                _AttachmentWatcher.Dispose();
            }
            if (_F0401Watcher != null)
            {
                _F0401Watcher.Dispose();
            }
            if (_A0101Watcher != null)
            {
                _A0101Watcher.Dispose();
            }
            if (_F0501Watcher != null)
            {
                _F0501Watcher.Dispose();
            }
            if (_G0401Watcher != null)
            {
                _G0401Watcher.Dispose();
            }
            if (_G0501Watcher != null)
            {
                _G0501Watcher.Dispose();
            }

        }

        public String ReportError()
        {
            StringBuilder sb = new StringBuilder();
            if (_AttachmentWatcher != null)
                sb.Append(_AttachmentWatcher.ReportError());
            if (_F0401Watcher != null)
                sb.Append(_F0401Watcher.ReportError());
            if (_A0101Watcher != null)
                sb.Append(_A0101Watcher.ReportError());
            if (_F0501Watcher != null)
                sb.Append(_F0501Watcher.ReportError());
            if (_G0401Watcher != null)
                sb.Append(_G0401Watcher.ReportError());
            if (_G0501Watcher != null)
                sb.Append(_G0501Watcher.ReportError());

            return sb.ToString();

        }

        public void SetRetry()
        {
            _AttachmentWatcher.Retry();
            _F0401Watcher.Retry();
            _A0101Watcher.Retry();
            _F0501Watcher.Retry();
            _G0401Watcher.Retry();
            _G0501Watcher.Retry();
        }



        public Type UIConfigType
        {
            get { return typeof(InvoiceClient.MainContent.MIGInvoiceConfig); }
        }

        private class LocalSettings
        {
            public string Attachment { get; set; } = "Attachment";
            public string F0401 { get; set; } = "F0401";
            public string F0501 { get; set; } = "F0501";
            public string A0101 { get; set; } = "A0101";
            public string A0501 { get; set; } = "A0501";
            public string G0401 { get; set; } = "G0401";
            public string G0501 { get; set; } = "G0501";
            public string B0401 { get; set; } = "B0401";
            public string B0501 { get; set; } = "B0501";
        }
    }
}
