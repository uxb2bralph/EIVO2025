using ApplicationResource;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Wordprocessing;
using WebHome.Models.ViewModel;
using WebHome.Properties;
using PdfSharp.Pdf.Content.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;

namespace WebHome.Models
{

    public class InvoiceNumberApply
    {
        public string PID { get; set; }

        [Required(ErrorMessage = "必輸欄位")]
        public string MOFBranch { get; set; }
        [Required(ErrorMessage = "必輸欄位")]
        public string TaxBranch { get; set; }
        #region 申請整合服務平台
        public bool ApplyUse { get; set; } = false;
        public int? ApplyUse_NeedNumber { get; set; } //取用1, 查詢2

        //[電子發票字軌號碼]及[配號方式]
        public bool ApplyNumber { get; set; } = false;
        public int? ApplyNumberNomal { get; set; } //一般1,特種2
        public ApplyNumberType ApplyNumberType { get; set; } = ApplyNumberType.None;
        public int? ApplyNumberQuantity { get; set; } = null;
        public int? NumberDuration { get; set; } = null; //期配1,年配2
        public int? GetNumberByYearReason { get; set; } = null; //[年配資格]
        public string GetNumberByYearReasonBusiness { get; set; } = string.Empty;    //[年配資格為連鎖加盟經營]

        //[停用整合服務平台功能]
        public bool StopUse { get; set; } = false;

        //[委任加值服務中心申請]
        public bool ApplyByAppoint { get; set; } = false;
        [Display(Name = "委任起日")]
        [DateLessThan("ApplyByAppointDateTo", ErrorMessage = "委任迄日不應小於委任起日")]
        public DateTime? ApplyByAppointDateFrom { get; set; }
        [Display(Name = "委任迄日")]
        public DateTime? ApplyByAppointDateTo { get; set; }
        public DateTime? ApplyDate { get; set; }
        #endregion

        //[委任專業代理人查詢下戴電子發票]
        #region 委任專業代理人查詢下戴電子發票
        public bool AppointToAgnet { get; set; } = false;
        [Display(Name = "委任專案代理起日")]
        [DateLessThan("AgentDateFrom", ErrorMessage = "委任代理迄日不應小於委任代理起日")]
        public DateTime? AgentDateFrom { get; set; }
        [Display(Name = "委任專業代理迄日")]
        public DateTime? AgentDateTo { get; set; }
        public DateTime? AgentDate { get; set; }
        #endregion

        public bool IsApplyB2B { get; set; }
        #region 僅開立B2B或B2G電子發票營業人文件
        //必備文件
        public bool b2bRequiredDoc1 { get; set; } = false;
        public bool b2bRequiredDoc2 { get; set; } = false;
        public bool b2bRequiredDoc3 { get; set; } = false;
        public bool b2bRequiredDoc4 { get; set; } = false;
        //傳輸方式
        public bool? b2bTransferDoc1 { get; set; } = null;
        public string b2bTransferDoc1TurnkeyToken { get; set; } = string.Empty;
        public bool? b2bTransferDoc2 { get; set; } = null;
        public string b2bTransferDoc2BusinessID { get; set; } = string.Empty;
        public string b2bTransferDoc2BusinessTaxID { get; set; } = string.Empty;
        public bool? b2bTransferDoc3 { get; set; } = null;
        //其他文件
        public bool? b2bDoc1 { get; set; } = null;
        public bool? b2bDoc2 { get; set; } = null;
        public bool? b2bDoc3 { get; set; } = null;
        public bool? b2bDoc4 { get; set; } = null;
        public bool? b2bDoc5 { get; set; } = null;
        public bool? b2bDoc6 { get; set; } = null; //一,委託專業代理人事務委任書
        #endregion
        public bool IsApplyB2C { get; set; }
        #region 同時開立b2c及b2c電子發票營業人文件
        //必備文件
        public bool b2cRequiredDoc1 { get; set; } = false;
        public bool b2cRequiredDoc2 { get; set; } = false;
        public bool b2cRequiredDoc3 { get; set; } = false;
        public bool b2cRequiredDoc4 { get; set; } = false;
        public bool b2cRequiredDoc5 { get; set; } = false;
        public bool b2cRequiredDoc6 { get; set; } = false;
        public bool b2cRequiredDoc8 { get; set; } = false; 
        public bool b2cRequiredDoc7 { get; set; } = false;
        //傳輸方式
        public bool? b2cTransferDoc1 { get; set; } = null;
        public string b2cTransferDoc1TurnkeyToken { get; set; } = string.Empty;
        public bool? b2cTransferDoc2 { get; set; } = null;
        public string b2cTransferDoc2BusinessID { get; set; } = string.Empty;
        public string b2cTransferDoc2BusinessTaxID { get; set; } = string.Empty;
        public bool? b2cTransferDoc3 { get; set; } = null;

