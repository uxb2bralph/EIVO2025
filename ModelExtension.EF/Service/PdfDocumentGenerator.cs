using CommonLib.Core.Utility;
using ModelCore.DataEntity;
using ModelCore.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelCore.Service
{
    public static class PdfDocumentGenerator
    {
        public static String? CreateInvoicePdf(RenderStyleViewModel viewModel)
        {
            try
            {
                String contentUrl = $"{ModelExtension.Properties.AppSettings.Default.ShowInvoiceUrl}?{viewModel.ToQueryString()}";
                String pdf = Path.Combine(Logger.LogDailyPath, $"{Guid.NewGuid()}.pdf");
                contentUrl.ConvertHtmlToPDF(pdf, ModelExtension.Properties.AppSettings.Default.SessionTimeout);
                return pdf;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return null;

        }

        public static String? CreateAllowancePdf(RenderStyleViewModel viewModel)
        {
            try
            {
                String contentUrl = $"{ModelExtension.Properties.AppSettings.Default.ShowAllowanceUrl}?{viewModel.ToQueryString()}";
                String pdf = Path.Combine(Logger.LogDailyPath, $"{Guid.NewGuid()}.pdf");
                contentUrl.ConvertHtmlToPDF(pdf, ModelExtension.Properties.AppSettings.Default.SessionTimeout);
                return pdf;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return null;

        }

        public static Func<String, IEnumerable<String>, String>? MergePDF { get; set; }
    }
}
