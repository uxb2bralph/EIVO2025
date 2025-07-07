using CommonLib.Utility;
using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Schema.TurnKey.Allowance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ModelCore.Helper
{
    public static class AllowanceProcessExtensions
    {
        public static XmlDocument? CreateB0102(this InvoiceAllowance item)
        {
            var result = item.CreateConfirmedAllowanceMIG();
            if (result == null)
            {
                return null;
            }
            var docInv = result.ConvertToXml();
            docInv.DocumentElement!.SetAttribute("xmlns", ModelCore.Properties.AppSettings.Default.MIG.B0102);
            docInv.LoadXml(docInv.OuterXml);
            return docInv;
        }

        public static AllowanceConfirm? CreateConfirmedAllowanceMIG(this InvoiceAllowance item)
        {
            var queueItem = item.CDS_Document.DataProcessQueue
                .Where(q => q.StepID == (int)Naming.InvoiceStepDefinition.待傳送)
                .Where(q => q.ProcessType == (int)Naming.InvoiceProcessType.B0102)
                .FirstOrDefault();

            if (queueItem == null)
            {
                return null;
            }

            var result = new AllowanceConfirm
            {
                AllowanceNumber = $"{item.AllowanceNumber}",
                AllowanceDate = String.Format("{0:yyyyMMdd}", item.AllowanceDate),
                BuyerId = item.InvoiceAllowanceBuyer.ReceiptNo,
                SellerId = item.InvoiceAllowanceSeller.ReceiptNo,
                ReceiveDateTime = queueItem.DispatchDate,
                Remark = null,
                AllowanceType = item.AllowanceType.HasValue ? (AllowanceTypeEnum)item.AllowanceType.Value : AllowanceTypeEnum.Item2
            };

            return result;
        }

    }
}
