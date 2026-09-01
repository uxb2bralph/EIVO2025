using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelCore.DataEntity;
using ModelCore.Locale;
using WebHome.Helper;
using ModelCore.DataEntityWrapper;

namespace WebHome.Controllers
{
    [Route("api/auth")]
    public class AuthApiController : SampleController<InvoiceItem>
    {
        public AuthApiController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        /// <summary>
        /// POST /api/auth/login
        /// Accepts { id, password, rememberMe } and authenticates the user via cookie.
        /// Returns { success, pid, userName, roleId, roleName, redirectUrl } or { success:false, message }.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] AuthLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Id) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Ok(new { success = false, message = "請輸入帳號與密碼" });
            }

            LoginHandler login = new LoginHandler(this);
            string msg;
            if (!login.ProcessLogin(request.Id, request.Password, out msg, out UserProfileWrapper? member))
            {
                return Ok(new { success = false, message = msg ?? "登入失敗，請確認帳密" });
            }

            if (member.Entity.Expiration.HasValue && member.Entity.Expiration < DateTime.Today)
            {
                return Ok(new
                {
                    success = false,
                    message = "密碼已過期，請更新密碼",
                    requirePasswordChange = true
                });
            }

            var role = member.CurrentUserRole;
            int? roleId = role?.RoleID;
            string? roleName = roleId.HasValue
                ? Enum.GetName(typeof(Naming.RoleID), roleId.Value) ?? roleId.Value.ToString()
                : null;

            return Ok(new
            {
                success = true,
                pid = member.Entity.PID,
                userName = member.Entity.UserName ?? member.Entity.PID,
                roleId,
                roleName,
                redirectUrl = "/MainPage"
            });
        }

        /// <summary>
        /// GET /api/auth/me
        /// Returns current authenticated user's profile and role. Requires [Authorize].
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var profile = HttpContext.GetUser();
            if (profile == null)
            {
                return Unauthorized(new { message = "未登入" });
            }

            var role = profile.CurrentUserRole;
            int? roleId = role?.RoleID;
            string? roleName = roleId.HasValue
                ? Enum.GetName(typeof(Naming.RoleID), roleId.Value) ?? roleId.Value.ToString()
                : null;

            return Ok(new
            {
                pid = profile.Entity.PID,
                userName = profile.Entity.UserName ?? profile.Entity.PID,
                roleId,
                roleName
            });
        }

        /// <summary>
        /// POST /api/auth/logout
        /// Clears authentication cookie and signs the user out.
        /// </summary>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Logout();
            return Ok(new { success = true });
        }
    }

    public class AuthLoginRequest
    {
        public string? Id { get; set; }
        public string? Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
