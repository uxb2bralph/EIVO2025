using ModelCore.DataEntity;
using ModelCore.Locale;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CommonLib.Utility;
using ModelCore.InvoiceManagement;
using ModelCore.Schema.TXN;
using ModelCore.Helper;
using ProcessorUnit.Helper;
using CommonLib.Utility.Properties;

namespace ProcessorUnit.Execution
{
    public class AllowanceMigRequestProcessor : ProcessRequestExecutorForever
    {
        public AllowanceMigRequestProcessor()
        {
            appliedProcessType = Naming.InvoiceProcessType.G0401;
            processRequest = (docInv, requestItem) =>
            {
                Root result = this.CreateMessageToken();
                using (InvoiceManagerV2 manager = new InvoiceManagerV2(models))
                {
                    var items = manager.SaveAllowanceMIG(docInv, Naming.InvoiceProcessType.G0401);
                    InvoiceMigRequestProcessor.buildResponse(result, items);
                }
                return result;
            };
        }

        protected virtual XmlDocument prepareDocument(String invoiceFile)
        {
            XmlDocument docInv = new XmlDocument();
            docInv.Load(invoiceFile);
            return docInv;
        }

        protected Func<XmlDocument, ProcessRequest, Root> processRequest;

        protected override void ProcessRequestItem()
        {
            ProcessRequest requestItem = queueItem.ProcessRequest;
            String? requestFile = requestItem?.RequestPath?.StoreTargetPath();
            if (requestFile != null && File.Exists(requestFile))
            {
                requestItem!.ProcessStart = DateTime.Now;
                models.SubmitChanges();

                XmlDocument docInv = prepareDocument(requestFile);
                Root result = processRequest(docInv, requestItem);

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
