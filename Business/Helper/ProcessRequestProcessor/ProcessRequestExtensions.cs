using Business.Helper.ReportProcessor;
using CommonLib.Core.Utility;
using CommonLib.DataAccess;
using CommonLib.Utility;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement;
using ModelCore.InvoiceManagement.ErrorHandle;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;
using ModelCore.Schema.EIVO;
using ModelCore.Schema.TXN;
using ModelCore.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Business.Helper.ProcessRequestProcessor
{
    public static class ProcessRequestExtensions
    {
        public static void ZipInvoicePDF(this int taskID)
        {
            using (GenericManager<EIVOEntityDataContext> models = new GenericManager<EIVOEntityDataContext>())
            {
                ProcessRequest? taskItem = models.GetTable<ProcessRequest>()
                                            .Where(p => p.TaskID == taskID)
                                            .FirstOrDefault();
                if (taskItem?.ViewModel == null)
                    return;

                RenderStyleViewModel viewModel = JsonConvert.DeserializeObject<RenderStyleViewModel>(taskItem.ViewModel) ?? new RenderStyleViewModel { };

                if (taskItem.ResponsePath == null)
                {
                    taskItem.ResponsePath = Path.Combine(Logger.LogDailyPath, $"{Guid.NewGuid()}.zip");
                }

                Exception? exception = null;

                try
                {
                    String? outFile = Path.Combine(Logger.LogDailyPath, $"{Guid.NewGuid()}.zip");

                    var items = models.GetTable<DocumentPrintQueue>()
                        .Where(i => i.UID == taskItem.Sender)
                        .Join(models.GetTable<InvoiceItem>(),
                            i => i.DocID, d => d.InvoiceID, (i, d) => i);

                    using (var zipOut = System.IO.File.Create(outFile))
                    {
                        using (ZipArchive? zip = new ZipArchive(zipOut, ZipArchiveMode.Create))
                        {
                            viewModel.ChkItem = null;

                            if (PdfDocumentGenerator.CreateInvoicePdf != null)
                            {
                                foreach (var doc in items)
                                {
                                    viewModel.DocID = doc.DocID;
                                    var pdfFile = PdfDocumentGenerator.CreateInvoicePdf(viewModel);
                                    var item = doc.CDS_Document.InvoiceItem;
                                    models.MarkPrintedLog(item, taskItem.Sender!.Value);
                                    zip.CreateEntryFromFile(pdfFile, $"{item.TrackCode}{item.No}.pdf");
                                    File.Delete(pdfFile);
                                }
                            }
                        }
                    }

                    File.Move(outFile, taskItem.ResponsePath);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    exception = ex;
                    Logger.Error(ex);
                }

                if (exception != null)
                {
                    taskItem.ExceptionLog = new ExceptionLog
                    {
                        DataContent = exception.Message
                    };
                }

                taskItem.ProcessComplete = DateTime.Now;
                models.SubmitChanges();


            }

        }

        public static void ProcessInvoiceMailingPackage(this int taskID, MailTrackingCsvViewModel[] items)
        {
            using (GenericManager<EIVOEntityDataContext> models = new GenericManager<EIVOEntityDataContext>())
            {
                ProcessRequest? taskItem = models.GetTable<ProcessRequest>()
                                            .Where(p => p.TaskID == taskID)
                                            .FirstOrDefault();
                if (taskItem?.ViewModel == null)
                    return;

                RenderStyleViewModel? viewModel = JsonConvert.DeserializeObject<RenderStyleViewModel>(taskItem.ViewModel);

                if (taskItem.ResponsePath == null)
                {
                    taskItem.ResponsePath = Path.Combine(Logger.LogDailyPath, $"{Guid.NewGuid()}.zip");
                }

                taskItem.TotalCount = items?.Length ?? 0;
                models.SubmitChanges();

                Exception? exception = null;

                try
                {
                    String? outFile = Path.Combine(Logger.LogDailyPath, $"{Guid.NewGuid()}.zip");

                    using (var zipOut = System.IO.File.Create(outFile))
                    {
                        using (ZipArchive? zip = new ZipArchive(zipOut, ZipArchiveMode.Create))
                        {
                            int packageIdx = 1;
                            List<String> pdfItems = new List<string>();
                            List<String> invoicePdf = new List<string>();
                            List<String> attachmentItems = new List<string>();

                            foreach (var g in items)
                            {
                                InvoiceItem? item = null, idxItem = null;
                                String? pdfPackage = Path.Combine(Logger.LogDailyPath, $"{Guid.NewGuid()}.pdf");
                                pdfItems.Clear();
                                invoicePdf.Clear();
                                attachmentItems.Clear();

                                foreach (var v in g.InvoiceID)
                                {
                                    item = models.GetTable<InvoiceItem>().Where(i => i.InvoiceID == v).FirstOrDefault();
                                    if (item == null)
                                        continue;

                                    if (idxItem == null)
                                    {
                                        idxItem = item;
                                    }

                                    viewModel.DocID = item.InvoiceID;
                                    var pdfFile = PdfDocumentGenerator.CreateInvoicePdf(viewModel);
                                    pdfItems.Add(pdfFile);
                                    invoicePdf.Add(pdfFile);

                                    foreach (var attach in item.CDS_Document.Attachment)
                                    {
                                        if (System.IO.File.Exists(attach.StoredPath))
                                        {
                                            attachmentItems.Add(attach.StoredPath);
                                        }
                                    }
                                }

                                pdfItems.AddRange(attachmentItems);

                                if (pdfItems.Count > 0)
                                {
                                    if (pdfItems.Count > 1)
                                    {
                                        PdfDocumentGenerator.MergePDF(pdfPackage, pdfItems);
                                        invoicePdf.Add(pdfPackage);
                                    }
                                    else
                                    {
                                        pdfPackage = pdfItems[0];
                                    }
                                    zip.CreateEntryFromFile(pdfPackage, $"{packageIdx:000000}-{idxItem.TrackCode}{idxItem.No}-{item.InvoiceBuyer.CustomerName.EscapeFileNameCharacter('_')}.pdf");

                                }

                                taskItem.ProgressCount = packageIdx;
                                models.SubmitChanges();

                                packageIdx++;

                                foreach (var pdf in invoicePdf)
                                {
                                    try
                                    {
                                        System.IO.File.Delete(pdf);
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Error(ex);
                                    }
                                }
                            }
                        }
                    }

                    File.Move(outFile, taskItem.ResponsePath);

                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    exception = ex;
                    Logger.Error(ex);
                }

                if (exception != null)
                {
                    taskItem.ExceptionLog = new ExceptionLog
                    {
                        DataContent = exception.Message
                    };
                }

                taskItem.ProcessComplete = DateTime.Now;
                models.SubmitChanges();
            }

        }

        public static void ProcessInvoicePdfPackage(this int taskID, MailTrackingCsvViewModel[] items)
        {
            using (GenericManager<EIVOEntityDataContext> models = new GenericManager<EIVOEntityDataContext>())
            {
                ProcessRequest? taskItem = models.GetTable<ProcessRequest>()
                                            .Where(p => p.TaskID == taskID)
                                            .FirstOrDefault();
                if (taskItem?.ViewModel == null)
                    return;

                RenderStyleViewModel? viewModel = JsonConvert.DeserializeObject<RenderStyleViewModel>(taskItem.ViewModel);

                if (taskItem.ResponsePath == null)
                {
                    taskItem.ResponsePath = Path.Combine(Logger.LogDailyPath, $"{Guid.NewGuid()}.zip");
                }

                taskItem.TotalCount = items?.Length ?? 0;
                models.SubmitChanges();

                Exception? exception = null;

                try
                {
                    String? outFile = Path.Combine(Logger.LogDailyPath, $"{Guid.NewGuid()}.zip");
                    using (var zipOut = System.IO.File.Create(outFile))
                    {
                        using (ZipArchive? zip = new ZipArchive(zipOut, ZipArchiveMode.Create))
                        {
                            int packageIdx = 1;
                            foreach (var g in items)
                            {
                                int idx = 1;
                                foreach (var v in g.InvoiceID)
                                {
                                    InvoiceItem? item = models.GetTable<InvoiceItem>().Where(i => i.InvoiceID == v).FirstOrDefault();
                                    if (item == null)
                                        continue;

                                    viewModel.DocID = item.InvoiceID;
                                    var pdfFile = PdfDocumentGenerator.CreateInvoicePdf(viewModel);

                                    zip.CreateEntryFromFile(pdfFile, $"{packageIdx:000000}-{idx++:000}-{Path.GetFileName(pdfFile)}");

                                    foreach (var attach in item.CDS_Document.Attachment)
                                    {
                                        if (System.IO.File.Exists(attach.StoredPath))
                                        {
                                            zip.CreateEntryFromFile(attach.StoredPath, $"{packageIdx:000000}-{idx++:000}-{Path.GetFileName(attach.StoredPath)}");
                                        }
                                    }

                                    try
                                    {
                                        System.IO.File.Delete(pdfFile);
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Error(ex);
                                    }

                                }

                                taskItem.ProgressCount = packageIdx;
                                models.SubmitChanges();

                                packageIdx++;
                            }
                        }
                    }

                }

                catch (Exception ex)
                {
                    Logger.Error(ex);
                    exception = ex;
                    Logger.Error(ex);
                }

                if (exception != null)
                {
                    taskItem.ExceptionLog = new ExceptionLog
                    {
                        DataContent = exception.Message
                    };
                }

                taskItem.ProcessComplete = DateTime.Now;
                models.SubmitChanges();
            }
        }

    }
}
