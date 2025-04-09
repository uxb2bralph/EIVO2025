using ApplicationResource;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Wordprocessing;
using WebHome.Helper.DataQuery;
using WebHome.Properties;

using PdfSharp.Pdf.Content.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;

namespace WebHome.Models.ViewModel
{

    public class InvoiceNumberApplyWord
    {
        public static string UnCheckedBox = "□";
        public static string CheckedBox = "■";
        public static string UnCheckedRadioButton = "○";
        public static string CheckedRadioButton = "●";

        //ta00
        //t:text(r:radiobutton c:checkbox)
        //a:受理機關(b:申請項目 c:檢核文件 d:資料填寫)
        //0:區塊
        //0:流水號

        #region 受理機關

        public string ta00 { get; set; } = string.Empty; //MOFBranch
        public string ta01 { get; set; } = string.Empty; //TaxBranch
        #endregion

        #region 申請項目-[申請整合服務平台]
        
        public string cb10 { get; set; } = UnCheckedBox; //ApplyUse=true
        public string rb10 { get; set; } = UnCheckedRadioButton; //ApplyUse_NeedNumber=true
        public string rb11 { get; set; } = UnCheckedRadioButton; //ApplyUse_NeedNumber=false
        #endregion

        #region 申請項目-[電子發票字軌號碼]及[配號方式]
        //一般稅額計算
        public string cb20 { get; set; } = UnCheckedBox; //ApplyNumberNomal=true
        public string rb20 { get; set; } = UnCheckedRadioButton; //ApplyNumberType=apply
        public string rb21 { get; set; } = UnCheckedRadioButton; //ApplyNumberType=increase
        public string rb22 { get; set; } = UnCheckedRadioButton; //ApplyNumberType=deduct
        public string rb23 { get; set; } = UnCheckedRadioButton; //ApplyNumberType=zero
        public string tb20 { get; set; } = string.Empty; //ApplyNumberQuantity
        //特種稅額計算
        public string cb21 { get; set; } = UnCheckedBox; //ApplyNumberNomal=false
        public string rb24 { get; set; } = UnCheckedRadioButton; //ApplyNumberType=apply
        public string rb25 { get; set; } = UnCheckedRadioButton; //ApplyNumberType=increase
        public string rb26 { get; set; } = UnCheckedRadioButton; //ApplyNumberType=deduct
        public string rb27 { get; set; } = UnCheckedRadioButton; //ApplyNumberType=zero
        public string tb21 { get; set; } = string.Empty; //ApplyNumberQuantity
        public string cb30 { get; set; } = UnCheckedBox; //IsGetNumberByYear=false
        public string cb31 { get; set; } = UnCheckedBox; //IsGetNumberByYear=true
        public string rb30 { get; set; } = UnCheckedRadioButton; //GetNumberByYearReason=1
        public string rb31 { get; set; } = UnCheckedRadioButton; //GetNumberByYearReason=2
        public string rb32 { get; set; } = UnCheckedRadioButton; //GetNumberByYearReason=3
        public string rb33 { get; set; } = UnCheckedRadioButton; //GetNumberByYearReason=4
        public string rb34 { get; set; } = UnCheckedRadioButton; //GetNumberByYearReason=5
        public string tb30 { get; set; } = string.Empty; //GetNumberByYearReasonBusiness
        #endregion

        #region 申請項目-停用整合服務平台功能/委任加值服務中心申請
        public string cb40 { get; set; } = UnCheckedBox;
        public string cb50 { get; set; } = UnCheckedBox;
        public string cb51 { get; set; } = UnCheckedBox;

        #endregion

