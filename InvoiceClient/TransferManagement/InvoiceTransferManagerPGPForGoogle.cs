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
    public class InvoiceTransferManagerPGPForGoogle : ITransferManager
    {

        private InvoiceWatcher _PreInvoiceWatcher;
        private InvoiceWatcher _PGPResponseWatcher;
        private InvoiceWatcher _PDFPGPResponseWatcher;
        private InvoiceCancellationWatcherV2ForGoogle _CancellationWatcher;
        private AllowanceWatcherV2ForGoogle _AllowanceWatcher;
        private InvoiceWatcher _AllowancePGPResponseWatcher;
        private AllowanceContentToPDFWatcher _AllowanceContentWatcher;
        private AllowanceCancellationWatcherV2ForGoogle _AllowanceCancellationWatcher;
        private InvoiceWatcher _AttachmentWatcher;
        private InvoiceWatcher _InvoicePDFWatcher;

        private InvoiceTransferManagerPGPForGoogle.LocalSettings _Settings;
        public ITabWorkItem WorkItem { get; set; }

        public InvoiceTransferManagerPGPForGoogle()
        {
            string path = Path.Combine(Logger.LogPath, $"{this.GetType().Name}.json");
            if (File.Exists(path))
            {
                this._Settings = JsonConvert.DeserializeObject<InvoiceTransferManagerPGPForGoogle.LocalSettings>(File.ReadAllText(path));
            }
            else
            {
                this._Settings = new InvoiceTransferManagerPGPForGoogle.LocalSettings();
                File.WriteAllText(path, JsonConvert.SerializeObject((object)this._Settings));
            }
        }

        public void EnableAll(String fullPath)
        {
            _PreInvoiceWatcher = new InvoicePGPWatcherForGoogleExpress(Path.Combine(fullPath, Settings.Default.UploadPreInvoiceFolder));
            _PreInvoiceWatcher.StartUp();

            _PGPResponseWatcher = new PGPEncryptWatcherForGoogle(Path.Combine(fullPath, "ResponseForPGP"))
            {
                ResponsePath = Path.Combine(fullPath, Settings.Default.UploadPreInvoiceFolder + "(Response)")
            };
            _PGPResponseWatcher.StartUp();

            _PDFPGPResponseWatcher = new PGPEncryptWatcherForGoogle(Path.Combine(fullPath, "ZipPDFForPGP"))
            {
                ResponsePath = Path.Combine(Settings.Default.InvoiceTxnPath[0], Settings.Default.DownloadSaleInvoiceFolder)
            };
            _PDFPGPResponseWatcher.StartUp();

            _CancellationWatcher = new InvoiceCancellationWatcherV2ForGoogle(Path.Combine(fullPath, Settings.Default.UploadInvoiceCancellationFolder));
            _CancellationWatcher.StartUp();

            _AllowanceWatcher = new AllowanceWatcherV2ForGoogle(Path.Combine(fullPath, Settings.Default.UploadAllowanceFolder));
            _AllowanceWatcher.StartUp();

            _AllowancePGPResponseWatcher = new PGPEncryptWatcherForGoogle(_AllowanceWatcher.ResponsePath)
            {
                ResponsePath = Path.Combine(fullPath, Settings.Default.UploadAllowanceFolder + "(Response)"),
                AddedStore = _Settings.AllowancePGPStore,
            };
            _AllowancePGPResponseWatcher.StartUp();


            _AllowanceContentWatcher = new AllowanceContentToPDFWatcher(_AllowanceWatcher.PathToPDF)
            {
                ResponsePath = Path.Combine(Settings.Default.InvoiceTxnPath[0], Settings.Default.DownloadSaleInvoiceFolder)
            };
            _AllowanceContentWatcher.StartUp();

            _AllowanceCancellationWatcher = new AllowanceCancellationWatcherV2ForGoogle(Path.Combine(fullPath, Settings.Default.UploadAllowanceCancellationFolder));
            _AllowanceCancellationWatcher.StartUp();

            _AttachmentWatcher = new InvoiceAttachmentWatcherForGoogle(Path.Combine(fullPath, Settings.Default.UploadAttachmentFolder));
            _AttachmentWatcher.StartUp();

            _InvoicePDFWatcher = new InvoicePDFWatcherForZip(Path.Combine(fullPath, Settings.Default.ZipInvoice))
            {
                ResponsePath = Path.Combine(fullPath, "ZipPDFForPGP")
            };
            _InvoicePDFWatcher.StartUp();


            _CancellationWatcher.InitializeDependency(_PreInvoiceWatcher);
            _AllowanceWatcher.InitializeDependency(_PreInvoiceWatcher);
            _AllowanceCancellationWatcher.InitializeDependency(_AllowanceWatcher);
            _AttachmentWatcher.InitializeDependency(_PreInvoiceWatcher);

        }

        public void PauseAll()
        {
            if (_PreInvoiceWatcher != null)
            {
                _PreInvoiceWatcher.Dispose();
            }
            if (_PGPResponseWatcher != null)
            {
                _PGPResponseWatcher.Dispose();
            }
            if (_PDFPGPResponseWatcher != null)
            {
                _PDFPGPResponseWatcher.Dispose();
            }
            if (_CancellationWatcher != null)
            {
                _CancellationWatcher.Dispose();
            }
            if (_AllowanceWatcher != null)
            {
                _AllowanceWatcher.Dispose();
            }
            if (_AllowancePGPResponseWatcher != null)
            {
                _AllowancePGPResponseWatcher.Dispose();
            }

            if (_AllowanceContentWatcher != null)
            {
                _AllowanceContentWatcher.Dispose();
            }

            if (_AllowanceCancellationWatcher != null)
            {
                _AllowanceCancellationWatcher.Dispose();
            }
            if (_AttachmentWatcher != null)
            {
                _AttachmentWatcher.Dispose();
            }
            if (_InvoicePDFWatcher != null)
            {
                _InvoicePDFWatcher.Dispose();
            }

        }

        public String ReportError()
        {
            StringBuilder sb = new StringBuilder();
            if (_PreInvoiceWatcher != null)
                sb.Append(_PreInvoiceWatcher.ReportError());
            if (_CancellationWatcher != null)
                sb.Append(_CancellationWatcher.ReportError());
            if (_AllowanceWatcher != null)
                sb.Append(_AllowanceWatcher.ReportError());
            if (_AllowanceCancellationWatcher != null)
                sb.Append(_AllowanceCancellationWatcher.ReportError());
            if (_AttachmentWatcher != null)
                sb.Append(_AttachmentWatcher.ReportError());
            return sb.ToString();

        }

        public void SetRetry()
        {
            _PreInvoiceWatcher.Retry();
            _CancellationWatcher.Retry();
            _AllowanceWatcher.Retry();
            _AllowanceCancellationWatcher.Retry();
            _AttachmentWatcher.Retry();
        }



        public Type UIConfigType
        {
            get { return typeof(InvoiceClient.MainContent.GoogleInvoiceConfig); }
        }

        private class LocalSettings
        {
            public String AllowancePGPStore { get; set; }
        }
    }
}
