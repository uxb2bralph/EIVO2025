using ApplicationResource;
using Business.Helper;
using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using CommonLib.Utility;
using ModelCore.Helper;
using TaskCenter.Controllers;
using TaskCenter.Properties;
using System.IO.Compression;
using System.Text;
using CommonLib.Core.Utility;

namespace TaskCenter.Helper.RequestAction
{
    public static class UploadDataExtensions
    {
        public static Organization CheckRequest(this AuthQueryViewModel viewModel, SampleController controller)
        {
            var ModelState = controller.ModelState;
            var ViewBag = controller.ViewBag;
            var HttpContext = controller.HttpContext;
            var Request = controller.Request;
            var models = controller.DataSource;

            ViewBag.ViewModel = viewModel;

            if (viewModel.KeyID != null)
            {
                viewModel.AgentID = controller.DecryptKeyValue(viewModel, out bool expired);
                if (expired)
                {
                    ModelState.AddModelError("E1001", ErrorMessage.E1001);
                }
            }

            var item = models.GetTable<Organization>().Where(c => c.CompanyID == viewModel.AgentID).FirstOrDefault();
            if (item == null)
            {
                ModelState.AddModelError("E1003", ErrorMessage.E1003);
            }

            return item;

        }

        public static OrganizationToken CheckRequestToken(this AuthQueryViewModel viewModel, SampleController controller)
        {
            var item = viewModel.CheckRequest(controller);
            OrganizationToken token = item?.OrganizationToken;
            if (token == null)
            {
                controller.ModelState.AddModelError("E1003", ErrorMessage.E1003);
            }
            return token;

        }


        public static ProcessRequest SaveProcessRequest(this InvoiceRequestViewModel viewModel, SampleController controller)
        {
            var ModelState = controller.ModelState;
            var ViewBag = controller.ViewBag;
            var HttpContext = controller.HttpContext;
            var Request = controller.Request;
            var models = controller.DataSource;

            Organization item = viewModel.CheckRequest(controller);

            String requestPath = null;
            if (Request.Form.Files.Count == 0)
            {
                ModelState.AddModelError("E1002", ErrorMessage.E1002);
            }
            else
            {
                var file = Request.Form.Files[0];
                var fileName = file.FileName;
                if(fileName.StartsWith("=?utf-8?B?",StringComparison.InvariantCultureIgnoreCase )
                    && fileName.EndsWith("?="))
                {
                    fileName = Encoding.UTF8.GetString(Convert.FromBase64String(fileName.Substring(10, fileName.Length - 12)));
                }
                requestPath = DateTime.Today.DailyStorePath(DateTime.Now.Ticks + "_" + Path.GetFileName(fileName), out string path);
                file.SaveAs(path);
            }

            if(!viewModel.ProcessType.HasValue)
            {
                ModelState.AddModelError("E1004", ErrorMessage.E1004);
            }

            if (!ModelState.IsValid)
            {
                return null;
            }

            Naming.InvoiceProcessType processType = viewModel.ProcessType.Value;
            var processItem = new ProcessRequest
            {
                AgentID = item.CompanyID,
                Sender = viewModel.Sender,
                SubmitDate = DateTime.Now,
                RequestPath = requestPath,
                ProcessType = (int)processType,
                ProcessRequestQueue = new ProcessRequestQueue
                {

                },
                ViewModel = viewModel.JsonStringify(),
            };
            models.GetTable<ProcessRequest>().InsertOnSubmit(processItem);

            if (viewModel.ConditionID != null && viewModel.ConditionID.Length > 0)
            {
                processItem.ProcessRequestCondition.AddRange(viewModel.ConditionID
                    .Where(c => c.HasValue)
                    .Select(c => new ProcessRequestCondition
                    {
                        ConditionID = (int)c
                    }));
            }
            models.SubmitChanges();

            return processItem;

        }

