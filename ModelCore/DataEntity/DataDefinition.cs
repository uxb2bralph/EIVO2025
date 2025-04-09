using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using CommonLib.Utility;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;
using Newtonsoft.Json;

namespace ModelCore.DataEntity
{
    public partial class DataDefinition
    {
    }

    public partial class EIVOEntityDataContext
    {
        public EIVOEntityDataContext() :
                base(global::ModelCore.Properties.AppSettings.Default.ConnectionString, mappingSource)
        {
            OnCreated();
        }
    }

    public partial class TurnKey2DataContext
    {
        public TurnKey2DataContext() :
                base(global::ModelCore.Properties.AppSettings.Default.EINVTurnkey2ConnectionString, mappingSource)
        {
            OnCreated();
        }
    }

    public partial class InvoiceDeliveryTracking
    {
        public int? DuplicateCount { get; set; }
        public bool MergedItem { get; set; }
    }

    public partial class InvoiceItem
    {
        public int? PackageID { get; set; }
    }


    public class NotifyToProcessID : DocumentQueryViewModel
    {
        public int? MailToID { get; set; }
        public Organization? Seller { get; set; }
        public String? itemNO { get; set; }
        public String? Subject { get; set; }
    }

    public class NotifyMailInfo
    {
        public bool? isMail { get; set; }
        public InvoiceItem? InvoiceItem { get; set; }
    }

    public class InvoiceEntity
    {
        public InvoiceItem? MainItem { get; set; }
        public List<InvoiceProduct>? ItemDetails { get; set; }
        public Naming.UploadStatusDefinition? Status { get; set; }
        public String? Reason { get; set; }
    }

    public partial class InvoiceAmountType
    {
        public String? TaxTypeString => TaxType == (byte)Naming.TaxTypeDefinition.免稅
                ? ""
                : TaxType == (byte)Naming.TaxTypeDefinition.零稅率
                    ? "TZ"
                    : "TX";
    }

    public partial class InvoiceAllowanceItem
    {
        public String? TaxTypeString => TaxType == (byte)Naming.TaxTypeDefinition.免稅
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
        public OrganizationCustomSettingsModel Settings
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
        public enum Relationship
        {
            MasterBranch = 1,
        }
    }

    public partial class Organization
    {
        public IEnumerable<InvoiceIssuerAgent> BranchRelation
        {
            get => this.InvoiceIssuerAgent.Where(a => a.RelationType == (int)ModelCore.DataEntity.InvoiceIssuerAgent.Relationship.MasterBranch);
        }
    }

    public partial class UserRole
    {
        [DataMember]
        public UserProfile User
        {
            get => UserProfile;
            set => UserProfile = value;
        }
    }

    public partial class InvoiceAllowance
    {
        public String? TurnkeyAllowanceNo =>
            AllowanceNumber?.Length > 16
                ? $"{InvoiceAllowanceDetails.FirstOrDefault()?.InvoiceAllowanceItem.InvoiceNo}{AllowanceID % 1000000:000000}"
                : AllowanceNumber;
    }

    public partial class InvoiceDetail
    {
        [DataMember(IsRequired = false)]
        public InvoiceProduct Product { get => this.InvoiceProduct; set => this.InvoiceProduct = value; }
    }

    public partial class InvoiceSeller
    {
        [DataMember(IsRequired = false)]
        public Organization Institution { get => this.Organization; set => this.Organization = value; }  

    }

    public partial class InvoiceBuyer
    {
        [DataMember(IsRequired = false)]
        public Organization Institution { get => this.Organization; set => this.Organization = value; }

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
