using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;
using System.Text;
using CommonLib.Core.DataWork;
using CommonLib.Utility;
using ModelCore.DTOs;
using ModelCore.Locale;

namespace ModelCore.DataEntity
{
    public static partial class ExtensionMethods
    {
        public static InvoiceUserCarrierType? GetDefaultUserCarrierType(this GenericDbContext<ApplicationDbContext> mgr, String typeName)
        {
            return mgr.GetTable<InvoiceUserCarrierType>().Where(t => t.CarrierType == typeName).FirstOrDefault();
        }

        public static int? GetMemberCodeID(this GenericDbContext<ApplicationDbContext> mgr, String hashID)
        {
            var item = mgr.GetTable<MemberCode>().Where(m => m.HashID == hashID).FirstOrDefault();
            return item != null ? item.CodeID : (int?)null;
        }

        public static String GetMaskInvoiceNo(this InvoiceItem item)
        {
            return String.Format("{0}{1}", item.TrackCode, item.DonateMark == "0" ? item.No : item.No.Substring(0, 5) + "***");
        }

        public static string WinningTypeTransform(this String typeValue)
        {
            switch(typeValue)
            {
                case "0":
                    return "特獎";
                case "1":
                    return "頭獎";
                case "2":
                    return "二獎";
                case "3":
                    return "三獎";
                case "4":
                    return "四獎";
                case "5":
                    return "五獎";
                case "6":
                    return "六獎";
                default:
                    return typeValue;
            }
        }

        public static bool IsB2C(this InvoiceBuyer buyer)
        {
            return buyer.ReceiptNo == "0000000000";
        }

        public static bool IsB2C(this InvoiceAllowanceBuyer buyer)
        {
            return buyer.ReceiptNo == "0000000000";
        }


        public static bool IsCrossBorderMerchant(this GenericDbContext<ApplicationDbContext> models, int? companyID)
        {
            return models.GetTable<OrganizationCategory>().Any(c => c.CompanyID == companyID && c.CategoryID == (int)Naming.CategoryID.COMP_CROSS_BORDER_MURCHANT);
        }


        public static Naming.InvoiceCenterBusinessType? CheckBusinessType(this InvoiceItem item)
        {
            return item.CDS_Document.DocumentOwner != null
                ? item.CDS_Document.DocumentOwner.OwnerID == item.SellerID
                    ? Naming.InvoiceCenterBusinessType.銷項
                    : item.CDS_Document.DocumentOwner.OwnerID == item.InvoiceBuyer?.BuyerID
                    ? Naming.InvoiceCenterBusinessType.進項 : (Naming.InvoiceCenterBusinessType?)null
                : (Naming.InvoiceCenterBusinessType?)null;
        }

        public static Naming.InvoiceCenterBusinessType? CheckBusinessType(this InvoiceAllowance item)
        {
            return item.CDS_Document?.DocumentOwner != null
                ? item.CDS_Document?.DocumentOwner.OwnerID == item.InvoiceAllowanceSeller?.SellerID
                    ? Naming.InvoiceCenterBusinessType.銷項
                    : item.CDS_Document?.DocumentOwner.OwnerID == item.InvoiceAllowanceBuyer?.BuyerID
                    ? Naming.InvoiceCenterBusinessType.進項 : (Naming.InvoiceCenterBusinessType?)null
                : (Naming.InvoiceCenterBusinessType?)null;
        }

        public static bool IsEnterpriseGroupMember(this Organization org)
        {
            return org.EnterpriseGroupMember.Any();
        }

        public static Organization? GetOrganizationByThumbprint(this GenericDbContext<ApplicationDbContext> mgr, String thumbprint)
        {
            Organization? item = mgr.GetTable<Organization>().Where(t => t.OrganizationToken != null && t.OrganizationToken.Thumbprint == thumbprint).FirstOrDefault();
            if (item == null)
            {
                item = mgr.GetTable<Organization>().Where(t => t.OrganizationStatus != null && t.OrganizationStatus.Token != null && t.OrganizationStatus.Token.Thumbprint == thumbprint).FirstOrDefault();
            }
            return item;
        }

        public static IQueryable<UserProfile> GetUserListByCompanyID(this GenericDbContext<ApplicationDbContext> mgr, int? companyID, Naming.CategoryID? category = null)
        {
            var items = mgr.GetTable<OrganizationCategory>()
                    .Where(c => c.CompanyID == companyID);
            if(category.HasValue)
            {
                items = items.Where(c => c.CategoryID == (int)category);

            }
            return items
                .Join(mgr.GetTable<UserRole>(), c => c.OrgaCateID, r => r.OrgaCateID, (c, r) => r)
                .Select(r => r.UserProfile);
        }

