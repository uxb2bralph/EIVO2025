using System;
using System.Globalization;

namespace ModelCore.Helper
{
    /// <summary>
    /// 民國曆日期格式化擴充方法（遷移自 WebHome.Helper.DateTimeExtensions）。
    /// 置於共用擴充層（ModelExtension.EF），供 InvoiceNumberApplyService 等 Word 範本產出使用。
    /// </summary>
    public static class TaiwanDateExtensions
    {
        /// <summary>格式化為「民國yyy 年 mm 月 dd 日」。</summary>
        public static string ToFullTaiwanDate(this DateTime datetime)
        {
            TaiwanCalendar taiwanCalendar = new TaiwanCalendar();

            return string.Format("{0} 年 {1} 月 {2} 日",
                taiwanCalendar.GetYear(datetime),
                datetime.Month,
                datetime.Day);
        }

        /// <summary>格式化為「民國yyy/mm/dd」。</summary>
        public static string ToSimpleTaiwanDate(this DateTime datetime)
        {
            TaiwanCalendar taiwanCalendar = new TaiwanCalendar();

            return string.Format("{0}/{1}/{2}",
                taiwanCalendar.GetYear(datetime),
                datetime.Month,
                datetime.Day);
        }

        /// <summary>格式化為「民國yyy年mm月」。</summary>
        public static string ToYYYMMTaiwanDate(this DateTime datetime)
        {
            TaiwanCalendar taiwanCalendar = new TaiwanCalendar();

            return string.Format("{0}年{1}月",
                taiwanCalendar.GetYear(datetime),
                datetime.Month);
        }
    }
}
