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
using InvoiceClient.Agent.MIGReviser;
using InvoiceClient.Helper;

namespace InvoiceClient.TransferManagement
{
    public class MIGReviserManager : ITransferManager
    {
        private InvoiceWatcher? _F0401Watcher;
        private InvoiceWatcher? _F0501Watcher;
        private InvoiceWatcher? _G0401Watcher;
        private InvoiceWatcher? _G0501Watcher;
        private InvoiceWatcher? _F0701Watcher;
        private LocalSettings _Settings;
        public ITabWorkItem? WorkItem { get; set; }

        public MIGReviserManager()
        {
            string path = Path.Combine(Logger.LogPath, "MIGReviser.json");
            if (File.Exists(path))
            {
                this._Settings = JsonConvert.DeserializeObject<LocalSettings>(File.ReadAllText(path))!;
            }
            else
            {
                this._Settings = new LocalSettings();
            }
            File.WriteAllText(path, JsonConvert.SerializeObject((object)this._Settings!));
        }
        public void EnableAll(String fullPath)
        {
            _F0401Watcher = new F0401Watcher(Path.Combine(fullPath, _Settings.F0401));
            _F0401Watcher.StartUp();

            _F0701Watcher = new F0701Watcher(Path.Combine(fullPath, _Settings.F0701));
            _F0701Watcher.StartUp();

            _F0501Watcher = new F0501Watcher(Path.Combine(fullPath, _Settings.F0501));
            _F0501Watcher.StartUp();

            _G0401Watcher = new G0401Watcher(Path.Combine(fullPath, _Settings.G0401));
            _G0401Watcher.StartUp();

            _G0501Watcher = new G0501Watcher(Path.Combine(fullPath, _Settings.G0501));
            _G0501Watcher.StartUp();

        }

        public void PauseAll()
        {
            if (_F0401Watcher != null)
            {
                _F0401Watcher.Dispose();
            }
            if (_F0701Watcher != null)
            {
                _F0701Watcher.Dispose();
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
            if (_F0401Watcher != null)
                sb.Append(_F0401Watcher.ReportError());
            if (_F0701Watcher != null)
                sb.Append(_F0701Watcher.ReportError());
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
            _F0401Watcher!.Retry();
            _F0701Watcher!.Retry();
            _F0501Watcher!.Retry();
            _G0401Watcher!.Retry();
            _G0501Watcher!.Retry();
        }

        public Type UIConfigType
        {
            get { return typeof(InvoiceClient.MainContent.MIGInvoiceConfig); }
        }

        private class LocalSettings
        {
            public string F0401 { get; set; } = "F0401";
            public string F0501 { get; set; } = "F0501";
            public string F0701 { get; set; } = "F0701";
            public string G0401 { get; set; } = "G0401";
            public string G0501 { get; set; } = "G0501";
        }
    }
}
