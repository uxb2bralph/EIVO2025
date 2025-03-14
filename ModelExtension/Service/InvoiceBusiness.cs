using CommonLib.DataAccess;
using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Security.MembershipManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelCore.Service
{
    public static class InvoiceBusiness
    {
        public static void MarkPrintedLog(this GenericManager<EIVOEntityDataContext> models, InvoiceItem item, int uid)
        {
            if (!item.CDS_Document.DocumentPrintLog.Any(l => l.TypeID == (int)Naming.DocumentTypeDefinition.E_Invoice))
            {
                item.CDS_Document.DocumentPrintLog.Add(new DocumentPrintLog
                {
                    PrintDate = DateTime.Now,
                    UID = uid,
                    TypeID = (int)Naming.DocumentTypeDefinition.E_Invoice
                });
            }

            models.DeleteAnyOnSubmit<DocumentPrintQueue>(d => d.DocID == item.InvoiceID);
            models.DeleteAnyOnSubmit<DocumentAuthorization>(d => d.DocID == item.InvoiceID);
            models.SubmitChanges();
        }

        public static void MarkPrintedLog(this GenericManager<EIVOEntityDataContext> models, InvoiceAllowance item, int uid)
        {
            if (!item.CDS_Document.DocumentPrintLog.Any(l => l.TypeID == (int)Naming.DocumentTypeDefinition.E_Allowance))
            {
                item.CDS_Document.DocumentPrintLog.Add(new DocumentPrintLog
                {
                    PrintDate = DateTime.Now,
                    UID = uid,
                    TypeID = (int)Naming.DocumentTypeDefinition.E_Allowance
                });
            }

            models.DeleteAnyOnSubmit<DocumentPrintQueue>(d => d.DocID == item.AllowanceID);
            models.SubmitChanges();
        }
    }
}
