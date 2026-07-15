using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CommonLib.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelCore.DataEntity;
using ModelCore.DTOs;
using ModelCore.Helper;
using ModelCore.Models.ViewModel;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Interfaces;
using TaskCenter.Core.Services;
using E0401 = ModelCore.Schema.TurnKey.E0401;

namespace TaskCenter.Core.Controllers
{
    /// <summary>
    /// 電子發票配號區間維護 API（遷移自 WebHome InvoiceNoController.MaintainInvoiceNoInterval 之核心動作）。
    /// 第一階段涵蓋：查詢清單 / 新增 / 修改 / 刪除 / 鎖定，以及開立人、字軌選擇器。
    /// 目前以系統管理範圍查詢（profile=null）；分支機構角色範圍待 JWT 帶入角色/組織後補強。
    /// 進階動作（分割 / 均分 / POS / 主機構 / 分支機構 / E0401 匯出）另行分階段遷移。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class InvoiceNoIntervalController : ApiBaseController
    {
        private readonly IInvoiceNoIntervalService _service;

        public InvoiceNoIntervalController(
            IInvoiceNoIntervalService service,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
            _service = service;
        }

        /// <summary>查詢配號區間（分頁）。SellerKey 與 Year 為必要條件。</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseDto<PagedResultDto<InvoiceNoIntervalDatatableDto>>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 500)]
        public async Task<IActionResult> GetList([FromQuery] InvoiceNoIntervalQueryDto queryDto)
        {
            if (queryDto == null || string.IsNullOrEmpty(queryDto.SellerKey))
            {
                return CreateBadRequestResponse("請選擇開立人!!");
            }
            if (!queryDto.Year.HasValue)
            {
                return CreateBadRequestResponse("請選擇年份!!");
            }

            try
            {
                var result = await _service.GetPagedAsync(queryDto, IsAdmin(), User.GetCategoryId(), User.GetCompanyId());
                return CreateSuccessResponse(result, "Common.Retrieved");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving invoice no intervals");
                return CreateErrorResponse(500, "Common.RetrieveError");
            }
        }

        /// <summary>開立人候選清單（統編前綴 / 名稱包含），供前端選擇開立人。</summary>
        [HttpGet("Sellers")]
        [ProducesResponseType(typeof(ResponseDto<List<InvoiceNoSellerOptionDto>>), 200)]
        public async Task<IActionResult> Sellers([FromQuery] string? keyword)
        {
            try
            {
                var result = await _service.SearchSellersAsync(keyword, IsAdmin(), User.GetCategoryId(), User.GetCompanyId());
                return CreateSuccessResponse(result, "Common.Retrieved");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving sellers");
                return CreateErrorResponse(500, "Common.RetrieveError");
            }
        }

        /// <summary>字軌候選清單（依年度 + 期別），供新增配號區間選擇字軌。</summary>
        [HttpGet("TrackCodes")]
        [ProducesResponseType(typeof(ResponseDto<List<InvoiceTrackCodeOptionDto>>), 200)]
        public async Task<IActionResult> TrackCodes([FromQuery] int? year, [FromQuery] int? periodNo)
        {
            try
            {
                var result = await _service.GetTrackCodeOptionsAsync(year, periodNo);
                return CreateSuccessResponse(result, "Common.Retrieved");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving track code options");
                return CreateErrorResponse(500, "Common.RetrieveError");
            }
        }

        /// <summary>取得開立人之可用配號存量警戒值（OrganizationExtension.InvoiceNoSafetyStock）。</summary>
        [HttpGet("SafetyStock")]
        [ProducesResponseType(typeof(ResponseDto<InvoiceNoSafetyStockResultDto>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public IActionResult GetSafetyStock([FromQuery] string? sellerKey)
        {
            var sellerId = string.IsNullOrEmpty(sellerKey) ? (int?)null : sellerKey.DecryptKeyValue();
            if (!sellerId.HasValue)
            {
                return CreateBadRequestResponse("請選擇開立人!!");
            }
            if (!CanAccessSeller(sellerId.Value)) return CreateErrorResponse(403, "無權存取此開立人資料!!");

            var value = models!.GetTable<OrganizationExtension>()
                .Where(e => e.CompanyID == sellerId.Value)
                .Select(e => e.InvoiceNoSafetyStock)
                .FirstOrDefault();

            return CreateSuccessResponse(new InvoiceNoSafetyStockResultDto { SafetyStock = value }, "Common.Retrieved");
        }

        /// <summary>
        /// 設定開立人之可用配號存量警戒值（遷移自 Organization.CommitInvoiceNoSafetyStock）。
        /// SafetyStock 為 null 時清除。
        /// </summary>
        [HttpPost("SafetyStock")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public IActionResult CommitSafetyStock([FromBody] InvoiceNoSafetyStockDto dto)
        {
            var sellerId = dto == null || string.IsNullOrEmpty(dto.SellerKey) ? (int?)null : dto.SellerKey.DecryptKeyValue();
            if (!sellerId.HasValue)
            {
                return CreateBadRequestResponse("營業人錯誤!!");
            }
            if (!CanAccessSeller(sellerId.Value)) return CreateErrorResponse(403, "無權存取此開立人資料!!");

            var ext = models!.GetTable<OrganizationExtension>().FirstOrDefault(e => e.CompanyID == sellerId.Value);
            if (ext == null)
            {
                // 該營業人尚無延伸資料列時建立（PK = CompanyID，1:1 對應 Organization）。
                ext = new OrganizationExtension { CompanyID = sellerId.Value };
                models!.GetTable<OrganizationExtension>().Add(ext);
            }

            ext.InvoiceNoSafetyStock = dto!.SafetyStock;
            models!.SubmitChanges();

            return CreateSuccessResponse("Common.Saved");
        }

        /// <summary>
        /// 新增 / 修改配號區間（遷移自 InvoiceNoController.CommitItem + checkInput）。
        /// IntervalId 為 null 時新增（需 TrackId + SellerKey），否則修改。成功後前端重新查詢。
        /// </summary>
        [HttpPost("CommitItem")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public IActionResult CommitItem([FromBody] InvoiceNoIntervalEditDto dto)
        {
            if (dto == null)
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            var sellerId = string.IsNullOrEmpty(dto.SellerKey) ? (int?)null : dto.SellerKey.DecryptKeyValue();

            var model = dto.IntervalId.HasValue
                ? models!.GetTable<InvoiceNoInterval>().FirstOrDefault(i => i.IntervalID == dto.IntervalId.Value)
                : null;

            // 角色範圍檢查：修改時檢查該區間之開立人；新增時檢查目標開立人。
            var targetSeller = model?.SellerID ?? sellerId;
            if (targetSeller.HasValue && !CanAccessSeller(targetSeller.Value))
            {
                return CreateErrorResponse(403, "無權存取此開立人資料!!");
            }

            var errors = ValidateInterval(dto, sellerId, model);
            if (errors.Count > 0)
            {
                return CreateBadRequestResponse("Common.SaveError", errors);
            }

            var startNo = dto.StartNo!.Value;
            var endNo = dto.EndNo!.Value;

            if (model == null)
            {
                // 確保 (SellerID, TrackID) 之字軌指派存在（InvoiceNoInterval 以此為 FK）。
                var trackId = dto.TrackId!.Value;
                var hasAssignment = models!.GetTable<InvoiceTrackCodeAssignment>()
                    .Any(t => t.SellerID == sellerId!.Value && t.TrackID == trackId);
                if (!hasAssignment)
                {
                    models!.GetTable<InvoiceTrackCodeAssignment>().Add(new InvoiceTrackCodeAssignment
                    {
                        SellerID = sellerId!.Value,
                        TrackID = trackId,
                    });
                    models!.SubmitChanges();
                }

                model = new InvoiceNoInterval
                {
                    SellerID = sellerId!.Value,
                    TrackID = trackId,
                    StartNo = startNo,
                    EndNo = endNo,
                };
                models!.GetTable<InvoiceNoInterval>().Add(model);
            }
            else
            {
                model.StartNo = startNo;
                model.EndNo = endNo;
            }

            models!.SubmitChanges();

            // POS 機號（InvoiceNoSegment 與 InvoiceNoInterval 共用主鍵 SegmentID = IntervalID）。
            var device = dto.DeviceName.GetEfficientString();
            if (device == null)
            {
                models!.ExecuteCommand("delete InvoiceNoSegment where SegmentID = {0}", model.IntervalID);
            }
            else
            {
                var segment = models!.GetTable<InvoiceNoSegment>().FirstOrDefault(s => s.SegmentID == model.IntervalID);
                if (segment == null)
                {
                    segment = new InvoiceNoSegment { SegmentID = model.IntervalID };
                    models!.GetTable<InvoiceNoSegment>().Add(segment);
                }
                segment.DeviceName = device;
                models!.SubmitChanges();
            }

            return CreateSuccessResponse("Common.Saved");
        }

        /// <summary>刪除配號區間（遷移自 InvoiceNoController.DeleteNoInterval）。已配發 / 指派號碼者不可刪除。</summary>
        [HttpPost("DeleteItem")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public IActionResult DeleteItem([FromQuery] int id)
        {
            var model = models!.GetTable<InvoiceNoInterval>().FirstOrDefault(i => i.IntervalID == id);
            if (model == null)
            {
                return CreateBadRequestResponse("配號區間資料錯誤!!");
            }

            if (!CanAccessSeller(model.SellerID)) return CreateErrorResponse(403, "無權存取此開立人資料!!");

            var used = models!.GetTable<InvoiceNoAssignment>().Any(a => a.IntervalID == id)
                || models!.GetTable<InvoiceNoAllocation>().Any(a => a.IntervalID == id);
            if (used)
            {
                return CreateBadRequestResponse("該區間之號碼已經被使用,不可刪除!!");
            }

            try
            {
                models!.ExecuteCommand("delete InvoiceNoSegment where SegmentID = {0}", id);
                models!.ExecuteCommand("delete InvoiceNoInterval where IntervalID = {0}", id);
                return CreateSuccessResponse("Common.Saved");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error deleting invoice no interval {Id}", id);
                return CreateBadRequestResponse(ex.Message);
            }
        }

        /// <summary>鎖定 / 解除鎖定配號區間（遷移自 InvoiceNoController.LockInterval）。</summary>
        [HttpPost("LockItem")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public IActionResult LockItem([FromBody] InvoiceNoIntervalLockDto dto)
        {
            if (dto == null)
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            var model = models!.GetTable<InvoiceNoInterval>().FirstOrDefault(i => i.IntervalID == dto.IntervalId);
            if (model == null)
            {
                return CreateBadRequestResponse("配號區間資料錯誤!!");
            }

            if (!CanAccessSeller(model.SellerID)) return CreateErrorResponse(403, "無權存取此開立人資料!!");

            // 鎖定時以目前登入者 UID 作為鎖定標記；解除鎖定時清空。
            model.LockID = dto.Locked ? GetCurrentUid() : null;
            models!.SubmitChanges();

            return CreateSuccessResponse("Common.Saved");
        }

        /// <summary>
        /// 分割配號區間（遷移自 InvoiceNoController.SplitNoInterval）。
        /// 將已部分使用之區間的「尾段」切出為新區間：剩餘號碼須 &gt; 100，切點對齊 50 之倍數。
        /// </summary>
        [HttpPost("SplitItem")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public IActionResult SplitItem([FromQuery] int id)
        {
            var item = models!.GetTable<InvoiceNoInterval>().FirstOrDefault(i => i.IntervalID == id);
            if (item == null)
            {
                return CreateBadRequestResponse("配號區間資料錯誤!!");
            }

            if (!CanAccessSeller(item.SellerID)) return CreateErrorResponse(403, "無權存取此開立人資料!!");

            var currentNo = CurrentAllocatingNo(item);
            var remained = item.EndNo - currentNo + 1;
            if (remained <= 100)
            {
                return CreateBadRequestResponse("剩餘號碼不足，無法分割！");
            }

            var cutpoint = item.EndNo - ((remained - 100) / 50 * 50);
            if (cutpoint == item.EndNo)
            {
                return CreateBadRequestResponse("剩餘號碼不足單一本組數，無法分割！");
            }

            models!.GetTable<InvoiceNoInterval>().Add(new InvoiceNoInterval
            {
                EndNo = item.EndNo,
                SellerID = item.SellerID,
                TrackID = item.TrackID,
                StartNo = cutpoint + 1,
            });
            item.EndNo = cutpoint;
            models!.SubmitChanges();

            return CreateSuccessResponse("Common.Saved");
        }

        /// <summary>
        /// 本組數均分（遷移自 InvoiceNoController.CommitAllotment）。
        /// 將尚未使用之區間，自 StartNo 起每份 Parts*50 個號碼切分為多個區間（首份沿用原區間）。
        /// </summary>
        [HttpPost("AllotItem")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public IActionResult AllotItem([FromBody] InvoiceNoIntervalAllotDto dto)
        {
            if (dto == null || dto.Parts <= 0)
            {
                return CreateBadRequestResponse("請輸入均分本數!!");
            }

            var item = models!.GetTable<InvoiceNoInterval>().FirstOrDefault(i => i.IntervalID == dto.IntervalId);
            if (item == null)
            {
                return CreateBadRequestResponse("配號區間資料錯誤!!");
            }

            if (!CanAccessSeller(item.SellerID)) return CreateErrorResponse(403, "無權存取此開立人資料!!");

            // 已配發 / 指派號碼者不可均分（避免破壞已開立號碼）。
            var used = models!.GetTable<InvoiceNoAssignment>().Any(a => a.IntervalID == item.IntervalID)
                || models!.GetTable<InvoiceNoAllocation>().Any(a => a.IntervalID == item.IntervalID);
            if (used)
            {
                return CreateBadRequestResponse("該區間之號碼已經被使用,不可均分!!");
            }

            var interval = dto.Parts * 50;
            var startNo = item.StartNo + interval;
            var endNo = item.EndNo + 1;
            item.EndNo = startNo - 1;

            var intervals = models!.GetTable<InvoiceNoInterval>();
            while (startNo < endNo)
            {
                var intervalEndNo = Math.Min(startNo + interval, endNo);
                intervals.Add(new InvoiceNoInterval
                {
                    TrackID = item.TrackID,
                    SellerID = item.SellerID,
                    StartNo = startNo,
                    EndNo = intervalEndNo - 1,
                });
                startNo = intervalEndNo;
            }

            models!.SubmitChanges();

            return CreateSuccessResponse("Common.Saved");
        }

        /// <summary>
        /// 主機構配號（遷移自 InvoiceNoController.ApplyHeadquarter）。
        /// 若尚無涵蓋本區間之主機構配號，則以本區間之開立人為主機構、登錄一筆 InvoiceNoMainAssignment。
        /// </summary>
        [HttpPost("ApplyHeadquarter")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public IActionResult ApplyHeadquarter([FromQuery] int id)
        {
            var item = models!.GetTable<InvoiceNoInterval>().FirstOrDefault(i => i.IntervalID == id);
            if (item == null)
            {
                return CreateBadRequestResponse("配號區間資料錯誤!!");
            }

            if (!CanAccessSeller(item.SellerID)) return CreateErrorResponse(403, "無權存取此開立人資料!!");

            EnsureMainAssignment(item);
            return CreateSuccessResponse("Common.Saved");
        }

        /// <summary>
        /// 指派分支機構（遷移自 InvoiceNoController.ApplyBranch + CommitBranch）。
        /// 先確保主機構配號存在（等同舊版 ApplyBranch 先呼叫 ApplyHeadquarter），
        /// 再將本區間改配給指定分支機構開立人（沿用 / 建立其字軌指派並掛在主機構配號下）。
        /// </summary>
        [HttpPost("CommitBranch")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public IActionResult CommitBranch([FromBody] InvoiceNoIntervalBranchDto dto)
        {
            if (dto == null)
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            var item = models!.GetTable<InvoiceNoInterval>().FirstOrDefault(i => i.IntervalID == dto.IntervalId);
            if (item == null)
            {
                return CreateBadRequestResponse("配號區間資料錯誤!!");
            }

            if (!CanAccessSeller(item.SellerID)) return CreateErrorResponse(403, "無權存取此開立人資料!!");

            var branchSellerId = string.IsNullOrEmpty(dto.SellerKey) ? (int?)null : dto.SellerKey.DecryptKeyValue();
            if (!branchSellerId.HasValue)
            {
                return CreateBadRequestResponse("營業人錯誤!!");
            }

            // 確保主機構配號存在（舊版 ApplyBranch 先跑 ApplyHeadquarter）。
            EnsureMainAssignment(item);

            // 取涵蓋整個區間之主機構配號（主機構 = 本區間目前開立人）。
            var masterAssignment = models!.GetTable<InvoiceNoMainAssignment>()
                .FirstOrDefault(m => m.TrackID == item.TrackID && m.MasterID == item.SellerID
                    && m.StartNo <= item.StartNo && m.EndNo >= item.StartNo
                    && m.StartNo <= item.EndNo && m.EndNo >= item.EndNo);
            if (masterAssignment == null)
            {
                return CreateBadRequestResponse("請先設定總公司之號碼區間!!");
            }

            // 沿用 / 建立分支機構之字軌指派，並掛在主機構配號下（AssignmentID 指向主機構配號）。
            var branchAssignment = models!.GetTable<InvoiceTrackCodeAssignment>()
                .FirstOrDefault(t => t.SellerID == branchSellerId.Value && t.TrackID == item.TrackID);
            if (branchAssignment == null)
            {
                models!.GetTable<InvoiceTrackCodeAssignment>().Add(new InvoiceTrackCodeAssignment
                {
                    SellerID = branchSellerId.Value,
                    TrackID = item.TrackID,
                    AssignmentID = masterAssignment.AssignmentID,
                });
                models!.SubmitChanges();
            }

            // 將本區間改配給分支機構（透過 SellerID 之 FK 指向分支機構之字軌指派）。
            item.SellerID = branchSellerId.Value;
            models!.SubmitChanges();

            return CreateSuccessResponse("Common.Saved");
        }

        /// <summary>
        /// 下載 E0401（主機構分支機構配號）XML zip（遷移自 InvoiceNoController.DownloadE0401 +
        /// DownloadE0401.cshtml / DownloadE0401Query.cshtml）。因產物很小，改為同步建立 zip 直接回傳
        /// （取代舊版非同步 ProcessRequest + 輪詢下載）。
        /// 產物：每個分支機構一份 E0401 BranchTrack XML，壓成 E0402-{主機構統編}.zip。
        /// </summary>
        [HttpGet("DownloadE0401")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public IActionResult DownloadE0401([FromQuery] string? sellerKey, [FromQuery] int? year, [FromQuery] int? periodNo)
        {
            var sellerId = string.IsNullOrEmpty(sellerKey) ? (int?)null : sellerKey.DecryptKeyValue();
            if (!sellerId.HasValue) return CreateBadRequestResponse("請選擇開立人!!");
            if (!year.HasValue) return CreateBadRequestResponse("請選擇年份!!");
            if (!periodNo.HasValue) return CreateBadRequestResponse("請選擇期別!!");
            if (!CanAccessSeller(sellerId.Value)) return CreateErrorResponse(403, "無權存取此開立人資料!!");

            var y = (short)year.Value;
            var p = (short)periodNo.Value;

            var sellerReceiptNo = models!.GetTable<Organization>()
                .Where(o => o.CompanyID == sellerId.Value).Select(o => o.ReceiptNo).FirstOrDefault();

            // Path A：主機構配號（InvoiceNoMainAssignment，MasterID = 開立人 + 年度/期別）。
            var mains = models!.GetTable<InvoiceNoMainAssignment>()
                .Where(m => m.MasterID == sellerId.Value
                    && m.InvoiceTrackCodeAssignment.Track.Year == y
                    && m.InvoiceTrackCodeAssignment.Track.PeriodNo == p)
                .Select(m => new
                {
                    m.AssignmentID,
                    m.TrackID,
                    m.StartNo,
                    m.EndNo,
                    Year = m.InvoiceTrackCodeAssignment.Track.Year,
                    PeriodNo = m.InvoiceTrackCodeAssignment.Track.PeriodNo,
                    TrackCode = m.InvoiceTrackCodeAssignment.Track.TrackCode,
                    InvoiceType = m.InvoiceTrackCodeAssignment.Track.InvoiceType,
                    HeadReceiptNo = m.InvoiceTrackCodeAssignment.Seller.ReceiptNo,
                })
                .ToList();

            using var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
            {
                void WriteBranch(string? headBan, string? branchBan, short yr, short pd, string? track,
                    byte? invoiceType, int headBegin, int headEnd, List<(int StartNo, int EndNo)> intervals)
                {
                    var bt = new E0401.BranchTrack
                    {
                        Main = new E0401.Main
                        {
                            BranchBan = branchBan,
                            HeadBan = headBan,
                            YearMonth = string.Format("{0}{1:00}", yr - 1911, pd * 2),
                            InvoiceType = invoiceType == (byte)E0401.InvoiceTypeEnum.Item08
                                ? E0401.InvoiceTypeEnum.Item08 : E0401.InvoiceTypeEnum.Item07,
                            InvoiceTrack = track,
                            InvoiceBeginNo = string.Format("{0:00000000}", headBegin),
                            InvoiceEndNo = string.Format("{0:00000000}", headEnd),
                        },
                        Details = intervals.Select(iv => new E0401.DetailsBranchTrackItem
                        {
                            InvoiceBeginNo = string.Format("{0:00000000}", iv.StartNo),
                            InvoiceEndNo = string.Format("{0:00000000}", iv.EndNo),
                            InvoiceBooklet = (iv.EndNo - iv.StartNo + 1) / 50,
                        }).ToArray(),
                    };

                    var entry = zip.CreateEntry(string.Format("E0401-{0}{1:00}-{2}.xml", yr, pd * 2, branchBan));
                    using var s = entry.Open();
                    bt.ConvertToXml().Save(s);
                }

                if (mains.Count > 0)
                {
                    // 主機構配號存在：每個主機構配號下、依分支機構彙整其配號區間。
                    foreach (var main in mains)
                    {
                        var branches = models!.GetTable<InvoiceTrackCodeAssignment>()
                            .Where(b => b.AssignmentID == main.AssignmentID)
                            .Select(b => new { b.SellerID, b.Seller.ReceiptNo })
                            .ToList();

                        foreach (var branch in branches)
                        {
                            var intervals = models!.GetTable<InvoiceNoInterval>()
                                .Where(iv => iv.TrackID == main.TrackID && iv.SellerID == branch.SellerID)
                                .Select(iv => new { iv.StartNo, iv.EndNo })
                                .ToList()
                                .Select(iv => (iv.StartNo, iv.EndNo))
                                .ToList();

                            WriteBranch(main.HeadReceiptNo, branch.ReceiptNo, main.Year, main.PeriodNo,
                                main.TrackCode, main.InvoiceType, main.StartNo, main.EndNo, intervals);
                        }
                    }
                }
                else
                {
                    // 無主機構配號：以查詢到的配號區間、依開立人分組（表頭起迄為 0）。
                    var vm = new InquireNoIntervalViewModel { Year = year, PeriodNo = periodNo, SellerID = sellerId };
                    var rows = vm.InquireInvoiceNoInterval(models!, null)
                        .Select(iv => new
                        {
                            iv.SellerID,
                            BranchReceiptNo = iv.InvoiceTrackCodeAssignment.Seller.ReceiptNo,
                            Year = iv.InvoiceTrackCodeAssignment.Track.Year,
                            PeriodNo = iv.InvoiceTrackCodeAssignment.Track.PeriodNo,
                            TrackCode = iv.InvoiceTrackCodeAssignment.Track.TrackCode,
                            InvoiceType = iv.InvoiceTrackCodeAssignment.Track.InvoiceType,
                            iv.StartNo,
                            iv.EndNo,
                        })
                        .ToList();

                    if (rows.Count == 0)
                    {
                        return CreateBadRequestResponse("查無可匯出資料!!");
                    }

                    foreach (var g in rows.GroupBy(r => r.SellerID))
                    {
                        var first = g.First();
                        WriteBranch(sellerReceiptNo, first.BranchReceiptNo, first.Year, first.PeriodNo,
                            first.TrackCode, first.InvoiceType, 0, 0,
                            g.Select(r => (r.StartNo, r.EndNo)).ToList());
                    }
                }
            }

            ms.Position = 0;
            return File(ms.ToArray(), "application/zip", $"E0402-{sellerReceiptNo}.zip");
        }

        // ── helpers ──────────────────────────────────────────────────────────

        /// <summary>若尚無涵蓋本區間之主機構配號，登錄一筆（主機構 = 本區間開立人）。</summary>
        private void EnsureMainAssignment(InvoiceNoInterval item)
        {
            var exists = models!.GetTable<InvoiceNoMainAssignment>()
                .Any(m => m.TrackID == item.TrackID && m.MasterID == item.SellerID
                    && ((m.StartNo <= item.StartNo && m.EndNo >= item.StartNo)
                        || (m.StartNo <= item.EndNo && m.EndNo >= item.EndNo)));
            if (!exists)
            {
                models!.GetTable<InvoiceNoMainAssignment>().Add(new InvoiceNoMainAssignment
                {
                    TrackID = item.TrackID,
                    MasterID = item.SellerID,
                    StartNo = item.StartNo,
                    EndNo = item.EndNo,
                });
                models!.SubmitChanges();
            }
        }

        /// <summary>目前給號（等同 InvoiceNoInterval.CurrentAllocatingNo()，改以查詢取回避免載入整個明細集合）。</summary>
        private int CurrentAllocatingNo(InvoiceNoInterval item)
        {
            var maxAllocated = models!.GetTable<InvoiceNoAllocation>()
                .Where(a => a.IntervalID == item.IntervalID).Max(a => (int?)a.InvoiceNo);
            var maxAssigned = models!.GetTable<InvoiceNoAssignment>()
                .Where(a => a.IntervalID == item.IntervalID).Max(a => (int?)a.InvoiceNo);
            return Math.Max((maxAllocated + 1) ?? item.StartNo, (maxAssigned + 1) ?? item.StartNo);
        }

        private int? GetCurrentUid()
            => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : null;

        /// <summary>登入者是否為系統管理（可存取全部開立人）。</summary>
        private bool IsAdmin() => OrganizationScope.IsSystemAdmin(User.GetRoleId(), User.GetCategoryId());

        /// <summary>檢查登入者是否有權存取指定開立人（系統管理一律可；否則須在其角色範圍內）。</summary>
        private bool CanAccessSeller(int sellerId)
        {
            if (IsAdmin()) return true;
            var categoryId = User.GetCategoryId();
            var companyId = User.GetCompanyId();
            if (categoryId == null || companyId == null) return false;
            return OrganizationScope.AllowedOrganizations(models!, categoryId.Value, companyId.Value)
                .Any(o => o.CompanyID == sellerId);
        }

        /// <summary>配號區間輸入驗證（遷移自 InvoiceNoController.checkInput）。</summary>
        private List<string> ValidateInterval(InvoiceNoIntervalEditDto dto, int? sellerId, InvoiceNoInterval? model)
        {
            var errors = new List<string>();

            if (model == null)
            {
                if (!dto.TrackId.HasValue)
                {
                    errors.Add("字軌未設定!!");
                }
                if (!sellerId.HasValue)
                {
                    errors.Add("營業人錯誤!!");
                }
            }

            if (!dto.StartNo.HasValue || !(dto.StartNo >= 0 && dto.StartNo < 100000000))
            {
                errors.Add("起號非8位整數!!");
            }
            else if (!dto.EndNo.HasValue || !(dto.EndNo >= 0 && dto.EndNo < 100000000))
            {
                errors.Add("迄號非8位整數!!");
            }
            else if (dto.EndNo <= dto.StartNo || ((dto.EndNo - dto.StartNo + 1) % 50 != 0))
            {
                errors.Add("不符號碼大小順序與差距為50之倍數原則!!");
            }
            else
            {
                var startNo = dto.StartNo.Value;
                var endNo = dto.EndNo.Value;

                if (model != null)
                {
                    var used = models!.GetTable<InvoiceNoAssignment>().Any(a => a.IntervalID == model.IntervalID);
                    if (used)
                    {
                        errors.Add("該區間之號碼已經被使用,不可修改!!!!");
                    }
                    else
                    {
                        var overlap = models!.GetTable<InvoiceNoInterval>()
                            .Where(t => t.IntervalID != model.IntervalID && t.TrackID == model.TrackID)
                            .Where(t => (t.EndNo <= endNo && t.EndNo >= startNo)
                                || (t.StartNo <= endNo && t.StartNo >= startNo)
                                || (t.StartNo <= startNo && t.EndNo >= startNo)
                                || (t.StartNo <= endNo && t.EndNo >= endNo));
                        if (overlap.Any())
                        {
                            var receiptNo = overlap
                                .Select(t => t.InvoiceTrackCodeAssignment.Seller.ReceiptNo)
                                .FirstOrDefault();
                            errors.Add($"本區段營業人({receiptNo})已使用!!");
                        }
                    }
                }
                else if (dto.TrackId.HasValue)
                {
                    var trackId = dto.TrackId.Value;
                    var overlap = models!.GetTable<InvoiceNoInterval>()
                        .Where(t => t.TrackID == trackId)
                        .Where(t => (t.EndNo <= endNo && t.EndNo >= startNo)
                            || (t.StartNo <= endNo && t.StartNo >= startNo)
                            || (t.StartNo <= startNo && t.EndNo >= startNo)
                            || (t.StartNo <= endNo && t.EndNo >= endNo));
                    if (overlap.Any())
                    {
                        var receiptNo = overlap
                            .Select(t => t.InvoiceTrackCodeAssignment.Seller.ReceiptNo)
                            .FirstOrDefault();
                        errors.Add($"本區段營業人({receiptNo})已使用!!");
                    }
                }
            }

            return errors;
        }
    }
}
