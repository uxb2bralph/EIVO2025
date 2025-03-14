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

namespace ProcessorUnit.Execution
{
    public class InvoiceExcelRequestForIssuerA0401Processor : InvoiceExcelRequestProcessor
    {
        public InvoiceExcelRequestForIssuerA0401Processor() : base()
        {
            appliedProcessType = Naming.InvoiceProcessType.A0401_Xlsx_Allocation_ByIssuer;
            processDataSet = (ds, requestItem) =>
            {
                using (InvoiceDataSetManager manager = new InvoiceDataSetManager(models))
                {
                    return manager.SaveUploadInvoice(ds, requestItem, true);
                }
            };
        }
    }
}
