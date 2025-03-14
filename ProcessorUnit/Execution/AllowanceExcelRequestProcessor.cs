using ModelCore.InvoiceManagement;
using ModelCore.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessorUnit.Execution
{
    public class AllowanceExcelRequestProcessor : InvoiceExcelRequestProcessor
    {
        public AllowanceExcelRequestProcessor()
        {
            appliedProcessType = Naming.InvoiceProcessType.D0401_Xlsx;
            processDataSet = (ds, requestItem) =>
            {
                using (AllowanceDataSetManager manager = new AllowanceDataSetManager(models))
                {
                    return manager.SaveUploadAllowance(ds, requestItem);
                }
            };
        }
    }
}
