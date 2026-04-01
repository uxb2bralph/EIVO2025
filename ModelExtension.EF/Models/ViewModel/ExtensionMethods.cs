using ModelCore.DataEntity;
using ModelCore.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelCore.Models.ViewModel
{
    public static class ExtensionMethods
    {
        public static OrganizationViewModel ApplyFromModel(this OrganizationViewModel viewModel, Organization item)
        {
            if (item != null)
            {
                viewModel.ContactName = item.ContactName;
                viewModel.Fax = item.Fax;
                viewModel.LogoURL = item.LogoURL;
                viewModel.CompanyName = item.CompanyName;
                viewModel.CompanyID = item.CompanyID;
                viewModel.ReceiptNo = item.ReceiptNo;
                viewModel.Phone = item.Phone;
                viewModel.ContactFax = item.ContactFax;
                viewModel.ContactPhone = item.ContactPhone;
                viewModel.ContactMobilePhone = item.ContactMobilePhone;
                viewModel.BusinessContactPhone = item.OrganizationExtension?.BusinessContactPhone;
                viewModel.RegAddr = item.RegAddr;
                viewModel.UndertakerName = item.UndertakerName;
                viewModel.CustomerNo = item.OrganizationExtension?.CustomerNo;
                viewModel.Addr = item.Addr;
                viewModel.EnglishName = item.EnglishName;
                viewModel.EnglishAddr = item.EnglishAddr;
                viewModel.EnglishRegAddr = item.EnglishRegAddr;
                viewModel.ContactEmail = item.ContactEmail;
                viewModel.UndertakerPhone = item.UndertakerPhone;
                viewModel.UndertakerFax = item.UndertakerFax;
                viewModel.UndertakerMobilePhone = item.UndertakerMobilePhone;
                viewModel.InvoiceSignature = item.InvoiceSignature;
                viewModel.UndertakerID = item.UndertakerID;
                viewModel.ContactTitle = item.ContactTitle;
                if (item.OrganizationCategory.Count > 0)
                {
                    viewModel.CategoryID = (CategoryDefinition.CategoryEnum?)item.OrganizationCategory.First().CategoryID;
                }

                viewModel.CurrentLevel = item.OrganizationStatus.CurrentLevel;
                viewModel.SetToPrintInvoice = item.OrganizationStatus.SetToPrintInvoice;
                viewModel.InvoicePrintView = item.OrganizationStatus.InvoicePrintView;
                viewModel.C0401POSView = item.OrganizationCustomSetting?.Settings?.C0401POSView;
                viewModel.CustomNotificationView = item.OrganizationStatus.CustomNotificationView;
                viewModel.IronSteelIndustry = item.OrganizationStatus.IronSteelIndustry;
                viewModel.Entrusting = item.OrganizationStatus.Entrusting;
                viewModel.AuthorizationNo = item.OrganizationStatus.AuthorizationNo;
                viewModel.SetToOutsourcingCS = item.OrganizationStatus.SetToOutsourcingCS;
                viewModel.AllowancePrintView = item.OrganizationStatus.AllowancePrintView;
                viewModel.SetToNotifyCounterpartBySMS = item.OrganizationStatus.SetToNotifyCounterpartBySMS;
                viewModel.DownloadDataNumber = item.OrganizationStatus.DownloadDataNumber;
                viewModel.DownloadDispatch = item.OrganizationStatus.DownloadDispatch;
                viewModel.UploadBranchTrackBlank = item.OrganizationStatus.UploadBranchTrackBlank;
                viewModel.PrintAll = item.OrganizationStatus.PrintAll;
                viewModel.SettingInvoiceType = (Naming.InvoiceTypeDefinition?)item.OrganizationStatus.SettingInvoiceType;
                viewModel.SubscribeB2BInvoicePDF = item.OrganizationStatus.SubscribeB2BInvoicePDF;
                viewModel.UseB2BStandalone = item.OrganizationStatus.UseB2BStandalone;
                viewModel.DisableIssuingNotice = item.OrganizationStatus.DisableIssuingNotice;
                viewModel.NoticeSetting = (Naming.InvoiceNoticeStatus?)item.OrganizationStatus.InvoiceNoticeSetting;
                viewModel.EntrustToPrint = item.OrganizationStatus.EntrustToPrint;
                viewModel.EnableTrackCodeInvoiceNoValidation = item.OrganizationStatus.EnableTrackCodeInvoiceNoValidation;
                viewModel.IgnoreDuplicatedDataNumber = item.OrganizationStatus.IgnoreDuplicatedDataNumber;
                viewModel.CustomNotification = item.OrganizationExtension?.CustomNotification;
                viewModel.Settings = item.OrganizationSettings.Select(s => s.Settings).ToArray();
                viewModel.ExpirationDate = item.OrganizationExtension?.ExpirationDate;
                viewModel.CreationDate = item.OrganizationExtension?.CreationDate;
                viewModel.AutoBlankTrack = item.OrganizationExtension?.AutoBlankTrack;
                viewModel.AutoBlankTrackEmittance = item.OrganizationExtension?.AutoBlankTrackEmittance;
                viewModel.TaxNo = item.OrganizationExtension?.TaxNo;
                viewModel.AuthorizationNotBefore = item.OrganizationExtension?.AuthorizationNotBefore;
                viewModel.AuthorizationNotAfter = item.OrganizationExtension?.AuthorizationNotAfter;
                viewModel.InvoiceRequestNotBefore = item.OrganizationExtension?.InvoiceRequestNotBefore;
                viewModel.InvoiceRequestNotAfter = item.OrganizationExtension?.InvoiceRequestNotAfter;
            }
            return viewModel;
        }

    }
}
