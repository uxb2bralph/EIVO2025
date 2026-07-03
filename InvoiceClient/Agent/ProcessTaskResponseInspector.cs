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
using CommonLib.Core.Utility;
using CommonLib.Utility;
using InvoiceClient.Helper;
using InvoiceClient.Properties;
using InvoiceClient.TransferManagement;
using ModelCore.Models.ViewModel;
using ModelCore.Schema.TXN;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TaskCenter.Core.DTOs;

namespace InvoiceClient.Agent
{

    public class ProcessTaskResponseInspector : InvoiceServerInspector
    {
        public ProcessTaskResponseInspector()
        {

        }

        public override void StartUp()
        {
            InvokeService(async () => 
            { 
                await ReceiveProcessResponseAsync();
                await ReceiveProcessExceptionAsync();
            });
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
                    NotifyRequestCompletionResultDto? jsonResult = JsonConvert.DeserializeObject<NotifyRequestCompletionResultDto>(result);

                    if (jsonResult?.result == true)
                    {
                        var items = jsonResult.data ?? [];
                        foreach (var item in items)
                        {
                            viewModel.TaskID = item.TaskID;
                            await RetrieveResponseAsync(viewModel, item.ChannelName ?? string.Empty, item.ChannelResponse ?? string.Empty, item.ResponseName ?? string.Empty, item.TxnPath ?? string.Empty);
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

        private async Task ReceiveProcessExceptionAsync()
        {
            //System.Diagnostics.Debugger.Launch();
            String apiUrl = $"{ServerInspector.ServiceInfo.TaskCenterUrl}/InvoiceData/NotifyRequestException";
            var viewModel = new InvoiceRequestViewModel
            {
                KeyID = ServerInspector.ServiceInfo.AgentToken,
                Sender = ServerInspector.ServiceInfo.AgentUID,
            };

            var response = await InvoiceWatcher.TheHttpClient.PostAsJsonAsync(apiUrl, viewModel);

            if (response.IsSuccessStatusCode)
            {
                String result = await response.Content.ReadAsStringAsync();
                Logger.Info($"ReceiveProcessExceptionAsync response: {result}");
                try
                {
                    NotifyRequestExceptionResultDto? jsonResult = JsonConvert.DeserializeObject<NotifyRequestExceptionResultDto>(result);

                    if (jsonResult?.result == true)
                    {
                        var items = jsonResult.data ?? [];
                        foreach (var item in items)
                        {
                            viewModel.TaskID = item.TaskID;
                            if(item.OriginalData != null && item.RequestName != null)
                            {
                                String filePath = Path.Combine(Settings.Default.InvoiceTxnPath[0], $"{item.ChannelName}(Failure)").CheckStoredPath();
                                filePath = Path.Combine(filePath, item.RequestName.Substring(item.RequestName.IndexOf('_') + 1));
                                await System.IO.File.WriteAllBytesAsync(filePath, Convert.FromBase64String(item.OriginalData));
                                await System.IO.File.WriteAllTextAsync($"{filePath}.err", item.ExceptionMessage);
                            }
                            await CommitProcessExceptionAsync(viewModel);
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
        private async Task CommitProcessExceptionAsync(InvoiceRequestViewModel viewModel)
        {
            String apiUrl = $"{ServerInspector.ServiceInfo.TaskCenterUrl}/InvoiceData/CommitProcessException";

            var response = await InvoiceWatcher.TheHttpClient.PostAsJsonAsync(apiUrl, viewModel);

            if (response.IsSuccessStatusCode)
            {

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
