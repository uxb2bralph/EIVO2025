using CommonLib.Core.Utility;
using CommonLib.Utility;
using InvoiceClient.Agent.POSHelper;
using InvoiceClient.Helper;
using InvoiceClient.Properties;
using InvoiceClient.TransferManagement;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;
using ModelCore.Schema.TXN;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

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
            InvokeService(async () => {await ReceiveMIGAsync(); });
        }

        private static Naming.InvoiceProcessType[] _ActionType =
            {
                Naming.InvoiceProcessType.F0401,
                Naming.InvoiceProcessType.F0501,
                Naming.InvoiceProcessType.G0401,
                Naming.InvoiceProcessType.G0501,
                Naming.InvoiceProcessType.C0401,
                Naming.InvoiceProcessType.C0501,
                Naming.InvoiceProcessType.A0401,
                Naming.InvoiceProcessType.A0501,
                Naming.InvoiceProcessType.D0401,
                Naming.InvoiceProcessType.D0501,
                Naming.InvoiceProcessType.B0401,
                Naming.InvoiceProcessType.B0501,
            };
        private async Task ReceiveMIGAsync()
        {
            String apiUrl = $"{ServerInspector.ServiceInfo.TaskCenterUrl}/InvoiceData/RetrieveMIGResponse?keyID={HttpUtility.UrlEncode(ServerInspector.ServiceInfo.AgentToken)}";
            MIGResponseViewModel viewModel = new MIGResponseViewModel 
            {
                LastReceivedKey = null,
            };

            foreach (var processType in _ActionType)
            {
                bool processing = true;
                while (processing)
                {
                    try
                    {
                        viewModel!.KeyID = ServerInspector.ServiceInfo.AgentToken;
                        viewModel!.ProcessType = processType;

                        Logger.Debug($"Retrieve MIG:{apiUrl}");
                        Logger.Debug(viewModel.JsonStringify());

                        var response = await InvoiceWatcher.TheHttpClient.PostAsJsonAsync(apiUrl, viewModel);
                        if (response.IsSuccessStatusCode)
                        {
                            String result = await response.Content.ReadAsStringAsync();
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
                        else
                        {
                            Console.WriteLine($"Error: {response.StatusCode}");
                        }

                    }
                    catch (Exception ex)
                    {
                        Logger.Warn($"receive response error:{apiUrl}");
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

        public override Type? UIConfigType
        {
            get { return null; }
        }

    }
}
