using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommonLib.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelCore.DataEntity;
using ModelCore.DTOs;
using ModelCore.Helper;
using ModelCore.Locale;
using ModelCore.Notification;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Interfaces;
using TaskCenter.Core.Services;

namespace TaskCenter.Core.Controllers
{
    /// <summary>
    /// 使用者帳號管理 API（遷移自 WebHome AccountController 之 AccountIndex / Inquire 與列管理動作）。
    /// 由營業人資料管理頁「管理使用者」進入，以加密 OrgKeyId 限定營業人；
    /// 各列動作以加密的使用者 KeyID（UID）傳遞，沿用舊版以加密 KeyID 傳遞識別碼的做法。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class UserAccountController : ApiBaseController
    {
        private readonly IUserAccountService _userAccountService;

        public UserAccountController(
            IUserAccountService userAccountService,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
            _userAccountService = userAccountService;
        }

        /// <summary>
        /// 查詢指定營業人的使用者帳號（分頁）。
        /// 對應舊版 AccountController.AccountIndex + Inquire（以 SellerID 限定範圍）。
        /// </summary>
        /// <param name="queryDto">查詢條件（含加密 OrgKeyId）</param>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseDto<PagedResultDto<UserAccountDatatableDto>>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 500)]
        public async Task<IActionResult> GetList([FromQuery] UserAccountQueryDto queryDto)
        {
            if (queryDto == null || !TryResolveKey(queryDto.OrgKeyId, out var companyId))
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }
            queryDto.CompanyId = companyId;
            if (!CanAccessSeller(companyId)) return CreateErrorResponse(403, "無權存取此營業人資料!!");

            try
            {
                var result = await _userAccountService.GetPagedAsync(queryDto);
                return CreateSuccessResponse(result, "Common.Retrieved");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving user accounts");
                return CreateErrorResponse(500, "Common.RetrieveError");
            }
        }

