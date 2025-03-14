using WebHome.Helper;
using WebHome.Models.ViewModel;
using WebHome.Properties;

using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using CommonLib.Core.Utility;
using CommonLib.Utility;

namespace WebHome.Models
{
    public class InvoiceNumberApplyService
    {
        public string BusinessId { get; set; }
        public InvoiceNumberApplyWordSetting WordSet { get; }
        public PropertyInfo[] WordPropInfo => WordModel.GetType().GetProperties();
        public InvoiceNumberApplyWord WordModel { get; } = new InvoiceNumberApplyWord();

        public InvoiceNumberApply apply { get; }
        public string GetOutputFileName() => WordSet.OutputName;

        public InvoiceNumberApplyService(
            InvoiceNumberApply applyView
            , InvoiceNumberApplyWordSetting wordSet)
        {
            this.apply = applyView;
            this.WordSet = wordSet;
        }

        public InvoiceNumberApplyService(string businessId)
        {
            this.BusinessId = businessId;
        }

        public InvoiceNumberApplyService() { }

        public IEnumerable<FileInfo> GetApplyJsonFilesInfo()
        {
            DirectoryInfo di = new DirectoryInfo(AppSettings.Default.InvoiceNumberApplySetting.ApplyFileBaseFolder);

            if (!di.Exists) { di.Create(); }

            IEnumerable<FileInfo> fileInfos
                = di.GetFiles("*")
                    .Where(m => new Regex(AppSettings.Default.InvoiceNumberApplySetting.ApplyFileNameFormatReg)
                                    .IsMatch(m.Name));

            if (!string.IsNullOrEmpty(this.BusinessId))
                return fileInfos.Where(m => m.Name.Contains(this.BusinessId)).ToList();

            return fileInfos;
        }

        public IEnumerable<String> GetApplyJsonFiles()
        {
            var items = Directory.EnumerateFiles(AppSettings.Default.InvoiceNumberApplySetting.ApplyFileBaseFolder, "*.json", SearchOption.AllDirectories);

            if (AppSettings.Default.InvoiceNumberApplySetting.ApplyFileNameFormatReg != null)
            {
                items = items.Where(m => Regex.IsMatch(m, AppSettings.Default.InvoiceNumberApplySetting.ApplyFileNameFormatReg));
            }

            this.BusinessId = this.BusinessId.GetEfficientString();
            if (this.BusinessId != null)
            {
                items = items.Where(m => m.Contains(this.BusinessId));
            }

            return items;
        }

