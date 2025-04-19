using CommonLib.Core.Utility;
using CommonLib.Utility;
using ModelCore.DataEntity;
using ModelCore.Locale;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using TestConsole.Properties;

namespace TestConsole.Helper
{
    class TurnkeyRecycler
    {
        [STAThread]
        static void Main(string[] args)
        {
            FileLogger.Logger.OutputWriter = Console.Out;
            (new TurnkeyRecycler()).StartUp();
        }

        public void StartUp()
        {
            using(ModelSource <UserProfile> models = new ModelSource<UserProfile>())
            {
                CheckInvoice(models, Naming.DataProcessStatus.Done);
                CheckInvoice(models, Naming.DataProcessStatus.Exception);
                CheckInvoiceCancellation(models, Naming.DataProcessStatus.Done);
                CheckInvoiceCancellation(models, Naming.DataProcessStatus.Exception);
                CheckAllowance(models, Naming.DataProcessStatus.Done);
                CheckAllowance(models, Naming.DataProcessStatus.Exception);
                CheckAllowanceCancellation(models, Naming.DataProcessStatus.Done);
                CheckAllowanceCancellation(models, Naming.DataProcessStatus.Exception);


                CheckInvoice(models, Naming.DataProcessStatus.Done, "B2BSTORAGE", "A0401");
                CheckInvoice(models, Naming.DataProcessStatus.Exception, "B2BSTORAGE", "A0401");
                CheckInvoiceCancellation(models, Naming.DataProcessStatus.Done, "B2BSTORAGE", "A0501");
                CheckInvoiceCancellation(models, Naming.DataProcessStatus.Exception, "B2BSTORAGE", "A0501");
                CheckAllowance(models, Naming.DataProcessStatus.Done, "B2BSTORAGE", "B0401");
                CheckAllowance(models, Naming.DataProcessStatus.Exception, "B2BSTORAGE", "B0401");
                CheckAllowanceCancellation(models, Naming.DataProcessStatus.Done, "B2BSTORAGE", "B0501");
                CheckAllowanceCancellation(models, Naming.DataProcessStatus.Exception, "B2BSTORAGE", "B0501");

            }
        }

        private void ArchiveLog(String target)
        {

            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = AppSettings.Default.Turnkey.Command,
                Arguments = String.Format(AppSettings.Default.Turnkey.ArgsPattern, target),
                CreateNoWindow = true,
                //UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                //WorkingDirectory = AppDomain.CurrentDomain.RelativeSearchPath,
            };

            Logger.Info($"{info.FileName} {info.Arguments}");

            Process proc = new Process();
            proc.EnableRaisingEvents = true;
            //proc.Exited += new EventHandler(proc_Exited);

            //if (null != _eventHandler)
            //{
            //    proc.Exited += new EventHandler(_eventHandler);
            //}
            proc.StartInfo = info;
            proc.Start();
            proc.WaitForExit(86400000);
        }

