using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Threading;

using ModelCore.DataEntity;
using CommonLib.DataAccess;

using ModelCore.Locale;
using CommonLib.Utility;
using WebHome.Properties;
using ModelCore.InvoiceManagement;
using System.Net;
using System.Text;
using ModelCore.Models.ViewModel;
using ModelCore.Service;
using CommonLib.Core.Utility;

namespace WebHome.Helper
{
    public static partial class B2BExtensionMethods
    {
        public static readonly string  TempForReceivePDF;

        static B2BExtensionMethods()
        {
            TempForReceivePDF = Path.Combine(System.IO.Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory), "TempForReceivePDF");// Startup.MapPath("~/TempForReceivePDF");
            if (!Directory.Exists(TempForReceivePDF))
                Directory.CreateDirectory(TempForReceivePDF);
        }

        public static void ReceiveInvoiceItem(this UserProfile userProfile, GenericManager<EIVOEntityDataContext> mgr, InvoiceItem item)
        {
            userProfile.MoveToNextStep(item.CDS_Document, mgr);
            ThreadPool.QueueUserWorkItem(t =>
            {
                EIVOTurnkeyFactory.NotifyReceivedInvoice(userProfile, new EventArgs<InvoiceItem> { Argument = item });
            });
            
        }

        public static void IssueInvoiceItem(this UserProfile userProfile, GenericManager<EIVOEntityDataContext> mgr, InvoiceItem item)
        {
            item.InvoiceDate = DateTime.Now;
            userProfile.MoveToNextStep(item.CDS_Document, mgr);
        }

        public static void ReceiveInvoiceCancellation(this UserProfile userProfile, GenericManager<EIVOEntityDataContext> mgr, CDS_Document item)
        {
            userProfile.MoveToNextStep(item, mgr);
        }

        public static void IssueInvoiceCancellation(this UserProfile userProfile, GenericManager<EIVOEntityDataContext> mgr, CDS_Document item)
        {
            userProfile.MoveToNextStep(item, mgr);
        }

        public static void IssueInvoiceAllowance(this UserProfile userProfile, GenericManager<EIVOEntityDataContext> mgr, InvoiceAllowance item)
        {
            userProfile.MoveToNextStep(item.CDS_Document, mgr);
        }

        public static void ReceiveInvoiceAllowance(this UserProfile userProfile, GenericManager<EIVOEntityDataContext> mgr, InvoiceAllowance item)
        {
            userProfile.MoveToNextStep(item.CDS_Document, mgr);
        }


        public static void IssueAllowanceCancellation(this UserProfile userProfile, GenericManager<EIVOEntityDataContext> mgr, CDS_Document item)
        {
            userProfile.MoveToNextStep(item, mgr);
        }

        public static void ReceiveAllowanceCancellation(this UserProfile userProfile, GenericManager<EIVOEntityDataContext> mgr, CDS_Document item)
        {
            userProfile.MoveToNextStep(item, mgr);
        }

        public static void ReceiveReceipt(this UserProfile userProfile, GenericManager<EIVOEntityDataContext> mgr, CDS_Document item)
        {
            userProfile.MoveToNextStep(item, mgr);
        }

        public static void ReceiveReceiptCancellation(this UserProfile userProfile, GenericManager<EIVOEntityDataContext> mgr, CDS_Document item)
        {
            userProfile.MoveToNextStep(item, mgr);
        }

        public static bool MoveToNextStep(this UserProfile userProfile, CDS_Document item, GenericManager<EIVOEntityDataContext> mgr)
        {
            var flowStep = item.DocumentFlowStep;
            if (flowStep != null)
            {
                var currentStep = mgr.GetTable<DocumentFlowControl>().Where(f => f.StepID == flowStep.CurrentFlowStep).First();
                if (currentStep.NextStep.HasValue)
                {
                    var nextStep = currentStep.NextStepItem;
                    flowStep.CurrentFlowStep = nextStep.StepID;
                    item.CurrentStep = nextStep.LevelID;

                    mgr.GetTable<DocumentProcessLog>().InsertOnSubmit(new DocumentProcessLog
                    {
                        DocID = flowStep.DocID,
                        StepDate = DateTime.Now,
                        FlowStep = nextStep.LevelID,
                        UID = userProfile.UID
                    });

                    mgr.SubmitChanges();
                    return true;
                }
            }
            return false;
        }


