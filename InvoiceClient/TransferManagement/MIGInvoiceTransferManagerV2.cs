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
        private InvoiceWatcher _C0401Watcher;
        private InvoiceWatcher _C0501Watcher;
        private InvoiceWatcher _D0401Watcher;
        private InvoiceWatcher _A0401Watcher;
        private InvoiceWatcher _A0501Watcher;
        private InvoiceWatcher _B0401Watcher;
        private InvoiceWatcher _D0501Watcher;
        private InvoiceWatcher _B0501Watcher;
        private LocalSettings _Settings;
        public ITabWorkItem WorkItem { get; set; }

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

            _C0401Watcher = new C0401Watcher(Path.Combine(fullPath, _Settings.C0401));
            _C0401Watcher.StartUp();

            _A0401Watcher = new A0401Watcher(Path.Combine(fullPath, _Settings.A0401));
            _A0401Watcher.StartUp();

            _C0501Watcher = new C0501Watcher(Path.Combine(fullPath, _Settings.C0501));
            _C0501Watcher.StartUp();

            _A0501Watcher = new A0501Watcher(Path.Combine(fullPath, _Settings.A0501));
            _A0501Watcher.StartUp();

            _D0401Watcher = new Agent.MIGHelper.D0401Watcher(Path.Combine(fullPath, _Settings.D0401));
            _D0401Watcher.StartUp();

            _B0401Watcher = new B0401Watcher(Path.Combine(fullPath, _Settings.B0401));
            _B0401Watcher.StartUp();

            _D0501Watcher = new Agent.MIGHelper.D0501Watcher(Path.Combine(fullPath, _Settings.D0501));
            _D0501Watcher.StartUp();

            _B0501Watcher = new Agent.MIGHelper.B0501Watcher(Path.Combine(fullPath, _Settings.B0501));
            _B0501Watcher.StartUp();
        }

        public void PauseAll()
        {
            if (_AttachmentWatcher != null)
            {
                _AttachmentWatcher.Dispose();
            }
            if (_C0401Watcher != null)
            {
                _C0401Watcher.Dispose();
            }
            if (_A0401Watcher != null)
            {
                _A0401Watcher.Dispose();
            }
            if (_C0501Watcher != null)
            {
                _C0501Watcher.Dispose();
            }
            if (_A0501Watcher != null)
            {
                _A0501Watcher.Dispose();
            }
            if (_D0401Watcher != null)
            {
                _D0401Watcher.Dispose();
            }
            if (_B0401Watcher != null)
            {
                _B0401Watcher.Dispose();
            }
            if (_D0501Watcher != null)
            {
                _D0501Watcher.Dispose();
            }
            if (_B0501Watcher != null)
            {
                _B0501Watcher.Dispose();
            }
        }

        public String ReportError()
        {
            StringBuilder sb = new StringBuilder();
            if (_AttachmentWatcher != null)
                sb.Append(_AttachmentWatcher.ReportError());
            if (_C0401Watcher != null)
                sb.Append(_C0401Watcher.ReportError());
            if (_A0401Watcher != null)
                sb.Append(_A0401Watcher.ReportError());
            if (_C0501Watcher != null)
                sb.Append(_C0501Watcher.ReportError());
            if (_A0501Watcher != null)
                sb.Append(_A0501Watcher.ReportError());
            if (_D0401Watcher != null)
                sb.Append(_D0401Watcher.ReportError());
            if (_B0401Watcher != null)
                sb.Append(_B0401Watcher.ReportError());
            if (_D0501Watcher != null)
                sb.Append(_D0501Watcher.ReportError());
            if (_B0501Watcher != null)
                sb.Append(_B0501Watcher.ReportError());

            return sb.ToString();

        }

        public void SetRetry()
        {
            _AttachmentWatcher.Retry();
            _C0401Watcher.Retry();
            _A0401Watcher.Retry();
            _C0501Watcher.Retry();
            _A0501Watcher.Retry();
            _D0401Watcher.Retry();
            _B0401Watcher.Retry();
            _D0501Watcher.Retry();
            _B0501Watcher.Retry();
        }



        public Type UIConfigType
        {
            get { return typeof(InvoiceClient.MainContent.MIGInvoiceConfig); }
        }

        private class LocalSettings
        {
            public string Attachment { get; set; } = "Attachment";
            public string C0401 { get; set; } = "C0401";
            public string C0501 { get; set; } = "C0501";
            public string A0401 { get; set; } = "A0401";
            public string A0501 { get; set; } = "A0501";
            public string D0401 { get; set; } = "D0401";
            public string D0501 { get; set; } = "D0501";
            public string B0401 { get; set; } = "B0401";
            public string B0501 { get; set; } = "B0501";
        }
    }
}
