using ModelCore.DataEntity;
using ModelCore.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelCore.InvoiceManagement.InvoiceProcess
{
    public static class InvoiceProcessExtensions
    {
        public static Naming.InvoiceStepDefinition StepReadyToAllowanceMIG(this Organization seller)
        {
            return seller.OrganizationSettings.Any(s => s.Settings == "SendAllowanceMIGManually")
                ? Naming.InvoiceStepDefinition.待批次傳送
                : Naming.InvoiceStepDefinition.已開立;
        }

        public static bool HybridB2B(this Organization seller)
        {
            return seller.OrganizationSettings.Any(s => s.Settings == "HybridB2B");
        }

        public static bool InvoiceNotUploadedAlert(this Organization seller)
        {
            return seller.OrganizationSettings.Any(s => s.Settings == "InvoiceNotUploadedAlert");
        }

        public static bool DisableC0401Template(this Organization seller)
        {
            return seller.OrganizationSettings.Any(s => s.Settings == "DisableC0401Template");
        }


        public static bool InvoiceExchange(this Organization seller)
        {
            return seller.OrganizationSettings.Any(s => s.Settings == "InvoiceExchange");
        }

        public static bool IgnoreDuplicatedNo(this Organization seller)
        {
            return seller.OrganizationSettings.Any(s => s.Settings == "IgnoreDuplicatedNo");
        }

        public static bool ForcedAuditNo(this Organization seller)
        {
            return seller.OrganizationSettings.Any(s => s.Settings == "ForcedAuditNo");
        }

    }
}
