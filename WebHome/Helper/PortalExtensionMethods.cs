using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;



using CommonLib.DataAccess;


using WebHome.Properties;

using MessagingToolkit.QRCode.Codec;
using ModelCore.DataEntity;
using ModelCore.Locale;

using CommonLib.Utility;


namespace WebHome.Helper
{
    public static class PortalExtensionMethods
    {
        public static void NotifyToResetPassword(this UserProfile profile)
        {
            ThreadPool.QueueUserWorkItem(t =>
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadString($"{ModelExtension.Properties.AppSettings.Default.HostUrl}{VirtualPathUtility.ToAbsolute("~/Notification/ActivateUser")}?uid={profile.UID}&resetPass={true}");
                    }
                }
                catch (Exception ex)
                {
                    CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                }
            });

        }

        public static void NotifyTwoFactorSettings(this UserProfile profile)
        {
            ThreadPool.QueueUserWorkItem(t =>
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadString($"{ModelExtension.Properties.AppSettings.Default.HostUrl}{VirtualPathUtility.ToAbsolute("~/Notification/ActivateUser")}?uid={profile.UID}&resetPass={true}");
                    }
                }
                catch (Exception ex)
                {
                    CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                }
            });

        }

        public static void NotifyToActivate(this UserProfile profile)
        {
            Task.Run(() =>
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadString($"{ModelExtension.Properties.AppSettings.Default.HostUrl}{VirtualPathUtility.ToAbsolute("~/Notification/ActivateUser")}?uid={profile.UID}");
                    }
                }
                catch (Exception ex)
                {
                    CommonLib.Core.Utility.FileLogger.Logger.Warn("［網際優勢電子發票加值中心 會員啟用認證信］傳送失敗,原因 => " + ex.Message);
                    CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                }
            });
            //ThreadPool.QueueUserWorkItem(t =>
            //{

            //});
        }

        public static void NotifyToActivate(this String pid)
        {
            Task.Run(() =>
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadString($"{ModelExtension.Properties.AppSettings.Default.HostUrl}{VirtualPathUtility.ToAbsolute("~/Notification/ActivateUser")}?PID={HttpUtility.UrlEncode(pid)}");
                    }
                }
                catch (Exception ex)
                {
                    CommonLib.Core.Utility.FileLogger.Logger.Warn("［網際優勢電子發票加值中心 會員啟用認證信］傳送失敗,原因 => " + ex.Message);
                    CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                }
            });
            //ThreadPool.QueueUserWorkItem(t =>
            //{

            //});
        }

    }
}