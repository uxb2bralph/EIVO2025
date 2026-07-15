using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ModelCore.DTOs;
using TaskCenter.Core.Services;

namespace TaskCenter.Core
{
    /// <summary>
    /// 僅允許系統管理者（ROLE_SYS / COMP_SYS）存取的授權過濾器。
    /// 對應舊版 [AuthorizedSysAdmin]。用於後台系統管理專用之 Controller（如營業人資料管理、
    /// 新登錄營業人受理），避免一般登入者透過加密 keyId 直接操作任意營業人資料。
    /// 需搭配 [Authorize] 使用（先驗證身分，再判斷是否為系統管理）。
    /// </summary>
    public class SysAdminOnlyAttribute : System.Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (!OrganizationScope.IsSystemAdmin(user.GetRoleId(), user.GetCategoryId()))
            {
                context.Result = new ObjectResult(new BaseResponseDto
                {
                    Success = false,
                    Message = "僅系統管理者可使用此功能!!",
                })
                {
                    StatusCode = 403,
                };
            }
        }
    }
}