        private void CheckInvoice(ModelSource<UserProfile> models,  Naming.DataProcessStatus status,String storage = "B2CSTORAGE",String invoiceType = "C0401")
        {
            String storePath = Path.Combine(AppSettings.Default.Turnkey.UpCastPath, storage, invoiceType, status == Naming.DataProcessStatus.Done ? "BAK" : "ERR");
            String recyclePath = Path.Combine(AppSettings.Default.Turnkey.RecyclePath, invoiceType, status == Naming.DataProcessStatus.Done ? "BAK" : "ERR", $"{DateTime.Today:yyyyMMdd}")
                                    .CheckStoredPath();
            if (!Directory.Exists(storePath))
            {
                return;
            }

            XmlDocument doc = new XmlDocument();
            foreach (var f in Directory.EnumerateFiles(storePath, "*.xml", SearchOption.AllDirectories))
            {
                try
                {
                    doc.Load(f);
                    String invoiceNo = doc.DocumentElement?["Main"]?["InvoiceNumber"]?.InnerText;

                    Logger.Info($"{f} => {invoiceNo}");

                    if (invoiceNo == null || !Regex.IsMatch(invoiceNo, "[A-Za-z]{2}[0-9]{8}"))
                    {
                        continue;
                    }

                    int result = models.ExecuteCommand(@"
                        INSERT INTO [proc].DataProcessLog
                                       (DocID, LogDate, Status, StepID)
                        SELECT  InvoiceID, GETDATE(), {0}, {1}
                        FROM     InvoiceItem
                        WHERE   (TrackCode = {2}) AND (No = {3})",
                        (int)status,
                        (int)Naming.InvoiceStepDefinition.已傳送,
                        invoiceNo.Substring(0, 2),
                        invoiceNo.Substring(2));

                    if (result > 0)
                    {
                        File.Move(f, Path.Combine(recyclePath, Path.GetFileName(f)));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            ArchiveLog(recyclePath);
        }

        private void CheckInvoiceCancellation(ModelSource<UserProfile> models, Naming.DataProcessStatus status, String storage = "B2CSTORAGE", String invoiceType = "C0501")
        {
            String storePath = Path.Combine(AppSettings.Default.Turnkey.UpCastPath, storage, invoiceType, status == Naming.DataProcessStatus.Done ? "BAK" : "ERR");
            String recyclePath = Path.Combine(AppSettings.Default.Turnkey.RecyclePath, invoiceType, status == Naming.DataProcessStatus.Done ? "BAK" : "ERR", $"{DateTime.Today:yyyyMMdd}")
                                    .CheckStoredPath();
            if (!Directory.Exists(storePath))
            {
                return;
            }

            XmlDocument doc = new XmlDocument();
            foreach (var f in Directory.EnumerateFiles(storePath, "*.xml", SearchOption.AllDirectories))
            {
                try
                {
                    doc.Load(f);
                    String invoiceNo = doc.DocumentElement?["CancelInvoiceNumber"]?.InnerText;

                    Logger.Info($"{f} => {invoiceNo}");

                    if (invoiceNo == null || !Regex.IsMatch(invoiceNo, "[A-Za-z]{2}[0-9]{8}"))
                    {
                        continue;
                    }

                    int result = models.ExecuteCommand(@"
                        INSERT INTO [proc].DataProcessLog
                                       (DocID, LogDate, Status, StepID)
                        SELECT  DerivedDocument.DocID, GETDATE(), {0}, {1}
                        FROM     InvoiceItem INNER JOIN
                                       InvoiceCancellation ON InvoiceItem.InvoiceID = InvoiceCancellation.InvoiceID INNER JOIN
                                       DerivedDocument ON InvoiceItem.InvoiceID = DerivedDocument.SourceID
                        WHERE   (InvoiceCancellation.CancellationNo = {2})",
                        (int)status,
                        (int)Naming.InvoiceStepDefinition.已傳送,
                        invoiceNo);

                    if (result > 0)
                    {
                        File.Move(f, Path.Combine(recyclePath, Path.GetFileName(f)));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            ArchiveLog(recyclePath);
        }

        private void CheckAllowance(ModelSource<UserProfile> models, Naming.DataProcessStatus status, String storage = "B2CSTORAGE", String invoiceType = "D0401")
        {
            String storePath = Path.Combine(AppSettings.Default.Turnkey.UpCastPath, storage, invoiceType, status == Naming.DataProcessStatus.Done ? "BAK" : "ERR");
            String recyclePath = Path.Combine(AppSettings.Default.Turnkey.RecyclePath, invoiceType, status == Naming.DataProcessStatus.Done ? "BAK" : "ERR", $"{DateTime.Today:yyyyMMdd}")
                                    .CheckStoredPath();
            if (!Directory.Exists(storePath))
            {
                return;
            }

            XmlDocument doc = new XmlDocument();
            foreach (var f in Directory.EnumerateFiles(storePath, "*.xml", SearchOption.AllDirectories))
            {
                try
                {
                    doc.Load(f);
                    String allowanceNo = doc.DocumentElement?["Main"]?["AllowanceNumber"]?.InnerText;

                    Logger.Info($"{f} => {allowanceNo}");

                    if (allowanceNo == null)
                    {
                        continue;
                    }

                    int allowanceID = -1;
                    int.TryParse(allowanceNo, out allowanceID);
                    

                    int result = models.ExecuteCommand(@"
                        INSERT INTO [proc].DataProcessLog
                                       (DocID, LogDate, Status, StepID)
                        SELECT  AllowanceID, GETDATE(), {0}, {1}
                        FROM     InvoiceAllowance
                        WHERE   (AllowanceNumber = {2}) OR
                                       (AllowanceID = {3})",
                        (int)status,
                        (int)Naming.InvoiceStepDefinition.已傳送,
                        allowanceNo,
                        allowanceID);

                    if (result > 0)
                    {
                        File.Move(f, Path.Combine(recyclePath, Path.GetFileName(f)));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            ArchiveLog(recyclePath);
        }

        private void CheckAllowanceCancellation(ModelSource<UserProfile> models, Naming.DataProcessStatus status, String storage = "B2CSTORAGE", String invoiceType = "D0501")
        {
            String storePath = Path.Combine(AppSettings.Default.Turnkey.UpCastPath, storage, invoiceType, status == Naming.DataProcessStatus.Done ? "BAK" : "ERR");
            String recyclePath = Path.Combine(AppSettings.Default.Turnkey.RecyclePath, invoiceType, status == Naming.DataProcessStatus.Done ? "BAK" : "ERR", $"{DateTime.Today:yyyyMMdd}")
                                    .CheckStoredPath();
            if (!Directory.Exists(storePath))
            {
                return;
            }

            XmlDocument doc = new XmlDocument();
            foreach (var f in Directory.EnumerateFiles(storePath, "*.xml", SearchOption.AllDirectories))
            {
                try
                {
                    doc.Load(f);
                    String allowanceNo = doc.DocumentElement?["CancelAllowanceNumber"]?.InnerText;

                    Logger.Info($"{f} => {allowanceNo}");

                    if (allowanceNo == null)
                    {
                        continue;
                    }

                    int allowanceID = -1;
                    int.TryParse(allowanceNo, out allowanceID);

                    int result = models.ExecuteCommand(@"
                        INSERT INTO [proc].DataProcessLog
                                       (DocID, LogDate, Status, StepID)
                        SELECT  DerivedDocument.DocID, GETDATE(), {0}, {1}
                        FROM     InvoiceAllowance INNER JOIN
                                       InvoiceAllowanceCancellation ON InvoiceAllowance.AllowanceID = InvoiceAllowanceCancellation.AllowanceID INNER JOIN
                                       DerivedDocument ON InvoiceAllowance.AllowanceID = DerivedDocument.SourceID
                        WHERE   (InvoiceAllowance.AllowanceNumber = {2}) OR
                                       (DerivedDocument.DocID = {3})",
                        (int)status,
                        (int)Naming.InvoiceStepDefinition.已傳送,
                        allowanceNo,
                        allowanceID);

                    if (result > 0)
                    {
                        File.Move(f, Path.Combine(recyclePath, Path.GetFileName(f)));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            ArchiveLog(recyclePath);
        }

    }
}
