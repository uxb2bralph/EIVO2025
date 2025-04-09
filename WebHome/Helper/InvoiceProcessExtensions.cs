using CommonLib.DataAccess;
using CommonLib.Utility;
using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;

namespace WebHome.Helper
{
    public static class InvoiceProcessExtensions
    {
        public static void ProcessVoidInvoiceRequest(this GenericManager<EIVOEntityDataContext> models, Naming.VoidActionMode? mode, ReviseInvoiceViewModel? viewModel, InvoiceItem item)
        {
            var request = item.CDS_Document.VoidInvoiceRequest;
            if (request == null)
            {
                request = new VoidInvoiceRequest
                {
                    InvoiceNo = $"{item.TrackCode}{item.No}",
                };
                item.CDS_Document.VoidInvoiceRequest = request;
            }

            request.VoidDate = DateTime.Now;
            request.Reason = mode.HasValue ? $"{mode}" : "註銷重開";
            request.RequestType = (int?)mode;
            request.CommitDate = null;
            if (viewModel?.ReviseContent != null)
                request.ReviseContent = viewModel?.ReviseContent?.JsonStringify();
            models!.SubmitChanges();
        }
    }
}
using CommonLib.DataAccess;
using CommonLib.Utility;
using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;

namespace WebHome.Helper
{
    public static class InvoiceProcessExtensions
    {
        public static void ProcessVoidInvoiceRequest(this GenericManager<EIVOEntityDataContext> models, Naming.VoidActionMode? mode, ReviseInvoiceViewModel? viewModel, InvoiceItem item)
        {
            var request = item.CDS_Document.VoidInvoiceRequest;
            if (request == null)
            {
                request = new VoidInvoiceRequest
                {
                };
                item.CDS_Document.VoidInvoiceRequest = request;
            }

            request.VoidDate = DateTime.Now;
            request.Reason = mode.HasValue ? $"{mode}" : "註銷重開";
            request.RequestType = (int?)mode;
            request.CommitDate = null;
            if (viewModel?.ReviseContent != null)
                request.ReviseContent = viewModel?.ReviseContent?.JsonStringify();
            models!.SubmitChanges();
        }
    }
}
