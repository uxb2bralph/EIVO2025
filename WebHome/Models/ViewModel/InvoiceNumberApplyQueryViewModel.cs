using ModelCore.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebHome.Models.ViewModel
{
    public class InvoiceNumberApplyQueryViewModel: QueryViewModel
    {
        public string BusinessId { get; set; } = string.Empty;
        public DateTime ApplyUpdateTime { get; set; } = DateTime.MinValue;
    }
}