using CommonLib.Core.Utility;
using CommonLib.Utility;
using InvoiceClient.Helper;
using InvoiceClient.Properties;
using InvoiceClient.TransferManagement;
using ModelCore.Models.ViewModel;
using ModelCore.Schema.TXN;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace InvoiceClient.Agent
{

    public class ProcessTaskResponseInspector : InvoiceServerInspector
    {
        public ProcessTaskResponseInspector()
        {

        }

        public override void StartUp()
        {
            InvokeService(async () => { await ReceiveProcessResponseAsync(); });
        }

        private async Task ReceiveProcessResponseAsync()
        {
            //System.Diagnostics.Debugger.Launch();
            String apiUrl = $"{ServerInspector.ServiceInfo.TaskCenterUrl}/InvoiceData/NotifyRequestCompletion";
            var viewModel = new InvoiceRequestViewModel
            {
                KeyID = ServerInspector.ServiceInfo.AgentToken,
                Sender = ServerInspector.ServiceInfo.AgentUID,
            };

            var response = await InvoiceWatcher.TheHttpClient.PostAsJsonAsync(apiUrl, viewModel);

            if (response.IsSuccessStatusCode)
            {
                String result = await response.Content.ReadAsStringAsync();
                try
                {
                    dynamic? jsonResult = JsonConvert.DeserializeObject(result);

                    if (jsonResult?.result == true)
                    {
                        JArray items = (JArray)jsonResult.data;
                        foreach (dynamic item in items)
                        {
                            viewModel.TaskID = (int?)item.TaskID;
                            await RetrieveResponseAsync(viewModel, (String)item.ChannelName, (String)item.ChannelResponse, (String)item.ResponseName, (String)item.TxnPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn($"receive response error:{apiUrl}");
                    Logger.Error(ex);
                }
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
            }
        }

        private async Task RetrieveResponseAsync(InvoiceRequestViewModel viewModel, String channelName, String channelResponse, String responseName, String txnPath)
        {
            String apiUrl = $"{ServerInspector.ServiceInfo.TaskCenterUrl}/InvoiceData/CommitProcessResponse";

            var response = await InvoiceWatcher.TheHttpClient.PostAsJsonAsync(apiUrl, viewModel);

            if (response.IsSuccessStatusCode)
            {
                // The response may be a file or empty, depending on server logic
                var contentType = response.Content.Headers.ContentType?.MediaType;
                if (contentType == "application/octet-stream")
                {
                    var fileBytes = await response.Content.ReadAsByteArrayAsync();
                    String filePath = Path.Combine(txnPath ?? Settings.Default.InvoiceTxnPath[0], channelResponse);
                    filePath.CheckStoredPath();
                    filePath = Path.Combine(filePath, responseName.Substring(responseName.IndexOf('_') + 1));

                    System.IO.File.WriteAllBytes(filePath, fileBytes);
                }
                //else
                //{
                //    var result = await response.Content.ReadAsStringAsync();
                //    Logger.Warn("Response: " + result);
                //}
            }
            else
            {
                Logger.Warn($"Error: {response.StatusCode}");
            }

        }

        public override Type? UIConfigType
        {
            get { return null; }
        }

    }
}