        public static IQueryable<UserProfile> GetUserListByCompanyID(this GenericDbContext<ApplicationDbContext> models, int[] companyID,IQueryable<UserRoleDefinition>? rolesFilter = null)
        {
            var roles = rolesFilter ?? models.GetTable<UserRoleDefinition>();
            return models.GetTable<OrganizationCategory>()
                .Where(c => companyID.Contains(c.CompanyID))
                .Join(models.GetTable<UserRole>().Join(roles, u => u.RoleID, r => r.RoleID, (u, r) => u),
                    c => c.OrgaCateID, r => r.OrgaCateID, (c, r) => r)
                .Select(r => r.UserProfile);
        }

        public static IQueryable<Organization> GetQueryByAgent(this GenericDbContext<ApplicationDbContext> mgr, int agentID)
        {
            return mgr.GetTable<Organization>().Where(o => o.CompanyID == agentID
                || mgr.GetTable<InvoiceIssuerAgent>()
                    .Where(a => a.AgentID == agentID)
                    .Select(a => a.IssuerID).Contains(o.CompanyID));
        }

        public static IQueryable<Organization> GetQueryByBranchRelation(this GenericDbContext<ApplicationDbContext> mgr, int headquarter)
        {
            var branchItems = mgr.GetTable<InvoiceIssuerAgent>()
                    .Where(a => a.RelationType == (int)InvoiceIssuerAgent.RelationTypeEnum.MasterBranch)
                    .Where(a => a.AgentID == headquarter);
            return mgr.GetTable<Organization>().Where(o => o.CompanyID == headquarter
                || branchItems.Any(a => a.IssuerID == o.CompanyID));
        }


        public static IQueryable<InvoiceItem> GetInvoiceByAgent(this GenericDbContext<ApplicationDbContext> mgr, IQueryable<InvoiceItem> items, int agentID,bool excludeAgentID=false, Naming.InvoiceCenterBusinessType? businessType = null)
        {
            //return items.Join(mgr.GetTable<InvoiceIssuerAgent>().Where(a => a.AgentID == agentID),
            //        i => i.SellerID, a => a.IssuerID, (i, a) => i);
            if (businessType == Naming.InvoiceCenterBusinessType.進項)
            {
                var buyerItems = mgr.GetTable<InvoiceIssuerAgent>().Where(a => a.AgentID == agentID)
                    .Join(mgr.GetTable<Organization>(), a => a.IssuerID, c => c.CompanyID, (a, c) => c);
                return items.Join(mgr.GetTable<InvoiceBuyer>()
                                    .Join(buyerItems, b => b.ReceiptNo, c => c.ReceiptNo, (b, c) => b),
                    i => i.InvoiceID, b => b.InvoiceID, (i, b) => i);
            }

            var issuers = mgr.GetTable<InvoiceIssuerAgent>().Where(a => a.AgentID == agentID)
                .Select(a => a.IssuerID);

            return excludeAgentID
                ? items.Where(i => issuers.Any(a => a == i.SellerID))
                : items.Where(i => i.SellerID == agentID || issuers.Any(a => a == i.SellerID));
        }

        public static IQueryable<InvoiceAllowance> GetAllowanceByAgent(this GenericDbContext<ApplicationDbContext> mgr, int agentID)
        {
            return mgr.GetAllowanceByAgent(mgr.GetTable<InvoiceAllowance>(), agentID);
        }

        public static IQueryable<InvoiceAllowance> GetAllowanceByAgent(this GenericDbContext<ApplicationDbContext> mgr, IQueryable<InvoiceAllowance> items,int agentID)
        {
            var issuers = mgr.GetTable<InvoiceIssuerAgent>().Where(a => a.AgentID == agentID)
                .Select(a => a.IssuerID);
            return items.Join(mgr.GetTable<InvoiceAllowanceSeller>()
                                .Where(s => s.SellerID == agentID || issuers.Any(a => a == s.SellerID)),
                            a => a.AllowanceID, s => s.AllowanceID, (a, s) => a);

            //return items.Where(i => i.InvoiceAllowanceSeller.SellerID == agentID
            //        || mgr.GetTable<InvoiceIssuerAgent>()
            //            .Where(a => a.AgentID == agentID)
            //            .Select(a => a.IssuerID)
            //            .Contains(i.InvoiceAllowanceSeller.SellerID.Value));
        }

        public static String InvoiceNo(this InvoiceItem item)
        {
            return $"{item.TrackCode}{item.No}";
        }

        public static bool CheckNotice(this int? setting,Naming.InvoiceNoticeStatus checkStatus)
        {
            return setting.HasValue && (setting.Value & (int)checkStatus) > 0;
        }

