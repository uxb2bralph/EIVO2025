using CommonLib.Utility.Properties;
using ModelCore.Locale;
using ModelCore.Schema.TXN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceClient.Properties
{
    public partial class AppSettings : AppSettingsBase
    {
        static AppSettings()
        {
            _default = Initialize<AppSettings>(typeof(AppSettings).Namespace);
        }

        public AppSettings() : base()
        {

        }

        static AppSettings _default;
        public static AppSettings Default
        {
            get
            {
                return _default;
            }
        }

        public static void Reload()
        {
            Reload<AppSettings>(ref _default, typeof(AppSettings).Namespace);
        }

        public bool WatchSubDirectories { get; set; } = false;
        public ServiceInfo? @ServiceInfo { get; set; }
        public bool InstalledService { get; set; } = false;
        public bool UseMainForm { get; set; } = true;
        public String InvoiceViewUrlPattern { get; set; } = "http://localhost:5000/DataView/ShowInvoice?PrintCuttingLine=True&PaperStyle=A4&UseCustomView=True&ProcessType=C0401&DocID={0}";
        public String[] InvoiceTxnPath { get; set; } = ["C:\\UXB2B_EIVO"];
        public bool UseFolderProcessor { get; set; } = false;
        public string ActivationKey { get; set; }
        public string AppCulture { get; set; } = "en-US";
        public string AppTitle { get; set; } = "Electronic Invoice Transmission Service-Store Client";
        public int AutoInvServiceInterval { get; set; } = 30;
        public bool AutoRetry { get; set; } = false;
        public int AutoWelfareInterval { get; set; } = 30;
        public string B2BCounterpartBusinessFolder { get; set; } = "B2B相對營業人";
        public string B2BUploadAllowanceCancellationFolder { get; set; } = "B2BCancelAllowance";
        public string B2BUploadAllowanceFolder { get; set; } = "B2BAllowance";
        public string B2BUploadBuyerInvoiceFolder { get; set; } = "BuyerInvoice";
        public string B2BUploadInvoiceCancellationFolder { get; set; } = "B2BCancelInvoice";
        public string B2BUploadInvoiceFolder { get; set; } = "B2BSellerInvoice";
        public string B2BUploadReceiptCancellationFolder { get; set; } = "CancelReceipt";
        public string B2BUploadReceiptFolder { get; set; } = "Receipt";
        public bool ClearTxnPath { get; set; } = false;
        public string ClientID { get; set; }
        public string ConvertDataToAllowance { get; set; } = "http://localhost:5000/DataView/ConvertDataToAllowance";
        public string CsvEncoding { get; set; } = "Big5";
        public string DisplayName { get; set; } = "電子發票用戶端傳輸服務(EIVO03)";
        public string DownloadAllowanceCancellationFolder { get; set; } = "傳送大平台資料\\作廢折讓";
        public string DownloadAllowanceFolder { get; set; } = "傳送大平台資料\\折讓";
        public string DownloadDataFolder { get; set; } = "傳送大平台資料";
        public bool DownloadDataInAbsolutePath { get; set; } = false;
        public string DownloadInvoiceCancellationFolder { get; set; } = "傳送大平台資料\\作廢發票";
        public string DownloadInvoiceFolder { get; set; } = "傳送大平台資料\\發票";
        public string DownloadInvoiceMailTracking { get; set; } = "InvoiceMailTracking";
        public string DownloadInvoiceMapping { get; set; } = "InvoiceMap";
        public string DownloadInvoiceReturnedMail { get; set; } = "ReturnedMail";
        public string DownloadSaleInvoiceFolder { get; set; } = "InvoicePDF";
        public string DownloadWinningFolder { get; set; } = "BonusInvoice";
        public bool EnableB2BAutoReceiving { get; set; } = false;
        public bool EnableFailedUploadAlert { get; set; } = false;
        public int FileWatcherProcessCount { get; set; } = 0;
        public string InvoicePDFZipPrefix { get; set; } = "taiwan_uxb2b_scanned_gui_pdf_";
        public bool IsAutoInvService { get; set; } = true;
        public bool IsAutoWelfare { get; set; } = false;
        public string MainTabs { get; set; } = "InvoiceClient.MainContent.SystemConfigTab, InvoiceClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;";
        public int MaxFileCountInPDFZip { get; set; } = 1024;
        public int MaxWaitingTurns { get; set; } = 1;
        public string OutputEncoding { get; set; } = "utf-8";
        public bool OutputEncodingWithoutBOM { get; set; } = false;
        public int PackerCycleDelayInSeconds { get; set; } = 20;
        public string PDFGeneratorOutput { get; set; } = "E:\\UXB2B_EIVO_GOOGLE\\ZipInvoice";
        public int PGPWaitingForExitInMilliSeconds { get; set; } = 30000;
        public int ProcessArrayCount { get; set; } = 0;
        public int ProcessArrayIndex { get; set; } = 0;
        public int ProcessCount { get; set; } = 0;
        public bool ResponseUpload { get; set; } = true;
        public bool RetryCancellationWhenInvoiceNotFound { get; set; } = false;
        public bool RetryOnConnectException { get; set; } = false;
        public string RootCA { get; set; } = "UXB2B Certificate Center.cer";
        public string SellerReceiptNo { get; set; }
        public string ServerInspector { get; set; } = "InvoiceClient.Agent.VacantInvoiceNoInspector, InvoiceClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null; InvoiceClient.Agent.InvoiceMappingInspector, InvoiceClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null; InvoiceClient.Agent.B2BInvoiceInspector, InvoiceClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null; InvoiceClient.Agent.InvoiceMailTrackingInspector, InvoiceClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null; InvoiceClient.Agent.InvoicePDFInspector, InvoiceClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null; InvoiceClient.Agent.AllowancePDFGenerator, InvoiceClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null; InvoiceClient.Agent.InvoiceServerInspector, InvoiceClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;";
        public string ServiceName { get; set; } = "EIVO03ClientService(Product)";
        public string SignerCspName { get; set; }
        public string SignerKeyPassword { get; set; }
        public string SignerSubjectName { get; set; } = "UXSigner";
        public string TrackCodeFolder { get; set; } = "InvoiceTrackCode";
        public string TransferManager { get; set; } = "InvoiceClient.TransferManagement.XlsxInvoiceTransferManager, InvoiceClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;";
        public string UploadAllowanceCancellationFolder { get; set; } = "CancelAllowance";
        public string UploadAllowanceFolder { get; set; } = "Allowance";
        public string UploadAttachment { get; set; } = "http://localhost:5000/Published/UploadAttachmentForGoogle.ashx";
        public string UploadAttachmentFolder { get; set; } = "Attachment";
        public string UploadBranchTrackFolder { get; set; } = "InvoiceNoAssignment";
        public string UploadCsvAllowanceCancellationFolder { get; set; } = "CancelAllowance_csv";
        public string UploadCsvAllowanceFolder { get; set; } = "Allowance_csv";
        public string UploadCsvBuyerFolder { get; set; } = "B2B相對營業人_csv";
        public string UploadCsvInvoiceCancellationFolder { get; set; } = "CancelInvoice_csv";
        public string UploadCsvInvoiceFolder { get; set; } = "Invoice_csv";
        public string UploadCustomerCsvInvoiceFolder { get; set; } = "InvoiceFull_csv";
        public string UploadInvoiceCancellationFolder { get; set; } = "CancelInvoice";
        public string UploadInvoiceEnterprise { get; set; } = "Enterprise";
        public string UploadInvoiceFolder { get; set; } = "Invoice";
        public string UploadPreInvoiceFolder { get; set; } = "PreInvoice";
        public string UploadSellerInvoiceFolder { get; set; } = "SellerInvoice";
        public string VacantInvoiceNoFolder { get; set; } = "VacantInvoiceNo";
        public int WaitForInvoicePDFInSeconds { get; set; } = 300;
        public int WatcherProcessDelayInSeconds { get; set; } = 5;
        public string WelfareInfoFolder { get; set; } = "SWA";
        public int WS_TimeoutInMilliSeconds { get; set; } = 300000;
        public string ZipInvoice { get; set; } = "ZipInvoice";
        public string InvoiceClient_WS_Invoice_eInvoiceServiceClient { get; set; } = "http://localhost:5000/api/InvoiceService";
    }
}
