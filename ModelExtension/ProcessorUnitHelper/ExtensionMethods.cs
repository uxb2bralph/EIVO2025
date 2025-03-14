using CommonLib.DataAccess;
using ModelCore.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelCore.ProcessorUnitHelper
{
    public static class ExtensionMethods
    {
        public static ProcessorUnit RegisterProcessorUnit(this Guid instanceID, GenericManager<EIVOEntityDataContext> models)
        {
            var table = models.GetTable<ProcessorUnit>();
            var item = table.Where(t => t.ProcessorToken == instanceID).FirstOrDefault();
            if (item == null)
            {
                item = new ProcessorUnit
                {
                    ProcessorToken = instanceID,
                };
                table.InsertOnSubmit(item);
                models.SubmitChanges();
            }
            return item;
        }
    }
}
