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
    public partial class InvoiceNotificationManager : GenericEntityRepository<ApplicationDbContext, SMSNotificationQueue>, INotification
    {
        public InvoiceNotificationManager() : base() { }
        public InvoiceNotificationManager(GenericDbContext<ApplicationDbContext> manager) : base(manager) { }

        public void ProcessMessage()
        {
            var smsQueue = this.GetTable<SMSNotificationQueue>();
            DateTime now = DateTime.Now;
            foreach (var item in this.GetTable<DocumentReplication>().Select(r => r.Doc)
                .Where(d => d.DocType == (int)Naming.DocumentTypeDefinition.E_Invoice
                    && d.DocumentOwner!.Owner.OrganizationStatus.SetToNotifyCounterpartBySMS == true
                    && d.InvoiceItem!.InvoiceBuyer!.ReceiptNo!.Equals("0000000000")).ToList())
            {
                if (!smsQueue.Any(q => q.DocID == item.DocID))
                {
                    smsQueue.Add(new SMSNotificationQueue
                    {
                        DocID = item.DocID,
                        MessageID = (int)Naming.MessageTypeDefinition.發票開立通知,
                        SubmitDate = now
                    });
                    this.SubmitChanges();
                }
            }

            this.ExecuteCommand("delete from DocumentReplication");
            sendSMS(null);
        }

        private void sendSMS(object? stateInfo)
        {
            var logs = this.GetTable<SMSNotificationLog>();
            var items = this.GetTable<SMSNotificationQueue>()
                .Where(q => q.MessageID == (int)Naming.MessageTypeDefinition.發票開立通知);
            if (items.Count() > 0 && BuildMessageContent != null)
            {
                SMSHelper sms = new SMSHelper();
                if (sms.Start())
                {
                    foreach (var item in items.ToList())
                    {
                        var msg = BuildMessageContent(item.DocID);
                        var mobile = item.Doc.GetCounterpartMobile();
                        if (!String.IsNullOrEmpty(mobile))
                        {
                            if (sms.SendSMS(msg.Subject, msg.Content, mobile) && sms.TotalUnsent == 0)
                            {
                                logs.Add(new SMSNotificationLog
                                {
                                    DocID = item.DocID,
                                    MessageID = (int)Naming.MessageTypeDefinition.發票開立通知,
                                    SubmitDate = DateTime.Now,
                                    SendingContent = msg.Content,
                                    SendingMobil = mobile
                                });
                                this.SubmitChanges();
                            }
                            else if (ExceptionHandler != null)
                            {
                                ExceptionHandler(item.DocID, sms.GetDeliveryStatus(), null!);
                            }
                        }
                        this.EntityList.Remove(item);
                        this.SubmitChanges();
                    }
                    sms.Close();
                }
            }
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
