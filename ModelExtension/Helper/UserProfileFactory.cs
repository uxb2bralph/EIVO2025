using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Specialized;
using System.Xml;
using System.Security.Cryptography.X509Certificates;

using CommonLib.DataAccess;

using ModelCore.Resource;
using ModelCore.Properties;
using ModelCore.DataEntity;
using CommonLib.Utility;
using System.Linq;
using ModelCore.Security.MembershipManagement;

namespace ModelCore.Security.MembershipManagement
{

    /// <summary>
    /// Summary description for UserProfile.
    /// </summary>
    public static class UserProfileFactory
    {

        public static UserProfile? CreateInstance(int uid)
        {
            using UserProfileManager mgr = new UserProfileManager();
            return mgr.GetUserProfile(uid);
        }

        public static UserProfile? CreateInstance(string pid, string password)
        {
            UserProfile? profile = CreateInstance(pid);
            if (profile != null)
            {
                CipherDecipherSrv cipher = new CipherDecipherSrv(10);
                if (!String.IsNullOrEmpty(profile.Password) && password.Equals(cipher.decipher(profile.Password)))
                {
                    profile.Password = password;
                    return profile;
                }
                else if (String.Compare(ValidityAgent.HashPassword(password), profile.Password2, true) == 0)
                {
                    profile.Password = password;
                    return profile;
                }
            }

            return null;
        }

        public static UserProfile? CreateInstance(string pid)
        {
            using UserProfileManager mgr = new();
            return mgr.GetUserProfileByPID(pid);
        }

        public static UserProfile? CreateInstance(X509Certificate2 cert)
        {
            using UserProfileManager mgr = new UserProfileManager();
            return mgr.GetUserProfile(cert);
        }

        public static UserProfile? CreateInstance(Guid token)
        {
            using (UserProfileManager mgr = new UserProfileManager())
            {
               return mgr.GetUserProfile(token, ModelExtension.Properties.AppSettings.Default.SessionTimeout);
            }
        }
    }
}