        public string GetReplacedWordXmlString()
        {
            Dictionary<string, bool> checkedList
                = getRadioButtonAndCheckedBoxCondition(apply);

            //若條件為true, checked radiobutton or checkbox
            foreach (var checkedItem in checkedList)
            {
                if (checkedItem.Value)
                {
                    WordPropInfo
                        .Where(x => x.Name == checkedItem.Key)
                        .FirstOrDefault()
                        .SetValue(WordModel
                            , (checkedItem.Key.StartsWith("c")) ? InvoiceNumberApplyWord.CheckedBox : InvoiceNumberApplyWord.CheckedRadioButton);
                }
            }
            //電子發票字軌號碼申請
            WordModel.ta00 = apply.MOFBranch;
            WordModel.ta01 = apply.TaxBranch;

            if (apply.ApplyNumberQuantity != null)
            {
                if (apply.ApplyNumberNomal == 1)
                    WordModel.tb20 = apply.ApplyNumberQuantity.ToString();
                if (apply.ApplyNumberNomal == 2)
                    WordModel.tb21 = apply.ApplyNumberQuantity.ToString();
            }
            WordModel.tb30 = apply.GetNumberByYearReasonBusiness;
            WordModel.td00 = apply.BusinessName;
            WordModel.td01 = apply.BusinessID;
            WordModel.td02 = apply.BusinessTaxID;
            WordModel.td03 = apply.BusinessAddr;
            WordModel.td04 = apply.BusinessOwner;
            WordModel.td10 = apply.BusinessContactName;
            WordModel.td11 = apply.BusinessMobile;
            WordModel.td12 = apply.BusinessTelNo;
            WordModel.td13 = apply.BusinessFaxNo;
            WordModel.td14 = apply.BusinessContactAddr;
            WordModel.td15 = apply.BusinessEmail;
            //事務所(代理人)
            WordModel.td20 = apply.AgentName; //專業代理人名稱
            WordModel.td21 = apply.AgentID;//專業代理人統一編號
            WordModel.td22 = apply.AgentContactName;//專業代理人聯絡人
            WordModel.td23 = apply.AgentTelNo;//專業代理人聯絡人電話
            WordModel.td28 = apply.AgentBoss;//專業代理人負責人
            WordModel.td24 = (apply.AgentDateFrom == null) ? string.Empty : apply.AgentDateFrom?.ToFullTaiwanDate();
            WordModel.td26 = (apply.AgentDateFrom == null) ? string.Empty : apply.AgentDateFrom?.ToYYYMMTaiwanDate();
            WordModel.td25 = (apply.AgentDateTo == null) ? string.Empty : apply.AgentDateTo?.ToFullTaiwanDate();
            WordModel.td27 = (apply.AgentDateTo == null) ? string.Empty : apply.AgentDateTo?.ToYYYMMTaiwanDate();
            //僅開立B2B或B2G電子發票營業人-Turnkey上線通行碼
            WordModel.tc10 = apply.b2bTransferDoc1TurnkeyToken;
            //僅開立B2B或B2G電子發票營業人-代傳統編
            WordModel.tc30 = apply.b2bTransferDoc2BusinessID;
            //僅開立B2B或B2G電子發票營業人-代傳稅籍
            WordModel.tc31 = apply.b2bTransferDoc2BusinessTaxID;
            //委認期間
            WordModel.td35 = (apply.ApplyByAppointDateFrom == null) ? string.Empty : apply.ApplyByAppointDateFrom?.ToFullTaiwanDate();
            WordModel.td37 = (apply.ApplyByAppointDateFrom == null) ? string.Empty : apply.ApplyByAppointDateFrom?.ToYYYMMTaiwanDate();
            WordModel.td36 = (apply.ApplyByAppointDateTo == null) ? string.Empty : apply.ApplyByAppointDateTo?.ToFullTaiwanDate();
            WordModel.td38 = (apply.ApplyByAppointDateTo == null) ? string.Empty : apply.ApplyByAppointDateTo?.ToYYYMMTaiwanDate();
            WordModel.td40 = (apply.ApplyDate != null) ? apply.ApplyDate?.ToFullTaiwanDate() : (System.DateTime.Now).ToFullTaiwanDate();
            WordModel.tc40 = apply.b2cTransferDoc1TurnkeyToken;
            WordModel.tc41 = apply.b2cTransferDoc2BusinessID;
            WordModel.tc42 = apply.b2cTransferDoc2BusinessTaxID;
            WordModel.tc67 = apply.b2cDoc1Other;
            //系統廠商
            WordModel.tf00 = apply.SysSupplierBusinessID;
            WordModel.tf01 = apply.SysSupplierNo;
            WordModel.tf02 = apply.SysSupplierName;
            WordModel.tf03 = apply.SysSupplierVersion;
            //發票開立系統檢測作業
            WordModel.tf04 = apply.SysChkEmployeeTitle;
            WordModel.tf05 = apply.SysChkEmployeeName;
            WordModel.tf06 = apply.SysChkMessage;
            WordModel.tf07 = apply.SysChkDatetime?.ToFullTaiwanDate();

            //感熱紙送驗報告
            WordModel.tg00 = apply.TestBusinessName;
            WordModel.tg01 = apply.SubmitBusinessName;
            WordModel.tg02 = apply.ReportNo;
            WordModel.tg03 = apply.PaperName;
            WordModel.tg04 = apply.PaperNo;
            WordModel.tg05 = apply.ReportDate;

            XmlDocument doc = new XmlDocument();
            doc.Load(AppSettings.Default.InvoiceNumberApplySetting.GetWordTemplateFilePath(WordSet.TemplateFileName));
            StringBuilder sb = new StringBuilder(doc.OuterXml);

            foreach (var prop in WordPropInfo)
            {
                sb.Replace(prop.Name, prop.GetValue(WordModel)?.ToString());
            }

            return sb.ToString();
        }

