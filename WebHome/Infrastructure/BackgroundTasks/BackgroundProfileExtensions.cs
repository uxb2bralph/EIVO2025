using CommonLib.Core.DataWork;
using ModelCore.DataEntity;
using ModelCore.DataEntityWrapper;
using ModelCore.Security.MembershipManagement;

namespace WebHome.Infrastructure.BackgroundTasks
{
    /// <summary>
    /// 背景作業取得使用者資料的輔助方法。
    /// </summary>
    public static class BackgroundProfileExtensions
    {
        /// <summary>
        /// 請求執行緒取得可安全帶入背景作業的使用者識別（純值型別）。
        /// </summary>
        /// <remarks>
        /// 直接把 <see cref="UserProfileWrapper"/> 傳進背景作業是錯的：它的 <c>Entity</c>／
        /// <c>CurrentUserRole</c> 由請求的 DbContext 追蹤，請求結束後存取
        /// <c>CurrentUserRole.OrganizationCategory</c> 這類導覽屬性會擲出 ObjectDisposedException。
        /// </remarks>
        public static (int? UID, int? RoleIndex) ForBackgroundWork(this UserProfileWrapper? profile)
        {
            return (profile?.Entity?.UID, profile?.RoleIndex);
        }

        /// <summary>
        /// 在背景作業中以自己的 DbContext 重新載入使用者，並還原請求當下所選的角色。
        /// </summary>
        public static UserProfileWrapper? ReloadProfile(this GenericDbContext<ApplicationDbContext> db, int? uid, int? roleIndex)
        {
            if (uid == null)
            {
                return null;
            }

            UserProfileWrapper? profile = new UserProfileManager(db).GetUserProfile(uid.Value);
            //GetUserProfile 一律取第 0 個角色，這裡還原請求當下實際選用的角色。
            profile?.DetermineUserRole(roleIndex);
            return profile;
        }
    }
}
