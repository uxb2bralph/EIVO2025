using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Services;
using System.Xml;
using CoreWCF;
using CoreWCF.Configuration;

using WebHome.Helper;
using ModelCore.Models.ViewModel;

using WebHome.Properties;

using ModelCore.DataEntity;
using ModelCore.DocumentManagement;
using ModelCore.Helper;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;
using ModelCore.Schema.EIVO;
using ModelCore.Schema.EIVO.B2B;
using ModelCore.Schema.TurnKey;
using ModelCore.Schema.TXN;
using ModelCore.InvoiceManagement.ErrorHandle;
using Newtonsoft.Json;

using CommonLib.Security.UseCrypto;
using CommonLib.Utility;
using CommonLib.Core.Utility;
using ModelCore.Notification;
using ModelCore.Service;
using ModelCore.InvoiceManagement.InvoiceProcess;
using Business.Helper.InvoiceProcessor;

namespace WebHome.Published
{
    /// <summary>
    /// Summary description for eInvoiceService
    /// </summary>
    [ServiceContract(Namespace = "http://www.uxb2b.com/")]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public partial class eInvoiceService 
    {

        static eInvoiceService()
        {
            //ExceptionNotification.SendExceptionNotification =
            //    (o, e) =>
            //    {
            //        try
            //        {
            //            String subject = string.Format("{0} ERP/Gateway資料傳送異常通知",
            //                 e.Enterprise.EnterpriseName);

            //            String.Format("{0}{1}?companyID={2}",
            //                ModelExtension.Properties.AppSettings.Default.HostUrl,
            //                VirtualPathUtility.ToAbsolute(ModelExtension.Properties.AppSettings.Default.ExceptionNotificationUrl),
            //                e.CompanyID)
            //                .MailWebPage(String.IsNullOrEmpty(e.EMail) ? Uxnet.Web.Properties.ModelExtension.Properties.AppSettings.Default.WebMaster : e.EMail, subject);

            //        }
            //        catch (Exception ex)
            //        {
            //            Logger.Error(ex);
            //        }
            //    };

            //EIVONotificationFactory.NotifyCommissionedToReceive =
            //(o, e) =>
            //{
            //    try
            //    {
            //        using (InvoiceManager mgr = new InvoiceManager())
            //        {
            //            var seller = mgr.GetTable<Organization>().Where(c => c.CompanyID == e.Argument.Seller.CompanyID)
            //                .First().EnterpriseGroupMember.FirstOrDefault().EnterpriseGroup;

            //            var mailto = String.Join(",",
            //                mgr.GetUserListByCompanyID(e.Argument.MailToID, Naming.CategoryID.COMP_ENTERPRISE_GROUP)
            //            .Select(u => u.EMail)
            //            .Where(m => m != null));

            //            if (!String.IsNullOrEmpty(mailto))
            //            {
            //                string subject = String.Format("{0}{1}", e.Argument.Subject != null ? e.Argument.Subject : seller != null ? seller.EnterpriseName : "電子發票加值中心", " 自動接收通知");

            //                ((seller != null ? seller.EnterpriseName : "")
            //                    + "已開出您本期發票/收據/折讓,系統已自動幫您接受發票/收據/折讓資料," + System.Environment.NewLine
            //                    + "煩請上電子發票服務平台進行後續作業 https://eivo.uxifs.com")
            //                     .SendMailMessage(mailto, subject);

            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Logger.Error(ex);
            //    }
            //};

            //EIVONotificationFactory.NotifyReceivedInvoice =
            //            (o, e) =>
            //            {
            //                try
            //                {
            //                    using (InvoiceManager mgr = new InvoiceManager())
            //                    {
            //                        var invItem = mgr.EntityList.Where(i => i.InvoiceID == e.Argument.InvoiceID).First();

            //                        String pdfFile = mgr.PrepareToDownload(invItem, false);

            //                        System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(pdfFile, MediaTypeNames.Application.Octet);
            //                        ///修改附件檔名為發票號碼
            //                        ///
            //                        attachment.Name = String.Format("{0}{1}.pdf", e.Argument.TrackCode, e.Argument.No);

            //                        var mailTo = String.Join(",",
            //                            mgr.GetUserListByCompanyID(invItem.InvoiceBuyer.BuyerID, Naming.CategoryID.COMP_ENTERPRISE_GROUP)
            //                        .Select(u => u.EMail)
            //                        .Where(m => m != null));

            //                        if (!String.IsNullOrEmpty(mailTo))
            //                        {
            //                            var enterprise = invItem.InvoiceSeller.Organization.EnterpriseGroupMember.FirstOrDefault();

            //                            String Subject = String.Format("{0} 發票已接收通知(發票號碼:{1}{2})"
            //                                , enterprise != null ? enterprise.EnterpriseGroup.EnterpriseName : null, e.Argument.TrackCode, e.Argument.No);
            //                            String Body = " 您已接收本期由" + invItem.InvoiceSeller.CustomerName + "開出之發票資料,請參考附件發票證明聯,"
            //                                + System.Environment.NewLine
            //                                + "亦可登入電子發票平台查詢電子發票相關資訊," + System.Environment.NewLine
            //                                + "關於電子發票服務請至 https://eivo.uxifs.com";

            //                            Body.SendMailMessage(mailTo, Subject, new System.Net.Mail.Attachment[] { attachment });
            //                        }
            //                    }

            //                }
            //                catch (Exception ex)
            //                {
            //                    Logger.Error(ex);
            //                }
            //            };

            ExceptionNotification.SendExceptionNotification =
                (o, e) =>
                {
                    try
                    {
                        MailMessage message = new MailMessage();
                        message.ReplyToList.Add(ModelExtension.Properties.AppSettings.Default.ReplyTo);
                        message.From = new MailAddress(ModelExtension.Properties.AppSettings.Default.WebMaster);

                        if (String.IsNullOrEmpty(e.EMail))
                        {
                            message.To.Add(ModelExtension.Properties.AppSettings.Default.WebMaster);
                        }
                        else
                        {
                            String[] MailAdr = e.EMail.Trim().Split(',', ';', '、');
                            foreach (var addr in MailAdr)
                            {
                                if (!String.IsNullOrEmpty(addr))
                                {
                                    message.To.Add(addr);
                                }
                            }
                        }
                        message.Subject = "電子發票系統 營業人資料傳送異常通知";
                        message.IsBodyHtml = true;

                        using (WebClient wc = new WebClient())
                        {
                            wc.Encoding = Encoding.UTF8;
                            message.Body = wc.DownloadString(String.Format("{0}{1}?companyID={2}&MaxLogID={3}",
                                ModelExtension.Properties.AppSettings.Default.HostUrl,
                                VirtualPathUtility.ToAbsolute(ModelExtension.Properties.AppSettings.Default.ExceptionNotificationUrl),
                                e.CompanyID, e.MaxLogID));
                        }

                        SmtpClient smtpclient = new SmtpClient(ModelExtension.Properties.AppSettings.Default.MailServer);
                        smtpclient.Credentials = CredentialCache.DefaultNetworkCredentials;
                        smtpclient.Send(message);

                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                };

            ExceptionNotification.SendExceptionNotificationToSysAdmin =
                (e) =>
                {
                    try
                    {
                        MailMessage message = new MailMessage();
                        message.ReplyToList.Add(ModelExtension.Properties.AppSettings.Default.ReplyTo);
                        message.From = new MailAddress(ModelExtension.Properties.AppSettings.Default.WebMaster);

                        message.To.Add(ModelExtension.Properties.AppSettings.Default.WebMaster);
                        message.Subject = "電子發票系統 營業人資料傳送異常通知";
                        message.IsBodyHtml = true;

                        message.Body = e.ToString();

                        SmtpClient smtpclient = new SmtpClient(ModelExtension.Properties.AppSettings.Default.MailServer);
                        smtpclient.Credentials = CredentialCache.DefaultNetworkCredentials;
                        smtpclient.Send(message);

                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                };


            InvoiceNotification.ProcessMessage = () =>
                {
                    using (ModelCore.MessageManagement.InvoiceNotificationManager mgr = new ModelCore.MessageManagement.InvoiceNotificationManager())
                    {
                        mgr.BuildMessageContent = id =>
                        {

                            NotificationMesssage msg = new NotificationMesssage
                            {
                                Subject = "開立發票通知"
                            };

                            using (WebClient wc = new WebClient())
                            {
                                wc.Encoding = Encoding.UTF8;
                                msg.Content = wc.DownloadString(String.Format("{0}{1}?id={2}",
                                    ModelExtension.Properties.AppSettings.Default.HostUrl,
                                    VirtualPathUtility.ToAbsolute("~/Published/SMSInvoiceNotification.ashx"),
                                    id));
                            }

                            return msg;
                        };

                        mgr.ExceptionHandler = SharedFunction.AlertSMSError;
                        mgr.ProcessMessage();
                    }
                };



            PortalNotification.NotifyToActivate = PortalExtensionMethods.NotifyToActivate;
            PortalNotification.NotifyToResetPassword = PortalExtensionMethods.NotifyToResetPassword;

            PdfDocumentGenerator.MergePDF = (pdfOut, pdfSource) => pdfOut.MergePDF(pdfSource);

            AddOn();

        }

        static partial void AddOn();

        public static void StartUp() { }

        protected String? _clientID;
        protected int _channelID;
        protected Naming.InvoiceDataScope _dataScope = Naming.InvoiceDataScope.ForAll;

        private readonly IHttpContextAccessor? _httpContextAccessor;
        public eInvoiceService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            var cookie = _httpContextAccessor?.HttpContext?.Request?.Cookies?["cLang"];
            if (cookie != null && cookie != Settings.Default.DefaultUILanguage)
            {
                Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(cookie);
            }

        }

        [OperationContract]
        public virtual XmlDocument UploadInvoice(XmlDocument uploadData)
        {
            Root result = createMessageToken();

            try
            {
                CryptoUtility crypto = new CryptoUtility();
                uploadData.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(uploadData))
                {
                    InvoiceRoot invoice = uploadData.TrimAll().ConvertTo<InvoiceRoot>();
                    using (InvoiceManager mgr = new InvoiceManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            var items = mgr.SaveUploadInvoice(invoice, token);
                            if (items.Count > 0)
                            {
                                result.Response = new RootResponse
                                {
                                    InvoiceNo =
                                    items.Select(d => new RootResponseInvoiceNo
                                    {
                                        Value = invoice.Invoice[d.Key].InvoiceNumber,
                                        Description = d.Value.Message,
                                        ItemIndexSpecified = true,
                                        ItemIndex = d.Key
                                    }).ToArray()
                                };

                                ThreadPool.QueueUserWorkItem(ExceptionNotification.SendNotification,
                                    new ExceptionInfo
                                    {
                                        Token = token,
                                        ExceptionItems = items,
                                        InvoiceData = invoice
                                    });
                            }
                            else
                            {
                                result.Result.value = 1;
                            }

                            if (mgr.HasItem && token.Organization.OrganizationStatus.PrintAll == true)
                            {
                                SharedFunction.SendMailMessage(token.Organization.CompanyName + "電子發票已匯入,請執行發票列印作業!!", ModelExtension.Properties.AppSettings.Default.WebMaster, token.Organization.CompanyName + "電子發票開立郵件通知");
                            }

                        }
                        else
                        {
                            result.Result.message = "營業人憑證資料驗證不符!!";
                        }
                    }
                }
                else
                {
                    result.Result.message = "發票資料簽章不符!!";
                }

                //EIVONotificationFactory.Notify();

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.Result.message = ex.Message;
            }
            return result.ConvertToXml();
        }

