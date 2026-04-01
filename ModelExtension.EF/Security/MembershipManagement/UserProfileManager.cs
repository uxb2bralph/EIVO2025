using CommonLib.Core.DataWork;
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

        public UserProfileWrapper? GetUserProfile(int uid)
        {
            UserProfileWrapper? result = null;
            var item = _db.UserProfile.Where(u => u.UID == uid).FirstOrDefault();
            if (item != null)
            {
                result = new UserProfileWrapper(item);
                result.DetermineUserRole();
                this.GetCurrentSiteMenu(result);
            }
            return result;
        }

        public UserProfileWrapper? GetUserProfileByPID(string pid)
        {
            UserProfileWrapper? result = null;
            var item = _db.UserProfile.Where(u => u.PID == pid/* & u.UserProfileStatus.CurrentLevel != (int)Naming.MemberStatusDefinition.Mark_To_Delete*/).FirstOrDefault();
            if (item != null)
            {
                result = new UserProfileWrapper(item);
                result.DetermineUserRole();
                this.GetCurrentSiteMenu(result);
            }
            return result;
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
                UserProfileWrapper item = new UserProfileWrapper(auth.UserProfile);
                item.DetermineUserRole();
                GetCurrentSiteMenu(item);
                return item;
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
                UserProfileWrapper result = new UserProfileWrapper(item.UserProfile);
                result.DetermineUserRole();
                GetCurrentSiteMenu(result);
                return result;
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
