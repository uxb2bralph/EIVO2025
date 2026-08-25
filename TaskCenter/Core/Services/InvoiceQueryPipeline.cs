using System;
using CommonLib.Core.DataWork;
using CommonLib.Utility;
using ModelCore.DataEntity;
using ModelCore.DataEntityWrapper;
using ModelCore.Helper;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;
using ModelCore.Security.MembershipManagement;
using System.Linq;
using TaskCenter.Core.DTOs;

namespace TaskCenter.Core.Services
{
    /// <summary>
    /// 發票查詢管線共用邏輯（供 InvoiceProcessQuery / InvoiceSummary 等查詢服務共用）。
    /// 條件過濾一律重用 ModelExtension.EF 的 ModelSource&lt;InvoiceItem&gt;.BuildInvoiceQuery，
    /// 角色資料範圍以登入者 UID 還原 UserProfileWrapper 後由管線內 FilterInvoiceByRole 套用。
    /// </summary>
    internal static class InvoiceQueryPipeline
    {
        /// <summary>以登入者 UID 還原完整使用者資料（含 CurrentUserRole）。</summary>
        public static UserProfileWrapper? GetProfile(GenericDbContext<ApplicationDbContext> models, int uid)
        {
            return new UserProfileManager(models).GetUserProfile(uid);
        }

        /// <summary>依角色範圍 + 查詢條件建立過濾後（未排序）之 IQueryable。</summary>
        public static IQueryable<InvoiceItem> BuildQuery(
            GenericDbContext<ApplicationDbContext> models, InvoiceProcessQueryDto dto, UserProfileWrapper profile)
        {
            var ms = new ModelSource<InvoiceItem>(models);
            ms.BuildInvoiceQuery(MapToViewModel(dto), profile, "Common");
            return ms.Items;
        }

        /// <summary>DTO → InquireInvoiceViewModel（欄位對應舊版查詢表單）。</summary>
        public static InquireInvoiceViewModel MapToViewModel(InvoiceProcessQueryDto dto)
        {
            var vm = new InquireInvoiceViewModel
            {
                BuyerReceiptNo = dto.BuyerReceiptNo.GetEfficientString(),
                BuyerName = dto.BuyerName.GetEfficientString(),
                CustomerID = dto.CustomerId.GetEfficientString(),
                DateFrom = dto.DateFrom,
                DateTo = dto.DateTo,
                InvoiceNo = dto.InvoiceNo.GetEfficientString(),
                EndNo = dto.EndNo.GetEfficientString(),
                DataNo = dto.DataNo.GetEfficientString(),
                Attachment = dto.Attachment,
                Winning = dto.Winning,
                Cancelled = dto.Cancelled,
                PrintMark = dto.PrintMark.GetEfficientString(),
                Printed = dto.Printed,
                HasAddr = dto.HasAddr,
                CarrierType = dto.CarrierType.GetEfficientString(),
                CarrierNo = dto.CarrierNo.GetEfficientString(),
                IsNoticed = dto.IsNoticed,
            };

            if (!string.IsNullOrEmpty(dto.SellerKey))
            {
                vm.SellerID = dto.SellerKey.DecryptKeyValue();
            }
            if (!string.IsNullOrEmpty(dto.AgentKey))
            {
                vm.AgentID = dto.AgentKey.DecryptKeyValue();
            }
            if (dto.BusinessType.HasValue)
            {
                vm.BusinessType = (Naming.InvoiceCenterBusinessType)dto.BusinessType.Value;
            }

            return vm;
        }
    }
}
