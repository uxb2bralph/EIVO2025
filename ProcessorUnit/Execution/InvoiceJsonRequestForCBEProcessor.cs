using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Helper;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using CommonLib.Utility;
using ModelCore.InvoiceManagement;
using ClosedXML.Excel;
using System.Xml;
using ModelCore.Schema.TXN;
using Business.Helper.InvoiceProcessor;
using ProcessorUnit.Helper;
using Newtonsoft.Json;
using ModelCore.Schema.EIVO;
using ModelCore.Models.ViewModel;
using CommonLib.Utility.Properties;


namespace ProcessorUnit.Execution
{
    public class InvoiceJsonRequestForCBEProcessor : ProcessRequestExecutorForever
    {
        public static readonly JsonSerializerSettings _JSON_Settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public InvoiceJsonRequestForCBEProcessor()
        {
            appliedProcessType = Naming.InvoiceProcessType.C0401_Json_CBE;
            processRequest = (jsonData, requestItem) =>
            {
                Root result = this.CreateMessageToken();
                dynamic json = JsonConvert.DeserializeObject(jsonData);
                var s = JsonConvert.SerializeObject((object)json.InvoiceRoot.Invoice);
                InvoiceRoot invoice = new InvoiceRoot
                {
                    Invoice = JsonConvert.DeserializeObject<InvoiceRootInvoice[]>(s)
                };
                using (InvoiceManagerForCBE manager = new InvoiceManagerForCBE())
                {
                    if (requestItem.ViewModel != null)
                    {
                        InvoiceRequestViewModel viewModel = JsonConvert.DeserializeObject<InvoiceRequestViewModel>(requestItem.ViewModel);
                        manager.ApplyInvoiceDate = viewModel.ApplyInvoiceDate;
                    }

                    var token = manager.GetTable<OrganizationToken>().Where(t => t.CompanyID == requestItem.AgentID).FirstOrDefault();
                    if (token != null)
                    {
                        manager.UploadInvoiceAutoTrackNo(result, token, invoice);
                        manager.BindProcessedItem(requestItem);
                    }
                    else
                    {
                        result.Result.message = "Issuer token doesn't exist.";
                    }
                }
                return JsonConvert.SerializeObject(result, _JSON_Settings);
            };
        }

        protected virtual String prepareDocument(String invoiceFile)
        {
            return File.ReadAllText(invoiceFile);
        }


        protected Func<String, ProcessRequest, String> processRequest;

        protected override void ProcessRequestItem()
        {
            ProcessRequest requestItem = queueItem.ProcessRequest;
            String requestFile = requestItem.RequestPath.StoreTargetPath();
            if (File.Exists(requestFile))
            {
                Organization agent = requestItem.Organization;
                requestItem.ProcessStart = DateTime.Now;
                models.SubmitChanges();

                var uploadData = prepareDocument(requestFile);
                var result = processRequest(uploadData, requestItem);
                String responseName = $"{Path.GetFileNameWithoutExtension(requestFile)}_Response.json";
                String responsePath = Path.Combine(Path.GetDirectoryName(requestFile), responseName);

                if (ProcessorUnit.Properties.AppSettings.Default.ResponsePath != null)
                {
                    responsePath = responsePath.Replace(AppSettingsBase.AppRoot, ProcessorUnit.Properties.AppSettings.Default.ResponsePath);
                }

                File.WriteAllText(responsePath, result);
                requestItem.ProcessComplete = DateTime.Now;
                requestItem.ResponsePath = responsePath;
                if (requestItem.ProcessCompletionNotification == null)
                    requestItem.ProcessCompletionNotification = new ProcessCompletionNotification { };
                models.DeleteAnyOnSubmit<ProcessRequestQueue>(d => d.TaskID == queueItem.TaskID);
                models.SubmitChanges();

            }
        }

    }
}
