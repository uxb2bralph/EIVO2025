using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using InvoiceClient.Helper;
using InvoiceClient.Properties;

namespace InvoiceClient.MainContent
{
    public partial class SystemConfigTab : UserControl, ITabWorkItem
    {
        public SystemConfigTab()
        {
            InitializeComponent();
        }

        private void btnServerUrl_Click(object sender, EventArgs e)
        {
            Settings.Default.InvoiceClient_WS_Invoice_eInvoiceServiceClient = ServerUrl.Text;
            String actKey = Microsoft.VisualBasic.Interaction.InputBox("New input identification code:", "Enable the system", Settings.Default.ActivationKey);
            if (!String.IsNullOrEmpty(actKey) && InvoiceClient.Helper.AppSigner.ResetCertificate(actKey))
            {
                MessageBox.Show("Setup is complete!!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Connection failed!!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public string TabName
        {
            get { return "SystemConfigTab"; }
        }

        public string TabText
        {
            get { return "System Setting"; }
        }
        public void ReportStatus()
        {

        }
        private void SystemConfigTab_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                ServerUrl.Text = Settings.Default.InvoiceClient_WS_Invoice_eInvoiceServiceClient;
                InvoiceTxnPath.Text = Settings.Default.InvoiceTxnPath[0];
                SellerReceiptNo.Text = Settings.Default.SellerReceiptNo;
            }

        }

        private void btnTxnPath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = Settings.Default.InvoiceTxnPath[0];
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Settings.Default.InvoiceTxnPath[0] = dialog.SelectedPath;
                    MessageBox.Show("Setup is complete!!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Restart();
                }
            }
        }
    }
}
