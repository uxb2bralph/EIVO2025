using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using Microsoft.AspNetCore.Mvc;



using ClosedXML.Excel;
using CommonLib.DataAccess;
using WebHome.Helper;
using WebHome.Models;
using WebHome.Models.ViewModel;
using ModelCore.Models.ViewModel;
using ModelCore.DataEntity;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;
using ModelCore.Helper;
using ModelCore.Resource;

using CommonLib.Utility;
using ModelCore.InvoiceManagement.InvoiceProcess;
using ModelCore.Helper;
using Microsoft.AspNetCore.Authorization;

namespace WebHome.Controllers
{
    [Authorize]
    public class ProcessRequestController : SampleController<InvoiceItem>
    {
        public ProcessRequestController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // GET: ProcessRequest
        public ActionResult QueryIndex(ProcessRequestQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View("~/Views/ProcessRequest/Module/ProcessRequestQuery.cshtml");
        }

        public ActionResult ShowData(ProcessRequestQueryViewModel viewModel)
        {
            ViewResult result = (ViewResult)InquireRequest(viewModel);
            viewModel.ResultView = "~/Views/ProcessRequest/Module/ProcessRequestTable.cshtml";
            result.ViewName = "~/Views/ProcessRequest/ShowData.cshtml";
            return result;
        }


        public ActionResult InquireRequest(ProcessRequestQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            var profile = HttpContext.GetUser();

            if (viewModel.KeyID != null)
            {
                viewModel.TaskID = viewModel.DecryptKeyValue();
            }

            IQueryable<ProcessRequest> items = models.FilterProcessRequestByRole(profile, models.GetTable<ProcessRequest>());
            if(viewModel.TaskID.HasValue)
            {
                items = items.Where(p => p.TaskID == viewModel.TaskID);
            }

            if(viewModel.SubmitDateFrom.HasValue)
            {
                items = items.Where(p => p.SubmitDate >= viewModel.SubmitDateFrom);
            }

            if (viewModel.SubmitDateTo.HasValue)
            {
                items = items.Where(p => p.SubmitDate < viewModel.SubmitDateTo.Value.AddDays(1));
            }

            if (viewModel.ProcessStartFrom.HasValue)
            {
                items = items.Where(p => p.ProcessStart >= viewModel.ProcessStartFrom);
            }

            if (viewModel.ProcessStartTo.HasValue)
            {
                items = items.Where(p => p.ProcessStart < viewModel.ProcessStartTo.Value.AddDays(1));
            }

            if (viewModel.ProcessCompleteFrom.HasValue)
            {
                items = items.Where(p => p.ProcessComplete >= viewModel.ProcessCompleteFrom);
            }

            if (viewModel.ProcessCompleteTo.HasValue)
            {
                items = items.Where(p => p.ProcessComplete < viewModel.ProcessCompleteTo.Value.AddDays(1));
            }

            viewModel.QuickSearch = viewModel.QuickSearch.GetEfficientString();
            if (viewModel.QuickSearch != null)
            {
                items = items.Where(p => p.UserProfile.PID == viewModel.QuickSearch
                    || p.Organization.ReceiptNo == viewModel.QuickSearch);
            }

            viewModel.RecordCount = items.Count();
            if (viewModel.PageIndex.HasValue)
            {
                viewModel.PageIndex--;
                return View("~/Views/ProcessRequest/Module/ProcessRequestTable.cshtml", items);
            }
            else
            {
                viewModel.ResultView = "~/Views/ProcessRequest/Module/ProcessRequestTable.cshtml";
                return View("~/Views/Common/Module/QueryResult.cshtml", items);
            }

        }

    }
}