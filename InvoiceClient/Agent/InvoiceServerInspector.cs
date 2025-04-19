using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

using InvoiceClient.Properties;
using CommonLib.Core.Utility;
using CommonLib.Utility;
using ModelCore.Schema.TXN;
using InvoiceClient.Helper;
using InvoiceClient.TransferManagement;
using System.Threading.Tasks;

namespace InvoiceClient.Agent
{

    public class InvoiceServerInspector : ServerInspector
    {
        public class LocalSettings
        {
            public String ServiceHost { get; } = ServiceInfo.ServiceHost;  //"https://eguitest.uxifs.com/cbe";
        }

        public InvoiceServerInspector()
        {

        }

        Task _invoke = null;

        public override void StartUp()
        {
            InvokeService(AcknowledgeServer);
        }

        protected void InvokeService(Action doJob)
        {
            if (_invoke == null)
            {
                var t = Task.Run(() =>
                {
                    doJob();
                });

                t = t.ContinueWith(ts =>
                {
                    if (ts.IsFaulted)
                    {
                        Logger.Error(ts.Exception);
                    }

                    Task.Delay(Settings.Default.AutoInvServiceInterval > 0 ? Settings.Default.AutoInvServiceInterval * 60 * 1000 : 1800000).ContinueWith(ts1 =>
                    {
                        _invoke = null;
                        InvokeService(doJob);
                    });
                });

                _invoke = t;
            }
        }


        public override Type UIConfigType
        {
            get { return typeof(InvoiceClient.MainContent.InvoiceServerConfig); }
        }

        //public List<string> ExceuteInvoiceService()
        //{
        //    List<String> pathInfo = new List<string>();
        //    this.ExecutiveService(pathInfo);
        //    return pathInfo;
        //}
    }
}
