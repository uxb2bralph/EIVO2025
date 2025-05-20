using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

using InvoiceClient.Agent;
using InvoiceClient.Helper;
using InvoiceClient.Properties;
using ModelCore.DataEntity;
using ModelCore.Schema.EIVO;
using ModelCore.Schema.TXN;
using Newtonsoft.Json;
using CommonLib.Core.Utility;
using CommonLib.Utility;

namespace InvoiceClient.TransferManagement
{

    public abstract class ServerInspector
    {
        protected bool _isRunning;
        public abstract void StartUp();
        public abstract Type UIConfigType { get; }
        public ServerInspector? ChainedInspector
        { get; set; }
        public virtual void ExecutiveService(List<String> pathInfo)
        {
            if (ChainedInspector != null)
            {
                ChainedInspector.ExecutiveService(pathInfo);
            }
        }

        public static Organization GetRegisterdMember()
        {
            using (eInvoiceServiceClient invSvc = InvoiceWatcher.CreateInvoiceService())
            {

                try
                {
                    Root token = invSvc.CreateMessageToken("讀取用戶端資料");
                    XmlNode doc = invSvc.GetRegisteredMember(token.ConvertToXml().Sign());
                    if (doc != null)
                    {
                        return doc.DeserializeDataContract<ModelCore.DataEntity.Organization>();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            return null;
        }

        public static ServiceInfo @ServiceInfo
        {
            get;
            private set;
        }
        public static void PrepareServiceInfo()
        {
            using (eInvoiceServiceClient invSvc = InvoiceWatcher.CreateInvoiceService())
            {
                var tmpInfo = ServiceInfo ?? AppSettings.Default.ServiceInfo;
                try
                {
                    Root token = invSvc.CreateMessageToken("讀取系統服務資訊");
                    String result = invSvc.GetServiceInfo(token.ConvertToXml().Sign());
                    if (result != null)
                    {
                        Logger.Info("ServerInfo:" + result);
                        AppSettings.Default.ServiceInfo = ServiceInfo = JsonConvert.DeserializeObject<ServiceInfo>(result);
                        if (tmpInfo == null)
                        {
                            AppSettings.Default.Save();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    ServiceInfo = tmpInfo;
                    Task.Run((Action)(() =>
                    {
                        MessageBox.Show($"請檢查網址及網路是否正確!!\r\n{Settings.Default.InvoiceClient_WS_Invoice_eInvoiceServiceClient}", "伺服端連線異常", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    }));
                }
            }

            if (ServiceInfo == null)
            {
                //MessageBox.Show($"伺服端連線異常，請檢查網址及網路是否正確!!\r\n{Settings.Default.InvoiceClient_WS_Invoice_eInvoiceService}", "程式即將終止", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                MessageBox.Show($"用戶端ServiceInfo初始化失敗!!", "程式即將終止", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Process.GetCurrentProcess().Kill();
            }
            //else
            //{
            //    if(!AppSettings.Default.DefaultProcessType.HasValue)
            //    {
            //        AppSettings.Default.DefaultProcessType = ServiceInfo.DefaultProcessType;
            //        AppSettings.Default.Save();
            //    }
            //}
        }


        public static void AcknowledgeServer()
        {
            eInvoiceServiceClient invSvc = InvoiceWatcher.CreateInvoiceService();

            try
            {
                Root token = invSvc.CreateMessageToken("用戶端系統正在執行中");
                invSvc.AcknowledgeLivingReport(token.ConvertToXml().Sign());
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
