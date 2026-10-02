using System;
using System.Collections.Generic;
using System.Linq;
using CommonLib.Core.DataWork;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ModelCore.DataEntity;
using ModelCore.InvoiceManagement.ErrorHandle;
using ModelCore.Resource;

namespace ModelCore.Helper
{
    /// <summary>
    /// 單據號碼唯一索引（IX_InvoicePurchaseOrder_OrderNo_SellerID）衝突處理。
    /// 驗證階段已查過重複，只有並行上傳同一單據號碼時才會在儲存時撞到索引。
    /// </summary>
    public static class DataNumberConflictExtensions
    {
        public const string DataNumberUniqueIndex = "IX_InvoicePurchaseOrder_OrderNo_SellerID";

        public static bool IsDuplicateDataNumber(this Exception ex)
        {
            return ex is DbUpdateException { InnerException: SqlException sqlEx }
                && (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                && sqlEx.Message.Contains(DataNumberUniqueIndex, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 送出新發票。若單據號碼重複，捨棄本次所有待存變更（避免同一 context 後續 SubmitChanges 連帶失敗），
        /// 呼叫 onDiscarded（如歸還自動配號），再拋出 DuplicateDataNumberException。
        /// </summary>
        public static void SubmitInvoiceChanges(this GenericDbContext<ApplicationDbContext> models, Action? onDiscarded = null)
        {
            try
            {
                models.SubmitChanges();
            }
            catch (DbUpdateException ex) when (ex.IsDuplicateDataNumber())
            {
                var orderNo = models.DataContext.ChangeTracker.Entries<InvoicePurchaseOrder>()
                    .Where(e => e.State == EntityState.Added)
                    .Select(e => e.Entity.OrderNo)
                    .FirstOrDefault();

                models.DataContext.DiscardPendingChanges();
                onDiscarded?.Invoke();

                throw new DuplicateDataNumberException(String.Format(MessageResources.AlertDataNumberDuplicated, orderNo), ex);
            }
        }

        /// <summary>
        /// 將 context 還原到上次成功儲存的狀態：新增的實體脫離追蹤、修改/刪除的實體還原。
        /// </summary>
        public static void DiscardPendingChanges(this DbContext db)
        {
            var tracker = db.ChangeTracker;
            var pending = tracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                .ToList();
            var added = new HashSet<object>(pending.Where(e => e.State == EntityState.Added).Select(e => e.Entity),
                ReferenceEqualityComparer.Instance);

            bool autoDetect = tracker.AutoDetectChangesEnabled;
            tracker.AutoDetectChangesEnabled = false;
            try
            {
                foreach (var entry in pending)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            entry.State = EntityState.Detached;
                            break;
                        case EntityState.Modified:
                            entry.CurrentValues.SetValues(entry.OriginalValues);
                            entry.State = EntityState.Unchanged;
                            break;
                        case EntityState.Deleted:
                            entry.State = EntityState.Unchanged;
                            break;
                    }
                }

                if (added.Count == 0)
                {
                    return;
                }

                // 脫離追蹤不會修正導覽屬性：仍追蹤的實體（如 InvoiceNoInterval.InvoiceNoAssignment）
                // 集合內若還留著新實體，下次 DetectChanges 會被重新判定為 Added，必須一併移除。
                foreach (var entry in tracker.Entries())
                {
                    foreach (var collection in entry.Collections)
                    {
                        if (collection.CurrentValue == null)
                        {
                            continue;
                        }

                        var stale = collection.CurrentValue.Cast<object>().Where(added.Contains).ToList();
                        if (stale.Count == 0)
                        {
                            continue;
                        }

                        var accessor = collection.Metadata.GetCollectionAccessor()!;
                        foreach (var item in stale)
                        {
                            accessor.Remove(entry.Entity, item);
                        }
                    }

                    // 只清主體端的參考（外鍵在已捨棄的新實體上），相依端的外鍵已由上方還原
                    foreach (var reference in entry.References)
                    {
                        if (reference.Metadata is Microsoft.EntityFrameworkCore.Metadata.INavigation nav
                            && !nav.IsOnDependent
                            && reference.CurrentValue != null
                            && added.Contains(reference.CurrentValue))
                        {
                            nav.PropertyInfo?.SetValue(entry.Entity, null);
                        }
                    }
                }
            }
            finally
            {
                tracker.AutoDetectChangesEnabled = autoDetect;
            }
        }
    }
}
