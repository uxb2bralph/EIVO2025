using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

using InvoiceClient.Helper;
using InvoiceClient.Properties;

using ModelCore.InvoiceManagement.Validator;
using ModelCore.Resource;
using ModelCore.Schema.EIVO;
using ModelCore.Schema.EIVO.B2B;
using ModelCore.Schema.TXN;
using CommonLib.Core.Utility;
using CommonLib.Utility;

namespace InvoiceClient.Agent.POSHelper
{
    public class POSInvoiceWatcher : InvoiceWatcher
    {
        public POSInvoiceWatcher(String fullPath)
            : base(fullPath)
        {

        }

        private Root processUploadCore(eInvoiceServiceClient invSvc, XmlDocument docInv)
        {
            DateTime ts = DateTime.Now;
            Console.WriteLine($"start converting xml to object at {ts}");
            InvoiceRoot invoice = docInv.TrimAll().ConvertTo<InvoiceRoot>();

            Root result = new Root
            {
                UXB2B = "電子發票系統",
                Result = new RootResult
                {
                    timeStamp = DateTime.Now,
                    value = 0
                }
            };

            List<AutomationItem> automation = new List<AutomationItem>();
            List<InvoiceRootInvoice> eventItems = new List<InvoiceRootInvoice>();
            var items = applyInvoiceTrackNo(invoice, eventItems);
            if (items.Count > 0)
            {
                result.Response = new RootResponse
                {
                    InvoiceNo =
                    items.Select(d => new RootResponseInvoiceNo
                    {
                        Value = invoice.Invoice[d.Key].DataNumber,
                        Description = d.Value.Message,
                        ItemIndexSpecified = true,
                        ItemIndex = d.Key,
                    }).ToArray()
                };

                //失敗Response
                automation.AddRange(items.Select(d => new AutomationItem
                {
                    Description = d.Value.Message,
                    Status = 0,
                    Invoice = new AutomationItemInvoice
                    {
                        DataNumber = invoice.Invoice[d.Key].DataNumber
                    }
                }));
            }
            else
            {
                result.Result.value = 1;
            }

            //成功Response
            if (eventItems.Count > 0)
            {
                automation.AddRange(eventItems.Select(i => new AutomationItem
                {
                    Description = "",
                    Status = 1,
                    Invoice = new AutomationItemInvoice
                    {
                        InvoiceNumber = i.InvoiceNumber,
                        DataNumber = i.DataNumber,
                        InvoiceDate = i.InvoiceDate,
                        InvoiceTime = i.InvoiceTime,
                    }
                }));

                InvoiceRoot prepared = new InvoiceRoot
                {
                    Invoice = eventItems.ToArray()
                };

                var preparedDoc = prepared.ConvertToXml();
                preparedDoc.Save(Path.Combine(POSReady._Settings.PreparedInvoice, $"{Guid.NewGuid()}.xml"));
                preparedDoc.Save(Path.Combine(POSReady._Settings.SellerInvoice, $"{Guid.NewGuid()}.xml"));

            }
            result.Automation = automation.ToArray();
            Console.WriteLine($"total seconds: {(DateTime.Now - ts).TotalSeconds}");

            return result;
        }

        protected override XmlDocument prepareInvoiceDocument(string invoiceFile)
        {
            invoiceFile.ReviseXmlContent();
            return base.prepareInvoiceDocument(invoiceFile);
        }

        protected override Root processUpload(eInvoiceServiceClient invSvc, XmlDocument docInv)
        {
            return processUploadCore(invSvc, docInv);
        }

        private Dictionary<int, Exception> applyInvoiceTrackNo(InvoiceRoot item, List<InvoiceRootInvoice> eventItems)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();
            DateTime invoiceDate = DateTime.Now;

            if (item != null && item.Invoice != null && item.Invoice.Length > 0)
            {
                for (int idx = 0; idx < item.Invoice.Length; idx++)
                {
                    try
                    {
                        var invItem = item.Invoice[idx];

                        if (POSReady.Settings.SellerReceiptNo == null)
                        {
                            result.Add(idx, new Exception("Seller Id for allocating invoice no not ready."));
                            continue;
                        }
                        else if (invItem.SellerId != POSReady.Settings.SellerReceiptNo)
                        {
                            result.Add(idx, new Exception(String.Format(MessageResources.InvalidSellerOrAgent, invItem.SellerId, POSReady.Settings.SellerReceiptNo)));
                            continue;
                        }

                        invItem.BuyerId = invItem.BuyerId.GetEfficientString();
                        if (invItem.BuyerId == null)
                        {
                            invItem.BuyerId = "0000000000";
                        }

                        InvoiceIssue issue = InvoiceNoInspector.ConsumeInvoiceNo(invoiceDate);

                        if (issue != null)
                        {
                            if (invItem.PrintMark == "N" || invItem.PrintMark == "n")
                            {
                                invItem.RandomNumber = invItem.RandomNumber.GetEfficientString();
                                if (invItem.RandomNumber == null)
                                {
                                    invItem.RandomNumber = issue.random;
                                }

                                invItem.CarrierId1 = invItem.CarrierId1.GetEfficientString();
                                if (invItem.CarrierId1?.Length == 8 && InvoiceRootInvoiceValidator.__MatchCellPhoneBarcode.IsMatch(invItem.CarrierId1))
                                {
                                    invItem.CarrierType = InvoiceRootInvoiceValidator.__CELLPHONE_BARCODE;
                                }
                            }
                            else
                            {
                                invItem.RandomNumber = issue.random;
                            }
                            invItem.InvoiceNumber = issue.sn;
                            if (invItem.CustomerDefined == null)
                            {
                                invItem.CustomerDefined = new InvoiceRootInvoiceCustomerDefined
                                {

                                };
                            }
                            invItem.CustomerDefined.ProjectNo = issue.aesbase64;
                            invItem.InvoiceDate = $"{invoiceDate:yyyy/MM/dd}";
                            invItem.InvoiceTime = $"{invoiceDate:HH:mm:ss}";
                            eventItems.Add(invItem);
                        }
                        else
                        {
                            result.Add(idx, new Exception(String.Format(MessageResources.AlertNullTrackNoInterval, invItem.SellerId)));
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        result.Add(idx, ex);
                    }
                }
            }

            return result;
        }

    }
}
