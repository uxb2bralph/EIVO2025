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


namespace InvoiceClient.Agent.JsonHelper
{
    public class JsonDataWatcher<T> : InvoiceWatcher
    {

        public JsonDataWatcher(String fullPath)
            : base(fullPath)
        {

        }

        public Naming.InvoiceProcessType? ResponsibleProcessType { get; set; }

        public String ApiUrl { get; set; } = String.Empty;

        protected T? _result;

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
                _result = result;

                if (result == null)
                {
                    storeFile(fullPath, Path.Combine(_failedTxnPath, fileName));
                    processError("Result is null", null, fileName);
                }
                else
                {
                    File.WriteAllText(Path.Combine(_ResponsedPath, $"{Path.GetFileNameWithoutExtension(fileName)}_Response.json"), result.JsonStringify());
                    storeFile(fullPath, Path.Combine(Logger.LogDailyPath, fileName));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                storeFile(fullPath, Path.Combine(_failedTxnPath, fileName));
                LogFault(ex.Message, _failedTxnPath, fileName);
            }
        }

        protected override void LogFault(string fault, string failedPath, string fileName)
        {

        }

        protected T? UploadJsonTo(String jsonData, String url, IEnumerable<KeyValuePair<String, String>>? queryParams = null)
        {
            try
            {
                if (queryParams != null && queryParams.Any())
                {
                    String queryString = String.Join("&", queryParams.Select(kv =>
                        $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
                    url = $"{url}?{queryString}";
                }

                StringContent content = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");

                Task<HttpResponseMessage> task = TheHttpClient.PostAsync(url, content);
                task.Wait();
                HttpResponseMessage response = task.Result;
                response.EnsureSuccessStatusCode();
                Task<String> data = response.Content.ReadAsStringAsync();
                data.Wait();
                return JsonConvert.DeserializeObject<T>(data.Result);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return default(T);
            }
        }

        protected virtual T? processUpload(String requestFile)
        {
            return UploadJsonTo(requestFile, ApiUrl);
        }

        protected override void processError(string message, XmlDocument? docInv, string fileName)
        {
            Logger.Warn($"upload file ({fileName}) at fault, cause:\r\n{message}");
        }

    }

}
