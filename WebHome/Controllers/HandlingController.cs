using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;


using WebHome.Helper;
using WebHome.Models.ViewModel;
using ModelCore.Models.ViewModel;
using ModelCore.DataEntity;
using ModelCore.Locale;

using ModelCore.Helper;
using Newtonsoft.Json;
using CommonLib.Utility;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;

namespace WebHome.Controllers
{
    [Authorize]
    public class HandlingController : SampleController<InvoiceItem>
    {
        public HandlingController(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        public ActionResult Index()
        {
            ViewBag.Message = "Hello,World！";
            return View();
        }

        // GET: Handling
        public ActionResult DisableCompany(int companyID)
        {
            updateCompanyStatus(companyID, Naming.MemberStatusDefinition.Mark_To_Delete);
            return View("Index");
        }

        private void updateCompanyStatus(int companyID, Naming.MemberStatusDefinition status)
        {
            var profile = HttpContext.GetUser();
            if (profile.CheckSystemCompany() != true)
            {
                ViewBag.Message = "使用者非系統管理公司！";
                return;
            }

            ViewBag.Message = "資料不存在！";

            using (ModelSource<Organization> models = new ModelSource<Organization>())
            {
                OrganizationStatus item = models.GetTable<Organization>().Where(o => o.CompanyID == companyID)
                    .Select(o => o.OrganizationStatus).FirstOrDefault();
                if (item != null)
                {
                    item.CurrentLevel = (int)status;
                    models.SubmitChanges();
                    ViewBag.Message = "資料已更新！";
                }
            }
        }

        public ActionResult EnableCompany(int companyID)
        {
            updateCompanyStatus(companyID, Naming.MemberStatusDefinition.Checked);
            return View("Index");
        }

        public ActionResult ApplyRelationship(int companyID)
        {
            var item = models.GetTable<Organization>().Where(o => o.CompanyID == companyID).FirstOrDefault();
            if (item == null)
            {
                ViewBag.Message = "資料不存在！";
            }
            else
            {
                if (!item.IsEnterpriseGroupMember())
                {
                    models.GetTable<EnterpriseGroupMember>().InsertOnSubmit(
                        new EnterpriseGroupMember
                        {
                            EnterpriseID = (int)Naming.EnterpriseGroup.網際優勢股份有限公司,
                            CompanyID = item.CompanyID
                        });
                    models.SubmitChanges();
                    ViewBag.Message = "設定完成！";
                }
                else
                {
                    ViewBag.Message = "該開立人已是B2B營業人！";
                }
            }

            return View("Index");
        }

        public ActionResult CommitMasterOrganization(QueryViewModel viewModel)
        {
            int? companyID = null;
            if (viewModel.KeyID != null)
            {
                companyID = viewModel.DecryptKeyValue();
            }

            var item = models.GetTable<Organization>().Where(o => o.CompanyID == companyID).FirstOrDefault();
            if (item == null)
            {
                return Json(new { result = false, message = "資料不存在！" });
            }

            if (item.MasterOrganization == null)
            {
                item.MasterOrganization = new MasterOrganization
                {
                    EnterpriseName = item.CompanyName,
                };

                models.SubmitChanges();
            }
            else
            {
                models.ExecuteCommand(@"DELETE FROM center.MasterOrganization
                    WHERE   (MasterID = {0})", item.CompanyID);
            }

            return Json(new { result = true });
        }


        public ActionResult MailTracking(MailTrackingViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View("~/Views/Handling/InvoiceMailTracking.cshtml");
        }

        public ActionResult InquireToTrackMail(MailTrackingViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            viewModel.StartNo = viewModel.StartNo.GetEfficientString();
            viewModel.EndNo = viewModel.EndNo.GetEfficientString();
            viewModel.UserType = viewModel.UserType;

            IQueryable<InvoiceItem> items =
                models.GetTable<InvoiceItem>()
                    .Where(i => !models.GetTable<InvoiceCancellation>().Any(c => c.InvoiceID == i.InvoiceID));

            bool hasQuery = false;
            if (viewModel.DateFilter == FilterType.Include
                && viewModel.DateFrom.HasValue
                && viewModel.DateTo.HasValue)
            {
                hasQuery = true;
                items = items
                    .Where(i => i.InvoiceDate >= viewModel.DateFrom.Value)
                    .Where(i => i.InvoiceDate < viewModel.DateTo.Value.AddDays(1));
            }

            String trackCode = null;
            String endTrackCode = null;
            if (viewModel.StartNo != null)
            {
                if (viewModel.StartNo.Length != 10)
                {
                    ModelState.AddModelError("StartNo", "發票起號由2碼英文+8碼數字組成，共10碼。");
                }
                else
                {
                    hasQuery = true;
                    trackCode = viewModel.StartNo.Substring(0, 2);
                    String startNo = viewModel.StartNo.Substring(2, 8);

                    items = items.Where(i => i.TrackCode == trackCode)
                                .Where(i => String.Compare(i.No, startNo) >= 0);
                }
            }

            if (viewModel.EndNo != null)
            {
                if (viewModel.EndNo.Length != 10)
                {
                    ModelState.AddModelError("EndNo", "發票迄號由2碼英文+8碼數字組成，共10碼。");
                }
                else if (trackCode != null && (endTrackCode = viewModel.EndNo.Substring(0, 2)) != trackCode)
                {
                    ModelState.AddModelError("StartNo", "發票起、迄號字軌不相同！");
                    ModelState.AddModelError("EndNo", "發票起、迄號字軌不相同！");
                }
                else
                {
                    hasQuery = true;
                    String endNo = viewModel.EndNo.Substring(2, 8);

                    items = items.Where(i => i.TrackCode == endTrackCode)
                        .Where(i => String.Compare(i.No, endNo) <= 0);
                }
            }

            if (!hasQuery)
            {
                ModelState.AddModelError("Message", "請指定發票號碼或日期查詢條件！");
            }

            if (viewModel.UserType == null) //Amy
            {
                ModelState.AddModelError("EivoType", "請選擇發票種類！");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ModelState = ModelState;
                return View("~/Views/Shared/ReportInputError.cshtml");
            }

            if (viewModel.DateFilter == FilterType.Exclude)
            {
                IQueryable<InvoiceItem> exclusive = models.GetTable<InvoiceItem>();
                bool hasExclusive = false;
                if (viewModel.DateFrom.HasValue)
                {
                    hasExclusive = true;
                    exclusive = exclusive.Where(i => i.InvoiceDate >= viewModel.DateFrom);
                }

                if (viewModel.DateTo.HasValue)
                {
                    hasExclusive = true;
                    exclusive = exclusive.Where(i => i.InvoiceDate < viewModel.DateTo.Value.AddDays(1));
                }

                if (hasExclusive)
                {
                    items = items.Where(i => !exclusive.Any(x => x.InvoiceID == i.InvoiceID));
                }
            }

            if (viewModel.ChannelID.HasValue)
            {
                items = items.Join(models.GetTable<CDS_Document>().Where(d => d.ChannelID == (int?)viewModel.ChannelID),
                    i => i.InvoiceID, d => d.DocID, (i, d) => i);
            }

            if (viewModel.Attachment == 1 || viewModel.UserType == 1) //Amy-1121115-UserType
            {
                items = items.Join(models.GetTable<CDS_Document>().Where(d => d.Attachment.Any()),
                    i => i.InvoiceID, d => d.DocID, (i, d) => i);
            }
            else if (viewModel.Attachment == 0 || viewModel.UserType == 0)//Amy-1121115-UserType
            {
                items = items.Join(models.GetTable<CDS_Document>().Where(d => !d.Attachment.Any()),
                    i => i.InvoiceID, d => d.DocID, (i, d) => i);
            }

            //var resultItems = items
            //    .Where(i => i.InvoiceBuyer.Address != null && i.InvoiceBuyer.ReceiptNo == "0000000000")
            //    .OrderBy(i => i.TrackCode).ThenBy(i => i.No)
            //    .Union(items
            //        .Where(i => i.InvoiceBuyer.Address != null && i.InvoiceBuyer.ReceiptNo != "0000000000")
            //        .OrderBy(i => i.TrackCode).ThenBy(i => i.No));

            var recipients = models.GetTable<InvoiceBuyer>().Where(i => i.Address != null);

            var profile = HttpContext.GetUser();
            if (profile != null)
            {
                //models.DeleteAll<UserProfileProperty>(p => p.UID == profile.UID && p.PropertyID == (int)UserProfileProperty.PropertyType.ExclusiveBuyerMail);

                var exclusiveNo = viewModel.ExclusiveReceiptNo?.Split(',', ';', '\n', '、')
                                    .Select(s => s.GetEfficientString())
                                    .Where(s => s != null)
                                    .ToArray();

                if (exclusiveNo?.Length > 0)
                {
                    //models.GetTable<UserProfileProperty>()
                    //    .InsertOnSubmit(new UserProfileProperty 
                    //    {
                    //        UID = profile.UID,
                    //        PropertyID = (int)UserProfileProperty.PropertyType.ExclusiveBuyerMail,
                    //        Property = exclusiveNo.JsonStringify(),
                    //    });
                    //models.SubmitChanges();
                    recipients = recipients.Where(b => !exclusiveNo.Contains(b.ReceiptNo));
                }
            }

            var resultItems = items.Join(recipients,
                    i => i.InvoiceID, b => b.InvoiceID, (i, b) => i);
            //.OrderBy(i => i.InvoiceBuyer.ReceiptNo)
            //.ThenBy(i => i.InvoiceBuyer.Address)
            //.ThenBy(i => i.TrackCode)
            //.ThenBy(i => i.No);

            if (viewModel.UserType == 1)//Amy-1121115-UserType
            {
                return View("~/Views/Handling/MailTracking/QueryResult.cshtml", resultItems); //VIP
            }
            else
            {
                return View("~/Views/Handling/MailTracking/QueryResultGeneralUser.cshtml", resultItems); ////Amy:一般用戶
            }
        }

        public ActionResult InvoiceMailItems(bool? showTable, int[] id, int[] packageID)
        {
            List<InvoiceItem> items = new List<InvoiceItem>();
            if (id != null && id.Length > 0)
            {
                for (int idx = 0; idx < id.Length; idx++)
                {
                    var item = models.GetTable<InvoiceItem>().Where(i => i.InvoiceID == id[idx]).FirstOrDefault();
                    if (item != null)
                    {
                        item.PackageID = packageID[idx];
                    }
                    items.Add(item);
                }
            }
            else
            {
                items = models.GetTable<InvoiceItem>().Where(i => false).ToList();
            }

            if (showTable == true)
            {
                return View("~/Views/Handling/MailTracking/TableList.cshtml", items);
            }
            else
            {
                return View("~/Views/Handling/MailTracking/ItemList.cshtml", items);
            }
        }

        public ActionResult PackInvoice(int[] id)
        {
            IQueryable<InvoiceItem> items;
            if (id != null && id.Length > 0)
            {
                items = models.GetTable<InvoiceItem>().Where(i => id.Contains(i.InvoiceID));
            }
            else
            {
                items = models.GetTable<InvoiceItem>().Where(i => false);
            }

            if (items.Count() == 0)
            {
                ViewBag.Message = "發票資料錯誤！";
                return View("~/Views/Shared/AlertMessage.cshtml");
            }
            else
            {
                return View("~/Views/Handling/MailTracking/PackedItemList.cshtml", items);
            }
        }

        public ActionResult CommitMailTracking(String jsonData, int? deliveryStatus)
        {
            var items = JsonConvert.DeserializeObject<MailTrackingCsvViewModel[]>(jsonData);
            if (items == null || items.Length == 0)
            {
                ViewBag.Message = "請選擇郵寄項目！";
                return View("~/Views/Shared/AlertMessage.cshtml");
            }

            if (!deliveryStatus.HasValue)
            {
                ViewBag.Message = "請選擇郵寄過程！";
                return View("~/Views/Shared/AlertMessage.cshtml");
            }

            bool result = true;
            var table = models.GetTable<InvoiceDeliveryTracking>();
            foreach (var m in items)
            {
                foreach (var p in m.InvoiceID)
                {
                    if (m.DeliveryDate.HasValue)
                    {
                        var mailNo = m.MailNo1.GetEfficientString();
                        if (mailNo == null)
                            continue;

                        var item = new InvoiceDeliveryTracking
                        {
                            DeliveryDate = m.DeliveryDate.Value,
                            DeliveryStatus = deliveryStatus.Value,
                            InvoiceID = p.Value,
                            TrackingNo1 = mailNo,
                            TrackingNo2 = m.MailNo2
                        };
                        table.InsertOnSubmit(item);
                    }
                    else
                    {
                        result = false;
                    }
                }
            }

            try
            {
                if (result)
                {
                    models.SubmitChanges();
                    return Json(new { result = true });
                }
                else
                {
                    return Json(new { result = false, message = "資料錯誤！" });
                }
            }
            catch (Exception ex)
            {
                CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                return Json(new { result = false, message = ex.Message });
            }

        }

        public async Task<ActionResult> DownloadXlsxAsync(String jsonData)
        {
            var items = JsonConvert.DeserializeObject<MailTrackingCsvViewModel[]>(jsonData);
            if (items == null || items.Length == 0)
            {
                ViewBag.CloseWindow = true;
                ViewBag.Message = "請選擇郵寄項目！";
                return View("~/Views/Shared/AlertMessage.cshtml");
            }

            using (DataSet ds = new DataSet())
            {
                DataTable table = new DataTable();
                ds.Tables.Add(table);
                table.Columns.Add("掛號號碼1");
                table.Columns.Add("掛號號碼2"); //Amy-1121116
                table.Columns.Add("掛號號碼3"); //Amy-1121116
                table.Columns.Add("掛號號碼4"); //Amy-1130115
                table.Columns.Add("掛號號碼5"); //Amy-1130115
                table.Columns.Add("姓名");
                table.Columns.Add("寄件地名或地址");
                table.Columns.Add("備考");
                table.Columns.Add("附件檔頁數");
                table.Columns.Add("合計");
                //table.Columns.Add("作業方式");
                table.Columns.Add("遞件日期");
                table.Columns.Add("Google_id");
                table.Columns.Add("發票號碼");
                table.Columns.Add("發票日期");

                foreach (var m in items)
                {
                    var item = models.GetTable<InvoiceItem>().Where(i => i.InvoiceID == m.PackageID).FirstOrDefault();
                    var data = models.GetTable<DocumentPostLog>().Where(i => i.InvoiceID == m.PackageID).FirstOrDefault();//Amy-1130115, 因應郵務需求增加ZipCode, 及ChkCode
                    if (item == null)
                        continue;
                    var mailNo = m.MailNo1.GetEfficientString();
                    if (mailNo == null)
                        continue;
                    var mailNO2 = m.MailNo2.GetEfficientString(); //Amy-1121116
                    var mailNO3 = m.MailNo3.GetEfficientString(); //Amy-1121117
                    var row = table.NewRow();
                    table.Rows.Add(row);

                    row["掛號號碼1"] = mailNo;
                    row["掛號號碼2"] = mailNO2;
                    row["掛號號碼3"] = mailNO3;
                    //row["掛號號碼4"] = data.ZipCode; //Amy-1130115
                    row["掛號號碼4"] = (data?.ZipCode?.Length == 6) ? $"{data.ZipCode.Substring(0, 3)}00" : data?.ZipCode;  //Amy-1130119-郵遞區號等於6碼的只取前3碼,補2個零,若小於6碼的,取原資料
                    row["掛號號碼5"] = data?.ChkCode;//Amy-1130115
                    row["遞件日期"] = $"{m.DeliveryDate:yyyy/MM/dd}";
                    row["姓名"] = item.InvoiceBuyer.ContactName;
                    row["寄件地名或地址"] = item.InvoiceBuyer.Address;
                    int p1 = m.InvoiceID != null && m.InvoiceID.Length > 1 ? m.InvoiceID.Length : 0;
                    row["備考"] = $"{p1:###}";
                    int p2 = m.InvoiceID != null && m.InvoiceID.Length > 0 ? m.InvoiceID.Sum(i => i.Value.GetAttachedPdfPageCount(models)) : 0;
                    row["附件檔頁數"] = $"{p2:###}";
                    row["合計"] = $"{p1 + p2:###}";
                    //row["作業方式"] = ((Naming.ChannelIDType?)item.CDS_Document.ChannelID).ToString();
                    row["Google_id"] = item.InvoiceBuyer.CustomerID;
                    row["發票號碼"] = $"{item.TrackCode}{item.No}";
                    row["發票日期"] = $"{item.InvoiceDate:yyyy/MM/dd}";
                }

                await ds.SaveAsExcelAsync(Response, String.Format("attachment;filename={0}", HttpUtility.UrlEncode("掛號郵件號碼明細.xlsx")));
            }

            return new EmptyResult();
        }

        public ActionResult GeneratePostData(String jsonData) //Amy-1121114-郵政資訊
        {
            var items = JsonConvert.DeserializeObject<MailTrackingCsvViewModel[]>(jsonData);
            models.ExecuteCommand("delete DocumentPostLog ");

            foreach (var m in items)
            {
                var item = models.GetTable<InvoiceItem>().Where(i => i.InvoiceID == m.PackageID).FirstOrDefault();
                var MailNo1 = m.MailNo1.GetEfficientString();
                var MailNo2 = m.MailNo2.GetEfficientString();
                var MailNo3 = m.MailNo3.GetEfficientString();
                if (item == null || MailNo1 == null || MailNo2 == null || MailNo3 == null)
                {
                    ViewBag.Message = "產生郵件號碼失敗或不完整,請重新產生！";
                    return View("~/Views/Shared/AlertMessage.cshtml");
                }

                var pattern = @"(?<zipcode>^\d+?)(?<city>\D+[縣市])(?<district>\D+?(市區|鎮區|鎮市|[鄉鎮市區]))(?<others>.+)";
                Match zipMatch = Regex.Match(item.InvoiceBuyer.Address, pattern);
                var zipcode = zipMatch.Groups["zipcode"].ToString().PadRight(5, '0');
                var ChkCode = GetPostChkcode(MailNo1, MailNo2, MailNo3, zipcode);


                models.GetTable<DocumentPostLog>().InsertOnSubmit
                (new DocumentPostLog
                {
                    InvoiceID = item.InvoiceID,
                    PostCode = MailNo1.ToString(),
                    RegisterCode = m.MailNo2.GetEfficientString(),
                    ZipCode = zipcode,
                    MailType = m.MailNo3.ToString(),
                    ChkCode = ChkCode.ToString(),
                    CreateDate = DateTime.Now
                }
                );
                models.SubmitChanges();
            }
            return new EmptyResult();
        }

        private int GetPostChkcode(string MailNo1, string MailNo2, string MailNo3, string zipcode)
        {

            int oddSum = 0;
            int evenSum = 0;
            var sCode = MailNo1.Trim() + MailNo2.Trim() + MailNo3.Trim() + zipcode.Trim();
            // 计算奇数位和偶数位的数字之和
            for (int i = 0; i < sCode.Length; i++)
            {
                int digit = int.Parse(sCode[i].ToString());

                if (i % 2 == 0) // 奇数位
                {
                    oddSum += digit;
                }
                else // 偶数位
                {
                    evenSum += digit;
                }
            }
            var leftchar = (oddSum * 3 + evenSum).ToString().Substring(0, 1); //取第一個位元
            // 计算最终的 ChkCode
            int chkCode = 10 - int.Parse(leftchar);

            return chkCode;

        }


        // GET: Handling/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Handling/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Handling/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Handling/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