        //其他文件
        public int? b2cDoc1Type { get; set; } = null;
        public string b2cDoc1Other { get; set; } = string.Empty;

        public bool? b2cDoc2 { get; set; } = null;
        public bool? b2cDoc3 { get; set; } = null;
        public bool? b2cDoc4 { get; set; } = null;
        public bool? b2cDoc5 { get; set; } = null;
        public bool? b2cDoc6 { get; set; } = null;
        public bool? b2cDoc7 { get; set; } = null;
        public bool? b2cDoc8 { get; set; } = null; //二,委託專業代理人事務委任書(書表6)
        #endregion

        #region 申請人
        [Required(ErrorMessage = "必輸欄位")]
        [Display(Name = "營業人名稱")]
        [RegularExpression(@"^[\u4E00-\u9FFF]{0,50}$", ErrorMessage = "限中文字或長度過長")]
        public string BusinessName { get; set; }
        [Required(ErrorMessage = "必輸欄位")]
        [Display(Name = "統一編號")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "8碼(半型)數字")]
        public string BusinessID { get; set; }
        [RegularExpression(@"^\d{9}$", ErrorMessage = "9碼(半型)數字")]
        [Display(Name = "稅籍編號")]
        public string BusinessTaxID { get; set; }
        [Required(ErrorMessage = "必輸欄位")]
        [RegularExpression(@"^[\u4E00-\u9FFFA-Za-z0-9-,]{0,40}$", ErrorMessage = "限中文英數字(半型)或長度過長")]
        [Display(Name = "營業地址")]
        public string BusinessAddr { get; set; }
        [Required(ErrorMessage = "必輸欄位")]
        [RegularExpression(@"^[\u4E00-\u9FFFA-Za-z-, ]{0,40}$", ErrorMessage = "限中英文或長度過長")]
        [Display(Name = "負責人姓名")]
        public string BusinessOwner { get; set; }
        [Required(ErrorMessage = "必輸欄位")]
        [RegularExpression(@"^[\u4E00-\u9FFFA-Za-z-, ]{0,40}$", ErrorMessage = "限中英文或長度過長")]
        [Display(Name = "聯絡人姓名")]
        public string BusinessContactName { get; set; }
        [Required(ErrorMessage = "必輸欄位")]
        [RegularExpression(@"09\d{2}(\d{6}|-\d{3}-\d{3})", ErrorMessage = "手機格式有誤")]
        [Display(Name = "手機號碼")]
        public string BusinessMobile { get; set; }
        [Required(ErrorMessage = "必輸欄位")]
        [RegularExpression(@"^(09)([0-9]{8})$|(\d{2,3}-{1}\d{3,4}\d{4})$", ErrorMessage = "格式有誤, 市話請輸入XX-XXXXXXXX 或 XXX-XXXXXXX")]
        [Display(Name = "聯絡電話")]
        public string BusinessTelNo { get; set; }
        [RegularExpression(@"\d{2,3}-{1}\d{3,4}\d{4}", ErrorMessage = "格式有誤, 請輸入XX-XXXXXXXX 或 XXX-XXXXXXX")]
        [Display(Name = "傳真號碼")]
        public string BusinessFaxNo { get; set; }
        [Required(ErrorMessage = "必輸欄位")]
        [RegularExpression(@"^[\u4E00-\u9FFFA-Za-z0-9-,]{0,40}$", ErrorMessage = "限中文英數字(半型)或長度過長")]
        [Display(Name = "通訊地址")]
        public string BusinessContactAddr { get; set; }
        [Required(ErrorMessage = "必輸欄位")]
        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "格式有誤")]
        [Display(Name = "聯絡電子郵件信箱")]
        public string BusinessEmail { get; set; }
        #endregion

        #region 委任專業代理
        [Display(Name = "專業代理人名稱")]
        [RegularExpression(@"^[\u4E00-\u9FFF-, ]{0,40}$", ErrorMessage = "限中文字或長度過長")]
        public string AgentName { get; set; }
        [Display(Name = "統一編號")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "限8碼數字")]
        public string AgentID { get; set; }
        [RegularExpression(@"^[\u4E00-\u9FFFA-Za-z-, ]{0,40}$", ErrorMessage = "限中英文或長度過長")]
        [Display(Name = "聯絡人姓名")]
        public string AgentContactName { get; set; }
        [RegularExpression(@"^(09)([0-9]{2})$|(\d{2,3}-{1}\d{3,4}\d{4})$", ErrorMessage = "格式有誤, 市話請輸入XX-XXXXXXXX 或 XXX-XXXXXXX")]
        [Display(Name = "聯絡人電話")]
        public string AgentTelNo { get; set; }
        public string AppointDuration { get; set; }
        [RegularExpression(@"^[\u4E00-\u9FFFA-Za-z-, ]{0,40}$", ErrorMessage = "限中英文或長度過長")]
        [Display(Name = "負責人")]
        public string AgentBoss { get; set; }
        #endregion

        #region 系統廠商
        [Display(Name = "系統廠商統一編號")]
        public string SysSupplierID { get; set; }
        [Display(Name = "系統廠商統一編號")]
        public string SysSupplierBusinessID { get; set; }
        [Display(Name = "型號")]
        public string SysSupplierNo { get; set; }
        [Display(Name = "系統名稱")]
        public string SysSupplierName { get; set; }
        [Display(Name = "系統版本")]
        public string SysSupplierVersion { get; set; }
        #endregion

        #region 感熱紙送驗報告
        [Display(Name = "檢測單位")]
        public string PaperTestID { get; set; }
        [Display(Name = "檢測單位")]
        public string TestBusinessName { get; set; }
        [Display(Name = "送驗廠商")]
        public string SubmitBusinessName { get; set; }
        [Display(Name = "報告號碼")]
        public string ReportNo { get; set; }
        [Display(Name = "產品名稱")]
        public string PaperName { get; set; }
        [Display(Name = "產品型號")]
        public string PaperNo { get; set; }
        [Display(Name = "報告日期")]
        public string ReportDate { get; set; }
        #endregion

        #region 發票開立系統檢測報告
        [RegularExpression(@"^[\u4E00-\u9FFFA-Za-z]{0,10}$", ErrorMessage = "限中英文或長度過長")]
        [Display(Name = "檢測人員職稱")]
        public string SysChkEmployeeTitle { get; set; }
        [RegularExpression(@"^[\u4E00-\u9FFFA-Za-z]{0,40}$", ErrorMessage = "限中英文或長度過長")]
        [Display(Name = "檢測人員姓名")]
        public string SysChkEmployeeName { get; set; }
        [Display(Name = "完成檢測日期")]
        public DateTime? SysChkDatetime { get; set; }
        [RegularExpression(@"^[\u4E00-\u9FFFA-Za-z0-9\。\，]{0,200}$", ErrorMessage = "限中英文(全形。或，)或長度過長")]
        [Display(Name = "檢測項目之檢測結果為「不具備」者，其原因詳述如下：")]
        public string SysChkMessage { get; set; }
        #endregion 

        #region 發票開立系統檢測項目
        public bool? SysChk11 { get; set; } = null;
        public bool? SysChk12 { get; set; }
        public bool? SysChk13 { get; set; }
        public bool? SysChk21 { get; set; }
        public bool? SysChk31 { get; set; }
        public bool? SysChk32 { get; set; }
        public bool? SysChk41 { get; set; }
        public bool? SysChk42 { get; set; }
        public bool? SysChk51 { get; set; }
        public bool? SysChk61 { get; set; }
        public bool? SysChk71 { get; set; }
        public bool? SysChk81 { get; set; }
        public bool? SysChk82 { get; set; }
        public bool? SysChk83 { get; set; }
        public bool? SysChk84 { get; set; }
        public bool? SysChk85 { get; set; }
        public bool? SysChk86 { get; set; }
        public bool? SysChk87 { get; set; }
        public bool? SysChk88 { get; set; }
        public bool? SysChk89 { get; set; }
        public bool? SysChk91 { get; set; }
        public bool? SysChk92 { get; set; }
        public bool? SysChk93 { get; set; }
        public bool? SysChk94 { get; set; }
        public bool? SysChk101 { get; set; }
        public bool? SysChk111 { get; set; }
        #endregion

        #region 取得測試資料
        public InvoiceNumberApply getTestData()
        {
            InvoiceNumberApply data = getDefaultData();
            data.MOFBranch = "台北";
            data.TaxBranch = "大安";
            data.ApplyUse = true;
            data.ApplyNumber = true;
            data.ApplyNumberNomal = 1;
            data.ApplyNumberType = ApplyNumberType.Apply;
            data.ApplyNumberQuantity = 300;
            data.NumberDuration = 1;
            data.ApplyByAppoint = true;
            data.IsApplyB2C = true;
            data.b2cRequiredDoc1 = true;
            data.b2cRequiredDoc2 = true;
            data.b2cRequiredDoc3 = true;
            data.b2cRequiredDoc4 = true;
            data.b2cRequiredDoc5 = true;
            data.b2cRequiredDoc6 = true;
            data.b2cRequiredDoc7 = true;
            data.b2cTransferDoc1 = false;
            data.b2cTransferDoc1TurnkeyToken = string.Empty;
            data.b2cTransferDoc2 = true;
            data.b2cTransferDoc2BusinessID = "70762419";
            data.b2cTransferDoc2BusinessTaxID = "101700372";
            data.b2cRequiredDoc8 = true;
            data.b2cDoc1Type = 3;
            data.b2cDoc1Other = "我是其他文件";
            data.b2cDoc2 = true;
            data.b2cDoc3 = true;
            data.b2cDoc4 = true;
            data.b2cDoc5 = false;
            data.b2cDoc6 = false;
            data.b2cDoc7 = true;
            data.b2cDoc8 = true;
            data.SysChk11 = true;
            data.SysChk12 = true;
            data.SysChk13 = true;
            data.SysChk21 = true;
            data.SysChk31 = true;
            data.SysChk32 = true;
            data.SysChk41 = true;
            data.SysChk42 = true;
            data.SysChk51 = true;
            data.SysChk61 = true;
            data.SysChk71 = true;
            data.SysChk81 = true;
            data.SysChk82 = true;
            data.SysChk83 = true;
            data.SysChk84 = true;
            data.SysChk85 = true;
            data.SysChk86 = true;
            data.SysChk87 = true;
            data.SysChk88 = true;
            data.SysChk89 = true;
            data.SysChk91 = true;
            data.SysChk92 = true;
            data.SysChk93 = true;
            data.SysChk94 = true;
            data.SysChk101 = true;
            data.SysChk111 = true;
            data.BusinessName = "有限公司";
            data.BusinessID = "88888888";
            data.BusinessTaxID = "999999999";
            data.BusinessAddr = "新北市永和區環河西路三段333巷333號3樓之3A室";
            data.BusinessOwner = "yoyo老闆";
            data.BusinessContactName = "yoyo老闆太太";
            data.BusinessMobile = "0999999999";
            data.BusinessTelNo = "02-99999999";
            data.BusinessFaxNo = "02-78888889";
            data.BusinessContactAddr = "新北市永和區環河西路四段555巷666號7樓之8A室";
            data.BusinessEmail = "yoyo@ttt.com";
            data.AgentName = "我是代理人";
            data.AgentID = "77777777";
            data.AgentContactName = "我是代理人的聯絡人";
            data.AgentBoss = "我是代理人的負責人";
            data.AgentTelNo = "02-55555555";
            data.SysChkEmployeeTitle = "專員";
            data.SysChkEmployeeName = "我是檢測人員";
            data.SysChkDatetime = DateTime.Now.Date;
            data.SysChkMessage = "檢測完成沒有問題";
            data.ApplyByAppointDateFrom = DateTime.Now.Date;
            data.ApplyByAppointDateTo = DateTime.Now.Date.AddYears(10);
            return data;
        }
        #endregion

        #region 取得號碼申請樣版
        public InvoiceNumberApply getTemplateNumberApplyData()
        {
            InvoiceNumberApply data = getDefaultData();
            data.ApplyUse = true;
            data.ApplyNumber = true;
            data.ApplyNumberNomal = 1;
            data.ApplyNumberType = ApplyNumberType.Apply;
            data.ApplyNumberQuantity = 300;
            data.NumberDuration = 1;
            data.ApplyByAppoint = true;
            data.IsApplyB2C = true;
            data.b2cRequiredDoc1 = true;
            data.b2cRequiredDoc2 = true;
            data.b2cRequiredDoc3 = true;
            data.b2cRequiredDoc4 = true;
            data.b2cRequiredDoc5 = true;
            data.b2cTransferDoc1 = false;
            data.b2cTransferDoc1TurnkeyToken = string.Empty;
            data.b2cTransferDoc2 = true;
            data.b2cTransferDoc2BusinessID = "70762419";
            data.b2cTransferDoc2BusinessTaxID = "101700372";
            data.b2cRequiredDoc8 = true;
            data.b2cDoc1Type = 1;
            data.b2cDoc2 = true;
            data.b2cDoc3 = true;
            data.b2cDoc4 = true;
            data.b2cDoc5 = false;
            data.b2cDoc6 = false;
            data.b2cDoc7 = true;
            data.SysChk11 = true;
            data.SysChk12 = true;
            data.SysChk13 = true;
            data.SysChk21 = true;
            data.SysChk31 = true;
            data.SysChk32 = true;
            data.SysChk41 = true;
            data.SysChk42 = true;
            data.SysChk51 = true;
            data.SysChk61 = true;
            data.SysChk71 = true;
            data.SysChk81 = true;
            data.SysChk82 = true;
            data.SysChk83 = true;
            data.SysChk84 = true;
            data.SysChk85 = true;
            data.SysChk86 = true;
            data.SysChk87 = true;
            data.SysChk88 = true;
            data.SysChk89 = true;
            data.SysChk91 = true;
            data.SysChk92 = true;
            data.SysChk93 = true;
            data.SysChk94 = true;
            data.SysChk101 = true;
            data.SysChk111 = true;

            return data;
        }

        public InvoiceNumberApply getDefaultData()
        {
            return new InvoiceNumberApply(
            PID = string.Empty,
            MOFBranch = string.Empty,
            TaxBranch = string.Empty,
            ApplyUse = false,
            ApplyUse_NeedNumber = null,
            ApplyNumber = false,
            ApplyNumberNomal = null,
            ApplyNumberType = ApplyNumberType.None,
            ApplyNumberQuantity = null,
            NumberDuration = null,
            GetNumberByYearReason = null,
            GetNumberByYearReasonBusiness = string.Empty,
            StopUse = false,
            ApplyByAppoint = false,
            ApplyByAppointDateFrom = null,
            ApplyByAppointDateTo = null,
            IsApplyB2B = false,
            b2bRequiredDoc1 = false,
            b2bRequiredDoc2 = false,
            b2bRequiredDoc3 = false,
            b2bRequiredDoc4 = false,
            b2bTransferDoc1 = null,
            b2bTransferDoc1TurnkeyToken = string.Empty,
            b2bTransferDoc2 = null,
            b2bTransferDoc2BusinessID = string.Empty,
            b2bTransferDoc2BusinessTaxID = string.Empty,
            b2bTransferDoc3 = null,
            b2bDoc1 = null,
            b2bDoc2 = null,
            b2bDoc3 = null,
            b2bDoc4 = null,
            b2bDoc5 = null,
            IsApplyB2C = false,
            b2cRequiredDoc1 = false,
            b2cRequiredDoc2 = false,
            b2cRequiredDoc3 = false,
            b2cRequiredDoc4 = false,
            b2cRequiredDoc5 = false,
            b2cTransferDoc1 = false,
            b2cTransferDoc1TurnkeyToken = string.Empty,
            b2cTransferDoc2 = false,
            b2cTransferDoc2BusinessID = string.Empty,
            b2cTransferDoc2BusinessTaxID = string.Empty,
            b2cTransferDoc3 = false,
            b2cRequiredDoc8 = false,
            b2cDoc1Type = null,
            b2cDoc1Other = string.Empty,
            b2cDoc2 = null,
            b2cDoc3 = null,
            b2cDoc4 = null,
            b2cDoc5 = null,
            b2cDoc6 = null,
            b2cDoc7 = null,
            BusinessName = string.Empty,
            BusinessID = string.Empty,
            BusinessTaxID = string.Empty,
            BusinessAddr = string.Empty,
            BusinessOwner = string.Empty,
            BusinessContactName = string.Empty,
            BusinessMobile = string.Empty,
            BusinessTelNo = string.Empty,
            BusinessFaxNo = string.Empty,
            BusinessContactAddr = string.Empty,
            BusinessEmail = string.Empty,
            AgentName = string.Empty,
            AgentID = string.Empty,
            AgentContactName = string.Empty,
            AgentTelNo = string.Empty,
            SysSupplierBusinessID = string.Empty,
            SysSupplierNo = string.Empty,
            SysSupplierName = string.Empty,
            SysSupplierVersion = string.Empty,
            TestBusinessName = string.Empty,
            SubmitBusinessName = string.Empty,
            ReportNo = string.Empty,
            PaperName = string.Empty,
            PaperNo = string.Empty,
            SysChkEmployeeTitle = string.Empty,
            SysChkEmployeeName = string.Empty,
            SysChkDatetime = null,
            SysChkMessage = string.Empty,
            SysChk11 = null,
            SysChk12 = null,
            SysChk13 = null,
            SysChk21 = null,
            SysChk31 = null,
            SysChk32 = null,
            SysChk41 = null,
            SysChk42 = null,
            SysChk51 = null,
            SysChk61 = null,
            SysChk71 = null,
            SysChk81 = null,
            SysChk82 = null,
            SysChk83 = null,
            SysChk84 = null,
            SysChk85 = null,
            SysChk86 = null,
            SysChk87 = null,
            SysChk88 = null,
            SysChk89 = null,
            SysChk91 = null,
            SysChk92 = null,
            SysChk93 = null,
            SysChk94 = null,
            SysChk101 = null,
            SysChk111 = null


        );
        }

        public InvoiceNumberApply getTestDataWithWrongFormat()
        {
            InvoiceNumberApply applyViewModel = getTestData();
            applyViewModel.BusinessName = "123";
            applyViewModel.BusinessName = "yoyo有限公司";
            applyViewModel.BusinessID = "8888888";
            applyViewModel.BusinessTaxID = "999999999";
            applyViewModel.BusinessAddr = "新北市永和區環河西路三段333巷333號3樓之3A室新北市永和區環河西路三段333巷333號3樓之3A室";
            applyViewModel.BusinessOwner = "李圭歸瑰規硅閨邽龜鮭魚於瑜餘娛虞盂妤漁愚愉於余蝓腴予輿渝嵎榆算了我想得好累隨便啦啦";
            applyViewModel.BusinessContactName = "李圭歸瑰規硅閨邽龜鮭魚於瑜餘娛虞盂妤漁愚愉於余蝓腴予輿渝嵎榆算了我想得好累隨便啦啦";
            applyViewModel.BusinessMobile = "099999999";
            applyViewModel.BusinessTelNo = "0299999999";
            applyViewModel.BusinessFaxNo = "0298888889";
            applyViewModel.BusinessContactAddr = "新北市永和區環河西路四段555巷666號7樓之8A室新北市永和區環河西路三段333巷333號3樓之3A室";
            applyViewModel.BusinessEmail = "yoyottt.com";
            applyViewModel.AgentName = "李圭歸瑰規硅閨邽龜鮭魚於瑜餘娛虞盂妤漁愚愉於余蝓腴予輿渝嵎榆算了我想得好累隨便啦啦"; ;
            applyViewModel.AgentID = "7777777";
            applyViewModel.AgentContactName = "李圭歸瑰規硅閨邽龜鮭魚於瑜餘娛虞盂妤漁愚愉於余蝓腴予輿渝嵎榆算了我想得好累隨便啦啦"; ;
            applyViewModel.AgentTelNo = "0255555555";
            return applyViewModel;
        }
        #endregion


        public InvoiceNumberApply(string pID, string mOFBranch, string taxBranch, bool applyUse
            , int? applyUse_NeedNumber, bool applyNumber, int? applyNumberNomal, ApplyNumberType applyNumberType
            , int? applyNumberQuantity, int? isGetNumberByYear, int? getNumberByYearReason
            , string getNumberByYearReasonBusiness, bool stopUse, bool applyByAppoint, DateTime? applyByAppointDateFrom
            , DateTime? applyByAppointDateTo, bool isApplyB2B, bool b2bRequiredDoc1, bool b2bRequiredDoc2, bool b2bRequiredDoc3, bool b2bRequiredDoc4, bool? b2bTransferDoc1, string b2bTransferDoc1TurnkeyToken, bool? b2bTransferDoc2, string b2bTransferDoc2BusinessID, string b2bTransferDoc2BusinessTaxID, bool? b2bTransferDoc3, bool? b2bDoc1, bool? b2bDoc2, bool? b2bDoc3, bool? b2bDoc4, bool? b2bDoc5, bool isApplyB2C, bool b2cRequiredDoc1, bool b2cRequiredDoc2, bool b2cRequiredDoc3, bool b2cRequiredDoc4, bool b2cRequiredDoc5, bool? b2cTransferDoc1, string b2cTransferDoc1TurnkeyToken, bool? b2cTransferDoc2, string b2cTransferDoc2BusinessID, string b2cTransferDoc2BusinessTaxID, bool? b2cTransferDoc3, bool? b2cDoc1, int? b2cDoc1Type, string b2cDoc1Other, bool? b2cDoc2, bool? b2cDoc3, bool? b2cDoc4, bool? b2cDoc5, bool? b2cDoc6, bool? b2cDoc7, string businessName, string businessID, string businessTaxID, string businessAddr, string businessOwner, string businessContactName, string businessMobile, string businessTelNo, string businessFaxNo, string businessContactAddr, string businessEmail, string agentName, string agentID, string agentContactName, string agentTelNo, string sysSupplierBusinessID, string sysSupplierNo, string sysSupplierName, string sysSupplierVersion, string testBusinessName, string submitBusinessName, string reportNo, string paperName, string paperNo, string sysChkEmployeeTitle, string sysChkEmployeeName, DateTime? sysChkDatetime, string sysChkMessage, bool? sysChk11, bool? sysChk12, bool? sysChk13, bool? sysChk21, bool? sysChk31, bool? sysChk32, bool? sysChk41, bool? sysChk42, bool? sysChk51, bool? sysChk61, bool? sysChk71, bool? sysChk81, bool? sysChk82, bool? sysChk83, bool? sysChk84, bool? sysChk85, bool? sysChk86, bool? sysChk87, bool? sysChk88, bool? sysChk89, bool? sysChk91, bool? sysChk92, bool? sysChk93, bool? sysChk94, bool? sysChk101, bool? sysChk111)
        {
            PID = pID;
            MOFBranch = mOFBranch;
            TaxBranch = taxBranch;
            ApplyUse = applyUse;
            ApplyUse_NeedNumber = applyUse_NeedNumber;
            ApplyNumber = applyNumber;
            ApplyNumberNomal = applyNumberNomal;
            ApplyNumberType = applyNumberType;
            ApplyNumberQuantity = applyNumberQuantity;
            NumberDuration = isGetNumberByYear;
            GetNumberByYearReason = getNumberByYearReason;
            GetNumberByYearReasonBusiness = getNumberByYearReasonBusiness;
            StopUse = stopUse;
            ApplyByAppoint = applyByAppoint;
            ApplyByAppointDateFrom = applyByAppointDateFrom;
            ApplyByAppointDateTo = applyByAppointDateTo;
            IsApplyB2B = isApplyB2B;
            this.b2bRequiredDoc1 = b2bRequiredDoc1;
            this.b2bRequiredDoc2 = b2bRequiredDoc2;
            this.b2bRequiredDoc3 = b2bRequiredDoc3;
            this.b2bRequiredDoc4 = b2bRequiredDoc4;
            this.b2bTransferDoc1 = b2bTransferDoc1;
            this.b2bTransferDoc1TurnkeyToken = b2bTransferDoc1TurnkeyToken;
            this.b2bTransferDoc2 = b2bTransferDoc2;
            this.b2bTransferDoc2BusinessID = b2bTransferDoc2BusinessID;
            this.b2bTransferDoc2BusinessTaxID = b2bTransferDoc2BusinessTaxID;
            this.b2bTransferDoc3 = b2bTransferDoc3;
            this.b2bDoc1 = b2bDoc1;
            this.b2bDoc2 = b2bDoc2;
            this.b2bDoc3 = b2bDoc3;
            this.b2bDoc4 = b2bDoc4;
            this.b2bDoc5 = b2bDoc5;
            IsApplyB2C = isApplyB2C;
            this.b2cRequiredDoc1 = b2cRequiredDoc1;
            this.b2cRequiredDoc2 = b2cRequiredDoc2;
            this.b2cRequiredDoc3 = b2cRequiredDoc3;
            this.b2cRequiredDoc4 = b2cRequiredDoc4;
            this.b2cRequiredDoc5 = b2cRequiredDoc5;
            this.b2cTransferDoc1 = b2cTransferDoc1;
            this.b2cTransferDoc1TurnkeyToken = b2cTransferDoc1TurnkeyToken;
            this.b2cTransferDoc2 = b2cTransferDoc2;
            this.b2cTransferDoc2BusinessID = b2cTransferDoc2BusinessID;
            this.b2cTransferDoc2BusinessTaxID = b2cTransferDoc2BusinessTaxID;
            this.b2cTransferDoc3 = b2cTransferDoc3;
            this.b2cRequiredDoc8 = b2cRequiredDoc8;
            this.b2cDoc1Type = b2cDoc1Type;
            this.b2cDoc1Other = b2cDoc1Other;
            this.b2cDoc2 = b2cDoc2;
            this.b2cDoc3 = b2cDoc3;
            this.b2cDoc4 = b2cDoc4;
            this.b2cDoc5 = b2cDoc5;
            this.b2cDoc6 = b2cDoc6;
            this.b2cDoc7 = b2cDoc7;
            BusinessName = businessName;
            BusinessID = businessID;
            BusinessTaxID = businessTaxID;
            BusinessAddr = businessAddr;
            BusinessOwner = businessOwner;
            BusinessContactName = businessContactName;
            BusinessMobile = businessMobile;
            BusinessTelNo = businessTelNo;
            BusinessFaxNo = businessFaxNo;
            BusinessContactAddr = businessContactAddr;
            BusinessEmail = businessEmail;
            AgentName = agentName;
            AgentID = agentID;
            AgentContactName = agentContactName;
            AgentTelNo = agentTelNo;
            SysSupplierBusinessID = sysSupplierBusinessID;
            SysSupplierNo = sysSupplierNo;
            SysSupplierName = sysSupplierName;
            SysSupplierVersion = sysSupplierVersion;
            TestBusinessName = testBusinessName;
            SubmitBusinessName = submitBusinessName;
            ReportNo = reportNo;
            PaperName = paperName;
            PaperNo = paperNo;
            SysChkEmployeeTitle = sysChkEmployeeTitle;
            SysChkEmployeeName = sysChkEmployeeName;
            SysChkDatetime = sysChkDatetime;
            SysChkMessage = sysChkMessage;
            SysChk11 = sysChk11;
            SysChk12 = sysChk12;
            SysChk13 = sysChk13;
            SysChk21 = sysChk21;
            SysChk31 = sysChk31;
            SysChk32 = sysChk32;
            SysChk41 = sysChk41;
            SysChk42 = sysChk42;
            SysChk51 = sysChk51;
            SysChk61 = sysChk61;
            SysChk71 = sysChk71;
            SysChk81 = sysChk81;
            SysChk82 = sysChk82;
            SysChk83 = sysChk83;
            SysChk84 = sysChk84;
            SysChk85 = sysChk85;
            SysChk86 = sysChk86;
            SysChk87 = sysChk87;
            SysChk88 = sysChk88;
            SysChk89 = sysChk89;
            SysChk91 = sysChk91;
            SysChk92 = sysChk92;
            SysChk93 = sysChk93;
            SysChk94 = sysChk94;
            SysChk101 = sysChk101;
            SysChk111 = sysChk111;
        }

        public InvoiceNumberApply()
        {
        }

        public override string ToString()
        {
            return
            "MOFBranch=" + MOFBranch + ", " +
            "TaxBranch=" + TaxBranch + ", " +
            "ApplyUse=" + ApplyUse + ", " +
            "ApplyUse_NeedNumber=" + ApplyUse_NeedNumber + ", " +
            "ApplyNumber=" + ApplyNumber + ", " +
            "ApplyNumberNomal=" + ApplyNumberNomal + ", " +
            "ApplyNumberType=" + ApplyNumberType + ", " +
            "ApplyNumberQuantity=" + ApplyNumberQuantity + ", " +
            "IsGetNumberByYear=" + NumberDuration + ", " +
            "GetNumberByYearReason=" + GetNumberByYearReason + ", " +
            "GetNumberByYearReasonBusiness=" + GetNumberByYearReasonBusiness + ", " +
            "StopUse=" + StopUse + ", " +
            "ApplyByAppoint=" + ApplyByAppoint + ", " +
            "ApplyByAppointDateFrom=" + ApplyByAppointDateFrom + ", " +
            "ApplyByAppointDateTo=" + ApplyByAppointDateTo + ", " +
            "b2bRequiredDoc1=" + this.b2bRequiredDoc1 + ", " +
            "b2bRequiredDoc2=" + this.b2bRequiredDoc2 + ", " +
            "b2bRequiredDoc3=" + this.b2bRequiredDoc3 + ", " +
            "b2bRequiredDoc4=" + this.b2bRequiredDoc4 + ", " +
            "b2bTransferDoc1=" + this.b2bTransferDoc1 + ", " +
            "b2bTransferDoc1TurnkeyToken=" + this.b2bTransferDoc1TurnkeyToken + ", " +
            "b2bTransferDoc2=" + this.b2bTransferDoc2 + ", " +
            "b2bTransferDoc2BusinessID=" + this.b2bTransferDoc2BusinessID + ", " +
            "b2bTransferDoc2BusinessTaxID=" + this.b2bTransferDoc2BusinessTaxID + ", " +
            "b2bTransferDoc3=" + this.b2bTransferDoc3 + ", " +
            "b2bDoc1=" + this.b2bDoc1 + ", " +
            "b2bDoc2=" + this.b2bDoc2 + ", " +
            "b2bDoc3=" + this.b2bDoc3 + ", " +
            "b2bDoc4=" + this.b2bDoc4 + ", " +
            "b2bDoc5=" + this.b2bDoc5 + ", " +
            "b2cRequiredDoc1=" + this.b2cRequiredDoc1 + ", " +
            "b2cRequiredDoc2=" + this.b2cRequiredDoc2 + ", " +
            "b2cRequiredDoc3=" + this.b2cRequiredDoc3 + ", " +
            "b2cRequiredDoc4=" + this.b2cRequiredDoc4 + ", " +
            "b2cRequiredDoc5=" + this.b2cRequiredDoc5 + ", " +
            "b2cTransferDoc1=" + this.b2cTransferDoc1 + ", " +
            "b2cTransferDoc1TurnkeyToken=" + this.b2cTransferDoc1TurnkeyToken + ", " +
            "b2cTransferDoc2=" + this.b2cTransferDoc2 + ", " +
            "b2cTransferDoc2BusinessID=" + this.b2cTransferDoc2BusinessID + ", " +
            "b2cTransferDoc2BusinessTaxID=" + this.b2cTransferDoc2BusinessTaxID + ", " +
            "b2cTransferDoc3=" + this.b2cTransferDoc3 + ", " +
            "b2cRequiredDoc8=" + this.b2cRequiredDoc8 + ", " +
            "b2cDoc1Other=" + this.b2cDoc1Other + ", " +
            "b2cDoc2=" + this.b2cDoc2 + ", " +
            "b2cDoc3=" + this.b2cDoc3 + ", " +
            "b2cDoc4=" + this.b2cDoc4 + ", " +
            "b2cDoc5=" + this.b2cDoc5 + ", " +
            "b2cDoc6=" + this.b2cDoc6 + ", " +
            "b2cDoc7=" + this.b2cDoc7 + ", " +
            "BusinessName=" + BusinessName + ", " +
            "BusinessID=" + BusinessID + ", " +
            "BusinessTaxID=" + BusinessTaxID + ", " +
            "BusinessAddr=" + BusinessAddr + ", " +
            "BusinessOwner=" + BusinessOwner + ", " +
            "BusinessContactName=" + BusinessContactName + ", " +
            "BusinessMobile=" + BusinessMobile + ", " +
            "BusinessTelNo=" + BusinessTelNo + ", " +
            "BusinessFaxNo=" + BusinessFaxNo + ", " +
            "BusinessContactAddr=" + BusinessContactAddr + ", " +
            "BusinessEmail=" + BusinessEmail + ", " +
            "AgentName=" + AgentName + ", " +
            "AgentID=" + AgentID + ", " +
            "AgentContactName=" + AgentContactName + ", " +
            "AgentTelNo=" + AgentTelNo + ", " +
            "IsApplyB2C=" + IsApplyB2C + ", " +
            "IsApplyB2B=" + IsApplyB2B;
        }
    }




}