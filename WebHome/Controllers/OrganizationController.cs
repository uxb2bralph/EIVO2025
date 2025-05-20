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

using CommonLib.Utility;
using ModelCore.Helper;
using ModelCore.Helper;
using Microsoft.AspNetCore.Authorization;
using CommonLib.Core.Utility;

namespace WebHome.Controllers
{
    [Authorize]
    public class OrganizationController : SampleController<InvoiceItem>
    {
        public OrganizationController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // GET: Organization
        public ActionResult UpdateLogo(int? id)
        {
            var item = models.GetTable<Organization>().Where(c => c.CompanyID == id).FirstOrDefault();
            if (item == null)
            {
                ViewBag.Message = "營業人資料錯誤!!";
                return View("~/Views/Shared/AlertMessage.cshtml");
            }
            if (Request.Form.Files.Count == 0)
            {
                ViewBag.Message = "請設定公司LOGO標幟!!";
                return View("~/Views/Shared/AlertMessage.cshtml");
            }

            String path = Startup.MapPath("~/LOGO");
            path.CheckStoredPath();

            var file = Request.Form.Files[0];
            String fileName = item.ReceiptNo + "_" + Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            String storePath = Path.Combine(path, fileName);
            file.SaveAs(storePath);

            item.LogoURL = "LOGO/" + fileName;
            models.SubmitChanges();

            return View("~/Views/Organization/LogoUpdated.ascx", item);

        }

        public ActionResult EditItem(OrganizationViewModel viewModel)
        {
            Organization item = null;
            if (viewModel.KeyID != null)
            {
                viewModel.CompanyID = viewModel.DecryptKeyValue();
            }
            item = models.GetTable<Organization>().Where(u => u.CompanyID == viewModel.CompanyID).FirstOrDefault();

            viewModel.ApplyFromModel(item);

            ViewBag.ViewModel = viewModel;
            return View("~/Views/Organization/Module/EditItem.cshtml", item);
        }

        public ActionResult CommitItem(OrganizationViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            Organization item = viewModel.CommitOrganizationViewModel(models, ModelState);

            if (item == null)
            {
                ViewBag.ModelState = ModelState;
                return View("~/Views/Shared/ReportInputError.cshtml");
            }

            return Json(new { result = true });
        }

        public ActionResult CommitBusiness(OrganizationViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            Organization item = viewModel.CommitOrganizationViewModelForBusiness(models, ModelState);

            if (item == null)
            {
                ViewBag.ModelState = ModelState;
                return View("~/Views/Shared/ReportInputError.cshtml");
            }

            return Json(new { result = true });
        }


        public ActionResult ApplyIssuerAgent(OrganizationViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            if (viewModel.KeyID != null)
            {
                viewModel.CompanyID = viewModel.DecryptKeyValue();
            }
            Organization item = null;
            item = models.GetTable<Organization>().Where(u => u.CompanyID == viewModel.CompanyID).FirstOrDefault();

            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "開立人資料錯誤!!");
            }

