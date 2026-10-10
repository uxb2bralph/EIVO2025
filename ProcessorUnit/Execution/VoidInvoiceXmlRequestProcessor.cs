using ModelCore.DataEntity;
using ModelCore.Locale;

using System;
using System.IO;
using System.Linq;
using System.Xml;
using CommonLib.Utility;
using ModelCore.InvoiceManagement;
using ModelCore.Schema.TXN;
using ModelCore.Schema.EIVO;
using ModelCore.Helper;
using Business.Helper.InvoiceProcessor;
using ProcessorUnit.Helper;
using CommonLib.Utility.Properties;

namespace ProcessorUnit.Execution
{
    public class VoidInvoiceXmlRequestProcessor : ProcessRequestExecutorForever
    {
        public VoidInvoiceXmlRequestProcessor()
        {
            appliedProcessType = Naming.InvoiceProcessType.F0501_Xml;
            processRequest = (item, requestItem) =>
            {
                Root result = this.CreateMessageToken();
                using (InvoiceManagerV2 manager = new InvoiceManagerV2(models))
                {
                    var token = manager.GetTable<OrganizationToken>().Where(t => t.CompanyID == requestItem.AgentID).FirstOrDefault();
                    if (token != null)
                    {
                        manager.UploadInvoiceCancellation(result, item, token);
                        manager.BindProcessedItem(requestItem);
                    }
                    else
                    {
                        result.Result.message = "營業人資料不存在!!";
                    }
                }
                return result;
            };
        }

        protected virtual CancelInvoiceRoot prepareDocument(String invoiceFile)
        {
            XmlDocument docInv = new XmlDocument();
            docInv.Load(invoiceFile);
            return docInv.TrimAll().ConvertTo<CancelInvoiceRoot>();
        }

        protected Func<CancelInvoiceRoot, ProcessRequest, Root> processRequest;

        protected override void ProcessRequestItem()
        {
            ProcessRequest requestItem = queueItem.ProcessRequest;
            String? requestFile = requestItem?.RequestPath?.StoreTargetPath();
            if (requestFile != null && File.Exists(requestFile))
            {
                requestItem!.ProcessStart = DateTime.Now;
                models.SubmitChanges();

                CancelInvoiceRoot item = prepareDocument(requestFile);
                Root result = processRequest(item, requestItem);

                String responseName = $"{Path.GetFileNameWithoutExtension(requestFile)}_Response.xml";
                String responsePath = Path.Combine(Path.GetDirectoryName(requestFile)!, responseName);

                if (ProcessorUnit.Properties.AppSettings.Default.ResponsePath != null)
                {
                    responsePath = responsePath.Replace(AppSettingsBase.AppRoot, ProcessorUnit.Properties.AppSettings.Default.ResponsePath);
                }

                result.ConvertToXml().Save(responsePath);
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
