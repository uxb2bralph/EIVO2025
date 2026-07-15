using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommonLib.Core.Utility;
using CommonLib.DataAccess;
using CommonLib.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelCore.DataEntity;
using ModelCore.DTOs;
using ModelCore.Helper;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Interfaces;
using TaskCenter.Core.Services;

namespace TaskCenter.Core.Controllers
{
    /// <summary>
    /// 相對營業人資料維護 API（遷移自 WebHome BusinessRelationshipController 之
    /// MaintainRelationship / InquireBusinessRelationship、列管理動作（CommitItem / DeleteItem /
    /// Activate / Deactivate / SetEntrusting / SetEntrustToPrint），
    /// 互動式新增（CommitBusinessRelationshipViewModel），以及範本下載 / Excel 匯入（UploadCounterpartBusiness）。
    /// 關係以複合鍵（MasterID + RelativeID + BusinessID）識別。
    /// 本站以系統管理身份運作（沿用其他已遷移頁面之做法，不套用舊版逐使用者集團範圍過濾）。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class BusinessRelationshipController : ApiBaseController
    {
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        // 匯入 / 範本之相對營業人資料表欄位（與舊版 UploadCounterpartBusiness 讀取、CreateXlsx 輸出一致）。
        private static readonly string[] CounterpartColumns =
        {
            "營業人名稱", "統一編號", "相對營業人名稱", "相對營業人統一編號",
            "聯絡人電子郵件", "地址", "電話", "客戶代碼",
        };

        private readonly IBusinessRelationshipService _businessRelationshipService;

        public BusinessRelationshipController(
            IBusinessRelationshipService businessRelationshipService,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
            _businessRelationshipService = businessRelationshipService;
        }

        /// <summary>
        /// 查詢相對營業人關係（分頁）。對應舊版 InquireBusinessRelationship。
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseDto<PagedResultDto<BusinessRelationshipDatatableDto>>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 500)]
        public async Task<IActionResult> GetList([FromQuery] BusinessRelationshipQueryDto queryDto)
        {
            try
            {
                var result = await _businessRelationshipService.GetPagedAsync(
                    queryDto ?? new BusinessRelationshipQueryDto(),
                    IsAdmin(), User.GetCategoryId(), User.GetCompanyId());
                return CreateSuccessResponse(result, "Common.Retrieved");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving business relationships");
                return CreateErrorResponse(500, "Common.RetrieveError");
            }
        }

        /// <summary>
        /// 取得集團成員（主營業人）下拉選項（對應舊版 GroupMemberSelector）。
        /// </summary>
        [HttpGet("GroupMembers")]
        [ProducesResponseType(typeof(ResponseDto<List<GroupMemberDto>>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 500)]
        public async Task<IActionResult> GroupMembers()
        {
            try
            {
                var result = await _businessRelationshipService.GetGroupMembersAsync(
                    IsAdmin(), User.GetCategoryId(), User.GetCompanyId());
                return CreateSuccessResponse(result, "Common.Retrieved");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving group members");
                return CreateErrorResponse(500, "Common.RetrieveError");
            }
        }

        /// <summary>
        /// 修改相對營業人可編輯欄位（遷移自舊版 BusinessRelationshipController.CommitItem）。
        /// 以複合鍵定位既有關係，僅更新名稱 / 電子郵件 / 地址 / 電話 / 客戶代碼（沿用舊版）。
        /// </summary>
        [HttpPost("CommitItem")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public IActionResult CommitItem([FromBody] BusinessRelationshipEditDto dto)
        {
            if (dto == null)
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            // 沿用舊版 CommitItem：營業人名稱不可為空白。
            var companyName = dto.CompanyName.GetEfficientString();
            if (companyName == null)
            {
                return CreateBadRequestResponse("Common.SaveError", new[] { "營業人名稱格式錯誤" });
            }

            if (!CanAccessSeller(dto.MasterId)) return CreateErrorResponse(403, "無權存取此營業人資料!!");

            var item = models!.GetTable<BusinessRelationship>()
                .FirstOrDefault(b => b.MasterID == dto.MasterId
                    && b.RelativeID == dto.RelativeId
                    && b.BusinessID == dto.BusinessId);

            if (item == null)
            {
                return CreateBadRequestResponse("資料錯誤!!");
            }

            item.CompanyName = companyName;
            item.ContactEmail = dto.ContactEmail.GetEfficientString();
            item.Addr = dto.Addr.GetEfficientString();
            item.Phone = dto.Phone.GetEfficientString();
            item.CustomerNo = dto.CustomerNo.GetEfficientString();

            models.SubmitChanges();

            return CreateSuccessResponse("Common.Saved");
        }

        /// <summary>
        /// 新增相對營業人（對應舊版 AddItem 列 → CommitBusinessRelationshipViewModel）。
        /// 以所選集團成員為主營業人、統一編號定位（或新建）相對營業人 Organization，建立關係。
        /// </summary>
        [HttpPost("AddItem")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public IActionResult AddItem([FromBody] BusinessRelationshipAddDto dto)
        {
            if (dto == null)
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            if (!dto.MasterCompanyId.HasValue)
            {
                return CreateBadRequestResponse("Common.SaveError", new[] { "請選擇集團成員!!" });
            }
            if (!CanAccessSeller(dto.MasterCompanyId.Value)) return CreateErrorResponse(403, "無權存取此營業人資料!!");

            var receiptNo = dto.ReceiptNo.GetEfficientString();
            if (receiptNo == null)
            {
                return CreateBadRequestResponse("Common.SaveError", new[] { "請輸入相對營業人統一編號!!" });
            }

            var companyName = dto.CompanyName.GetEfficientString();
            if (companyName == null)
            {
                return CreateBadRequestResponse("Common.SaveError", new[] { "請輸入相對營業人名稱!!" });
            }

            var viewModel = new BusinessRelationshipViewModel
            {
                MasterID = dto.MasterCompanyId,
                ReceiptNo = receiptNo,
                CompanyName = companyName,
                ContactEmail = dto.ContactEmail.GetEfficientString(),
                Addr = dto.Addr.GetEfficientString(),
                Phone = dto.Phone.GetEfficientString(),
                CustomerNo = dto.CustomerNo.GetEfficientString(),
                BusinessID = (Naming.InvoiceCenterBusinessType?)dto.BusinessType ?? Naming.InvoiceCenterBusinessType.銷項,
                SettingInvoiceType = Naming.InvoiceTypeDefinition.一般稅額計算之電子發票,
            };

            var item = viewModel.CommitBusinessRelationshipViewModel(models!, ModelState);
            if (item == null)
            {
                var errors = ModelState
                    .Where(kv => kv.Value != null && kv.Value.Errors.Count > 0)
                    .SelectMany(kv => kv.Value!.Errors.Select(e => e.ErrorMessage))
                    .Where(m => !string.IsNullOrWhiteSpace(m))
                    .ToList();
                if (errors.Count == 0)
                {
                    errors.Add("資料錯誤!!");
                }
                return CreateBadRequestResponse("Common.SaveError", errors);
            }

            return CreateSuccessResponse("Common.Saved");
        }

        /// <summary>
        /// 刪除相對營業人關係（遷移自舊版 BusinessRelationshipController.DeleteItem）。
        /// 以複合鍵（BusinessID + MasterID + RelativeID）刪除。
        /// </summary>
        [HttpPost("DeleteItem")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public IActionResult DeleteItem([FromQuery] int businessId, [FromQuery] int masterId, [FromQuery] int relativeId)
        {
            if (!CanAccessSeller(masterId)) return CreateErrorResponse(403, "無權存取此營業人資料!!");

            // 沿用舊版原生 SQL 刪除。
            var count = models!.ExecuteCommand(
                "delete center.BusinessRelationship where MasterID = {0} and RelativeID = {1} and BusinessID = {2}",
                masterId, relativeId, businessId);

            if (count == 0)
            {
                return CreateBadRequestResponse("資料錯誤");
            }

            return CreateSuccessResponse("Common.Saved");
        }

        /// <summary>
        /// 停用相對營業人關係（遷移自舊版 Deactivate）。將 CurrentLevel 設為 Mark_To_Delete。
        /// </summary>
        [HttpPost("Deactivate")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult Deactivate([FromQuery] int businessId, [FromQuery] int masterId, [FromQuery] int relativeId)
            => SetLevel(businessId, masterId, relativeId, Naming.MemberStatusDefinition.Mark_To_Delete);

        /// <summary>
        /// 啟用相對營業人關係（遷移自舊版 Activate）。將 CurrentLevel 設為 Checked。
        /// </summary>
        [HttpPost("Activate")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult Activate([FromQuery] int businessId, [FromQuery] int masterId, [FromQuery] int relativeId)
            => SetLevel(businessId, masterId, relativeId, Naming.MemberStatusDefinition.Checked);

        /// <summary>
        /// 設定相對營業人自動接收（遷移自舊版 SetEntrusting）。更新相對營業人 OrganizationStatus.Entrusting。
        /// </summary>
        [HttpPost("SetEntrusting")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult SetEntrusting([FromQuery] int businessId, [FromQuery] int masterId, [FromQuery] int relativeId, [FromQuery] bool status)
        {
            if (!CanAccessSeller(masterId)) return CreateErrorResponse(403, "無權存取此營業人資料!!");

            var status0 = LoadRelativeStatus(businessId, masterId, relativeId);
            if (status0 == null)
            {
                return CreateNotFoundResponse("營業人資料錯誤!!");
            }

            status0.Entrusting = status;
            models!.SubmitChanges();
            return CreateSuccessResponse("Common.Saved");
        }

        /// <summary>
        /// 設定相對營業人主動列印（遷移自舊版 SetEntrustToPrint）。更新相對營業人 OrganizationStatus.EntrustToPrint。
        /// </summary>
        [HttpPost("SetEntrustToPrint")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult SetEntrustToPrint([FromQuery] int businessId, [FromQuery] int masterId, [FromQuery] int relativeId, [FromQuery] bool status)
        {
            if (!CanAccessSeller(masterId)) return CreateErrorResponse(403, "無權存取此營業人資料!!");

            var status0 = LoadRelativeStatus(businessId, masterId, relativeId);
            if (status0 == null)
            {
                return CreateNotFoundResponse("營業人資料錯誤!!");
            }

            status0.EntrustToPrint = status;
            models!.SubmitChanges();
            return CreateSuccessResponse("Common.Saved");
        }

        /// <summary>
        /// 下載相對營業人資料範本（對應舊版維護頁「下載範本」，格式同 CreateXlsx / UploadCounterpartBusiness）。
        /// 產生含「相對營業人」工作表（8 欄位）的空白範本 Excel。
        /// </summary>
        [HttpGet("DownloadTemplate")]
        [Produces(ExcelContentType)]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        public IActionResult DownloadTemplate()
        {
            var table = new DataTable("相對營業人");
            foreach (var col in CounterpartColumns)
            {
                table.Columns.Add(new DataColumn(col, typeof(string)));
            }

            using var ds = new DataSet();
            ds.Tables.Add(table);

            using var xls = ds.ConvertToExcel();
            using var ms = new MemoryStream();
            xls.SaveAs(ms);

            return File(ms.ToArray(), ExcelContentType, "相對營業人範本.xlsx");
        }

        /// <summary>
        /// 匯入相對營業人 Excel（遷移自舊版 UploadCounterpartBusiness）。
        /// 讀取「相對營業人」工作表，逐列以 CommitBusinessRelationshipViewModel 新增 / 更新，
        /// 於原資料附加「處理狀態」欄後同步回傳結果檔。成功回傳結果 Excel；未選檔 / 無資料表則回傳 JSON 錯誤。
        /// </summary>
        /// <param name="excelFile">相對營業人資料 Excel（單一檔案，須含「相對營業人」工作表）</param>
        /// <param name="businessType">營業人類別（1=銷項、2=進項）；套用至所有匯入列，null 預設銷項。</param>
        [HttpPost("UploadCounterpart")]
        [Produces(ExcelContentType, "application/json")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 500)]
        public IActionResult UploadCounterpart(IFormFile? excelFile, [FromForm] int? businessType)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                return CreateBadRequestResponse("未選取檔案或檔案上傳失敗!!");
            }

            try
            {
                // 儲存上傳檔至當日記錄目錄（沿用舊版以 Ticks 前綴避免檔名衝突）。
                var fileName = Path.Combine(
                    CommonLib.Core.Utility.Logger.LogDailyPath,
                    $"{DateTime.Now.Ticks}_{Path.GetFileName(excelFile.FileName)}");
                using (var fs = new FileStream(fileName, FileMode.Create))
                {
                    excelFile.CopyTo(fs);
                }

                using var ds = fileName.ImportExcelXLS();
                var table = ds.Tables.Count == 0
                    ? null
                    : ds.Tables.Cast<DataTable>().FirstOrDefault(t => t.TableName.Contains("相對營業人"));
                if (table == null)
                {
                    return CreateBadRequestResponse("Excel檔未包含【相對營業人】資料表!!");
                }

                table.Columns.Add(new DataColumn("處理狀態", typeof(string)));
                var statusIdx = table.Columns.Count - 1;

                var businessId = (Naming.InvoiceCenterBusinessType?)businessType ?? Naming.InvoiceCenterBusinessType.銷項;

                // 非系統管理者：僅允許匯入其角色範圍內主營業人（統編）之資料列，其餘標記略過（對應舊版 available 白名單）。
                HashSet<string>? allowedMasterNos = null;
                if (!IsAdmin())
                {
                    var categoryId = User.GetCategoryId();
                    var companyId = User.GetCompanyId();
                    if (categoryId == null || companyId == null)
                    {
                        return CreateErrorResponse(403, "無權操作!!");
                    }
                    allowedMasterNos = OrganizationScope.AllowedOrganizations(models!, categoryId.Value, companyId.Value)
                        .Where(o => o.ReceiptNo != null)
                        .Select(o => o.ReceiptNo!)
                        .ToHashSet();
                }

                foreach (DataRow row in table.Rows)
                {
                    try
                    {
                        if (allowedMasterNos != null)
                        {
                            var masterNo = row.GetString("統一編號").GetEfficientString();
                            if (masterNo == null || !allowedMasterNos.Contains(masterNo))
                            {
                                row[statusIdx] = "非所屬營業人，略過";
                                continue;
                            }
                        }

                        var viewModel = new BusinessRelationshipViewModel
                        {
                            MasterName = row.GetString("營業人名稱"),
                            MasterNo = row.GetString("統一編號"),
                            CompanyName = row.GetString("相對營業人名稱"),
                            ReceiptNo = row.GetString("相對營業人統一編號"),
                            ContactEmail = row.GetString("聯絡人電子郵件"),
                            Addr = row.GetString("地址"),
                            Phone = row.GetString("電話"),
                            CustomerNo = row.GetString("客戶代碼"),
                            BusinessID = businessId,
                            SettingInvoiceType = Naming.InvoiceTypeDefinition.一般稅額計算之電子發票,
                        };

                        ModelState.Clear();
                        viewModel.CommitBusinessRelationshipViewModel(models!, ModelState);

                        if (!ModelState.IsValid)
                        {
                            var errors = ModelState
                                .Where(kv => kv.Value != null && kv.Value.Errors.Count > 0)
                                .SelectMany(kv => kv.Value!.Errors.Select(e => e.ErrorMessage))
                                .Where(m => !string.IsNullOrWhiteSpace(m));
                            row[statusIdx] = string.Join("、", errors);
                        }
                    }
                    catch (Exception ex)
                    {
                        row[statusIdx] = ex.Message;
                    }
                }

                using var xls = ds.ConvertToExcel();
                using var ms = new MemoryStream();
                xls.SaveAs(ms);

                return File(ms.ToArray(), ExcelContentType, "相對營業人(回應).xlsx");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error uploading counterpart business excel");
                return CreateErrorResponse(500, ex.Message);
            }
        }

        /// <summary>登入者是否為系統管理（可存取全部主營業人）。</summary>
        private bool IsAdmin() => OrganizationScope.IsSystemAdmin(User.GetRoleId(), User.GetCategoryId());

        /// <summary>是否有權以指定主營業人（MasterID）操作（系統管理一律可；否則須在其角色範圍內）。</summary>
        private bool CanAccessSeller(int masterId)
            => OrganizationScope.CanAccessSeller(models!, User.GetRoleId(), User.GetCategoryId(), User.GetCompanyId(), masterId);

        /// <summary>
        /// 更新關係狀態 CurrentLevel（啟用 / 停用共用）。
        /// </summary>
        private IActionResult SetLevel(int businessId, int masterId, int relativeId, Naming.MemberStatusDefinition level)
        {
            if (!CanAccessSeller(masterId)) return CreateErrorResponse(403, "無權存取此營業人資料!!");

            var item = models!.GetTable<BusinessRelationship>()
                .FirstOrDefault(b => b.MasterID == masterId && b.RelativeID == relativeId && b.BusinessID == businessId);
            if (item == null)
            {
                return CreateNotFoundResponse("營業人資料錯誤!!");
            }

            item.CurrentLevel = (int)level;
            models.SubmitChanges();
            return CreateSuccessResponse("Common.Saved");
        }

        /// <summary>
        /// 載入相對營業人（Relative）的 OrganizationStatus，供自動接收 / 主動列印切換使用。
        /// </summary>
        private OrganizationStatus? LoadRelativeStatus(int businessId, int masterId, int relativeId)
        {
            var item = models!.GetTable<BusinessRelationship>()
                .FirstOrDefault(b => b.MasterID == masterId && b.RelativeID == relativeId && b.BusinessID == businessId);
            if (item == null)
            {
                return null;
            }

            return models.GetTable<Organization>()
                .Where(o => o.CompanyID == relativeId)
                .Select(o => o.OrganizationStatus)
                .FirstOrDefault();
        }
    }
}
