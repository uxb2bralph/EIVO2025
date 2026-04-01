using ModelCore.Properties;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModelCore.DTOs
{
    public class QueryDto
    {
        public int? PageSize { get; set; } = AppSettings.Default.PageSize;
        public int? PageIndex { get; set; }
        public int? PageOffset { get; set; } = 0;
        public bool? Paging { get; set; }
    }
}
