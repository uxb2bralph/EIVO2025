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
using WebHome.Helper;
using WebHome.Models;
using WebHome.Models.ViewModel;
using ModelCore.Models.ViewModel;
using WebHome.Properties;
using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Schema.TurnKey.E0402;

using CommonLib.Utility;
using Microsoft.AspNetCore.Authorization;

namespace WebHome.Controllers.TrackCodeNo
{
    [Authorize]
    public class TrackCodeNoAssignmentController : SampleController<InvoiceItem>
    {
        public TrackCodeNoAssignmentController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // GET: TrackCodeNoAssignment
        public ActionResult Index()
        {
            return View();
        }
    }
}