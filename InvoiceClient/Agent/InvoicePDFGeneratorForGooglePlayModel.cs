using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceClient.Agent
{
    public class InvoicePDFGeneratorForGooglePlayModel
    {
        public DateTime? Date { get; set; }
        public string? Path { get; set; }
        public string? OrderNo { get; set; }
        public String? Url { get; set; }
    }

    public class InvoicePDFWatcherForZipModel 
    {
        public DateTime? Date { get; set; }
        public string? Path { get; set; }
        public string? FileName { get; set; }
        public int? Status { get; set; }
    }

    public class PGPEncryptWatcherModel
    {
        public string? FileName { get; set; }
        public int? Status { get; set; }
    }
}
