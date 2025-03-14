using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Microsoft.AspNetCore.Mvc;



using ClosedXML.Excel;
using WebHome.Helper;
using WebHome.Models;
using WebHome.Models.ViewModel;
using ModelCore.Models.ViewModel;
using WebHome.Properties;
using ModelCore.DataEntity;
using ModelCore.Locale;

using CommonLib.Utility;
using WebHome.Helper.Security.Authorization;
using ModelCore.Helper;
using ModelCore.Helper;
using Microsoft.AspNetCore.Authorization;

namespace WebHome.Controllers
{
   
    public class UserProfileController : SampleController<InvoiceItem>
    {
        public UserProfileController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        [HttpGet]
        [Authorize]
        public ActionResult EditMySelf(bool? forCheck)
        {
            var profile = HttpContext.GetUser();
            UserProfileViewModel viewModel = new UserProfileViewModel
            {
                WaitForCheck = forCheck
            };

            var item = models.GetTable<UserProfile>().Where(u => u.PID == profile.PID).FirstOrDefault();

            if (item != null)
            {
                viewModel.KeyID = item.UID.EncryptKey();
                viewModel.PID = item.PID;
                viewModel.UserName = item.UserName;
                viewModel.EMail = item.EMail;
                viewModel.Address = item.Address;
                viewModel.Phone = item.Phone;
                viewModel.MobilePhone = item.MobilePhone;
                viewModel.Phone2 = item.Phone2;
                viewModel.SellerID = item.UserRole.FirstOrDefault()?.OrganizationCategory.CompanyID;
            }

            ViewBag.ViewModel = viewModel;
            return View(viewModel);
        }

        [Authorize]
        public ActionResult EditItem(UserProfileViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            var item = models.GetTable<UserProfile>().Where(u => u.UID == viewModel.UID).FirstOrDefault();

            if (item == null)
            {
                if (viewModel.ResetID.HasValue)
                {
                    item = models.GetTable<ResetUserPassword>().Where(r => r.ResetID == viewModel.ResetID)
                        .Select(r => r.UserProfile).FirstOrDefault();
                }
            }

            if (item != null)
            {
                viewModel.KeyID = item.UID.EncryptKey();
                viewModel.PID = item.PID;
                viewModel.UserName = item.UserName;
                viewModel.EMail = item.EMail;
                viewModel.Address = item.Address;
                viewModel.Phone = item.Phone;
                viewModel.MobilePhone = item.MobilePhone;
                viewModel.Phone2 = item.Phone2;
                viewModel.RoleID = (Naming.RoleID?)item.UserRole.Select(r => r.RoleID).FirstOrDefault();
            }

            return View("~/Views/UserProfile/EditUserProfile.cshtml", model:item);
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(UserProfileViewModel viewModel)
        {
            ViewResult result = (ViewResult)EditItem(viewModel);
            UserProfile item = result.Model as UserProfile;
            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "資料錯誤!!");
            }

            String msg;
            if((new LoginHandler(this)).ProcessLogin(item.PID, out msg))
            {
                viewModel.WaitForCheck = true;
                return View("EditMySelf", item);
            }
            else
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "資料錯誤!!");
            }
        }

        //[RoleAuthorize(new Naming.RoleID[] { Naming.RoleID.ROLE_SYS, Naming.RoleID.ROLE_SELLER })]
        [Authorize]
        public ActionResult Commit(UserProfileViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            if (!String.IsNullOrEmpty(viewModel.KeyID))
            {
                viewModel.UID = viewModel.DecryptKeyValue();
            }

            var profile = HttpContext.GetUser();
            UserProfile item = viewModel.CommitUserProfileViewModel(models, ModelState, profile);

            if (!ModelState.IsValid)
            {
                ViewBag.ModelState = this.ModelState;
                return View("~/Views/Shared/ReportInputError.cshtml");
            }

            if (item == null)
            {
                item = models.GetTable<UserProfile>().Where(u => u.UID == viewModel.UID).FirstOrDefault();
            }

            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "資料錯誤!!");
            }
            else if (viewModel.WaitForCheck == true)
            {
                return View("~/Views/UserProfile/AccountChecked.cshtml");
            }
            else
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "資料已修改!!");
            }
        }

    }
}