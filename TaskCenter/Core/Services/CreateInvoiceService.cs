using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonLib.Core.DataWork;
using CommonLib.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;
using ModelCore.Security.MembershipManagement;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Interfaces;

namespace TaskCenter.Core.Services
{
    /// <summary>
    /// 線上開立發票服務實作（遷移自 WebHome InvoiceBusinessController.CommitInvoice / CommitA0101）。
    /// 開立驗證與 InvoiceItem 組裝重用 ModelExtension.EF 之 InvoiceViewModelValidator（F0401 存證）與
    /// A0101ViewModelValidator（B2B 交換）；寫入沿用 GetTable&lt;T&gt;().Add + PushStepQueueOnSubmit + SubmitChanges。
    /// </summary>
    public class CreateInvoiceService : ICreateInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateInvoiceService> _logger;

        public CreateInvoiceService(IUnitOfWork unitOfWork, ILogger<CreateInvoiceService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // ── 開立人候選（沿用 InvoiceProcessQuery 之角色範圍作法）──────────

        public async Task<List<CreateInvoiceSellerOptionDto>> SearchSellersAsync(
            string? keyword, bool isAdmin, int? categoryId, int? companyId)
        {
            const int maxResults = 20;
            var models = new GenericDbContext<ApplicationDbContext>(_unitOfWork.Context);

            IQueryable<Organization> query;
            if (isAdmin)
            {
                var categories = new[]
                {
                    (int)Naming.CategoryID.COMP_E_INVOICE_B2C_SELLER,
                    (int)Naming.CategoryID.COMP_VIRTUAL_CHANNEL,
                    (int)Naming.CategoryID.COMP_E_INVOICE_GOOGLE_TW,
                    (int)Naming.CategoryID.COMP_INVOICE_AGENT,
                };
                query = models.GetTable<Organization>()
                    .Where(o => o.OrganizationCategory.Any(c => categories.Contains(c.CategoryID)));
            }
            else
            {
                query = OrganizationScope.AllowedOrganizations(models, categoryId ?? 0, companyId ?? 0);
            }

            query = query.AsNoTracking();
            var trimmed = keyword?.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                query = query.Where(o => o.ReceiptNo!.StartsWith(trimmed) || o.CompanyName!.Contains(trimmed));
            }

            var sellers = await query
                .OrderBy(o => o.ReceiptNo)
                .Take(maxResults)
                .Select(o => new { o.CompanyID, o.ReceiptNo, o.CompanyName })
                .ToListAsync();

            return sellers.Select(o => new CreateInvoiceSellerOptionDto
            {
                SellerKey = o.CompanyID.EncryptKey(),
                ReceiptNo = o.ReceiptNo,
                CompanyName = o.CompanyName,
            }).ToList();
        }

        // ── 相對營業人（買受人）查詢 ─────────────────────────────────────
        // 以開立人的相對營業人關係（BusinessRelationship，銷項）為範圍，供表單帶入 / autocomplete。
        // 對應舊版 Home/SearchCounterpart（seller 分支）與 DataEntity/Organization（以關係資料回填）。

        public async Task<List<CounterpartOptionDto>> SearchCounterpartsAsync(int sellerId, string? term)
        {
            var trimmed = term?.Trim();
            if (string.IsNullOrEmpty(trimmed) || sellerId <= 0)
            {
                return new List<CounterpartOptionDto>();
            }

            var models = new GenericDbContext<ApplicationDbContext>(_unitOfWork.Context);

            var rows = await models.GetTable<BusinessRelationship>()
                .AsNoTracking()
                .Where(b => b.MasterID == sellerId
                    && b.BusinessID == (int)Naming.InvoiceCenterBusinessType.銷項
                    && (b.Relative.ReceiptNo!.StartsWith(trimmed)
                        || b.Relative.CompanyName!.Contains(trimmed)
                        || (b.CompanyName != null && b.CompanyName.Contains(trimmed))))
                .OrderBy(b => b.Relative.ReceiptNo)
                .Take(20)
                .Select(b => new CounterpartOptionDto
                {
                    ReceiptNo = b.Relative.ReceiptNo,
                    CompanyName = b.CompanyName ?? b.Relative.CompanyName,
                    Address = b.Addr ?? b.Relative.Addr,
                    Phone = b.Phone ?? b.Relative.Phone,
                    Email = b.ContactEmail ?? b.Relative.ContactEmail,
                    CustomerId = b.CustomerNo,
                })
                .ToListAsync();

            return rows;
        }

