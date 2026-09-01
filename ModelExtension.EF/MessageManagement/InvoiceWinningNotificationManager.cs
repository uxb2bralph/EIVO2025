using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonLib.Core.DataWork;
using CommonLib.Utility;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.Locale;

namespace ModelCore.MessageManagement
{
    /// <summary>
    /// 由 LINQ to SQL (MessageEntityDataContext) 遷移至 EF Core；
    /// 相關資料表 (SMSNotificationQueue / SMSNotificationLog) 已納入 ApplicationDbContext。
    /// </summary>
    public partial class InvoiceWinningNotificationManager : GenericEntityRepository<ApplicationDbContext, SMSNotificationQueue>, INotification
    {
        public InvoiceWinningNotificationManager() : base() { }
        public InvoiceWinningNotificationManager(GenericDbContext<ApplicationDbContext> manager) : base(manager) { }

        public int Year { get; set; }
        public int MonthFrom { get; set; }
        public int MonthTo { get; set; }

        public void ProcessMessage()
        {
            var smsQueue = this.GetTable<SMSNotificationQueue>();
            DateTime now = DateTime.Now;

            // 舊版 MessageEntity dbml 直接對應 InvoiceWinningNumber 的 Year/MonthFrom/MonthTo 欄位；
            // EF 模型改由 UniformInvoiceWinningNumber (Year/Period) 取得，期別 = 迄月 / 2。
            int period = MonthTo / 2;
            var winningInvoices = this.GetTable<InvoiceWinningNumber>()
                .Where(n => n.Winning!.Year == Year && n.Winning.Period == period)
                .Select(n => n.Invoice)
                .Where(n => n.Seller!.OrganizationStatus.SetToNotifyCounterpartBySMS == true);

            foreach (var item in winningInvoices.ToList())
            {
                if (!String.IsNullOrEmpty(item.InvoiceBuyer?.Phone))
                {
                    if (!smsQueue.Any(q => q.DocID == item.InvoiceID))
                    {
                        smsQueue.Add(new SMSNotificationQueue
                        {
                            DocID = item.InvoiceID,
                            MessageID = (int)Naming.MessageTypeDefinition.發票中獎通知,
                            SubmitDate = now
                        });
                        this.SubmitChanges();
                    }
                }
            }

            sendSMS(null);
        }

        private void sendSMS(object? stateInfo)
        {
            var logs = this.GetTable<SMSNotificationLog>();
            var items = this.GetTable<SMSNotificationQueue>()
                .Where(q => q.MessageID == (int)Naming.MessageTypeDefinition.發票中獎通知);
            if (items.Count() > 0)
            {
                SMSHelper sms = new SMSHelper();
                if (sms.Start())
                {
                    String? mobile;
                    foreach (var item in items.ToList())
                    {
                        var msg = buildMessageContent(item.DocID, out mobile);
                        if (!String.IsNullOrEmpty(mobile))
                        {
                            if (sms.SendSMS("發票中獎通知", msg, mobile) && sms.TotalUnsent == 0)
                            {
                                logs.Add(new SMSNotificationLog
                                {
                                    DocID = item.DocID,
                                    MessageID = (int)Naming.MessageTypeDefinition.發票中獎通知,
                                    SubmitDate = DateTime.Now,
                                    SendingContent = msg,
                                    SendingMobil = mobile
                                });
                                this.SubmitChanges();
                            }
                            else if (ExceptionHandler != null)
                            {
                                ExceptionHandler(item.DocID, sms.GetDeliveryStatus(), msg);
                            }
                        }
                        this.EntityList.Remove(item);
                        this.SubmitChanges();
                    }
                    sms.Close();
                }
            }
        }

        private String buildMessageContent(int docID, out String? mobile)
        {
            InvoiceItem item = this.GetTable<InvoiceItem>().Where(i => i.InvoiceID == docID).First();
            mobile = item.InvoiceBuyer?.Phone;
            StringBuilder msg = new StringBuilder();
            msg.Append(item.InvoiceSeller?.CustomerName).Append("通知\r\n");
            msg.Append("您的").Append(((Naming.B2BInvoiceDocumentTypeDefinition)item.CDS_Document.DocType).ToString());
            msg.Append(String.Format("{0}{1}", item.TrackCode, item.No)).Append("已中獎\r\n");
            msg.Append("紙本發票將於10日內寄給您\r\n");
            msg.Append("備註:").Append(String.Join("", item.Product.Select(d => d.InvoiceProductItem.FirstOrDefault()?.Remark)));
            return msg.ToString();
        }

        public Func<int, NotificationMesssage>? BuildMessageContent
        {
            get;
            set;
        }

        public Action<int, String, String>? ExceptionHandler
        {
            get;
            set;
        }
    }
}
