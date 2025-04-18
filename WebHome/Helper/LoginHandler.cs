﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Resource;
using ModelCore.Security.MembershipManagement;

namespace WebHome.Helper
{
    public class LoginHandler
    {
        private ControllerBase controller;
        public LoginHandler(ControllerBase controller)
        {
            //
            // TODO: Add constructor logic here
            //
            this.controller = controller;
        }

        public string? RedirectToAsLoginSuccessfully { get; set; }

        public bool ProcessLogin(string pid, string password, out string msg, out UserProfile member)
        {
            member = UserProfileFactory.CreateInstance(pid, password);
            bool auth = processLoginUsingRole(out msg, member);
            //if (up != null)
            //{
            //   if (up.Profile.UserProfileStatus.CurrentLevel == (int)Naming.MemberStatusDefinition.Wait_For_Check)
            //   {
            //       up.CurrentSiteMenu = "WaitForCheckMenu.xml";
            //       msg = VirtualPathUtility.ToAbsolute("~/UserProfile/EditMySelf?forCheck=True");
            //   }
            //}
            return auth;
        }

        public bool ProcessLogin(string pid, out String msg)
        {
            UserProfile up = UserProfileFactory.CreateInstance(pid);
            return processLoginUsingRole(out msg, up);
        }

        //public bool NormalLogin(string pid)
        //{
        //    bool bAuth = false;

        //    UserProfileMember up = UserProfileFactory.CreateInstance(pid);
        //    if (up != null)	//new login
        //    {
        //        HttpContext.Current.Session.Add(WebKey.USER_PROFILE, up);
        //        bAuth = true;
        //    }

        //    if (bAuth)
        //    {
        //        string url = FormsAuthentication.GetRedirectUrl(pid, false);
        //        FormsAuthentication.SetAuthCookie(pid, false);

        //        if (url != null && url.Length > 0 && !url.EndsWith("default.aspx"))
        //        {
        //            System.Web.Security.FormsAuthentication.RedirectFromLoginPage(pid, false);
        //        }
        //        else
        //        {
        //            if (up.RoleIndex >= 0)
        //            {

        //            }
        //            else
        //            {
        //                bAuth = false;
        //            }
        //        }
        //    }

        //    return bAuth;

        //}


        public bool ProcessLogin(X509Certificate2 signerCert, out string msg)
        {
            UserProfile up = UserProfileFactory.CreateInstance(signerCert);
            return processLoginUsingRole(out msg, up);
        }

        private bool processLoginUsingRole(out string? msg, UserProfile up)
        {
            msg = null;
            bool bAuth = false;
            if (up != null)	//new login
            {
                var task =  controller.HttpContext.SignOnAsync(up);
                task.Wait();
                bAuth = true;
            }

            if (bAuth)
            {
                string url = controller.Request.Query["ReturnUrl"];

                if (url != null && url.Length > 0 && !url.EndsWith("default.aspx"))
                {
                    //System.Web.Security.FormsAuthentication.RedirectFromLoginPage(up.PID, false);
                    msg = url;
                }
                else if (up.UserProfileStatus.CurrentLevel == (int)Naming.MemberStatusDefinition.Wait_For_Check)
                {
                    up.CurrentSiteMenu = "WaitForCheckMenu.xml";
                    msg = VirtualPathUtility.ToAbsolute("~/UserProfile/EditMySelf?forCheck=True");
                }
                else
                {
                    if (up.RoleIndex >= 0)
                    {
                        if (String.IsNullOrEmpty(RedirectToAsLoginSuccessfully))
                        {
                            //HttpContext.Current.Response.Redirect("MainPage.aspx", true);
                            msg = "~/Home/MainPage";
                        }
                        else
                        {
                            //HttpContext.Current.Response.Redirect(RedirectToAsLoginSuccessfully, true);
                            msg = RedirectToAsLoginSuccessfully;
                        }
                    }
                    else
                    {
                        bAuth = false;
                        msg = "User role has not been approved!!";
                        //msg = "使用者角色尚未被核定!!";
                    }
                }
            }
            else
            {
                msg = "We could not find your information. Please sign in again";
                //msg = "系統找不到您的資料，請重新登入!!";
            }

            return bAuth;
        }
    }
}