        /// <summary>
        /// 載入單一使用者帳號的編輯資料（遷移自 UserProfileController.EditItem）。
        /// 沿用舊版以加密 KeyID 傳遞 UID；密碼欄位不回傳。
        /// </summary>
        /// <param name="keyId">加密後的 UID（來自列表 keyId 欄位）</param>
        [HttpGet("EditItem")]
        [ProducesResponseType(typeof(ResponseDto<UserAccountEditDto>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult EditItem([FromQuery] string keyId)
        {
            if (!TryResolveKey(keyId, out var uid))
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            if (!CanAccessUser(uid)) return CreateErrorResponse(403, "帳號非所屬會員使用者!!");

            var data = models!.GetTable<UserProfile>()
                .Where(u => u.UID == uid)
                .Select(u => new
                {
                    u.UID,
                    u.PID,
                    u.UserName,
                    u.EMail,
                    u.Address,
                    u.Phone,
                    u.MobilePhone,
                    u.Phone2,
                    Role = u.UserRole
                        .Select(r => new { r.RoleID, CompanyName = r.OrgaCate.Company.CompanyName })
                        .FirstOrDefault(),
                })
                .FirstOrDefault();

            if (data == null)
            {
                return CreateNotFoundResponse("帳號資料錯誤!!");
            }

            var dto = new UserAccountEditDto
            {
                KeyId = keyId,
                Pid = data.PID,
                UserName = data.UserName,
                Email = data.EMail,
                Address = data.Address,
                Phone = data.Phone,
                MobilePhone = data.MobilePhone,
                Phone2 = data.Phone2,
                RoleId = data.Role?.RoleID,
                CompanyName = data.Role?.CompanyName,
            };
            return CreateSuccessResponse(dto, "Common.Retrieved");
        }

        /// <summary>
        /// 新增 / 修改使用者帳號（遷移自 UserProfileController.Commit → CommitUserProfileViewModel）。
        /// 沿用舊版驗證與寫入邏輯：KeyId 為 null 時新增，否則修改；所屬營業人以 OrgKeyId 傳遞。
        /// 本站未設定 NotifyToActivate 時，新增帳號不寄送確認信（帳號維持等待確認，可由列選單「啟用」）。
        /// </summary>
        [HttpPost("Commit")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public IActionResult Commit([FromBody] UserAccountEditDto dto)
        {
            if (dto == null || !TryResolveKey(dto.OrgKeyId, out var companyId))
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }
            if (!CanAccessSeller(companyId)) return CreateErrorResponse(403, "無權存取此營業人資料!!");

            // 修改時解密 UID；新增時為 null。
            int? uid = null;
            if (!string.IsNullOrWhiteSpace(dto.KeyId))
            {
                if (!TryResolveKey(dto.KeyId, out var resolvedUid))
                {
                    return CreateBadRequestResponse("Common.InvalidParameter");
                }
                uid = resolvedUid;
            }

            var item = uid.HasValue
                ? models!.GetTable<UserProfile>().FirstOrDefault(u => u.UID == uid.Value)
                : null;

            // 沿用舊版 UserProfileValueCheck / CommitUserRoleViewModel 的驗證。
            var errors = new List<string>();

            var pid = dto.Pid.GetEfficientString();
            if (pid == null)
            {
                errors.Add("帳號不可為空白!!");
            }

            var password = dto.Password.GetEfficientString();
            var password1 = dto.Password1.GetEfficientString();
            if (password != null)
            {
                if (password.Length < 6)
                {
                    errors.Add("密碼不可少於６個字碼!!");
                }
                else if (password != password1)
                {
                    errors.Add("二組密碼輸入不同!!");
                }
                else if (!Regex.IsMatch(password, "^(?=.*\\d)(?=.*[a-zA-Z])"))
                {
                    errors.Add("密碼須由英文、數字組成!!");
                }
            }
            else if (item == null)
            {
                // 新增帳號必填密碼。
                errors.Add("密碼不可為空白!!");
            }

            if (!dto.RoleId.HasValue)
            {
                errors.Add("請選擇身份設定!!");
            }

            // PID 唯一性檢查（新增 / 修改分別排除自身）。
            if (pid != null)
            {
                var pidTaken = item != null
                    ? models!.GetTable<UserProfile>().Any(u => u.UID != item.UID && u.PID == pid)
                    : models!.GetTable<UserProfile>().Any(u => u.PID == pid);
                if (pidTaken)
                {
                    errors.Add("這個帳號已被使用，請更換申請帳號!!");
                }
            }

            // 所屬營業人 → OrgaCateID。
            var orgaCateId = models!.GetTable<OrganizationCategory>()
                .Where(c => c.CompanyID == companyId)
                .Select(c => (int?)c.OrgaCateID)
                .FirstOrDefault();
            if (!orgaCateId.HasValue)
            {
                errors.Add("請選擇所屬會員!!");
            }

            if (errors.Count > 0)
            {
                return CreateBadRequestResponse("Common.SaveError", errors);
            }

            // 寫入 UserProfile（沿用舊版）。
            bool isNew = item == null;
            if (isNew)
            {
                item = new UserProfile
                {
                    UserProfileStatus = new UserProfileStatus
                    {
                        CurrentLevel = (int)Naming.MemberStatusDefinition.Wait_For_Check,
                    },
                };
                models!.GetTable<UserProfile>().Add(item);
            }

            item!.PID = pid!;
            item.UserName = dto.UserName;

            // 沿用舊版 UpdatePassword：留空則不變更原密碼。
            if (password != null)
            {
                item.Password2 = ValidityAgent.MakePassword(password);
            }
            item.Expiration = DateTime.Today.AddDays(
                ModelExtension.Properties.AppSettings.Default.UserPasswordValidDays);

            item.EMail = dto.Email;
            item.MailID = item.EMail.GetEfficientString()?.Split(';', ',', '，')?[0];
            item.Address = dto.Address;
            item.Phone = dto.Phone;
            item.MobilePhone = dto.MobilePhone;
            item.Phone2 = dto.Phone2;

            models.SubmitChanges();

            // 清除該帳號的重設密碼要求（沿用舊版）。
            models.ExecuteCommand("DELETE FROM ResetUserPassword WHERE UID = {0}", item.UID);

            if (isNew)
            {
                // NotifyToActivate 未於本站設定時略過（避免 NRE）。
                PortalNotification.NotifyToActivate?.Invoke(item);
            }

            // 指派身份角色（沿用舊版 CommitUserRoleViewModel：先刪除既有再新增，一使用者一角色綁定）。
            models.ExecuteCommand("DELETE FROM UserRole WHERE UID = {0}", item.UID);
            models.GetTable<UserRole>().Add(new UserRole
            {
                UID = item.UID,
                OrgaCateID = orgaCateId!.Value,
                RoleID = dto.RoleId ?? (int)Naming.EIVOUserRoleID.會員,
            });
            models.SubmitChanges();

            return CreateSuccessResponse("Common.Saved");
        }

        /// <summary>
        /// 啟用帳號（遷移自 AccountController.Activate）。
        /// 沿用舊版：將 UserProfile.LevelID 設為 Checked（人員已確認）。
        /// </summary>
        /// <param name="keyId">加密後的 UID（來自列表 keyId 欄位）</param>
        [HttpPost("Activate")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult Activate([FromQuery] string keyId)
            => UpdateLevel(keyId, Naming.MemberStatusDefinition.Checked);

        /// <summary>
        /// 停用帳號（遷移自 AccountController.Deactivate）。
        /// 沿用舊版：將 UserProfile.LevelID 設為 Mark_To_Delete（註記停用）。
        /// </summary>
        /// <param name="keyId">加密後的 UID（來自列表 keyId 欄位）</param>
        [HttpPost("Deactivate")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult Deactivate([FromQuery] string keyId)
            => UpdateLevel(keyId, Naming.MemberStatusDefinition.Mark_To_Delete);

        /// <summary>
        /// 重送啟用（確認）信（遷移自 AccountController.SendConfirmation）。
        /// 沿用舊版：呼叫 NotifyToActivate 通知；若通知服務未於本站設定則不寄送。
        /// </summary>
        /// <param name="keyId">加密後的 UID（來自列表 keyId 欄位）</param>
        [HttpPost("SendConfirmation")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 404)]
        public IActionResult SendConfirmation([FromQuery] string keyId)
        {
            if (!TryResolveKey(keyId, out var uid))
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            var item = models!.GetTable<UserProfile>().FirstOrDefault(u => u.UID == uid);
            if (item == null)
            {
                return CreateNotFoundResponse("帳號資料錯誤!!");
            }

            if (!CanAccessUser(uid)) return CreateErrorResponse(403, "帳號非所屬會員使用者!!");

            var notify = PortalNotification.NotifyToActivate;
            if (notify == null)
            {
                return CreateBadRequestResponse("通知服務未設定，無法寄送確認信!!");
            }

            notify(item);
            return CreateSuccessResponse("確認信已送出!!");
        }

        /// <summary>
        /// 刪除帳號（遷移自 AccountController.DeleteItem）。
        /// 沿用舊版以加密 KeyID 傳遞 UID；本頁由系統管理入口進入，故不套用舊版「所屬會員」限制。
        /// </summary>
        /// <param name="keyId">加密後的 UID（來自列表 keyId 欄位）</param>
        [HttpPost("DeleteItem")]
        [ProducesResponseType(typeof(BaseResponseDto), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 500)]
        public IActionResult DeleteItem([FromQuery] string keyId)
        {
            if (!TryResolveKey(keyId, out var uid))
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            var item = models!.GetTable<UserProfile>().FirstOrDefault(u => u.UID == uid);
            if (item == null)
            {
                return CreateBadRequestResponse("帳號資料錯誤!!");
            }

            if (!CanAccessUser(uid)) return CreateErrorResponse(403, "帳號非所屬會員使用者!!");

            try
            {
                models.GetTable<UserProfile>().Remove(item);
                models.SubmitChanges();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error deleting user account");
                return CreateErrorResponse(500, ex.Message);
            }

            return CreateSuccessResponse("Common.Saved");
        }

        /// <summary>
        /// 更新帳號的 UserProfile.LevelID（啟用 / 停用共用）。
        /// </summary>
        private IActionResult UpdateLevel(string keyId, Naming.MemberStatusDefinition level)
        {
            if (!TryResolveKey(keyId, out var uid))
            {
                return CreateBadRequestResponse("Common.InvalidParameter");
            }

            var item = models!.GetTable<UserProfile>().FirstOrDefault(u => u.UID == uid);
            if (item == null)
            {
                return CreateNotFoundResponse("帳號資料錯誤!!");
            }

            if (!CanAccessUser(uid)) return CreateErrorResponse(403, "帳號非所屬會員使用者!!");

            item.LevelID = (int)level;
            models.SubmitChanges();

            return CreateSuccessResponse("Common.Saved");
        }

        /// <summary>登入者是否為系統管理（可存取全部營業人）。</summary>
        private bool IsAdmin() => OrganizationScope.IsSystemAdmin(User.GetRoleId(), User.GetCategoryId());

        /// <summary>是否有權存取指定營業人（系統管理一律可；否則須在其角色範圍內）。</summary>
        private bool CanAccessSeller(int companyId)
            => OrganizationScope.CanAccessSeller(models!, User.GetRoleId(), User.GetCategoryId(), User.GetCompanyId(), companyId);

        /// <summary>
        /// 是否有權存取指定使用者（系統管理一律可；否則該使用者所屬營業人須在登入者角色範圍內）。
        /// 對應舊版 AccountController.DeleteItem 之「所屬會員」限制。
        /// </summary>
        private bool CanAccessUser(int uid)
        {
            if (IsAdmin()) return true;
            var categoryId = User.GetCategoryId();
            var companyId = User.GetCompanyId();
            if (categoryId == null || companyId == null) return false;

            var allowed = OrganizationScope.AllowedOrganizations(models!, categoryId.Value, companyId.Value);
            var userCompanyIds = models!.GetTable<UserRole>().Where(r => r.UID == uid).Select(r => r.OrgaCate.CompanyID);
            return allowed.Any(o => userCompanyIds.Contains(o.CompanyID));
        }

        /// <summary>
        /// 解密 KeyID 取得整數識別碼（CompanyID 或 UID）；解密失敗回傳 false 並記錄警告。
        /// </summary>
        private bool TryResolveKey(string? keyId, out int value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(keyId))
            {
                return false;
            }

            try
            {
                value = keyId.DecryptKeyValue();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Invalid user account keyId");
                return false;
            }
        }
    }
}
