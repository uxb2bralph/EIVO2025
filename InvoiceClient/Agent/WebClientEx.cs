using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.ComponentModel;

namespace InvoiceClient.Agent
{
    public class WebClientEx : WebClient
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Timeout { get; set; } = -1; // default is -1, no timeout
        protected WebRequest? _request;

        public WebClientEx() : base()
        {
            this.Encoding = Encoding.UTF8;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            _request = base.GetWebRequest(address);
            _request.Timeout = Timeout;
            return _request;
        }

        public WebResponse? Response => _request != null ? GetWebResponse(_request) : null;
    }
}
