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
using Microsoft.EntityFrameworkCore;
using CommonLib.Utility;

namespace TaskCenter.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class InvoiceQueryController : SampleController
    {
        public InvoiceQueryController(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
        }

        [HttpPost("Inquire")]
        public async Task<ActionResult> InquireAsync([FromBody] InvoiceDataQueryViewModel viewModel)
        {
            if(viewModel == null)
            {
                viewModel = await PrepareViewModelAsync<InvoiceDataQueryViewModel>();
            }

            Organization? item = viewModel.CheckRequest(this);

            if (item != null)
            {
                switch (viewModel.QueryType)
                {
                    case DataQueryType.Invoice:
                    case DataQueryType.CountInvoice:
                        return await JsonInvoiceAsync(viewModel, item);

                    case DataQueryType.VoidInvoice:
                    case DataQueryType.CountVoidInvoice:
                        return await JsonVoidInvoiceAsync(viewModel, item);

                    case DataQueryType.Allowance:
                    case DataQueryType.CountAllowance:
                        return await JsonAllowanceAsync(viewModel, item);

                    case DataQueryType.VoidAllowance:
                    case DataQueryType.CountVoidAllowance:
                        return await JsonVoidAllowanceAsync(viewModel, item);

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

        private ActionResult JsonInvoiceNoAllocation(InvoiceDataQueryViewModel viewModel, Organization agent)
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


        private async Task<ActionResult> JsonInvoiceAsync(InvoiceDataQueryViewModel viewModel, Organization agent)
        {
            if (viewModel == null)
            {
                viewModel = await PrepareViewModelAsync<InvoiceDataQueryViewModel>();
            }

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

                var dataItems = items.AsNoTracking().ToList().Select(c => c.CreateF0401(true)).ToList();
                return Content(dataItems.JsonStringify(), "application/json");
            }
        }

        private async Task<ActionResult> JsonVoidInvoiceAsync(InvoiceDataQueryViewModel viewModel, Organization agent)
        {
            if (viewModel == null)
            {
                viewModel = await PrepareViewModelAsync<InvoiceDataQueryViewModel>();
            }

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

                var dataItems = items.AsNoTracking().ToList().Select(c => c.CreateF0501(true)).ToList();
                return Content(dataItems.JsonStringify(), "application/json");
            }
        }

        private async Task<ActionResult> JsonAllowanceAsync(InvoiceDataQueryViewModel viewModel, Organization agent)
        {
            if (viewModel == null)
            {
                viewModel = await PrepareViewModelAsync<InvoiceDataQueryViewModel>();
            }

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
                var dataItems = items.AsNoTracking().ToList().Select(c => c.CreateG0401(models, true)).ToList();
                return Content(dataItems.JsonStringify().Replace(".00000", ""), "application/json");
            }
        }

        private async Task<ActionResult> JsonVoidAllowanceAsync(InvoiceDataQueryViewModel viewModel, Organization agent)
        {
            if (viewModel == null)
            {
                viewModel = await PrepareViewModelAsync<InvoiceDataQueryViewModel>();
            }

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

                var dataItems = items.AsNoTracking().ToList().Select(c => c.CreateG0501(true)).ToList();
                return Content(dataItems.JsonStringify(), "application/json");

            }

        }
    }
}