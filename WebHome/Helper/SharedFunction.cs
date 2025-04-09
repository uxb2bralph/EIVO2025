using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Web;
using WebHome.Helper;
using WebHome.Models.ViewModel;
using ModelCore.Models.ViewModel;
using WebHome.Properties;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;
using ModelCore.Security.MembershipManagement;
using ModelCore.MessageManagement;
using CommonLib.Core.Utility;

namespace WebHome.Helper
{
    public static class SharedFunction
    {
        #region "Using Thread to Send Notify Mail"
        /// <summary>
        /// 定義類別參數
        /// </summary>
        public class _MailQueryState
        {
            public int setYear { get; set; }
            public int setPeriod { get; set; }
            public string allInvoiceID { get; set; }
        }

        /// <summary>
        /// 外部呼叫執行簡訊通知
        /// </summary>
        /// <param name="mailState"></param>
        public static void doSendSMSMessage(_MailQueryState mailState)
        {
            ThreadPool.QueueUserWorkItem(doSendSMSMessage, mailState);
        }

        private static void doSendSMSMessage(object stateInfo)
        {
            _MailQueryState state = (_MailQueryState)stateInfo;
            int year = state.setYear;
            int period = state.setPeriod;
            int smonth = (period * 2) - 1;
            int emonth = period * 2;
            try
            {
                InvoiceWinningNotificationManager smsMgr = new InvoiceWinningNotificationManager();
                smsMgr.Year = year;
                smsMgr.MonthFrom = smonth;
                smsMgr.MonthTo = emonth;
                smsMgr.ExceptionHandler = AlertSMSError;

                smsMgr.ProcessMessage();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        #endregion

        public static void MailWebPage(this String url, String mailTo, String subject, CustomSmtpHost smtpSettings = null, params string[] attachment)
        {

            System.Net.Mail.Attachment[] items = null;

            if (attachment != null && attachment.Length > 0)
            {
                items = attachment.Where(f => File.Exists(f))
                    .Select(f => new System.Net.Mail.Attachment(f, MediaTypeNames.Application.Octet))
                    .ToArray();
            }

            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                wc.DownloadString(url)
                    .SendSmtpMessage(mailTo, subject, ModelExtension.Properties.AppSettings.Default.WebMaster, items, ModelExtension.Properties.AppSettings.Default.ReplyTo, smtpSettings);

            }
        }

        public static void MailWebPage(this String url, NameValueCollection items, String mailTo, String subject, CustomSmtpHost smtpSettings = null)
        {
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                wc.Encoding.GetString(wc.UploadValues(url, items))
                    .SendSmtpMessage(mailTo, subject, ModelExtension.Properties.AppSettings.Default.WebMaster, null, ModelExtension.Properties.AppSettings.Default.ReplyTo, smtpSettings);
            }
        }

        public static void SendMailMessage(this String body, String mailTo, String subject, CustomSmtpHost smtpSettings = null)
        {
            body.SendSmtpMessage(mailTo, subject, ModelExtension.Properties.AppSettings.Default.WebMaster, null, ModelExtension.Properties.AppSettings.Default.ReplyTo, smtpSettings);
        }

        public static void AlertSMSError(int id, String reason, String content)
        {
            NameValueCollection items = new NameValueCollection();
            items["id"] = id.ToString();
            items["reason"] = reason;
            items["sms"] = content;
            String url = String.Format("{0}{1}", ModelExtension.Properties.AppSettings.Default.HostUrl,
                VirtualPathUtility.ToAbsolute("~/Published/SMSExceptionNotification"));

            ThreadPool.QueueUserWorkItem(stateInfo =>
            {
                try
                {
                    url.MailWebPage(items, ModelExtension.Properties.AppSettings.Default.WebMaster, "簡訊傳送失敗通知");
                }
                catch (Exception ex)
                {
                    Logger.Warn(String.Format("簡訊傳送失敗通知:{0}", url));
                    Logger.Error(ex);
                }
            });
        }

        public static void SendMailMessage(this String body, String mailTo, String subject, CustomSmtpHost? smtpSettings = null, params String[] attachment)
        {
            System.Net.Mail.Attachment[]? items = null;

            if (attachment != null && attachment.Length > 0)
            {
                items = attachment.Where(f => File.Exists(f))
                    .Select(f => new System.Net.Mail.Attachment(f, MediaTypeNames.Application.Octet))
                    .ToArray();
            }

            body.SendSmtpMessage(mailTo, subject, ModelExtension.Properties.AppSettings.Default.ServiceMailBox, items, smtpSettings: smtpSettings);

        }

        public static void SendMailMessage(this String body, String mailTo, String subject, System.Net.Mail.Attachment[] attachment, CustomSmtpHost? smtpSettings = null)
        {
            body.SendSmtpMessage(mailTo, subject, ModelExtension.Properties.AppSettings.Default.ServiceMailBox, attachment, smtpSettings: smtpSettings);
        }
    }
}