            return View("~/Views/Organization/Module/ApplyIssuerAgent.cshtml", item);
        }

        public ActionResult ApplyMaster(OrganizationViewModel viewModel)
        {
            var result = ApplyIssuerAgent(viewModel);
            Organization? item = (result as ViewResult)?.Model as Organization;
            if (item != null)
            {
                ((ViewResult)result).ViewName = "~/Views/Organization/Module/ApplyMaster.cshtml";
            }
            return result;
        }


        public ActionResult ApplyBillingPlan(OrganizationViewModel viewModel)
        {
            ViewResult result = (ViewResult)ApplyIssuerAgent(viewModel);
            Organization item = result.Model as Organization;
            if (item == null)
            {
                return result;
            }

            return View("~/Views/Organization/Module/ApplyBillingPlan.cshtml", item);

        }


        bool CheckAgentCycle(int issuerID, int? agentID,out InvoiceIssuerAgent? cycleAgent)
        {
            cycleAgent = null;
            var agentItems = models!.GetTable<InvoiceIssuerAgent>()
                    .Where(a => a.IssuerID == agentID)
                    .ToList();

            foreach (var agent in agentItems)
            {
                if(agent.AgentID == agent.IssuerID)
                {
                    continue;
                }

                if (agent.AgentID == issuerID)
                {
                    cycleAgent = agent;
                    return true;
                }

                if (CheckAgentCycle(issuerID, agent.AgentID, out cycleAgent))
                {
                    return true;
                }
            }

            return false;
        }
        public ActionResult CommitIssuerAgent(OrganizationViewModel viewModel, int[]? agentID)
        {
            ViewResult result = (ViewResult)ApplyIssuerAgent(viewModel);
            Organization? item = result.Model as Organization;

            if (item == null)
            {
                return result;
            }

            CommitIssuerAgent(item, agentID);

            if (!ModelState.IsValid)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: ModelState.ErrorMessage());
            }

            return Json(new { result = true });
        }

        private int CommitIssuerAgent(Organization item, int[]? agentID, bool masterBranch = false)
        {
            int result = 0;
            if (agentID != null && agentID.Length > 0)
            {
                foreach (var id in agentID)
                {
                    InvoiceIssuerAgent cycleAgent = null;
                    if (CheckAgentCycle(item.CompanyID, id, out cycleAgent))
                    {
                        ModelState.AddModelError("Message", $"發生循環經銷({cycleAgent.InvoiceIssuer.ReceiptNo}, {cycleAgent.InvoiceIssuer.CompanyName})!!");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                //models!.ExecuteCommand("delete InvoiceIssuerAgent where IssuerID = {0}", item.CompanyID);
                //if (agentID != null && agentID.Length > 0)
                //{
                //    foreach (var id in agentID)
                //    {
                //        models.ExecuteCommand("insert InvoiceIssuerAgent (AgentID,IssuerID) values ({0},{1})", id, item.CompanyID);
                //    }
                //}
                foreach (var id in agentID!)
                {
                    result += models!.ExecuteCommand(@"INSERT INTO InvoiceIssuerAgent
                             (AgentID, IssuerID)
                        SELECT {0}, {1}
                        WHERE (NOT EXISTS
                                 (SELECT NULL FROM InvoiceIssuerAgent
                        WHERE (AgentID = {0}) AND (IssuerID = {1})))", item.CompanyID, id);

                    if (masterBranch)
                    {
                        models.ExecuteCommand(@"Update InvoiceIssuerAgent set RelationType = {2}
                            WHERE AgentID = {0} AND IssuerID = {1}", item.CompanyID, id, (int)InvoiceIssuerAgent.RelationTypeEnum.MasterBranch);
                    }
                }
            }

            return result;
        }

        public ActionResult CommitMaster(OrganizationViewModel viewModel, int[]? masterID)
        {
            ViewResult result = (ViewResult)ApplyIssuerAgent(viewModel);
            Organization item = result.Model as Organization;

            if (item == null)
            {
                return result;
            }

            if (masterID != null && masterID.Length > 0)
            {
                foreach (var id in masterID)
                {
                    if (!models.GetTable<MasterOrganization>().Any(m => m.MasterID == id))
                    {
                        return View("~/Views/Shared/AlertMessage.cshtml", model: $"指派非主機構!!");
                    }
                }
            }

            models.ExecuteCommand("DELETE FROM center.MasterBranches WHERE (BranchID = {0})", item.CompanyID);
            if (masterID != null && masterID.Length > 0)
            {
                foreach (var id in masterID)
                {
                    models.ExecuteCommand(@"INSERT INTO center.MasterBranches (MasterID, BranchID)
                            VALUES ({0},{1})", id, item.CompanyID);
                }
            }

            return Json(new { result = true });
        }

        public ActionResult CommitBillingPlan(OrganizationViewModel viewModel)
        {
            ViewResult result = (ViewResult)ApplyIssuerAgent(viewModel);
            Organization item = result.Model as Organization;

            if (item == null)
            {
                return result;
            }

            for (int i = 0; i < viewModel.GradeCount?.Length && i < viewModel.BasicFee?.Length; i++)
            {
                if (viewModel.GradeCount[i] > 0)
                {
                    if (!(viewModel.BasicFee[i] > 0))
                    {
                        ModelState.AddModelError($"BasicFee,{i}", "請輸入金額!!");
                    }
                }
                else
                {
                    if (viewModel.BasicFee[i] > 0)
                    {
                        ModelState.AddModelError($"GradeCount,{i}", "請輸入開立張數!!");
                    }
                }
            }

            for (int i = 0; i < viewModel.UpperBound?.Length && i < viewModel.UnitFee?.Length; i++)
            {
                if (viewModel.UpperBound[i] > 0)
                {
                    if (!(viewModel.UnitFee[i] > 0))
                    {
                        ModelState.AddModelError($"UnitFee,{i}", "請輸入金額!!");
                    }
                }
                else
                {
                    if (viewModel.UnitFee[i] > 0)
                    {
                        ModelState.AddModelError($"UpperBound,{i}", "請輸入開立張數!!");
                    }
                }
            }

            for (int i = 0; i < viewModel.Fee?.Length; i++)
            {
                if (viewModel.Fee[i] < 0)
                {
                    ModelState.AddModelError($"Fee,{i}", "請輸入金額!!");
                }
            }

            if (!(viewModel.CalcType?.Length > 0 && viewModel.CalcType[0] == Naming.DocumentTypeDefinition.E_Invoice))
            {
                ModelState.AddModelError($"CalcType,0", "請勾選計算發票開立張數!!");
            }

            if(!ModelState.IsValid)
            {
                return View("~/Views/Shared/ReportInputError.cshtml");
            }

            models.ExecuteCommand("DELETE FROM billing.BillingGrade WHERE (CompanyID = {0})", item.CompanyID);
            for (int i = 0; i < viewModel.GradeCount?.Length && i < viewModel.BasicFee?.Length; i++)
            {
                if (viewModel.GradeCount[i]>0 && viewModel.BasicFee[i]>0)
                {
                    models.ExecuteCommand(@"INSERT INTO billing.BillingGrade
                            (CompanyID, GradeCount, BasicFee) values ({0},{1},{2})",
                        item.CompanyID, viewModel.GradeCount[i], viewModel.BasicFee[i]);
                }
            }

            models.ExecuteCommand("DELETE FROM billing.BillingIncrement WHERE (CompanyID = {0})", item.CompanyID);
            for (int i = 0; i < viewModel.UpperBound?.Length && i < viewModel.UnitFee?.Length; i++)
            {
                if (viewModel.UpperBound[i] > 0 && viewModel.UnitFee[i] > 0)
                {
                    models.ExecuteCommand(@"INSERT INTO billing.BillingIncrement
                            (CompanyID, UpperBound, UnitFee) values ({0},{1},{2})",
                        item.CompanyID, viewModel.UpperBound[i], viewModel.UnitFee[i]);
                }
            }

            models.ExecuteCommand("DELETE FROM billing.BillingCalculation WHERE (CompanyID = {0})", item.CompanyID);
            for (int i = 0; i < viewModel.CalcType?.Length; i++)
            {
                if (viewModel.CalcType[i].HasValue)
                {
                    models.ExecuteCommand(@"INSERT INTO billing.BillingCalculation
                            (CompanyID, TypeID) values ({0},{1})",
                        item.CompanyID, (int)viewModel.CalcType[i]);
                }
            }

            models.ExecuteCommand("DELETE FROM billing.ExtraBillingItem WHERE (CompanyID = {0})", item.CompanyID);
            for (int i = 0; i < viewModel.Fee?.Length; i++)
            {
                if (viewModel.Fee[i] > 0)
                {
                    models.GetTable<ExtraBillingItem>()
                        .InsertOnSubmit(new ExtraBillingItem 
                        {
                            CompanyID = item.CompanyID,
                            ItemName = viewModel.ItemName[i],
                            Fee = viewModel.Fee[i].Value,
                            BillingDate = viewModel.BillingDate[i],
                            BillingType = (int?)viewModel.BillingType[i],
                        });
                    models.SubmitChanges();
                    //models.ExecuteCommand(@"INSERT INTO billing.ExtraBillingItem
                    //        (CompanyID, ItemName, Fee, BillingDate, BillingType) 
                    //        values ({0},{1},{2},{3},{4})",
                    //    item.CompanyID, viewModel.ItemName[i], viewModel.Fee[i], 
                    //    viewModel.BillingDate[i], (int?)viewModel.BillingType[i]);
                }
            }

            if (viewModel.BillingCycle.HasValue)
            {
                if (item.BillingExtension == null)
                {
                    item.BillingExtension = new BillingExtension { };
                }
                item.BillingExtension.BillingCycleInMonth = viewModel.BillingCycle.Value;
                models.SubmitChanges();
            }
            else
            {
                models.ExecuteCommand(@"DELETE FROM billing.BillingExtension
                        WHERE   (CompanyID = {0})", item.CompanyID);
            }

            return Json(new { result = true });
        }

        public ActionResult CloneBillingPlan(OrganizationViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            Organization item = null;
            item = models.GetTable<Organization>().Where(u => u.CompanyID == viewModel.SellerID).FirstOrDefault();

            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "複製來源營業人資料錯誤!!");
            }

            if (viewModel.ChkItem == null || viewModel.ChkItem.Length == 0)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "請勾選複製目標營業人!!");
            }

            foreach (var id in viewModel.ChkItem)
            {
                models.ExecuteCommand("DELETE FROM billing.BillingGrade WHERE (CompanyID = {0})", id);
                models.ExecuteCommand(@"INSERT INTO billing.BillingGrade
                       (CompanyID, GradeCount, BasicFee)
                        SELECT  {0}, GradeCount, BasicFee
                        FROM     billing.BillingGrade
                        WHERE   (CompanyID = {1})", id, item.CompanyID);
                models.ExecuteCommand("DELETE FROM billing.BillingIncrement WHERE (CompanyID = {0})", id);
                models.ExecuteCommand(@"INSERT INTO billing.BillingIncrement
                       (CompanyID, UpperBound, UnitFee)
                        SELECT  {0}, UpperBound, UnitFee
                        FROM     billing.BillingIncrement AS BillingIncrement_1
                        WHERE   (CompanyID = {1})", id, item.CompanyID);
                models.ExecuteCommand("DELETE FROM billing.BillingCalculation WHERE (CompanyID = {0})", id);
                models.ExecuteCommand(@"INSERT INTO billing.BillingCalculation
                       (CompanyID, TypeID)
                        SELECT  {0}, TypeID
                        FROM     billing.BillingCalculation
                        WHERE   (CompanyID = {1})", id, item.CompanyID);
                models.ExecuteCommand("DELETE FROM billing.ExtraBillingItem WHERE (CompanyID = {0})", id);
                models.ExecuteCommand(@"INSERT INTO billing.ExtraBillingItem
                       (CompanyID, ItemName, Fee, BillingDate, BillingType)
                        SELECT  {0}, ItemName, Fee, BillingDate, BillingType
                        FROM     billing.ExtraBillingItem
                        WHERE   (CompanyID = {1})", id, item.CompanyID);
            }

            return Json(new { result = true });
        }

        public ActionResult ApplyHeadquarter(OrganizationViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            Organization? item = null;
            item = models!.GetTable<Organization>().Where(u => u.CompanyID == viewModel.SellerID).FirstOrDefault();

            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "總機構營業人資料錯誤!!");
            }

            if (viewModel.ChkItem == null || viewModel.ChkItem.Length == 0)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "請勾選分支機構營業人!!");
            }

            int result = CommitIssuerAgent(item, viewModel.ChkItem, true);

            return Json(new { result = true, message = result });
        }

        public ActionResult RevokeHeadquarter(OrganizationViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            Organization item = null;
            item = models.GetTable<Organization>().Where(u => u.CompanyID == viewModel.SellerID).FirstOrDefault();

            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "總機構營業人資料錯誤!!");
            }


            int result = models.ExecuteCommand(@"DELETE InvoiceIssuerAgent WHERE AgentID = {0}", item.CompanyID);

            return Json(new { result = true, message = result });
        }

        public ActionResult ProcessHeadquarter(OrganizationViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            Organization item = null;
            item = models.GetTable<Organization>().Where(u => u.CompanyID == viewModel.SellerID).FirstOrDefault();

            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "總機構營業人資料錯誤!!");
            }

            return View("~/Views/Organization/Module/ProcessHeadquarter.cshtml", item);

        }

        public ActionResult GatewaySettings(OrganizationViewModel viewModel)
        {
            ViewResult result = (ViewResult)ApplyIssuerAgent(viewModel);
            Organization item = result.Model as Organization;
            if (item == null)
                return result;
            
            return View("~/Views/Organization/Module/GatewaySettings.cshtml", item);
        }

        public ActionResult CommitDefaultProcessType(OrganizationViewModel viewModel)
        {
            ViewResult result = (ViewResult)ApplyIssuerAgent(viewModel);
            Organization item = result.Model as Organization;
            if (item == null)
                return result;

            item.OrganizationStatus.InvoiceClientDefaultProcessType = (int?)viewModel.DefaultProcessType;
            models.SubmitChanges();

            return Json(new { result = true });
        }

        public ActionResult CommitInvoiceNoSafetyStock(OrganizationViewModel viewModel)
        {
            ViewResult result = (ViewResult)ApplyIssuerAgent(viewModel);
            Organization item = result.Model as Organization;
            if (item == null)
                return result;

            item.OrganizationExtension.InvoiceNoSafetyStock = (int?)viewModel.InvoiceNoSafetyStock;
            models.SubmitChanges();

            return Json(new { result = true });
        }

        public ActionResult CustomSettings(CustomSmtpHost viewModel)
        {
            Organization item = null;
            if (viewModel.KeyID != null)
            {
                viewModel.CompanyID = viewModel.DecryptKeyValue();
            }
            item = models.GetTable<Organization>().Where(u => u.CompanyID == viewModel.CompanyID).FirstOrDefault();

            ViewBag.ViewModel = viewModel;
            return View("~/Views/Organization/Module/CustomSettings.cshtml", item);
        }

        public ActionResult CommitSmtpSettings(CustomSmtpHost viewModel)
        {
            ViewBag.ViewModel = viewModel;
            CustomSmtpHost item = viewModel.CommitCustomSmtpHost(models, ModelState);

            if (item == null)
            {
                ViewBag.ModelState = ModelState;
                return View("~/Views/Shared/ReportInputError.cshtml");
            }

            return Json(new { result = true });
        }

        public ActionResult DisableSmtpSettings(CustomSmtpHost viewModel)
        {
            ViewBag.ViewModel = viewModel;

            if (viewModel.KeyID != null)
            {
                viewModel.CompanyID = viewModel.DecryptKeyValue();
            }

            var recordCount = models.ExecuteCommand(@"UPDATE CustomSmtpHost
                                SET        Status = {0}
                                WHERE   (CompanyID = {1})",
                                (int)CustomSmtpHost.StatusType.Disabled,
                                viewModel.CompanyID);

            return Json(new { result = true, recordCount });
        }

        public ActionResult LoadSmtpSettings(CustomSmtpHost viewModel)
        {
            ViewBag.ViewModel = viewModel;
            var item = viewModel.LoadCustomSmtpHostFor(models);

            if (item == null)
            {
                return View("~/Views/Shared/AlertMessageDialog.cshtml", "營業人資料錯誤!!");
            }

            return View("~/Views/Organization/Module/CustomSmtp.cshtml", item);
        }

        //protected override void HandleUnknownAction(string actionName)
        //{
        //    if (!String.IsNullOrEmpty(actionName))
        //    {
        //        this.View(actionName).ExecuteResult(this.ControllerContext);
        //    }
        //}

    }
}