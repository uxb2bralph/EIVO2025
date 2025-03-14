using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Services;
using System.Xml;


using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;
using ModelCore.Schema.EIVO;
using ModelCore.Schema.TurnKey;
using ModelCore.Schema.TXN;
using System.IO;
using System.Data;
using ModelCore.Security.MembershipManagement;
using CoreWCF;
using CommonLib.Security.UseCrypto;
using WebHome.Helper;
using CommonLib.Utility;

using CommonLib.Core.Utility;

namespace WebHome.Published
{

    public partial class eInvoiceService
    {

        [OperationContract]
        public virtual XmlDocument UploadInvoiceBuyerCmsCSV(byte[] uploadData)
        {
            Root result = createMessageToken();

            try
            {
                CryptoUtility crypto = new CryptoUtility();
                byte[] dataToSign;
                if (crypto.VerifyEnvelopedPKCS7(uploadData, out dataToSign))
                {
                    String fileName = Path.Combine(Logger.LogDailyPath, String.Format("InvoiceBuyer_{0}.csv", Guid.NewGuid()));
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

                            BranchBusinessCounterpartUploadManager csvMgr = new BranchBusinessCounterpartUploadManager(mgr);
                            csvMgr.BusinessType = Naming.InvoiceCenterBusinessType.銷項;
                            csvMgr.MasterID = token.CompanyID;
                            Encoding encoding = dataToSign.IsUtf8(dataToSign.Length) ? Encoding.UTF8 : Encoding.GetEncoding(ModelExtension.Properties.AppSettings.Default.CsvUploadEncoding);
                            csvMgr.ParseData(UserProfileFactory.CreateInstance(ModelExtension.Properties.AppSettings.Default.SystemAdmin), fileName, encoding);

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
                                        BusinessCounterpartError = items
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
        public virtual XmlDocument UploadInvoiceCmsCSV(byte[] uploadData)
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

                    using (InvoiceManagerV3 mgr = new InvoiceManagerV3())
                    {
                        ///憑證資料檢查
                        ///
                        var token = mgr.GetTable<OrganizationToken>().Where(t => t.Thumbprint == crypto.SignerCertificate.Thumbprint).FirstOrDefault();
                        if (token != null)
                        {
                            CsvInvoiceUploadManagerV2 csvMgr = new CsvInvoiceUploadManagerV2(mgr, token.CompanyID);
                            csvMgr.DefaultInvoiceType = Naming.InvoiceTypeDefinition.一般稅額計算之電子發票;
                            csvMgr.AutoTrackNo = false;
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
                
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.Result.message = ex.Message;
            }
            return result.ConvertToXml();
        }

    }
}