using CommonLib.Core.DataWork;
using CommonLib.Core.Utility;
using Microsoft.EntityFrameworkCore;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.Models.ViewModel;

namespace TestConsoleCore
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //ExportF0401(args);
            if(args.Length > 1)
            {
                //MigrateInvoiceItem(args[0], args[1]);
                //MigrateInvoiceCancellation(args[0], args[1]);
                MigrateInvoiceAllowance(args[0], args[1]);
            }
        }

        /// <summary>
        /// 將來源資料庫 (srcConn) 的所有 InvoiceItem 及其關聯實體移轉到目的端資料庫 (destConn)。
        ///
        /// 設計重點：
        /// 1. 不保留來源端的識別碼 (Identity)。InvoiceID 對應的 CDS_Document.DocID、InvoiceProduct.ProductID、
        ///    InvoiceProductItem.ItemID 皆為 Identity 欄位，移轉時由目的端重新產生。
        ///    透過 EF Core 的「物件圖 (object graph) 串接新增」自動傳遞新主鍵：
        ///      - CDS_Document(主) → InvoiceItem(從，共用主鍵)
        ///      - InvoiceItem(主) → InvoiceDonation / Extension / Carrier / AmountType / PurchaseOrder / Buyer / Seller(從，共用主鍵)
        ///      - InvoiceProduct(主) → InvoiceProductItem(從) 及多對多 InvoiceDetails 關聯表
        ///    因此內部關聯 (InvoiceID / ProductID) 不需手動重編，EF 會在 SaveChanges 時自動填入。
        /// 2. 指向「不移轉」之 Organization 的外鍵，改以「目的端相同 ReceiptNo(統編) 的 Organization.CompanyID」對應：
        ///      - InvoiceItem.SellerID  (需求 3)
        ///      - InvoiceItem.DonationID(同屬 Organization 外鍵，一併以相同邏輯對應)
        ///      - InvoiceSeller.SellerID(需求 4)
        ///      - InvoiceBuyer.BuyerID  (需求 5)
        ///    賣方 (SellerID) 對應不到時，整張發票略過並記錄；其餘對應不到時設為 null。
        /// 3. 指向其他「不移轉」參照表 (InvoiceTrackCode / CurrencyType / InvoicePurchaseOrderUpload) 的外鍵，
        ///    保留來源值，但先驗證目的端是否存在；不存在則設為 null 並計數警告，避免 FK 違反導致整批失敗。
        /// </summary>
        private static void MigrateInvoiceItem(string srcConn, string destConn)
        {
            using var src = CreateContext(srcConn);
            using var dest = CreateContext(destConn);

            // ── 1. 建立 Organization 對應：來源 CompanyID → 統編(ReceiptNo) → 目的端 CompanyID ──
            var srcOrgReceiptNo = src.Organization.AsNoTracking()
                .Where(o => o.ReceiptNo != null)
                .Select(o => new { o.CompanyID, o.ReceiptNo })
                .ToDictionary(o => o.CompanyID, o => o.ReceiptNo!);

            var destOrgByReceiptNo = dest.Organization.AsNoTracking()
                .Where(o => o.ReceiptNo != null)
                .Select(o => new { o.CompanyID, o.ReceiptNo })
                .ToList()
                .GroupBy(o => o.ReceiptNo!)
                .ToDictionary(g => g.Key, g => g.First().CompanyID);

            // 來源 Organization 外鍵 → 目的端 Organization 外鍵（對應不到回傳 null）
            int? RemapOrg(int? srcCompanyId)
            {
                if (srcCompanyId is null) return null;
                if (srcOrgReceiptNo.TryGetValue(srcCompanyId.Value, out var receiptNo)
                    && destOrgByReceiptNo.TryGetValue(receiptNo, out var destCompanyId))
                {
                    return destCompanyId;
                }
                return null;
            }

            // ── 2. 目的端參照資料：保留來源外鍵值前先驗證存在，缺少則設為 null ──
            var destTrackIds = dest.InvoiceTrackCode.AsNoTracking().Select(t => t.TrackID).ToHashSet();
            var destCurrencyIds = dest.CurrencyType.AsNoTracking().Select(c => c.CurrencyID).ToHashSet();
            var destUploadIds = dest.InvoicePurchaseOrderUpload.AsNoTracking().Select(u => u.UploadID).ToHashSet();

            // 來源 ProductID → 目的端 ProductID（跨發票共用的 InvoiceProduct 只新增一次，其餘以關聯帶入）
            var productMap = new Dictionary<int, int>();

            int lastId = 0;
            const int batchSize = 200;
            long migrated = 0, skipped = 0, missingFkWarnings = 0;

            while (true)
            {
                // 以 InvoiceID 為游標的 keyset 分批讀取來源（記憶體用量與 batchSize 成正比，與總量無關）。
                var batch = src.InvoiceItem.AsNoTracking()
                    .Where(x => x.InvoiceID > lastId)
                    .OrderBy(x => x.InvoiceID)
                    .Include(x => x.Invoice)            // CDS_Document
                    .Include(x => x.InvoiceDonation)
                    .Include(x => x.InvoiceItemExtension)
                    .Include(x => x.InvoiceCarrier)
                    .Include(x => x.InvoiceAmountType)
                    .Include(x => x.InvoicePurchaseOrder)
                    .Include(x => x.InvoiceBuyer)
                    .Include(x => x.InvoiceSeller)
                    .Include(x => x.Product).ThenInclude(p => p.InvoiceProductItem)
                    .AsSplitQuery()
                    .Take(batchSize)
                    .ToList();

                if (batch.Count == 0)
                    break;

                lastId = batch[^1].InvoiceID;

                foreach (var srcItem in batch)
                {
                    if (srcItem.Invoice is null)
                    {
                        Console.WriteLine($"[SKIP] InvoiceID={srcItem.InvoiceID} 缺少 CDS_Document，略過。");
                        skipped++;
                        continue;
                    }

                    // 賣方 (需求 3)：對應不到視為無法移轉，整張略過。
                    int? sellerId = RemapOrg(srcItem.SellerID);
                    if (srcItem.SellerID is not null && sellerId is null)
                    {
                        Console.WriteLine($"[SKIP] InvoiceID={srcItem.InvoiceID} 找不到對應賣方 Organization (來源 SellerID={srcItem.SellerID})。");
                        skipped++;
                        continue;
                    }

                    // ── 組裝目的端物件圖（PK 皆留預設值，交由 EF/Identity 產生並串接傳遞）──
                    var newDoc = new CDS_Document
                    {
                        DocType = srcItem.Invoice.DocType,
                        DocDate = srcItem.Invoice.DocDate,
                        CurrentStep = srcItem.Invoice.CurrentStep,
                        ChannelID = srcItem.Invoice.ChannelID,
                        ProcessType = srcItem.Invoice.ProcessType,
                    };

                    var newItem = new InvoiceItem
                    {
                        Invoice = newDoc,                       // 共用主鍵：DocID → InvoiceID 由 EF 自動傳遞
                        No = srcItem.No,
                        InvoiceDate = srcItem.InvoiceDate,
                        CheckNo = srcItem.CheckNo,
                        Remark = srcItem.Remark,
                        BuyerRemark = srcItem.BuyerRemark,
                        CustomsClearanceMark = srcItem.CustomsClearanceMark,
                        TaxCenter = srcItem.TaxCenter,
                        PermitDate = srcItem.PermitDate,
                        PermitWord = srcItem.PermitWord,
                        PermitNumber = srcItem.PermitNumber,
                        Category = srcItem.Category,
                        RelateNumber = srcItem.RelateNumber,
                        InvoiceType = srcItem.InvoiceType,
                        GroupMark = srcItem.GroupMark,
                        DonateMark = srcItem.DonateMark,
                        SellerID = sellerId,                                  // 需求 3
                        DonationID = RemapOrg(srcItem.DonationID),            // 同屬 Organization 外鍵
                        RandomNo = srcItem.RandomNo,
                        TrackCode = srcItem.TrackCode,
                        BondedAreaConfirm = srcItem.BondedAreaConfirm,
                        PrintMark = srcItem.PrintMark,
                        ProcessType = srcItem.ProcessType,
                        TrackID = ValidateRefFk(srcItem.TrackID, destTrackIds, ref missingFkWarnings,
                                                srcItem.InvoiceID, nameof(InvoiceItem.TrackID)),
                    };

                    if (srcItem.InvoiceDonation is { } d)
                    {
                        newItem.InvoiceDonation = new InvoiceDonation { AgencyCode = d.AgencyCode };
                    }

                    if (srcItem.InvoiceItemExtension is { } ext)
                    {
                        newItem.InvoiceItemExtension = new InvoiceItemExtension
                        {
                            ExtraRemark = ext.ExtraRemark,
                            ProjectNo = ext.ProjectNo,
                            PurchaseNo = ext.PurchaseNo,
                            StampDutyFlag = ext.StampDutyFlag,
                        };
                    }

                    if (srcItem.InvoiceCarrier is { } carrier)
                    {
                        newItem.InvoiceCarrier = new InvoiceCarrier
                        {
                            CarrierType = carrier.CarrierType,
                            CarrierNo = carrier.CarrierNo,
                            CarrierNo2 = carrier.CarrierNo2,
                        };
                    }

                    if (srcItem.InvoiceAmountType is { } amt)
                    {
                        newItem.InvoiceAmountType = new InvoiceAmountType
                        {
                            TaxType = amt.TaxType,
                            SalesAmount = amt.SalesAmount,
                            TaxAmount = amt.TaxAmount,
                            TaxRate = amt.TaxRate,
                            TotalAmount = amt.TotalAmount,
                            TotalAmountInChinese = amt.TotalAmountInChinese,
                            DiscountAmount = amt.DiscountAmount,
                            Adjustment = amt.Adjustment,
                            OriginalCurrencyAmount = amt.OriginalCurrencyAmount,
                            ExchangeRate = amt.ExchangeRate,
                            CurrencyID = ValidateRefFk(amt.CurrencyID, destCurrencyIds, ref missingFkWarnings,
                                                       srcItem.InvoiceID, nameof(InvoiceAmountType.CurrencyID)),
                            FreeTaxSalesAmount = amt.FreeTaxSalesAmount,
                            ZeroTaxSalesAmount = amt.ZeroTaxSalesAmount,
                            BondedAreaConfirm = amt.BondedAreaConfirm,
                            ZeroTaxRateReason = amt.ZeroTaxRateReason,
                        };
                    }

                    if (srcItem.InvoicePurchaseOrder is { } po)
                    {
                        newItem.InvoicePurchaseOrder = new InvoicePurchaseOrder
                        {
                            UploadID = ValidateRefFk(po.UploadID, destUploadIds, ref missingFkWarnings,
                                                     srcItem.InvoiceID, nameof(InvoicePurchaseOrder.UploadID)),
                            OrderNo = po.OrderNo,
                            PurchaseDate = po.PurchaseDate,
                        };
                    }

                    if (srcItem.InvoiceSeller is { } seller)
                    {
                        newItem.InvoiceSeller = new InvoiceSeller
                        {
                            ReceiptNo = seller.ReceiptNo,
                            PostCode = seller.PostCode,
                            Address = seller.Address,
                            Name = seller.Name,
                            SellerID = RemapOrg(seller.SellerID),             // 需求 4
                            CustomerID = seller.CustomerID,
                            ContactName = seller.ContactName,
                            Phone = seller.Phone,
                            EMail = seller.EMail,
                            CustomerName = seller.CustomerName,
                            Fax = seller.Fax,
                            PersonInCharge = seller.PersonInCharge,
                            RoleRemark = seller.RoleRemark,
                        };
                    }

                    if (srcItem.InvoiceBuyer is { } buyer)
                    {
                        newItem.InvoiceBuyer = new InvoiceBuyer
                        {
                            ReceiptNo = buyer.ReceiptNo,
                            PostCode = buyer.PostCode,
                            Address = buyer.Address,
                            Name = buyer.Name,
                            BuyerID = RemapOrg(buyer.BuyerID),                // 需求 5
                            CustomerID = buyer.CustomerID,
                            ContactName = buyer.ContactName,
                            Phone = buyer.Phone,
                            EMail = buyer.EMail,
                            CustomerName = buyer.CustomerName,
                            Fax = buyer.Fax,
                            PersonInCharge = buyer.PersonInCharge,
                            RoleRemark = buyer.RoleRemark,
                            CustomerNumber = buyer.CustomerNumber,
                            BuyerMark = buyer.BuyerMark,
                        };
                    }

                    // ── 產品 (多對多)：已移轉過的 InvoiceProduct 以 stub 帶入關聯，避免重複新增 ──
                    var newProductsBySrcId = new Dictionary<int, InvoiceProduct>();
                    foreach (var srcProduct in srcItem.Product)
                    {
                        if (productMap.TryGetValue(srcProduct.ProductID, out var destProductId))
                        {
                            // 已存在於目的端：附加既有實體 stub，EF 僅建立多對多關聯列。
                            var stub = new InvoiceProduct { ProductID = destProductId };
                            dest.InvoiceProduct.Attach(stub);
                            newItem.Product.Add(stub);
                        }
                        else
                        {
                            var newProduct = new InvoiceProduct { Brief = srcProduct.Brief };
                            foreach (var pi in srcProduct.InvoiceProductItem)
                            {
                                newProduct.InvoiceProductItem.Add(CloneProductItem(pi));
                            }
                            newItem.Product.Add(newProduct);
                            newProductsBySrcId[srcProduct.ProductID] = newProduct;
                        }
                    }

                    dest.Add(newItem);

                    try
                    {
                        dest.SaveChanges();

                        // 記錄本次新增之 InvoiceProduct 的目的端 ProductID，供後續發票共用。
                        foreach (var (srcProductId, product) in newProductsBySrcId)
                        {
                            productMap[srcProductId] = product.ProductID;
                        }

                        migrated++;
                        Console.WriteLine($"[OK] 來源 InvoiceID={srcItem.InvoiceID} → 目的 InvoiceID={newItem.InvoiceID}");
                    }
                    catch (Exception ex)
                    {
                        skipped++;
                        Console.WriteLine($"[FAIL] InvoiceID={srcItem.InvoiceID} 移轉失敗：{ex.GetBaseException().Message}");
                    }

                    // 清除追蹤狀態（含產品 stub），下一張發票重新組裝。
                    dest.ChangeTracker.Clear();
                }
            }

            Console.WriteLine($"Done. 成功={migrated}, 略過/失敗={skipped}, 外鍵改為 null 警告={missingFkWarnings}");
        }

        /// <summary>
        /// 將來源資料庫 (srcConn) 的所有 InvoiceCancellation(作廢發票主檔) 移轉到目的端 (destConn)。
        ///
        /// 設計重點：
        /// 1. InvoiceCancellation 與 InvoiceItem 為 1:1，主鍵 InvoiceID 同時是指向 InvoiceItem 的外鍵
        ///    (ValueGeneratedNever)。由於兩端 InvoiceItem 的 InvoiceID(Identity) 不同，無法沿用來源 InvoiceID。
        /// 2. 改以來源 InvoiceItem 的 (TrackCode, No, InvoiceDate) 三欄到目的端比對出對應的 InvoiceItem，
        ///    取得目的端 InvoiceID 作為新 InvoiceCancellation 的主鍵。
        ///    比對結果不唯一(0 筆或多筆)時略過該筆並記錄。
        /// 3. 僅移轉 InvoiceCancellation 主檔欄位；多對多的 Upload(InvoiceCancellationUpload) 不在本次範圍。
        /// </summary>
        private static void MigrateInvoiceCancellation(string srcConn, string destConn)
        {
            using var src = CreateContext(srcConn);
            using var dest = CreateContext(destConn);

            int lastId = 0;
            const int batchSize = 200;
            long migrated = 0, skipped = 0;

            while (true)
            {
                // 以 InvoiceID 為游標的 keyset 分批讀取來源作廢主檔，並帶出對應 InvoiceItem 的比對鍵。
                var batch = src.InvoiceCancellation.AsNoTracking()
                    .Where(c => c.InvoiceID > lastId)
                    .OrderBy(c => c.InvoiceID)
                    .Include(c => c.Invoice)
                    .Take(batchSize)
                    .ToList();

                if (batch.Count == 0)
                    break;

                lastId = batch[^1].InvoiceID;

                foreach (var srcCancel in batch)
                {
                    var srcInvoice = srcCancel.Invoice;
                    if (srcInvoice is null)
                    {
                        Console.WriteLine($"[SKIP] 來源 InvoiceCancellation(InvoiceID={srcCancel.InvoiceID}) 缺少對應 InvoiceItem，略過。");
                        skipped++;
                        continue;
                    }

                    // 以 (TrackCode, No, InvoiceDate) 比對目的端 InvoiceItem（取 2 筆以偵測不唯一）。
                    var destInvoiceIds = dest.InvoiceItem.AsNoTracking()
                        .Where(x => x.TrackCode == srcInvoice.TrackCode
                                 && x.No == srcInvoice.No
                                 && x.InvoiceDate == srcInvoice.InvoiceDate)
                        .Select(x => x.InvoiceID)
                        .Take(2)
                        .ToList();

                    if (destInvoiceIds.Count == 0)
                    {
                        Console.WriteLine($"[SKIP] 找不到目的端 InvoiceItem (TrackCode={srcInvoice.TrackCode}, No={srcInvoice.No}, InvoiceDate={srcInvoice.InvoiceDate:yyyy-MM-dd})。");
                        skipped++;
                        continue;
                    }

                    if (destInvoiceIds.Count > 1)
                    {
                        Console.WriteLine($"[SKIP] 目的端 InvoiceItem 比對到多筆 (TrackCode={srcInvoice.TrackCode}, No={srcInvoice.No}, InvoiceDate={srcInvoice.InvoiceDate:yyyy-MM-dd})，無法判定。");
                        skipped++;
                        continue;
                    }

                    int destInvoiceId = destInvoiceIds[0];

                    // 1:1：目的端若已有此發票的作廢資料則略過，避免主鍵衝突。
                    if (dest.InvoiceCancellation.AsNoTracking().Any(c => c.InvoiceID == destInvoiceId))
                    {
                        Console.WriteLine($"[SKIP] 目的端 InvoiceID={destInvoiceId} 已存在 InvoiceCancellation，略過。");
                        skipped++;
                        continue;
                    }

                    var newCancel = new InvoiceCancellation
                    {
                        InvoiceID = destInvoiceId,
                        CancellationNo = srcCancel.CancellationNo,
                        CancelDate = srcCancel.CancelDate,
                        CancelReason = srcCancel.CancelReason,
                        ReturnTaxDocumentNo = srcCancel.ReturnTaxDocumentNo,
                        Remark = srcCancel.Remark,
                    };

                    try
                    {
                        dest.Add(newCancel);
                        dest.SaveChanges();
                        migrated++;
                        Console.WriteLine($"[OK] 作廢移轉 來源 InvoiceID={srcCancel.InvoiceID} → 目的 InvoiceID={destInvoiceId} (No={srcInvoice.No})");
                    }
                    catch (Exception ex)
                    {
                        skipped++;
                        Console.WriteLine($"[FAIL] 作廢移轉失敗 來源 InvoiceID={srcCancel.InvoiceID}：{ex.GetBaseException().Message}");
                    }

                    dest.ChangeTracker.Clear();
                }
            }

            Console.WriteLine($"Done. 作廢成功={migrated}, 略過/失敗={skipped}");
        }

        /// <summary>
        /// 將來源資料庫 (srcConn) 的所有 InvoiceAllowance(發票折讓主檔) 及其關聯實體移轉到目的端 (destConn)。
        ///
        /// 設計重點：
        /// 1. 不保留來源識別碼。InvoiceAllowance 與 CDS_Document 為 1:1 共用主鍵 (AllowanceID = DocID，
        ///    AllowanceID 為 ValueGeneratedNever，DocID 為 Identity)；InvoiceAllowanceItem.ItemID 亦為 Identity。
        ///    透過 EF Core 物件圖串接新增自動傳遞新主鍵：
        ///      - CDS_Document(主) → InvoiceAllowance(從，共用主鍵)
        ///      - InvoiceAllowance(主) → Cancellation / ItemExtension / Seller / Buyer(從，共用主鍵)
        ///      - InvoiceAllowance(主) ↔ InvoiceAllowanceItem(多對多 InvoiceAllowanceDetails 關聯表)
        /// 2. 移轉的關聯：CDS_Document、InvoiceAllowanceCancellation、InvoiceAllowanceItem、
        ///    InvoiceAllowanceItemExtension，以及 Seller / Buyer。
        /// 3. Organization 外鍵改以「目的端相同 ReceiptNo(統編) 的 Organization.CompanyID」對應：
        ///      - InvoiceAllowanceSeller.SellerID(需求 3)
        ///      - InvoiceAllowanceBuyer.BuyerID (需求 4)
        ///    對應不到時設為 null。
        /// 4. InvoiceAllowance.InvoiceID(指向被折讓的原始 InvoiceItem)：以來源 InvoiceItem 的
        ///    (TrackCode, No, InvoiceDate) 比對目的端 InvoiceItem 取得新 InvoiceID；找不到/不唯一則設 null。
        /// 5. InvoiceAllowanceItem.ProductItemID(指向已以新 Identity 移轉的 InvoiceProductItem) 無可靠對應鍵，
        ///    一律設為 null 並計數；CurrencyID 不存在於目的端時亦設 null。
        /// </summary>
        private static void MigrateInvoiceAllowance(string srcConn, string destConn)
        {
            using var src = CreateContext(srcConn);
            using var dest = CreateContext(destConn);

            // ── Organization 對應：來源 CompanyID → 統編(ReceiptNo) → 目的端 CompanyID ──
            var srcOrgReceiptNo = src.Organization.AsNoTracking()
                .Where(o => o.ReceiptNo != null)
                .Select(o => new { o.CompanyID, o.ReceiptNo })
                .ToDictionary(o => o.CompanyID, o => o.ReceiptNo!);

            var destOrgByReceiptNo = dest.Organization.AsNoTracking()
                .Where(o => o.ReceiptNo != null)
                .Select(o => new { o.CompanyID, o.ReceiptNo })
                .ToList()
                .GroupBy(o => o.ReceiptNo!)
                .ToDictionary(g => g.Key, g => g.First().CompanyID);

            int? RemapOrg(int? srcCompanyId)
            {
                if (srcCompanyId is null) return null;
                if (srcOrgReceiptNo.TryGetValue(srcCompanyId.Value, out var receiptNo)
                    && destOrgByReceiptNo.TryGetValue(receiptNo, out var destCompanyId))
                {
                    return destCompanyId;
                }
                return null;
            }

            var destCurrencyIds = dest.CurrencyType.AsNoTracking().Select(c => c.CurrencyID).ToHashSet();

            // 來源折讓明細 ItemID → 目的端 ItemID（跨折讓共用的明細只新增一次，其餘以關聯帶入）
            var allowanceItemMap = new Dictionary<int, int>();
            // 來源原始 InvoiceID → 目的端 InvoiceID（折讓所指向的原發票，依自然鍵比對後快取）
            var invoiceIdMap = new Dictionary<int, int?>();

            int lastId = 0;
            const int batchSize = 200;
            long migrated = 0, skipped = 0, productItemDropped = 0, currencyDropped = 0, invoiceUnresolved = 0;

            while (true)
            {
                // 以 AllowanceID 為游標的 keyset 分批讀取來源折讓主檔與其關聯。
                var batch = src.InvoiceAllowance.AsNoTracking()
                    .Where(a => a.AllowanceID > lastId)
                    .OrderBy(a => a.AllowanceID)
                    .Include(a => a.Allowance)                 // CDS_Document
                    .Include(a => a.Invoice)                   // 原始 InvoiceItem（取自然鍵用）
                    .Include(a => a.InvoiceAllowanceCancellation)
                    .Include(a => a.InvoiceAllowanceItemExtension)
                    .Include(a => a.InvoiceAllowanceSeller)
                    .Include(a => a.InvoiceAllowanceBuyer)
                    .Include(a => a.Item)
                    .AsSplitQuery()
                    .Take(batchSize)
                    .ToList();

                if (batch.Count == 0)
                    break;

                lastId = batch[^1].AllowanceID;

                foreach (var srcAllow in batch)
                {
                    if (srcAllow.Allowance is null)
                    {
                        Console.WriteLine($"[SKIP] AllowanceID={srcAllow.AllowanceID} 缺少 CDS_Document，略過。");
                        skipped++;
                        continue;
                    }

                    // 解析折讓指向的原始 InvoiceItem → 目的端 InvoiceID（依 TrackCode/No/InvoiceDate 比對並快取）。
                    int? destInvoiceId = null;
                    if (srcAllow.InvoiceID is int srcInvId)
                    {
                        if (!invoiceIdMap.TryGetValue(srcInvId, out destInvoiceId))
                        {
                            destInvoiceId = ResolveDestInvoiceId(dest, srcAllow.Invoice);
                            invoiceIdMap[srcInvId] = destInvoiceId;
                        }
                        if (destInvoiceId is null)
                        {
                            invoiceUnresolved++;
                            Console.WriteLine($"[WARN] AllowanceID={srcAllow.AllowanceID} 的原始發票無法對應到目的端 InvoiceItem，InvoiceID 設為 null。");
                        }
                    }

                    var newDoc = new CDS_Document
                    {
                        DocType = srcAllow.Allowance.DocType,
                        DocDate = srcAllow.Allowance.DocDate,
                        CurrentStep = srcAllow.Allowance.CurrentStep,
                        ChannelID = srcAllow.Allowance.ChannelID,
                        ProcessType = srcAllow.Allowance.ProcessType,
                    };

                    int? currencyId = srcAllow.CurrencyID;
                    if (currencyId is not null && !destCurrencyIds.Contains(currencyId.Value))
                    {
                        currencyDropped++;
                        currencyId = null;
                    }

                    var newAllow = new InvoiceAllowance
                    {
                        Allowance = newDoc,                    // 共用主鍵：DocID → AllowanceID 由 EF 自動傳遞
                        AllowanceNumber = srcAllow.AllowanceNumber,
                        AllowanceType = srcAllow.AllowanceType,
                        AllowanceDate = srcAllow.AllowanceDate,
                        TotalAmount = srcAllow.TotalAmount,
                        TaxAmount = srcAllow.TaxAmount,
                        InvoiceID = destInvoiceId,             // 需求 4：重新對應原始發票
                        SellerId = srcAllow.SellerId,          // 統編字串，非外鍵，原樣保留
                        BuyerId = srcAllow.BuyerId,
                        CurrencyID = currencyId,
                        IssueDate = srcAllow.IssueDate,
                    };

                    if (srcAllow.InvoiceAllowanceCancellation is { } cancel)
                    {
                        newAllow.InvoiceAllowanceCancellation = new InvoiceAllowanceCancellation
                        {
                            CancelDate = cancel.CancelDate,
                            Remark = cancel.Remark,
                            CancelReason = cancel.CancelReason,
                        };
                    }

                    if (srcAllow.InvoiceAllowanceItemExtension is { } ext)
                    {
                        newAllow.InvoiceAllowanceItemExtension = new InvoiceAllowanceItemExtension
                        {
                            ExtraRemark = ext.ExtraRemark,
                        };
                    }

                    if (srcAllow.InvoiceAllowanceSeller is { } seller)
                    {
                        newAllow.InvoiceAllowanceSeller = new InvoiceAllowanceSeller
                        {
                            SellerID = RemapOrg(seller.SellerID),     // 需求 3
                            Name = seller.Name,
                            CustomerName = seller.CustomerName,
                            ReceiptNo = seller.ReceiptNo,
                            PostCode = seller.PostCode,
                            Address = seller.Address,
                            CustomerID = seller.CustomerID,
                            ContactName = seller.ContactName,
                            Phone = seller.Phone,
                            EMail = seller.EMail,
                            Fax = seller.Fax,
                            PersonInCharge = seller.PersonInCharge,
                            RoleRemark = seller.RoleRemark,
                        };
                    }

                    if (srcAllow.InvoiceAllowanceBuyer is { } buyer)
                    {
                        newAllow.InvoiceAllowanceBuyer = new InvoiceAllowanceBuyer
                        {
                            BuyerID = RemapOrg(buyer.BuyerID),        // 需求 4
                            Name = buyer.Name,
                            CustomerName = buyer.CustomerName,
                            ReceiptNo = buyer.ReceiptNo,
                            PostCode = buyer.PostCode,
                            Address = buyer.Address,
                            CustomerID = buyer.CustomerID,
                            ContactName = buyer.ContactName,
                            Phone = buyer.Phone,
                            EMail = buyer.EMail,
                            Fax = buyer.Fax,
                            PersonInCharge = buyer.PersonInCharge,
                            RoleRemark = buyer.RoleRemark,
                        };
                    }

                    // ── 折讓明細 (多對多)：已移轉過者以 stub 帶入關聯，避免重複新增 ──
                    var newItemsBySrcId = new Dictionary<int, InvoiceAllowanceItem>();
                    foreach (var srcDetail in srcAllow.Item)
                    {
                        if (allowanceItemMap.TryGetValue(srcDetail.ItemID, out var destItemId))
                        {
                            var stub = new InvoiceAllowanceItem { ItemID = destItemId };
                            dest.InvoiceAllowanceItem.Attach(stub);
                            newAllow.Item.Add(stub);
                        }
                        else
                        {
                            if (srcDetail.ProductItemID is not null)
                                productItemDropped++;

                            var newDetail = new InvoiceAllowanceItem
                            {
                                No = srcDetail.No,
                                InvoiceNo = srcDetail.InvoiceNo,
                                Piece = srcDetail.Piece,
                                Amount = srcDetail.Amount,
                                Tax = srcDetail.Tax,
                                ProductItemID = null,          // 設計重點 5：無可靠對應鍵，設 null
                                InvoiceDate = srcDetail.InvoiceDate,
                                ItemNo = srcDetail.ItemNo,
                                OriginalSequenceNo = srcDetail.OriginalSequenceNo,
                                PieceUnit = srcDetail.PieceUnit,
                                OriginalDescription = srcDetail.OriginalDescription,
                                TaxType = srcDetail.TaxType,
                                UnitCost = srcDetail.UnitCost,
                                UnitCost2 = srcDetail.UnitCost2,
                                Piece2 = srcDetail.Piece2,
                                Amount2 = srcDetail.Amount2,
                                PieceUnit2 = srcDetail.PieceUnit2,
                                Remark = srcDetail.Remark,
                            };
                            newAllow.Item.Add(newDetail);
                            newItemsBySrcId[srcDetail.ItemID] = newDetail;
                        }
                    }

                    dest.Add(newAllow);

                    try
                    {
                        dest.SaveChanges();

                        foreach (var (srcItemId, detail) in newItemsBySrcId)
                        {
                            allowanceItemMap[srcItemId] = detail.ItemID;
                        }

                        migrated++;
                        Console.WriteLine($"[OK] 折讓移轉 來源 AllowanceID={srcAllow.AllowanceID} → 目的 AllowanceID={newAllow.AllowanceID}");
                    }
                    catch (Exception ex)
                    {
                        skipped++;
                        Console.WriteLine($"[FAIL] 折讓移轉失敗 來源 AllowanceID={srcAllow.AllowanceID}：{ex.GetBaseException().Message}");
                    }

                    dest.ChangeTracker.Clear();
                }
            }

            Console.WriteLine($"Done. 折讓成功={migrated}, 略過/失敗={skipped}, 原始發票未對應={invoiceUnresolved}, " +
                              $"ProductItemID 清空={productItemDropped}, CurrencyID 清空={currencyDropped}");
        }

        /// <summary>
        /// 以來源 InvoiceItem 的 (TrackCode, No, InvoiceDate) 比對目的端 InvoiceItem，回傳唯一對應的 InvoiceID；
        /// 找不到或比對到多筆時回傳 null。
        /// </summary>
        private static int? ResolveDestInvoiceId(ApplicationDbContext dest, InvoiceItem? srcInvoice)
        {
            if (srcInvoice is null) return null;

            var matches = dest.InvoiceItem.AsNoTracking()
                .Where(x => x.TrackCode == srcInvoice.TrackCode
                         && x.No == srcInvoice.No
                         && x.InvoiceDate == srcInvoice.InvoiceDate)
                .Select(x => x.InvoiceID)
                .Take(2)
                .ToList();

            return matches.Count == 1 ? matches[0] : null;
        }

        /// <summary>建立指向指定連線字串的 ApplicationDbContext。</summary>
        private static ApplicationDbContext CreateContext(string connectionString)
        {
            var context = new ApplicationDbContext();
            // OnConfiguring 預設使用設定檔連線字串，這裡覆寫為傳入的連線字串。
            context.Database.SetConnectionString(connectionString);
            return context;
        }

        /// <summary>
        /// 驗證指向「不移轉參照表」的外鍵值是否存在於目的端；不存在則回傳 null 並計數警告。
        /// </summary>
        private static int? ValidateRefFk(int? srcValue, HashSet<int> destValidIds, ref long warningCount,
                                          int srcInvoiceId, string fkName)
        {
            if (srcValue is null) return null;
            if (destValidIds.Contains(srcValue.Value)) return srcValue;

            warningCount++;
            Console.WriteLine($"[WARN] InvoiceID={srcInvoiceId} 的 {fkName}={srcValue} 在目的端不存在，改為 null。");
            return null;
        }

        /// <summary>複製 InvoiceProductItem（PK / FK 留預設值，交由 EF 串接傳遞）。</summary>
        private static InvoiceProductItem CloneProductItem(InvoiceProductItem src) => new()
        {
            No = src.No,
            Spec = src.Spec,
            Piece = src.Piece,
            Piece2 = src.Piece2,
            PieceUnit = src.PieceUnit,
            PieceUnit2 = src.PieceUnit2,
            Weight = src.Weight,
            WeightUnit = src.WeightUnit,
            UnitFreight = src.UnitFreight,
            UnitCost = src.UnitCost,
            UnitCost2 = src.UnitCost2,
            FreightAmount = src.FreightAmount,
            CostAmount = src.CostAmount,
            CostAmount2 = src.CostAmount2,
            OriginalPrice = src.OriginalPrice,
            Remark = src.Remark,
            RelateNumber = src.RelateNumber,
            TaxType = src.TaxType,
            ItemNo = src.ItemNo,
        };

        private static void ExportF0401(string[] args)
        {
            InvoiceDataQueryViewModel viewModel = new InvoiceDataQueryViewModel
            {
            };

            // 每批處理的筆數，記憶體用量與此值成正比，與資料總量無關。
            // 可由第一個命令列參數覆寫，未提供或無效時預設 500。
            int batchSize = 500;
            if (args.Length > 0 && int.TryParse(args[0], out int parsedBatchSize) && parsedBatchSize > 0)
            {
                batchSize = parsedBatchSize;
            }
            Console.WriteLine($"Batch size: {batchSize}");

            using (GenericDbContext<ApplicationDbContext> models = new GenericDbContext<ApplicationDbContext>())
            {
                // 1. 只建立一次基礎查詢（IQueryable 為延遲執行，尚未打 DB）。
                IQueryable<InvoiceItem> baseQuery =
                    models.GetInvoiceByAgent(models.GetTable<InvoiceItem>(), 24124);

                bool effective = false;
                baseQuery = baseQuery.InquireInvoice(viewModel, models, ref effective);

                // 2. 以 InvoiceID 為游標的 keyset 分頁，逐批取出、處理、釋放。
                //    不用 Skip/Take 深分頁（深頁時 DB 要掃過並丟棄前面所有列，越往後越慢）。
                int lastId = 0;
                long total = 0;

                while (true)
                {
                    var batch = baseQuery
                        .Where(x => x.InvoiceID > lastId)   // keyset 游標
                        .OrderBy(x => x.InvoiceID)
                        // 明確載入 CreateF0401 會用到的導覽屬性，避免逐筆延遲載入造成 N+1。
                        .Include(x => x.InvoiceBuyer)
                        .Include(x => x.InvoiceCarrier)
                        .Include(x => x.InvoiceDonation)
                        .Include(x => x.InvoiceSeller)
                        .Include(x => x.InvoicePurchaseOrder)
                        .Include(x => x.InvoiceAmountType).ThenInclude(a => a!.Currency)
                        .Include(x => x.Seller).ThenInclude(s => s!.OrganizationCustomSetting)
                        .Include(x => x.Seller).ThenInclude(s => s!.OrganizationSettings)
                        .Include(x => x.Invoice).ThenInclude(d => d.DataProcessLog)
                        .Include(x => x.Product).ThenInclude(p => p.InvoiceProductItem)
                        // 有多個集合導覽（OrganizationSettings / DataProcessLog / InvoiceProductItem），
                        // 用 split query 避免單一 JOIN 造成笛卡兒乘積式的資料膨脹。
                        .AsSplitQuery()
                        .AsNoTracking()
                        .Take(batchSize)
                        .ToList();

                    if (batch.Count == 0)
                        break;

                    foreach (var item in batch)
                    {
                        var mig = item.CreateF0401(true);
                        string filePath = Path.Combine(
                            FileLogger.Logger.LogDailyPath, $"F0401_{item.TrackCode}{item.No}.xml");
                        mig.Save(filePath);
                        Console.WriteLine($"InvoiceID: {item.InvoiceID}, FilePath: {filePath}");
                    }

                    lastId = batch[^1].InvoiceID;   // 已 OrderBy(InvoiceID)，最後一筆即下一批游標
                    total += batch.Count;

                    // 釋放此批的內部快取（AsNoTracking 下已不追蹤，這裡是長迴圈的保險措施）。
                    models.DataContext.ChangeTracker.Clear();
                }

                Console.WriteLine($"Done. Total exported: {total}");
            }
        }
    }
}