        public static void StoreMIG(this InvoiceRequestViewModel viewModel, SampleController controller)
        {
            var ModelState = controller.ModelState;
            var ViewBag = controller.ViewBag;
            var HttpContext = controller.HttpContext;
            var Request = controller.Request;
            var models = controller.DataSource;

            Organization item = viewModel.CheckRequest(controller);

            if (!viewModel.ProcessType.HasValue)
            {
                ModelState.AddModelError("E1004", ErrorMessage.E1004);
            }

            String? requestPath = null;
            if (Request.Form.Files.Count == 0)
            {
                ModelState.AddModelError("E1002", ErrorMessage.E1002);
            }
            else
            {
                var file = Request.Form.Files[0];
                switch(viewModel.ProcessType)
                {
                    case Naming.InvoiceProcessType.C0401:
                    case Naming.InvoiceProcessType.C0501:
                    case Naming.InvoiceProcessType.D0401:
                    case Naming.InvoiceProcessType.D0501:
                    case Naming.InvoiceProcessType.C0701:
                        requestPath = Path.Combine(ModelExtension.Properties.AppSettings.Default.EINVTurnkeyRoot, "UpCast", "B2CSTORAGE", viewModel.ProcessType.ToString(), "SRC");
                        break;

                    case Naming.InvoiceProcessType.A0401:
                    case Naming.InvoiceProcessType.A0501:
                    case Naming.InvoiceProcessType.B0401:
                    case Naming.InvoiceProcessType.B0501:
                    case Naming.InvoiceProcessType.A0101:
                        requestPath = Path.Combine(ModelExtension.Properties.AppSettings.Default.EINVTurnkeyRoot, "UpCast", "B2BSTORAGE", viewModel.ProcessType.ToString(), "SRC");
                        break;

                }

                if(requestPath!=null)
                {
                    file.SaveAs(requestPath);
                }
            }

        }

        public static void CheckMIG(this InvoiceRequestViewModel viewModel, SampleController controller)
        {
            var ModelState = controller.ModelState;
            var ViewBag = controller.ViewBag;
            var HttpContext = controller.HttpContext;
            var Request = controller.Request;
            var models = controller.DataSource;

            Organization item = viewModel.CheckRequest(controller);

            if (!viewModel.ProcessType.HasValue)
            {
                ModelState.AddModelError("E1004", ErrorMessage.E1004);
            }

            if (viewModel.DataNo != null && viewModel.DataNo.Length > 0)
            {

            }

        }

        public static void SaveAttachment(this InvoiceRequestViewModel viewModel, SampleController controller)
        {
            var ModelState = controller.ModelState;
            var ViewBag = controller.ViewBag;
            var HttpContext = controller.HttpContext;
            var Request = controller.Request;
            var models = controller.DataSource;

            Organization item = viewModel.CheckRequest(controller);

            if (Request.Form.Files.Count == 0)
            {
                ModelState.AddModelError("E1002", ErrorMessage.E1002);
            }

            if (!ModelState.IsValid)
            {
                return;
            }

            String storePath = ModelExtension.Properties.AppSettings.Default.AttachmentTempStore.PrefixStorePath();
            for (int i = 0; i < Request.Form.Files.Count; i++)
            {
                var file = Request.Form.Files[i];
                String fileName = Path.GetFileName(file.FileName);
                String path = Path.Combine(storePath, fileName);
                file.SaveAs(path);

                if (fileName.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase))
                {
                    using(FileStream stream = File.OpenRead(path))
                    {
                        ExtractZipStream(stream, storePath);
                        stream.Close();
                    }

                    File.Move(path, Path.Combine(Logger.LogDailyPath, $"{Guid.NewGuid()}.zip"));
                }
            }

        }

        public static void ExtractZipStream(Stream zipStream, string storePath)
        {
            using (ZipArchive zip = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                foreach (var entry in zip.Entries)
                {
                    if (!String.IsNullOrEmpty(entry.Name))
                    {
                        try
                        {
                            entry.ExtractToFile(Path.Combine(storePath, entry.Name), true);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }
                    }
                }
            }
        }
    }
}