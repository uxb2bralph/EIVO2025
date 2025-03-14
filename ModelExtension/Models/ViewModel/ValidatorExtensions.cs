using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CommonLib.DataAccess;
using ModelCore.DataEntity;

using ModelCore.Locale;
using ModelCore.Resource;
using ModelCore.Schema.EIVO;
using CommonLib.Utility;

namespace ModelCore.Models.ViewModel
{
    public static class ValidatorExtensions
    {
        public readonly static String[] __InvoiceTypeList = { /*"01", "02", "03", "04", "05", "06",*/ "07", "08" };

        public static bool IsValidInvoiceType(this byte? invoiceType)
        {
            return invoiceType.HasValue ? invoiceType.Value.IsValidInvoiceType() : false;
        }

        public static bool IsValidInvoiceType(this byte invoiceType)
        {
            return __InvoiceTypeList.Contains(String.Format("{0:00}", invoiceType));
        }

        public static bool IsValidInvoiceType(this String invoiceType, out byte data)
        {
            if (byte.TryParse(invoiceType, out data))
            {
                return data.IsValidInvoiceType();
            }
            return false;
        }

    }
}
