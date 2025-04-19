namespace InvoiceClient.MainContent
{
    partial class POSChannelConfig
    {
        /// <summary> 
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 元件設計工具產生的程式碼

        /// <summary> 
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器
        /// 修改這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnRefresh = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.FailedInvoiceInfo = new System.Windows.Forms.TextBox();
            this.btnRetry = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.JobStatus = new System.Windows.Forms.Label();
            this.btnUserPrinter = new System.Windows.Forms.Button();
            this.SellerNo = new System.Windows.Forms.Label();
            this.btnResetReceiptNo = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.PrinterList = new System.Windows.Forms.ListBox();
            this.SetToPreload = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnRefresh
            // 
            this.btnRefresh.AutoSize = true;
            this.btnRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRefresh.Location = new System.Drawing.Point(570, 47);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(135, 40);
            this.btnRefresh.TabIndex = 18;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.FailedInvoiceInfo);
            this.panel1.Location = new System.Drawing.Point(0, 47);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(465, 564);
            this.panel1.TabIndex = 17;
            // 
            // FailedInvoiceInfo
            // 
            this.FailedInvoiceInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FailedInvoiceInfo.Location = new System.Drawing.Point(0, 0);
            this.FailedInvoiceInfo.Multiline = true;
            this.FailedInvoiceInfo.Name = "FailedInvoiceInfo";
            this.FailedInvoiceInfo.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.FailedInvoiceInfo.Size = new System.Drawing.Size(461, 560);
            this.FailedInvoiceInfo.TabIndex = 10;
            this.FailedInvoiceInfo.Text = "Invoice data transfer failed!!";
            // 
            // btnRetry
            // 
            this.btnRetry.Enabled = false;
            this.btnRetry.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRetry.Location = new System.Drawing.Point(473, 47);
            this.btnRetry.Name = "btnRetry";
            this.btnRetry.Size = new System.Drawing.Size(84, 40);
            this.btnRetry.TabIndex = 16;
            this.btnRetry.Text = "Retry";
            this.btnRetry.UseVisualStyleBackColor = true;
            this.btnRetry.Click += new System.EventHandler(this.btnRetry_Click);
            // 
            // btnPause
            // 
            this.btnPause.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnPause.Location = new System.Drawing.Point(570, 0);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(135, 40);
            this.btnPause.TabIndex = 15;
            this.btnPause.Text = "Pause";
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // btnSend
            // 
            this.btnSend.Enabled = false;
            this.btnSend.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSend.Location = new System.Drawing.Point(473, 0);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(84, 40);
            this.btnSend.TabIndex = 14;
            this.btnSend.Text = "Start Up";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // JobStatus
            // 
            this.JobStatus.AutoSize = true;
            this.JobStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.JobStatus.Location = new System.Drawing.Point(1, 0);
            this.JobStatus.Name = "JobStatus";
            this.JobStatus.Size = new System.Drawing.Size(188, 29);
            this.JobStatus.TabIndex = 13;
            this.JobStatus.Text = "System Status：";
            // 
            // btnUserPrinter
            // 
            this.btnUserPrinter.AutoSize = true;
            this.btnUserPrinter.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnUserPrinter.Location = new System.Drawing.Point(473, 94);
            this.btnUserPrinter.Name = "btnUserPrinter";
            this.btnUserPrinter.Size = new System.Drawing.Size(232, 40);
            this.btnUserPrinter.TabIndex = 19;
            this.btnUserPrinter.Text = "列印";
            this.btnUserPrinter.UseVisualStyleBackColor = true;
            this.btnUserPrinter.Click += new System.EventHandler(this.btnUserPrinter_Click);
            // 
            // SellerNo
            // 
            this.SellerNo.AutoSize = true;
            this.SellerNo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.SellerNo.Location = new System.Drawing.Point(473, 144);
            this.SellerNo.Name = "SellerNo";
            this.SellerNo.Size = new System.Drawing.Size(128, 29);
            this.SellerNo.TabIndex = 20;
            this.SellerNo.Text = "ReceiptNo";
            // 
            // btnResetReceiptNo
            // 
            this.btnResetReceiptNo.AutoSize = true;
            this.btnResetReceiptNo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnResetReceiptNo.Location = new System.Drawing.Point(473, 185);
            this.btnResetReceiptNo.Name = "btnResetReceiptNo";
            this.btnResetReceiptNo.Size = new System.Drawing.Size(232, 40);
            this.btnResetReceiptNo.TabIndex = 21;
            this.btnResetReceiptNo.Text = "重設營業人統編";
            this.btnResetReceiptNo.UseVisualStyleBackColor = true;
            this.btnResetReceiptNo.Click += new System.EventHandler(this.btnResetReceiptNo_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.Location = new System.Drawing.Point(473, 284);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(205, 29);
            this.label1.TabIndex = 22;
            this.label1.Text = "選取發票印表機：";
            // 
            // PrinterList
            // 
            this.PrinterList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PrinterList.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.PrinterList.FormattingEnabled = true;
            this.PrinterList.ItemHeight = 29;
            this.PrinterList.Location = new System.Drawing.Point(477, 320);
            this.PrinterList.Name = "PrinterList";
            this.PrinterList.Size = new System.Drawing.Size(336, 294);
            this.PrinterList.TabIndex = 23;
            this.PrinterList.SelectedIndexChanged += new System.EventHandler(this.PrinterList_SelectedIndexChanged);
            // 
            // SetToPreload
            // 
            this.SetToPreload.AutoSize = true;
            this.SetToPreload.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.SetToPreload.Location = new System.Drawing.Point(473, 235);
            this.SetToPreload.Name = "SetToPreload";
            this.SetToPreload.Size = new System.Drawing.Size(232, 40);
            this.SetToPreload.TabIndex = 24;
            this.SetToPreload.Text = "發票號碼預載";
            this.SetToPreload.UseVisualStyleBackColor = true;
            this.SetToPreload.Click += new System.EventHandler(this.SetToPreload_Click);
            // 
            // POSChannelConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.SetToPreload);
            this.Controls.Add(this.PrinterList);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnResetReceiptNo);
            this.Controls.Add(this.SellerNo);
            this.Controls.Add(this.btnUserPrinter);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnRetry);
            this.Controls.Add(this.btnPause);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.JobStatus);
            this.Name = "POSChannelConfig";
            this.Size = new System.Drawing.Size(816, 637);
            this.VisibleChanged += new System.EventHandler(this.POSChannelConfig_VisibleChanged);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnRetry;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Label JobStatus;
        private System.Windows.Forms.Button btnUserPrinter;
        private System.Windows.Forms.Label SellerNo;
        private System.Windows.Forms.Button btnResetReceiptNo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox PrinterList;
        private System.Windows.Forms.Button SetToPreload;
        private System.Windows.Forms.TextBox FailedInvoiceInfo;
    }
}
