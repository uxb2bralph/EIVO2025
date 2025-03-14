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
using Microsoft.AspNetCore.Mvc;
using CommonLib.Utility;

namespace TaskCenter.Controllers
{
    public class InvoiceQueryController : SampleController
    {
        public InvoiceQueryController(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
        }

        public ActionResult Inquire(InvoiceDataQueryViewModel viewModel)
        {
            Organization item = viewModel.CheckRequest(this);

            if (item != null)
            {
                switch (viewModel.QueryType)
                {
                    case DataQueryType.Invoice:
                    case DataQueryType.CountInvoice:
                        return JsonInvoice(viewModel, item);

                    case DataQueryType.VoidInvoice:
                    case DataQueryType.CountVoidInvoice:
                        return JsonVoidInvoice(viewModel, item);

                    case DataQueryType.Allowance:
                    case DataQueryType.CountAllowance:
                        return JsonAllowance(viewModel, item);

                    case DataQueryType.VoidAllowance:
                    case DataQueryType.CountVoidAllowance:
                        return JsonVoidAllowance(viewModel, item);

                    case DataQueryType.InovoiceNoAllocation:
                        return JsonInvoiceNoAllocation(viewModel, item);

                }
            }

            if (!ModelState.IsValid)
            {
                return Json(new { result = false, errorCode = ModelState.AllErrorKey() });
            }

            return Json(new { result = true });
        }

        public ActionResult JsonInvoiceNoAllocation(InvoiceDataQueryViewModel viewModel, Organization agent)
        {
            IQueryable<InvoiceTrackCodeAssignment> assignments = models!.GetTable<InvoiceTrackCodeAssignment>();
            IQueryable<InvoiceNoInterval> items = models.GetTable<InvoiceNoInterval>();

            viewModel.IssuerNo = viewModel.IssuerNo.GetEfficientString();
            if (viewModel.IssuerNo != null)
            {
                var orgItems = models.GetTable<Organization>().Where(c => c.ReceiptNo == viewModel.IssuerNo);
                var issuers = models.GetTable<InvoiceIssuerAgent>().Where(x => x.AgentID == agent.CompanyID)
                                        .Where(x => orgItems.Any(o => o.CompanyID == x.IssuerID));
                assignments = assignments.Where(a => issuers.Any(x => x.IssuerID == a.SellerID));
            }
            else
            {
                var issuers = models.GetTable<InvoiceIssuerAgent>().Where(x => x.AgentID == agent.CompanyID);
                assignments = assignments.Where(a => a.SellerID == agent.CompanyID
                                || issuers.Any(x => x.IssuerID == a.SellerID));
            }

            IQueryable<InvoiceTrackCode> trackItems = models.GetTable<InvoiceTrackCode>();
            if (viewModel.Year.HasValue)
            {
                trackItems = trackItems.Where(x => x.Year == viewModel.Year);
            }

            if (viewModel.PeriodNo.HasValue)
            {
                trackItems = trackItems.Where(x => x.PeriodNo == viewModel.PeriodNo);
            }

            assignments = assignments.Where(a => trackItems.Any(x => x.TrackID == a.TrackID));
            items = items.Where(i => assignments.Any(a => a.SellerID == i.SellerID && a.TrackID == i.TrackID));

            return View("~/Views/InvoiceQuery/JsonInvoiceNoAllocation.cshtml", items);

        }


        public ActionResult JsonInvoice(InvoiceDataQueryViewModel viewModel, Organization agent)
        {
            IQueryable<InvoiceItem> items = models!.GetInvoiceByAgent(models!.GetTable<InvoiceItem>(), agent.CompanyID);

            bool effective = false;
            items = items.InquireInvoice(viewModel, models, ref effective);

            if (viewModel.QueryType == DataQueryType.CountInvoice)
            {
                return Json(new { TotalCount = items.Count() });
            }
            else
            {
                if (viewModel.PageIndex > 0 && viewModel.PageSize > 0)
                {
                    items = items.Skip((viewModel.PageIndex.Value - 1) * viewModel.PageSize.Value)
                        .Take(viewModel.PageSize.Value);
                }

                var dataItems = items.Select(c => c.CreateInvoiceMIG(true)).ToList();
                return Content(dataItems.JsonStringify(), "application/json");
            }
        }

        public ActionResult JsonVoidInvoice(InvoiceDataQueryViewModel viewModel, Organization agent)
        {
            IQueryable<InvoiceItem> items = models!.GetInvoiceByAgent(models!.GetTable<InvoiceItem>(), agent.CompanyID);

            bool effective = false;
            items = items.InquireVoidInvoice(viewModel, models,ref effective);

            if (viewModel.QueryType == DataQueryType.CountVoidInvoice)
            {
                return Json(new { TotalCount = items.Count() });
            }
            else
            {
                if (viewModel.PageIndex > 0 && viewModel.PageSize > 0)
                {
                    items = items.Skip((viewModel.PageIndex.Value - 1) * viewModel.PageSize.Value)
                        .Take(viewModel.PageSize.Value);
                }

                var dataItems = items.Select(c => c.CreateInvoiceCancellationMIG(true)).ToList();
                return Content(dataItems.JsonStringify(), "application/json");
            }
        }

        public ActionResult JsonAllowance(InvoiceDataQueryViewModel viewModel, Organization agent)
        {
            IQueryable<InvoiceAllowance> items = models!.GetAllowanceByAgent(agent.CompanyID);
            bool effective = false;
            items = items.InquireAllowance(viewModel, models!, ref effective);

            if (viewModel.QueryType == DataQueryType.CountAllowance)
            {
                return Json(new { TotalCount = items.Count() });
            }
            else
            {
                if (viewModel.PageIndex > 0 && viewModel.PageSize > 0)
                {
                    items = items.Skip((viewModel.PageIndex.Value - 1) * viewModel.PageSize.Value)
                        .Take(viewModel.PageSize.Value);
                }
                var dataItems = items.Select(c => c.CreateAllowanceMIG(models, true)).ToList();
                return Content(dataItems.JsonStringify().Replace(".00000", ""), "application/json");
            }
        }

        public ActionResult JsonVoidAllowance(InvoiceDataQueryViewModel viewModel, Organization agent)
        {
            IQueryable<InvoiceAllowance> items = models!.GetAllowanceByAgent(agent.CompanyID);

            bool effective = false;
            items = items.InquireVoidAllowance(viewModel, models!, ref effective);

            if (viewModel.QueryType == DataQueryType.CountVoidAllowance)
            {
                return Json(new { TotalCount = items.Count() });
            }
            else
            {
                if (viewModel.PageIndex > 0 && viewModel.PageSize > 0)
                {
                    items = items.Skip((viewModel.PageIndex.Value - 1) * viewModel.PageSize.Value)
                        .Take(viewModel.PageSize.Value);
                }

                var dataItems = items.Select(c => c.CreateAllowanceCancellationMIG(true)).ToList();
                return Content(dataItems.JsonStringify(), "application/json");

            }

        }
    }
}