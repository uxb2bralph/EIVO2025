using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

using InvoiceClient.Helper;
using InvoiceClient.Properties;
using InvoiceClient.TransferManagement;
using ModelCore.Locale;
using ModelCore.Schema.EIVO;
using ModelCore.Schema.EIVO.B2B;
using ModelCore.Schema.TXN;
using Newtonsoft.Json;
using CommonLib.Core.Utility;
using CommonLib.Utility;


namespace InvoiceClient.Agent
{
    public class ProcessRequestWatcher : InvoiceWatcher
    {

        public ProcessRequestWatcher(String fullPath)
            : base(fullPath)
        {

        }

        public Naming.InvoiceProcessType? ResponsibleProcessType { get; set; }

        protected override void prepareStorePath(string fullPath)
        {
            _ResponsedPath = fullPath + "(Response)";
            _ResponsedPath.CheckStoredPath();

            _failedTxnPath = fullPath + "(Failure)";
            _failedTxnPath.CheckStoredPath();

            if (__FailedTxnPath != null)
            {
                __FailedTxnPath.Add(_failedTxnPath);
            }

            _inProgressPath = Path.Combine(Logger.LogPath, Path.GetFileName(fullPath), $"{Process.GetCurrentProcess().Id}");
            _inProgressPath.CheckStoredPath();
        }

        protected override void processFile(string invFile)
        {
            if (!File.Exists(invFile))
                return;

            String fileName = Path.GetFileName(invFile);
            String fullPath = Path.Combine(_inProgressPath, fileName);
            try
            {
                File.Move(invFile, fullPath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return;
            }

            try
            {
                var result = processUpload(fullPath);

                if (result.result != true)
                {
                    storeFile(fullPath, Path.Combine(_failedTxnPath, fileName));
                    processError(result.message, null, fileName);
                }
                else
                {
                    storeFile(fullPath, Path.Combine(Logger.LogDailyPath, fileName));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                storeFile(fullPath, Path.Combine(_failedTxnPath, fileName));
            }
        }

        protected JsonResult UploadTo(String requestFile, String url, IEnumerable<KeyValuePair<String,String>> queryParams = null)
        {
            try
            {
                using(FileStream stream = File.OpenRead(requestFile))
                {
                    MultipartFormDataContent content = new MultipartFormDataContent();
                    if (queryParams != null && queryParams.Count() > 0)
                    {
                        foreach (var kv in queryParams)
                        {
                            content.Add(new StringContent(kv.Value), kv.Key);
                        }
                        //content.Add(new FormUrlEncodedContent(queryParams));
                    }

                    String fileName = Path.GetFileName(requestFile);
                    StreamContent fileStream = new StreamContent(stream);
                    // Replace the problematic line with the following:
                    fileStream.Headers.Add("Content-Type", fileName.GetMimeType());
                    content.Add(fileStream, "attachment", fileName);

                    Task<HttpResponseMessage> task = TheHttpClient.PostAsync(url, content);
                    task.Wait();
                    HttpResponseMessage response = task.Result;
                    response.EnsureSuccessStatusCode();
                    Task<String> data = response.Content.ReadAsStringAsync();
                    data.Wait();
                    return JsonConvert.DeserializeObject<JsonResult>(data.Result);
                }
            }
            catch(Exception ex)
            {
                Logger.Error(ex);

                return new JsonResult 
                {
                    result = false,
                    message = ex.ToString(),
                };
            }
        }


        //protected JsonResult UploadTo(String requestFile, String url, DynamicQueryStringParameter queryParams = null)
        //{
        //    using (WebClientEx client = new WebClientEx())
        //    {
        //        client.Timeout = 43200000;
        //        client.Encoding = Encoding.UTF8;

        //        if (queryParams != null)
        //        {
        //            url += queryParams.ToQueryString();
        //        }

        //        byte[] data = client.UploadFile(url, requestFile);
        //        return JsonConvert.DeserializeObject<JsonResult>(client.Encoding.GetString(data));

        //        //using (var write = client.OpenWrite(url))
        //        //{
        //        //    using (var file = File.OpenRead(requestFile))
        //        //    {
        //        //        file.CopyTo(write);
        //        //    }

        //        //    var response = client.Response;
        //        //    using (StreamReader reader = new StreamReader(response.GetResponseStream(), client.Encoding))
        //        //    {
        //        //        return JsonConvert.DeserializeObject<JsonResult>(reader.ReadToEnd());
        //        //    }
        //        //}
        //    }
        //}

        protected virtual JsonResult processUpload(String requestFile)
        {
            return UploadTo(requestFile, $"{ServerInspector.ServiceInfo.TaskCenterUrl}/InvoiceData/UploadProcessRequest?keyID={HttpUtility.UrlEncode(ServerInspector.ServiceInfo.AgentToken)}&sender={ServerInspector.ServiceInfo.AgentUID}&processType={(int?)ResponsibleProcessType}");
        }

        protected override void processError(string message, XmlDocument docInv, string fileName)
        {
            Logger.Warn($"upload file ({fileName}) at fault, cause:\r\n{message}");
        }

    }

    public class JsonResult
    {
        public bool? result { get; set; }
        public String message { get; set; }
    }
}
