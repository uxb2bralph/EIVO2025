using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Helper;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CommonLib.Utility;
using ModelCore.InvoiceManagement;
using ModelCore.Schema.TXN;
using ModelCore.Schema.EIVO;
using Business.Helper.InvoiceProcessor;
using ProcessorUnit.Helper;
using CommonLib.Utility.Properties;

namespace ProcessorUnit.Execution
{
    public class InvoiceXmlRequestProcessor : ProcessRequestExecutorForever
    {
        public InvoiceXmlRequestProcessor()
        {
            appliedProcessType = Naming.InvoiceProcessType.F0401_Xml;
            processRequest = (invoice, requestItem) =>
            {
                Root result = this.CreateMessageToken();
                using (InvoiceManagerV2 manager = new InvoiceManagerV2(models) { ProcessType = Naming.InvoiceProcessType.F0401 })
                {
                    var token = manager.GetTable<OrganizationToken>().Where(t => t.CompanyID == requestItem.AgentID).FirstOrDefault();
                    if (token != null)
                    {
                        if (requestItem.ProcessRequestCondition.Any(c => c.ConditionID == (int)ProcessRequestCondition.ConditionType.UseLastPeriodTrackCodeNo))
                        {
                            manager.ApplyInvoiceDate = new DateTime(DateTime.Today.Year, (DateTime.Today.Month + 1) / 2 * 2 - 1, 1).AddDays(-1);
                        }

                        issueInvoice(manager, result, token, invoice);
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

        protected virtual void issueInvoice(InvoiceManagerV2 manager, Root result, OrganizationToken token, InvoiceRoot invoice)
        {
            manager.UploadInvoiceAutoTrackNo(invoice, result, token);
        }

        protected virtual InvoiceRoot prepareDocument(String invoiceFile)
        {
            XmlDocument docInv = new XmlDocument();
            docInv.Load(invoiceFile);

            ///去除"N/A"資料
            ///
            var nodes = docInv.SelectNodes("//*[text()='N/A']");
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes.Item(i);
                node.RemoveChild(node.SelectSingleNode("text()"));
            }
            ///

            return docInv.TrimAll().ConvertTo<InvoiceRoot>();
        }

        protected Func<InvoiceRoot, ProcessRequest, Root> processRequest;

        protected override void ProcessRequestItem()
        {
            ProcessRequest requestItem = queueItem.ProcessRequest;
            String? requestFile = requestItem?.RequestPath?.StoreTargetPath();
            if (requestFile != null && File.Exists(requestFile))
            {
                requestItem!.ProcessStart = DateTime.Now;
                models.SubmitChanges();

                InvoiceRoot invoice = prepareDocument(requestFile);
                Root result = processRequest(invoice, requestItem);

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
