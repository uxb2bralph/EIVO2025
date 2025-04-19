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
using InvoiceClient.Agent.POSHelper;
using CommonLib.Core.Utility;
using CommonLib.Utility;
using System.Drawing.Printing;

namespace InvoiceClient.MainContent
{
    public partial class POSChannelConfig : UserControl, ITabWorkItem
    {
        private ITransferManager _transferMgr;
        private bool showDefaultPrinter = false;
        public POSChannelConfig()
        {
            InitializeComponent();
            _transferMgr = InvoiceClientTransferManager.GetTransferManager(typeof(POSInvoiceTransferManager));
            String printerName = null;
            using (PrintDocument doc = new PrintDocument())
            {
                printerName = POSReady.Settings.DefaultPOSPrinter ??
                    doc.DefaultPageSettings.PrinterSettings.PrinterName;
            }
            int selected = 0;
            for (int i = 0; i < PrinterSettings.InstalledPrinters.Count; i++)
            {
                var item = PrinterSettings.InstalledPrinters[i];
                PrinterList.Items.Add(item);
                if (printerName == item)
                {
                    selected = i;
                }
            }
            PrinterList.SelectedIndex = selected;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            btnSend.Enabled = false;
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
            btnUserPrinter.Text = $"列印({(POSReady.Settings.UserPOSPrinter ? "作用中" : "已暫停")})";
            SellerNo.Text = $"統編：{POSReady.Settings.SellerReceiptNo}";
            SetToPreload.Text = $"發票取號預載({(POSReady.Settings.PreloadInvoiceNo ? "作用中" : "已暫停")})";
        }

        private void POSChannelConfig_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                showDefaultPrinter = true;
                RefreshContent();
            }

            if (POSReady.Settings.SellerReceiptNo == null)
            {
                ResetSellerReceiptNo();
                if (this.Visible)
                {
                    RefreshContent();
                }
            }

        }

        public string TabName
        {
            get { return "POSChannelTab"; }
        }

        public string TabText
        {
            get { return "POS電子發票"; }
        }
        public void ReportStatus()
        {
            this.Invoke(new Action(() =>
            {
                FailedInvoiceInfo.Text = _transferMgr.ReportError();
            }));
        }
        private void btnUserPrinter_Click(object sender, EventArgs e)
        {
            POSReady.Settings.UserPOSPrinter = !POSReady.Settings.UserPOSPrinter;
            POSReady.Settings.Save();
            RefreshContent();
            MessageBox.Show("變更設定需重新啟動!!");
        }

        private void btnResetReceiptNo_Click(object sender, EventArgs e)
        {
            ResetSellerReceiptNo();
            RefreshContent();
        }

        private void ResetSellerReceiptNo()
        {
            POSReady.Settings.SellerReceiptNo = Microsoft.VisualBasic.Interaction.InputBox("請輸入營業人統一編號：", "重設統一編號", POSReady.Settings.SellerReceiptNo).GetEfficientString();
            POSReady.Settings.Save();
        }

        private void PrinterList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (PrinterList.SelectedIndex >= 0)
            {
                var selectedValue = PrinterList.Items[PrinterList.SelectedIndex].ToString();
                if (Win32.Winspool.SetDefaultPrinter(selectedValue))
                {
                    POSReady.Settings.DefaultPOSPrinter = selectedValue;
                    POSReady.Settings.Save();
                    if (showDefaultPrinter)
                    {
                        MessageBox.Show(String.Format("已設定預設印表機 => {0}", selectedValue), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    String msg = String.Format("預設印表機設定失敗 => {0}", selectedValue);
                    MessageBox.Show(msg, "無法選擇印表機", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SetToPreload_Click(object sender, EventArgs e)
        {
            POSReady.Settings.PreloadInvoiceNo = !POSReady.Settings.PreloadInvoiceNo;
            POSReady.Settings.Save();
            RefreshContent();
        }

    }
}
