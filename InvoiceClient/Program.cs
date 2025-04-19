using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Collections;
using System.Configuration.Install;
using System.ServiceProcess;
using System.IO;
using InvoiceClient.Properties;
using System.Threading;
using InvoiceClient.Agent;
using ModelCore.Resource;
using InvoiceClient.Helper;
using InvoiceClient.Agent.POSHelper;
using InvoiceClient.TransferManagement;
using System.Net;
using CommonLib.Core.Utility;
using CommonLib.Utility;

namespace InvoiceClient
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {     
            /// SSL憑證信任設定
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                | SecurityProtocolType.Tls11
                | SecurityProtocolType.Tls /*| SecurityProtocolType.Ssl3*/;

            if (!String.IsNullOrEmpty(Settings.Default.AppCulture))
            {
                Thread.CurrentThread.CurrentUICulture = MessageResources.Culture = System.Globalization.CultureInfo.GetCultureInfo(Settings.Default.AppCulture);
            }

            if (Settings.Default.ClearTxnPath)
            {
                ClearDirectory(Settings.Default.InvoiceTxnPath[0]);

                if (AppSettings.Default.InvoiceTxnPath?.Length > 0)
                {
                    foreach (var item in AppSettings.Default.InvoiceTxnPath)
                    {
                        ClearDirectory(item);
                    }
                }
            }

            if(Environment.UserInteractive)
            {
                if (AppSigner.SignerCertificate == null)
                {
                    if (String.IsNullOrEmpty(Settings.Default.ActivationKey))
                    {
                        if (!InitializeActivation())
                        {
                            MessageBox.Show("Could not create identification!!", "Activation failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Application.Exit();
                            return;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid Signer Certificate !!", "Activation failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Application.Exit();
                        return;
                    }
                }
            }

            if (Environment.UserInteractive /*|| Debugger.IsAttached*/
                && AppSettings.Default.UseMainForm
                && AppSettings.Default.InvoiceTxnPath?.Length == 1)
            {
                Application.EnableVisualStyles();
                //Application.SetCompatibleTextRenderingDefault(false);

                var form = new MainForm
                {
                    //WindowState = FormWindowState.Minimized,
                    //ShowInTaskbar = false
                };
                if(!POSReady.Settings.UserClose)
                {
                    form.WindowState = FormWindowState.Minimized;
                    form.ShowInTaskbar = false;
                }
                Application.Run(form);
            }
            else
            {
                if (AppSigner.SignerCertificate == null)
                {
                    Logger.Error("Signer Certificate not ready, Please Check...");
                    Application.Exit();
                    return;
                }

                if (ServiceController.GetServices().Where(s => s.ServiceName == Settings.Default.ServiceName).Any())
                {
                    ServiceBase[] services =
                    {
                        new InvoiceClientService()
                    };
                    ServiceBase.Run(services);
                }
                else
                {
                    Application.Run(new MyApplicationContext(() => 
                    {
                        if (AppSettings.Default.InvoiceTxnPath?.Length > 0)
                        {
                            foreach (var item in AppSettings.Default.InvoiceTxnPath)
                            {
                                InvoiceClientTransferManager.StartUp(item);
                            }
                        }
                        else
                        {
                            InvoiceClientTransferManager.StartUp(Settings.Default.InvoiceTxnPath[0]);
                        }
                    }));
                }
            }
            
        }

        class MyApplicationContext : ApplicationContext
        {
            public MyApplicationContext(Action action)
            {
                if(action != null)
                {
                    action();
                }
            }
        }

        internal static bool InitializeActivation()
        {
            String actKey = Microsoft.VisualBasic.Interaction.InputBox("New input identification code:", "Enable the system");
            if (!String.IsNullOrEmpty(actKey) && InvoiceClient.Helper.AppSigner.ResetCertificate(actKey))
            {
                InvoiceClient.Helper.AppSigner.InstallRootCA();
                MessageBox.Show("New input identification code!!", "Enable the system", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            return false;
        }


        internal static void Install(bool undo, string[] args)
        {
            try
            {
                using (Installer inst = new Installer())
                {
                    IDictionary state = new Hashtable();
                    try
                    {
                        if (undo)
                        {
                            inst.Uninstall(state);
                            AppSettings.Default.InstalledService = false;
                            AppSettings.Default.Save();
                            MessageBox.Show("服務已移除!!", "服務設定", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            inst.Install(state);
                            inst.Commit(state);
                            AppSettings.Default.InstalledService = false;
                            AppSettings.Default.Save();
                            MessageBox.Show("服務安裝成功!!", "服務設定", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch
                    {
                        try
                        {
                            inst.Rollback(state);
                        }
                        catch
                        {
                        }
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                MessageBox.Show("服務安裝失敗:\r\n" + ex.Message, "服務設定", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void ClearDirectory(String path)
        {
            path.CheckStoredPath();
            foreach (var item in Directory.GetDirectories(path))
            {
                try
                {
                    Directory.Delete(item, true);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
