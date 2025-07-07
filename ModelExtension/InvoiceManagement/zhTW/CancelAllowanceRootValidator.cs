using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModelCore.Schema.EIVO;
using ModelCore.DataEntity;
using System.Globalization;
using System.Text.RegularExpressions;
using ModelCore.Helper;
using CommonLib.DataAccess;
using ModelCore.Locale;
using ModelCore.InvoiceManagement.Validator;


namespace ModelCore.InvoiceManagement.zhTW
{
    public static partial class CancelAllowanceRootValidator
    {
        //檢查基本必填項目(作廢折讓單)
        public static Exception? VoidAllowance(this CancelAllowanceRootCancelAllowance item, GenericManager<EIVOEntityDataContext> models, Organization? owner,ref InvoiceAllowanceCancellation? voidItem, ref DerivedDocument? derivedDoc, out bool duplicateProcess, Naming.InvoiceProcessType? processType = Naming.InvoiceProcessType.G0501)
        {
            duplicateProcess = false;
            DateTime cancelDate= DateTime.Now;
            InvoiceAllowance? allowance = models.GetTable<InvoiceAllowance>().Where(a => a.AllowanceNumber == item.CancelAllowanceNumber).FirstOrDefault();
            if (allowance == null)
            {
                return new Exception(String.Format("折讓證明單不存在，折讓證明單號碼:{0}，TAG：< CancelAllowanceNumber />", item.CancelAllowanceNumber));
            }

            if (allowance.InvoiceAllowanceCancellation != null)
            {
                if (processType.IgnoreDuplicatedNo())
                {
                    duplicateProcess = true;
                    voidItem = allowance.InvoiceAllowanceCancellation;
                    derivedDoc = allowance.CDS_Document.ChildDocument.FirstOrDefault();
                    return null;
                }
                else
                    return new Exception(String.Format("作廢折讓單已存在,折讓單號碼:{0}", item.CancelAllowanceNumber));
            }


            DateTime allowanceDate;
            if (String.IsNullOrEmpty(item.AllowanceDate))
            {
                return new Exception("折讓證明單日期，TAG：< AllowanceDate />");
            }
            if (!DateTime.TryParseExact(item.AllowanceDate, "yyyy/MM/dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out allowanceDate))
            {
                return new Exception(String.Format("折讓證明單日期錯誤(YYYY/MM/DD)；傳送資料:{0}", item.AllowanceDate));
            }


            if (item.BuyerId != "0000000000" && !Regex.IsMatch(item.BuyerId, "^[0-9]{8}$"))
            {
                return new Exception(String.Format("買方識別碼錯誤，傳送資料：{0}，TAG:< BuyerId />", item.BuyerId));
            }

            Organization? seller = models.GetTable<Organization>().Where(o => o.ReceiptNo == item.SellerId).FirstOrDefault();

            if (seller == null)
            {
                return new Exception(String.Format("賣方為非註冊營業人,開立人統一編號:{0}，TAG:< SellerId />", item.SellerId));
            }

            if (owner != null && seller.CompanyID != owner.CompanyID && !models.GetTable<InvoiceIssuerAgent>().Any(a => a.AgentID == owner.CompanyID && a.IssuerID == seller.CompanyID))
            {
                return new Exception(String.Format("簽章設定人與發票開立人不符,開立人統一編號:{0}，TAG:< SellerId />", item.SellerId));
            }

            if (String.IsNullOrEmpty(item.CancelDate))
            {
                return new Exception("作廢日期，TAG：< CancelDate />");
            }

            if (String.IsNullOrEmpty(item.CancelTime))
            {
                return new Exception("作廢時間，TAG：< CancelTime />");
            }

            if (!DateTime.TryParseExact(String.Format("{0} {1}", item.CancelDate, item.CancelTime), "yyyy/MM/dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out cancelDate))
            {
                return new Exception(String.Format("作廢折讓證明單作廢日期、作廢時間格式錯誤(YYYY/MM/DD HH:mm:ss)；傳送資料:{0} {1}", item.CancelDate, item.CancelTime));
            }

            if (String.IsNullOrEmpty(item.CancelReason))
            {
                return new Exception("作廢原因不可空白，TAG：< CancelReason />");
            }

            if (item.CancelReason.Length > 20)
            {
                return new Exception(String.Format("資料格式長度最少1碼，最多20碼，傳送資料：{0}，TAG：< CancelReason />", item.CancelReason));
            }

            if (item.Remark != null && item.Remark.Length > 200)
            {
                return new Exception(String.Format("備註資料長度不可超過200，傳送資料：{0}，TAG：< Remark />", item.Remark));
            }

            voidItem = allowance.PrepareVoidItem(models, ref derivedDoc, processType);
            voidItem.Remark = item.Remark;
            voidItem.CancelDate = cancelDate;
            voidItem.CancelReason = item.CancelReason;

            models.SubmitChanges();
            

            return null;

        }

        public static Exception CheckMandatoryFields_Proxy(this CancelAllowanceRootCancelAllowance item, GenericManager<EIVOEntityDataContext> mgr, OrganizationToken owner, out InvoiceAllowance allowance, out DateTime cancelDate)
        {
            cancelDate = default(DateTime);

            allowance = mgr.GetTable<InvoiceAllowance>().Where(a => a.AllowanceNumber == item.CancelAllowanceNumber).FirstOrDefault();
            if (allowance == null)
            {
                return new Exception(String.Format("折讓證明單不存在，折讓證明單號碼:{0}，TAG：< CancelAllowanceNumber />", item.CancelAllowanceNumber));
            }

            if (allowance.InvoiceAllowanceCancellation != null)
            {
                return new Exception(String.Format("作廢折讓單已存在,折讓單號碼:{0}", item.CancelAllowanceNumber));
            }


            DateTime allowanceDate;
            if (String.IsNullOrEmpty(item.AllowanceDate))
            {
                return new Exception("折讓證明單日期，TAG：< AllowanceDate />");
            }
            if (!DateTime.TryParseExact(item.AllowanceDate, "yyyy/MM/dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out allowanceDate))
            {
                return new Exception(String.Format("折讓證明單日期錯誤(YYYY/MM/DD)；傳送資料:{0}", item.AllowanceDate));
            }


            if (item.BuyerId != "0000000000" && !Regex.IsMatch(item.BuyerId, "^[0-9]{8}$"))
            {
                return new Exception(String.Format("買方識別碼錯誤，傳送資料：{0}，TAG:< BuyerId />", item.BuyerId));
            }

            Organization seller = mgr.GetTable<Organization>().Where(o => o.ReceiptNo == item.SellerId).FirstOrDefault();

            if (seller == null)
            {
                return new Exception(String.Format("賣方為非註冊營業人,開立人統一編號:{0}，TAG:< SellerId />", item.SellerId));
            }

            //代理營業人
            var proxyOrg = mgr.GetTable<Organization>().Where(i => i.CompanyID == owner.CompanyID).FirstOrDefault();
            //上傳資料營業人
            var Org = seller;
            //上傳營業人資料是否註冊在代理營業人下
            var proxy = mgr.GetTable<InvoiceIssuerAgent>().Where(i => i.AgentID == proxyOrg.CompanyID && i.IssuerID == Org.CompanyID).FirstOrDefault();
            if (proxy == null && proxyOrg.ReceiptNo != item.SellerId)
            {
                return new Exception(String.Format("上傳營業人並無登記在此發票開立代理營業人底下:{0}，TAG:< SellerId />；代理發票開立營業人:{1}", Org.ReceiptNo, proxyOrg.ReceiptNo));
            }

            if (String.IsNullOrEmpty(item.CancelDate))
            {
                return new Exception("作廢日期，TAG：< CancelDate />");
            }

            if (String.IsNullOrEmpty(item.CancelTime))
            {
                return new Exception("作廢時間，TAG：< CancelTime />");
            }

            if (!DateTime.TryParseExact(String.Format("{0} {1}", item.CancelDate, item.CancelTime), "yyyy/MM/dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out cancelDate))
            {
                return new Exception(String.Format("作廢折讓證明單作廢日期、作廢時間格式錯誤(YYYY/MM/DD HH:mm:ss)；傳送資料:{0} {1}", item.CancelDate, item.CancelTime));
            }

            if (String.IsNullOrEmpty(item.CancelReason))
            {
                return new Exception("作廢原因不可空白，TAG：< CancelReason />");
            }

            if (item.CancelReason.Length > 20)
            {
                return new Exception(String.Format("資料格式長度最少1碼，最多20碼，傳送資料：{0}，TAG：< CancelReason />", item.CancelReason));
            }

            if (item.Remark != null && item.Remark.Length > 200)
            {
                return new Exception(String.Format("備註資料長度不可超過200，傳送資料：{0}，TAG：< Remark />", item.Remark));
            }

            return null;

        }

    }
}