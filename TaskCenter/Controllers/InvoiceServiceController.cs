using ApplicationResource;
using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using ModelCore.Helper;
using TaskCenter.Helper.RequestAction;
using TaskCenter.Properties;
using ModelCore.InvoiceManagement;
using ModelCore.Security;
using System.Security.Cryptography;
using System.Xml;
using ModelCore.Schema.EIVO;
using ModelCore.Schema.TXN;
using Business.Helper.InvoiceProcessor;
using Microsoft.AspNetCore.Mvc;
using CommonLib.Utility;

namespace TaskCenter.Controllers
{
    public class InvoiceServiceController : SampleController
    {
        public InvoiceServiceController(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
        }

        public ActionResult UploadInvoiceAutoTrackNo(InvoiceRequestViewModel viewModel)
        {
            InvoiceRoot? invoice = FromJsonBody<InvoiceRequestViewModel>()?.InvoiceRoot;
            Root result = createMessageToken();
            if (invoice != null)
            {
                using (InvoiceManagerV3 manager = new InvoiceManagerV3 { InvoiceClientID = viewModel.ClientID, ProcessType = viewModel.ProcessType })
                {
                    OrganizationToken token = viewModel.CheckRequestToken(this);
                    manager.ApplyInvoiceDate = viewModel?.ApplyInvoiceDate;
                    manager.UploadInvoiceAutoTrackNo(invoice, result, token);
                }
            }
            return Content(result.JsonStringify(), "application/json");
        }

        public ActionResult UploadInvoice(InvoiceRequestViewModel viewModel)
        {
            Root result = createMessageToken();
            InvoiceRoot? invoice = FromJsonBody<InvoiceRequestViewModel>()?.InvoiceRoot;
            if (invoice != null)
            {
                using (InvoiceManagerV3 manager = new InvoiceManagerV3 { InvoiceClientID = viewModel?.ClientID, ProcessType = viewModel?.ProcessType })
                {
                    OrganizationToken token = viewModel.CheckRequestToken(this);
                    manager.UploadInvoice(invoice, result, token);
                }
            }


            return Content(result.JsonStringify(), "application/json");
        }

        public ActionResult UploadInvoiceCancellation(InvoiceRequestViewModel viewModel)
        {
            Root result = createMessageToken();
            CancelInvoiceRoot item = FromJsonBody<InvoiceRequestViewModel>().CancelInvoiceRoot;
            using (InvoiceManagerV3 manager = new InvoiceManagerV3 { })
            {
                OrganizationToken token = viewModel.CheckRequestToken(this);
                manager.UploadInvoiceCancellation(result, item, token);
            }
            return Content(result.JsonStringify(), "application/json");
        }

        public ActionResult UploadAllowance(InvoiceRequestViewModel viewModel)
        {
            Root result = createMessageToken();
            AllowanceRoot allowance = FromJsonBody<InvoiceRequestViewModel>().AllowanceRoot;
            using (InvoiceManagerV3 manager = new InvoiceManagerV3 { })
            {
                OrganizationToken token = viewModel.CheckRequestToken(this);
                manager.UploadAllowance(result, allowance, token);
            }
            return Content(result.JsonStringify(), "application/json");
        }

        public ActionResult UploadAllowanceCancellation(InvoiceRequestViewModel viewModel)
        {
            Root result = createMessageToken();
            CancelAllowanceRoot? item = FromJsonBody<InvoiceRequestViewModel>()?.CancelAllowanceRoot;
            if (item != null)
            {
                using (InvoiceManagerV3 manager = new InvoiceManagerV3 { })
                {
                    OrganizationToken token = viewModel.CheckRequestToken(this);
                    manager.UploadAllowanceCancellation(result, item, token);
                }
            }
            return Content(result.JsonStringify(), "application/json");
        }

        public ActionResult GetStorageToken(InvoiceRequestViewModel viewModel)
        {
            String? storageToken = null;
            viewModel.StoragePath = viewModel?.StoragePath.GetEfficientString();
            if(viewModel?.StoragePath !=null)
            {
                using (InvoiceManagerV3 manager = new InvoiceManagerV3 { InvoiceClientID = viewModel.ClientID, ProcessType = viewModel.ProcessType })
                {
                    OrganizationToken token = viewModel.CheckRequestToken(this);
                    if (token != null)
                    {
                        storageToken = Path.Combine($"{token?.CompanyID:00000000}", viewModel.StoragePath).EncryptData();
                    }
                }
            }

            return Json(new { StorageToken = storageToken });
        }

        protected Root createMessageToken()
        {
            Root result = new Root
            {
                UXB2B = "電子發票系統",
                Result = new RootResult
                {
                    timeStamp = DateTime.Now,
                    value = 0
                }
            };
            return result;
        }
    }
}