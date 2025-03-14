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
    public class InvoiceExcelRequestForVACProcessor : InvoiceExcelRequestProcessor
    {
        public InvoiceExcelRequestForVACProcessor() : base()
        {
            appliedProcessType = Naming.InvoiceProcessType.C0401_Xlsx_Allocation_ByVAC;
            processDataSet = (ds, requestItem) =>
            {
                using (InvoiceDataSetManager manager = new InvoiceDataSetManager(models))
                {
                    return manager.SaveUploadInvoiceAutoTrackNoForVAC(ds, requestItem);
                }
            };
        }
    }
}