        public static int PushDocumentSubscriptionQueue(this GenericDbContext<ApplicationDbContext> models, int docID)
        {
            return models.ExecuteCommand(@"INSERT INTO DocumentSubscriptionQueue
                                                                (DocID)
                                    SELECT          DocID
                                    FROM              CDS_Document
                                    WHERE          (DocID = {0}) AND (NOT EXISTS
                                            (SELECT          NULL
                                                FROM               DocumentSubscriptionQueue
                                                WHERE           (DocID = {0})))", docID);
        }

        public static int CurrentAllocatingNo(this InvoiceNoInterval item)
        {
            var currentAllocated = (item.InvoiceNoAllocation.Max(a => (int?)a.InvoiceNo) + 1) ?? item.StartNo;
            var currentNo = (item.InvoiceNoAssignment.Max(a => a.InvoiceNo) + 1) ?? item.StartNo;
            return Math.Max(currentAllocated, currentNo);
        }

        public static ColumnAttribute? GetColumnAttribute(this PropertyInfo propertyInfo)
        {
            if (Attribute.IsDefined(propertyInfo, typeof(ColumnAttribute)))
            {
                return (ColumnAttribute?)Attribute.GetCustomAttribute(propertyInfo, typeof(ColumnAttribute));
            }

            return null;
        }
        public static bool CheckPrimaryKey(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetColumnAttribute()?.IsPrimaryKey == true;

            //if (propertyInfo.CustomAttributes?.Any() == true)
            //{
            //    if (propertyInfo.CustomAttributes
            //        .Any(p => p.NamedArguments
            //            .Any(a => a.MemberName == "IsPrimaryKey" && a.TypedValue.Value.Equals(true))))
            //    {
            //        return true;
            //    }
            //}
            //return false;
        }
        public static string GetJsonString(this InvoiceItem item)
        {
            ///TODO: Build DTOs to avoid circular reference
            var json = item.JsonStringify();
            return json;
        }

        public static string GetJsonString(this InvoiceAllowance item)
        {
            ///TODO: Build DTOs to avoid circular reference
            var json = item.JsonStringify();
            return json;
        }

    }

    public partial class EIVOEntityManager<TEntity> : GenericEntityRepository<ApplicationDbContext, TEntity>
        where TEntity : class, new()
    {
        public EIVOEntityManager() : base() { }
        public EIVOEntityManager(GenericDbContext<ApplicationDbContext>? manager) : base(manager) { }

        //protected virtual void applyProcessFlow(CDS_Document doc, int ownerID, Naming.B2BInvoiceDocumentTypeDefinition typeID, Naming.InvoiceCenterBusinessType businessID)
        //{
        //    var flow = this.GetTable<DocumentTypeFlow>().Where(f => f.TypeID == (int)typeID
        //        && f.CompanyID == ownerID && f.BusinessID == (int)businessID).FirstOrDefault();

        //    if (flow != null && flow.DocumentFlow.InitialStep.HasValue)
        //    {
        //        var initialStep = flow.DocumentFlow.DocumentFlowControl;
        //        doc.CurrentStep = initialStep.LevelID;

        //        doc.DocumentFlowStep = new DocumentFlowStep
        //        {
        //            CurrentFlowStep = initialStep.StepID
        //        };
        //    }
        //}

        //protected virtual void applyProcessFlow(CDS_Document doc, Naming.B2BInvoiceDocumentTypeDefinition typeID)
        //{
        //    var flow = this.GetTable<CommonDocumentTypeFlow>().Where(f => f.TypeID == (int)typeID).FirstOrDefault();

        //    if (flow != null && flow.DocumentFlow.InitialStep.HasValue)
        //    {
        //        var initialStep = flow.DocumentFlow.DocumentFlowControl;
        //        doc.CurrentStep = initialStep.LevelID;

        //        doc.DocumentFlowStep = new DocumentFlowStep
        //        {
        //            CurrentFlowStep = initialStep.StepID
        //        };
        //    }
        //}

        public List<TEntity>? EventItems
        {
            get;
            protected set;
        }

    }

    public class UnassignedInvoiceNoSummary
    {
        public string? CompanyName { get; set; }
        public string? ReceiptNo { get; set; }
        public InvoiceTrackCode? TrackCode { get; set; }
        public int StartNo { get; set; }
        public int EndNo { get; set; }
        // public OrganizationDepartment Department { get; set; }
        public UserProfile? VirtualUser { get; set; }
        public int CompanyID { get; set; }
        public int SegmentID { get; set; }
        public int TrackID { get; set; }
    }

}
