using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebHome.Models
{
    public class InvoiceNumberApplyResponse
    {

        public bool IsInvalid { get; set; } = false;

        public string Message { get; set; } = string.Empty;
        public string DisplayMessage
        {
            get
            {
                return string.Format("{0}{1}", ((string.IsNullOrEmpty(BusinessID))?string.Empty: "BusinessID="+BusinessID+ "-"), Message);
            }
            
        } 
        public string BusinessID { get; set; }
        public string RedirectUrl { get; set; }
        public InvoiceNumberApplyResponse(string message, string businessID="")
        {
            IsInvalid = true;
            Message = message;
            BusinessID = businessID;
        }

        public InvoiceNumberApplyResponse()
        {
            IsInvalid=false;
            RedirectUrl = "../../Account/CbsLogin";
        }
    }
}