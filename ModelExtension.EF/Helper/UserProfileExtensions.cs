using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Specialized;
using System.Collections;

using CommonLib.Core.DataWork;

using System.Collections.Generic;
using System.Linq;
using ModelCore.DataEntity;
using ModelCore.Locale;
using System.Xml.Linq;
using ModelCore.DTOs;
using ModelCore.DataEntityWrapper;

namespace ModelCore.Helper
{

    public static class UserProfileExtensions
    {
        public static UserProfile LoadInstance(this UserProfileWrapper profile, GenericDbContext<ApplicationDbContext> models)
        {
            return models.GetTable<UserProfile>().Where(u => u.UID == profile.Entity.UID).First();
        }

        public static UserRole? LoadCurrentUserRole(this UserProfileWrapper profile, GenericDbContext<ApplicationDbContext> models)
        {
            if (profile.RoleIndex.HasValue)
            {
                return models.GetTable<UserRole>().Where(r => r.UID == profile.Entity.UID)
                                .Skip(profile.RoleIndex.Value).FirstOrDefault();
            }

            return null;
        }

        public static string? GetCurentSiteMenu(this UserRole role, GenericDbContext<ApplicationDbContext> models)
        {
            if (role != null)
            {
                return models.GetTable<UserMenu>().Where(m => m.RoleID == role.RoleID
                        && m.CategoryID == role.OrganizationCategory.CategoryID)
                    .Select(m => m.Menu)
                    .FirstOrDefault()?.SiteMenu;
            }

            return null;
        }

        public static XElement? GetOrganizationCategoryUserRoleMenuContent(this UserRole role, GenericDbContext<ApplicationDbContext> models)
        {
            if (role != null)
            {
                var menu = models.GetTable<OrganizationCategoryUserRole>().Where(m => m.RoleID == role.RoleID && m.OrgaCateID == role.OrgaCateID).FirstOrDefault();
                if (menu?.MainMenu != null)
                {
                    return XElement.Parse(menu.MainMenu);
                }
            }

            return null;
        }


        public static string? CurrentSiteMenu(this UserProfileWrapper profile, GenericDbContext<ApplicationDbContext> models)    
        {
            UserRole? role = null;
            if(profile.CurrentSiteMenu == null)
            {
                role = profile.LoadCurrentUserRole(models);
                profile.CurrentSiteMenu = role?.GetCurentSiteMenu(models);
            }

            if(profile.CurrentSiteMenu == null)
            {
                if(role != null)
                {
                    var menu = role?.GetOrganizationCategoryUserRoleMenuContent(models);
                    if (menu != null)
                    {
                        profile.CurrentSiteMenu = String.Format("OrgaCate_{0}_{1}.xml", role?.OrgaCateID, role.RoleID);
                    }
                }
            }

            return profile.CurrentSiteMenu;

        }

        public static Organization? CurrentCompany(this UserProfileWrapper profile, GenericDbContext<ApplicationDbContext> models)
        {
            return profile.LoadCurrentUserRole(models)?
                    .OrganizationCategory.Company;
        }

        public static string? CompanyName(this UserProfileWrapper profile, GenericDbContext<ApplicationDbContext> models)
        {
            return profile?.CurrentCompany(models)?.CompanyName;
        }

        public static IEnumerable<UserRole> UserRoleTable(this UserProfileWrapper profile, GenericDbContext<ApplicationDbContext> models)
        {
            return models.GetTable<UserRole>().Where(r=>r.UID == profile.Entity.UID);
        }

        public static bool ChooseUserRoleBySpecifiedInfo(this UserProfileWrapper  profile, GenericDbContext<ApplicationDbContext> models, int companyID, Naming.CategoryID cateID)
        {
            int index = 0;
            foreach (var item in profile.UserRoleTable(models))
            {
                if (item.OrganizationCategory.CategoryID == (int)cateID && item.OrganizationCategory.CompanyID == companyID)
                {
                    profile.RoleIndex = index;
                    return true;
                }
                else
                {
                    index++;
                }
            }
            return false;
        }

    }
}