        public InvoiceNumberApply GetApplyViewModelFromJson()
        {
            InvoiceNumberApply item = null;

            try
            {
                string filePath = AppSettings.Default.InvoiceNumberApplySetting.GetApplyFilePath(this.BusinessId);
                item = filePath.DeserializeObjectFromFile<InvoiceNumberApply>();
            }
            catch (FileNotFoundException ex)
            {
                Logger.Warn("businessID=" + this.BusinessId + ";  " + ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.Message);
            }


            return item;
        }

        private static Dictionary<string, bool> getRadioButtonAndCheckedBoxCondition(
            InvoiceNumberApply item)
        {
            Dictionary<string, bool> checkedList
                = new Dictionary<string, bool>() { };

            checkedList.Add("cb10", item.ApplyUse == true);
            checkedList.Add("rb10", item.ApplyUse_NeedNumber == 1);
            checkedList.Add("rb11", item.ApplyUse_NeedNumber == 2);
            checkedList.Add("cb20", item.ApplyNumberNomal == 1);
            checkedList.Add("rb20", (item.ApplyNumberNomal == 1) && item.ApplyNumberType == ApplyNumberType.Apply);
            checkedList.Add("rb21", (item.ApplyNumberNomal == 1) && item.ApplyNumberType == ApplyNumberType.Increase);
            checkedList.Add("rb22", (item.ApplyNumberNomal == 1) && item.ApplyNumberType == ApplyNumberType.Deduct);
            checkedList.Add("rb23", (item.ApplyNumberNomal == 1) && item.ApplyNumberType == ApplyNumberType.Zero);
            checkedList.Add("cb21", (item.ApplyNumberNomal == 2));
            checkedList.Add("rb24", (item.ApplyNumberNomal == 2) && item.ApplyNumberType == ApplyNumberType.Apply);
            checkedList.Add("rb25", (item.ApplyNumberNomal == 2) && item.ApplyNumberType == ApplyNumberType.Increase);
            checkedList.Add("rb26", (item.ApplyNumberNomal == 2) && item.ApplyNumberType == ApplyNumberType.Deduct);
            checkedList.Add("rb27", (item.ApplyNumberNomal == 2) && item.ApplyNumberType == ApplyNumberType.Zero);
            checkedList.Add("cb30", item.NumberDuration == 1);
            checkedList.Add("cb31", item.NumberDuration == 2);
            checkedList.Add("rb30", item.GetNumberByYearReason == 1);
            checkedList.Add("rb31", item.GetNumberByYearReason == 2);
            checkedList.Add("rb32", item.GetNumberByYearReason == 3);
            checkedList.Add("rb33", item.GetNumberByYearReason == 4);
            checkedList.Add("rb34", item.GetNumberByYearReason == 5);
            checkedList.Add("cb40", item.StopUse);
            checkedList.Add("cb50", item.ApplyByAppoint);
            checkedList.Add("cb51", item.AppointToAgnet);
            checkedList.Add("cc00", item.b2bRequiredDoc1);
            checkedList.Add("cc01", item.b2bRequiredDoc2);
            checkedList.Add("cc02", item.b2bRequiredDoc3);
            checkedList.Add("cc03", item.b2bRequiredDoc4);
            checkedList.Add("cc11", item.b2bTransferDoc1 == true);
            checkedList.Add("cc12", item.b2bTransferDoc1 == false);
            checkedList.Add("cc15", item.b2bTransferDoc2 == true);
            checkedList.Add("cc16", item.b2bTransferDoc2 == false);
            checkedList.Add("cc17", item.b2bTransferDoc3 == true);
            checkedList.Add("cc18", item.b2bTransferDoc3 == false);
            checkedList.Add("cc20", item.b2bDoc1 == true);
            checkedList.Add("cc21", item.b2bDoc1 == false);
            checkedList.Add("cc22", item.b2bDoc2 == true);
            checkedList.Add("cc23", item.b2bDoc2 == false);
            checkedList.Add("cc24", item.b2bDoc3 == true);
            checkedList.Add("cc25", item.b2bDoc3 == false);
            checkedList.Add("cc26", item.b2bDoc4 == true);
            checkedList.Add("cc27", item.b2bDoc4 == false);
            checkedList.Add("cc28", item.b2bDoc5 == true);
            checkedList.Add("cc29", item.b2bDoc5 == false);
            //一,委託專業代理人事務委任書
            checkedList.Add("cc64", item.b2bDoc6 == true);
            checkedList.Add("cc65", item.b2bDoc6 == false);

            checkedList.Add("cc30", item.b2cRequiredDoc1);
            checkedList.Add("cc31", item.b2cRequiredDoc2);
            checkedList.Add("cc32", item.b2cRequiredDoc3);
            checkedList.Add("cc33", item.b2cRequiredDoc4);
            checkedList.Add("cc34", item.b2cRequiredDoc5);
            checkedList.Add("cc68", item.b2cRequiredDoc6);
            checkedList.Add("cc50", item.b2cRequiredDoc8);
            checkedList.Add("cc69", item.b2cRequiredDoc7);
            checkedList.Add("cc40", item.b2cTransferDoc1 == true);
            checkedList.Add("cc41", item.b2cTransferDoc1 == false);
            checkedList.Add("cc42", item.b2cTransferDoc2 == true);
            checkedList.Add("cc43", item.b2cTransferDoc2 == false);

            checkedList.Add("rc64", item.b2cDoc1Type == 1);
            checkedList.Add("rc65", item.b2cDoc1Type == 2);
            checkedList.Add("rc66", item.b2cDoc1Type == 3);
            checkedList.Add("cc52", item.b2cDoc2 == true);
            checkedList.Add("cc53", item.b2cDoc2 == false);
            checkedList.Add("cc54", item.b2cDoc3 == true);
            checkedList.Add("cc55", item.b2cDoc3 == false);
            checkedList.Add("cc56", item.b2cDoc4 == true);
            checkedList.Add("cc57", item.b2cDoc4 == false);
            checkedList.Add("cc58", item.b2cDoc5 == true);
            checkedList.Add("cc59", item.b2cDoc5 == false);
            checkedList.Add("cc60", item.b2cDoc6 == true);
            checkedList.Add("cc61", item.b2cDoc6 == false);
            checkedList.Add("cc62", item.b2cDoc7 == true);
            checkedList.Add("cc63", item.b2cDoc7 == false);
            checkedList.Add("cc66", item.b2cDoc8 == true);//二,委託專業代理人事務委任書(書表6)
            checkedList.Add("cc67", item.b2cDoc8 == false);
            checkedList.Add("ce00", item.SysChk11 == true);
            checkedList.Add("ce01", item.SysChk11 == false);
            checkedList.Add("ce02", item.SysChk12 == true);
            checkedList.Add("ce03", item.SysChk12 == false);
            checkedList.Add("ce04", item.SysChk13 == true);
            checkedList.Add("ce05", item.SysChk13 == false);
            checkedList.Add("ce06", item.SysChk21 == true);
            checkedList.Add("ce07", item.SysChk21 == false);
            checkedList.Add("ce10", item.SysChk31 == true);
            checkedList.Add("ce11", item.SysChk31 == false);
            checkedList.Add("ce12", item.SysChk32 == true);
            checkedList.Add("ce13", item.SysChk32 == false);
            checkedList.Add("ce14", item.SysChk41 == true);
            checkedList.Add("ce15", item.SysChk41 == false);
            checkedList.Add("ce16", item.SysChk42 == true);
            checkedList.Add("ce17", item.SysChk42 == false);
            checkedList.Add("ce18", item.SysChk51 == true);
            checkedList.Add("ce19", item.SysChk51 == false);
            checkedList.Add("ce20", item.SysChk61 == true);
            checkedList.Add("ce21", item.SysChk61 == false);
            checkedList.Add("ce22", item.SysChk71 == true);
            checkedList.Add("ce23", item.SysChk71 == false);
            checkedList.Add("ce30", item.SysChk81 == true);
            checkedList.Add("ce31", item.SysChk81 == false);
            checkedList.Add("ce32", item.SysChk82 == true);
            checkedList.Add("ce33", item.SysChk82 == false);
            checkedList.Add("ce34", item.SysChk83 == true);
            checkedList.Add("ce35", item.SysChk83 == false);
            checkedList.Add("ce36", item.SysChk84 == true);
            checkedList.Add("ce37", item.SysChk84 == false);
            checkedList.Add("ce38", item.SysChk85 == true);
            checkedList.Add("ce39", item.SysChk85 == false);
            checkedList.Add("ce40", item.SysChk86 == true);
            checkedList.Add("ce41", item.SysChk86 == false);
            checkedList.Add("ce42", item.SysChk87 == true);
            checkedList.Add("ce43", item.SysChk87 == false);
            checkedList.Add("ce44", item.SysChk88 == true);
            checkedList.Add("ce45", item.SysChk88 == false);
            checkedList.Add("ce46", item.SysChk89 == true);
            checkedList.Add("ce47", item.SysChk89 == false);
            checkedList.Add("ce50", item.SysChk91 == true);
            checkedList.Add("ce51", item.SysChk91 == false);
            checkedList.Add("ce52", item.SysChk92 == true);
            checkedList.Add("ce53", item.SysChk92 == false);
            checkedList.Add("ce54", item.SysChk93 == true);
            checkedList.Add("ce55", item.SysChk93 == false);
            checkedList.Add("ce56", item.SysChk94 == true);
            checkedList.Add("ce57", item.SysChk94 == false);
            checkedList.Add("ce58", item.SysChk101 == true);
            checkedList.Add("ce59", item.SysChk101 == false);
            checkedList.Add("ce60", item.SysChk111 == true);
            checkedList.Add("ce61", item.SysChk111 == false);
            return checkedList;
        }

