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

using ClosedXML.Excel;
using ModelCore.Models.ViewModel;
using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Security.MembershipManagement;
using CommonLib.Utility;
using Microsoft.AspNetCore.Mvc;
using ModelCore.Helper;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ModelCore.Helper
{
    public static class MvcValidation
    {
        public static void OrganizationValueCheck(this OrganizationViewModel viewModel, ModelStateDictionary modelState)
        {
            viewModel.CompanyName = viewModel.CompanyName.GetEfficientString();
            if (viewModel.CompanyName==null)
            {
                //檢查名稱
                modelState.AddModelError("CompanyName", "請輸入公司名稱!!");
                
            }
            viewModel.ReceiptNo = viewModel.ReceiptNo.GetEfficientString();
            if (String.IsNullOrEmpty(viewModel.ReceiptNo))
            {
                //檢查名稱
                modelState.AddModelError("ReceiptNo", "請輸入公司統編!!");
                
            }
            viewModel.Addr = viewModel.Addr.GetEfficientString();
            //if (String.IsNullOrEmpty(viewModel.Addr))
            //{
            //    //檢查名稱
            //    modelState.AddModelError("Addr", "請輸入公司地址!!");
                
            //}
            viewModel.Phone = viewModel.Phone.GetEfficientString();
            //if (String.IsNullOrEmpty(viewModel.Phone))
            //{
            //    //檢查名稱
            //    modelState.AddModelError("Phone", "請輸入公司電話!!");
                
            //}

            viewModel.ContactEmail = viewModel.ContactEmail.GetEfficientString();
            if (viewModel.ContactEmail != null)
            {
                Regex reg = new Regex("\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*");
                if (!reg.IsMatch(viewModel.ContactEmail))
                {
                    //檢查email
                    modelState.AddModelError("ContactEmail", "電子信箱尚未輸入或輸入錯誤!!");
                }
            }

            viewModel.TaxNo = viewModel.TaxNo.GetEfficientString();
            if (viewModel.TaxNo != null)
            {
                if(!Regex.IsMatch(viewModel.TaxNo,"\\d{9}"))
                {
                    modelState.AddModelError("TaxNo", "請輸入9位數稅籍編號!!");
                }
            }

        }

        public static void UserProfileValueCheck(this UserProfileViewModel viewModel, UserProfile profile, ModelStateDictionary modelState,bool forcedPassword = true)
        {
            viewModel.PID = viewModel.PID.GetEfficientString();
            if (String.IsNullOrEmpty(viewModel.PID))
            {
                modelState.AddModelError("PID", "帳號不可為空白!!");
            }

            viewModel.Password = viewModel.Password.GetEfficientString();
            viewModel.Password1 = viewModel.Password1.GetEfficientString();
            if (forcedPassword)
            {
                if (viewModel.Password != null)
                {
                    Regex reg = new Regex("^(?=.*\\d)(?=.*[a-zA-Z])");
                    if (viewModel.Password.Length < 6)
                    {
                        //檢查密碼
                        modelState.AddModelError("PassWord", "密碼不可少於６個字碼!!");
                    }

                    else if (viewModel.Password != viewModel.Password1)
                    {
                        //檢查密碼
                        modelState.AddModelError("PassWord1", "二組密碼輸入不同!!");
                    }
                    else
                    {
                        if (profile.IsSystemAdmin())
                        {
                        }
                        else if (!reg.IsMatch(viewModel.Password))
                        {
                            //檢查密碼
                            modelState.AddModelError("PassWord", "密碼須由英文、數字組成!!");
                        }
                    }
                }
            }
        }

        public static void UserRoleValueCheck(this UserRoleViewModel viewModel, UserProfile profile, ModelStateDictionary modelState)
        {
            if(!viewModel.SellerID.HasValue)
            {
                modelState.AddModelError("SellerID", "請選擇所屬會員!!");
            }

            if(!viewModel.UID.HasValue)
            {
                modelState.AddModelError("UID", "帳號不存在!!");
            }

            if (!viewModel.RoleID.HasValue)
            {
                modelState.AddModelError("RoleID", "請選擇身份設定!!");
            }
            else if (!profile.IsSystemAdmin())
            {
                if (!Enum.IsDefined(typeof(Naming.EIVOMemberRoleID), (int)viewModel.RoleID))
                {
                    modelState.AddModelError("RoleID", "請選擇身份設定!!");
                }
            }
        }

        public static void CustomSmtpHostValueCheck(this CustomSmtpHost viewModel, ModelStateDictionary modelState)
        {
            viewModel.Host = viewModel.Host.GetEfficientString();
            if (viewModel.Host == null)
            {
                //檢查名稱
                modelState.AddModelError("Host", "請輸入郵件伺服器!!");

            }

            viewModel.MailFrom = viewModel.MailFrom.GetEfficientString();
            if (String.IsNullOrEmpty(viewModel.MailFrom))
            {
                //檢查名稱
                modelState.AddModelError("MailFrom", "請輸入寄件人email!!");
            }

        }

    }
}