        #region 僅開立B2B或B2G電子發票營業人文件
        public string cc00 { get; set; } = UnCheckedBox; //b2bRequiredDoc1
        public string cc01 { get; set; } = UnCheckedBox;
        public string cc02 { get; set; } = UnCheckedBox;
        public string cc03 { get; set; } = UnCheckedBox;
        public string tc10 { get; set; } = UnCheckedBox; //b2bTransferDoc1TurnkeyToken
        public string cc11 { get; set; } = UnCheckedBox; //b2bTransferDoc1=true
        public string cc12 { get; set; } = UnCheckedBox; //b2bTransferDoc1=false
        public string tc13 { get; set; } = UnCheckedBox; //b2bTransferDoc2BusinessID
        public string tc14 { get; set; } = UnCheckedBox; //b2bTransferDoc2BusinessTaxID
        public string cc15 { get; set; } = UnCheckedBox; //b2bTransferDoc2=true
        public string cc16 { get; set; } = UnCheckedBox; //b2bTransferDoc2=false
        public string cc17 { get; set; } = UnCheckedBox;//b2bTransferDoc3=true
        public string cc18 { get; set; } = UnCheckedBox;//b2bTransferDoc3=false
        public string cc20 { get; set; } = UnCheckedBox;//b2bDoc1=true
        public string cc21 { get; set; } = UnCheckedBox;//b2bDoc1=false
        public string cc22 { get; set; } = UnCheckedBox;//b2bDoc2=true
        public string cc23 { get; set; } = UnCheckedBox;//b2bDoc2=false
        public string cc24 { get; set; } = UnCheckedBox;//b2bDoc3=true
        public string cc25 { get; set; } = UnCheckedBox;//b2bDoc3=false
        public string cc26 { get; set; } = UnCheckedBox;//b2bDoc4=true
        public string cc27 { get; set; } = UnCheckedBox;//b2bDoc4=false
        public string cc28 { get; set; } = UnCheckedBox;//b2bDoc5=true
        public string cc29 { get; set; } = UnCheckedBox;//b2bDoc5=false
        public string cc64 { get; set; } = UnCheckedBox;//b2bDoc6=true
        public string cc65 { get; set; } = UnCheckedBox;//b2bDoc6=false
        
        public string tc30 { get; set; } = string.Empty;//b2bTransferDoc2BusinessID
        public string tc31 { get; set; } = string.Empty;//b2bTransferDoc2BusinessTaxID
        #endregion

        #region 同時開立b2c及b2c電子發票營業人文件
        public string cc30 { get; set; } = UnCheckedBox;//b2cRequiredDoc1
        public string cc31 { get; set; } = UnCheckedBox;//b2cRequiredDoc2
        public string cc32 { get; set; } = UnCheckedBox;//b2cRequiredDoc3
        public string cc33 { get; set; } = UnCheckedBox;//b2cRequiredDoc4
        public string cc34 { get; set; } = UnCheckedBox;//b2cRequiredDoc5
        public string cc68 { get; set; } = UnCheckedBox;//b2cRequiredDoc6
        public string cc69 { get; set; } = UnCheckedBox;//b2cRequiredDoc7
        public string tc40 { get; set; } = UnCheckedBox;//b2cTransferDoc1TurnkeyToken
        public string cc40 { get; set; } = UnCheckedBox;//b2cTransferDoc1=true
        public string cc41 { get; set; } = UnCheckedBox;//b2cTransferDoc1=false
        public string tc41 { get; set; } = UnCheckedBox;//b2cTransferDoc2BusinessID
        public string tc42 { get; set; } = UnCheckedBox;//b2cTransferDoc2BusinessTaxID
        public string cc42 { get; set; } = UnCheckedBox;//b2cTransferDoc2=true
        public string cc43 { get; set; } = UnCheckedBox;//b2cTransferDoc2=false


