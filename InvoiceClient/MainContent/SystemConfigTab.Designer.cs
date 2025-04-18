﻿namespace InvoiceClient.MainContent
{
    partial class SystemConfigTab
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SellerReceiptNo = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.btnTxnPath = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.btnServerUrl = new System.Windows.Forms.Button();
            this.ServerUrl = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.InvoiceTxnPath = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // SellerReceiptNo
            // 
            this.SellerReceiptNo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.SellerReceiptNo.Location = new System.Drawing.Point(0, 176);
            this.SellerReceiptNo.Name = "SellerReceiptNo";
            this.SellerReceiptNo.ReadOnly = true;
            this.SellerReceiptNo.Size = new System.Drawing.Size(590, 35);
            this.SellerReceiptNo.TabIndex = 33;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label7.Location = new System.Drawing.Point(0, 145);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(188, 29);
            this.label7.TabIndex = 32;
            this.label7.Text = "Store Number：";
            // 
            // btnTxnPath
            // 
            this.btnTxnPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTxnPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnTxnPath.Location = new System.Drawing.Point(659, 98);
            this.btnTxnPath.Name = "btnTxnPath";
            this.btnTxnPath.Size = new System.Drawing.Size(154, 39);
            this.btnTxnPath.TabIndex = 22;
            this.btnTxnPath.Text = "Set";
            this.btnTxnPath.UseVisualStyleBackColor = true;
            this.btnTxnPath.Click += new System.EventHandler(this.btnTxnPath_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label2.Location = new System.Drawing.Point(0, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(435, 29);
            this.label2.TabIndex = 21;
            this.label2.Text = "Electronic Invoice Transaction Folder：";
            // 
            // btnServerUrl
            // 
            this.btnServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnServerUrl.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnServerUrl.Location = new System.Drawing.Point(659, 26);
            this.btnServerUrl.Name = "btnServerUrl";
            this.btnServerUrl.Size = new System.Drawing.Size(154, 39);
            this.btnServerUrl.TabIndex = 20;
            this.btnServerUrl.Text = "Confirm";
            this.btnServerUrl.UseVisualStyleBackColor = true;
            this.btnServerUrl.Click += new System.EventHandler(this.btnServerUrl_Click);
            // 
            // ServerUrl
            // 
            this.ServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ServerUrl.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.ServerUrl.Location = new System.Drawing.Point(0, 32);
            this.ServerUrl.Name = "ServerUrl";
            this.ServerUrl.Size = new System.Drawing.Size(642, 35);
            this.ServerUrl.TabIndex = 19;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(306, 29);
            this.label1.TabIndex = 18;
            this.label1.Text = "Server-Side Service URL：";
            // 
            // InvoiceTxnPath
            // 
            this.InvoiceTxnPath.AutoSize = true;
            this.InvoiceTxnPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.InvoiceTxnPath.Location = new System.Drawing.Point(0, 100);
            this.InvoiceTxnPath.Name = "InvoiceTxnPath";
            this.InvoiceTxnPath.Size = new System.Drawing.Size(0, 29);
            this.InvoiceTxnPath.TabIndex = 4;
            // 
            // SystemConfigTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.InvoiceTxnPath);
            this.Controls.Add(this.SellerReceiptNo);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.btnTxnPath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnServerUrl);
            this.Controls.Add(this.ServerUrl);
            this.Controls.Add(this.label1);
            this.Name = "SystemConfigTab";
            this.Size = new System.Drawing.Size(827, 628);
            this.VisibleChanged += new System.EventHandler(this.SystemConfigTab_VisibleChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox SellerReceiptNo;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnTxnPath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnServerUrl;
        private System.Windows.Forms.TextBox ServerUrl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label InvoiceTxnPath;



    }
}
