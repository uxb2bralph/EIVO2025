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
    public class AllowanceJsonRequestProcessor : InvoiceJsonRequestForCBEProcessor
    {

        public AllowanceJsonRequestProcessor()
        {
            appliedProcessType = Naming.InvoiceProcessType.D0401_Json;
            processRequest = (jsonData, requestItem) => 
            {
                Root result = this.CreateMessageToken();
                dynamic json = JsonConvert.DeserializeObject(jsonData);
                var s = JsonConvert.SerializeObject((object)json.AllowanceRoot.Allowance);
                AllowanceRoot allowance = new AllowanceRoot
                {
                    Allowance = JsonConvert.DeserializeObject<AllowanceRootAllowance[]>(s)
                }; 
                using (InvoiceManagerForCBE manager = new InvoiceManagerForCBE())
                {
                    var token = manager.GetTable<OrganizationToken>().Where(t => t.CompanyID == requestItem.AgentID).FirstOrDefault();
                    if (token != null)
                    {
                        manager.UploadAllowance(result, allowance, token);
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
