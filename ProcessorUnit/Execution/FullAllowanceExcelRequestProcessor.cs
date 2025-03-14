using ModelCore.InvoiceManagement;
using ModelCore.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessorUnit.Execution
{
    public class FullAllowanceExcelRequestProcessor : AllowanceExcelRequestProcessor
    {
        public FullAllowanceExcelRequestProcessor()
        {
            appliedProcessType = Naming.InvoiceProcessType.D0401_Full_Xlsx;
            processDataSet = (ds, requestItem) =>
            {
                using (FullAllowanceDataSetManager manager = new FullAllowanceDataSetManager(models))
                {
                    return manager.SaveUploadAllowance(ds, requestItem);
                }
            };
        }
    }
}
