using Microsoft.EntityFrameworkCore;
using ModelCore.DataEntity;

namespace WebHome.Helper
{
    /// <summary>
    /// 清單頁共用的 eager loading 設定。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 清單頁的每一列都會經由 DataItem 分部檢視存取多個導覽屬性；在 lazy loading proxy 之下
    /// 每一個導覽都是一次獨立查詢（N+1），一頁 10 筆就可能發出數十次往返。這裡把各欄位
    /// 可能用到的導覽一次載入。
    /// </para>
    /// <para>
    /// 只納入 <b>參考型</b>（單一實體）導覽與少數必要的集合：參考型導覽是同一列的 LEFT JOIN，
    /// 不會放大結果列數；集合型導覽每多一個就會與其他集合形成笛卡兒乘積，因此 Attachment、
    /// Product 這類不一定顯示的集合仍維持 lazy loading。
    /// </para>
    /// <para>
    /// 呼叫時機：務必在 <c>Skip</c>/<c>Take</c> 之前套用。
    /// </para>
    /// </remarks>
    public static class QueryIncludeExtensions
    {
        /// <summary>
        /// 發票清單（InvoiceProcess / InvoiceQuery / InvoiceAudit 等）每列會用到的導覽屬性。
        /// </summary>
        public static IQueryable<InvoiceItem> IncludeInvoiceListNavigations(this IQueryable<InvoiceItem> items)
        {
            return items
                .Include(i => i.InvoiceBuyer)
                .Include(i => i.InvoiceSeller)
                .Include(i => i.Seller)
                .Include(i => i.InvoiceAmountType!).ThenInclude(a => a.Currency)
                .Include(i => i.InvoiceCarrier)
                .Include(i => i.InvoiceCancellation)
                .Include(i => i.InvoicePurchaseOrder)
                .Include(i => i.InvoiceWinningNumber)
                .Include(i => i.InvoiceDonation)
                .Include(i => i.CDS_Document).ThenInclude(d => d.IssuingNotice)
                .Include(i => i.CDS_Document).ThenInclude(d => d.DataProcessLog)
                .Include(i => i.CDS_Document).ThenInclude(d => d.ChildDocument);
        }

        /// <summary>
        /// 折讓清單（AllowanceProcess / InvoiceAudit 等）每列會用到的導覽屬性。
        /// </summary>
        public static IQueryable<InvoiceAllowance> IncludeAllowanceListNavigations(this IQueryable<InvoiceAllowance> items)
        {
            return items
                .Include(a => a.InvoiceAllowanceBuyer)
                .Include(a => a.InvoiceAllowanceSeller)
                .Include(a => a.InvoiceAllowanceCancellation)
                .Include(a => a.Currency)
                .Include(a => a.InvoiceAllowanceDetails)
                .Include(a => a.CDS_Document).ThenInclude(d => d.DataProcessLog);
        }

        /// <summary>
        /// 配號區間清單（InvoiceNo/Module/DataItem）每列會用到的導覽屬性。
        /// </summary>
        /// <remarks>
        /// DataItem 會取用 InvoiceTrackCodeAssignment 的 Seller/Track，以及 InvoiceNoSegment；
        /// 這些都是參考型導覽，一次 LEFT JOIN 載入不會放大列數。
        /// InvoiceNoAssignment／InvoiceNoAllocation 是資料量可能很大的集合，維持 lazy loading。
        /// </remarks>
        public static IQueryable<InvoiceNoInterval> IncludeInvoiceNoIntervalListNavigations(this IQueryable<InvoiceNoInterval> items)
        {
            return items
                .Include(i => i.InvoiceTrackCodeAssignment).ThenInclude(a => a.Seller)
                .Include(i => i.InvoiceTrackCodeAssignment).ThenInclude(a => a.Track)
                .Include(i => i.InvoiceNoSegment);
        }

        /// <summary>
        /// 營業人清單（OrganizationQuery / Organization 等）每列會用到的導覽屬性。
        /// </summary>
        public static IQueryable<Organization> IncludeOrganizationListNavigations(this IQueryable<Organization> items)
        {
            return items
                .Include(o => o.OrganizationStatus!).ThenInclude(s => s.CurrentLevelNavigation)
                .Include(o => o.OrganizationExtension)
                .Include(o => o.MasterOrganization)
                .Include(o => o.OrganizationCategory)
                .Include(o => o.EnterpriseGroupMember);
        }
    }
}