        public string rc64 { get; set; } = UnCheckedRadioButton;//b2cDoc1Type=1
        public string rc65 { get; set; } = UnCheckedRadioButton;//b2cDoc1Type=2
        public string rc66 { get; set; } = UnCheckedRadioButton;//b2cDoc1Type=3
        public string tc67 { get; set; } = string.Empty;//b2cDoc1Other
        public string cc50 { get; set; } = UnCheckedBox;//b2cRequiredDoc8=true
        public string cc52 { get; set; } = UnCheckedBox;//b2cDoc2=true
        public string cc53 { get; set; } = UnCheckedBox;//b2cDoc2=false
        public string cc54 { get; set; } = UnCheckedBox;//b2cDoc3=true
        public string cc55 { get; set; } = UnCheckedBox;//b2cDoc3=false
        public string cc56 { get; set; } = UnCheckedBox;//b2cDoc4=true
        public string cc57 { get; set; } = UnCheckedBox;//b2cDoc4=false
        public string cc58 { get; set; } = UnCheckedBox;//b2cDoc5=true
        public string cc59 { get; set; } = UnCheckedBox;//b2cDoc5=false
        public string cc60 { get; set; } = UnCheckedBox;//b2cDoc6=true
        public string cc61 { get; set; } = UnCheckedBox;//b2cDoc6=false
        public string cc62 { get; set; } = UnCheckedBox;//b2cDoc7=true
        public string cc63 { get; set; } = UnCheckedBox;//b2cDoc7=false
        public string cc66 { get; set; } = UnCheckedBox;//b2cDoc8=true
        public string cc67 { get; set; } = UnCheckedBox;//b2cDoc8=false
        #endregion

        #region 申請人/聯絡方式
        public string td00 { get; set; } = string.Empty; //BusinessName
        public string td01 { get; set; } = string.Empty; //BusinessID
        public string td02 { get; set; } = string.Empty; //BusinessTaxID
        public string td03 { get; set; } = string.Empty; //BusinessAddr
        public string td04 { get; set; } = string.Empty; //BusinessOwner 
        public string td10 { get; set; } = string.Empty; //BusinessContactName
        public string td11 { get; set; } = string.Empty; //BusinessMobile
        public string td12 { get; set; } = string.Empty; //BusinessTelNo
        public string td13 { get; set; } = string.Empty; //BusinessFaxNo
        public string td14 { get; set; } = string.Empty; //BusinessContactAddr 
        public string td15 { get; set; } = string.Empty; //BusinessEmail 

        #endregion
        #region 委任專業代理
        public string td20 { get; set; } = string.Empty; //AgentName
        public string td21 { get; set; } = string.Empty; //AgentID 
        public string td22 { get; set; } = string.Empty; //AgentContactName 
        public string td23 { get; set; } = string.Empty; //AgentTelNo 
        public string td28 { get; set; } = string.Empty; //Agent負責人
        public string td24 { get; set; } = string.Empty; //AgentDateFrom
        public string td25 { get; set; } = string.Empty; //AgentDateTo
        public string td26 { get; set; } = string.Empty; //AgentDateFrom
        public string td27 { get; set; } = string.Empty; //AgentDateTo
        #endregion

        #region 系統廠商
        public string tf00 { get; set; }
        public string tf01 { get; set; }
        public string tf02 { get; set; }
        public string tf03 { get; set; }
        #endregion

        #region 發票開立系統檢測作業
        public string tf04 { get; set; }
        public string tf05 { get; set; }
        public string tf07 { get; set; }
        public string tf06 { get; set; }
        #endregion

