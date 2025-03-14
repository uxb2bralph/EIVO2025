using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Microsoft.AspNetCore.Mvc;

using CommonLib.Utility;
using WebHome.Helper;
using global::ModelCore.DataEntity;
using global::ModelCore.Locale;
using WebHome.Models.ViewModel;
using ModelCore.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http.Extensions;

namespace WebHome.Helper.Security.Authorization
{

    public class AuthorizedSysAdminAttribute : TypeFilterAttribute
    {
        public AuthorizedSysAdminAttribute() : base(typeof(RoleRequirementFilter))
        {
            Arguments = new object[] { (Func<UserProfile, bool>)(u => u.IsSystemAdmin()) };
        }
    }


    public class RoleAuthorizeAttribute : TypeFilterAttribute
    {
        public RoleAuthorizeAttribute(Naming.RoleID[] roleID) : base(typeof(RoleRequirementFilter))
        {
            Arguments = new object[] { (Func<UserProfile, bool>)(u => u.IsAuthorized(roleID)) };
        }
    }

    public class RoleRequirementFilter : IAuthorizationFilter
    {
        protected Func<UserProfile, bool> _auth;

        public RoleRequirementFilter(Func<UserProfile, bool> checkAuth)
        {
            _auth = checkAuth;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                var result = context.HttpContext.GetUserAsync();
                result.Wait();
                if (!_auth(result.Result))
                {
                    context.Result = new RedirectToRouteResult(
                        new RouteValueDictionary
                        {
                                { "controller", "Account" },
                                { "action", "Login" },
                                { "id", string.Empty }
                        });
                }
            }
            else
            {
                // 未登入，轉至登入頁面
                string rtURL = "";
                rtURL = UriHelper.GetEncodedPathAndQuery(context.HttpContext.Request);
                context.Result = new RedirectToRouteResult(new RouteValueDictionary
                {
                    { "controller", "Account" },
                    { "action", "Login" },
                    { "ReturnUrl", rtURL }
                });
            }

        }
    }

}