using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;

using InvoiceClient.Agent;
using InvoiceClient.Helper;
using InvoiceClient.Properties;
using ModelCore.Schema.EIVO;
using CommonLib.Core.Utility;
using CommonLib.Utility;
using InvoiceClient.TransferManagement;
using System.Threading.Tasks;
using InvoiceClient.Agent.POSHelper;
using InvoiceClient.Agent.RuntimeHelper;

namespace InvoiceClient
{
    public partial class MainForm : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static MainForm? AppMainForm
        {
            get;
            private set;
        }

        public static void Alert(string message,String caption,MessageBoxIcon icon = MessageBoxIcon.Warning,ITransferManager transferManager = null)
        {
            if (AppMainForm != null)
            {
                Task.Run(() =>
                {
                    MessageBox.Show(message, $"[{DateTime.Now}]{caption}", MessageBoxButtons.OK, icon);
                });

                if (AppMainForm.Visible)
                {
                    transferManager?.WorkItem?.ReportStatus();
                }
            }
        }

        public MainForm()
        {
            InitializeComponent();
            this.invoiceClientServiceController.ServiceName = Settings.Default.ServiceName;
            initializeWorkItem();
            AppMainForm = this;
        }

        private void initializeWorkItem()
        {
            foreach (Type type in InvoiceClient.TransferManagement.InvoiceClientTransferManager.AllMainTabs)
            {
                try
                {
                    if (type != null)
                        createTab(type);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }


            foreach (var manager in InvoiceClient.TransferManagement.InvoiceClientTransferManager.AllTransferManager)
            {
                try
                {
                    Type type = manager.UIConfigType;
                    if (type != null)
                    {
                        manager.WorkItem = createTab(type);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            foreach (var inspector in InvoiceClient.TransferManagement.InvoiceClientTransferManager.AllServerInspector)
            {
                try
                {
                    Type type = inspector.UIConfigType;
                    if (type != null)
                        createTab(type);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        private ITabWorkItem createTab(Type type)
        {
            if (type.GetInterface("InvoiceClient.Helper.ITabWorkItem") != null)
            {
                ITabWorkItem workItem = (ITabWorkItem)type.Assembly.CreateInstance(type.FullName);
                Control c = (Control)workItem;
                if (c != null)
                {
                    var tab = new TabPage();
                    tab.SuspendLayout();
                    this.tabControl1.Controls.Add(tab);
                    tab.Controls.Add((Control)workItem);
                    tab.Name = workItem.TabName;
                    tab.Padding = new System.Windows.Forms.Padding(3);
                    tab.Text = workItem.TabText;
                    tab.UseVisualStyleBackColor = true;

                    c.Dock = System.Windows.Forms.DockStyle.Fill;
                    tab.ResumeLayout();

                    return workItem;
                }
            }

            return null;
        }

        private void miClose_Click(object sender, EventArgs e)
        {
            //this.Close();
            Settings.Default.Save();
            Application.Exit();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //this.ShowInTaskbar = false;
            this.Text = Settings.Default.AppTitle;
            notifyIcon.Visible = true;

            InvoiceClientTransferManager.StartUp(Settings.Default.InvoiceTxnPath[0]);

//            this.Hide();
        }

        private void miRestore_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }

        private void MainForm_Deactivate(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
                //this.Hide();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.Save();
            if(!POSReady.Settings.UserClose)
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;

                    this.WindowState = FormWindowState.Minimized;
                    this.ShowInTaskbar = false;
                }
            }
        }

        private void miActivate_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Please determine whether to establish the connection service with the new identification code?", "Enable the system", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                String actKey = Microsoft.VisualBasic.Interaction.InputBox("New input identification code:", "Enable the system");
                if (!String.IsNullOrEmpty(actKey) && InvoiceClient.Helper.AppSigner.ResetCertificate(actKey))
                {
                    MessageBox.Show("Connection service is enabled!!", "Enable the system", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

    }
}
