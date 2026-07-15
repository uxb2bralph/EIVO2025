using System.Collections.Generic;
using System.IO;

namespace ModelCore.Properties
{
    /// <summary>
    /// 電子發票字軌號碼申請功能設定（遷移自 WebHome.Properties.InvoiceNumberApplySetting）。
    /// 置於共用模型層（ModelCore.EF），供 WebHome 與 TaskCenter 共用。
    /// 各路徑 / 檔名格式由使用端（AppSettings）於組態設定，本型別僅保存設定值與組合路徑之輔助方法。
    /// </summary>
    public class InvoiceNumberApplySetting
    {
        /// <summary>申請 JSON 檔存放的基底資料夾。</summary>
        public string ApplyFileBaseFolder { get; set; } = string.Empty;

        /// <summary>Word 範本資料夾（絕對路徑）。</summary>
        public string WordTemplateFolder { get; set; } = string.Empty;

        /// <summary>取得指定 Word 範本檔的完整路徑。</summary>
        public string GetWordTemplateFilePath(string wordFileName)
        {
            return Path.Combine(WordTemplateFolder, wordFileName);
        }

        /// <summary>歸檔（備份）資料夾。</summary>
        public string ApplyFileBackupFolder { get; set; } = string.Empty;

        /// <summary>申請 JSON 檔名格式（{0}=統一編號）。</summary>
        public string ApplyFileNameFormat { get; set; } = "{0}.json";
        public string GetApplyFileName(string businessId) { return string.Format(ApplyFileNameFormat, businessId); }
        public string GetApplyFilePath(string businessId) { return Path.Combine(ApplyFileBaseFolder, GetApplyFileName(businessId)); }

        /// <summary>Word 打包 zip 檔名格式（{0}=統一編號）。</summary>
        public string ZipFileNameFormat { get; set; } = "{0}.zip";
        public string GetZipFileName(string businessId) { return string.Format(ZipFileNameFormat, businessId); }
        public string GetZipFilePath(string businessId) { return Path.Combine(ApplyFileBaseFolder, GetZipFileName(businessId)); }

        /// <summary>申請 JSON 檔名比對規則（正規表示式）；null 表示不過濾。</summary>
        public string? ApplyFileNameFormatReg { get; set; }

        /// <summary>建立申請時是否寄送通知信。</summary>
        public bool NotifyEnable { get; set; }

        public IEnumerable<InvoiceNumberApplySysSupplier>? SysSupplier { get; set; }
        public IEnumerable<InvoiceNumberApplyPaperTestSet>? PaperTestSet { get; set; }
    }

    /// <summary>單一 Word 範本設定（遷移自 WebHome.Properties.InvoiceNumberApplyWordSetting）。</summary>
    public class InvoiceNumberApplyWordSetting
    {
        public string ID { get; set; }
        public string OutputName { get; set; }
        public string TemplateFileName { get; set; }

        public override string ToString()
        {
            return
                "ID=" + ID +
                "OutputName=" + OutputName +
                "TemplateFileName=" + TemplateFileName;
        }
    }

    /// <summary>感熱紙送驗報告預設組（遷移自 WebHome.Properties.InvoiceNumberApplyPaperTestSet）。</summary>
    public class InvoiceNumberApplyPaperTestSet
    {
        public string ID { get; set; }
        public string TestBusinessName { get; set; }
        public string SubmitBusinessName { get; set; }
        public string ReportNo { get; set; }
        public string PaperName { get; set; }
        public string PaperNo { get; set; }
        public string ReportDate { get; set; }
    }

    /// <summary>系統廠商預設組（遷移自 WebHome.Properties.InvoiceNumberApplySysSupplier）。</summary>
    public class InvoiceNumberApplySysSupplier
    {
        public string ID { get; set; }
        public string BusinessID { get; set; }
        public string No { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
    }
}
