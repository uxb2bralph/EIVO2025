using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CommonLib.Core.Utility;
using CommonLib.DataAccess;
using CommonLib.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelCore.DataEntity;
using ModelCore.DTOs;
using ModelCore.Helper;
using ModelCore.Locale;
using TaskCenter.Core.DTOs;

namespace TaskCenter.Core.Controllers
{
    /// <summary>
    /// 營業人客製化設定 API（OrganizationCustomSetting.SettingData）。
    /// 提供 E0501 自動取號設定（遷移自 WebHome Organization/ApplyE0501Settings.cshtml），
    /// 以及主機構批次配號設定（Settings.BranchInvoiceNoAssignments）之 Excel 範本下載 / 匯入。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [SysAdminOnly]
    [Produces("application/json")]
    public class OrganizationSettingsController : ApiBaseController
    {
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        /// <summary>主機構批次配號工作表名稱（範本產生與匯入比對共用）</summary>
        private const string BranchAssignmentSheetName = "配號設定";

        private const string ColumnReceiptNo = "分支機構統一編號";
        private const string ColumnBooklets = "配號本組數";
        private const string ColumnInitialLock = "預設鎖定";

        public OrganizationSettingsController(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
        }

        /// <summary>
        /// 載入 E0501 自動取號設定（遷移自舊版 ApplyE0501Settings.cshtml 之顯示分支）。
        /// 尚未建立 OrganizationCustomSetting 時回傳預設值（停用 = 否、預設鎖定 = 否、保留本組數 = 空）。
        /// 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法。
        /// </summary>
        /// <param name="keyId">加密後的 CompanyID（來自營業人列表 / 編輯表單的 keyId 欄位）</param>
        [HttpGet("E0501")]
        [ProducesResponseType(typeof(ResponseDto<E0501SettingsDto>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult E0501([FromQuery] string keyId)
        {
            if (!TryResolveCompany(keyId, out var companyId))
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            if (!models!.GetTable<Organization>().Any(o => o.CompanyID == companyId))
            {
                return CreateNotFoundResponse("資料錯誤!!");
            }

            var custSettings = models.GetTable<OrganizationCustomSetting>()
                .Where(s => s.CompanyID == companyId)
                .FirstOrDefault();

            var dto = new E0501SettingsDto { KeyId = keyId };
            if (custSettings != null)
            {
                var settings = custSettings.Settings;
                dto.DisableAutoUpdate = settings.DisableE0501AutoUpdate == Naming.Truth.True;
                dto.InitialLock = settings.E0501InitialLock == Naming.Truth.True;
                dto.ReservedBooklets = settings.E0501ReservedBooklets;
                dto.BranchAssignments = BuildAssignmentDtos(settings.BranchInvoiceNoAssignments, companyId);
            }

            return CreateSuccessResponse(dto, "Common.Retrieved");
        }

        /// <summary>
        /// 儲存 E0501 自動取號設定（遷移自舊版 ApplyE0501Settings.cshtml 之 CustomSettings 提交分支）。
        /// 沿用舊版：設定值存於 OrganizationCustomSetting.SettingData(JSON)，不存在時新增；
        /// 僅異動 E0501 相關三個欄位，同一份 JSON 內的其餘設定原樣保留。
        /// </summary>
        [HttpPost("CommitE0501")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult CommitE0501([FromBody] E0501SettingsDto dto)
        {
            if (dto == null || !TryResolveCompany(dto.KeyId, out var companyId))
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            if (dto.ReservedBooklets < 0)
            {
                return CreateBadRequestResponse("Common.SaveError", new[] { "保留本組數不得小於 0!!" });
            }

            if (!models!.GetTable<Organization>().Any(o => o.CompanyID == companyId))
            {
                return CreateNotFoundResponse("資料錯誤!!");
            }

            var custSettings = models.GetTable<OrganizationCustomSetting>()
                .Where(s => s.CompanyID == companyId)
                .FirstOrDefault();

            if (custSettings == null)
            {
                custSettings = new OrganizationCustomSetting { CompanyID = companyId };
                models.GetTable<OrganizationCustomSetting>().Add(custSettings);
            }

            // Settings 為 [NotMapped] 的 JSON 投影，異動後須 Accept() 回寫 SettingData 才會被異動追蹤。
            custSettings.Settings.DisableE0501AutoUpdate = dto.DisableAutoUpdate ? Naming.Truth.True : Naming.Truth.False;
            custSettings.Settings.E0501InitialLock = dto.InitialLock ? Naming.Truth.True : Naming.Truth.False;
            custSettings.Settings.E0501ReservedBooklets = dto.ReservedBooklets;
            custSettings.Accept();

            models.SubmitChanges();

            return CreateSuccessResponse("Common.Saved");
        }

        /// <summary>
        /// 下載主機構批次配號範本（Excel）。
        /// 「配號設定」工作表之資料列即 BranchInvoiceNoAssignmentModel（分支機構統一編號 / 配號本組數 / 預設鎖定），
        /// 已有設定時帶出現行內容，否則以該主機構的分支機構清單預填；另附「分支機構」工作表供對照。
        /// </summary>
        /// <param name="keyId">加密後的 CompanyID（來自營業人列表 / 編輯表單的 keyId 欄位）</param>
        [HttpGet("BranchAssignmentTemplate")]
        [Produces(ExcelContentType, "application/json")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult BranchAssignmentTemplate([FromQuery] string keyId)
        {
            if (!TryResolveCompany(keyId, out var companyId))
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            if (!models!.GetTable<Organization>().Any(o => o.CompanyID == companyId))
            {
                return CreateNotFoundResponse("資料錯誤!!");
            }

            var branches = QueryBranches(companyId);
            var assignments = models.GetTable<OrganizationCustomSetting>()
                .Where(s => s.CompanyID == companyId)
                .FirstOrDefault()?.Settings.BranchInvoiceNoAssignments;

            var settingTable = new DataTable(BranchAssignmentSheetName);
            settingTable.Columns.Add(new DataColumn(ColumnReceiptNo, typeof(string)));
            settingTable.Columns.Add(new DataColumn(ColumnBooklets, typeof(int)));
            settingTable.Columns.Add(new DataColumn(ColumnInitialLock, typeof(string)));

            // 已有設定時帶出現行內容供修改；尚未設定則以分支機構清單預填空白配號列。
            if (assignments?.Length > 0)
            {
                foreach (var item in assignments)
                {
                    var row = settingTable.NewRow();
                    row[0] = item.ReceiptNo ?? string.Empty;
                    if (item.Booklets.HasValue)
                    {
                        row[1] = item.Booklets.Value;
                    }
                    row[2] = item.InitialLock == Naming.Truth.True ? "是" : "否";
                    settingTable.Rows.Add(row);
                }
            }
            else
            {
                foreach (var branch in branches)
                {
                    var row = settingTable.NewRow();
                    row[0] = branch.ReceiptNo ?? string.Empty;
                    row[2] = "否";
                    settingTable.Rows.Add(row);
                }
            }

            // 「分支機構」工作表：可配號的分支機構統編與名稱（僅供對照，匯入時不讀取）。
            var branchTable = new DataTable("分支機構");
            branchTable.Columns.Add(new DataColumn("統一編號", typeof(string)));
            branchTable.Columns.Add(new DataColumn("分支機構名稱", typeof(string)));
            foreach (var branch in branches)
            {
                var row = branchTable.NewRow();
                row[0] = branch.ReceiptNo ?? string.Empty;
                row[1] = branch.CompanyName ?? string.Empty;
                branchTable.Rows.Add(row);
            }

            using var ds = new DataSet();
            ds.Tables.Add(settingTable);
            ds.Tables.Add(branchTable);

            using var xls = ds.ConvertToExcel();
            using var ms = new MemoryStream();
            xls.SaveAs(ms);

            return File(ms.ToArray(), ExcelContentType, "主機構批次配號範本.xlsx");
        }

        /// <summary>
        /// 匯入主機構批次配號 Excel（立即傳送）。
        /// 讀取「配號設定」工作表，逐列轉為 BranchInvoiceNoAssignmentModel 後【整批取代】
        /// Settings.BranchInvoiceNoAssignments；同一份設定 JSON 內的 E0501 等其餘項目原樣保留。
        /// 任一列驗證失敗即整批不儲存，並回傳各列錯誤訊息。
        /// </summary>
        /// <param name="excelFile">配號設定 Excel（單一檔案，須含「配號設定」工作表）</param>
        /// <param name="keyId">加密後的 CompanyID</param>
        [HttpPost("UploadBranchAssignments")]
        [ProducesResponseType(typeof(ResponseDto<BranchInvoiceNoAssignmentDto[]>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        [ProducesResponseType(typeof(BaseResponseDto), 500)]
        public IActionResult UploadBranchAssignments(IFormFile? excelFile, [FromForm] string? keyId)
        {
            if (!TryResolveCompany(keyId, out var companyId))
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            if (excelFile == null || excelFile.Length == 0)
            {
                return CreateBadRequestResponse("未選取檔案或檔案上傳失敗!!");
            }

            if (!models!.GetTable<Organization>().Any(o => o.CompanyID == companyId))
            {
                return CreateNotFoundResponse("資料錯誤!!");
            }

            try
            {
                // 儲存上傳檔至當日記錄目錄（沿用既有匯入功能以 Ticks 前綴避免檔名衝突）。
                var fileName = Path.Combine(
                    CommonLib.Core.Utility.Logger.LogDailyPath,
                    $"{DateTime.Now.Ticks}_{Path.GetFileName(excelFile.FileName)}");
                using (var fs = new FileStream(fileName, FileMode.Create))
                {
                    excelFile.CopyTo(fs);
                }

                using var ds = fileName.ImportExcelByClosedXML();
                var table = ds.Tables.Count == 0
                    ? null
                    : ds.Tables.Cast<DataTable>().FirstOrDefault(t => t.TableName.Contains(BranchAssignmentSheetName));
                if (table == null)
                {
                    return CreateBadRequestResponse($"Excel檔未包含【{BranchAssignmentSheetName}】資料表!!");
                }

                if (!table.Columns.Contains(ColumnReceiptNo))
                {
                    return CreateBadRequestResponse($"【{BranchAssignmentSheetName}】資料表未包含【{ColumnReceiptNo}】欄位!!");
                }

                // 可配號對象：本主機構名下的分支機構（統編 → 名稱）。
                var branchNames = QueryBranchNames(companyId);

                var errors = new List<string>();
                var assignments = new List<BranchInvoiceNoAssignmentModel>();
                var appeared = new HashSet<string>();

                for (int idx = 0; idx < table.Rows.Count; idx++)
                {
                    // 對應使用者在 Excel 中看到的列號（第 1 列為標題）。
                    var rowNo = idx + 2;
                    var row = table.Rows[idx];

                    var receiptNo = row.GetString(ColumnReceiptNo).GetEfficientString();
                    var bookletsText = table.Columns.Contains(ColumnBooklets)
                        ? row.GetString(ColumnBooklets).GetEfficientString()
                        : null;
                    var lockText = table.Columns.Contains(ColumnInitialLock)
                        ? row.GetString(ColumnInitialLock).GetEfficientString()
                        : null;

                    // 完全空白列視為結尾補白，直接略過。
                    if (receiptNo == null && bookletsText == null && lockText == null)
                    {
                        continue;
                    }

                    if (receiptNo == null)
                    {
                        errors.Add($"第 {rowNo} 列：{ColumnReceiptNo}不得空白");
                        continue;
                    }

                    if (!branchNames.ContainsKey(receiptNo))
                    {
                        errors.Add($"第 {rowNo} 列：統一編號 {receiptNo} 非本主機構之分支機構");
                        continue;
                    }

                    if (!appeared.Add(receiptNo))
                    {
                        errors.Add($"第 {rowNo} 列：統一編號 {receiptNo} 重複");
                        continue;
                    }

                    int? booklets = null;
                    if (bookletsText != null)
                    {
                        // OLEDB 可能將數值欄讀為浮點數（如「10」讀成「10.0」），故先轉 decimal 再檢查是否為整數。
                        if (!decimal.TryParse(bookletsText, out var value) || value != decimal.Truncate(value))
                        {
                            errors.Add($"第 {rowNo} 列：{ColumnBooklets}【{bookletsText}】非有效整數");
                            continue;
                        }

                        if (value < 0)
                        {
                            errors.Add($"第 {rowNo} 列：{ColumnBooklets}不得小於 0");
                            continue;
                        }

                        booklets = (int)value;
                    }

                    assignments.Add(new BranchInvoiceNoAssignmentModel
                    {
                        ReceiptNo = receiptNo,
                        Booklets = booklets,
                        InitialLock = ParseTruth(lockText) ? Naming.Truth.True : Naming.Truth.False,
                    });
                }

                if (errors.Count > 0)
                {
                    return CreateBadRequestResponse("配號設定資料有誤，未儲存!!", errors);
                }

                if (assignments.Count == 0)
                {
                    return CreateBadRequestResponse($"【{BranchAssignmentSheetName}】資料表無資料列!!");
                }

                var custSettings = models.GetTable<OrganizationCustomSetting>()
                    .Where(s => s.CompanyID == companyId)
                    .FirstOrDefault();

                if (custSettings == null)
                {
                    custSettings = new OrganizationCustomSetting { CompanyID = companyId };
                    models.GetTable<OrganizationCustomSetting>().Add(custSettings);
                }

                // Settings 為 [NotMapped] 的 JSON 投影，異動後須 Accept() 回寫 SettingData 才會被異動追蹤。
                custSettings.Settings.BranchInvoiceNoAssignments = assignments.ToArray();
                custSettings.Accept();

                models.SubmitChanges();

                return CreateSuccessResponse(
                    BuildAssignmentDtos(custSettings.Settings.BranchInvoiceNoAssignments, companyId),
                    $"已匯入 {assignments.Count} 筆配號設定!!");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error uploading branch invoice no assignments");
                return CreateErrorResponse(500, ex.Message);
            }
        }

        /// <summary>
        /// 查詢主機構名下的分支機構（InvoiceIssuerAgent 之 MasterBranch 關聯：Agent = 主機構、Issuer = 分支機構）。
        /// </summary>
        private List<BranchInfo> QueryBranches(int companyId)
        {
            return models!.GetTable<InvoiceIssuerAgent>()
                .Where(a => a.AgentID == companyId
                    && a.RelationType == (int)InvoiceIssuerAgent.RelationTypeEnum.MasterBranch)
                .Select(a => new BranchInfo
                {
                    ReceiptNo = a.Issuer.ReceiptNo,
                    CompanyName = a.Issuer.CompanyName,
                })
                .AsNoTracking()
                .ToList();
        }

        /// <summary>
        /// 分支機構統編 → 名稱對照（同一統編重複登錄時取第一筆）。
        /// </summary>
        private Dictionary<string, string?> QueryBranchNames(int companyId)
        {
            return QueryBranches(companyId)
                .Where(b => b.ReceiptNo != null)
                .GroupBy(b => b.ReceiptNo!)
                .ToDictionary(g => g.Key, g => g.First().CompanyName);
        }

        /// <summary>
        /// 將設定 JSON 中的配號資料轉為 DTO，並以分支機構清單補上名稱（不在清單中者名稱留空）。
        /// </summary>
        private BranchInvoiceNoAssignmentDto[] BuildAssignmentDtos(
            BranchInvoiceNoAssignmentModel[]? assignments, int companyId)
        {
            if (assignments == null || assignments.Length == 0)
            {
                return [];
            }

            var branchNames = QueryBranchNames(companyId);

            return assignments
                .Select(item => new BranchInvoiceNoAssignmentDto
                {
                    ReceiptNo = item.ReceiptNo,
                    CompanyName = item.ReceiptNo != null && branchNames.TryGetValue(item.ReceiptNo, out var name)
                        ? name
                        : null,
                    Booklets = item.Booklets,
                    InitialLock = item.InitialLock == Naming.Truth.True,
                })
                .ToArray();
        }

        /// <summary>
        /// 解讀 Excel 的是 / 否欄位；空白視為否。
        /// </summary>
        private static bool ParseTruth(string? text)
        {
            if (text == null)
            {
                return false;
            }

            return text switch
            {
                "是" or "Y" or "y" or "1" or "V" or "v" or "true" or "TRUE" or "True" => true,
                _ => false,
            };
        }

        /// <summary>分支機構統編與名稱（配號驗證與名稱顯示用）</summary>
        private class BranchInfo
        {
            public string? ReceiptNo { get; set; }
            public string? CompanyName { get; set; }
        }

        /// <summary>
        /// 解密 KeyID 取得 CompanyID；解密失敗回傳 false 並記錄警告。
        /// </summary>
        private bool TryResolveCompany(string? keyId, out int companyId)
        {
            companyId = 0;
            if (string.IsNullOrWhiteSpace(keyId))
            {
                return false;
            }

            try
            {
                companyId = keyId.DecryptKeyValue();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Invalid organization keyId");
                return false;
            }
        }
    }
}
