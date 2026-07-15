using System.Linq;
using CommonLib.Core.DataWork;
using ModelCore.DataEntity;
using ModelCore.Locale;

namespace TaskCenter.Core.Services
{
    /// <summary>
    /// 依登入者角色/類別建立可存取的營業人範圍（移植自 QueryExtensions.InitializeOrganizationQuery）。
    /// 來源為 JWT claims（roleId / categoryId / companyId），取代舊版 UserProfileWrapper.CurrentUserRole。
    /// </summary>
    public static class OrganizationScope
    {
        /// <summary>是否為系統管理（角色 ROLE_SYS 或類別 COMP_SYS）→ 可存取全部。</summary>
        public static bool IsSystemAdmin(int? roleId, int? categoryId)
            => roleId == (int)Naming.RoleID.ROLE_SYS || categoryId == (int)Naming.CategoryID.COMP_SYS;

        /// <summary>
        /// 依類別回傳可存取的營業人（開立人）查詢範圍。
        /// COMP_SYS：所有賣方/虛擬通路/Google/代理；代理/Google：其下轄開立人；賣方/虛擬通路/跨境：僅自己。
        /// </summary>
        public static IQueryable<Organization> AllowedOrganizations(
            GenericDbContext<ApplicationDbContext> models, int categoryId, int companyId)
        {
            switch ((Naming.CategoryID)categoryId)
            {
                case Naming.CategoryID.COMP_SYS:
                    return models.GetTable<Organization>().Where(o => o.OrganizationCategory.Any(c =>
                        c.CategoryID == (int)Naming.CategoryID.COMP_E_INVOICE_B2C_SELLER
                        || c.CategoryID == (int)Naming.CategoryID.COMP_VIRTUAL_CHANNEL
                        || c.CategoryID == (int)Naming.CategoryID.COMP_E_INVOICE_GOOGLE_TW
                        || c.CategoryID == (int)Naming.CategoryID.COMP_INVOICE_AGENT));

                case Naming.CategoryID.COMP_INVOICE_AGENT:
                case Naming.CategoryID.COMP_E_INVOICE_GOOGLE_TW:
                    return models.GetQueryByAgent(companyId);

                case Naming.CategoryID.COMP_E_INVOICE_B2C_SELLER:
                case Naming.CategoryID.COMP_VIRTUAL_CHANNEL:
                case Naming.CategoryID.COMP_CROSS_BORDER_MURCHANT:
                    return models.GetTable<Organization>().Where(o => o.CompanyID == companyId);

                default:
                    return models.GetTable<Organization>().Where(o => false);
            }
        }

        /// <summary>
        /// 是否有權存取指定營業人（系統管理一律可；否則須在其角色可存取範圍內）。
        /// </summary>
        public static bool CanAccessSeller(
            GenericDbContext<ApplicationDbContext> models,
            int? roleId, int? categoryId, int? companyId, int targetCompanyId)
        {
            if (IsSystemAdmin(roleId, categoryId)) return true;
            if (categoryId == null || companyId == null) return false;
            return AllowedOrganizations(models, categoryId.Value, companyId.Value)
                .Any(o => o.CompanyID == targetCompanyId);
        }
    }
}