        #region 發票開立系統檢測項目
        public string ce00 { get; set; } = UnCheckedBox; //SysChk11=true
        public string ce01 { get; set; } = UnCheckedBox; //SysChk11=false
        public string ce02 { get; set; } = UnCheckedBox; //SysChk12
        public string ce03 { get; set; } = UnCheckedBox;//SysChk12
        public string ce04 { get; set; } = UnCheckedBox;//SysChk13
        public string ce05 { get; set; } = UnCheckedBox;//SysChk13
        public string ce06 { get; set; } = UnCheckedBox;//SysChk13
        public string ce07 { get; set; } = UnCheckedBox;//SysChk13
        public string ce10 { get; set; } = UnCheckedBox;//SysChk13
        public string ce11 { get; set; } = UnCheckedBox;//SysChk13
        public string ce12 { get; set; } = UnCheckedBox;//SysChk13
        public string ce13 { get; set; } = UnCheckedBox;//SysChk13
        public string ce14 { get; set; } = UnCheckedBox;//SysChk13
        public string ce15 { get; set; } = UnCheckedBox;//SysChk13
        public string ce16 { get; set; } = UnCheckedBox;//SysChk13
        public string ce17 { get; set; } = UnCheckedBox;//SysChk13
        public string ce18 { get; set; } = UnCheckedBox;//SysChk13
        public string ce19 { get; set; } = UnCheckedBox;//SysChk13
        public string ce20 { get; set; } = UnCheckedBox;//SysChk13
        public string ce21 { get; set; } = UnCheckedBox;//SysChk13
        public string ce22 { get; set; } = UnCheckedBox;//SysChk13
        public string ce23 { get; set; } = UnCheckedBox;//SysChk13
        public string ce30 { get; set; } = UnCheckedBox;//SysChk13
        public string ce31 { get; set; } = UnCheckedBox;//SysChk13
        public string ce32 { get; set; } = UnCheckedBox;//SysChk13
        public string ce33 { get; set; } = UnCheckedBox;//SysChk13
        public string ce34 { get; set; } = UnCheckedBox;//SysChk13
        public string ce35 { get; set; } = UnCheckedBox;//SysChk13
        public string ce36 { get; set; } = UnCheckedBox;//SysChk13
        public string ce37 { get; set; } = UnCheckedBox;//SysChk13
        public string ce38 { get; set; } = UnCheckedBox;//SysChk13
        public string ce39 { get; set; } = UnCheckedBox;//SysChk13
        public string ce40 { get; set; } = UnCheckedBox;//SysChk13
        public string ce41 { get; set; } = UnCheckedBox;//SysChk13
        public string ce42 { get; set; } = UnCheckedBox;//SysChk13
        public string ce43 { get; set; } = UnCheckedBox;//SysChk13
        public string ce44 { get; set; } = UnCheckedBox;//SysChk13
        public string ce45 { get; set; } = UnCheckedBox;//SysChk13
        public string ce46 { get; set; } = UnCheckedBox;//SysChk13
        public string ce47 { get; set; } = UnCheckedBox;//SysChk13
        public string ce50 { get; set; } = UnCheckedBox;//SysChk13
        public string ce51 { get; set; } = UnCheckedBox;//SysChk13
        public string ce52 { get; set; } = UnCheckedBox;//SysChk13
        public string ce53 { get; set; } = UnCheckedBox;//SysChk13
        public string ce54 { get; set; } = UnCheckedBox;//SysChk13
        public string ce55 { get; set; } = UnCheckedBox;//SysChk13
        public string ce56 { get; set; } = UnCheckedBox;//SysChk13
        public string ce57 { get; set; } = UnCheckedBox;//SysChk13
        public string ce58 { get; set; } = UnCheckedBox;//SysChk13
        public string ce59 { get; set; } = UnCheckedBox;//SysChk13
        public string ce60 { get; set; } = UnCheckedBox;//SysChk13
        public string ce61 { get; set; } = UnCheckedBox;//SysChk13
        #endregion

        #region 委任加值服務中心
        public string td30 { get; set; } = "網際優勢股份有限公司"; //委任加值服務中心名稱
        public string td31 { get; set; } = "02-23962339"; //委任加值服務中心聯絡電話
        public string td32 { get; set; } = "70762419"; //委任加值服務中心統一編號 
        public string td33 { get; set; } = "101700372"; //委任加值服務中心稅籍編號
        public string td34 { get; set; } = "王瑞弟"; //委任加值服務中心負責人
        public string td35 { get; set; } = string.Empty;//ApplyByAppointDateFrom
        public string td36 { get; set; } = string.Empty;//ApplyByAppointDateTo
        public string td37 { get; set; } = string.Empty;//ApplyByAppointDateFrom
        public string td38 { get; set; } = string.Empty; //ApplyByAppointDateTo
        public string td40 { get; set; } = string.Empty; //申請日期
        #endregion

        #region 感熱紙送驗報告
        public string tg00 { get; set; }//TestBusinessName { get; set; }
        public string tg01 { get; set; }//SubmitBusinessName { get; set; }
        public string tg02 { get; set; }//ReportNo { get; set; }
        public string tg03 { get; set; }//PaperName { get; set; }
        public string tg04 { get; set; }//PaperNo { get; set; }
        public string tg05 { get; set; }//PaperDate { get; set; }
        #endregion



    }

    public enum ApplyNumberType
    {
        None = 0,
        Apply = 1,
        Increase =2,
        Deduct =3,
        Zero =4
    }


}