using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using System.Xml;

using WebHome.Helper;
using ModelCore.Models.ViewModel;
using WebHome.Models.ViewModel;

using WebHome.Properties;
using WebHome.Helper.Security.Authorization;
using ModelCore.DataEntity;
using ModelCore.DocumentManagement;
using ModelCore.Helper;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;
using ModelCore.Schema.EIVO.B2B;
using ModelCore.Schema.TurnKey;
using ModelCore.Schema.TXN;

using CommonLib.Utility;
using CommonLib.Security.UseCrypto;
using Google.Authenticator;
using Microsoft.AspNetCore.Authorization;
using ModelCore.Security.MembershipManagement;
using CommonLib.Core.AspNetMvc;
using Microsoft.AspNetCore.WebUtilities;
using ModelCore.Security;

namespace WebHome.Controllers
{
    public class AccountController : SampleController<InvoiceItem>
    {
        public AccountController(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        // GET: Account
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> CbsLogin(CbsLoginViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            ViewBag.ModelState = this.ModelState;

            if (!ModelState.IsValid)
            {
                return View("~/Views/Account/CbsLogin.cshtml");
            }

            if (WebHome.Properties.AppSettings.Default.UseGoogleAuthenticator)
            {
                var item = UserProfileFactory.CreateInstance(viewModel.PID!, viewModel.Password!);
                if (item == null)
                {
                    ModelState.AddModelError("PID", "login failed ！");
                    return View("~/Views/Account/CbsLogin.cshtml");
                }

                item = item.LoadInstance(models!).PrepareTwoFactorKey(models!);

                return View("~/Views/Account/TwoFactorLogin.cshtml", item);

            }

            LoginHandler login = new LoginHandler(this);
            String msg;
            if (!login.ProcessLogin(viewModel.PID!, viewModel.Password!, out msg, out UserProfile member))
            {
                ModelState.AddModelError("PID", msg);
                return View("~/Views/Account/CbsLogin.cshtml");
            }

            if (member.Expiration < DateTime.Today)
            {
                return View("~/Views/Account/ChangePassword.cshtml", member);
            }

            viewModel.ReturnUrl = viewModel.ReturnUrl.GetEfficientString();
            return Redirect(viewModel.ReturnUrl ?? msg ?? "~/Account/CbsLogin");

        }

        [AllowAnonymous]
        public ActionResult CbsLogin()
        {
            //UserProfile profile = HttpContext.GetUser();
            //if (profile == null)
            //    return View();
            //else
            //    return processLogin(profile);
            this.HttpContext.Logout();

            return View("~/Views/Account/CbsLogin.cshtml");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult TwoFactorAuth(TwoFactorViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            if (viewModel.KeyID != null)
            {
                viewModel.UID = viewModel.DecryptKeyValue();
            }

            var item = models.GetTable<UserProfile>().Where(u => u.UID == viewModel.UID).FirstOrDefault();
            if (item == null)
            {
                ModelState.AddModelError("CodeDigit", "login failed ！");
                return View("~/Views/Shared/ReportInputError.cshtml");
            }

            viewModel.CodeDigit = viewModel.CodeDigit.GetEfficientString()?.Replace(" ", "");
            TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
            if (!TwoFacAuth.ValidateTwoFactorPIN(Encoding.Default.GetBytes(item.UserProfileExtension.TwoFactorKey), viewModel.CodeDigit))
            {
                ModelState.AddModelError("CodeDigit", "login failed ！");
                return View("~/Views/Shared/ReportInputError.cshtml");
            }

            LoginHandler login = new LoginHandler(this);
            String msg;
            if (!login.ProcessLogin(item.PID, out msg))
            {
                ModelState.AddModelError("CodeDigit", "login failed ！");
                return View("~/Views/Shared/ReportInputError.cshtml");
            }

            return new JavaScriptResult($"window.location.href = '{VirtualPathUtility.ToAbsolute(msg ?? "~/Account/CbsLogin")}';");

        }


        // GET: Account
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            var result = await CbsLogin(viewModel);
            if (!ModelState.IsValid)
            {
                return View("~/Views/Account/Login.cshtml");
            }
            else
            {
                return result;
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> LoginToProcess(CbsLoginViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            _ = await CbsLogin(viewModel);
            return Json(new { result = ModelState.IsValid });
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            ViewBag.ViewModel = new LoginViewModel { };
            ViewResult result = (ViewResult)CbsLogin();
            if (ModelExtension.Properties.AppSettings.Default.EIVO_Service != 0)
                result.ViewName = "~/Views/Account/Login.cshtml";
            return result;
        }

        public ActionResult ForgetPassword()
        {
            return View("~/Views/Account/ForgetPassword.cshtml");
        }

        public ActionResult CommitToResetPass(LoginViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            ViewBag.ModelState = this.ModelState;

            if (!ModelState.IsValid)
            {
                return View("ForgetPassword");
            }

            var item = models.GetTable<UserProfile>().Where(u => u.PID == viewModel.PID).FirstOrDefault();
            if (item == null)
            {
                ModelState.AddModelError("PID", "帳號錯誤！");
            }

            if (!ModelState.IsValid)
            {
                return View("ForgetPassword");
            }

            item.NotifyToResetPassword();
            return View("ForgetPassword", model: "重設密碼信件已送出至您的信箱！");

        }



        public async Task<ActionResult> CaptchaImgAsync(String code)
        {

            string captcha = Encoding.Default.GetString(AppResource.Instance.Decrypt(Convert.FromBase64String(code)));

            Response.Clear();
            Response.ContentType = "image/Png";
            using (Bitmap bmp = new Bitmap(120, 30))
            {
                int x1 = 0;
                int y1 = 0;
                int x2 = 0;
                int y2 = 0;
                int x3 = 0;
                int y3 = 0;
                int intNoiseWidth = 25;
                int intNoiseHeight = 15;
                using (Graphics g = Graphics.FromImage(bmp))
                {

                    //設定字型
                    using (Font font = new Font("Courier New", 16, FontStyle.Bold))
                    {

                        //設定圖片背景
                        g.Clear(Color.CadetBlue);

                        //產生雜點
                        for (int i = 0; i < 100; i++)
                        {
                            x1 = ValidityAgent.RANDOM.Next(0, bmp.Width);
                            y1 = ValidityAgent.RANDOM.Next(0, bmp.Height);
                            bmp.SetPixel(x1, y1, Color.DarkGreen);
                        }

                        using (Pen pen = new Pen(Brushes.Gray))
                        {
                            //產生擾亂弧線
                            for (int i = 0; i < 15; i++)
                            {
                                x1 = ValidityAgent.RANDOM.Next(bmp.Width - intNoiseWidth);
                                y1 = ValidityAgent.RANDOM.Next(bmp.Height - intNoiseHeight);
                                x2 = ValidityAgent.RANDOM.Next(1, intNoiseWidth);
                                y2 = ValidityAgent.RANDOM.Next(1, intNoiseHeight);
                                x3 = ValidityAgent.RANDOM.Next(0, 45);
                                y3 = ValidityAgent.RANDOM.Next(-270, 270);
                                g.DrawArc(pen, x1, y1, x2, y2, x3, y3);
                            }
                        }

                        //把GenPassword()方法換成你自己的密碼產生器，記得把產生出來的密碼存起來日後才能與user的輸入做比較。

                        g.DrawString(captcha, font, Brushes.Black, 3, 3);

                        using (FileBufferingWriteStream output = new FileBufferingWriteStream())
                        {
                            bmp.Save(output, ImageFormat.Png);
                            await output.DrainBufferAsync(Response.Body);
                        }

                        await Response.Body.FlushAsync();

                        //context.
                    }
                }
            }

            return new EmptyResult();
        }

        [RoleAuthorize(new Naming.RoleID[] { Naming.RoleID.ROLE_SYS, Naming.RoleID.ROLE_SELLER })]
        public ActionResult AccountIndex(UserAccountQueryViewModel viewModel, bool? showTab)
        {
            ViewBag.ViewModel = viewModel;
            ViewBag.ShowTab = showTab;

            var profile = HttpContext.GetUser();
            if (profile.IsSystemAdmin())
            {

            }
            else
            {
                if (!viewModel.SellerID.HasValue)
                {
                    viewModel.SellerID = profile.CurrentUserRole.OrganizationCategory.CompanyID;
                }
            }

            return View("~/Views/Account/AccountIndex.cshtml");
        }

        [AuthorizedSysAdmin()]
        public ActionResult SystemAccountIndex(UserAccountQueryViewModel viewModel, bool? showTab)
        {
            ViewBag.ViewModel = viewModel;
            ViewBag.ShowTab = showTab;

            viewModel.QueryAction = "InquireSystemAccount";

            return View("~/Views/Account/AccountIndex.cshtml");
        }

        [RoleAuthorize(new Naming.RoleID[] { Naming.RoleID.ROLE_SYS, Naming.RoleID.ROLE_SELLER })]
        public ActionResult Inquire(UserAccountQueryViewModel viewModel)
        {

            var profile = HttpContext.GetUser();

            ViewBag.ViewModel = viewModel;
            IQueryable<UserProfile> items = models.GetTable<UserProfile>();

            if (viewModel.SellerID.HasValue)
            {
                items = items.FilterByOrganization(models, viewModel.SellerID.Value);
            }
            else if (!profile.IsSystemAdmin())
            {
                items = items.FilterByOrganization(models, profile.CurrentUserRole.OrganizationCategory.CompanyID);
            }

            viewModel.PID = viewModel.PID.GetEfficientString();
            if (viewModel.PID != null)
            {
                items = items.Where(u => u.PID.StartsWith(viewModel.PID));
            }
            viewModel.UserName = viewModel.UserName.GetEfficientString();
            if (viewModel.UserName != null)
            {
                items = items.Where(u => u.UserName.Contains(viewModel.UserName));
            }
            if (viewModel.RoleID.HasValue)
            {
                items = (new UserProfileManager(models)).GetUserByUserRole(items, (int)viewModel.RoleID);
            }

            if (viewModel.LevelID.HasValue)
            {
                items = items.Where(u => u.UserProfileStatus.CurrentLevel == viewModel.LevelID);
            }

            viewModel.PageSize = viewModel.PageSize.HasValue && viewModel.PageSize > 0
                ? viewModel.PageSize.Value
                : ModelCore.Properties.AppSettings.Default.PageSize;

            if (viewModel.PageIndex.HasValue)
            {
                viewModel.PageIndex -= 1;
                return View("~/Views/Account/Module/ItemList.cshtml", items);
            }
            else
            {
                viewModel.PageIndex = 0;
                return View("~/Views/Account/Module/QueryResult.cshtml", items);
            }
        }

        [AuthorizedSysAdmin()]
        public ActionResult InquireSystemAccount(UserAccountQueryViewModel viewModel)
        {
            ViewResult result = (ViewResult)Inquire(viewModel);
            IQueryable<UserProfile> items = (IQueryable<UserProfile>)result.Model;
            items = items.Where(u => u.UserRole.Any(r => r.RoleID == (int)Naming.RoleID.ROLE_SYS));

            return View(result.ViewName, items);
        }

        public ActionResult DataItem(int? id)
        {
            var item = models.GetTable<UserProfile>().Where(d => d.UID == id).FirstOrDefault();

            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "帳號資料錯誤！");
            }

            return View("~/Views/Account/Module/DataItem.ascx", item);

        }

        public ActionResult SendConfirmation(int? id)
        {
            var item = models.GetTable<UserProfile>().Where(u => u.UID == id).FirstOrDefault();
            if (item != null)
            {
                item.NotifyToActivate();
                return View("~/Views/Shared/AlertMessageDialog.cshtml", model: "確認信已送出！");
            }

            return View("~/Views/Shared/AlertMessageDialog.cshtml", model: "帳號資料錯誤！");
        }

        public ActionResult Deactivate(int? id)
        {
            ViewResult result = (ViewResult)DataItem(id);
            var item = result.Model as UserProfile;
            if (item != null)
            {
                item.LevelID = (int)Naming.MemberStatusDefinition.Mark_To_Delete;
                models.SubmitChanges();
            }

            return result;
        }

        public ActionResult Activate(int? id)
        {
            ViewResult result = (ViewResult)DataItem(id);
            var item = result.Model as UserProfile;
            if (item != null)
            {
                item.LevelID = (int)Naming.MemberStatusDefinition.Checked;
                models.SubmitChanges();
            }

            return result;
        }

        public ActionResult DeleteItem(int? id)
        {
            var item = models.GetTable<UserProfile>().Where(d => d.UID == id).FirstOrDefault();

            if (item == null)
            {
                return Json(new { result = false, message = "帳號資料錯誤！" });
            }

            var profile = HttpContext.GetUser();
            if (!profile.IsSystemAdmin())
            {
                if (!models.GetTable<UserRole>().Any(r => r.UID == item.UID && r.OrgaCateID == profile.CurrentUserRole.OrgaCateID))
                {
                    return Json(new { result = false, message = "帳號非所屬會員使用者！" });
                }
            }

            try
            {
                models.GetTable<UserProfile>().DeleteOnSubmit(item);
                models.SubmitChanges();
            }
            catch (Exception ex)
            {
                return Json(new { result = false, message = ex.Message });
            }

            return Json(new { result = true });

        }

        [AllowAnonymous]
        public ActionResult CBESignUp(OrganizationViewModel viewModel)
        {
            Organization item = null;
            if (viewModel.KeyID != null)
            {
                viewModel.CompanyID = viewModel.DecryptKeyValue();
                item = models.GetTable<Organization>().Where(u => u.CompanyID == viewModel.CompanyID).FirstOrDefault();
            }

            viewModel.ApplyFromModel(item);

            ViewBag.ViewModel = viewModel;
            return View("~/Views/Organization/CBE_SignUp.cshtml", item);
        }

        public ActionResult CommitCBESignUp(OrganizationViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            if (viewModel.KeyID != null)
            {
                viewModel.CompanyID = viewModel.DecryptKeyValue();
            }

            viewModel.SettingInvoiceType = Naming.InvoiceTypeDefinition.一般稅額計算之電子發票;
            Organization item = viewModel.CommitOrganizationViewModel(models, ModelState);

            if (item == null)
            {
                return View("~/Views/Shared/ReportInputError.cshtml");
            }

            return Json(new { result = true });

        }

        public ActionResult ChangeLanguage(String lang)
        {
            var cLang = lang.GetEfficientString() ?? Settings.Default.DefaultUILanguage;
            Response.Cookies.Append("cLang", cLang);
            return Json(new { result = true, message = System.Globalization.CultureInfo.CurrentCulture.Name });
        }

        public ActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }

        public ActionResult CommitPassword(UserProfileViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            var profile = HttpContext.GetUser();
            if (profile == null)
            {
                return View("~/Views/Shared/AlertMessageDialog.cshtml", "資料錯誤，請重新登入！");
            }

            UserProfile item = profile.LoadInstance(models!);
            viewModel.PID = item?.PID;

            viewModel.UserProfileValueCheck(profile, ModelState);

            if (!ModelState.IsValid)
            {
                ViewBag.ModelState = this.ModelState;
                return View("~/Views/Shared/ReportInputError.cshtml");
            }

            item!.UpdatePassword(viewModel);
            models!.SubmitChanges();
            return new JavaScriptResult($"alert('密碼變更完成！'); window.location.href = '{VirtualPathUtility.ToAbsolute("~/Home/MainPage")}';");
        }


    }
}