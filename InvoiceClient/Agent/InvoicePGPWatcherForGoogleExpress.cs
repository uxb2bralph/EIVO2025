using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using InvoiceClient.Properties;
using System.Xml;
using CommonLib.Core.Utility;
using CommonLib.Utility;
using InvoiceClient.Helper;
using ModelCore.Schema.EIVO;
using System.Threading;
using ModelCore.Schema.TXN;
using System.Diagnostics;
using System.Globalization;
using ModelCore.Locale;

using ModelCore.InvoiceManagement;
using ModelCore.DataEntity;
using Business.Helper.InvoiceProcessor;

namespace InvoiceClient.Agent
{
    public class InvoicePGPWatcherForGoogleExpress : InvoicePGPWatcherForGoogle
    {
        public DateTime? ApplyInvoiceDate { get; set; }
        public InvoicePGPWatcherForGoogleExpress(String fullPath) : base(fullPath)
        {

        }

        protected override void processFile(String invFile)
        {
            if (!File.Exists(invFile))
                return;

            String fileName = Path.GetFileName(invFile);
            String invoiceRequest = fileName;
            String fullPath = Path.Combine(_inProgressPath, fileName);
            try
            {
                File.Move(invFile, fullPath);
            }
            catch (Exception ex)
            {
                Logger.Warn("move file error: " + invFile);
                Logger.Error(ex);
                return;
            }

            if (fullPath.EndsWith(".gpg", StringComparison.CurrentCultureIgnoreCase)
                    || fullPath.EndsWith(".pgp", StringComparison.CurrentCultureIgnoreCase))
            {
                processPGP(fullPath);
                return;
            }

            Root result = new Root();
            try
            {
                XmlDocument docInv = prepareInvoiceDocument(fullPath);
                result = processUploadCore(null, docInv, invoiceRequest);

                if (result.Result.value != 1)
                {
                    if (result.Response != null && result.Response.InvoiceNo != null && result.Response.InvoiceNo.Length > 0)
                    {
                        processError(result.Response.InvoiceNo, docInv, fileName);
                        storeFile(fullPath, Path.Combine(Logger.LogDailyPath, fileName));
                    }
                    else
                    {
                        processError(result.Result.message, docInv, fileName);
                        storeFile(fullPath, Path.Combine(_failedTxnPath, fileName));
                    }
                }
                else
                {
                    storeFile(fullPath, Path.Combine(Logger.LogDailyPath, fileName));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                storeFile(fullPath, Path.Combine(_failedTxnPath, fileName));
            }
            finally
            {
                if (result.Automation != null)
                {
                    Automation auto = new Automation { Item = result.Automation };
                    String responseName = fileName.Replace("request", "response")
                            .Replace("_OUT_", "_IN_");
                    responseName = Path.Combine(_ResponsedPath, responseName);
                    auto.ConvertToXml().SaveDocumentWithEncoding(responseName);
                }
            }
        }

        protected override Root processUpload(eInvoiceServiceClient invSvc, XmlDocument docInv)
        {
            return processUploadCore(invSvc, docInv, null);
        }


        private Root processUploadCore(eInvoiceServiceClient invSvc, XmlDocument docInv,String invoiceRequest)
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

            using (GoogleInvoiceManagerV2 models = new GoogleInvoiceManagerV2 { InvoiceClientID = Settings.Default.ClientID, ChannelID = (int)determineChannelID(invoiceRequest), IgnoreDuplicateDataNumberException = true, ApplyInvoiceDate = this.ApplyInvoiceDate })
            {
                ///憑證資料檢查
                ///
                var token = models.GetTable<OrganizationToken>().Where(t => t.Thumbprint == AppSigner.SignerCertificate.Thumbprint).FirstOrDefault();
                if (token != null)
                {
                    var requestItem = new ProcessRequest
                    {
                        AgentID = token.CompanyID,
                        SubmitDate = DateTime.Now,
                        RequestPath = invoiceRequest,
                        ProcessType = (int)Naming.InvoiceProcessType.C0401_Xml_CBE,
                    };
                    models.GetTable<ProcessRequest>().InsertOnSubmit(requestItem);
                    models.SubmitChanges();

                    List<AutomationItem> automation = new List<AutomationItem>();
                    var items = models.SaveUploadInvoiceAutoTrackNo(invoice, token);
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
                    if (models.EventItems != null && models.EventItems.Count > 0)
                    {
                        automation.AddRange(models.EventItems.Select(i => new AutomationItem
                        {
                            Description = "",
                            Status = 1,
                            Invoice = new AutomationItemInvoice
                            {
                                InvoiceNumber = i.TrackCode + i.No,
                                DataNumber = i.InvoicePurchaseOrder.OrderNo,
                                InvoiceDate = String.Format("{0:yyyy/MM/dd}", i.InvoiceDate),
                                InvoiceTime = String.Format("{0:HH:mm:ss}", i.InvoiceDate),
                            }
                        }));

                        models.BindProcessedItem(requestItem);
                    }

                    result.Automation = automation.ToArray();
                }
                else
                {
                    result.Result.message = "Merchant evidence does not match the validation!!";
                }
            }

            Console.WriteLine($"total seconds: {(DateTime.Now - ts).TotalSeconds}");

            return result;
        }

    }
}
