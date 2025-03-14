using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using Microsoft.AspNetCore.Mvc;

using System.Xml;


using ClosedXML.Excel;
using WebHome.Helper;
using WebHome.Models;
using WebHome.Models.ViewModel;
using ModelCore.Models.ViewModel;
using WebHome.Properties;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;

using ModelCore.Helper;
using CommonLib.Utility;
using CommonLib.DataAccess;
using Microsoft.AspNetCore.Authorization;

namespace WebHome.Controllers
{
    [Authorize]
    public class InvoiceAuditController : SampleController<InvoiceItem>
    {
        public InvoiceAuditController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // GET: InvoiceAudit
        public ActionResult QueryIndex(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            viewModel.QueryForm = "~/Views/InvoiceAudit/Module/InvoiceQuery.cshtml";
            viewModel.DocType = Naming.DocumentTypeDefinition.E_Invoice;
            return View("~/Views/InvoiceAudit/QueryIndex.cshtml");
        }

        public ActionResult AllowanceIndex(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            viewModel.QueryForm = "~/Views/InvoiceAudit/Module/AllowanceQuery.cshtml";
            viewModel.DocType = Naming.DocumentTypeDefinition.E_Allowance;
            return View("~/Views/InvoiceAudit/QueryIndex.cshtml");
        }

        public ActionResult InquireInvoice(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            var profile = HttpContext.GetUser();

            bool effective = false;
            IQueryable<InvoiceItem> items = models.FilterInvoiceByRole(profile, DataSource.Items)
                    .InquireInvoice(viewModel, models, ref effective);

            if (viewModel.PageIndex.HasValue)
            {
                viewModel.PageIndex--;
                return View("~/Views/InvoiceAudit/Module/InvoiceTable.cshtml", items);
            }
            else
            {
                viewModel.ResultView = "~/Views/InvoiceAudit/Module/InvoiceTable.cshtml";
                return View("~/Views/InvoiceAudit/Module/InvoiceQueryResult.cshtml", items);
            }

        }

        public ActionResult InquireAllowance(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            var profile = HttpContext.GetUser();

            bool effective = false;
            IQueryable<InvoiceAllowance> items = models.FilterAllowanceByRole(profile, models.GetTable<InvoiceAllowance>())
                    .InquireAllowance(viewModel, models, ref effective);

            if (viewModel.PageIndex.HasValue)
            {
                viewModel.PageIndex--;
                return View("~/Views/InvoiceAudit/Module/AllowanceTable.cshtml", items);
            }
            else
            {
                viewModel.ResultView = "~/Views/InvoiceAudit/Module/AllowanceTable.cshtml";
                return View("~/Views/InvoiceAudit/Module/AllowanceQueryResult.cshtml", items);
            }

        }

    }
}