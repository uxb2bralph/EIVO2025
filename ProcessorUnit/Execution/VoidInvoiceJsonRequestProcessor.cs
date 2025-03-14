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

namespace ProcessorUnit.Execution
{
    public class VoidInvoiceJsonRequestProcessor : InvoiceJsonRequestForCBEProcessor
    {

        public VoidInvoiceJsonRequestProcessor()
        {
            appliedProcessType = Naming.InvoiceProcessType.C0501_Json;
            processRequest = (jsonData, requestItem) => 
            {
                Root result = this.CreateMessageToken();
                dynamic json = JsonConvert.DeserializeObject(jsonData);
                var s = JsonConvert.SerializeObject((object)json.CancelInvoiceRoot.CancelInvoice);
                CancelInvoiceRoot cancellation = new CancelInvoiceRoot
                {
                    CancelInvoice = JsonConvert.DeserializeObject<CancelInvoiceRootCancelInvoice[]>(s)
                }; 
                using (InvoiceManagerForCBE manager = new InvoiceManagerForCBE())
                {
                    var token = manager.GetTable<OrganizationToken>().Where(t => t.CompanyID == requestItem.AgentID).FirstOrDefault();
                    if (token != null)
                    {
                        manager.UploadInvoiceCancellation(result, cancellation, token);
                    }
                    else
                    {
                        result.Result.message = "Issuer token doesn't exist.";
                    }
                }
                return JsonConvert.SerializeObject(result, _JSON_Settings);
            };
        }

    }
}
