using ApplicationResource;
using CommonLib.Utility;
using Microsoft.AspNetCore.Mvc;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.Locale;


using ModelCore.Security;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

using TaskCenter.Helper.RequestAction;
using TaskCenter.Properties;
using ModelCore.InvoiceManagement;
using ModelCore.Models.ViewModel;

namespace TaskCenter.Controllers
{
    public class AccountController : SampleController
    {
        public AccountController(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
        }

        public ActionResult Auth([FromBody] AuthTokenViewModel viewModel)
        {
            /**
                Seed: RANDOM[16]
                Authorization: Base64(ToHexString(SHA256([Vendor 統編] + [Activation Key] + [Seed])))
	            SellerID: "[商家統編]"
             */

            var seller = models!.GetTable<Organization>().Where(c => c.ReceiptNo == viewModel.SellerID).FirstOrDefault();
            OrganizationToken token = null;
            if (seller != null)
            {
                token = models!.CheckAuthToken(seller, viewModel);
                if (token != null)
                {
                    if (viewModel.SampleQuery == true)
                    {
                        return Content((new InvoiceDataQueryViewModel
                        {
                            AccessToken = token.CompanyID.EncryptKey(),
                            DateFrom = DateTime.Today,
                        }).JsonStringify(), "application/json");
                    }
                    else
                    {
                        return Content((new AuthQueryViewModel
                        {
                            AccessToken = token.CompanyID.EncryptKey(),
                        }).JsonStringify(), "application/json");
                    }
                }
            }

            return Json(new 
            {
                AccessToken = (String?)null,
            });
        }

        public ActionResult BuildAuth([FromBody] AuthTokenViewModel viewModel)
        {
            viewModel.SellerID = viewModel.SellerID.GetEfficientString();
            var token = models!.GetTable<Organization>().Where(c => c.ReceiptNo == viewModel.SellerID)
                    .FirstOrDefault()?.OrganizationToken;

            if (token != null)
            {
                viewModel.Seed = viewModel.Seed.GetEfficientString();
                if (viewModel.Seed == null)
                {
                    viewModel.Seed = $"{DateTime.Now.Ticks % 100000000:00000000}";
                }

                using (SHA256 hash = SHA256.Create())
                {
                    viewModel.Authorization = token.ComputeAuthorization(hash, viewModel.Seed);
                }
            }

            return Content(viewModel.JsonStringify(), "application/json");
        }
    }
}