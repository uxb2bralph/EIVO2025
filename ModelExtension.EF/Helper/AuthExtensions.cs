using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;
using ModelCore.Properties;
using ModelCore.Security.MembershipManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Utility;
using ModelCore.DTOs;
using ModelCore.DataEntityWrapper;

namespace ModelCore.Helper
{
    public static class AuthExtensions
    {
        public static String ComputeAuthorization(this OrganizationToken token, SHA256 hash, String seed)
        {
            String computedAuth = Convert.ToBase64String(
                    Encoding.Default.GetBytes(
                        String.Concat(
                            hash.ComputeHash(
                                Encoding.Default.GetBytes($"{token.Company.ReceiptNo}{token.KeyID}{seed}")
                            ).Select(b => b.ToString("x2"))
                        )
                    )
                );

            return computedAuth;
        }

        public static String ComputeAuthorization(this String keyID, String receiptNo, SHA256 hash, String seed)
        {
            String computedAuth = Convert.ToBase64String(
                    Encoding.Default.GetBytes(
                        String.Concat(
                            hash.ComputeHash(
                                Encoding.Default.GetBytes($"{receiptNo}{keyID}{seed}")
                            ).Select(b => b.ToString("x2"))
                        )
                    )
                );

            return computedAuth;
        }

        public static bool IsSystemAdmin(this UserProfileWrapper profile)
        {
            return profile != null && profile.CurrentUserRole?.RoleID == (int)Naming.RoleID.ROLE_SYS;
        }

        public static bool IsAuthorized(this UserProfileWrapper profile, params Naming.RoleID[] roleID)
        {
            return profile != null && profile.CurrentUserRole != null && roleID.Contains((Naming.RoleID)profile.CurrentUserRole.RoleID);
        }

        public static bool IsAuthorized(this UserProfileWrapper profile, int[] roleID)
        {
            return profile != null && profile.CurrentUserRole != null && roleID.Contains(profile.CurrentUserRole.RoleID);
        }
    }

    public enum LoginStatus
    {
        FirstLogin,
        ExpiredPassword,
        ExpiredSystemPassword,
        NeedAuthorized,
        NormalLogin
    }
}