        public static Naming.InvoiceCenterBusinessType? CheckBusinessType(this InvoiceItem invoice, GenericManager<EIVOEntityDataContext> mgr,int companyID)
        {
            if (invoice.InvoiceSeller.SellerID == companyID)
            {
                return Naming.InvoiceCenterBusinessType.銷項;
            }
            else if (invoice.InvoiceBuyer.BuyerID == companyID)
            {
                return Naming.InvoiceCenterBusinessType.進項;
            }
            else if (mgr.GetTable<BusinessRelationship>().Any(b => b.MasterID == invoice.InvoiceSeller.SellerID && b.RelativeID == invoice.InvoiceBuyer.BuyerID))
            {
                return Naming.InvoiceCenterBusinessType.銷項;
            }
            else if (mgr.GetTable<BusinessRelationship>().Any(b => b.MasterID == invoice.InvoiceBuyer.BuyerID && b.RelativeID == invoice.InvoiceSeller.SellerID))
            {
                return Naming.InvoiceCenterBusinessType.進項;
            }
            return (Naming.InvoiceCenterBusinessType?)null;
        }

//        public static String CreateContentAsPDF(this HttpServerUtility Server, String relativePath,double timeOutInMinute)
//        {
//            String path = Startup.MapPath("~/temp");
//            path.CheckStoredPath();

//            Guid uniqueID = Guid.NewGuid();
//            String saveTo = Path.Combine(System.IO.Path, String.Format("{0}.htm",uniqueID ));
//            String pdfFile = Path.Combine(System.IO.Path, String.Format("{0}.pdf", uniqueID));
//            String tempHtml = Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogDailyPath, String.Format("{0}.htm", uniqueID));

//            using (StreamWriter sw = new StreamWriter(tempHtml))
//            {
//                Server.Execute(relativePath, sw, true);
//                sw.Flush();
//                sw.Close();
//            }
//            File.Move(tempHtml, saveTo);

////            convertHtmlToPDF(saveTo, pdfFile, timeOutInMinute);

//            saveTo.ConvertHtmlToPDF(pdfFile, timeOutInMinute);

//            if (File.Exists(pdfFile))
//            {
//                File.Delete(saveTo);

//                bool checking = true;
//                while (checking)
//                {
//                    try
//                    {
//                        using (var fs = File.OpenRead(pdfFile))
//                        {
//                            fs.Close();
//                            checking = false;
//                        }
//                    }
//                    catch (Exception ex)
//                    {
//                        CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
//                    }
//                }
//                return pdfFile;
//            }

//            return null;
//        }

        //private static void convertHtmlToPDF(String htmlFile, String pdfFile, double timeOutInMinute)
        //{
        //    String pdfTrigger = Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogDailyPath, String.Format("{0}.txt", Path.GetFileNameWithoutExtension(htmlFile)));
        //    using (StreamWriter sw = new StreamWriter(pdfTrigger))
        //    {
        //        sw.Write(htmlFile);
        //        sw.Flush();
        //        sw.Close();
        //    }
        //    File.Move(pdfTrigger, Path.Combine(ModelExtension.Properties.AppSettings.Default.PDFWorkingQueue, Path.GetFileName(pdfTrigger)));

        //    DateTime deadline = DateTime.Now.AddMinutes(timeOutInMinute);

        //    while (!File.Exists(pdfFile) && DateTime.Now < deadline)
        //    {
        //        Thread.Yield();
        //    }
        //}