        // ── 產品快速查詢 ────────────────────────────────────────────────
        // 依登入者角色（FilterProductCatalogByRole）與所選開立人限縮；對應舊版 ProductCatalog/QuickSearch。

        public async Task<List<ProductOptionDto>> SearchProductsAsync(int uid, int sellerId, string? productName)
        {
            var models = new GenericDbContext<ApplicationDbContext>(_unitOfWork.Context);

            var profile = new UserProfileManager(models).GetUserProfile(uid);
            if (profile?.CurrentUserRole == null)
            {
                return new List<ProductOptionDto>();
            }

            var items = models.FilterProductCatalogByRole(profile, models.GetTable<ProductCatalog>().AsNoTracking());
            if (sellerId > 0)
            {
                items = items.Where(p => p.Supplier.Any(s => s.CompanyID == sellerId));
            }

            var name = productName?.Trim();
            if (!string.IsNullOrEmpty(name) && name != "*")
            {
                items = items.Where(p => p.ProductName.Contains(name));
            }

            var rows = await items
                .OrderBy(p => p.ProductName)
                .Take(30)
                .Select(p => new ProductOptionDto
                {
                    ProductId = p.ProductID,
                    ProductName = p.ProductName,
                    SalePrice = p.SalePrice,
                    Remark = p.Remark,
                    Barcode = p.Barcode,
                    Spec = p.Spec,
                    PieceUnit = p.PieceUnit,
                })
                .ToListAsync();

            return rows;
        }

        // ── 開立（F0401 存證 / A0101 交換）────────────────────────────────

