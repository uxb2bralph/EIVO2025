using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ModelCore.DataEntity;
using ModelCore.Locale;

namespace ModelCore.InvoiceManagement.Validator
{
    public static partial class ExtensionMethods
    {
        public const String __InvoiceNoPattern = "([A-Za-z]{2})([0-9]{8})";
        public static Exception OrganizationValueCheck(this Organization dataItem)
        {
            if (String.IsNullOrEmpty(dataItem.CompanyName))
            {
                //檢查名稱
                return new Exception("請輸入公司名稱!!");
            }
            if (String.IsNullOrEmpty(dataItem.ReceiptNo))
            {
                //檢查名稱
                return new Exception("請輸入公司統編!!");
                
            }
            if (String.IsNullOrEmpty(dataItem.Addr))
            {
                //檢查名稱
                return new Exception("請輸入公司地址!!");
                
            }
            if (String.IsNullOrEmpty(dataItem.Phone))
            {
                //檢查名稱
                return new Exception("請輸入公司電話!!");
                
            }

            Regex reg = new Regex("\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*");

            if (String.IsNullOrEmpty(dataItem.ContactEmail) || !reg.IsMatch(dataItem.ContactEmail))
            {
                //檢查email
                return new Exception("電子信箱尚未輸入或輸入錯誤!!");
            }

            return null;
        }
        public static Match ParseInvoiceNo(this String invoiceNo)
        {
            return Regex.Match(invoiceNo ?? "", __InvoiceNoPattern);
        }

        public static bool IgnoreDuplicatedNo(this Naming.InvoiceProcessType? processType)
        {
            return processType == Naming.InvoiceProcessType.ReceivedA0101
                    || processType == Naming.InvoiceProcessType.ReceivedA0201
                    || processType == Naming.InvoiceProcessType.ReceivedB0101
                    || processType == Naming.InvoiceProcessType.ReceivedB0201;
        }
    }
}
