using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CommonLib.Utility;
using ModelCore.DataEntity;
using ModelCore.DTOs;
using ModelCore.Helper;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Interfaces;

namespace TaskCenter.Core.Controllers
{
    /// <summary>
    /// 營業人資料查詢 API（遷移自 WebHome OrganizationQueryController）。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [SysAdminOnly]
    [Produces("application/json")]
    public class OrganizationQueryController : ApiBaseController
    {
        private readonly IOrganizationQueryService _organizationQueryService;

        public OrganizationQueryController(
            IOrganizationQueryService organizationQueryService,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
            _organizationQueryService = organizationQueryService;
        }

        /// <summary>
        /// 查詢營業人資料（分頁）。
        /// </summary>
        /// <param name="queryDto">查詢條件</param>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseDto<PagedResultDto<OrganizationDatatableDto>>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 500)]
        public async Task<IActionResult> GetList([FromQuery] OrganizationQueryDto queryDto)
        {
            try
            {
                var result = await _organizationQueryService.GetPagedAsync(queryDto);
                return CreateSuccessResponse(result, "Common.Retrieved");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving organizations");
                return CreateErrorResponse(500, "Common.RetrieveError");
            }
        }

        /// <summary>
        /// 依關鍵字搜尋所屬經銷商候選清單（類別為經銷商的營業人），供前端 autocomplete 使用。
        /// 對應舊版 InquireOrganization.cshtml 之 AgentID 下拉，改以統編/名稱關鍵字即時查詢。
        /// </summary>
        /// <param name="keyword">統編前綴或名稱關鍵字；為空時回傳前 N 筆</param>
        [HttpGet("Agents")]
        [ProducesResponseType(typeof(ResponseDto<List<OrganizationAgentDto>>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 500)]
        public async Task<IActionResult> Agents([FromQuery] string? keyword)
        {
            try
            {
                var result = await _organizationQueryService.SearchAgentsAsync(keyword);
                return CreateSuccessResponse(result, "Common.Retrieved");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving organization agents");
                return CreateErrorResponse(500, "Common.RetrieveError");
            }
        }

        /// <summary>
        /// 依關鍵字搜尋主機構候選清單（已設定為主機構的營業人），供「設為分支機構」選擇主機構的 autocomplete 使用。
        /// 對應舊版 Home/SearchHeadquarter。
        /// </summary>
        /// <param name="keyword">統編前綴或名稱關鍵字；為空時回傳空集合</param>
        [HttpGet("Headquarters")]
        [ProducesResponseType(typeof(ResponseDto<List<HeadquarterDto>>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 500)]
        public async Task<IActionResult> Headquarters([FromQuery] string? keyword)
        {
            try
            {
                var result = await _organizationQueryService.SearchHeadquartersAsync(keyword);
                return CreateSuccessResponse(result, "Common.Retrieved");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving headquarters");
                return CreateErrorResponse(500, "Common.RetrieveError");
            }
        }

        /// <summary>
        /// 載入單一營業人編輯資料（遷移自 OrganizationController.EditItem）。
        /// 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法。
        /// </summary>
        /// <param name="keyId">加密後的 CompanyID（來自列表 keyId 欄位）</param>
        [HttpGet("EditItem")]
        [ProducesResponseType(typeof(ResponseDto<OrganizationEditDto>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult EditItem([FromQuery] string keyId)
        {
            if (string.IsNullOrWhiteSpace(keyId))
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            int companyId;
            try
            {
                companyId = keyId.DecryptKeyValue();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Invalid organization keyId");
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            var item = models!.GetTable<Organization>()
                .Where(o => o.CompanyID == companyId)
                .FirstOrDefault();

            if (item == null)
            {
                return CreateNotFoundResponse("Organization.NotFound");
            }

            // 重用舊版 ApplyFromModel 將實體攤平到 ViewModel，再投影為前端 DTO。
            var viewModel = new OrganizationViewModel().ApplyFromModel(item);
            var dto = MapToEditDto(viewModel, keyId);
            return CreateSuccessResponse(dto, "Common.Retrieved");
        }

        /// <summary>
        /// 儲存營業人編輯資料（遷移自 OrganizationController.CommitItem）。
        /// 重用舊版 CommitOrganizationViewModel 的驗證與寫入邏輯。
        /// </summary>
        [HttpPost("CommitItem")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public IActionResult CommitItem([FromBody] OrganizationEditDto dto)
        {
            if (dto == null)
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            var viewModel = MapToViewModel(dto);
            var item = viewModel.CommitOrganizationViewModel(models!, ModelState);

            if (item == null)
            {
                var errors = ModelState
                    .Where(kv => kv.Value != null && kv.Value.Errors.Count > 0)
                    .SelectMany(kv => kv.Value!.Errors.Select(e => e.ErrorMessage))
                    .Where(m => !string.IsNullOrWhiteSpace(m))
                    .ToList();
                return CreateBadRequestResponse("Common.SaveError", errors);
            }

            return CreateSuccessResponse("Common.Saved");
        }

        /// <summary>
        /// 停用營業人（遷移自 WebHome HandlingController.DisableCompany）。
        /// 將營業人狀態註記為停用（Mark_To_Delete）。
        /// 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法。
        /// </summary>
        /// <param name="keyId">加密後的 CompanyID（來自列表 keyId 欄位）</param>
        [HttpPost("DisableItem")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult DisableItem([FromQuery] string keyId)
        {
            if (string.IsNullOrWhiteSpace(keyId))
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            int companyId;
            try
            {
                companyId = keyId.DecryptKeyValue();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Invalid organization keyId");
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            // 沿用舊版 updateCompanyStatus：僅更新營業人狀態的 CurrentLevel。
            var status = models!.GetTable<Organization>()
                .Where(o => o.CompanyID == companyId)
                .Select(o => o.OrganizationStatus)
                .FirstOrDefault();

            if (status == null)
            {
                return CreateNotFoundResponse("Organization.NotFound");
            }

            status.CurrentLevel = (int)Naming.MemberStatusDefinition.Mark_To_Delete;
            models.SubmitChanges();

            return CreateSuccessResponse("Common.Saved");
        }

        /// <summary>
        /// 啟用營業人：將已註記停用（Mark_To_Delete）的營業人狀態改回啟用（Checked）。
        /// 沿用停用做法，以加密 KeyID 傳遞 CompanyID。
        /// </summary>
        /// <param name="keyId">加密後的 CompanyID（來自列表 keyId 欄位）</param>
        [HttpPost("EnableItem")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult EnableItem([FromQuery] string keyId)
        {
            if (string.IsNullOrWhiteSpace(keyId))
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            int companyId;
            try
            {
                companyId = keyId.DecryptKeyValue();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Invalid organization keyId");
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            // 沿用舊版 updateCompanyStatus：僅更新營業人狀態的 CurrentLevel。
            var status = models!.GetTable<Organization>()
                .Where(o => o.CompanyID == companyId)
                .Select(o => o.OrganizationStatus)
                .FirstOrDefault();

            if (status == null)
            {
                return CreateNotFoundResponse("Organization.NotFound");
            }

            status.CurrentLevel = (int)Naming.MemberStatusDefinition.Checked;
            models.SubmitChanges();

            return CreateSuccessResponse("Common.Saved");
        }

        /// <summary>
        /// 載入用戶端 G/W 設定（遷移自 OrganizationController.GatewaySettings）。
        /// 目前僅回傳「傳送 Excel 發票開立方式」（InvoiceClientDefaultProcessType）。
        /// 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法。
        /// </summary>
        /// <param name="keyId">加密後的 CompanyID（來自列表 keyId 欄位）</param>
        [HttpGet("GatewaySettings")]
        [ProducesResponseType(typeof(ResponseDto<GatewaySettingsDto>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult GatewaySettings([FromQuery] string keyId)
        {
            if (string.IsNullOrWhiteSpace(keyId))
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            int companyId;
            try
            {
                companyId = keyId.DecryptKeyValue();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Invalid organization keyId");
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            var settings = models!.GetTable<Organization>()
                .Where(o => o.CompanyID == companyId)
                .Select(o => new
                {
                    o.OrganizationStatus,
                    CertificateKeyID = o.OrganizationToken != null ? o.OrganizationToken.KeyID : null,
                })
                .FirstOrDefault();

            if (settings?.OrganizationStatus == null)
            {
                return CreateNotFoundResponse("Organization.NotFound");
            }

            var dto = new GatewaySettingsDto
            {
                KeyId = keyId,
                DefaultProcessType = settings.OrganizationStatus.InvoiceClientDefaultProcessType,
                CertificateKeyId = settings.CertificateKeyID?.ToString(),
            };
            return CreateSuccessResponse(dto, "Common.Retrieved");
        }

        /// <summary>
        /// 儲存用戶端 G/W 之「傳送 Excel 發票開立方式」
        /// （遷移自 OrganizationController.CommitDefaultProcessType）。
        /// 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法。
        /// </summary>
        /// <param name="keyId">加密後的 CompanyID（來自列表 keyId 欄位）</param>
        /// <param name="defaultProcessType">Naming.InvoiceProcessType 之 Xlsx 系列值</param>
        [HttpPost("CommitDefaultProcessType")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult CommitDefaultProcessType([FromQuery] string keyId, [FromQuery] int defaultProcessType)
        {
            if (string.IsNullOrWhiteSpace(keyId))
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            int companyId;
            try
            {
                companyId = keyId.DecryptKeyValue();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Invalid organization keyId");
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            var status = models!.GetTable<Organization>()
                .Where(o => o.CompanyID == companyId)
                .Select(o => o.OrganizationStatus)
                .FirstOrDefault();

            if (status == null)
            {
                return CreateNotFoundResponse("Organization.NotFound");
            }

            // 沿用舊版 CommitDefaultProcessType：僅更新 InvoiceClientDefaultProcessType。
            status.InvoiceClientDefaultProcessType = defaultProcessType;
            models.SubmitChanges();

            return CreateSuccessResponse("Common.Saved");
        }

        /// <summary>
        /// 上載並更新營業人 PKCS12(PFX) 憑證（遷移自 WebHome CertificateIdentityController.CommitItemAsync）。
        /// 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法；以 multipart/form-data 上傳憑證檔與 PIN Code。
        /// </summary>
        /// <param name="keyId">加密後的 CompanyID（來自列表 keyId 欄位）</param>
        /// <param name="pin">PFX 憑證的 PIN Code（保護密碼）</param>
        /// <param name="pfxFile">PKCS12(PFX) 憑證檔</param>
        [HttpPost("CommitCertificate")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public async Task<IActionResult> CommitCertificate(
            [FromForm] string keyId,
            [FromForm] string? pin,
            IFormFile? pfxFile)
        {
            if (string.IsNullOrWhiteSpace(keyId))
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            if (pfxFile == null || pfxFile.Length == 0)
            {
                return CreateBadRequestResponse("未選取檔案或檔案上傳失敗");
            }

            int companyId;
            try
            {
                companyId = keyId.DecryptKeyValue();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Invalid organization keyId");
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            var item = models!.GetTable<Organization>()
                .Where(o => o.CompanyID == companyId)
                .FirstOrDefault();

            if (item == null)
            {
                return CreateNotFoundResponse("Organization.NotFound");
            }

            try
            {
                // 沿用舊版：讀入 PFX bytes、以 PIN 開啟憑證（可匯出），再更新 OrganizationToken。
                using var input = pfxFile.OpenReadStream();
                using var ms = new MemoryStream();
                await input.CopyToAsync(ms);
                var buf = ms.ToArray();

                var cert = new X509Certificate2(buf, pin, X509KeyStorageFlags.Exportable);

                if (item.OrganizationToken == null)
                {
                    item.OrganizationToken = new OrganizationToken { CompanyID = item.CompanyID };
                }

                var newKeyID = Guid.NewGuid();
                item.OrganizationToken.X509Certificate = Convert.ToBase64String(cert.RawData);
                item.OrganizationToken.Thumbprint = cert.Thumbprint;
                item.OrganizationToken.KeyID = newKeyID;
                item.OrganizationToken.PKCS12 = Convert.ToBase64String(
                    cert.Export(X509ContentType.Pkcs12, newKeyID.ToString().Substring(0, 8)));

                models.SubmitChanges();
                return CreateSuccessResponse(
                    item.OrganizationToken.KeyID.ToString(),
                    $"更新憑證金鑰:{item.OrganizationToken.KeyID}");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error committing organization certificate");
                return CreateErrorResponse(500, ex.Message);
            }
        }

        /// <summary>
        /// 載入店家 POS 機清單（遷移自 WebHome InvoiceBusinessController.ApplyPOSDevice）。
        /// 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法。
        /// </summary>
        /// <param name="keyId">加密後的 CompanyID（來自列表 keyId 欄位）</param>
        [HttpGet("POSDevices")]
        [ProducesResponseType(typeof(ResponseDto<List<POSDeviceDto>>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult POSDevices([FromQuery] string keyId)
        {
            if (!TryResolveCompany(keyId, out var companyId))
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            if (!models!.GetTable<Organization>().Any(o => o.CompanyID == companyId))
            {
                return CreateNotFoundResponse("Organization.NotFound");
            }

            var devices = models.GetTable<POSDevice>()
                .Where(p => p.CompanyID == companyId)
                .OrderBy(p => p.DeviceID)
                .Select(p => new POSDeviceDto { DeviceId = p.DeviceID, PosNo = p.POSNo })
                .ToList();

            return CreateSuccessResponse(devices, "Common.Retrieved");
        }

        /// <summary>
        /// 新增或編輯店家 POS 機編號（遷移自 WebHome InvoiceBusinessController.CommitPOS）。
        /// 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法；DeviceId 為 null 時視為新增。
        /// </summary>
        [HttpPost("CommitPOS")]
        [ProducesResponseType(typeof(ResponseDto<POSDeviceDto>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult CommitPOS([FromBody] CommitPOSDeviceDto dto)
        {
            if (dto == null || !TryResolveCompany(dto.KeyId, out var companyId))
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            if (!models!.GetTable<Organization>().Any(o => o.CompanyID == companyId))
            {
                return CreateNotFoundResponse("Organization.NotFound");
            }

            var posNo = dto.PosNo.GetEfficientString();
            if (posNo == null)
            {
                return CreateBadRequestResponse("POS機編號錯誤!!");
            }

            var deviceId = dto.DeviceId;

            // 沿用舊版：同一營業人下不得有重複的 POS 機編號（編輯時排除自身）。
            var duplicated = models.GetTable<POSDevice>()
                .Any(p => p.CompanyID == companyId && p.POSNo == posNo && p.DeviceID != deviceId);
            if (duplicated)
            {
                return CreateBadRequestResponse("已存在相同的POS機編號!!");
            }

            var item = deviceId.HasValue
                ? models.GetTable<POSDevice>()
                    .FirstOrDefault(p => p.CompanyID == companyId && p.DeviceID == deviceId.Value)
                : null;

            if (item == null)
            {
                item = new POSDevice { CompanyID = companyId };
                models.GetTable<POSDevice>().Add(item);
            }
            item.POSNo = posNo;

            models.SubmitChanges();

            var result = new POSDeviceDto { DeviceId = item.DeviceID, PosNo = item.POSNo };
            return CreateSuccessResponse(result, "Common.Saved");
        }

        /// <summary>
        /// 刪除店家 POS 機（遷移自 WebHome InvoiceBusinessController.DeletePOS）。
        /// 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法。
        /// </summary>
        /// <param name="keyId">加密後的 CompanyID（來自列表 keyId 欄位）</param>
        /// <param name="deviceId">要刪除的 POS 機序號</param>
        [HttpPost("DeletePOS")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public IActionResult DeletePOS([FromQuery] string keyId, [FromQuery] int deviceId)
        {
            if (!TryResolveCompany(keyId, out var companyId))
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            var item = models!.DeleteAny<POSDevice>(d => d.CompanyID == companyId && d.DeviceID == deviceId);
            if (item == null)
            {
                return CreateBadRequestResponse("POS機編號錯誤!!");
            }

            return CreateSuccessResponse("Common.Saved");
        }

        /// <summary>
        /// 載入發票經銷商候選清單（遷移自 OrganizationController.ApplyIssuerAgent）。
        /// 回傳所有「發票開立代理」類別的營業人，並標示是否已指派給指定開立人。
        /// 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法。
        /// </summary>
        /// <param name="keyId">加密後的開立人 CompanyID（來自列表 keyId 欄位）</param>
        [HttpGet("IssuerAgents")]
        [ProducesResponseType(typeof(ResponseDto<List<IssuerAgentDto>>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult IssuerAgents([FromQuery] string keyId)
        {
            if (!TryResolveCompany(keyId, out var issuerId))
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            if (!models!.GetTable<Organization>().Any(o => o.CompanyID == issuerId))
            {
                return CreateNotFoundResponse("Organization.NotFound");
            }

            // 已指派給此開立人的經銷商 CompanyID 集合。
            var assignedAgentIds = models.GetTable<InvoiceIssuerAgent>()
                .Where(a => a.IssuerID == issuerId)
                .Select(a => a.AgentID)
                .ToHashSet();

            // 所有「發票開立代理」類別的候選經銷商（沿用舊版 ApplyIssuerAgent.cshtml 之查詢）。
            var candidates = models.GetTable<Organization>()
                .Where(o => o.OrganizationCategory.Any(c => c.CategoryID == (int)Naming.CategoryID.COMP_INVOICE_AGENT))
                .OrderBy(o => o.ReceiptNo)
                .Select(o => new { o.CompanyID, o.ReceiptNo, o.CompanyName })
                .ToList();

            // EncryptKey 無法於 EF 查詢中翻譯，故 materialize 後再投影為 DTO。
            var result = candidates
                .Select(o => new IssuerAgentDto
                {
                    KeyId = o.CompanyID.EncryptKey(),
                    ReceiptNo = o.ReceiptNo,
                    CompanyName = o.CompanyName,
                    Selected = assignedAgentIds.Contains(o.CompanyID),
                })
                .ToList();

            return CreateSuccessResponse(result, "Common.Retrieved");
        }

        /// <summary>
        /// 設定發票經銷商（遷移自 OrganizationController.CommitIssuerAgent）。
        /// 沿用舊版循環經銷檢查；開立人與各經銷商均以加密 KeyID 傳遞 CompanyID。
        /// </summary>
        [HttpPost("CommitIssuerAgent")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult CommitIssuerAgent([FromBody] CommitIssuerAgentDto dto)
        {
            if (dto == null || !TryResolveCompany(dto.KeyId, out var issuerId))
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            if (!models!.GetTable<Organization>().Any(o => o.CompanyID == issuerId))
            {
                return CreateNotFoundResponse("Organization.NotFound");
            }

            // 解密每個經銷商 KeyID → CompanyID（沿用舊版以加密 KeyID 傳遞的做法）。
            var agentIds = new List<int>();
            if (dto.AgentKeyIds != null)
            {
                foreach (var agentKeyId in dto.AgentKeyIds)
                {
                    if (!TryResolveCompany(agentKeyId, out var agentId))
                    {
                        return CreateBadRequestResponse("Common.InvalidParameter");
                    }
                    agentIds.Add(agentId);
                }
            }
            agentIds = agentIds.Distinct().ToList();

            // 循環經銷檢查（沿用舊版 CheckAgentCycle）。
            foreach (var agentId in agentIds)
            {
                if (CheckAgentCycle(issuerId, agentId, out var cycleAgent))
                {
                    var offender = models.GetTable<Organization>()
                        .Where(o => o.CompanyID == cycleAgent!.IssuerID)
                        .Select(o => new { o.ReceiptNo, o.CompanyName })
                        .FirstOrDefault();
                    return CreateBadRequestResponse($"發生循環經銷({offender?.ReceiptNo}, {offender?.CompanyName})!!");
                }
            }

            // 沿用舊版 INSERT WHERE NOT EXISTS 語意：只刪除取消勾選的、只新增尚未存在的，
            // 避免同一 SaveChanges 內對相同複合鍵 (AgentID, IssuerID) 既刪又增造成追蹤衝突。
            var existing = models.GetTable<InvoiceIssuerAgent>()
                .Where(a => a.IssuerID == issuerId)
                .ToList();

            var toRemove = existing.Where(a => !agentIds.Contains(a.AgentID)).ToList();
            if (toRemove.Count > 0)
            {
                models.GetTable<InvoiceIssuerAgent>().RemoveRange(toRemove);
            }

            var existingAgentIds = existing.Select(a => a.AgentID).ToHashSet();
            foreach (var agentId in agentIds.Where(id => !existingAgentIds.Contains(id)))
            {
                models.GetTable<InvoiceIssuerAgent>().Add(new InvoiceIssuerAgent
                {
                    AgentID = agentId,
                    IssuerID = issuerId,
                });
            }

            models.SubmitChanges();

            return CreateSuccessResponse("Common.Saved");
        }

        /// <summary>
        /// 循環經銷檢查（沿用舊版 OrganizationController.CheckAgentCycle）。
        /// 沿著「經銷商的經銷商」關係遞迴，若開立人本身出現在鏈上即形成循環。
        /// </summary>
        private bool CheckAgentCycle(int issuerId, int agentId, out InvoiceIssuerAgent? cycleAgent)
        {
            cycleAgent = null;
            var agentItems = models!.GetTable<InvoiceIssuerAgent>()
                .Where(a => a.IssuerID == agentId)
                .ToList();

            foreach (var agent in agentItems)
            {
                if (agent.AgentID == agent.IssuerID)
                {
                    continue;
                }

                if (agent.AgentID == issuerId)
                {
                    cycleAgent = agent;
                    return true;
                }

                if (CheckAgentCycle(issuerId, agent.AgentID, out cycleAgent))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 設定為 B2B 營業人（遷移自 WebHome HandlingController.ApplyRelationship）。
        /// 將營業人加入企業群組（網際優勢股份有限公司）成為 B2B 營業人。
        /// 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法。
        /// </summary>
        /// <param name="keyId">加密後的 CompanyID（來自列表 keyId 欄位）</param>
        [HttpPost("ApplyRelationship")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult ApplyRelationship([FromQuery] string keyId)
        {
            if (!TryResolveCompany(keyId, out var companyId))
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            if (!models!.GetTable<Organization>().Any(o => o.CompanyID == companyId))
            {
                return CreateNotFoundResponse("資料不存在!!");
            }

            // 沿用舊版 IsEnterpriseGroupMember 判斷（org.EnterpriseGroupMember.Any()）：
            // 直接查詢會員表避免依賴 EF 延遲載入，已是企業群組成員即不重複加入。
            if (models.GetTable<EnterpriseGroupMember>().Any(m => m.CompanyID == companyId))
            {
                return CreateSuccessResponse("該開立人已是B2B營業人!!");
            }

            models.GetTable<EnterpriseGroupMember>().Add(new EnterpriseGroupMember
            {
                EnterpriseID = (int)Naming.EnterpriseGroup.網際優勢股份有限公司,
                CompanyID = companyId,
            });
            models.SubmitChanges();

            return CreateSuccessResponse("設定完成!!");
        }

        /// <summary>
        /// 切換主機構設定（遷移自 WebHome HandlingController.CommitMasterOrganization）。
        /// 沿用舊版切換邏輯：尚未設定則建立 MasterOrganization，已設定則移除。
        /// 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法；回傳切換後是否為主機構。
        /// </summary>
        /// <param name="keyId">加密後的 CompanyID（來自列表 keyId 欄位）</param>
        [HttpPost("CommitMaster")]
        [ProducesResponseType(typeof(ResponseDto<bool>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        [ProducesResponseType(typeof(BaseResponseDto), 500)]
        public IActionResult CommitMaster([FromQuery] string keyId)
        {
            if (!TryResolveCompany(keyId, out var companyId))
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            var org = models!.GetTable<Organization>()
                .Where(o => o.CompanyID == companyId)
                .Select(o => new { o.CompanyID, o.CompanyName })
                .FirstOrDefault();

            if (org == null)
            {
                return CreateNotFoundResponse("資料不存在!!");
            }

            // 沿用舊版切換邏輯：直接查詢 MasterOrganization 避免依賴 EF 延遲載入。
            var existing = models.GetTable<MasterOrganization>()
                .FirstOrDefault(m => m.MasterID == companyId);

            bool isMaster;
            try
            {
                if (existing == null)
                {
                    // 沿用舊版：以營業人名稱作為機關名稱建立主機構。
                    models.GetTable<MasterOrganization>().Add(new MasterOrganization
                    {
                        MasterID = companyId,
                        EnterpriseName = org.CompanyName,
                    });
                    isMaster = true;
                }
                else
                {
                    models.GetTable<MasterOrganization>().Remove(existing);
                    isMaster = false;
                }
                models.SubmitChanges();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error toggling master organization");
                return CreateErrorResponse(500, ex.Message);
            }

            return CreateSuccessResponse(isMaster, isMaster ? "已設定為主機構!!" : "已取消主機構設定!!");
        }

        /// <summary>
        /// 設為分支機構（遷移自 WebHome OrganizationController.ApplyHeadquarter → CommitIssuerMaster）。
        /// 將勾選的營業人設定為指定主機構的分支機構：於 InvoiceIssuerAgent 建立
        /// AgentID = 主機構、IssuerID = 分支、RelationType = MasterBranch 的關係。
        /// 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法，並沿用循環經銷檢查。
        /// </summary>
        [HttpPost("ApplyHeadquarter")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult ApplyHeadquarter([FromBody] ApplyHeadquarterDto dto)
        {
            if (dto == null || !TryResolveCompany(dto.HeadquarterKeyId, out var headquarterId))
            {
                return CreateBadRequestResponse("總機構營業人資料錯誤!!");
            }

            if (!models!.GetTable<Organization>().Any(o => o.CompanyID == headquarterId))
            {
                return CreateNotFoundResponse("總機構營業人資料錯誤!!");
            }

            // 沿用舊版：未勾選分支機構時回報錯誤。
            if (dto.BranchKeyIds == null || dto.BranchKeyIds.Count == 0)
            {
                return CreateBadRequestResponse("請勾選分支機構營業人!!");
            }

            // 解密每個分支機構 KeyID → CompanyID（沿用舊版以加密 KeyID 傳遞的做法）。
            var branchIds = new List<int>();
            foreach (var branchKeyId in dto.BranchKeyIds)
            {
                if (!TryResolveCompany(branchKeyId, out var branchId))
                {
                    return CreateBadRequestResponse("Common.InvalidParameter");
                }
                branchIds.Add(branchId);
            }
            branchIds = branchIds.Distinct().ToList();

            // 循環經銷檢查（沿用舊版 CommitIssuerMaster：分支為 Issuer 位置、主機構為 Agent 位置）。
            foreach (var branchId in branchIds)
            {
                if (CheckAgentCycle(branchId, headquarterId, out var cycleAgent))
                {
                    var offender = models.GetTable<Organization>()
                        .Where(o => o.CompanyID == cycleAgent!.IssuerID)
                        .Select(o => new { o.ReceiptNo, o.CompanyName })
                        .FirstOrDefault();
                    return CreateBadRequestResponse($"發生循環經銷({offender?.ReceiptNo}, {offender?.CompanyName})!!");
                }
            }

            var masterBranch = (int)InvoiceIssuerAgent.RelationTypeEnum.MasterBranch;

            // 沿用舊版 CommitIssuerMaster：主機構本身不應同時為他人的分支，故先移除以此主機構為分支
            // （IssuerID = headquarter）的既有主機構-分支關係。
            // 註：舊版此處未限定 RelationType，會連帶誤刪此營業人作為開立人的經銷商關係（IssuerID 同時用於
            // 經銷關係）；此處限定 RelationType = MasterBranch，保留原意同時避免經銷關係遭誤刪。
            var selfAsBranch = models.GetTable<InvoiceIssuerAgent>()
                .Where(a => a.IssuerID == headquarterId && a.RelationType == masterBranch)
                .ToList();
            if (selfAsBranch.Count > 0)
            {
                models.GetTable<InvoiceIssuerAgent>().RemoveRange(selfAsBranch);
            }

            // 沿用舊版 INSERT WHERE NOT EXISTS 語意：僅新增尚未存在的關係，並設定 RelationType = MasterBranch。
            foreach (var branchId in branchIds)
            {
                var relation = models.GetTable<InvoiceIssuerAgent>()
                    .FirstOrDefault(a => a.AgentID == headquarterId && a.IssuerID == branchId);
                if (relation == null)
                {
                    relation = new InvoiceIssuerAgent { AgentID = headquarterId, IssuerID = branchId };
                    models.GetTable<InvoiceIssuerAgent>().Add(relation);
                }
                relation.RelationType = masterBranch;
            }

            models.SubmitChanges();

            return CreateSuccessResponse("設定完成!!");
        }

        /// <summary>
        /// 複製收費標準（遷移自 WebHome OrganizationController.CloneBillingPlan）。
        /// 將來源營業人的收費設定（BillingGrade / BillingIncrement / BillingCalculation / ExtraBillingItem）
        /// 複製到勾選的目標營業人；每個目標營業人原有的收費設定會先被刪除再從來源複製（取代）。
        /// 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法。
        /// </summary>
        [HttpPost("CloneBillingPlan")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult CloneBillingPlan([FromBody] CloneBillingPlanDto dto)
        {
            if (dto == null || !TryResolveCompany(dto.SourceKeyId, out var sourceId))
            {
                return CreateBadRequestResponse("複製來源營業人資料錯誤!!");
            }

            if (!models!.GetTable<Organization>().Any(o => o.CompanyID == sourceId))
            {
                return CreateNotFoundResponse("複製來源營業人資料錯誤!!");
            }

            // 沿用舊版：未勾選複製目標時回報錯誤。
            if (dto.TargetKeyIds == null || dto.TargetKeyIds.Count == 0)
            {
                return CreateBadRequestResponse("請勾選複製目標營業人!!");
            }

            // 解密每個目標營業人 KeyID → CompanyID（沿用舊版以加密 KeyID 傳遞的做法）。
            var targetIds = new List<int>();
            foreach (var targetKeyId in dto.TargetKeyIds)
            {
                if (!TryResolveCompany(targetKeyId, out var targetId))
                {
                    return CreateBadRequestResponse("Common.InvalidParameter");
                }
                targetIds.Add(targetId);
            }
            // 排除來源本身並去除重複，避免自我複製造成的無謂刪除/寫入。
            targetIds = targetIds.Distinct().Where(id => id != sourceId).ToList();

            // 沿用舊版 CloneBillingPlan：逐一目標營業人「先刪除原設定，再從來源複製」四張收費設定表。
            foreach (var targetId in targetIds)
            {
                models.ExecuteCommand("DELETE FROM billing.BillingGrade WHERE (CompanyID = {0})", targetId);
                models.ExecuteCommand(@"INSERT INTO billing.BillingGrade
                       (CompanyID, GradeCount, BasicFee)
                        SELECT  {0}, GradeCount, BasicFee
                        FROM     billing.BillingGrade
                        WHERE   (CompanyID = {1})", targetId, sourceId);
                models.ExecuteCommand("DELETE FROM billing.BillingIncrement WHERE (CompanyID = {0})", targetId);
                models.ExecuteCommand(@"INSERT INTO billing.BillingIncrement
                       (CompanyID, UpperBound, UnitFee)
                        SELECT  {0}, UpperBound, UnitFee
                        FROM     billing.BillingIncrement AS BillingIncrement_1
                        WHERE   (CompanyID = {1})", targetId, sourceId);
                models.ExecuteCommand("DELETE FROM billing.BillingCalculation WHERE (CompanyID = {0})", targetId);
                models.ExecuteCommand(@"INSERT INTO billing.BillingCalculation
                       (CompanyID, TypeID)
                        SELECT  {0}, TypeID
                        FROM     billing.BillingCalculation
                        WHERE   (CompanyID = {1})", targetId, sourceId);
                models.ExecuteCommand("DELETE FROM billing.ExtraBillingItem WHERE (CompanyID = {0})", targetId);
                models.ExecuteCommand(@"INSERT INTO billing.ExtraBillingItem
                       (CompanyID, ItemName, Fee, BillingDate, BillingType)
                        SELECT  {0}, ItemName, Fee, BillingDate, BillingType
                        FROM     billing.ExtraBillingItem
                        WHERE   (CompanyID = {1})", targetId, sourceId);
            }

            return CreateSuccessResponse("複製完成!!");
        }

        /// <summary>
        /// 載入客製化服務設定 — SMTP 郵件伺服器（遷移自 OrganizationController.CustomSettings / LoadSmtpSettings）。
        /// 回傳最新一筆設定；已停用（Disabled）視為未設定。密碼不回傳。
        /// 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法。
        /// </summary>
        /// <param name="keyId">加密後的 CompanyID（來自列表 keyId 欄位）</param>
        [HttpGet("CustomSmtpSettings")]
        [ProducesResponseType(typeof(ResponseDto<CustomSmtpSettingsDto>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult CustomSmtpSettings([FromQuery] string keyId)
        {
            if (!TryResolveCompany(keyId, out var companyId))
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            if (!models!.GetTable<Organization>().Any(o => o.CompanyID == companyId))
            {
                return CreateNotFoundResponse("營業人資料錯誤!!");
            }

            // 沿用舊版：取 HostID 最新一筆；Status = Disabled 視為未設定。
            var settings = models.GetTable<CustomSmtpHost>()
                .Where(s => s.CompanyID == companyId)
                .OrderByDescending(s => s.HostID)
                .Select(s => new
                {
                    s.Host,
                    s.Port,
                    s.EnableSsl,
                    s.UserName,
                    s.MailFrom,
                    s.Status,
                    HasPassword = s.Password != null,
                })
                .FirstOrDefault();

            var dto = new CustomSmtpSettingsDto { KeyId = keyId };
            if (settings != null && settings.Status != (int)CustomSmtpHost.StatusType.Disabled)
            {
                dto.Configured = true;
                dto.Host = settings.Host;
                dto.Port = settings.Port;
                dto.EnableSsl = settings.EnableSsl;
                dto.UserName = settings.UserName;
                dto.MailFrom = settings.MailFrom;
                dto.HasPassword = settings.HasPassword;
            }

            return CreateSuccessResponse(dto, "Common.Retrieved");
        }

        /// <summary>
        /// 儲存客製化 SMTP 設定（遷移自 OrganizationController.CommitSmtpSettings → CommitCustomSmtpHost）。
        /// 沿用舊版：取最新一筆重用（不存在則新增），Port 預設 25、Status 設為啟用；
        /// Password 留空表示不變更原密碼。以加密 KeyID 傳遞 CompanyID。
        /// </summary>
        [HttpPost("CommitCustomSmtp")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult CommitCustomSmtp([FromBody] CommitCustomSmtpDto dto)
        {
            if (dto == null || !TryResolveCompany(dto.KeyId, out var companyId))
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            if (!models!.GetTable<Organization>().Any(o => o.CompanyID == companyId))
            {
                return CreateNotFoundResponse("營業人資料錯誤!!");
            }

            // 沿用舊版 CustomSmtpHostValueCheck：Host 與 MailFrom 必填。
            var errors = new List<string>();
            var host = dto.Host.GetEfficientString();
            if (host == null)
            {
                errors.Add("請輸入郵件伺服器!!");
            }
            var mailFrom = dto.MailFrom.GetEfficientString();
            if (mailFrom == null)
            {
                errors.Add("請輸入寄件人email!!");
            }
            if (errors.Count > 0)
            {
                return CreateBadRequestResponse("Common.SaveError", errors);
            }

            // 沿用舊版：取最新一筆重用（含已停用者），不存在則新增。
            var item = models.GetTable<CustomSmtpHost>()
                .Where(s => s.CompanyID == companyId)
                .OrderByDescending(s => s.HostID)
                .FirstOrDefault();

            if (item == null)
            {
                item = new CustomSmtpHost { CompanyID = companyId };
                models.GetTable<CustomSmtpHost>().Add(item);
            }

            item.Host = host!;
            item.Port = dto.Port ?? 25;
            item.EnableSsl = dto.EnableSsl ?? false;
            item.UserName = dto.UserName;
            // Password 留空表示不變更原密碼（避免每次儲存都須重新輸入而誤清）。
            var password = dto.Password.GetEfficientString();
            if (password != null)
            {
                item.Password = password;
            }
            item.MailFrom = mailFrom!;
            item.Status = (int)CustomSmtpHost.StatusType.Enabled;

            models.SubmitChanges();

            return CreateSuccessResponse("Common.Saved");
        }

        /// <summary>
        /// 停用客製化 SMTP 設定（遷移自 OrganizationController.DisableSmtpSettings）。
        /// 沿用舊版：將該營業人所有 CustomSmtpHost 的 Status 設為 Disabled。
        /// 以加密 KeyID 傳遞 CompanyID。
        /// </summary>
        /// <param name="keyId">加密後的 CompanyID（來自列表 keyId 欄位）</param>
        [HttpPost("DisableCustomSmtp")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public IActionResult DisableCustomSmtp([FromQuery] string keyId)
        {
            if (!TryResolveCompany(keyId, out var companyId))
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            models!.ExecuteCommand(
                "UPDATE CustomSmtpHost SET Status = {0} WHERE CompanyID = {1}",
                (int)CustomSmtpHost.StatusType.Disabled, companyId);

            return CreateSuccessResponse("Common.Saved");
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

        /// <summary>
        /// 將載入後的 OrganizationViewModel 投影為前端編輯 DTO。
        /// </summary>
        private static OrganizationEditDto MapToEditDto(OrganizationViewModel vm, string keyId)
        {
            return new OrganizationEditDto
            {
                KeyId = keyId,
                CompanyId = vm.CompanyID,
                ReceiptNo = vm.ReceiptNo,
                CompanyName = vm.CompanyName,
                Addr = vm.Addr,
                Phone = vm.Phone,
                Fax = vm.Fax,
                UndertakerName = vm.UndertakerName,
                CategoryId = (int?)vm.CategoryID,
                CustomerNo = vm.CustomerNo,
                ContactName = vm.ContactName,
                ContactTitle = vm.ContactTitle,
                ContactPhone = vm.ContactPhone,
                ContactMobilePhone = vm.ContactMobilePhone,
                ContactEmail = vm.ContactEmail,
                CreationDate = vm.CreationDate,
                ExpirationDate = vm.ExpirationDate,
                AuthorizationNotBefore = vm.AuthorizationNotBefore,
                AuthorizationNotAfter = vm.AuthorizationNotAfter,
                InvoiceRequestNotBefore = vm.InvoiceRequestNotBefore,
                InvoiceRequestNotAfter = vm.InvoiceRequestNotAfter,
                TaxNo = vm.TaxNo,
                SetToPrintInvoice = vm.SetToPrintInvoice,
                InvoicePrintView = vm.InvoicePrintView,
                C0401POSView = vm.C0401POSView,
                AllowancePrintView = vm.AllowancePrintView,
                AuthorizationNo = vm.AuthorizationNo,
                EntrustToPrint = vm.EntrustToPrint,
                DownloadDataNumber = vm.DownloadDataNumber,
                UploadBranchTrackBlank = vm.UploadBranchTrackBlank,
                AutoBlankTrack = vm.AutoBlankTrack,
                AutoBlankTrackEmittance = vm.AutoBlankTrackEmittance,
                PrintAll = vm.PrintAll,
                SettingInvoiceType = (int?)vm.SettingInvoiceType,
                SubscribeB2BInvoicePDF = vm.SubscribeB2BInvoicePDF,
                EnableTrackCodeInvoiceNoValidation = vm.EnableTrackCodeInvoiceNoValidation,
                SetToOutsourcingCS = vm.SetToOutsourcingCS,
                DownloadDispatch = vm.DownloadDispatch,
                SetToNotifyCounterpartBySMS = vm.SetToNotifyCounterpartBySMS,
                UseB2BStandalone = vm.UseB2BStandalone,
                Settings = vm.Settings,
                NoticeStatus = DecomposeNoticeSetting(vm.NoticeSetting),
                BusinessContactPhone = vm.BusinessContactPhone,
                CustomNotificationView = vm.CustomNotificationView,
                CustomNotification = vm.CustomNotification,
                LogoUrl = vm.LogoURL,
            };
        }

        /// <summary>
        /// 將前端編輯 DTO 還原為 OrganizationViewModel，供 CommitOrganizationViewModel 使用。
        /// </summary>
        private static OrganizationViewModel MapToViewModel(OrganizationEditDto dto)
        {
            return new OrganizationViewModel
            {
                KeyID = dto.KeyId,
                CompanyID = dto.CompanyId,
                ReceiptNo = dto.ReceiptNo,
                CompanyName = dto.CompanyName,
                Addr = dto.Addr,
                Phone = dto.Phone,
                Fax = dto.Fax,
                UndertakerName = dto.UndertakerName,
                CategoryID = (CategoryDefinition.CategoryEnum?)dto.CategoryId,
                CustomerNo = dto.CustomerNo,
                ContactName = dto.ContactName,
                ContactTitle = dto.ContactTitle,
                ContactPhone = dto.ContactPhone,
                ContactMobilePhone = dto.ContactMobilePhone,
                ContactEmail = dto.ContactEmail,
                CreationDate = dto.CreationDate,
                ExpirationDate = dto.ExpirationDate,
                AuthorizationNotBefore = dto.AuthorizationNotBefore,
                AuthorizationNotAfter = dto.AuthorizationNotAfter,
                InvoiceRequestNotBefore = dto.InvoiceRequestNotBefore,
                InvoiceRequestNotAfter = dto.InvoiceRequestNotAfter,
                TaxNo = dto.TaxNo,
                SetToPrintInvoice = dto.SetToPrintInvoice,
                InvoicePrintView = dto.InvoicePrintView,
                C0401POSView = dto.C0401POSView,
                AllowancePrintView = dto.AllowancePrintView,
                AuthorizationNo = dto.AuthorizationNo,
                EntrustToPrint = dto.EntrustToPrint,
                DownloadDataNumber = dto.DownloadDataNumber,
                UploadBranchTrackBlank = dto.UploadBranchTrackBlank,
                AutoBlankTrack = dto.AutoBlankTrack,
                AutoBlankTrackEmittance = dto.AutoBlankTrackEmittance,
                PrintAll = dto.PrintAll,
                SettingInvoiceType = (Naming.InvoiceTypeDefinition?)dto.SettingInvoiceType,
                SubscribeB2BInvoicePDF = dto.SubscribeB2BInvoicePDF,
                EnableTrackCodeInvoiceNoValidation = dto.EnableTrackCodeInvoiceNoValidation,
                SetToOutsourcingCS = dto.SetToOutsourcingCS,
                DownloadDispatch = dto.DownloadDispatch,
                SetToNotifyCounterpartBySMS = dto.SetToNotifyCounterpartBySMS,
                UseB2BStandalone = dto.UseB2BStandalone,
                Settings = dto.Settings,
                NoticeStatus = dto.NoticeStatus,
                BusinessContactPhone = dto.BusinessContactPhone,
                CustomNotificationView = dto.CustomNotificationView,
                CustomNotification = dto.CustomNotification,
            };
        }

        /// <summary>
        /// 將 InvoiceNoticeSetting 位元遮罩拆解為已啟用的個別旗標位元值清單，
        /// 供前端勾選對應的通知 checkbox。
        /// </summary>
        private static int[] DecomposeNoticeSetting(Naming.InvoiceNoticeStatus? noticeSetting)
        {
            if (!noticeSetting.HasValue)
            {
                return Array.Empty<int>();
            }

            var mask = (int)noticeSetting.Value;
            var result = new List<int>();
            foreach (Naming.InvoiceNoticeStatus flag in Enum.GetValues(typeof(Naming.InvoiceNoticeStatus)))
            {
                if ((mask & (int)flag) != 0)
                {
                    result.Add((int)flag);
                }
            }
            return result.ToArray();
        }
    }
}
