using CommonLib.Core.Utility;
using CommonLib.DataAccess;
using CommonLib.Utility;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement.enUS;
using ModelCore.InvoiceManagement.ErrorHandle;
using ModelCore.InvoiceManagement.Validator;
using ModelCore.Locale;
using ModelCore.Properties;
using ModelCore.Schema.EIVO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace ModelCore.InvoiceManagement
{
    public partial class GoogleInvoiceManager : InvoiceManagerV2
    {
        public static string AttachmentPoolPath
        {
            get;
            private set;
        }

        static GoogleInvoiceManager()
        {
            AttachmentPoolPath = Path.Combine(Logger.LogPath, ModelExtension.Properties.AppSettings.Default.UploadedFilePath);

            if (!Directory.Exists(AttachmentPoolPath))
                Directory.CreateDirectory(AttachmentPoolPath);
        }


        public GoogleInvoiceManager()
            : base()
        {

        }
        public GoogleInvoiceManager(GenericManager<EIVOEntityDataContext> mgr)
            : base(mgr)
        {

        }

        public override Dictionary<int, Exception> SaveUploadInvoiceAutoTrackNo(InvoiceRoot item, OrganizationToken owner)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();

            if (item != null && item.Invoice != null && item.Invoice.Length > 0)
            {
                //Organization donatory = owner.Organization.InvoiceWelfareAgencies.Select(w => w.WelfareAgency.Organization).FirstOrDefault();
                EventItems = null;
                List<InvoiceItem> eventItems = new List<InvoiceItem>();
                bool forTerms = ChannelID.HasValue && ChannelID == (int)Naming.ChannelIDType.ForGoogleTerms;

                bool countInfo = item.Invoice.Length > 1000;
                if (countInfo)
                {
                    Console.WriteLine($"Large file process:{item.Invoice.Length}");
                }

                int count = 0, dbCheckCount = 180;
                InvoiceManager workingMgr = new InvoiceManager();
                GoogleInvoiceRootInvoiceValidator validator = new GoogleInvoiceRootInvoiceValidator(workingMgr, owner.Organization)
                {
                    UseDefaultCrossBorderMerchantCarrier = true,
                };
                validator.StartAutoTrackNo(ApplyInvoiceDate);
                for (int idx = 0; idx < item.Invoice.Length; idx++, count++)
                {
                    if (count == dbCheckCount)
                    {
                        count = 0;
                        validator.EndAutoTrackNo();
                        workingMgr.Dispose();
                        workingMgr = new InvoiceManager();
                        validator = new GoogleInvoiceRootInvoiceValidator(workingMgr, owner.Organization)
                        {
                            UseDefaultCrossBorderMerchantCarrier = true,
                        };
                        validator.StartAutoTrackNo(ApplyInvoiceDate);
                    }

                    try
                    {
                        var invItem = item.Invoice[idx];

                        InvoiceItem newItem = validator.SaveRootInvoice(invItem, forTerms, InvoiceClientID, ChannelID, out Exception ex);
                        if (countInfo)
                        {
                            Console.Write("+");
                        }

                        if (ex != null)
                        {
                            if (IgnoreDuplicateDataNumberException && (ex is DuplicateDataNumberException))
                            {
                                newItem = ((DuplicateDataNumberException)ex).CurrentPO.InvoiceItem;
                                if (newItem == null)
                                {
                                    result.Add(idx, ex);
                                    continue;
                                }
                            }
                            else
                            {
                                result.Add(idx, ex);
                                continue;
                            }
                        }

                        eventItems.Add(newItem);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        result.Add(idx, ex);
                    }
                }
                validator.EndAutoTrackNo();
                workingMgr.Dispose();

                if (eventItems.Count > 0)
                {
                    HasItem = true;
                    if (forTerms)
                    {
                        GoogleInvoiceExtensionMethods.MatchGoogleInvoiceAttachment();
                    }
                }
                EventItems = eventItems;
            }

            return result;
        }

        private void checkAttachmentFromPool(InvoicePurchaseOrder order)
        {
            //發票附件檢查

            #region 抓取暫存資料夾內檔案名稱

            var fileList = Directory.GetFiles(AttachmentPoolPath,String.Format("{0}*.*",order.OrderNo));
            if (fileList.Length > 0)
            {
                Dictionary<String, String> fileItems = new Dictionary<string, string>();

                //取得暫存資料夾底下檔案名稱
                foreach (var tempFile in fileList)
                {
                    String fileName = Path.GetFileName(tempFile);
                    String storedPath = Path.Combine(Logger.LogDailyPath, fileName);

                    fileItems.Add(tempFile, storedPath);
                    String keyName = Path.GetFileNameWithoutExtension(fileName);

                    order.InvoiceItem.CDS_Document
                        .Attachment.Add(new Attachment
                        {
                            KeyName = keyName,
                            StoredPath = storedPath
                        });
                }

                ThreadPool.QueueUserWorkItem(stateInfo =>
                    {
                        foreach (var item in fileItems)
                        {
                            if (File.Exists(item.Value))
                            {
                                File.Delete(item.Value);
                            }
                            File.Move(item.Key, item.Value);
                        }
                    });
            }
            #endregion
        }

    }
}