        public Task<CreateInvoiceCommitResult> CommitAsync(CreateInvoiceRequestDto dto, int sellerId)
        {
            var models = new GenericDbContext<ApplicationDbContext>(_unitOfWork.Context);
            var ds = new ModelSource<InvoiceItem>(models);

            var seller = ds.GetTable<Organization>().Where(o => o.CompanyID == sellerId).FirstOrDefault();
            if (seller == null)
            {
                return Task.FromResult(CreateInvoiceCommitResult.Error("發票開立人錯誤!!"));
            }

            var processType = (Naming.InvoiceProcessType)dto.ProcessType;
            var vm = MapToViewModel(dto, sellerId, processType);

            try
            {
                InvoiceItem newItem;
                Exception? ex;

                if (processType == Naming.InvoiceProcessType.A0101)
                {
                    var validator = new A0101ViewModelValidator<InvoiceItem>(ds, seller);
                    ex = validator.Validate(vm);
                    if (ex != null) return Task.FromResult(CreateInvoiceCommitResult.Error(ex.Message));
                    newItem = validator.InvoiceItem;
                }
                else
                {
                    var validator = new InvoiceViewModelValidator<InvoiceItem>(ds, seller);
                    ex = validator.Validate(vm);
                    if (ex != null) return Task.FromResult(CreateInvoiceCommitResult.Error(ex.Message));
                    newItem = validator.InvoiceItem;
                }

                newItem.CDS_Document.ProcessType = (int?)processType;

                // 內容預覽：驗證後直接組裝預覽，不寫入（對應舊版 ForPreview 回傳 InvoiceContent）。
                if (dto.ForPreview)
                {
                    return Task.FromResult(CreateInvoiceCommitResult.ForPreview(BuildPreview(newItem, dto)));
                }

                ds.GetTable<InvoiceItem>().Add(newItem);
                if (processType == Naming.InvoiceProcessType.A0101)
                {
                    newItem.CDS_Document.PushStepQueueOnSubmit(ds, Naming.InvoiceStepDefinition.待傳送, Naming.InvoiceProcessType.A0101);
                }
                else
                {
                    newItem.CDS_Document.PushStepQueueOnSubmit(ds, Naming.InvoiceStepDefinition.已開立, Naming.InvoiceProcessType.F0401);
                }
                ds.SubmitChanges();

                return Task.FromResult(CreateInvoiceCommitResult.Success(new CreateInvoiceResultDto
                {
                    KeyId = newItem.InvoiceID.EncryptKey(),
                    TrackCode = newItem.TrackCode,
                    No = newItem.No,
                    InvoiceNo = $"{newItem.TrackCode}{newItem.No}",
                    PrintMark = newItem.PrintMark,
                    HasCarrier = newItem.InvoiceCarrier != null,
                    ProcessType = dto.ProcessType,
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error committing invoice for seller {SellerId}", sellerId);
                return Task.FromResult(CreateInvoiceCommitResult.Error(ex.Message));
            }
        }

        // ── DTO → InvoiceViewModel ──────────────────────────────────────

        private static InvoiceViewModel MapToViewModel(CreateInvoiceRequestDto dto, int sellerId, Naming.InvoiceProcessType processType)
        {
            var vm = new InvoiceViewModel
            {
                SellerID = sellerId,
                InvoiceProcessType = processType,
                BuyerReceiptNo = dto.BuyerReceiptNo.GetEfficientString(),
                BuyerName = dto.BuyerName,
                Address = dto.Address,
                Phone = dto.Phone,
                EMail = dto.EMail,
                CustomerID = dto.CustomerID.GetEfficientString(),
                BuyerMark = dto.BuyerMark,
                Counterpart = dto.Counterpart,
                B2BRelation = dto.B2BRelation,
                InvoiceType = dto.InvoiceType,
                TaxType = dto.TaxType,
                CustomsClearanceMark = dto.CustomsClearanceMark,
                TaxRate = dto.TaxRate,
                SalesAmount = dto.SalesAmount,
                TaxAmount = dto.TaxAmount,
                TotalAmount = dto.TotalAmount,
                DiscountAmount = dto.DiscountAmount,
                CarrierType = dto.CarrierType.GetEfficientString(),
                CarrierId1 = dto.CarrierId1,
                CarrierId2 = dto.CarrierId2,
                NPOBAN = dto.NPOBAN,
                Remark = dto.Remark,
                DataNumber = dto.DataNumber.GetEfficientString(),
                InvoiceDate = dto.InvoiceDate ?? DateTime.Now,
                CheckNo = dto.CheckNo,
                BuyerRemark = dto.BuyerRemark,
                RelateNumber = dto.RelateNumber,
                Category = dto.Category,
                ForPreview = dto.ForPreview,
            };

            // 隨機碼：使用者有填則用之，否則保留建構子預設（checkBusiness 亦會於空值時補產）。
            if (!string.IsNullOrEmpty(dto.RandomNo))
            {
                vm.RandomNo = dto.RandomNo;
            }

            // 發票明細：DTO 的品項清單 → InvoiceViewModel 的平行陣列。
            var lines = dto.Lines ?? new List<CreateInvoiceLineDto>();
            vm.Brief = lines.Select(l => l.Brief).ToArray();
            vm.ItemNo = lines.Select(l => l.ItemNo).ToArray();
            vm.ItemRemark = lines.Select(l => l.Remark).ToArray();
            vm.UnitCost = lines.Select(l => l.UnitCost).ToArray();
            vm.CostAmount = lines.Select(l => l.CostAmount).ToArray();
            vm.Piece = lines.Select(l => l.Piece).ToArray();

            return vm;
        }

        // ── 內容預覽組裝 ────────────────────────────────────────────────

        private static InvoicePreviewDto BuildPreview(InvoiceItem newItem, CreateInvoiceRequestDto dto)
        {
            var amt = newItem.InvoiceAmountType;
            var buyer = newItem.InvoiceBuyer;

            return new InvoicePreviewDto
            {
                InvoiceNo = $"{newItem.TrackCode}{newItem.No}",
                InvoiceDate = newItem.InvoiceDate,
                RandomNo = newItem.RandomNo,
                SellerName = newItem.InvoiceSeller?.CustomerName,
                SellerReceiptNo = newItem.InvoiceSeller?.ReceiptNo,
                BuyerName = buyer?.CustomerName ?? buyer?.Name,
                BuyerReceiptNo = buyer?.ReceiptNo,
                BuyerAddress = buyer?.Address,
                BuyerEmail = buyer?.EMail,
                IsB2C = buyer?.ReceiptNo == "0000000000",
                TaxTypeLabel = amt?.TaxType != null
                    ? ((Naming.TaxTypeDefinition)amt.TaxType.Value).ToString()
                    : null,
                SalesAmount = amt?.SalesAmount,
                TaxAmount = amt?.TaxAmount,
                TotalAmount = amt?.TotalAmount,
                CarrierType = newItem.InvoiceCarrier?.CarrierType,
                CarrierNo = newItem.InvoiceCarrier?.CarrierNo,
                AgencyCode = newItem.InvoiceDonation?.AgencyCode,
                Remark = dto.Remark,
                Lines = (dto.Lines ?? new List<CreateInvoiceLineDto>())
                    .Select((l, i) => new InvoicePreviewLineDto
                    {
                        Seq = i + 1,
                        ItemNo = l.ItemNo,
                        Description = l.Brief,
                        Quantity = l.Piece,
                        UnitPrice = l.UnitCost,
                        Amount = l.CostAmount,
                        Remark = l.Remark,
                    })
                    .ToList(),
            };
        }
    }
}
