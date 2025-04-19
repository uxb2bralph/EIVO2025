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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web;
using ModelCore.Models.ViewModel;
using System.Net;
using ModelCore.Locale;
using InvoiceClient.Agent.POSHelper;

namespace InvoiceClient.Agent
{

    public class MIGResponseInspector : POSReady
    {
        public MIGResponseInspector()
        {
            Logger.Info($"Activate Inspector:{this.GetType().FullName}");
        }

        public override void StartUp()
        {
            InvokeService(ReceiveMIG);
        }

        private static Naming.InvoiceProcessType[] _ActionType =
            {
                Naming.InvoiceProcessType.C0401,
                Naming.InvoiceProcessType.C0501,
                Naming.InvoiceProcessType.A0401,
                Naming.InvoiceProcessType.A0501,
                Naming.InvoiceProcessType.D0401,
                Naming.InvoiceProcessType.D0501,
                Naming.InvoiceProcessType.B0401,
                Naming.InvoiceProcessType.B0501,
            };
        private void ReceiveMIG()
        {
            String url = $"{ServerInspector.ServiceInfo.TaskCenterUrl}/InvoiceData/RetrieveMIGResponse?keyID={HttpUtility.UrlEncode(ServerInspector.ServiceInfo.AgentToken)}";
            MIGResponseViewModel viewModel = new MIGResponseViewModel { };

            foreach (var processType in _ActionType)
            {
                viewModel.ProcessType = processType;
                bool processing = true;
                while (processing)
                {
                    try
                    {
                        using (WebClientEx client = new WebClientEx())
                        {
                            client.Timeout = 43200000;
                            client.Headers[HttpRequestHeader.ContentType] = "application/json";

                            Logger.Debug($"Retrieve MIG:{url}");
                            Logger.Debug(viewModel.JsonStringify());

                            String result = client.UploadString(url, viewModel.JsonStringify());
                            viewModel = JsonConvert.DeserializeObject<MIGResponseViewModel>(result);
                            processing = viewModel!.Items?.Length > 0;
                            viewModel.LastReceivedKey = null;

                            if (processing)
                            {
                                foreach (var item in viewModel.Items!)
                                {
                                    StoreMIG(processType, item);
                                }
                                viewModel.LastReceivedKey = viewModel.Items!
                                    .Where(m => m.DocID.HasValue)
                                    .Select(m => m.DocID!.Value).ToArray();
                                viewModel.Items = null;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Logger.Warn($"receive response error:{url}");
                        Logger.Error(ex);
                        processing = false;
                    }

                }
            }

        }

        private void StoreMIG(Naming.InvoiceProcessType processType, MIGContent item)
        {
            String storePath = Path.Combine(_Settings.MIGResponse, item.ReceiptNo ?? "0000000000", $"{item.DocDate:yyyyMMdd}", $"{processType}")
                .CheckStoredPath();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(item.MIG!.Replace(".0000000+08:00", ""));
            doc.SaveDocumentWithEncoding(Path.Combine(storePath, $"{item.No.EscapeFileNameCharacter('_')}.xml"), new System.Text.UTF8Encoding(false));
        }

        public override Type UIConfigType
        {
            get { return null; }
        }

    }
}
