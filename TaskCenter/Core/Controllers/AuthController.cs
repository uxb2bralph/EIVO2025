using Azure;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using ModelCore.DTOs;
using System.Security.Claims;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Handlers;
using TaskCenter.Core.Interfaces;

namespace TaskCenter.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ApiBaseController
    {
        private readonly IElementaryService _elementaryService;

        /// <summary>
        /// AuthController constructor
        /// </summary>
        /// <param name="elementaryService"></param>

        public AuthController(
            IElementaryService elementaryService,
            IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
            _elementaryService = elementaryService;
        }

        /// <summary>
        /// User login
        /// </summary>
        /// <param name="loginDto">Login credentials</param>
        /// <returns>Authentication result with JWT token</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ResponseDto<LoginResultDto>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 401)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                LoginHandler handler = new LoginHandler(_elementaryService);
                var result = await handler.AuthenticateAsync(loginDto, HttpContext);
                if (result == null)
                {
                    return CreateUnauthorizedResponse("Auth.LoginFailed");
                }

                return CreateSuccessResponse(result, "Auth.LoginSuccess");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error during login for user: {Username}", loginDto.Id);
                return CreateErrorResponse(500, "Auth.LoginError");
            }
        }

        /// <summary>
        /// Refresh JWT token
        /// </summary>
        /// <param name="refreshToken">Refresh token</param>
        /// <returns>New authentication result</returns>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(ResponseDto<LoginResultDto>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            try
            {
                LoginHandler handler = new LoginHandler(_elementaryService);

                var result = await handler.RefreshTokenAsync(refreshToken, HttpContext);
                if (result == null)
                {
                    return CreateBadRequestResponse("Error.BadRequest", new[] { "Invalid refresh token" });
                }

                return CreateSuccessResponse(result, "Auth.LoginSuccess");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error during token refresh");
                return CreateErrorResponse(500, "Auth.LoginError");
            }
        }


        /// <summary>
        /// Validate JWT token
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>Validation result</returns>
        [HttpPost("validate")]
        [ProducesResponseType(typeof(ResponseDto<bool>), 200)]
        public IActionResult ValidateToken([FromBody] string token)
        {
            try
            {
                LoginHandler handler = new LoginHandler(_elementaryService);
                var isValid = handler.ValidateToken(token, out ClaimsPrincipal principal);
                var messageKey = isValid ? "Success" : "Error.General";
                return CreateSuccessResponse(isValid, messageKey);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error during token validation");
                return CreateErrorResponse(500, "Error.General");
            }
        }

        /// <summary>
        /// Get information about the currently authenticated user.
        /// </summary>
        /// <returns>User information including id and name.</returns>
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            return Ok(new
            {
                id = User.FindFirstValue(ClaimTypes.NameIdentifier),
                name = User.Identity?.Name,
                pid = User.FindFirstValue("pid"),
                realName = User.FindFirstValue(ClaimTypes.GivenName),
            });
        }

        /// <summary>
        /// Initiates authentication with an external provider or returns current user info if already authenticated.
        /// </summary>
        /// <param name="provider">The external authentication provider (e.g., "microsoft" or "google").</param>
        /// <returns>
        /// If the user is authenticated, returns user information. Otherwise, initiates authentication challenge with the specified provider.
        /// </returns>
        [HttpGet("auth")]
        public async Task<IActionResult> AuthAsync(String? provider)
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                try
                {
                    var handler = new LoginHandler(_elementaryService);
                    var result = await handler.AuthenticateAsync(User.FindFirstValue("pid") ?? String.Empty, HttpContext);
                    if (result == null)
                    {
                        return await Logout();
                    }

                    return CreateSuccessResponse(result, "Auth.LoginSuccess");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error during external authentication for user: {id}, {PID}", User.FindFirstValue(ClaimTypes.NameIdentifier), User.FindFirstValue("pid"));
                    return await Logout();
                }
            }
            else
            {
                return BadRequest(new { error = "Invalid or missing provider." });
            }
        }

        /// <summary>
        /// Logout current user (clears authentication cookie and JWT cookie).
        /// </summary>
        /// <remarks>
        /// Signs out the local cookie (scheme "Cookies") and removes the JWT cookie ("AuthToken") if present.
        /// </remarks>
        [HttpGet("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Sign out local auth cookie
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                // Remove stored JWT (if your front-end stored it in cookie AuthToken)
                Response.Cookies.Delete("AuthToken");
                return CreateSuccessResponse("Auth.LogoutSuccess");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error during logout");
                return CreateErrorResponse(500, "Auth.LogoutError");
            }
        }

        /// <summary>
        /// Get information about the currently authenticated user.
        /// </summary>
        /// <returns>User information including id and name.</returns>
        [HttpGet("who")]
        public IActionResult Who()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                return Ok(new
                {
                    User.Identity?.AuthenticationType,
                    id = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    name = User.Identity?.Name,
                    pid = User.FindFirstValue(ClaimTypes.Email),
                    realName = User.FindFirstValue(ClaimTypes.GivenName),
                });
            }
            return Ok(new { id = 0, name = "Anonymous" });
        }
    }
}
