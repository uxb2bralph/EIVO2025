using ModelCore.DataEntity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelCore.InvoiceManagement.Validator;
using CommonLib.Utility;
using System.Threading;
using ModelCore.Locale;
using ModelCore.Helper;
using CommonLib.DataAccess;
using CommonLib.Core.Utility;

namespace ModelCore.InvoiceManagement
{
    public class AttachmentManager : EIVOEntityManager<InvoiceItem>
    {
        public AttachmentManager() : base() { }
        public AttachmentManager(GenericManager<EIVOEntityDataContext> mgr) : base(mgr) { }

        private void AttachToDocument(String keyName, String storedPath, int docID)
        {
            var table = this.GetTable<Attachment>();
            var attachment = table.Where(a => a.KeyName == keyName).FirstOrDefault();
            if (attachment == null)
            {
                attachment = new Attachment
                {
                    DocID = docID,
                    KeyName = keyName,
                };

                table.InsertOnSubmit(attachment);
            }
            else if (attachment.DocID != docID)
            {
                return;
            }

            String targetPath = Path.Combine(Logger.LogDailyPath, Path.GetFileName(storedPath));
            if(File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }
            File.Move(storedPath, targetPath);

            attachment.StoredPath = targetPath;
            this.SubmitChanges();
        }

        public void CheckAttachmentFromPool(String poolPath)
        {
            IEnumerable<String> files = Directory.EnumerateFiles(poolPath);
            if (files.Count() > 0)
            {
                List<int> docID = new List<int>();
                foreach (var filePath in files)
                {
                    String fileName = Path.GetFileNameWithoutExtension(filePath);
                    String[] fileNames = fileName.Split('_');
                    var match = fileNames[0].ParseInvoiceNo();
                    if (match.Success)
                    {
                        var item = this.GetTable<InvoiceItem>()
                            .Where(i => i.TrackCode == match.Groups[1].Value && i.No == match.Groups[2].Value)
                            .FirstOrDefault();

                        if (item != null)
                        {
                            AttachToDocument(fileName, filePath, item.InvoiceID);
                            docID.Add(item.InvoiceID);
                        }
                    }
                    else
                    {
                        var allowance = this.GetTable<InvoiceAllowance>().Where(a => a.AllowanceNumber == fileNames[0]).FirstOrDefault();
                        if (allowance != null)
                        {
                            AttachToDocument(fileName, filePath, allowance.AllowanceID);
                        }
                    }
                }

                foreach(var id in docID)
                {
                    if (this.ExecuteCommand(@"
                                    UPDATE          [proc].C0401DispatchQueue
                                    SET                   StepID = {2}
                                    WHERE          DocID = {0} And StepID = {1}", id, (int)Naming.InvoiceStepDefinition.文件準備中, (int)Naming.InvoiceStepDefinition.已接收資料待通知) == 0)
                    {
                        this.ExecuteCommand(@"
                                    UPDATE          [proc].A0401DispatchQueue
                                    SET                   StepID = {2}
                                    WHERE          DocID = {0} And StepID = {1}", id, (int)Naming.InvoiceStepDefinition.文件準備中, (int)Naming.InvoiceStepDefinition.已接收資料待通知);
                    }
                }
            }
        }

        private static int __BusyCount;

        public static int MatchAttachment()
        {
            if (Interlocked.Increment(ref __BusyCount) == 1)
            {
                Task.Run(() => 
                {
                    try
                    {
                        (new AttachmentManager()).CheckAttachmentFromPool(ModelExtension.Properties.AppSettings.Default.AttachmentTempStore.PrefixStorePath());
                    }
                    catch(Exception ex)
                    {
                        Logger.Error(ex);
                    }

                    Interlocked.Exchange(ref __BusyCount, 0);
                });
            }

            return __BusyCount;
        }
    }
}
