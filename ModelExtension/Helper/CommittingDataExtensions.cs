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
using ModelCore.Models.ViewModel;
using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Security.MembershipManagement;
using CommonLib.Utility;
using ModelCore.Helper;
using CommonLib.DataAccess;
using ModelCore.Notification;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ModelCore.Helper
{
    public static class CommittingDataExtensions
    {
        public static Organization CommitOrganizationViewModel(this OrganizationViewModel viewModel, GenericManager<EIVOEntityDataContext> models, ModelStateDictionary modelState)
        {
            viewModel.OrganizationValueCheck(modelState);

            if (viewModel.KeyID != null)
            {
                viewModel.CompanyID = viewModel.DecryptKeyValue();
            }
            Organization item = models.GetTable<Organization>().Where(u => u.CompanyID == viewModel.CompanyID).FirstOrDefault();

            if (viewModel.ReceiptNo != null)
            {
                if (item == null || item.ReceiptNo != viewModel.ReceiptNo)
                {
                    if (models.GetTable<Organization>().Any(o => o.ReceiptNo == viewModel.ReceiptNo))
                    {
                        modelState.AddModelError("ReceiptNo", "相同的企業統編已存在!!");
                    }
                }
            }

            if (!viewModel.CategoryID.HasValue)
            {
                modelState.AddModelError("CategoryID", "請設定公司類別!!");
            }

            if (!viewModel.SettingInvoiceType.HasValue)
            {
                modelState.AddModelError("SettingInvoiceType", "請設定發票類別!!");
            }

            if (!modelState.IsValid)
            {
                return null;
            }

            OrganizationCategory orgaCate = null;
            if (item == null)
            {
                item = new Organization
                {
                };

                models.GetTable<Organization>().InsertOnSubmit(item);
            }

            if (item.OrganizationStatus == null)
            {
                item.OrganizationStatus = new OrganizationStatus
                {
                    CurrentLevel = (int)Naming.MemberStatusDefinition.Wait_For_Check
                };
            }

            if (item.OrganizationExtension == null)
            {
                item.OrganizationExtension = new OrganizationExtension { };
            }

            if (item.OrganizationCustomSetting == null)
            {
                item.OrganizationCustomSetting = new OrganizationCustomSetting { };
            }

            orgaCate = item.OrganizationCategory.FirstOrDefault();
            if (orgaCate == null)
            {
                orgaCate = new OrganizationCategory
                {
                    Organization = item
                };

            }

            orgaCate.CategoryID = (int)viewModel.CategoryID;

            item.ReceiptNo = viewModel.ReceiptNo;
            item.CompanyName = viewModel.CompanyName;
            item.Addr = viewModel.Addr;
            item.Phone = viewModel.Phone;
            item.Fax = viewModel.Fax;
            item.UndertakerName = viewModel.UndertakerName;
            item.ContactName = viewModel.ContactName;
            item.ContactTitle = viewModel.ContactTitle;
            item.ContactPhone = viewModel.ContactPhone;
            item.ContactMobilePhone = viewModel.ContactMobilePhone;
            item.ContactEmail = viewModel.ContactEmail;
            item.OrganizationExtension.CustomerNo = viewModel.CustomerNo.GetEfficientString();
            item.OrganizationStatus.SetToPrintInvoice = viewModel.SetToPrintInvoice;
            item.OrganizationStatus.SetToOutsourcingCS = viewModel.SetToOutsourcingCS;
            item.OrganizationStatus.InvoicePrintView = viewModel.SetToPrintInvoice == true ? viewModel.InvoicePrintView.GetEfficientString() : null;
            item.OrganizationCustomSetting.Settings.C0401POSView = viewModel.SetToPrintInvoice == true ? viewModel.C0401POSView.GetEfficientString() : null;
            item.OrganizationCustomSetting.Accept();
            item.OrganizationStatus.AllowancePrintView = viewModel.SetToPrintInvoice == true ? viewModel.AllowancePrintView.GetEfficientString() : null;
            item.OrganizationStatus.CustomNotificationView = viewModel.CustomNotificationView.GetEfficientString();

            item.OrganizationStatus.AuthorizationNo = viewModel.AuthorizationNo.GetEfficientString();
            item.OrganizationStatus.SetToNotifyCounterpartBySMS = viewModel.SetToNotifyCounterpartBySMS;
            item.OrganizationStatus.DownloadDataNumber = viewModel.DownloadDataNumber;
            item.OrganizationStatus.UploadBranchTrackBlank = viewModel.UploadBranchTrackBlank;
            item.OrganizationStatus.PrintAll = viewModel.PrintAll;
            item.OrganizationStatus.SettingInvoiceType = (int?)viewModel.SettingInvoiceType;
            item.OrganizationStatus.SubscribeB2BInvoicePDF = viewModel.SubscribeB2BInvoicePDF;
            item.OrganizationStatus.UseB2BStandalone = viewModel.UseB2BStandalone;
            item.OrganizationStatus.DisableIssuingNotice = viewModel.DisableIssuingNotice ?? true;
            item.OrganizationStatus.InvoiceNoticeSetting = viewModel.NoticeStatus != null && viewModel.NoticeStatus.Length > 0 ? viewModel.NoticeStatus.Sum() : (int?)null;
            item.OrganizationStatus.DisableWinningNotice = (item.OrganizationStatus.InvoiceNoticeSetting & (int)Naming.InvoiceNoticeStatus.Winning) == 0;
            item.OrganizationStatus.EntrustToPrint = viewModel.EntrustToPrint == true;
            item.OrganizationStatus.EnableTrackCodeInvoiceNoValidation = viewModel.EnableTrackCodeInvoiceNoValidation;
            item.OrganizationStatus.IgnoreDuplicatedDataNumber = viewModel.IgnoreDuplicatedDataNumber;
            item.OrganizationStatus.DownloadDispatch = viewModel.DownloadDispatch;

            var extension = item.OrganizationExtension;
            if (extension == null)
            {
                extension = item.OrganizationExtension = new OrganizationExtension { };
            }
            extension.CustomNotification = HttpUtility.HtmlDecode(viewModel.CustomNotification.GetEfficientString());
            extension.BusinessContactPhone = viewModel.BusinessContactPhone;
            extension.ExpirationDate = viewModel.ExpirationDate;
            extension.CreationDate = viewModel.CreationDate;
            extension.AutoBlankTrack = viewModel.AutoBlankTrack;
            extension.AutoBlankTrackEmittance = viewModel.AutoBlankTrackEmittance;
            extension.CustomerNo = viewModel.CustomerNo.GetEfficientString();
            extension.TaxNo = viewModel.TaxNo.GetEfficientString();
            extension.AuthorizationNotBefore = viewModel.AuthorizationNotBefore;
            extension.AuthorizationNotAfter = viewModel.AuthorizationNotAfter;
            extension.InvoiceRequestNotBefore = viewModel.InvoiceRequestNotBefore;
            extension.InvoiceRequestNotAfter = viewModel.InvoiceRequestNotAfter;

            models.SubmitChanges();

            models.ExecuteCommand("delete OrganizationSettings where CompanyID = {0}", item.CompanyID);
            if (viewModel.Settings != null && viewModel.Settings.Length > 0)
            {
                foreach (var setting in viewModel.Settings.Distinct().Select(s => s.GetEfficientString()))
                {
                    if (setting == null)
                    {
                        continue;
                    }

                    item.OrganizationSettings.Add(new OrganizationSettings
                    {
                        Settings = setting
                    });
                }

                models.SubmitChanges();
            }

            //if (isNewItem)
            //{
            //    models.CreateDefaultUser(item, orgaCate);
            //}

            if (orgaCate.CategoryID == (int)CategoryDefinition.CategoryEnum.經銷商)
            {
                if (!models.GetTable<InvoiceIssuerAgent>().Any(a => a.IssuerID == item.CompanyID && a.AgentID == item.CompanyID))
                {
                    models.ExecuteCommand(
                            @"INSERT INTO InvoiceIssuerAgent
                                (AgentID, IssuerID)
                                VALUES ({0},{0})", item.CompanyID);
                }
            }
            else
            {
                models.ExecuteCommand(
                    @"DELETE FROM InvoiceIssuerAgent
                        WHERE (AgentID = {0}) AND (IssuerID = {0})", item.CompanyID);
            }

            return item;
        }

        public static Organization CommitOrganizationViewModelForBusiness(this OrganizationViewModel viewModel, GenericManager<EIVOEntityDataContext> models, ModelStateDictionary modelState)
        {
            viewModel.OrganizationValueCheck(modelState);

            if (viewModel.KeyID != null)
            {
                viewModel.CompanyID = viewModel.DecryptKeyValue();
            }

            Organization item = models.GetTable<Organization>().Where(u => u.CompanyID == viewModel.CompanyID).FirstOrDefault();

            if (item == null || item.ReceiptNo != viewModel.ReceiptNo)
            {
                modelState.AddModelError("ReceiptNo", "營業人統編不符!!");
            }

            if (!modelState.IsValid)
            {
                return null;
            }

            item.CompanyName = viewModel.CompanyName;
            item.Addr = viewModel.Addr;
            item.Phone = viewModel.Phone;
            item.Fax = viewModel.Fax;
            item.UndertakerName = viewModel.UndertakerName;
            item.ContactName = viewModel.ContactName;
            item.ContactTitle = viewModel.ContactTitle;
            item.ContactPhone = viewModel.ContactPhone;
            item.ContactMobilePhone = viewModel.ContactMobilePhone;
            item.ContactEmail = viewModel.ContactEmail;
            item.OrganizationExtension.CustomerNo = viewModel.CustomerNo.GetEfficientString();

            var extension = item.OrganizationExtension;
            extension.CustomerNo = viewModel.CustomerNo.GetEfficientString();
            extension.TaxNo = viewModel.TaxNo.GetEfficientString();
            extension.AuthorizationNotBefore = viewModel.AuthorizationNotBefore;
            extension.AuthorizationNotAfter = viewModel.AuthorizationNotAfter;
            extension.InvoiceRequestNotBefore = viewModel.InvoiceRequestNotBefore;
            extension.InvoiceRequestNotAfter = viewModel.InvoiceRequestNotAfter;

            models.SubmitChanges();

            return item;
        }

        public static UserProfile CommitUserProfileViewModel(this UserProfileViewModel viewModel, GenericManager<EIVOEntityDataContext> models, ModelStateDictionary modelState, UserProfile creator)
        {
            UserProfile item = null;

            if (!String.IsNullOrEmpty(viewModel.KeyID))
            {
                viewModel.UID = viewModel.DecryptKeyValue();
            }

            item = models.GetTable<UserProfile>().Where(u => u.UID == viewModel.UID).FirstOrDefault();

            viewModel.UserProfileValueCheck(creator, modelState);
            if (viewModel.PID != null)
            {
                if ((item != null && models.GetTable<UserProfile>().Any(u => u.UID != item.UID && u.PID == viewModel.PID))
                   || (item == null && models.GetTable<UserProfile>().Any(u => u.PID == viewModel.PID)))
                {
                    modelState.AddModelError("PID", "這個帳號已被使用，請更換申請帳號!!");
                }
            }

            if (item == null)
            {
                if (viewModel.Password == null)
                {
                    ///新增帳號
                    ///
                    modelState.AddModelError("Password", "密碼不可為空白!!");
                }
            }


            if (!modelState.IsValid)
            {
                return null;
            }

            bool isNew = false;
            if (item == null)
            {
                item = new UserProfile
                {
                    UserProfileStatus = new UserProfileStatus
                    {
                        CurrentLevel = (int)Naming.MemberStatusDefinition.Wait_For_Check
                    },
                    Creator = creator.UID,
                };

                models.GetTable<UserProfile>().InsertOnSubmit(item);
                isNew = true;
            }

            item.PID = viewModel.PID;
            item.UserName = viewModel.UserName;
            UpdatePassword(item, viewModel);

            item.EMail = viewModel.EMail;
            item.MailID = item.EMail.GetEfficientString()?
                .Split(';', ',', ',')?[0];

            item.Address = viewModel.Address;
            item.Phone = viewModel.Phone;
            item.MobilePhone = viewModel.MobilePhone;
            item.Phone2 = viewModel.Phone2;

            models.SubmitChanges();

            models.ExecuteCommand("delete ResetUserPassword where UID = {0}", item.UID);

            if (isNew)
            {
                PortalNotification.NotifyToActivate(item);
                viewModel.UID = item.UID;
            }

            viewModel.KeyID = null;
            viewModel.CommitUserRoleViewModel(models, modelState, creator);

            return item;
        }

        public static void UpdatePassword(this UserProfile item, UserProfileViewModel viewModel)
        {
            if (!String.IsNullOrEmpty(viewModel.Password))
            {
                item.Password2 = ValidityAgent.MakePassword(viewModel.Password);
                if (viewModel.WaitForCheck == true)
                    item.UserProfileStatus.CurrentLevel = (int)Naming.MemberStatusDefinition.Checked;
            }
            item.Expiration = DateTime.Today.AddDays(ModelExtension.Properties.AppSettings.Default.UserPasswordValidDays);
        }

        public static UserRole CommitUserRoleViewModel(this UserRoleViewModel viewModel, GenericManager<EIVOEntityDataContext> models, ModelStateDictionary modelState, UserProfile creator)
        {
            UserRoleViewModel tmp = viewModel;
            if (!String.IsNullOrEmpty(viewModel.KeyID))
            {
                tmp = JsonConvert.DeserializeObject<UserRoleViewModel>(viewModel.KeyID.DecryptData());
            }

            viewModel.UserRoleValueCheck(creator, modelState);

            int? orgaCateID = models.GetTable<OrganizationCategory>().Where(c => c.CompanyID == viewModel.SellerID)
                    .Select(c => c.OrgaCateID).FirstOrDefault();

            if (!orgaCateID.HasValue)
            {
                modelState.AddModelError("SellerID", "請選擇所屬會員!!");
            }

            if (!modelState.IsValid)
            {
                return null;
            }

            models.ExecuteCommand("delete UserRole where UID = {0}", viewModel.UID);

            UserRole item = new UserRole
            {
                OrgaCateID = orgaCateID.Value,
                RoleID = ((int?)viewModel.RoleID) ?? (int)Naming.EIVOUserRoleID.會員,
                UID = viewModel.UID.Value,
            };

            models.GetTable<UserRole>().InsertOnSubmit(item);
            models.SubmitChanges();

            return item;
        }

        public static UserProfile CreateDefaultUser(this GenericManager<EIVOEntityDataContext> models, Organization item, OrganizationCategory orgaCate)
        {
            var userProfile = new UserProfile
            {
                PID = item.ReceiptNo,
                Phone = item.Phone,
                EMail = item.ContactEmail,
                Address = item.Addr,
                UserProfileExtension = new UserProfileExtension
                {
                    IDNo = item.ReceiptNo
                },
                UserProfileStatus = new UserProfileStatus
                {
                    CurrentLevel = (int)Naming.MemberStatusDefinition.Wait_For_Check
                }
            };
            userProfile.MailID = userProfile.EMail.GetEfficientString()?
                .Split(';', ',', ',')?[0];
            models.GetTable<UserRole>().InsertOnSubmit(new UserRole
            {
                RoleID = (int)Naming.RoleID.ROLE_SELLER,
                UserProfile = userProfile,
                OrganizationCategory = orgaCate
            });

            models.SubmitChanges();

            return userProfile;
        }

        public static BusinessRelationship CommitBusinessRelationshipViewModel(this BusinessRelationshipViewModel viewModel, GenericManager<EIVOEntityDataContext> models, ModelStateDictionary modelState)
        {
            BusinessRelationship item = null;
            Organization orgItem = models.GetTable<Organization>().Where(o => o.ReceiptNo == viewModel.ReceiptNo).FirstOrDefault();

            if (orgItem == null)
            {
                orgItem = CommitOrganizationViewModel(viewModel, models, modelState);
            }

            Organization masterItem = models.GetTable<Organization>().Where(m => m.CompanyID == viewModel.MasterID).FirstOrDefault();
            if (masterItem == null)
            {
                masterItem = models.GetTable<Organization>().Where(m => m.ReceiptNo == viewModel.MasterNo).FirstOrDefault();
            }

            if (masterItem == null)
            {
                masterItem = (new OrganizationViewModel
                {
                    ReceiptNo = viewModel.MasterNo,
                    CompanyName = viewModel.MasterName,
                }).CommitOrganizationViewModel(models, modelState);
            }

            if (masterItem == null || orgItem == null)
            {
                return null;
            }

            if (!viewModel.BusinessID.HasValue)
            {
                viewModel.BusinessID = Naming.InvoiceCenterBusinessType.銷項;
            }

            item = models.GetTable<BusinessRelationship>()
                .Where(b => b.MasterID == masterItem.CompanyID)
                .Where(b => b.RelativeID == orgItem.CompanyID)
                .Where(b => b.BusinessID == (int)viewModel.BusinessID)
                .FirstOrDefault();

            if (item == null)
            {
                item = new BusinessRelationship
                {
                    RelativeID = orgItem.CompanyID,
                    BusinessID = (int)viewModel.BusinessID,
                    MasterID = masterItem.CompanyID,
                    CurrentLevel = viewModel.CompanyStatus
                };
                models.GetTable<BusinessRelationship>().InsertOnSubmit(item);
            }

            item.CompanyName = viewModel.CompanyName.GetEfficientString();
            item.ContactEmail = viewModel.ContactEmail.GetEfficientString();
            item.Addr = viewModel.Addr.GetEfficientString();
            item.Phone = viewModel.Phone.GetEfficientString();
            item.CustomerNo = viewModel.CustomerNo.GetEfficientString();

            models.SubmitChanges();

            return item;
        }

        public static CustomSmtpHost CommitCustomSmtpHost(this CustomSmtpHost viewModel, GenericManager<EIVOEntityDataContext> models, ModelStateDictionary modelState)
        {
            viewModel.CustomSmtpHostValueCheck(modelState);

            Organization orgItem = LoadCustomSmtpHostFor(viewModel, models);

            if (orgItem == null)
            {
                modelState.AddModelError("Message", "營業人資料錯誤!!");
            }

            if (!modelState.IsValid)
            {
                return null;
            }

            CustomSmtpHost item = models.GetTable<CustomSmtpHost>()
                .Where(u => u.CompanyID == viewModel.CompanyID)
                .OrderByDescending(u => u.HostID)
                .FirstOrDefault();

            if (item == null)
            {
                item = new CustomSmtpHost
                {
                    CompanyID = orgItem.CompanyID,
                };

                models.GetTable<CustomSmtpHost>().InsertOnSubmit(item);
            }

            item.Host = viewModel.Host;
            item.Port = viewModel.Port ?? 25;
            item.EnableSsl = viewModel.EnableSsl ?? false;
            item.UserName = viewModel.UserName;
            item.Password = viewModel.Password;
            item.MailFrom = viewModel.MailFrom;
            item.Status = (int)CustomSmtpHost.StatusType.Enabled;

            models.SubmitChanges();

            return item;
        }

        public static Organization LoadCustomSmtpHostFor(this CustomSmtpHost viewModel, GenericManager<EIVOEntityDataContext> models)
        {
            if (viewModel.KeyID != null)
            {
                viewModel.CompanyID = viewModel.DecryptKeyValue();
            }

            Organization orgItem = models.GetTable<Organization>()
                                    .Where(o => o.CompanyID == viewModel.CompanyID)
                                    .FirstOrDefault();
            return orgItem;
        }
    }
}