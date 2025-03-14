using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModelCore.Schema.EIVO;

using ModelCore.DataEntity;
using System.Globalization;
using System.Text.RegularExpressions;
using CommonLib.DataAccess;

namespace ModelCore.InvoiceManagement.Validator
{
    public static partial class D0501Validator
    {
        //檢查基本必填項目(作廢折讓單)
        public static Exception CheckMandatoryFields(this ModelCore.Schema.TurnKey.D0501.CancelAllowance item, GenericManager<EIVOEntityDataContext> mgr, OrganizationToken owner, out InvoiceAllowance allowance, out DateTime cancelDate)
        {
            cancelDate = default;

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
            if (!DateTime.TryParseExact(item.AllowanceDate, "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None, out allowanceDate))
            {
                return new Exception(String.Format("折讓證明單日期錯誤(YYYYMMDD)；傳送資料:{0}", item.AllowanceDate));
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

            if (seller.CompanyID != owner.CompanyID && !mgr.GetTable<InvoiceIssuerAgent>().Any(a => a.AgentID == owner.CompanyID && a.IssuerID == seller.CompanyID))
            {
                return new Exception(String.Format("簽章設定人與發票開立人不符,開立人統一編號:{0}，TAG:< SellerId />", item.SellerId));
            }

            if (String.IsNullOrEmpty(item.CancelDate))
            {
                return new Exception("作廢日期，TAG：< CancelDate />");
            }

            if (!DateTime.TryParseExact(item.CancelDate, "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None, out cancelDate))
            {
                return new Exception(String.Format("作廢折讓證明單作廢日期、作廢時間格式錯誤(YYYY/MM/DD HH:mm:ss)；傳送資料:{0} {1}", item.CancelDate, item.CancelTime));
            }
            cancelDate = cancelDate.Add(item.CancelTime.TimeOfDay);

            if (String.IsNullOrEmpty(item.CancelReason))
            {
                return new Exception("作廢原因不可空白，TAG：< CancelReason />");
            }

            if (item.CancelReason == null || item.CancelReason.Length > 20)
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