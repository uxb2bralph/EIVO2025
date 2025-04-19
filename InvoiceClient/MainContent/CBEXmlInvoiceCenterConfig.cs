using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using InvoiceClient.Agent;
using InvoiceClient.Properties;
using InvoiceClient.Helper;
using InvoiceClient.TransferManagement;

namespace InvoiceClient.MainContent
{
    public partial class CBEXmlInvoiceCenterConfig : UserControl , ITabWorkItem
    {
        private ITransferManager _transferMgr;

        public CBEXmlInvoiceCenterConfig()
        {
            InitializeComponent();
            _transferMgr = InvoiceClientTransferManager.GetTransferManagerForCurrentUI(typeof(CBEXmlInvoiceCenterConfig));
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            btnSend.Enabled  = false;
            btnPause.Enabled = true;
            InvoiceClientTransferManager.StartUp(Settings.Default.InvoiceTxnPath[0]);
            JobStatus.Text = "系統狀態：執行中...";
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            btnSend.Enabled = true;
            btnPause.Enabled = false;
            _transferMgr.PauseAll();
            JobStatus.Text = "系統狀態：已停止...";
        }

        private void btnRetry_Click(object sender, EventArgs e)
        {
            _transferMgr.SetRetry();
            FailedInvoiceInfo.Text = String.Empty;
            btnRetry.Enabled = false;
            MessageBox.Show("已重新執行!!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshContent();
        }

        public void RefreshContent()
        {
            FailedInvoiceInfo.Text = _transferMgr.ReportError();
            btnRetry.Enabled = !String.IsNullOrEmpty(FailedInvoiceInfo.Text);
        }

        private void InvoiceCenterConfig_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
                RefreshContent();
        }

        public string TabName
        {
            get { return "CBEXmlInvoiceTab"; }
        }

        public string TabText
        {
            get { return "境外電商電子發票"; }
        }

        public void ReportStatus()
        {

        }
    }
}
