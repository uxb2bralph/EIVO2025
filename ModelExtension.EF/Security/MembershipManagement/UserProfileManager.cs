using CommonLib.Core.DataWork;
using Microsoft.EntityFrameworkCore;
using ModelCore.DataEntity ;
using ModelCore.DataEntityWrapper;
using ModelCore.Helper;
using ModelCore.Locale;
using ModelCore.Properties;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ModelCore.Security.MembershipManagement
{
    public partial class UserProfileManager : GenericEntityRepository<ApplicationDbContext ,UserProfile>
    {
        public UserProfileManager() : base() { }
        public UserProfileManager(GenericDbContext<ApplicationDbContext> mgr) : base(mgr) { }

        /// <summary>
        /// 登入使用者的查詢；一次載入 profile 快取後仍會用到的導覽。
        /// </summary>
        /// <remarks>
        /// 登入後的 <see cref="UserProfileWrapper"/> 會被快取在 HttpContext（甚至跨請求傳遞），
        /// 但建立它的 DbContext 在方法結束時就已釋放。View 與權限判斷會存取
        /// <c>CurrentUserRole.OrganizationCategory.Company</c> 等導覽，若維持 lazy loading
        /// 便會在 context 釋放後拋出 LazyLoadOnDisposedContextWarning，因此在此一次載入。
        /// </remarks>
        private IQueryable<UserProfile> LoginUserProfile => _db.UserProfile
                .Include(u => u.UserRole).ThenInclude(r => r.OrganizationCategory).ThenInclude(c => c.Company)
                .Include(u => u.UserProfileStatus);

        private UserProfileWrapper BuildLoginProfile(UserProfile item)
        {
            UserProfileWrapper result = new UserProfileWrapper(item);
            result.DetermineUserRole();
            this.GetCurrentSiteMenu(result);
            return result;
        }

        public UserProfileWrapper? GetUserProfile(int uid)
        {
            var item = LoginUserProfile.Where(u => u.UID == uid).FirstOrDefault();
            return item == null ? null : BuildLoginProfile(item);
        }

        public UserProfileWrapper? GetUserProfileByPID(string pid)
        {
            var item = LoginUserProfile.Where(u => u.PID == pid/* & u.UserProfileStatus.CurrentLevel != (int)Naming.MemberStatusDefinition.Mark_To_Delete*/).FirstOrDefault();
            return item == null ? null : BuildLoginProfile(item);
        }


        public bool UpdateUserProfileCert(int uid, X509Certificate2 cert)
        {
            UserProfile? item = this.EntityList.Where(u => u.UID == uid).FirstOrDefault();
            if (item != null)
            {
                UserAuth? auth = GetTable<UserAuth>().Where(a => a.Thumbprint == cert.Thumbprint).FirstOrDefault();
                if (auth == null)
                {
                    auth = new UserAuth
                    {
                        UserProfile = item ,
                        Thumbprint = cert.Thumbprint,
                        X509Certificate = Convert.ToBase64String(cert.RawData)
                    };

                    this.GetTable<UserAuth>().Add(auth);
                    this.SubmitChanges();
                    return true; 
                }
            }
            return false;
        }

        public UserProfileWrapper? GetUserProfile(X509Certificate2 cert)
        {
            UserAuth? auth = GetTable<UserAuth>().Where(a => a.Thumbprint == cert.Thumbprint).FirstOrDefault();
            if (auth != null && auth.AuthID == auth.UserProfile.UserAuth.OrderByDescending(a => a.AuthID).FirstOrDefault()?.AuthID)
            {
                return GetUserProfile(auth.UID);
            }
            return null;
        }

        public Guid? LogonUser(X509Certificate2 cert)
        {
            UserAuth? auth = this.GetTable<UserAuth>().Where(a => a.Thumbprint == cert.Thumbprint).FirstOrDefault();
            if (auth != null)
            {
                return LogonUser(auth.UID);
            }

            return null;
        }

        public Guid? LogonUser(int uid)
        {
            UserToken item = new UserToken
            {
                LogonTime = DateTime.Now,
                UID = uid,
                Token = Guid.NewGuid()
            };

            this.GetTable<UserToken>().Add(item);
            this.SubmitChanges();

            return item.Token;
        }

        public UserProfileWrapper? GetUserProfile(Guid token, double intervalInMinutes)
        {
            UserToken? item = this.GetTable<UserToken>().Where(t => t.Token == token).FirstOrDefault();

            if (item != null && item.LogonTime.AddMinutes(intervalInMinutes) >= DateTime.Now)
            {
                item.LogonTime = DateTime.Now;
                this.SubmitChanges();
                return GetUserProfile(item.UID);
            }
            return null;
        }

        public void CreateConsumerProfile(UserProfile profile)
        {
            var orgaCate = this.GetTable<OrganizationCategory>().Where(w => w.CategoryID==(int)Naming.CategoryID.COMP_E_INVOICE_B2C_BUYER).FirstOrDefault();
            if(orgaCate!=null)
            {
                profile.UserRole.Add(new UserRole
                {
                    OrgaCateID = orgaCate.OrgaCateID,
                    RoleID = (int)Naming.RoleID.ROLE_BUYER
                });

                this.GetTable<UserProfileStatus>().Add(new UserProfileStatus
                {
                    UserProfile = profile,
                    CurrentLevel = (int)Naming.MemberStatusDefinition.Wait_For_Check,
                });

                this.EntityList.Add(profile);
                this.SubmitChanges();
            }
        }

        public IQueryable<UserProfile> GetAllSellerUser(IQueryable<UserProfile> items)
        {
            return items.Join(this.GetTable<UserRole>().Where(r => r.OrganizationCategory.CategoryID == (int)Naming.CategoryID.COMP_E_INVOICE_B2C_SELLER), u => u.UID, r => r.UID, (u, r) => u);
        }

        public IQueryable<UserProfile> GetAllBuyerUser(IQueryable<UserProfile> items)
        {
            return items.Join(this.GetTable<UserRole>().Where(r => r.OrganizationCategory.CategoryID == (int)Naming.CategoryID.COMP_E_INVOICE_B2C_BUYER), u => u.UID, r => r.UID, (u, r) => u);
        }

        public IQueryable<UserProfile> GetUserByUserRole(IQueryable<UserProfile> items,int roleID)
        {
            return items.Join(this.GetTable<UserRole>().Where(r => r.RoleID == roleID), u => u.UID, r => r.UID, (u, r) => u);
        }


        public string? GetCurrentSiteMenu(UserProfileWrapper profile)
        {
            return profile?.CurrentSiteMenu(this);
        }
    }
}
