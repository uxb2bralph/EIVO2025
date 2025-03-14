using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;

using WebHome.Helper;
using ModelCore.DataEntity;
using ModelCore.Locale;
using Newtonsoft.Json;
using CommonLib.Utility;

namespace WebHome.Controllers
{
    public class DataEntityController : SampleController<InvoiceItem>
    {
        public DataEntityController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // GET: DataFlow
        public ActionResult Organization(int? id, int? masterID)
        {
            BusinessRelationship relation = null;
            if (masterID.HasValue)
            {
                relation = models.GetTable<BusinessRelationship>()
                    .Where(r => r.MasterID == masterID)
                    .Where(r => r.RelativeID == id).FirstOrDefault();
            }

            if (relation != null)
            {
                var orgItem = relation.Counterpart;
                return Json(new
                {
                    orgItem.ReceiptNo,
                    relation.CompanyName,
                    relation.ContactEmail,
                    relation.Addr,
                    relation.Phone,
                });
            }

            var item = models.GetTable<Organization>().Where(o => o.CompanyID == id).FirstOrDefault();
            return Content(item.JsonStringify(), "application/json");
        }

        public ActionResult OrganizationExtension(int id)
        {
            var item = models.GetTable<OrganizationExtension>().Where(o => o.CompanyID == id).FirstOrDefault();
            return Content(item.JsonStringify(), "application/json");
        }

    }
}