using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ModelCore.DataEntity;
using ModelCore.Locale;

namespace ModelCore.Helper
{
    public static class DataStoreExtensionMethods
    {
        public static void Save(this InvoiceEntity entity)
        {
            using (ModelSource<InvoiceItem> models = new ModelSource<InvoiceItem>())
            {
                var item = models.ConvertToInvoiceItem(entity);
                if (item != null)
                {
                    models.GetTable<InvoiceItem>().InsertOnSubmit(item);
                    models.SubmitChanges();
                    entity.Status = Naming.UploadStatusDefinition.匯入成功;
                }
            }
        }
    }
}
