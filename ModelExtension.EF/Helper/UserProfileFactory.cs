using CommonLib.Core.DataWork;
using CommonLib.Utility;
using ModelCore.DataEntity;
using ModelCore.DataEntityWrapper;
using ModelCore.Properties;
using ModelCore.Resource;
using ModelCore.Security.MembershipManagement;
using System;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace ModelCore.Security.MembershipManagement
{

    /// <summary>
    /// Summary description for UserProfile.
    /// </summary>
    public static class UserProfileFactory
    {

        public static UserProfileWrapper? CreateInstance(UserProfileManager manager, int uid)
        {
            // try to create UserProfileManager with DI-provided ApplicationDbContext when available
            //var sp = WebHome.Startup.ServiceProvider;
            //if (sp != null)
            //{
            //    var db = sp.GetService(typeof(ModelCore.DataEntity.ApplicationDbContext)) as ModelCore.DataEntity.ApplicationDbContext;
            //    if (db != null)
            //    {
            //        using UserProfileManager manager = new UserProfileManager(new CommonLib.Core.DataWork.GenericDbContext<ModelCore.DataEntity.ApplicationDbContext>(db));
            //        return manager.GetUserProfile(uid);
            //    }
            //}

            return manager.GetUserProfile(uid);
        }

        public static UserProfileWrapper? CreateInstance(UserProfileManager manager,string pid, string password)
        {
            var profile = CreateInstance(manager, pid);
            if (profile != null)
            {
                CipherDecipherSrv cipher = new CipherDecipherSrv(10);
                if (!String.IsNullOrEmpty(profile.Entity.Password) && password.Equals(cipher.decipher(profile.Entity.Password)))
                {
                    profile.Entity.Password = password;
                    return profile;
                }
                else if (String.Compare(ValidityAgent.HashPassword(password), profile.Entity.Password2, true) == 0)
                {
                    profile.Entity.Password = password;
                    return profile;
                }
                else if (String.Compare(ValidityAgent.MakePassword(password), profile.Entity.Password2, true) == 0)
                {
                    profile.Entity.Password = password;
                    return profile;
                }
            }

            return null;
        }

        public static UserProfileWrapper? CreateInstance(UserProfileManager manager,string pid)
        {
            return manager.GetUserProfileByPID(pid);
        }

        public static UserProfileWrapper? CreateInstance(X509Certificate2 cert)
        {
            using UserProfileManager mgr = new UserProfileManager();
            return mgr.GetUserProfile(cert);
        }

        public static UserProfileWrapper? CreateInstance(Guid token)
        {
            using (UserProfileManager mgr = new UserProfileManager())
            {
               return mgr.GetUserProfile(token, ModelExtension.Properties.AppSettings.Default.SessionTimeout);
            }
        }
    }
}
