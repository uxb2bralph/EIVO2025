using System;

namespace ArchiveData.Mapping
{
    public class InvoiceDto
    {
        public int InvoiceID { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime? InvoiceDate { get; set; }
    }

    public class AllowanceDto
    {
        public int AllowanceID { get; set; }
        public string AllowanceNumber { get; set; } = string.Empty;
        public DateTime? AllowanceDate { get; set; }
    }
}