        public static String CreatePdfFile(this InvoiceItem item,bool alwaysCreateNew = false)
        {

            String fileName = TempForReceivePDF.GetDateStylePath(item.InvoiceDate.Value) + "\\" + item.TrackCode + item.No + ".pdf";

            if (alwaysCreateNew || !File.Exists(fileName))
            {
                RenderStyleViewModel viewModel = new RenderStyleViewModel
                {
                    DocID = item.InvoiceID,
                    ProcessType = Naming.InvoiceProcessType.A0401,
                    PaperStyle = "A4",
                };

                String tempPDF = PdfDocumentGenerator.CreateInvoicePdf(viewModel);

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                File.Copy(tempPDF, fileName, true);
                try
                {
                    File.Delete(tempPDF);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }

                //using (WebClient client = new WebClient())
                //{
                //    client.Encoding = Encoding.UTF8;
                //    String tempPDF = client.DownloadString(String.Format("{0}{1}?id={2}&nameOnly=true&processType=A0401",
                //                            ModelExtension.Properties.AppSettings.Default.HostUrl,
                //                            VirtualPathUtility.ToAbsolute("~/DataView/PrintSingleInvoiceAsPDF"),
                //                            item.InvoiceID));

                //    if (File.Exists(fileName))
                //    {
                //        File.Delete(fileName);
                //    }

                //    //Logger.Debug($"move file: {tempPDF} -> {fileName}");

                //    //File.Move(tempPDF, fileName);
                //    File.Copy(tempPDF, fileName, true);
                //    try
                //    {
                //        File.Delete(tempPDF);
                //    }
                //    catch(Exception ex)
                //    {
                //        Logger.Error(ex);
                //    }
                //}
            }

            return fileName;
        }

        public static String PrepareToDownload(this GenericManager<EIVOEntityDataContext> mgr, InvoiceItem item,bool? isMail)
        {
            String fileName = item.CreatePdfFile();

            if (File.Exists(fileName))
            {

                var docQ = mgr.GetTable<DocumentSubscriptionQueue>();

                if (item.InvoiceBuyer?.Organization?.OrganizationStatus?.EntrustToPrint == true
                    && !docQ.Any(q => q.DocID == item.InvoiceID) && (bool)!isMail)
                {
                    docQ.InsertOnSubmit(new DocumentSubscriptionQueue { DocID = item.InvoiceID });
                    mgr.SubmitChanges();
                }

                return fileName;
            }

            return null;

        }

        public static String CreatePdfFile(this InvoiceAllowance item, bool alwaysCreateNew = true)
        {

            String fileName = $"{TempForReceivePDF.GetDateStylePath(item.AllowanceDate.Value)}\\{item.AllowanceNumber}.pdf";

            if (alwaysCreateNew || !File.Exists(fileName))
            {

                using (WebClient client = new WebClient())
                {
                    String tempPDF = client.DownloadString(String.Format("{0}{1}?id={2}&nameOnly=true",
                                            ModelExtension.Properties.AppSettings.Default.HostUrl,
                                            VirtualPathUtility.ToAbsolute("~/DataView/PrintSingleAllowanceAsPDF"),
                                            item.AllowanceID));

                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }

                    File.Move(tempPDF, fileName);
                }
            }

            return fileName;
        }

        public static String PrepareToDownload(this GenericManager<EIVOEntityDataContext> mgr, InvoiceAllowance item)
        {
            String fileName = item.CreatePdfFile();

            if (File.Exists(fileName))
            {

                var docQ = mgr.GetTable<DocumentSubscriptionQueue>();

                if (item.InvoiceAllowanceBuyer?.Organization?.OrganizationStatus?.EntrustToPrint == true
                    && !docQ.Any(q => q.DocID == item.AllowanceID))
                {
                    docQ.InsertOnSubmit(new DocumentSubscriptionQueue { DocID = item.AllowanceID });
                    mgr.SubmitChanges();
                }

                return fileName;
            }

            return null;

        }


    }
}