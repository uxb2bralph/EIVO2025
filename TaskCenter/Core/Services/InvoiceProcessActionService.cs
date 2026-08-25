using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonLib.Core.DataWork;
using CommonLib.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelCore.DataEntity;
using ModelCore.DataEntityWrapper;
using ModelCore.Helper;
using ModelCore.InvoiceManagement;
using ModelCore.Security.MembershipManagement;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Interfaces;

namespace TaskCenter.Core.Services
{
    /// <summary>
    /// 發票逐列作業服務實作（遷移自 WebHome InvoiceProcessController.CancelInvoiceAsync）。
    /// 作廢核心沿用 ModelExtension.EF 之 InvoiceManager.VoidInvoice；
    /// 作業對象先以登入者角色範圍（FilterInvoiceByRole）限縮，避免越權作廢。
    /// </summary>
    public class InvoiceProcessActionService : IInvoiceProcessActionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<InvoiceProcessActionService> _logger;

        public InvoiceProcessActionService(IUnitOfWork unitOfWork, ILogger<InvoiceProcessActionService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<CancelInvoiceResultDto> CancelAsync(IEnumerable<string> keyIds, int uid)
        {
            var result = new CancelInvoiceResultDto();

            // 解密請求鍵；保留原鍵以便回報越權 / 查無者。
            var requested = (keyIds ?? Enumerable.Empty<string>())
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Distinct()
                .Select(k => new { KeyId = k, InvoiceId = TryDecrypt(k) })
                .ToList();
            result.RequestedCount = requested.Count;
            if (requested.Count == 0)
            {
                return result;
            }

            var models = new GenericDbContext<ApplicationDbContext>(_unitOfWork.Context);

            var profile = new UserProfileManager(models).GetUserProfile(uid);
            if (profile?.CurrentUserRole == null)
            {
                // 無有效角色：全部視為無權作業。
                result.SkippedNos.AddRange(requested.Select(r => r.KeyId));
                return result;
            }

            var validIds = requested.Where(r => r.InvoiceId.HasValue).Select(r => r.InvoiceId!.Value).ToList();

            // 以角色範圍限縮：僅取登入者可視範圍內的發票。
            var scoped = models.FilterInvoiceByRole(
                profile,
                models.GetTable<InvoiceItem>().Where(i => validIds.Contains(i.InvoiceID)));

            // 記錄範圍內每張發票是否已作廢，用以區分「完成作廢」與「已作廢（略過）」。
            var scopedInfo = await scoped
                .Select(i => new { i.InvoiceID, IsCancelled = i.InvoiceCancellation != null })
                .ToListAsync();
            var scopedIds = new HashSet<int>(scopedInfo.Select(i => i.InvoiceID));

            // 範圍外 / 查無 / 解密失敗 → 略過（以加密鍵回報）。
            foreach (var r in requested)
            {
                if (!r.InvoiceId.HasValue || !scopedIds.Contains(r.InvoiceId.Value))
                {
                    result.SkippedNos.Add(r.KeyId);
                }
            }

            // 僅對範圍內、尚未作廢者執行作廢。
            var toCancel = scopedInfo.Where(i => !i.IsCancelled).Select(i => i.InvoiceID).ToList();
            if (toCancel.Count > 0)
            {
                var mgr = new InvoiceManager(models);
                mgr.VoidInvoice(toCancel);

                var cancelled = mgr.EventItems ?? new List<InvoiceItem>();
                result.CancelledCount = cancelled.Count;
                result.CancelledNos.AddRange(cancelled.Select(i => $"{i.TrackCode}{i.No}"));
            }

            // 範圍內但已作廢者 → 略過（可還原號碼）。
            var alreadyCancelledIds = scopedInfo.Where(i => i.IsCancelled).Select(i => i.InvoiceID).ToList();
            if (alreadyCancelledIds.Count > 0)
            {
                var nos = await models.GetTable<InvoiceItem>()
                    .Where(i => alreadyCancelledIds.Contains(i.InvoiceID))
                    .Select(i => i.TrackCode + i.No)
                    .ToListAsync();
                result.SkippedNos.AddRange(nos);
            }

            return result;
        }

        private static int? TryDecrypt(string keyId)
        {
            try
            {
                return keyId.DecryptKeyValue();
            }
            catch
            {
                return null;
            }
        }
    }
}
