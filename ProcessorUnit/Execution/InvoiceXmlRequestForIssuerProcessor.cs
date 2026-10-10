using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.InvoiceManagement;
using ModelCore.Schema.TXN;
using ModelCore.Schema.EIVO;
using Business.Helper.InvoiceProcessor;

namespace ProcessorUnit.Execution
{
    public class InvoiceXmlRequestForIssuerProcessor : InvoiceXmlRequestProcessor
    {
        public InvoiceXmlRequestForIssuerProcessor() : base()
        {
            appliedProcessType = Naming.InvoiceProcessType.F0401_Xml_Allocation_ByIssuer;
        }

        protected override void issueInvoice(InvoiceManagerV2 manager, Root result, OrganizationToken token, InvoiceRoot invoice)
        {
            manager.UploadInvoice(invoice, result, token);
        }
    }
}
