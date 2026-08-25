using System;

namespace TaskCenter.Core.DTOs
{
    /// <summary>
    /// 發票月報表查詢條件（遷移自 WebHome InvoiceQueryController.MonthlyReport /
    /// InquireMonthlyReport 之 MonthlyReportQueryViewModel）。對應選單「下載發票月報表」
    /// （/InvoiceQuery/MonthlyReport）。查詢表單僅有開立人 / 代理業者 / 發票日期起迄三項。
    /// SellerKey / AgentKey 為加密後的 Organization.CompanyID（沿用其他遷移做法）。
    /// </summary>
    public class MonthlyReportQueryDto
    {
        /// <summary>開立人（加密後的 CompanyID；對應舊版 SellerID）。與 AgentKey 至少須擇一。</summary>
        public string? SellerKey { get; set; }

        /// <summary>代理業者（加密後的 CompanyID；對應舊版 AgentID）。與 SellerKey 至少須擇一。</summary>
        public string? AgentKey { get; set; }

        /// <summary>統計起日（必填）；實際統計自該日所屬月份之 1 日起算。</summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>統計迄日（必填）；實際統計含該日所屬月份整月。</summary>
        public DateTime? DateTo { get; set; }
    }
}
