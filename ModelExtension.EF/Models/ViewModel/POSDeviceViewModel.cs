using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ModelCore.Models.ViewModel
{
    public class POSDeviceViewModel : AuthTokenViewModel
    {
        [JsonIgnore]
        public String company_id
        {
            get => SellerID;
            set => SellerID = value;
        }

        public int? quantity { get; set; }

        public int? Booklet { get; set; }
        public String? InvoiceNo { get; set; }
        public String? AllowanceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public String? DeviceNo { get; set; }
    }
}