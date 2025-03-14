using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using System.Xml;

using WebHome.Helper;
using WebHome.Models.ViewModel;
using ModelCore.Models.ViewModel;

using WebHome.Properties;
using ModelCore.DataEntity;
using ModelCore.DocumentManagement;
using ModelCore.Helper;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;
using ModelCore.Schema.EIVO.B2B;
using ModelCore.Schema.TurnKey;
using ModelCore.Schema.TXN;

using CommonLib.Utility;
using CommonLib.Security.UseCrypto;

namespace WebHome.Controllers
{
    public class IssuerController : SampleController<InvoiceItem>
    {
        public IssuerController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // GET: Issuer
        public ActionResult Index()
        {
            return View();
        }
    }
}