        [OperationContract]
        public virtual XmlDocument UploadAllowance(XmlDocument uploadData)
        {
            Root result = createMessageToken();

            try
            {
                CryptoUtility crypto = new CryptoUtility();
                uploadData.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(uploadData))
                {
                    AllowanceRoot allowance = uploadData.TrimAll().ConvertTo<AllowanceRoot>();
                    using (InvoiceManager mgr = new InvoiceManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            var items = mgr.SaveUploadAllowance(allowance, token);
                            if (items.Count > 0)
                            {
                                result.Response = new RootResponse
                                {
                                    InvoiceNo =
                                    items.Select(d => new RootResponseInvoiceNo
                                    {
                                        Value = allowance.Allowance[d.Key].AllowanceNumber,
                                        Description = d.Value.Message,
                                        ItemIndexSpecified = true,
                                        ItemIndex = d.Key
                                    }).ToArray()
                                };

                                ThreadPool.QueueUserWorkItem(ExceptionNotification.SendNotification,
                                    new ExceptionInfo
                                    {
                                        Token = token,
                                        ExceptionItems = items,
                                        AllowanceData = allowance
                                    });
                            }
                            else
                            {
                                result.Result.value = 1;
                            }
                        }
                        else
                        {
                            result.Result.message = "營業人憑證資料驗證不符!!";
                        }
                    }
                }
                else
                {
                    result.Result.message = "發票資料簽章不符!!";
                }


            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.Result.message = ex.Message;
            }
            return result.ConvertToXml();
        }

        //[OperationContract]
        //public virtual XmlDocument UploadB1401(XmlDocument uploadData)
        //{
        //    Root result = createMessageToken();

        //    try
        //    {
        //        CryptoUtility crypto = new CryptoUtility();
        //        uploadData.PreserveWhitespace = true;
        //if (crypto.VerifyXmlSignature(uploadData))
        //        {
        //            ModelCore.Schema.TurnKey.B1401.Allowance allowance = uploadData.TrimAll().ConvertTo<ModelCore.Schema.TurnKey.B1401.Allowance>();
        //            using (B2BInvoiceManager mgr = new B2BInvoiceManager())
        //            {
        //                ///憑證資料檢查
        //                ///
        //                var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
        //                if (token != null)
        //                {
        //                    mgr.SaveB1401(allowance, token);
        //                    result.Result.value = 1;
        //                }
        //                else
        //                {
        //                    result.Result.message = "會員憑證資料驗證不符!!";
        //                }
        //            }
        //        }
        //        else
        //        {
        //            result.Result.message = "發票資料簽章不符!!";
        //        }

        //        EIVOPlatformFactory.Notify();
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error(ex);
        //        result.Result.message = ex.Message;
        //    }
        //    return result.ConvertToXml();
        //}

        [OperationContract]
        public virtual XmlDocument UploadG0401(XmlDocument uploadData)
        {
            Root result = createMessageToken();
            using (InvoiceManagerV3 manager = new InvoiceManagerV3 { })
            {
                manager.UploadAllowance(uploadData, result, Naming.InvoiceProcessType.G0401);
            }
            return result.ConvertToXml();
        }

        [OperationContract]
        public virtual XmlDocument UploadG0501(XmlDocument uploadData)
        {
            Root result = createMessageToken();

            try
            {
                CryptoUtility crypto = new CryptoUtility();
                uploadData.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(uploadData))
                {
                    using (B2BInvoiceManager mgr = new B2BInvoiceManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            mgr.SaveAllowanceCancellationMIG(uploadData, Naming.InvoiceProcessType.G0501);
                            result.Result.value = 1;
                        }
                        else
                        {
                            result.Result.message = "會員憑證資料驗證不符!!";
                        }
                    }
                }
                else
                {
                    result.Result.message = "發票資料簽章不符!!";
                }
                //EIVONotificationFactory.Notify();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.Result.message = ex.Message;
            }
            return result.ConvertToXml();
        }


        [OperationContract]
        public virtual XmlDocument UploadInvoiceAutoTrackNo(XmlDocument uploadData)
        {
            return UploadInvoiceAutoTrackNoV2(uploadData);

            //Root result = createMessageToken();

            //try
            //{
            //    CryptoUtility crypto = new CryptoUtility();
            //    uploadData.PreserveWhitespace = true;
            //if (crypto.VerifyXmlSignature(uploadData))
            //    {
            //        InvoiceRoot invoice = uploadData.TrimAll().ConvertTo<InvoiceRoot>();
            //        using (InvoiceManager mgr = new InvoiceManager())
            //        {
            //            ///憑證資料檢查
            //            ///
            //            var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
            //            if (token != null)
            //            {
            //                var items = mgr.SaveUploadInvoiceAutoTrackNo(invoice, token);
            //                if (items.Count > 0)
            //                {
            //                    result.Response = new RootResponse
            //                    {
            //                        InvoiceNo =
            //                        items.Select(d => new RootResponseInvoiceNo
            //                        {
            //                            Value = invoice.Invoice[d.Key].InvoiceNumber,
            //                            Description = d.Value.Message,
            //                            ItemIndexSpecified = true,
            //                            ItemIndex = d.Key
            //                        }).ToArray()
            //                    };

            //                    ThreadPool.QueueUserWorkItem(ExceptionNotification.SendNotification,
            //                        new ExceptionInfo
            //                        {
            //                            Token = token,
            //                            ExceptionItems = items,
            //                            InvoiceData = invoice
            //                        });
            //                }
            //                else
            //                {
            //                    result.Result.value = 1;
            //                }

            //                if (mgr.HasItem && token.Organization.OrganizationStatus.PrintAll == true)
            //                {
            //                    SharedFunction.SendMailMessage(token.Organization.CompanyName + "電子發票已匯入,請執行發票列印作業!!", ModelExtension.Properties.AppSettings.Default.WebMaster, token.Organization.CompanyName + "電子發票開立郵件通知");
            //                }

            //            }
            //            else
            //            {
            //                result.Result.message = "營業人憑證資料驗證不符!!";
            //            }
            //        }
            //    }
            //    else
            //    {
            //        result.Result.message = "發票資料簽章不符!!";
            //    }

            //    
            //}
            //catch (Exception ex)
            //{
            //    Logger.Error(ex);
            //    result.Result.message = ex.Message;
            //}
            //return result.ConvertToXml();
        }

        [OperationContract]
        public virtual XmlDocument UploadInvoiceCmsCSVAutoTrackNo(byte[] uploadData)
        {
            Root result = createMessageToken();

            try
            {
                CryptoUtility crypto = new CryptoUtility();
                byte[] dataToSign;
                if (crypto.VerifyEnvelopedPKCS7(uploadData, out dataToSign))
                {
                    String fileName = Path.Combine(Logger.LogDailyPath, String.Format("Invoice_{0}.csv", Guid.NewGuid()));
                    using (FileStream fs = new FileStream(fileName, FileMode.Create))
                    {
                        fs.Write(dataToSign, 0, dataToSign.Length);
                        fs.Flush();
                        fs.Close();
                    }

                    using (InvoiceManager mgr = new InvoiceManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            CsvInvoiceUploadManager csvMgr = new CsvInvoiceUploadManager(mgr, token.CompanyID);
                            Encoding encoding = dataToSign.IsUtf8(dataToSign.Length) ? Encoding.UTF8 : Encoding.GetEncoding(ModelExtension.Properties.AppSettings.Default.CsvUploadEncoding);
                            csvMgr.ParseData(null, fileName, encoding);
                            if (!csvMgr.Save())
                            {
                                var items = csvMgr.ErrorList;
                                result.Response = new RootResponse
                                {
                                    InvoiceNo =
                                    items.Select(d => new RootResponseInvoiceNo
                                    {
                                        Value = d.DataContent,
                                        Description = d.Status,
                                        ItemIndexSpecified = true,
                                        ItemIndex = csvMgr.ItemList.IndexOf(d)
                                    }).ToArray()
                                };

                                ThreadPool.QueueUserWorkItem(ExceptionNotification.SendNotification,
                                    new ExceptionInfo
                                    {
                                        Token = token,
                                        InvoiceError = items
                                    });
                            }
                            else
                            {
                                result.Result.value = 1;

                                if (token.Organization.OrganizationStatus.PrintAll == true)
                                {
                                    SharedFunction.SendMailMessage(token.Organization.CompanyName + "電子發票已匯入,請執行發票列印作業!!", ModelExtension.Properties.AppSettings.Default.WebMaster, token.Organization.CompanyName + "電子發票開立郵件通知");
                                }

                            }
                        }
                        else
                        {
                            result.Result.message = "營業人憑證資料驗證不符!!";
                        }
                    }
                }
                else
                {
                    result.Result.message = "發票資料簽章不符!!";
                }

                //EIVONotificationFactory.Notify();

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.Result.message = ex.Message;
            }
            return result.ConvertToXml();
        }

        [OperationContract]
        public virtual XmlDocument UploadInvoiceCancellationCmsCSV(byte[] uploadData)
        {
            Root result = createMessageToken();

            try
            {
                CryptoUtility crypto = new CryptoUtility();
                byte[] dataToSign;
                if (crypto.VerifyEnvelopedPKCS7(uploadData, out dataToSign))
                {
                    String fileName = Path.Combine(Logger.LogDailyPath, String.Format("CancelInvoie_{0}.csv", Guid.NewGuid()));
                    using (FileStream fs = new FileStream(fileName, FileMode.Create))
                    {
                        fs.Write(dataToSign, 0, dataToSign.Length);
                        fs.Flush();
                        fs.Close();
                    }

                    using (InvoiceManager mgr = new InvoiceManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            CsvInvoiceCancellationUploadManager csvMgr = new CsvInvoiceCancellationUploadManager(mgr, token.CompanyID);
                            Encoding encoding = dataToSign.IsUtf8(dataToSign.Length) ? Encoding.UTF8 : Encoding.GetEncoding(ModelExtension.Properties.AppSettings.Default.CsvUploadEncoding);
                            csvMgr.ParseData(null, fileName, encoding);
                            if (!csvMgr.Save())
                            {
                                var items = csvMgr.ErrorList;
                                result.Response = new RootResponse
                                {
                                    InvoiceNo =
                                    items.Select(d => new RootResponseInvoiceNo
                                    {
                                        Value = d.DataContent,
                                        Description = d.Status,
                                        ItemIndexSpecified = true,
                                        ItemIndex = csvMgr.ItemList.IndexOf(d)
                                    }).ToArray()
                                };

                                ThreadPool.QueueUserWorkItem(ExceptionNotification.SendNotification,
                                    new ExceptionInfo
                                    {
                                        Token = token,
                                        InvoiceCancellationError = items
                                    });
                            }
                            else
                            {
                                result.Result.value = 1;
                            }
                        }
                        else
                        {
                            result.Result.message = "營業人憑證資料驗證不符!!";
                        }
                    }
                }
                else
                {
                    result.Result.message = "發票資料簽章不符!!";
                }


            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.Result.message = ex.Message;
            }
            return result.ConvertToXml();
        }

        [OperationContract]
        public virtual XmlDocument UploadAllowanceCancellationCmsCSV(byte[] uploadData)
        {
            Root result = createMessageToken();

            try
            {
                CryptoUtility crypto = new CryptoUtility();
                byte[] dataToSign;
                if (crypto.VerifyEnvelopedPKCS7(uploadData, out dataToSign))
                {
                    String fileName = Path.Combine(Logger.LogDailyPath, String.Format("CancelInvoie_{0}.csv", Guid.NewGuid()));
                    using (FileStream fs = new FileStream(fileName, FileMode.Create))
                    {
                        fs.Write(dataToSign, 0, dataToSign.Length);
                        fs.Flush();
                        fs.Close();
                    }

                    using (InvoiceManager mgr = new InvoiceManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            CsvAllowanceCancellationUploadManager csvMgr = new CsvAllowanceCancellationUploadManager(mgr, token.CompanyID);
                            Encoding encoding = dataToSign.IsUtf8(dataToSign.Length) ? Encoding.UTF8 : Encoding.GetEncoding(ModelExtension.Properties.AppSettings.Default.CsvUploadEncoding);
                            csvMgr.ParseData(null, fileName, encoding);
                            if (!csvMgr.Save())
                            {
                                var items = csvMgr.ErrorList;
                                result.Response = new RootResponse
                                {
                                    InvoiceNo =
                                    items.Select(d => new RootResponseInvoiceNo
                                    {
                                        Value = d.DataContent,
                                        Description = d.Status,
                                        ItemIndexSpecified = true,
                                        ItemIndex = csvMgr.ItemList.IndexOf(d)
                                    }).ToArray()
                                };

                                ThreadPool.QueueUserWorkItem(ExceptionNotification.SendNotification,
                                    new ExceptionInfo
                                    {
                                        Token = token,
                                        AllowanceCancellationError = items
                                    });
                            }
                            else
                            {
                                result.Result.value = 1;
                                //csvMgr.ItemList.Select(i => i.Entity.InvoiceID).SendB2CInvoiceCancellationMail();
                            }
                        }
                        else
                        {
                            result.Result.message = "營業人憑證資料驗證不符!!";
                        }
                    }
                }
                else
                {
                    result.Result.message = "發票資料簽章不符!!";
                }


            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.Result.message = ex.Message;
            }
            return result.ConvertToXml();
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

        [OperationContract]
        public virtual XmlDocument UploadInvoiceCancellation(XmlDocument uploadData)
        {
            Root result = createMessageToken();

            try
            {
                CryptoUtility crypto = new CryptoUtility();
                uploadData.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(uploadData))
                {
                    CancelInvoiceRoot item = uploadData.TrimAll().ConvertTo<CancelInvoiceRoot>();
                    using (InvoiceManager mgr = new InvoiceManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            var items = mgr.SaveUploadInvoiceCancellation(item, token);
                            if (items.Count > 0)
                            {
                                result.Response = new RootResponse
                                {
                                    InvoiceNo =
                                    items.Select(d => new RootResponseInvoiceNo
                                    {
                                        Value = item.CancelInvoice[d.Key].CancelInvoiceNumber,
                                        Description = d.Value.Message,
                                        ItemIndexSpecified = true,
                                        ItemIndex = d.Key,
                                        StatusCode = (d.Value is MarkToRetryException) ? "R01" : null,
                                    }).ToArray()
                                };

                                ThreadPool.QueueUserWorkItem(ExceptionNotification.SendNotification,
                                                                    new ExceptionInfo
                                                                    {
                                                                        Token = token,
                                                                        ExceptionItems = items,
                                                                        CancelInvoiceData = item
                                                                    });
                            }
                            else
                            {
                                result.Result.value = 1;
                            }
                        }
                        else
                        {
                            result.Result.message = "營業人憑證資料驗證不符!!";
                        }
                    }
                }
                else
                {
                    result.Result.message = "發票資料簽章不符!!";
                }


            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.Result.message = ex.Message;
            }
            return result.ConvertToXml();
        }

        [OperationContract]
        public virtual XmlDocument UploadAllowanceCancellation(XmlDocument uploadData)
        {
            Root result = createMessageToken();

            try
            {
                CryptoUtility crypto = new CryptoUtility();
                uploadData.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(uploadData))
                {
                    CancelAllowanceRoot item = uploadData.TrimAll().ConvertTo<CancelAllowanceRoot>();
                    using (InvoiceManager mgr = new InvoiceManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            var items = mgr.SaveUploadAllowanceCancellation(item, token);
                            if (items.Count > 0)
                            {
                                result.Response = new RootResponse
                                {
                                    InvoiceNo =
                                    items.Select(d => new RootResponseInvoiceNo
                                    {
                                        Value = item.CancelAllowance[d.Key].CancelAllowanceNumber,
                                        Description = d.Value.Message,
                                        ItemIndexSpecified = true,
                                        ItemIndex = d.Key
                                    }).ToArray()
                                };

                                ThreadPool.QueueUserWorkItem(ExceptionNotification.SendNotification,
                                                                    new ExceptionInfo
                                                                    {
                                                                        Token = token,
                                                                        ExceptionItems = items,
                                                                        CancelAllowanceData = item
                                                                    });
                            }
                            else
                            {
                                result.Result.value = 1;
                            }
                        }
                        else
                        {
                            result.Result.message = "營業人憑證資料驗證不符!!";
                        }
                    }
                }
                else
                {
                    result.Result.message = "發票資料簽章不符!!";
                }


            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.Result.message = ex.Message;
            }
            return result.ConvertToXml();
        }

        [OperationContract]
        public XmlDocument GetUpdatedWelfareAgenciesInfo(String sellerReceiptNo)
        {
            try
            {
                using (InvoiceManager mgr = new InvoiceManager())
                {
                    var items = mgr.GetUpdatedWelfareAgenciesForSeller(sellerReceiptNo);
                    if (items != null && items.Count() > 0)
                    {
                        SocialWelfareAgenciesRoot welfare = new SocialWelfareAgenciesRoot
                        {
                            CompanyBan = sellerReceiptNo,
                            SocialWelfareAgencies = items.Select(i => new SocialWelfareAgenciesRootSocialWelfareAgencies
                            {
                                Address = i.InvoiceWelfareAgency.WelfareAgency.Organization.Addr,
                                Ban = i.InvoiceWelfareAgency.WelfareAgency.Organization.ReceiptNo,
                                Email = String.IsNullOrEmpty(i.InvoiceWelfareAgency.WelfareAgency.Organization.ContactEmail) ? "N/A" : i.InvoiceWelfareAgency.WelfareAgency.Organization.ContactEmail,
                                Name = i.InvoiceWelfareAgency.WelfareAgency.Organization.CompanyName,
                                TEL = i.InvoiceWelfareAgency.WelfareAgency.Organization.Phone,
                                Code = String.IsNullOrEmpty(i.InvoiceWelfareAgency.WelfareAgency.AgencyCode) ? "待登錄" : i.InvoiceWelfareAgency.WelfareAgency.AgencyCode
                            }).ToArray()
                        };

                        mgr.GetTable<WelfareReplication>().DeleteAllOnSubmit(items);
                        mgr.SubmitChanges();

                        return welfare.ConvertToXml();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return null;
        }

        [OperationContract]
        public XmlDocument GetWelfareAgenciesInfo(String sellerReceiptNo)
        {
            try
            {
                using (InvoiceManager mgr = new InvoiceManager())
                {
                    var items = mgr.GetWelfareAgenciesForSeller(sellerReceiptNo);
                    if (items != null && items.Count() > 0)
                    {
                        SocialWelfareAgenciesRoot welfare = new SocialWelfareAgenciesRoot
                        {
                            CompanyBan = sellerReceiptNo,
                            SocialWelfareAgencies = items.Select(i => new SocialWelfareAgenciesRootSocialWelfareAgencies
                            {
                                Address = i.WelfareAgency.Organization.Addr,
                                Ban = i.WelfareAgency.Organization.ReceiptNo,
                                Email = String.IsNullOrEmpty(i.WelfareAgency.Organization.ContactEmail) ? "N/A" : i.WelfareAgency.Organization.ContactEmail,
                                Name = i.WelfareAgency.Organization.CompanyName,
                                TEL = i.WelfareAgency.Organization.Phone,
                                Code = String.IsNullOrEmpty(i.WelfareAgency.AgencyCode) ? "待登錄" : i.WelfareAgency.AgencyCode
                            }).ToArray()
                        };
                        return welfare.ConvertToXml();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return null;
        }

        [OperationContract]
        public virtual XmlDocument GetIncomingInvoices(XmlDocument sellerInfo)
        {
            RootA0101 result = new RootA0101
            {
                UXB2B = "電子發票系統",
                Result = new RootResult
                {
                    timeStamp = DateTime.Now,
                    value = 0
                }
            };

            try
            {
                CryptoUtility crypto = new CryptoUtility();
                sellerInfo.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(sellerInfo))
                {
                    using (InvoiceManager mgr = new InvoiceManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            buildIncomingInvoices(result, mgr, token.CompanyID);
                            Root root = sellerInfo.ConvertTo<Root>();
                            acknowledgeReport(mgr, token, root.Request.periodicalIntervalSpecified ? root.Request.periodicalInterval : (int?)null);
                        }
                        else
                        {
                            result.Result.message = "營業人憑證資料驗證不符!!";
                        }
                    }
                }
                else
                {
                    result.Result.message = "發票資料簽章不符!!";
                }

                //
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.Result.message = ex.Message;
            }
            return result.ConvertToXml();
        }

        protected virtual void buildIncomingInvoices(Root result, InvoiceManager mgr, int companyID)
        {
            var table = mgr.GetTable<DocumentDownloadQueue>();
            var items = table.Join(mgr.GetTable<CDS_Document>()
                    .Where(d => d.DocumentOwner.OwnerID == companyID && d.DocType == (int)Naming.DocumentTypeDefinition.E_Invoice),
                    l => l.DocID, d => d.DocID, (l, d) => l);

            if (items.Count() > 0)
            {
                result.Response = new RootResponseForA0101
                {
                    Invoice =
                    items.Select(d => d.CDS_Document.InvoiceItem.CreateInvoiceMIG(false)).ToArray()
                };

                table.DeleteAllOnSubmit(items);
                mgr.SubmitChanges();

                result.Result.value = 1;
            }
        }

        [OperationContract]
        public virtual XmlDocument GetInvoicesMap(XmlDocument sellerInfo)
        {
            RootB2CInvoiceMapping result = new RootB2CInvoiceMapping
            {
                UXB2B = "電子發票系統",
                Result = new RootResult
                {
                    timeStamp = DateTime.Now,
                    value = 0
                }
            };

            try
            {
                CryptoUtility crypto = new CryptoUtility();
                sellerInfo.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(sellerInfo))
                {
                    using (InvoiceManager mgr = new InvoiceManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null && token.Organization.OrganizationStatus.DownloadDataNumber == true)
                        {
                            buildInvoicesMap(result, mgr, token.CompanyID);
                            Root root = sellerInfo.ConvertTo<Root>();
                            acknowledgeReport(mgr, token, root.Request.periodicalIntervalSpecified ? root.Request.periodicalInterval : (int?)null);
                        }
                        else
                        {
                            result.Result.message = "營業人憑證資料驗證不符!!";
                        }
                    }
                }
                else
                {
                    result.Result.message = "發票資料簽章不符!!";
                }

                //
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.Result.message = ex.Message;
            }
            return result.ConvertToXml();
        }

        protected virtual void buildInvoicesMap(Root result, InvoiceManager mgr, int companyID)
        {
            var table = mgr.GetTable<DocumentMappingQueue>();
            var items = table
                .Join(mgr.GetTable<CDS_Document>()
                    .Where(d => d.DocType == (int)Naming.DocumentTypeDefinition.E_Invoice)
                    .Join(mgr.GetTable<DocumentOwner>().Where(o => o.OwnerID == companyID),
                        d => d.DocID, o => o.DocID, (d, o) => d),
                q => q.DocID, d => d.DocID, (q, d) => q);

            if (items.Count() > 0)
            {
                result.Response = new RootResponseForB2CInvoiceMapping
                {
                    InvoiceMapRoot = new InvoiceMapRoot
                    {
                        InvoiceMap = items.Select(d => new InvoiceMapRootInvoiceMap
                        {
                            InvoiceNumber = d.CDS_Document.InvoiceItem.TrackCode + d.CDS_Document.InvoiceItem.No,
                            InvoiceDate = String.Format("{0:yyyy/MM/dd}", d.CDS_Document.InvoiceItem.InvoiceDate),
                            DataNumber = d.CDS_Document.InvoiceItem.InvoicePurchaseOrder.OrderNo,
                            InvoiceTime = String.Format("{0:HH:mm:ss}", d.CDS_Document.InvoiceItem.InvoiceDate),
                            SellerId = d.CDS_Document.InvoiceItem.InvoiceSeller.ReceiptNo
                        }).ToArray()
                    }
                };

                table.DeleteAllOnSubmit(items);
                mgr.SubmitChanges();

                result.Result.value = 1;
            }
        }

        [OperationContract]
        public void AcknowledgeLivingReport(XmlDocument sellerInfo)
        {
            try
            {
                CryptoUtility crypto = new CryptoUtility();
                sellerInfo.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(sellerInfo))
                {
                    using (InvoiceManager mgr = new InvoiceManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            Root root = sellerInfo.ConvertTo<Root>();
                            acknowledgeReport(mgr, token, root.Request.periodicalIntervalSpecified ? root.Request.periodicalInterval : (int?)null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        protected void acknowledgeReport(InvoiceManager mgr, OrganizationToken token, int? periodicalInterval)
        {
            mgr.ExecuteCommand(@"
                    UPDATE          OrganizationStatus
                    SET                   LastTimeToAcknowledge = {0}, RequestPeriodicalInterval = {1}
                    WHERE          (CompanyID = {2})", DateTime.Now, periodicalInterval, token.CompanyID);
        }

        [OperationContract]
        public XmlDocument GetIncomingWinningInvoices(XmlDocument sellerInfo)
        {
            try
            {
                CryptoUtility crypto = new CryptoUtility();
                sellerInfo.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(sellerInfo))
                {
                    using (InvoiceManager mgr = new InvoiceManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            var table = mgr.GetTable<InvoiceWinningNumber>();
                            var items = table.Where(w => w.InvoiceItem.CDS_Document.DocumentOwner.OwnerID == token.CompanyID && !w.DownloadDate.HasValue);
                            if (items.Count() > 0)
                            {
                                var welfare = token.Organization.InvoiceWelfareAgencies.Select(w => w.WelfareAgency.Organization).FirstOrDefault();
                                String welfareReceiptNo = welfare != null ? welfare.ReceiptNo : null;
                                BonusInvoiceRoot root = new BonusInvoiceRoot
                                {
                                    BonusInvoice = items.Select(w => new BonusInvoiceRootBonusInvoice
                                    {
                                        InvoiceNumber = String.Concat(w.InvoiceItem.TrackCode, w.InvoiceItem.No),
                                        SWABan = welfareReceiptNo
                                    }).ToArray()
                                };
                                foreach (var item in items)
                                {
                                    item.DownloadDate = DateTime.Now;
                                }
                                mgr.SubmitChanges();

                                return root.ConvertToXml();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return null;
        }

        [OperationContract]
        public String GetSignerCertificateContent(String activationKey)
        {
            using (InvoiceManager mgr = new InvoiceManager())
            {
                Guid keyID;
                if (Guid.TryParse(activationKey, out keyID))
                {
                    var item = mgr.GetTable<OrganizationToken>().Where(t => t.KeyID == keyID).FirstOrDefault();
                    if (item != null)
                    {
                        return item.PKCS12;
                    }
                }
            }
            return null;
        }

        [OperationContract]
        public XmlDocument GetRegisteredMember(XmlDocument sellerInfo)
        {
            try
            {
                CryptoUtility crypto = new CryptoUtility();
                sellerInfo.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(sellerInfo))
                {
                    using (InvoiceManager mgr = new InvoiceManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            return token.Organization.SerializeDataContractToXml();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return null;
        }

        //[OperationContract]
        //public virtual XmlDocument UploadA1101(XmlDocument uploadData)
        //{
        //    Root result = createMessageToken();

        //    try
        //    {
        //        CryptoUtility crypto = new CryptoUtility();
        //        uploadData.PreserveWhitespace = true;
        //if (crypto.VerifyXmlSignature(uploadData))
        //        {
        //            ModelCore.Schema.TurnKey.A1101.Invoice invoice = uploadData.TrimAll().ConvertTo<ModelCore.Schema.TurnKey.A1101.Invoice>();
        //            using (B2BInvoiceManager mgr = new B2BInvoiceManager())
        //            {
        //                ///憑證資料檢查
        //                ///
        //                var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
        //                if (token != null)
        //                {
        //                    mgr.SaveA1101(invoice, token);
        //                    result.Result.value = 1;
        //                }
        //                else
        //                {
        //                    result.Result.message = "會員憑證資料驗證不符!!";
        //                }
        //            }
        //        }
        //        else
        //        {
        //            result.Result.message = "發票資料簽章不符!!";
        //        }

        //        EIVOPlatformFactory.Notify();
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error(ex);
        //        result.Result.message = ex.Message;
        //    }
        //    return result.ConvertToXml();
        //}

        //[OperationContract]
        //public virtual XmlDocument UploadA1401(XmlDocument uploadData)
        //{
        //    Root result = createMessageToken();

        //    try
        //    {
        //        CryptoUtility crypto = new CryptoUtility();
        //        uploadData.PreserveWhitespace = true;
        //if (crypto.VerifyXmlSignature(uploadData))
        //        {
        //            ModelCore.Schema.TurnKey.A1401.Invoice invoice = uploadData.TrimAll().ConvertTo<ModelCore.Schema.TurnKey.A1401.Invoice>();
        //            using (B2BInvoiceManager mgr = new B2BInvoiceManager())
        //            {
        //                ///憑證資料檢查
        //                ///
        //                var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
        //                if (token != null)
        //                {
        //                    mgr.SaveA1401(invoice, token);
        //                    result.Result.value = 1;
        //                }
        //                else
        //                {
        //                    result.Result.message = "會員憑證資料驗證不符!!";
        //                }
        //            }
        //        }
        //        else
        //        {
        //            result.Result.message = "發票資料簽章不符!!";
        //        }

        //        EIVOPlatformFactory.Notify();
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error(ex);
        //        result.Result.message = ex.Message;
        //    }
        //    return result.ConvertToXml();
        //}

        [OperationContract]
        public virtual XmlDocument UploadF0401(XmlDocument uploadData)
        {
            Root result = createMessageToken();

            try
            {
                CryptoUtility crypto = new CryptoUtility();
                uploadData.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(uploadData))
                {
                    using (B2BInvoiceManager mgr = new B2BInvoiceManager { ProcessType = Naming.InvoiceProcessType.A0401 })
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            mgr.SaveInvoiceMIG(uploadData, Naming.InvoiceProcessType.F0401);
                            result.Result.value = 1;
                        }
                        else
                        {
                            result.Result.message = "會員憑證資料驗證不符!!";
                        }
                    }
                }
                else
                {
                    result.Result.message = "發票資料簽章不符!!";
                }

                //EIVONotificationFactory.Notify();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.Result.message = ex.Message;
            }
            return result.ConvertToXml();
        }

        [OperationContract]
        public virtual XmlDocument UploadF0501(XmlDocument uploadData)
        {
            Root result = createMessageToken();

            try
            {
                CryptoUtility crypto = new CryptoUtility();
                uploadData.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(uploadData))
                {
                    using (B2BInvoiceManager mgr = new B2BInvoiceManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            mgr.SaveAllowanceCancellationMIG(uploadData, Naming.InvoiceProcessType.F0501);
                            result.Result.value = 1;
                        }
                        else
                        {
                            result.Result.message = "會員憑證資料驗證不符!!";
                        }
                    }
                }
                else
                {
                    result.Result.message = "發票資料簽章不符!!";
                }
                //EIVONotificationFactory.Notify();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.Result.message = ex.Message;
            }
            return result.ConvertToXml();
        }


        //[OperationContract]
        //public virtual XmlDocument UploadB1101(XmlDocument uploadData)
        //{
        //    Root result = createMessageToken();

        //    try
        //    {
        //        CryptoUtility crypto = new CryptoUtility();
        //        uploadData.PreserveWhitespace = true;
        //if (crypto.VerifyXmlSignature(uploadData))
        //        {
        //            ModelCore.Schema.TurnKey.B1101.Allowance allowance = uploadData.TrimAll().ConvertTo<ModelCore.Schema.TurnKey.B1101.Allowance>();
        //            using (B2BInvoiceManager mgr = new B2BInvoiceManager())
        //            {
        //                ///憑證資料檢查
        //                ///
        //                var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
        //                if (token != null)
        //                {
        //                    mgr.SaveB1101(allowance, token);
        //                    result.Result.value = 1;
        //                }
        //                else
        //                {
        //                    result.Result.message = "會員憑證資料驗證不符!!";
        //                }
        //            }
        //        }
        //        else
        //        {
        //            result.Result.message = "發票資料簽章不符!!";
        //        }

        //        EIVOPlatformFactory.Notify();
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error(ex);
        //        result.Result.message = ex.Message;
        //    }
        //    return result.ConvertToXml();
        //}

        [OperationContract]
        public virtual XmlDocument UploadA0201(XmlDocument uploadData)
        {
            Root result = createMessageToken();

            try
            {
                CryptoUtility crypto = new CryptoUtility();
                uploadData.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(uploadData))
                {
                    using (B2BInvoiceManager mgr = new B2BInvoiceManager { ProcessType = Naming.InvoiceProcessType.A0201 })
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {

                            mgr.SaveA0201(uploadData);
                            result.Result.value = 1;
                        }
                        else
                        {
                            result.Result.message = "營業人憑證資料驗證不符!!";
                        }
                    }
                }
                else
                {
                    result.Result.message = "發票資料簽章不符!!";
                }

                //EIVONotificationFactory.Notify();

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.Result.message = ex.Message;
            }
            return result.ConvertToXml();
        }

        [OperationContract]
        public virtual XmlDocument UploadB0201(XmlDocument uploadData)
        {
            Root result = createMessageToken();

            try
            {
                CryptoUtility crypto = new CryptoUtility();
                uploadData.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(uploadData))
                {
                    using (B2BInvoiceManager mgr = new B2BInvoiceManager { ProcessType = Naming.InvoiceProcessType.B0201 })
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            mgr.SaveB0201(uploadData);
                            result.Result.value = 1;
                        }
                        else
                        {
                            result.Result.message = "會員憑證資料驗證不符!!";
                        }
                    }
                }
                else
                {
                    result.Result.message = "發票資料簽章不符!!";
                }

                //EIVONotificationFactory.Notify();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.Result.message = ex.Message;
            }
            return result.ConvertToXml();
        }


        [OperationContract]
        public XmlDocument B2CUploadInvoice(XmlDocument uploadData)
        {
            Root result = createMessageToken();

            try
            {
                CryptoUtility crypto = new CryptoUtility();
                uploadData.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(uploadData))
                {
                    InvoiceRoot invoice = uploadData.TrimAll().ConvertTo<InvoiceRoot>();
                    using (B2CInvoiceManager mgr = new B2CInvoiceManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            var items = mgr.SaveUploadInvoice(invoice, token);
                            if (items.Count > 0)
                            {
                                result.Response = new RootResponse
                                {
                                    InvoiceNo =
                                    items.Select(d => new RootResponseInvoiceNo
                                    {
                                        Value = invoice.Invoice[d.Key].InvoiceNumber,
                                        Description = d.Value.Message,
                                        ItemIndexSpecified = true,
                                        ItemIndex = d.Key
                                    }).ToArray()
                                };

                                ThreadPool.QueueUserWorkItem(ExceptionNotification.SendNotification,
                                    new ExceptionInfo
                                    {
                                        Token = token,
                                        ExceptionItems = items,
                                        InvoiceData = invoice
                                    });
                            }
                            else
                            {
                                result.Result.value = 1;
                            }
                        }
                        else
                        {
                            result.Result.message = "營業人憑證資料驗證不符!!";
                        }
                    }
                }
                else
                {
                    result.Result.message = "發票資料簽章不符!!";
                }


            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.Result.message = ex.Message;
            }
            return result.ConvertToXml();
        }
        

        [OperationContract]
        public String[] ReceiveContentAsPDFForSeller(XmlDocument sellerInfo, String clientID)
        {
            _dataScope = Naming.InvoiceDataScope.SellerOnly;
            return ReceiveContentAsPDF(sellerInfo, clientID);
        }

        [OperationContract]
        public String[] ReceiveContentAsPDFForIssuer(XmlDocument sellerInfo, String clientID)
        {
            _dataScope = Naming.InvoiceDataScope.IssuerOnly;
            return ReceiveContentAsPDF(sellerInfo, clientID);
        }

        [OperationContract]
        public String[] ReceiveContentAsPDF(XmlDocument sellerInfo, String clientID)
        {
            try
            {
                CryptoUtility crypto = new CryptoUtility();
                sellerInfo.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(sellerInfo))
                {
                    using (InvoiceManager mgr = new InvoiceManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)//&& token.Organization.OrganizationStatus.EntrustToPrint == true
                        {
                            Root root = sellerInfo.ConvertTo<Root>();
                            acknowledgeReport(mgr, token, root.Request.periodicalIntervalSpecified ? root.Request.periodicalInterval : (int?)null);


                            List<String> pdfFiles = new List<String>();
                            pdfFiles.Add($"{ModelExtension.Properties.AppSettings.Default.HostUrl}{VirtualPathUtility.ToAbsolute("~/DataView/GetCustomerInvoicePDF")}");

                            IQueryable<CDS_Document> docItems = mgr.GetTable<CDS_Document>();
                            clientID = clientID.GetEfficientString();
                            if (clientID != null)
                            {
                                docItems = docItems.Join(mgr.GetTable<DocumentOwner>().Where(o => o.ClientID == clientID), d => d.DocID, o => o.DocID, (d, o) => d);
                            }

                            if (root.Request.channelIDSpecified)
                            {
                                docItems = docItems.Where(d => d.ChannelID == root.Request.channelID);
                            }

                            var items = mgr.GetTable<DocumentSubscriptionQueue>()
                                .Join(docItems, s => s.DocID, d => d.DocID, (s, d) => d)
                                //.Where(d => d.DocumentOwner.ClientID == clientID)
                                .Join(mgr.GetTable<InvoiceItem>(), d => d.DocID, i => i.InvoiceID, (d, i) => i);
                            IQueryable<InvoiceItem> queryItems;
                            if (_dataScope == Naming.InvoiceDataScope.SellerOnly)
                            {
                                queryItems = items.Where(i => i.SellerID == token.CompanyID);
                            }
                            else if (_dataScope == Naming.InvoiceDataScope.IssuerOnly)
                            {
                                queryItems = mgr.GetInvoiceByAgent(items, token.CompanyID, true);
                            }
                            else
                            {
                                queryItems = mgr.GetInvoiceByAgent(items, token.CompanyID);
                            }

                            //if (root.Request != null && root.Request.processIndexSpecified && root.Request.totalProcessCountSpecified
                            //    && root.Request.processIndex >= 0 && root.Request.totalProcessCount > 0)
                            //{
                            //    var mode = Math.Max(DateTime.Now.Ticks % 1048576, 1);
                            //    queryItems = queryItems.Where(i => (i.InvoiceID % root.Request.totalProcessCount) == root.Request.processIndex)
                            //                    .OrderBy(i => i.InvoiceID % mode);
                            //}

                            int resultCount = 0;
                            //while (resultCount < Settings.Default.MaxResponseCountPerBatch)
                            lock (typeof(eInvoiceService))
                            {
                                var resultItems = queryItems.Take(ModelExtension.Properties.AppSettings.Default.MaxResponseCountPerBatch).ToList();
                                //if (resultItems.Count() < 1)
                                //{
                                //    break;
                                //}
                                if (resultItems.Count > 0)
                                {
                                    foreach (var item in resultItems)
                                    {
                                        if (mgr.ExecuteCommand("delete DocumentSubscriptionQueue where DocID = {0}", item.InvoiceID) > 0)
                                        {
                                            pdfFiles.Add(
                                                String.Join("\t"
                                                    , $"{item.TrackCode}{item.No}"
                                                    , item.InvoicePurchaseOrder?.OrderNo
                                                    , HttpUtility.UrlEncode(item.InvoiceID.EncryptKey())));

                                            mgr.ExecuteCommand(@"INSERT INTO [proc].DataProcessLog
                                                            (DocID, LogDate, Status, StepID)
                                                            VALUES          ({0},{1},{2},{3})",
                                                    item.InvoiceID, DateTime.Now, (int)Naming.DataProcessStatus.Done,
                                                    (int)Naming.InvoiceStepDefinition.PDF待傳輸);

                                            resultCount++;
                                        }
                                        else
                                        {
                                            Logger.Warn($"DocumentSubscriptionQueue delete return 0 : {item.InvoiceID}");
                                        }
                                    }
                                }
                            }
                            return pdfFiles.ToArray();

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return null;
        }

        [OperationContract]
        public String[] ReceiveAllowancePDF(XmlDocument sellerInfo, String clientID)
        {
            try
            {
                CryptoUtility crypto = new CryptoUtility();
                sellerInfo.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(sellerInfo))
                {
                    using (InvoiceManager mgr = new InvoiceManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)//&& token.Organization.OrganizationStatus.EntrustToPrint == true
                        {
                            Root root = sellerInfo.ConvertTo<Root>();
                            acknowledgeReport(mgr, token, root.Request.periodicalIntervalSpecified ? root.Request.periodicalInterval : (int?)null);

                            List<String> pdfFiles = new List<String>
                            {
                                $"{ModelExtension.Properties.AppSettings.Default.HostUrl}{VirtualPathUtility.ToAbsolute("~/DataView/GetCustomerAllowancePDF")}"
                            };

                            IQueryable<CDS_Document> docItems = mgr.GetTable<CDS_Document>();
                            clientID = clientID.GetEfficientString();
                            if (clientID != null)
                            {
                                docItems = docItems.Join(mgr.GetTable<DocumentOwner>().Where(o => o.ClientID == clientID), d => d.DocID, o => o.DocID, (d, o) => d);
                            }

                            DateTime executionTime = DateTime.Now;

                            var items = mgr.GetTable<DocumentSubscriptionQueue>()
                                .Where(d => !d.WaitUntil.HasValue || d.WaitUntil <= executionTime)
                                .Join(docItems, s => s.DocID, d => d.DocID, (s, d) => d)
                                //.Where(d => d.DocumentOwner.ClientID == clientID)
                                .Join(mgr.GetTable<InvoiceAllowance>(), d => d.DocID, i => i.AllowanceID, (d, i) => i);
                            var queryItems = mgr.GetAllowanceByAgent(items, token.CompanyID);

                            //if (root.Request != null && root.Request.processIndexSpecified && root.Request.totalProcessCountSpecified
                            //    && root.Request.processIndex >= 0 && root.Request.totalProcessCount > 0)
                            //{
                            //    var mode = Math.Max(DateTime.Now.Ticks % 1048576, 1);
                            //    queryItems = queryItems.Where(i => (i.AllowanceID % root.Request.totalProcessCount) == root.Request.processIndex)
                            //                    .OrderBy(i => i.AllowanceID % mode);
                            //}

                            executionTime = executionTime.AddMinutes(15);

                            lock (typeof(eInvoiceService))
                            {
                                foreach (var item in queryItems.Take(ModelExtension.Properties.AppSettings.Default.MaxResponseCountPerBatch))
                                {
                                    if (mgr.ExecuteCommand("update DocumentSubscriptionQueue set WaitUntil = {1} where DocID = {0}", item.AllowanceID, executionTime) > 0)
                                    {
                                        pdfFiles.Add(
                                            String.Join("\t"
                                                , $"{item.AllowanceNumber}"
                                                , HttpUtility.UrlEncode(item.AllowanceID.EncryptKey())));

                                        //mgr.ExecuteCommand(@"INSERT INTO [proc].DataProcessLog
                                        //                    (DocID, LogDate, Status, StepID)
                                        //                    VALUES          ({0},{1},{2},{3})",
                                        //        item.AllowanceID, DateTime.Now, (int)Naming.DataProcessStatus.Done,
                                        //        (int)Naming.InvoiceStepDefinition.PDF待傳輸);
                                    }
                                    else
                                    {
                                        Logger.Warn($"DocumentSubscriptionQueue update return 0 : {item.AllowanceID}");
                                    }
                                }
                            }

                            return pdfFiles.ToArray();

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return null;
        }

        [OperationContract]
        public bool DeleteTempForReceivePDF(XmlDocument sellerInfo, String PdfFile)
        {
            try
            {
                CryptoUtility crypto = new CryptoUtility();
                sellerInfo.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(sellerInfo))
                {
                    using (InvoiceManager mgr = new InvoiceManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)//&& token.Organization.OrganizationStatus.EntrustToPrint == true
                        {
                            Root root = sellerInfo.ConvertTo<Root>();
                            acknowledgeReport(mgr, token, root.Request.periodicalIntervalSpecified ? root.Request.periodicalInterval : (int?)null);
                            CipherDecipherSrv cipher = new CipherDecipherSrv();
                            var invoiceNo = cipher.decipher(PdfFile.Split('?')[1]);
                            var item = mgr.GetTable<InvoiceItem>().Where(i => i.TrackCode == invoiceNo.Substring(0, 2) && i.No == invoiceNo.Substring(2, 8)).FirstOrDefault();
                            if (item != null)
                            {
                                var orgUser = mgr.GetTable<UserProfile>().Where(u => u.PID == token.Organization.ReceiptNo).FirstOrDefault();
                                if (orgUser != null)
                                {
                                    mgr.GetTable<DocumentDownloadLog>().InsertOnSubmit(new DocumentDownloadLog
                                    {
                                        DocID = item.InvoiceID,
                                        TypeID = item.CDS_Document.DocType!.Value,
                                        DownloadDate = DateTime.Now,
                                        UID = orgUser.UID
                                    });

                                    mgr.DeleteAnyOnSubmit<DocumentSubscriptionQueue>(d => d.DocID == item.InvoiceID);

                                    mgr.SubmitChanges();
                                }
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return true;
        }
        [OperationContract]
        public virtual String[] GetInvoiceMailTracking(XmlDocument sellerInfo, String clientID)
        {
            List<string> result = new List<string>();
            try
            {
                CryptoUtility crypto = new CryptoUtility();
                sellerInfo.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(sellerInfo))
                {
                    using (InvoiceManager mgr = new InvoiceManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            var items = mgr.GetTable<InvoiceItem>()
                                .Where(i => i.SellerID == token.CompanyID)
                                .Join(mgr.GetTable<CDS_Document>().Where(d => d.DocumentOwner.ClientID == clientID),
                                    i => i.InvoiceID, d => d.DocID, (i, d) => i)
                                .Join(mgr.GetTable<InvoiceDeliveryTracking>()
                                    .Where(t => t.DeliveryStatus == (int)Naming.InvoiceDeliveryStatus.待傳送),
                                i => i.InvoiceID, t => t.InvoiceID, (i, t) => t);
                            if (items.Count() > 0)
                            {
                                StringBuilder sb = new StringBuilder();
                                foreach (var item in items)
                                {
                                    sb.Append(string.Format("{0:yyyy/MM/dd}", item.DeliveryDate));//寄送日期
                                    sb.Append("|");
                                    sb.Append(item.InvoiceItem.InvoiceBuyer.CustomerID);//GoogleId
                                    sb.Append("|");
                                    sb.Append(item.InvoiceItem.TrackCode + item.InvoiceItem.No);//發票號碼
                                    sb.Append("|");
                                    sb.Append(item.TrackingNo1).Append(item.TrackingNo2);//掛號號碼
                                    sb.Append("|");
                                    sb.Append(item.InvoiceItem.InvoiceBuyer.ContactName);//收件人
                                    sb.Append("|");
                                    sb.Append(item.InvoiceItem.InvoiceBuyer.Address);//收件人地址
                                    result.Add(sb.ToString());
                                    item.DeliveryStatus = (int)Naming.InvoiceDeliveryStatus.已傳送;
                                    sb.Clear();
                                }
                                mgr.SubmitChanges();
                                return result.ToArray();
                            }
                        }

                    }
                }



            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return null;
        }
        [OperationContract]
        public virtual String[] GetInvoiceReturnedMail(XmlDocument sellerInfo, String clientID)
        {
            List<string> result = new List<string>();
            try
            {
                CryptoUtility crypto = new CryptoUtility();
                sellerInfo.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(sellerInfo))
                {
                    using (InvoiceManager mgr = new InvoiceManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            var items = mgr.GetTable<InvoiceItem>()
                                .Where(i => i.SellerID == token.CompanyID)
                                .Join(mgr.GetTable<CDS_Document>().Where(d => d.DocumentOwner.ClientID == clientID),
                                    i => i.InvoiceID, d => d.DocID, (i, d) => i)
                                .Join(mgr.GetTable<InvoiceDeliveryTracking>()
                                    .Where(t => t.DeliveryStatus == (int)Naming.InvoiceDeliveryStatus.申請退回),
                                i => i.InvoiceID, t => t.InvoiceID, (i, t) => t);

                            if (items.Count() > 0)
                            {
                                StringBuilder sb = new StringBuilder();
                                foreach (var item in items)
                                {
                                    sb.Append(string.Format("{0:yyyy/MM/dd}", item.DeliveryDate));//重寄日期
                                    sb.Append("|");
                                    sb.Append(item.InvoiceItem.InvoiceBuyer.CustomerID);//GoogleId
                                    sb.Append("|");
                                    sb.Append(item.InvoiceItem.TrackCode + item.InvoiceItem.No);//發票號碼
                                    sb.Append("|");
                                    sb.Append(item.TrackingNo1).Append(item.TrackingNo2);//掛號號碼
                                    sb.Append("|");
                                    sb.Append(item.InvoiceItem.InvoiceBuyer.ContactName);//收件人
                                    sb.Append("|");
                                    sb.Append(item.InvoiceItem.InvoiceBuyer.Address);//收件人地址
                                    sb.Append("|");
                                    sb.Append("");//備註
                                    result.Add(sb.ToString());
                                    item.DeliveryStatus = (int)Naming.InvoiceDeliveryStatus.已傳送;
                                    sb.Clear();
                                }
                                mgr.SubmitChanges();
                                return result.ToArray();
                            }
                        }

                    }
                }



            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return null;
        }

        [OperationContract]
        public virtual void AlertFailedTransaction(XmlDocument sellerInfo)
        {
            try
            {
                CryptoUtility crypto = new CryptoUtility();
                sellerInfo.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(sellerInfo))
                {
                    using (InvoiceManager mgr = new InvoiceManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            Root root = sellerInfo.ConvertTo<Root>();
                            String message = "營業人(" + token.Organization.CompanyName + ") G/W 資料傳送失敗資料夾尚有資料未處理如下:\r\n" +
                                            root.Request.actionName;
                            ExceptionNotification.SendExceptionNotificationToSysAdmin(new Exception(message));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        [OperationContract()]
        public XmlDocument B2BUploadReceipt(System.Xml.XmlDocument uploadData)
        {
            Root result = createMessageToken();

            try
            {
                CryptoUtility crypto = new CryptoUtility();
                uploadData.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(uploadData))
                {
                    ModelCore.Schema.EIVO.B2B.ReceiptRoot receiptAll = uploadData.ConvertTo<ModelCore.Schema.EIVO.B2B.ReceiptRoot>();
                    using (ReceiptManager mgr = new ReceiptManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            mgr.PrepareSignerCertificate(token.Organization);

                            var items = mgr.SaveUploadReceipt(receiptAll, token);
                            if (items.Count > 0)
                            {
                                result.Response = new RootResponse
                                {
                                    InvoiceNo =
                                    items.Select(d => new RootResponseInvoiceNo
                                    {
                                        Value = receiptAll.Receipt[d.Key].ReceiptNumber,
                                        Description = d.Value.Message,
                                        ItemIndexSpecified = true,
                                        ItemIndex = d.Key
                                    }).ToArray()
                                };

                                ThreadPool.QueueUserWorkItem(ExceptionNotification.SendNotification,
                                    new ExceptionInfo
                                    {
                                        Token = token,
                                        ExceptionItems = items,
                                        ReceiptData = receiptAll
                                    });
                            }
                            else
                            {
                                result.Result.value = 1;
                            }
                        }
                        else
                        {
                            result.Result.message = "會員憑證資料驗證不符!!";
                        }
                    }
                }
                else
                {
                    result.Result.message = "資料簽章不符!!";
                }

                //
                //EIVOPlatformFactory.Notify();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.Result.message = ex.Message;
            }
            return result.ConvertToXml();
        }

        [OperationContract()]
        public XmlDocument B2BUploadReceiptCancellation(XmlDocument uploadData)
        {
            Root result = createMessageToken();

            try
            {
                CryptoUtility crypto = new CryptoUtility();
                uploadData.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(uploadData))
                {
                    ModelCore.Schema.EIVO.B2B.CancelReceiptRoot item = uploadData.ConvertTo<ModelCore.Schema.EIVO.B2B.CancelReceiptRoot>();
                    using (ReceiptManager mgr = new ReceiptManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {

                            var items = mgr.SaveUploadReceiptCancellation(item, token);
                            if (items.Count > 0)
                            {
                                result.Response = new RootResponse
                                {
                                    InvoiceNo =
                                    items.Select(d => new RootResponseInvoiceNo
                                    {
                                        Value = item.CancelReceipt[d.Key].CancelReceiptNumber,
                                        Description = d.Value.Message,
                                        ItemIndexSpecified = true,
                                        ItemIndex = d.Key
                                    }).ToArray()
                                };

                                ThreadPool.QueueUserWorkItem(ExceptionNotification.SendNotification,
                                                                    new ExceptionInfo
                                                                    {
                                                                        Token = token,
                                                                        ExceptionItems = items,
                                                                        CancelReceiptData = item
                                                                    });

                            }
                            else
                            {
                                result.Result.value = 1;
                            }
                        }
                        else
                        {
                            result.Result.message = "會員憑證資料驗證不符!!";
                        }
                    }
                }
                else
                {
                    result.Result.message = "收據資料簽章不符!!";
                }

                //
                //EIVOPlatformFactory.Notify();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.Result.message = ex.Message;
            }
            return result.ConvertToXml();
        }

        [OperationContract()]
        public XmlDocument B2BUploadAllowance(XmlDocument uploadData)
        {
            Root result = createMessageToken();

            try
            {
                CryptoUtility crypto = new CryptoUtility();
                uploadData.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(uploadData))
                {
                    AllowanceRoot allowance = uploadData.ConvertTo<AllowanceRoot>();
                    using (B2BInvoiceManager mgr = new B2BInvoiceManager { ProcessType = Naming.InvoiceProcessType.B0401 })
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            var items = mgr.SaveUploadAllowance(allowance, token);
                            if (items.Count > 0)
                            {
                                result.Response = new RootResponse
                                {
                                    InvoiceNo =
                                    items.Select(d => new RootResponseInvoiceNo
                                    {
                                        Value = allowance.Allowance[d.Key].AllowanceNumber,
                                        ItemIndexSpecified = true,
                                        ItemIndex = d.Key,
                                        Description = d.Value.Message
                                    }).ToArray()
                                };

                                ThreadPool.QueueUserWorkItem(ExceptionNotification.SendNotification,
                                    new ExceptionInfo
                                    {
                                        Token = token,
                                        ExceptionItems = items,
                                        AllowanceData = allowance
                                    });
                            }
                            else
                            {
                                result.Result.value = 1;
                            }
                        }
                        else
                        {
                            result.Result.message = "會員憑證資料驗證不符!!";
                        }
                    }
                }
                else
                {
                    result.Result.message = "發票資料簽章不符!!";
                }

                //EIVOPlatformFactory.Notify();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.Result.message = ex.Message;
            }
            return result.ConvertToXml();
        }

        [OperationContract]
        public XmlDocument B2BUploadAllowanceCancellation(XmlDocument uploadData)
        {
            Root result = createMessageToken();

            try
            {
                CryptoUtility crypto = new CryptoUtility();
                uploadData.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(uploadData))
                {
                    CancelAllowanceRoot item = uploadData.ConvertTo<CancelAllowanceRoot>();
                    using (B2BInvoiceManager mgr = new B2BInvoiceManager { ProcessType = Naming.InvoiceProcessType.B0501 })
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {

                            var items = mgr.SaveUploadAllowanceCancellation(item, token);
                            if (items.Count > 0)
                            {
                                result.Response = new RootResponse
                                {
                                    InvoiceNo =
                                    items.Select(d => new RootResponseInvoiceNo
                                    {
                                        Value = item.CancelAllowance[d.Key].CancelAllowanceNumber,
                                        ItemIndexSpecified = true,
                                        ItemIndex = d.Key,
                                        Description = d.Value.Message
                                    }).ToArray()
                                };

                                ThreadPool.QueueUserWorkItem(ExceptionNotification.SendNotification,
                                                                    new ExceptionInfo
                                                                    {
                                                                        Token = token,
                                                                        ExceptionItems = items,
                                                                        CancelAllowanceData = item
                                                                    });

                            }
                            else
                            {
                                result.Result.value = 1;
                            }
                        }
                        else
                        {
                            result.Result.message = "會員憑證資料驗證不符!!";
                        }
                    }
                }
                else
                {
                    result.Result.message = "發票資料簽章不符!!";
                }

                //EIVOPlatformFactory.Notify();

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.Result.message = ex.Message;
            }
            return result.ConvertToXml();
        }

        [OperationContract()]
        public XmlDocument B2BUploadInvoiceCancellation(XmlDocument uploadData)
        {
            Root result = createMessageToken();

            try
            {
                CryptoUtility crypto = new CryptoUtility();
                uploadData.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(uploadData))
                {
                    CancelInvoiceRoot item = uploadData.ConvertTo<CancelInvoiceRoot>();
                    using (B2BInvoiceManager mgr = new B2BInvoiceManager { ProcessType = Naming.InvoiceProcessType.A0501 })
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            mgr.PrepareSignerCertificate(token.Organization);

                            var items = mgr.SaveUploadInvoiceCancellation(item, token);
                            if (items.Count > 0)
                            {
                                result.Response = new RootResponse
                                {
                                    InvoiceNo =
                                    items.Select(d => new RootResponseInvoiceNo
                                    {
                                        Value = item.CancelInvoice[d.Key].CancelInvoiceNumber,
                                        Description = d.Value.Message,
                                        ItemIndexSpecified = true,
                                        ItemIndex = d.Key
                                    }).ToArray()
                                };

                                ThreadPool.QueueUserWorkItem(ExceptionNotification.SendNotification,
                                                                    new ExceptionInfo
                                                                    {
                                                                        Token = token,
                                                                        ExceptionItems = items,
                                                                        CancelInvoiceData = item
                                                                    });

                            }
                            else
                            {
                                result.Result.value = 1;
                            }
                        }
                        else
                        {
                            result.Result.message = "會員憑證資料驗證不符!!";
                        }
                    }
                }
                else
                {
                    result.Result.message = "發票資料簽章不符!!";
                }

                //
                //EIVOPlatformFactory.Notify();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.Result.message = ex.Message;
            }
            return result.ConvertToXml();
        }

        [OperationContract]
        public XmlDocument UploadBranchTrackBlank(XmlDocument uploadData)
        {
            ModelCore.Schema.EIVO.RootBranchTrackBlank result = new ModelCore.Schema.EIVO.RootBranchTrackBlank
            {
                UXB2B = "電子發票系統",
                Result = new RootResult
                {
                    timeStamp = DateTime.Now,
                    value = 0
                }
            };
            try
            {
                CryptoUtility crypto = new CryptoUtility();
                uploadData.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(uploadData))
                {
                    //InvoiceNoRequestRoot invoiceNoRequest = uploadData.ConvertTo<InvoiceNoRequestRoot>();
                    ModelCore.Schema.TurnKey.E0402.BranchTrackBlank item = uploadData.ConvertTo<ModelCore.Schema.TurnKey.E0402.BranchTrackBlank>();
                    using (B2BInvoiceManager mgr = new B2BInvoiceManager { ProcessType = Naming.InvoiceProcessType.E0401 })
                    {
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            var ex = mgr.CheckBranchTrackBlank(item);
                            if (ex != null)
                            {
                                result.Result.message = ex.Message;

                                Dictionary<int, Exception> errorResult = new Dictionary<int, Exception>();
                                errorResult.Add(0, ex);
                                ThreadPool.QueueUserWorkItem(ExceptionNotification.SendNotification,
                                new ExceptionInfo
                                {
                                    Token = token,
                                    BranchTrackBlankError = item,
                                    ExceptionItems = errorResult,
                                });
                            }
                            else
                            {
                                //var response = new ModelCore.Schema.EIVO.RootResponseForBranchTrackBlank();

                                var TurnkeyResult = new ModelCore.Schema.TurnKey.E0402.BranchTrackBlank
                                {
                                    Main = new ModelCore.Schema.TurnKey.E0402.Main
                                    {
                                        HeadBan = item.Main.HeadBan,
                                        BranchBan = item.Main.BranchBan,
                                        InvoiceType = (ModelCore.Schema.TurnKey.E0402.InvoiceTypeEnum)item.Main.InvoiceType,
                                        YearMonth = item.Main.YearMonth,
                                        InvoiceTrack = item.Main.InvoiceTrack
                                    },
                                    Details = buildE0402Details(item)
                                };
                                result.Response = new ModelCore.Schema.EIVO.RootResponseForBranchTrackBlank()
                                {
                                    TrackBlank = item
                                };

                                E0402Handler.WriteToTurnkey(TurnkeyResult);
                                result.Result.value = 1;
                            }
                        }
                        else
                        {
                            result.Result.message = "終端設備使用者憑證資料驗證不符!!";
                        }
                    }
                }
                else
                {
                    result.Result.message = "簽章不符!!";
                }

                //GovPlatformFactoryForB2C.Notify();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.Result.message = ex.Message;
            }
            return result.ConvertToXml();
        }
        private static ModelCore.Schema.TurnKey.E0402.DetailsBranchTrackBlankItem[] buildE0402Details(ModelCore.Schema.TurnKey.E0402.BranchTrackBlank item)
        {
            List<ModelCore.Schema.TurnKey.E0402.DetailsBranchTrackBlankItem> items = new List<ModelCore.Schema.TurnKey.E0402.DetailsBranchTrackBlankItem>();

            foreach (var detail in item.Details.ToList())
            {
                items.Add(new ModelCore.Schema.TurnKey.E0402.DetailsBranchTrackBlankItem
                {
                    InvoiceBeginNo = String.Format("{0:00000000}", detail.InvoiceBeginNo),
                    InvoiceEndNo = String.Format("{0:00000000}", detail.InvoiceEndNo)
                }
                );
            }
            return items.ToArray();
        }
        //[OperationContract]
        //public override XmlDocument GetIncomingInvoices(XmlDocument sellerInfo)
        //{
        //    RootA1401 result = new RootA1401
        //    {
        //        UXB2B = "電子發票系統－集團加值中心",
        //        Result = new RootResult
        //        {
        //            timeStamp = DateTime.Now,
        //            value = 0
        //        }
        //    };

        //    try
        //    {
        //        CryptoUtility crypto = new CryptoUtility();
        //        sellerInfo.PreserveWhitespace = true;
        //if (crypto.VerifyXmlSignature(sellerInfo))
        //        {
        //            using (InvoiceManager mgr = new InvoiceManager())
        //            {
        //                ///憑證資料檢查
        //                ///
        //                var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
        //                if (token != null)
        //                {
        //                    buildIncomingInvoices(result, mgr, token.CompanyID);
        //                    Root root = sellerInfo.ConvertTo<Root>();
        //                    acknowledgeReport(mgr, token, root.Request.periodicalIntervalSpecified ? root.Request.periodicalInterval : (int?)null);
        //                }
        //                else
        //                {
        //                    result.Result.message = "會員憑證資料驗證不符!!";
        //                }
        //            }
        //        }
        //        else
        //        {
        //            result.Result.message = "發票資料簽章不符!!";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error(ex);
        //        result.Result.message = ex.Message;
        //    }
        //    return result.ConvertToXml();
        //}

        //protected override void buildIncomingInvoices(Root result, InvoiceManager mgr, int companyID)
        //{
        //    var table = mgr.GetTable<DocumentDispatch>();
        //    var items = mgr.GetTable<CDS_Document>().Where(d => d.CurrentStep == (int)Naming.B2BInvoiceStepDefinition.待接收 && d.DocType == (int)Naming.DocumentTypeDefinition.E_Invoice)
        //            .Join(mgr.GetTable<InvoiceItem>().Where(i => i.InvoiceBuyer.BuyerID == companyID)
        //                , d => d.DocID, i => i.InvoiceID, (d, i) => d);


        //    if (items.Count() > 0)
        //    {
        //        result.Response = new RootResponseForA1401
        //        {
        //            Invoice =
        //                items.Select(d => d.InvoiceItem.CreateA1401()).ToArray(),
        //            DataNumber = items.Select(d => d.InvoiceItem.B2BBuyerInvoiceTag.DataNumber).ToArray()
        //        };

        //        result.Result.value = 1;
        //    }
        //}


        //[OperationContract]
        //public XmlDocument B2BReceiveA1401(XmlDocument sellerInfo)
        //{
        //    try
        //    {
        //        CryptoUtility crypto = new CryptoUtility();
        //        sellerInfo.PreserveWhitespace = true;
        //if (crypto.VerifyXmlSignature(sellerInfo))
        //        {
        //            using (InvoiceManager mgr = new InvoiceManager())
        //            {
        //                ///憑證資料檢查
        //                ///
        //                var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
        //                if (token != null)
        //                {
        //                    Root root = sellerInfo.ConvertTo<Root>();
        //                    acknowledgeReport(mgr, token, root.Request.periodicalIntervalSpecified ? root.Request.periodicalInterval : (int?)null);

        //                    var table = mgr.GetTable<DocumentDispatch>();
        //                    var items = mgr.GetTable<CDS_Document>().Where(d => d.CurrentStep == (int)Naming.B2BInvoiceStepDefinition.待接收 && d.DocType == (int)Naming.DocumentTypeDefinition.E_Invoice)
        //                            .Join(mgr.GetTable<InvoiceItem>().Where(i => i.InvoiceBuyer.BuyerID == token.CompanyID)
        //                                , d => d.DocID, i => i.InvoiceID, (d, i) => d);

        //                    if (items.Count() > 0)
        //                    {
        //                        var item = items.First();
        //                        var result = item.InvoiceItem.CreateA1401();
        //                        result.DataNumber = item.InvoiceItem.B2BBuyerInvoiceTag != null ? item.InvoiceItem.B2BBuyerInvoiceTag.DataNumber : null;
        //                        result.InvoiceID = item.DocID.ToString();

        //                        item.MoveToNextStep(mgr);

        //                        return result.ConvertToXml();
        //                    }

        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error(ex);
        //    }
        //    return null;
        //}

        [OperationContract()]
        public void AcknowledgeReceiving(XmlDocument uploadData)
        {
            try
            {
                CryptoUtility crypto = new CryptoUtility();
                PKCS7Log log = crypto.PKCS7Log as PKCS7Log;
                if (log != null)
                {
                    log.Crypto = crypto;
                    log.Catalog = Naming.CACatalogDefinition.UXGW自動接收;
                }
                crypto.VerifyXmlSignature(uploadData);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        [OperationContract()]
        public virtual void NotifyCounterpartBusiness(XmlDocument uploadData)
        {
            try
            {
                CryptoUtility crypto = new CryptoUtility();
                uploadData.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(uploadData))
                {
                    //EIVOPlatformManager mgr = new EIVOPlatformManager();
                    //mgr.NotifyToProcess();
                    //EIVONotificationFactory.Notify();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }


        //[OperationContract]
        //public XmlDocument B2BReceiveB1401(XmlDocument sellerInfo)
        //{
        //    try
        //    {
        //        CryptoUtility crypto = new CryptoUtility();
        //        sellerInfo.PreserveWhitespace = true;
        //if (crypto.VerifyXmlSignature(sellerInfo))
        //        {
        //            using (InvoiceManager mgr = new InvoiceManager())
        //            {
        //                ///憑證資料檢查
        //                ///
        //                var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
        //                if (token != null)
        //                {
        //                    Root root = sellerInfo.ConvertTo<Root>();
        //                    acknowledgeReport(mgr, token, root.Request.periodicalIntervalSpecified ? root.Request.periodicalInterval : (int?)null);

        //                    var table = mgr.GetTable<DocumentDispatch>();
        //                    var items = mgr.GetTable<CDS_Document>().Where(d => d.CurrentStep == (int)Naming.B2BInvoiceStepDefinition.待接收 && d.DocType == (int)Naming.DocumentTypeDefinition.E_Allowance)
        //                            .Join(mgr.GetTable<InvoiceAllowance>().Where(i => i.InvoiceAllowanceSeller.SellerID == token.CompanyID)
        //                                , d => d.DocID, i => i.AllowanceID, (d, i) => d);

        //                    if (items.Count() > 0)
        //                    {
        //                        var item = items.First();
        //                        var result = item.InvoiceAllowance.CreateB1401();

        //                        item.MoveToNextStep(mgr);

        //                        return result.ConvertToXml();
        //                    }

        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error(ex);
        //    }
        //    return null;
        //}

        [OperationContract]
        public XmlDocument? B2BReceiveA0501(XmlDocument sellerInfo)
        {
            try
            {
                CryptoUtility crypto = new CryptoUtility();
                sellerInfo.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(sellerInfo))
                {
                    using (InvoiceManager mgr = new InvoiceManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            Root root = sellerInfo.ConvertTo<Root>();
                            acknowledgeReport(mgr, token, root.Request.periodicalIntervalSpecified ? root.Request.periodicalInterval : (int?)null);

                            var table = mgr.GetTable<DocumentDispatch>();
                            var items = mgr.GetTable<CDS_Document>().Where(d => d.CurrentStep == (int)Naming.InvoiceStepDefinition.待接收 && d.DocType == (int)Naming.DocumentTypeDefinition.E_InvoiceCancellation)
                                    .Join(mgr.GetTable<DerivedDocument>()
                                        .Join(mgr.GetTable<InvoiceItem>().Where(i => i.InvoiceBuyer.BuyerID == token.CompanyID), d => d.SourceID, i => i.InvoiceID, (d, i) => d)
                                    , d => d.DocID, i => i.DocID, (d, i) => d);

                            if (items.Count() > 0)
                            {
                                var item = items.First();
                                var result = item.DerivedDocument.ParentDocument.InvoiceItem.CreateF0501();

                                item.MoveToNextStep(mgr);

                                return result;
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return null;
        }

        [OperationContract]
        public XmlDocument? B2BReceiveB0501(XmlDocument sellerInfo)
        {
            try
            {
                CryptoUtility crypto = new CryptoUtility();
                sellerInfo.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(sellerInfo))
                {
                    using (InvoiceManager mgr = new InvoiceManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            Root root = sellerInfo.ConvertTo<Root>();
                            acknowledgeReport(mgr, token, root.Request.periodicalIntervalSpecified ? root.Request.periodicalInterval : (int?)null);

                            var table = mgr.GetTable<DocumentDispatch>();
                            var items = mgr.GetTable<CDS_Document>().Where(d => d.CurrentStep == (int)Naming.InvoiceStepDefinition.待接收 && d.DocType == (int)Naming.DocumentTypeDefinition.E_AllowanceCancellation)
                                    .Join(mgr.GetTable<DerivedDocument>()
                                        .Join(mgr.GetTable<InvoiceAllowance>().Where(i => i.InvoiceAllowanceSeller.SellerID == token.CompanyID)
                                        , d => d.SourceID, i => i.AllowanceID, (d, i) => d)
                                    , d => d.DocID, i => i.DocID, (d, i) => d);

                            if (items.Count() > 0)
                            {
                                var item = items.First();
                                var result = item.DerivedDocument.ParentDocument.InvoiceAllowance.CreateG0501();

                                item.MoveToNextStep(mgr);

                                return result;
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return null;
        }

        [OperationContract]
        public String[] ReceiveContentAsPDFB2B(XmlDocument sellerInfo)
        {
            try
            {
                CryptoUtility crypto = new CryptoUtility();
                sellerInfo.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(sellerInfo))
                {
                    using (InvoiceManager mgr = new InvoiceManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)//&& token.Organization.OrganizationStatus.EntrustToPrint == true
                        {
                            Root root = sellerInfo.ConvertTo<Root>();
                            acknowledgeReport(mgr, token, root.Request.periodicalIntervalSpecified ? root.Request.periodicalInterval : (int?)null);

                            List<String> pdfFiles = new List<String>();
                            pdfFiles.Add(String.Format("{0}{1}",
                                    ModelExtension.Properties.AppSettings.Default.HostUrl,
                                    VirtualPathUtility.ToAbsolute("~/Published/GetInvoicePDF.ashx")));
                            CipherDecipherSrv cipher = new CipherDecipherSrv();

                            var items = mgr.GetTable<DocumentSubscriptionQueue>()
                                .Join(mgr.GetTable<CDS_Document>().Where(d => d.DocumentOwner.OwnerID == token.CompanyID),
                                    q => q.DocID, d => d.DocID, (q, d) => d)
                                .Select(d => d.InvoiceItem);

                            foreach (var item in items)
                            {
                                pdfFiles.Add(String.Join(",",
                                    item.InvoiceSeller.ReceiptNo,
                                    item.InvoiceDate.Value.Year.ToString(),
                                    string.Format("{0:MM}", item.InvoiceDate.Value),
                                    string.Format("{0:dd}", item.InvoiceDate.Value),
                                    item.TrackCode + item.No,
                                    item.InvoiceID.ToString(),
                                    item.CDS_Document.CustomerDefined != null ? item.CDS_Document.CustomerDefined.IsolationFolder : null
                                    ));

                            }
                            return pdfFiles.ToArray();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return null;
        }

        [OperationContract]
        public bool DeleteTempForReceivePDFB2B(XmlDocument sellerInfo, int docID)
        {
            try
            {
                CryptoUtility crypto = new CryptoUtility();
                sellerInfo.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(sellerInfo))
                {
                    using (InvoiceManager mgr = new InvoiceManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)//&& token.Organization.OrganizationStatus.EntrustToPrint == true
                        {
                            Root root = sellerInfo.ConvertTo<Root>();
                            acknowledgeReport(mgr, token, root.Request.periodicalIntervalSpecified ? root.Request.periodicalInterval : (int?)null);

                            var tableQ = mgr.GetTable<DocumentSubscriptionQueue>();
                            var item = tableQ.Where(d => d.DocID == docID && d.CDS_Document.DocumentOwner.OwnerID == token.CompanyID).FirstOrDefault();

                            if (item != null)
                            {

                                mgr.GetTable<DocumentDownloadLog>().InsertOnSubmit(new DocumentDownloadLog
                                {
                                    DocID = item.DocID,
                                    TypeID = (int)item.CDS_Document.DocType,
                                    DownloadDate = DateTime.Now,
                                    UID = token.Organization.OrganizationCategory.First().UserRole.First().UID
                                });

                                tableQ.DeleteOnSubmit(item);

                                mgr.SubmitChanges();

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return true;
        }

        //public static String CreatePdfFile(int InvoiceID)
        //{
        //    String File = string.Empty;
        //    using (WebClient client = new WebClient())
        //    {
        //        File = client.DownloadString(String.Format("{0}{1}?id={2}&nameOnly=true&printAll='0'",
        //                                ModelExtension.Properties.AppSettings.Default.HostUrl,
        //                                VirtualPathUtility.ToAbsolute("~/DataView/PrintSingleInvoiceAsPDF"),
        //                                InvoiceID));
        //    }
        //    return File;
        //}

        //public static void DocumentDownload(string pdfFile, InvoiceItem item)
        //{

        //    string TempForReceivePDF = Path.Combine(System.IO.Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory), "TempForReceivePDF");// Server.MapPath("~/TempForReceivePDF");
        //    if (!Directory.Exists(TempForReceivePDF))
        //        Directory.CreateDirectory(TempForReceivePDF);
        //    String dest = Path.Combine(TempForReceivePDF, string.Format("{0}_{1:yyyy_MM}_{2}{3}.pdf", item.InvoiceSeller.Organization.ReceiptNo, item.InvoiceDate, item.TrackCode, item.No));
        //    if (item.InvoiceBuyer.Organization.OrganizationStatus.EntrustToPrint == true
        //        && !File.Exists(dest))
        //    {
        //        File.Copy(pdfFile, dest);
        //    }
        //    Logger.Info(string.Format("系統寄送已自動接收發票資料:{0}{1} 至相對營業人 {2}(Source:{3}) ", item.TrackCode, item.No, item.InvoiceBuyer.ReceiptNo, pdfFile));

        //}

        [OperationContract()]
        public XmlDocument UploadCounterpartBusiness(System.Xml.XmlDocument uploadData)
        {
            Root result = createMessageToken();

            try
            {
                CryptoUtility crypto = new CryptoUtility();
                uploadData.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(uploadData))
                {
                    using (B2BInvoiceManager mgr = new B2BInvoiceManager { ProcessType = Naming.InvoiceProcessType.CounterpartBusiness })
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            BusinessCounterpartXmlUploadManager csvMgr = new BusinessCounterpartXmlUploadManager(mgr);
                            csvMgr.BusinessType = Naming.InvoiceCenterBusinessType.銷項;
                            csvMgr.MasterID = token.CompanyID;
                            if (token.Organization.IsEnterpriseGroupMember())
                            {
                                csvMgr.MasterGroup = token.Organization.EnterpriseGroupMember.First().EnterpriseGroup.EnterpriseGroupMember.Select(m => m.CompanyID).ToArray();
                            }
                            csvMgr.SaveData(uploadData);

                            if (csvMgr.ErrorList.Count > 0)
                            {
                                var items = csvMgr.ErrorList;
                                result.Response = new RootResponse
                                {
                                    InvoiceNo =
                                    items.Select(d => new RootResponseInvoiceNo
                                    {
                                        Value = d.DataContent,
                                        Description = d.Status,
                                        ItemIndexSpecified = true,
                                        ItemIndex = csvMgr.ItemList.IndexOf(d)
                                    }).ToArray()
                                };

                                ThreadPool.QueueUserWorkItem(ExceptionNotification.SendNotification,
                                    new ExceptionInfo
                                    {
                                        Token = token,
                                        CounterpartBusinessError = items
                                    });
                            }
                            else
                            {
                                result.Result.value = 1;
                            }
                        }
                        else
                        {
                            result.Result.message = "會員憑證資料驗證不符!!";
                        }
                    }
                }
                else
                {
                    result.Result.message = "發票資料簽章不符!!";
                }

                //
                //EIVOPlatformFactory.Notify();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.Result.message = ex.Message;
            }
            return result.ConvertToXml();
        }

        [OperationContract]
        public virtual String GetServiceInfo(XmlDocument sellerInfo)
        {
            try
            {
                CryptoUtility crypto = new CryptoUtility();
                sellerInfo.PreserveWhitespace = true;
                if (crypto.VerifyXmlSignature(sellerInfo))
                {
                    Root root = sellerInfo.ConvertTo<Root>();

                    using (InvoiceManager models = new InvoiceManager())
                    {
                        ///憑證資料檢查
                        ///
                        var token = models.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            var userItems = token.Organization.OrganizationCategory.SelectMany(c => c.UserRole).Where(r => r.RoleID == (int)Naming.EIVOMemberRoleID.會員);
                            if (root.Request?.processIndexSpecified == true)
                            {
                                userItems = userItems.Skip(root.Request.processIndex);
                            }
                            var user = userItems.FirstOrDefault();
                            ServiceInfo info = new ServiceInfo
                            {
                                AgentToken = token.CompanyID.EncryptKey(),
                                TaskCenterUrl = $"{ModelExtension.Properties.AppSettings.Default.TaskCenterUrl}",
                                ServiceHost = $"{ModelExtension.Properties.AppSettings.Default.HostUrl}{VirtualPathUtility.ToAbsolute("~")}",
                                AgentUID = user?.UID,
                                DefaultProcessType = (Naming.InvoiceProcessType?)token.Organization.OrganizationStatus.InvoiceClientDefaultProcessType,
                            };
                            return JsonConvert.SerializeObject(info);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return null;
        }

    }
}
