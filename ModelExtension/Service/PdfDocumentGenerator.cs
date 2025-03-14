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
        public static Func<RenderStyleViewModel, String>? CreateInvoicePdf { get; set; }

        public static Func<String, IEnumerable<String>, String>? MergePDF { get; set; }
    }
}
