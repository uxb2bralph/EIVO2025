using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Xml;
using ApplicationResource;
using Business.Helper.InvoiceProcessor;
using CommonLib.Utility;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelCore.DataEntity;
using ModelCore.DTOs;
using ModelCore.Helper;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;
using ModelCore.Schema.EIVO;
using ModelCore.Schema.TXN;
using ModelCore.Security;
using Newtonsoft.Json;
using TaskCenter.Core;
using TaskCenter.Core.Attributes;
using TaskCenter.Core.Services;
using TaskCenter.Helper.RequestAction;
using TaskCenter.Properties;

namespace TaskCenter.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    //對外收單 API 須維持 HTTP 200 + Root 的回應格式，故停用自動模型驗證，改由 CheckRequestBody 回報
    [SuppressModelStateInvalidFilter]
    public class InvoiceServiceController : SampleController
    {
        public InvoiceServiceController(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
            DumpRequest = AppSettings.Default.EnableRequestDump;
        }

        [HttpPost("UploadInvoiceAutoTrackNo")]
        [ProducesResponseType(typeof(Root), 200)]
        public ActionResult UploadInvoiceAutoTrackNo([FromBody] InvoiceRequestViewModel? viewModel)
        {
            Root result = createMessageToken();
            ContentResult? invalid = CheckRequestBody(viewModel, result);
            if (invalid != null)
            {
                return invalid;
            }

            InvoiceRoot? invoice = viewModel!.InvoiceRoot;
            if (invoice != null)
            {
                if (viewModel.ProcessType == Naming.InvoiceProcessType.F0401_Json_CBE)
                {
                    using (InvoiceManagerForCBE manager = new InvoiceManagerForCBE { InvoiceClientID = viewModel.ClientID, ProcessType = viewModel.ProcessType })
                    {
                        OrganizationToken? token = viewModel.CheckRequestToken(this);
                        if(token == null)
                        {
                            result.Result.value = 0;
                            result.Result.message = "Token 驗證失敗!!";
                            return Content(result.JsonStringify(), "application/json");
                        }
                        manager.ApplyInvoiceDate = viewModel?.ApplyInvoiceDate;
                        manager.UploadInvoiceAutoTrackNo(invoice, result, token);
                    }
                }
                else
                {
                    using (InvoiceManagerV3 manager = new InvoiceManagerV3 { InvoiceClientID = viewModel.ClientID, ProcessType = viewModel.ProcessType })
                    {
                        OrganizationToken? token = viewModel.CheckRequestToken(this);
                        if(token == null)
                        {
                            result.Result.value = 0;
                            result.Result.message = "Token 驗證失敗!!";
                            return Content(result.JsonStringify(), "application/json");
                        }
                        manager.ApplyInvoiceDate = viewModel?.ApplyInvoiceDate;
                        manager.UploadInvoiceAutoTrackNo(invoice, result, token);
                    }
                }

            }
            return Content(result.JsonStringify(), "application/json");
        }

        [HttpPost("UploadInvoice")]
        [ProducesResponseType(typeof(Root), 200)]
        public ActionResult UploadInvoice([FromBody] InvoiceRequestViewModel? viewModel)
        {
            Root result = createMessageToken();
            ContentResult? invalid = CheckRequestBody(viewModel, result);
            if (invalid != null)
            {
                return invalid;
            }

            InvoiceRoot? invoice = viewModel!.InvoiceRoot;
            if (invoice != null)
            {
                if (viewModel.ProcessType == Naming.InvoiceProcessType.F0401_Json_CBE)
                {
                    using (InvoiceManagerForCBE manager = new InvoiceManagerForCBE { InvoiceClientID = viewModel?.ClientID, ProcessType = viewModel?.ProcessType })
                    {
                        OrganizationToken? token = viewModel?.CheckRequestToken(this);
                        if(token == null)
                        {
                            result.Result.value = 0;
                            result.Result.message = "Token 驗證失敗!!";
                            return Content(result.JsonStringify(), "application/json");
                        }
                        manager.UploadInvoice(invoice, result, token);
                    }
                }
                else
                {
                    using (InvoiceManagerV3 manager = new InvoiceManagerV3 { InvoiceClientID = viewModel?.ClientID, ProcessType = viewModel?.ProcessType })
                    {
                        OrganizationToken? token = viewModel?.CheckRequestToken(this);
                        if(token == null)
                        {
                            result.Result.value = 0;
                            result.Result.message = "Token 驗證失敗!!";
                            return Content(result.JsonStringify(), "application/json");
                        }
                        manager.UploadInvoice(invoice, result, token);
                    }
                }
            }


            return Content(result.JsonStringify(), "application/json");
        }

        [HttpPost("ReviseInvoice")]
        [ProducesResponseType(typeof(Root), 200)]
        public ActionResult ReviseInvoice([FromBody] InvoiceRequestViewModel? viewModel)
        {
            Root result = createMessageToken();
            ContentResult? invalid = CheckRequestBody(viewModel, result);
            if (invalid != null)
            {
                return invalid;
            }

            InvoiceRoot? invoice = viewModel!.InvoiceRoot;
            if (invoice != null)
            {
                using (InvoiceManagerV2 manager = new InvoiceManagerV2 { InvoiceClientID = viewModel?.ClientID, ProcessType = viewModel?.ProcessType })
                {
                    OrganizationToken? token = viewModel?.CheckRequestToken(this);
                    if (token == null)
                    {
                        result.Result.value = 0;
                        result.Result.message = "Token 驗證失敗!!";
                        return Content(result.JsonStringify(), "application/json");
                    }
                    manager.ReviseInvoice(invoice, result, token);
                }
            }


            return Content(result.JsonStringify(), "application/json");
        }


        [HttpPost("UploadInvoiceCancellation")]
        [ProducesResponseType(typeof(Root), 200)]
        public ActionResult UploadInvoiceCancellation([FromBody] InvoiceRequestViewModel? viewModel)
        {
            Root result = createMessageToken();
            ContentResult? invalid = CheckRequestBody(viewModel, result);
            if (invalid != null)
            {
                return invalid;
            }

            CancelInvoiceRoot item = viewModel!.CancelInvoiceRoot!;
            using (InvoiceManagerV3 manager = new InvoiceManagerV3 { })
            {
                OrganizationToken? token = viewModel?.CheckRequestToken(this);
                if(token == null)
                {
                    result.Result.value = 0;
                    result.Result.message = "Token 驗證失敗!!";
                    return Content(result.JsonStringify(), "application/json");
                }
                manager.UploadInvoiceCancellation(result, item, token);
            }
            return Content(result.JsonStringify(), "application/json");
        }

        [HttpPost("UploadAllowance")]
        [ProducesResponseType(typeof(Root), 200)]
        public ActionResult UploadAllowance([FromBody] InvoiceRequestViewModel? viewModel)
        {
            Root result = createMessageToken();
            ContentResult? invalid = CheckRequestBody(viewModel, result);
            if (invalid != null)
            {
                return invalid;
            }

            AllowanceRoot allowance = viewModel!.AllowanceRoot!;
            using (InvoiceManagerV3 manager = new InvoiceManagerV3 { })
            {
                OrganizationToken? token = viewModel.CheckRequestToken(this);
                if(token == null)
                {
                    result.Result.value = 0;
                    result.Result.message = "Token 驗證失敗!!";
                    return Content(result.JsonStringify(), "application/json");
                }
                manager.UploadAllowance(result, allowance, token);
            }
            return Content(result.JsonStringify(), "application/json");
        }

        /// <summary>
        /// 開立折讓單。僅需提供 <c>AllowanceItem[0].OriginalInvoiceNumber</c>、<c>SellerId</c>、
        /// <c>TotalAmount</c>(不含稅折讓金額)、<c>TaxAmount</c>，折讓明細由原發票
        /// <see cref="InvoiceProductItem"/> 依序逐項扣抵產生後，交由 <c>UploadAllowance</c> 存證。
        /// </summary>
        [HttpPost("ApplyAllowance")]
        [ProducesResponseType(typeof(Root), 200)]
        public ActionResult ApplyAllowance([FromBody] InvoiceRequestViewModel? viewModel)
        {
            Root result = createMessageToken();
            ContentResult? invalid = CheckRequestBody(viewModel, result);
            if (invalid != null)
            {
                return invalid;
            }

            AllowanceRoot? allowance = viewModel!.AllowanceRoot;
            using (InvoiceManagerV3 manager = new InvoiceManagerV3 { })
            {
                OrganizationToken? token = viewModel?.CheckRequestToken(this);
                if (token == null)
                {
                    result.Result.value = 0;
                    result.Result.message = "Token 驗證失敗!!";
                    return Content(result.JsonStringify(), "application/json");
                }

                if (allowance?.Allowance == null || allowance.Allowance.Length == 0)
                {
                    result.Result.value = 0;
                    result.Result.message = "折讓資料不存在!!";
                    return Content(result.JsonStringify(), "application/json");
                }

                Dictionary<int, Exception> failure = new Dictionary<int, Exception>();
                for (int idx = 0; idx < allowance.Allowance.Length; idx++)
                {
                    try
                    {
                        Exception? ex = BuildAllowanceDetails(allowance.Allowance[idx], token);
                        if (ex != null)
                        {
                            failure.Add(idx, ex);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "ApplyAllowance");
                        failure.Add(idx, ex);
                    }
                }

                //明細無法產生即不開立，避免同一批次中部分折讓造成金額不符
                if (failure.Count > 0)
                {
                    ReportAllowanceFailure(result, allowance, failure);
                    return Content(result.JsonStringify(), "application/json");
                }

                manager.UploadAllowance(result, allowance, token);
            }
            return Content(result.JsonStringify(), "application/json");
        }

        /// <summary>
        /// 由 <c>OriginalInvoiceNumber</c> 取回原發票，檢查開立人/代理關係後，
        /// 依原發票消費明細逐項扣抵折讓金額，回填 <see cref="AllowanceRootAllowance"/> 的折讓明細與表頭欄位。
        /// </summary>
        /// <returns>檢查不通過時回傳錯誤，通過時回傳 null。</returns>
        private Exception? BuildAllowanceDetails(AllowanceRootAllowance item, OrganizationToken token)
        {
            String? sellerId = item.SellerId.GetEfficientString();
            var seller = sellerId == null
                ? null
                : models!.GetTable<Organization>().Where(o => o.ReceiptNo == sellerId).AsNoTracking().FirstOrDefault();

            if (seller == null)
            {
                return new Exception(String.Format(ModelCore.Resource.MessageResources.AlertInvalidSeller, item.SellerId));
            }

            //開立人本人或其代理人方可開立折讓
            if (seller.CompanyID != token.CompanyID
                && !models!.GetTable<InvoiceIssuerAgent>().Any(a => a.AgentID == token.CompanyID && a.IssuerID == seller.CompanyID))
            {
                return new Exception(String.Format(ModelCore.Resource.MessageResources.AlertSellerSignature, sellerId));
            }

            String? invoiceNo = item.AllowanceItem?.Select(a => a.OriginalInvoiceNumber.GetEfficientString())
                                    .FirstOrDefault(n => n?.Length == 10);
            if (invoiceNo == null)
            {
                return new Exception(String.Format(ModelCore.Resource.MessageResources.InvalidAllowance_NoInvoiceData,
                                        item.AllowanceItem?.Select(a => a.OriginalInvoiceNumber).FirstOrDefault()));
            }

            String trackCode = invoiceNo.Substring(0, 2);
            String no = invoiceNo.Substring(2);

            var invoice = models!.GetTable<InvoiceItem>()
                .Where(v => v.SellerID == seller.CompanyID)
                .Where(v => v.TrackCode == trackCode)
                .Where(v => v.No == no)
                .Include(v => v.InvoiceSeller)
                .Include(v => v.InvoiceBuyer)
                .Include(v => v.InvoiceAmountType)
                .Include(v => v.InvoiceCancellation)
                .Include(v => v.Product)
                    .ThenInclude(p => p.InvoiceProductItem)
                .OrderByDescending(v => v.InvoiceID)
                .AsNoTracking()
                .AsSplitQuery()
                .FirstOrDefault();

            if (invoice == null)
            {
                return new Exception(String.Format(ModelCore.Resource.MessageResources.InvalidAllowance_NoInvoiceData, invoiceNo));
            }

            if (invoice.InvoiceCancellation != null)
            {
                return new Exception(ModelCore.Resource.MessageResources.InvalidAllowance_InvoiceHasBeenCanceled);
            }

            if (invoice.InvoiceSeller?.ReceiptNo != sellerId)
            {
                return new Exception(String.Format(ModelCore.Resource.MessageResources.AlertAllowance_InvoiceSellerIsDifferent, invoiceNo));
            }

            var productItems = invoice.Product
                .SelectMany(p => p.InvoiceProductItem.Select(d => new { Product = p, Detail = d }))
                .OrderBy(p => p.Detail.No)
                .ThenBy(p => p.Detail.ItemID)
                .ToList();

            decimal itemsTotal = productItems.Sum(p => p.Detail.CostAmount ?? 0m);
            if (itemsTotal <= 0)
            {
                return new Exception($"發票 {invoiceNo} 查無可折讓之消費明細!!");
            }

            //B2C 明細金額為含稅價，須換算為未稅消費金額方可與折讓金額比對
            var amountType = invoice.InvoiceAmountType;
            decimal netTotal = itemsTotal;
            if (amountType?.TotalAmount > 0)
            {
                decimal gross = amountType.TotalAmount.Value;
                decimal net = gross - (amountType.TaxAmount ?? 0m);
                if (Math.Abs(itemsTotal - gross) < Math.Abs(itemsTotal - net))
                {
                    netTotal = net;
                }
            }

            decimal allowanceAmount = item.TotalAmount;
            if (allowanceAmount <= 0)
            {
                return new Exception($"折讓金額須大於零，傳送資料：{allowanceAmount}，TAG：< TotalAmount />");
            }

            if (allowanceAmount > netTotal)
            {
                return new Exception($"折讓金額 {allowanceAmount:#,##0.####} 大於發票 {invoiceNo} 消費總額 {netTotal:#,##0.####}!!");
            }

            //依明細順序逐項扣抵折讓金額
            List<AllowanceRootAllowanceAllowanceItem> details = new List<AllowanceRootAllowanceAllowanceItem>();
            List<decimal> deductions = new List<decimal>();
            decimal balance = allowanceAmount;
            short seqNo = 1;

            foreach (var p in productItems)
            {
                if (balance <= 0)
                {
                    break;
                }

                decimal itemAmount = Math.Round((p.Detail.CostAmount ?? 0m) * netTotal / itemsTotal);
                if (itemAmount <= 0)
                {
                    continue;
                }

                decimal deduction = Math.Min(balance, itemAmount);
                balance -= deduction;

                decimal piece = p.Detail.Piece ?? 0m;
                decimal quantity = piece > 0 ? Math.Round(piece * deduction / itemAmount) : 0m;
                if (quantity <= 0)
                {
                    quantity = 1m;
                }

                String description = p.Product.Brief.GetEfficientString() ?? p.Detail.ItemNo.GetEfficientString() ?? invoiceNo;
                String? unit = p.Detail.PieceUnit.GetEfficientString();
                byte taxType = p.Detail.TaxType ?? amountType?.TaxType ?? (byte)Naming.TaxTypeDefinition.應稅;

                details.Add(new AllowanceRootAllowanceAllowanceItem
                {
                    OriginalInvoiceNumber = invoiceNo,
                    OriginalSequenceNumber = p.Detail.No,
                    OriginalSequenceNumberSpecified = p.Detail.No.HasValue,
                    Item = p.Detail.ItemNo,
                    OriginalDescription = description.Length > 256 ? description.Substring(0, 256) : description,
                    Unit = unit?.Length > 6 ? unit.Substring(0, 6) : unit,
                    Quantity = quantity,
                    UnitPrice = p.Detail.UnitCost ?? 0m,
                    Amount = deduction,
                    TaxType = Enum.IsDefined(typeof(Naming.TaxTypeDefinition), (int)taxType)
                                ? taxType
                                : (byte)Naming.TaxTypeDefinition.應稅,
                    AllowanceSequenceNumber = seqNo++,
                    Remark = p.Detail.Remark,
                });
                deductions.Add(deduction);
            }

            if (details.Count == 0)
            {
                return new Exception($"發票 {invoiceNo} 查無可折讓之消費明細!!");
            }

            //捨入誤差歸入最後一筆，確保明細合計等於折讓金額
            if (balance != 0)
            {
                var lastItem = details[details.Count - 1];
                lastItem.Amount += balance;
                // lastItem.UnitPrice = Math.Round(lastItem.Amount / lastItem.Quantity);
                deductions[deductions.Count - 1] += balance;
            }

            //稅額依各項折讓金額比例分攤，餘數歸入最後一筆
            decimal taxBalance = item.TaxAmount;
            for (int i = 0; i < details.Count; i++)
            {
                decimal tax = i == details.Count - 1
                    ? taxBalance
                    : Math.Round(item.TaxAmount * deductions[i] / allowanceAmount, 0, MidpointRounding.AwayFromZero);
                details[i].Tax = tax;
                taxBalance -= tax;
            }

            item.AllowanceItem = details.ToArray();
            item.AllowanceNumber = item.AllowanceNumber.GetEfficientString() ?? invoiceNo;
            item.AllowanceDate = item.AllowanceDate.GetEfficientString() ?? String.Format("{0:yyyy/MM/dd}", DateTime.Today);
            item.SellerId = sellerId;
            item.SellerName = item.SellerName.GetEfficientString() ?? seller.CompanyName;
            item.BuyerId = item.BuyerId.GetEfficientString() ?? invoice.InvoiceBuyer?.ReceiptNo.GetEfficientString() ?? "0000000000";

            String? buyerName = item.BuyerName.GetEfficientString()
                                    ?? (invoice.InvoiceBuyer?.CustomerName ?? invoice.InvoiceBuyer?.Name).GetEfficientString();
            item.BuyerName = buyerName?.Length > 60 ? buyerName.Substring(0, 60) : buyerName;

            if (item.Contact == null && invoice.InvoiceBuyer != null)
            {
                item.Contact = new AllowanceRootAllowanceContact
                {
                    Name = invoice.InvoiceBuyer.CustomerName ?? invoice.InvoiceBuyer.Name,
                    Address = invoice.InvoiceBuyer.Address,
                    TEL = invoice.InvoiceBuyer.Phone,
                    Email = invoice.InvoiceBuyer.EMail,
                };
            }

            return null;
        }

        /// <summary>折讓明細產生失敗時，以與 <c>UploadAllowance</c> 相同的格式回報錯誤。</summary>
        private static void ReportAllowanceFailure(Root result, AllowanceRoot allowance, Dictionary<int, Exception> failure)
        {
            result.Result.value = 0;
            result.Result.message = String.Join("\r\n", failure.Select(d => d.Value.Message));
            result.Response = new RootResponse
            {
                InvoiceNo =
                failure.Select(d => new RootResponseInvoiceNo
                {
                    Value = allowance.Allowance[d.Key].AllowanceNumber,
                    Description = d.Value.Message,
                    ItemIndexSpecified = true,
                    ItemIndex = d.Key
                }).ToArray()
            };

            result.Automation = failure.Select(d => new AutomationItem
            {
                Description = d.Value.Message,
                Status = 0,
                Allowance = new AutomationItemAllowance
                {
                    AllowanceNumber = allowance.Allowance[d.Key].AllowanceNumber,
                    SellerId = allowance.Allowance[d.Key].SellerId,
                },
            }).ToArray();
        }

        [HttpPost("UploadAllowanceCancellation")]
        [ProducesResponseType(typeof(Root), 200)]
        public ActionResult UploadAllowanceCancellation([FromBody] InvoiceRequestViewModel? viewModel)
        {
            Root result = createMessageToken();
            ContentResult? invalid = CheckRequestBody(viewModel, result);
            if (invalid != null)
            {
                return invalid;
            }

            CancelAllowanceRoot? item = viewModel!.CancelAllowanceRoot;
            if (item != null)
            {
                using (InvoiceManagerV3 manager = new InvoiceManagerV3 { })
                {
                    OrganizationToken? token = viewModel.CheckRequestToken(this);
                    if(token == null)
                    {
                        result.Result.value = 0;
                        result.Result.message = "Token 驗證失敗!!";
                        return Content(result.JsonStringify(), "application/json");
                    }
                    manager.UploadAllowanceCancellation(result, item, token);
                }
            }
            return Content(result.JsonStringify(), "application/json");
        }

        [HttpPost("GetStorageToken")]
        public ActionResult GetStorageToken([FromBody] InvoiceRequestViewModel? viewModel)
        {
            String? storageToken = null;
            //自動模型驗證已停用，請求內容解析失敗時 viewModel 為 null
            if (viewModel != null)
            {
                viewModel.StoragePath = viewModel.StoragePath.GetEfficientString();
            }
            if(viewModel?.StoragePath !=null)
            {
                using (InvoiceManagerV3 manager = new InvoiceManagerV3 { InvoiceClientID = viewModel.ClientID, ProcessType = viewModel.ProcessType })
                {
                    OrganizationToken? token = viewModel?.CheckRequestToken(this);
                    if (token != null)
                    {
                        storageToken = Path.Combine($"{token?.CompanyID:00000000}", viewModel!.StoragePath).EncryptData();
                    }
                }
            }

            return Json(new { StorageToken = storageToken });
        }

        [HttpPost("EditInvoice")]
        [ProducesResponseType(typeof(InvoiceRoot), 200)]
        [ProducesResponseType(typeof(Root), 400)]
        public ActionResult<InvoiceRoot> EditInvoice([FromBody] InvoiceViewModel viewModel)
        {
            Root result = createMessageToken();

            if (viewModel == null || String.IsNullOrEmpty(viewModel.KeyID))
            {
                result.Result.value = 0;
                result.Result.message = "營業人資料錯誤!!";
                return BadRequest(result);
            }

            var organization = viewModel.CheckRequest(this);
            if(organization == null)
            {
                result.Result.value = 0;
                result.Result.message = "營業人資料錯誤!!";
                return BadRequest(result);
            }

            var item = models!.GetTable<InvoiceItem>()
                .Where(i => i.TrackCode == viewModel.TrackCode)
                .Where(i => i.No == viewModel.No)
                .Where(i => i.Seller!=null && i.Seller.ReceiptNo == viewModel.SellerReceiptNo)
                .Include(i => i.InvoiceSeller)
                .Include(i => i.InvoiceBuyer)
                .Include(i => i.InvoiceCarrier)
                .Include(i => i.InvoiceDonation)
                .Include(i => i.InvoiceAmountType)
                .Include(i => i.CDS_Document)
                .Include(i => i.Product)
                    .ThenInclude(p => p.InvoiceProductItem)
                .AsNoTracking()
                .AsSplitQuery()
                .FirstOrDefault();

            if (item == null)
            {
                result.Result.value = 0;
                result.Result.message = "查無發票資料!!";
                return BadRequest(result);
            }

            InvoiceRoot editModel = BuildInvoiceViewModel(item);

            return Json(editModel);
        }

        /// <summary>
        /// 檢查請求內容是否成功繫結。本控制器以 <see cref="SuppressModelStateInvalidFilterAttribute"/>
        /// 停用了 [ApiController] 的自動模型驗證（須維持 HTTP 200 + <see cref="Root"/> 的回應格式），
        /// 故於此自行檢查並以中文訊息回報。
        /// </summary>
        /// <returns>繫結成功時回傳 null，失敗時回傳含中文訊息的 Root 回應。</returns>
        private ContentResult? CheckRequestBody(InvoiceRequestViewModel? viewModel, Root result)
        {
            String? message = SuppressModelStateInvalidFilterAttribute.GetModelStateError(HttpContext);
            if (viewModel != null && message == null && ModelState.IsValid)
            {
                return null;
            }

            result.Result.value = 0;
            result.Result.message = message ?? ApiValidationResponse.DescribeMessage(ModelState);
            return Content(result.JsonStringify(), "application/json");
        }

        protected Root createMessageToken()
        {
            Root result = new Root
            {
                UXB2B = "電子發票系統",
                Result = new RootResult
                {
                    timeStamp = DateTime.Now,
                    value = 0
                }
            };
            return result;
        }

        /// <summary>
        /// 將 <see cref="InvoiceItem"/> 實體轉換為對外交換用的 <see cref="InvoiceRoot"/>/<see cref="InvoiceRootInvoice"/>。
        /// 對應欄位與 <c>B2BExtensionMethods.CreateSellerInvoiceRoot</c> 一致（開立人/買方/載具/捐贈/金額/明細）。
        /// </summary>
        private InvoiceRoot BuildInvoiceViewModel(InvoiceItem item)
        {
            var amount = item.InvoiceAmountType;
            var buyer = item.InvoiceBuyer;
            var carrier = item.InvoiceCarrier;
            var donation = item.InvoiceDonation;

            var invoice = new InvoiceRootInvoice
            {
                InvoiceID = item.InvoiceID,
                InvoiceNumber = String.Format("{0}{1}", item.TrackCode, item.No),
                InvoiceDate = String.Format("{0:yyyy/MM/dd}", item.InvoiceDate),
                InvoiceTime = String.Format("{0:HH:mm:ss}", item.InvoiceDate),
                SellerId = item.InvoiceSeller?.ReceiptNo,
                InvoiceType = item.InvoiceType.HasValue ? String.Format("{0:00}", item.InvoiceType.Value) : null,
                CustomsClearanceMark = item.CustomsClearanceMark,
                CustomsClearanceMarkSpecified = item.CustomsClearanceMark.HasValue,
                RandomNumber = item.RandomNo,
                DonateMark = donation == null ? "0" : "1",
                NPOBAN = donation?.AgencyCode,
                Currency = "TWD",
                PrintMark = item.PrintMark ?? "Y",
            };

            // 買方資訊
            if (buyer != null)
            {
                invoice.BuyerId = buyer.ReceiptNo == "0000000000" ? null : buyer.ReceiptNo;
                invoice.BuyerName = buyer.CustomerName ?? buyer.Name;
                invoice.BuyerMark = (byte?)buyer.BuyerMark;
                invoice.CustomerID = buyer.CustomerID;
                invoice.ContactName = buyer.CustomerName ?? buyer.Name;
                invoice.Phone = buyer.Phone;
                invoice.Address = buyer.Address;
                invoice.EMail = buyer.EMail;
                invoice.Contact = new InvoiceRootInvoiceContact
                {
                    Name = buyer.CustomerName ?? buyer.Name,
                    Address = buyer.Address,
                    TEL = buyer.Phone,
                    Email = buyer.EMail,
                };
            }

            // 載具資訊
            if (carrier != null)
            {
                invoice.CarrierType = carrier.CarrierType;
                invoice.CarrierId1 = carrier.CarrierNo;
                invoice.CarrierId2 = carrier.CarrierNo2;
            }

            // 金額資訊
            if (amount != null)
            {
                invoice.TaxType = amount.TaxType ?? 0;
                invoice.TaxRate = amount.TaxRate ?? 0m;
                invoice.TaxRateSpecified = amount.TaxRate.HasValue;
                invoice.TaxAmount = amount.TaxAmount ?? 0m;
                invoice.TotalAmount = amount.TotalAmount ?? 0m;
                invoice.SalesAmount = amount.SalesAmount ?? 0m;
                invoice.FreeTaxSalesAmount = amount.FreeTaxSalesAmount;
                invoice.FreeTaxSalesAmountSpecified = amount.FreeTaxSalesAmount.HasValue;
                invoice.ZeroTaxSalesAmount = amount.ZeroTaxSalesAmount;
                invoice.ZeroTaxSalesAmountSpecified = amount.ZeroTaxSalesAmount.HasValue;
                invoice.DiscountAmount = amount.DiscountAmount ?? 0m;
                invoice.DiscountAmountSpecified = amount.DiscountAmount.HasValue;
            }

            // 發票明細
            invoice.InvoiceItem = BuildInvoiceItemDetails(item);

            return new InvoiceRoot
            {
                CompanyBan = item.InvoiceSeller?.ReceiptNo,
                ProcessType = item.CDS_Document?.ProcessType is int processType
                    ? ((Naming.InvoiceProcessType)processType).ToString()
                    : null,
                Invoice = new[] { invoice },
            };
        }

        /// <summary>將發票商品明細（<see cref="InvoiceProduct"/> → <see cref="InvoiceProductItem"/>）展開為交換用明細列。</summary>
        private static InvoiceRootInvoiceInvoiceItem[] BuildInvoiceItemDetails(InvoiceItem item)
        {
            var details = new List<InvoiceRootInvoiceInvoiceItem>();
            short sequence = 1;

            foreach (var product in item.Product)
            {
                foreach (var productItem in product.InvoiceProductItem)
                {
                    details.Add(new InvoiceRootInvoiceInvoiceItem
                    {
                        Description = product.Brief,
                        Quantity = productItem.Piece ?? 0m,
                        Unit = productItem.PieceUnit,
                        UnitPrice = productItem.UnitCost ?? 0m,
                        Amount = productItem.CostAmount ?? 0m,
                        SequenceNumber = sequence++,
                        Item = productItem.ItemNo,
                        Remark = productItem.Remark,
                        TaxType = productItem.TaxType,
                        TaxTypeSpecified = productItem.TaxType.HasValue,
                    });
                }
            }

            return details.ToArray();
        }

    }
}