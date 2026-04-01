using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using CommonLib.Utility;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;
using ModelCore.Schema.TurnKey.Invoice;
using Newtonsoft.Json;

namespace ModelCore.DataEntity
{
    public partial class DataDefinition
    {
    }


    public partial class InvoiceDeliveryTracking
    {
        public virtual int? DuplicateCount { get; set; }
        public virtual bool MergedItem { get; set; }
    }

    public class NotifyMailInfo
    {
        public virtual bool? isMail { get; set; }
        public virtual InvoiceItem? InvoiceItem { get; set; }
    }

    public class InvoiceEntity
    {
        public virtual InvoiceItem? MainItem { get; set; }
        public virtual List<InvoiceProduct>? ItemDetails { get; set; }
        public virtual Naming.UploadStatusDefinition? Status { get; set; }
        public virtual String? Reason { get; set; }
    }

    public partial class InvoiceAmountType
    {
        public virtual String? TaxTypeString => TaxType == (byte)Naming.TaxTypeDefinition.免稅
                ? ""
                : TaxType == (byte)Naming.TaxTypeDefinition.零稅率
                    ? "TZ"
                    : "TX";
    }

    public partial class InvoiceAllowanceItem
    {
        public virtual String? TaxTypeString => TaxType == (byte)Naming.TaxTypeDefinition.免稅
                ? ""
                : TaxType == (byte)Naming.TaxTypeDefinition.零稅率
                    ? "TZ"
                    : "TX";
    }

    public partial class CustomSmtpHost
    {
        public enum StatusType
        {
            Disabled = 0,
            Enabled = 1,
        }
    }

    public partial class OrganizationCustomSetting
    {
        private OrganizationCustomSettingsModel? _settings;
        public virtual OrganizationCustomSettingsModel Settings
        {
            get
            {
                if (_settings == null)
                {
                    if (SettingData != null)
                    {
                        _settings = JsonConvert.DeserializeObject<OrganizationCustomSettingsModel>(SettingData);
                    }
                }

                if (_settings == null)
                {
                        _settings = new OrganizationCustomSettingsModel { };
                        Accept();
                }

                return _settings;
            }
        }

        public void Accept()
        {
            SettingData = _settings?.JsonStringify();
        }
    }

    public class OrganizationCustomSettingsModel
    {
        public String? C0401POSView { get; set; }
        public Naming.Truth? DisableE0501AutoUpdate { get; set; }
        public Naming.Truth? E0501InitialLock { get; set; }
        public int? E0501ReservedBooklets { get; set; }
        public ZeroTaxRateReasonEnum? ZeroTaxRateReason { get; set; }
    }

    public partial class CategoryDefinition
    {
        public enum CategoryEnum
        {
            發票開立營業人 = 15,	                                //	2	賣方	     sketch_seller.gif
            相對營業人 = 16,	            //	3	買方	     sketch_buyer.gif
            GoogleTaiwan = 17,                            //  4  
            集團成員 = 18,
            營業人發票自動配號 = 19,
            經銷商 = 20,
            境外電商 = 23,
            主機構 = 24,
        }
    }

    public partial class ExtraBillingItem
    {
        public enum BillingTypeEnum
        {
            PayOnce = 1,
            PayContinuous = 2,
        }
    }

    public partial class InvoiceIssuerAgent
    {
        public enum RelationTypeEnum
        {
            MasterBranch = 1,
        }
    }

    public partial class Organization
    {
        public virtual Organization? Headquarter
        {
            get => this.AsInvoiceIssuer.Where(a => a.RelationType == (int)ModelCore.DataEntity.InvoiceIssuerAgent.RelationTypeEnum.MasterBranch).FirstOrDefault()?.Agent;
        }
    }

    public partial class InvoiceAllowance
    {
        public String? TurnkeyAllowanceNo =>
            AllowanceNumber?.Length > 16
                ? $"{InvoiceAllowanceDetails.FirstOrDefault()?.InvoiceNo}{AllowanceID % 1000000:000000}"
                : AllowanceNumber;
    }


    public partial class UserProfileProperty
    {
        public enum PropertyType
        {
            ExclusiveBuyerMail = 1,
        }

        public String? ToPlainText()
        {
            if (Property == null)
            {
                return null;
            }

            switch ((PropertyType)PropertyID)
            {
                case PropertyType.ExclusiveBuyerMail:
                    return String.Join("\r\n", JsonConvert.DeserializeObject<String[]>(Property)!);

                default:
                    return Property;
            }
        }

        public void ApplyPlainText(String? text)
        {
            if (text == null)
            {
                Property = null;
                return;
            }

            switch ((PropertyType)PropertyID)
            {
                case PropertyType.ExclusiveBuyerMail:
                    Property = text!.Split(',', ';', '\n', '、')
                                    .Select(s => s.GetEfficientString())
                                    .Where(s => s != null)
                                    .ToArray().JsonStringify();
                    break;

                default:
                    Property = text;
                    break;
            }

        }

    }

    public partial class ReviseInvoiceContent
    {
        public String? ReceiptNo { get; set; }
        public String? SellerName { get; set; }
    }
}
