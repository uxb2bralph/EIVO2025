using System.Security.Claims;
using ModelCore.Locale;

namespace TaskCenter.Core
{
    /// <summary>
    /// 目前登入者（ClaimsPrincipal）之角色 / 組織識別輔助方法。
    /// 對應 LoginHandler 於 JWT 帶入的 claims：UID、roleId、companyId、categoryId。
    /// 供 seller-scoped API 判斷角色與資料範圍（取代舊版 HttpContext.GetUser() + UserProfileWrapper）。
    /// </summary>
    public static class CurrentUserExtensions
    {
        /// <summary>登入者 UID（ClaimTypes.NameIdentifier）。</summary>
        public static int? GetUserId(this ClaimsPrincipal user)
            => int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var v) ? v : null;

        /// <summary>登入者主要角色代碼（Naming.RoleID）。</summary>
        public static int? GetRoleId(this ClaimsPrincipal user)
            => int.TryParse(user.FindFirstValue("roleId"), out var v) ? v : null;

        /// <summary>登入者所屬營業人 CompanyID。</summary>
        public static int? GetCompanyId(this ClaimsPrincipal user)
            => int.TryParse(user.FindFirstValue("companyId"), out var v) ? v : null;

        /// <summary>登入者所屬營業人類別（Naming.CategoryID）。</summary>
        public static int? GetCategoryId(this ClaimsPrincipal user)
            => int.TryParse(user.FindFirstValue("categoryId"), out var v) ? v : null;

        /// <summary>是否為系統管理角色（ROLE_SYS）。</summary>
        public static bool IsSystemAdmin(this ClaimsPrincipal user)
            => user.GetRoleId() == (int)Naming.RoleID.ROLE_SYS;
    }
}
