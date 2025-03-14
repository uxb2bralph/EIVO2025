using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

using ModelCore.DataEntity;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;
using ModelCore.Schema.EIVO;
using CommonLib.Utility;
using CommonLib.Core.Utility;

namespace ModelCore.Helper
{
    public static class GoogleInvoiceExtensionMethods
    {
        private static int __MatchingBusyCount;

        public static void MatchGoogleInvoiceAttachment()
        {
            if (Interlocked.Increment(ref __MatchingBusyCount) == 1)
            {
                ThreadPool.QueueUserWorkItem(t =>
                {
                    try
                    {
                        using (var models = new ModelSource<InvoiceItem>())
                        {
                            do
                            {
                                var items = models.GetTable<CustomerDefined>()
                                    .Join(models.GetTable<CDS_Document>().Where(d => d.ChannelID == (int)Naming.ChannelIDType.ForGoogleTerms),
                                        c => c.DocID, d => d.DocID, (c, d) => d)
                                    .Select(d => d.InvoiceItem.InvoicePurchaseOrder);


                                foreach (var item in items)
                                {
                                    try
                                    {
                                        if (models.CheckAttachmentFromPool(item))
                                            EIVOTurnkeyFactory.NotifyIssuedInvoice(new NotifyToProcessID 
                                            {
                                                DocID = item.InvoiceID,
                                                AppendAttachment = true,
                                            });
                                    }
                                    catch(Exception ex)
                                    {
                                        Logger.Error(ex);
                                    }
                                }

                                models.ExecuteCommand(@"
                                        DELETE FROM CustomerDefined
                                        FROM              CustomerDefined INNER JOIN
                                                                    CDS_Document ON CustomerDefined.DocID = CDS_Document.DocID INNER JOIN
                                                                    Attachment ON CDS_Document.DocID = Attachment.DocID
                                        WHERE          (CDS_Document.ChannelID = {0})", (int)Naming.ChannelIDType.ForGoogleTerms);

                            } while (Interlocked.Decrement(ref __MatchingBusyCount) > 0);

                            MatchAttachmentPoolWithPurchaseOrder(models);

                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        Interlocked.Exchange(ref __MatchingBusyCount, 0);
                    }
                });
            }
        }

        public static bool CheckAttachmentFromPool<TEntity>(this ModelSource<TEntity> models, InvoicePurchaseOrder item)
            where TEntity : class, new()
        {
            //發票附件檢查
            #region 抓取暫存資料夾內檔案名稱

            var table = models.GetTable<Attachment>();
            var fileList = Directory.GetFiles(GoogleInvoiceManager.AttachmentPoolPath, String.Format("{0}*.*", item.OrderNo));
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

                    var attachment = table.Where(a => a.KeyName == keyName).FirstOrDefault();
                    if (attachment == null)
                    {
                        attachment = new Attachment
                        {
                            DocID = item.InvoiceID,
                            KeyName = keyName,
                        };
                        table.InsertOnSubmit(attachment);
                    }
                    else if (attachment.DocID != item.InvoiceID)
                    {
                        continue;
                    }

                    attachment.StoredPath = storedPath;
                    models.SubmitChanges();
                }


                foreach (var f in fileItems)
                {
                    if (File.Exists(f.Value))
                    {
                        File.Delete(f.Value);
                    }
                    File.Move(f.Key, f.Value);
                }

                return true;
            }

            return false;
            #endregion
        }


        public static void MatchAttachmentPoolWithPurchaseOrder<TEntity>(this ModelSource<TEntity> models)
            where TEntity : class, new()
        {
            var table = models.GetTable<InvoicePurchaseOrder>();
            var fileList = Directory.GetFiles(GoogleInvoiceManager.AttachmentPoolPath);
            if (fileList != null && fileList.Length > 0)
            {
                //取得暫存資料夾底下檔案名稱
                foreach (var tempFile in fileList)
                {
                    try
                    {
                        String fileName = Path.GetFileName(tempFile);
                        String keyName = Path.GetFileNameWithoutExtension(fileName);

                        var item = table.Where(p => p.OrderNo == keyName).FirstOrDefault();
                        if (item == null)
                            continue;

                        String storedPath = Path.Combine(Logger.LogDailyPath, fileName);

                        if (File.Exists(storedPath))
                        {
                            File.Delete(storedPath);
                        }
                        File.Move(tempFile, storedPath);

                        var attachment = models.GetTable<Attachment>().Where(a => a.DocID == item.InvoiceID && a.KeyName == keyName).FirstOrDefault();
                        if (attachment == null)
                        {
                            attachment = new Attachment
                            {
                                DocID = item.InvoiceID,
                                KeyName = keyName,
                            };

                            models.GetTable<Attachment>().InsertOnSubmit(attachment);
                        }

                        attachment.StoredPath = storedPath;
                        models.SubmitChanges();

                        EIVOTurnkeyFactory.NotifyIssuedInvoice(new NotifyToProcessID
                            {
                                DocID = item.InvoiceID,
                                AppendAttachment = true
                            }
                        );
                    }
                    catch(Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }
            }
        }

    }
}