        public OrganizationViewModel ApplyConvertedOrganization(InvoiceNumberApply apply)
        {
            Organization item = new Organization();
            item.ReceiptNo = apply.BusinessID;
            item.OrganizationStatus = new OrganizationStatus();
            item.CompanyName = apply.BusinessName;
            item.Addr = apply.BusinessAddr;
            item.Phone = apply.BusinessTelNo;
            item.Fax = apply.BusinessFaxNo;
            item.UndertakerName = apply.BusinessOwner;
            item.ContactName = apply.BusinessContactName;
            item.ContactPhone = apply.BusinessTelNo;
            item.ContactMobilePhone = apply.BusinessMobile;
            item.ContactEmail = apply.BusinessEmail;
            OrganizationViewModel organizationViewModel = new OrganizationViewModel();
            if (apply.ApplyNumberNomal == 1)
            {
                item.OrganizationStatus.SettingInvoiceType = (int?)Naming.InvoiceTypeDefinition.一般稅額計算之電子發票;
            }
            if (apply.ApplyNumberNomal == 2)
            {
                item.OrganizationStatus.SettingInvoiceType = (int?)Naming.InvoiceTypeDefinition.特種稅額計算之電子發票;
            }

            organizationViewModel.ApplyFromModel(item);
            return organizationViewModel;
        }

        //public static void moveJsonFile(string businessID)
        //{

        //    FileInfo fileInfo
        //         = new InvoiceNumberApplyService(businessID).GetApplyJsonFilesInfo().FirstOrDefault();

        //    fileInfo.MoveTo(string.Format("{0}\\{1}.{2}"
        //        , AppSettings.Default.InvoiceNumberApplySetting.ApplyFileBackupFolder
        //        , fileInfo.Name
        //        , System.DateTime.Now.ToString("yyyyMMddHHmmss")));
        //}

        public static void MoveJsonFile(string applyFilePath)
        {
            if (File.Exists(applyFilePath))
            {
                String target = Path.Combine(AppSettings.Default.InvoiceNumberApplySetting.ApplyFileBackupFolder,
                    $"{Path.GetFileName(applyFilePath)}.{DateTime.Now:yyyyMMddHHmmss}");
                File.Move(applyFilePath, target); ;
            }

        }
    }
}