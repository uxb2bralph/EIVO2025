using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Specialized;
using System.Collections;

using CommonLib.DataAccess;

using System.Collections.Generic;
using System.Linq;
using ModelCore.DataEntity;
using ModelCore.Locale;
using System.Xml.Linq;

namespace ModelCore.Helper
{

    public static class UserProfileExtensions
    {
        public static UserProfile LoadInstance(this UserProfile profile, GenericManager<EIVOEntityDataContext> models)
        {
            return models.GetTable<UserProfile>().Where(u => u.UID == profile.UID).First();
        }

        public static UserRole? LoadCurrentUserRole(this UserProfile profile, GenericManager<EIVOEntityDataContext> models)
        {
            if (profile.RoleIndex.HasValue)
            {
                return models.GetTable<UserRole>().Where(r => r.UID == profile.UID)
                                .Skip(profile.RoleIndex.Value).FirstOrDefault();
            }

            return null;
        }

        public static string? GetCurentSiteMenu(this UserRole role, GenericManager<EIVOEntityDataContext> models)
        {
            if (role != null)
            {
                return models.GetTable<UserMenu>().Where(m => m.RoleID == role.RoleID
                        && m.CategoryID == role.OrganizationCategory.CategoryID)
                    .Select(m => m.MenuControl)
                    .FirstOrDefault()?.SiteMenu;
            }

            return null;
        }

        public static XElement? GetOrganizationCategoryUserRoleMenuContent(this UserRole role, GenericManager<EIVOEntityDataContext> models)
        {
            if (role != null)
            {
                var menu = models.GetTable<OrganizationCategoryUserRole>().Where(m => m.RoleID == role.RoleID && m.OrgaCateID == role.OrgaCateID).FirstOrDefault();
                if (menu != null)
                {
                    return menu.MainMenu;
                }
            }

            return null;
        }


        public static string? CurrentSiteMenu(this UserProfile profile, GenericManager<EIVOEntityDataContext> models)    
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

        public static Organization? CurrentCompany(this UserProfile profile, GenericManager<EIVOEntityDataContext> models)
        {
            return profile.LoadCurrentUserRole(models)?
                    .OrganizationCategory.Organization;
        }

        public static string? CompanyName(this UserProfile profile, GenericManager<EIVOEntityDataContext> models)
        {
            return profile?.CurrentCompany(models)?.CompanyName;
        }

        public static IEnumerable<UserRole> UserRoleTable(this UserProfile profile, GenericManager<EIVOEntityDataContext> models)
        {
            return models.GetTable<UserRole>().Where(r=>r.UID == profile.UID);
        }

        public static bool ChooseUserRoleBySpecifiedInfo(this UserProfile profile, GenericManager<EIVOEntityDataContext> models, int companyID, Naming.CategoryID cateID)
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
