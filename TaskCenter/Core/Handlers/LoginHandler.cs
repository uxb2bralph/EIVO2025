using CommonLib.Core.DataWork;
using CommonLib.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ModelCore.DataEntity;
using ModelCore.DTOs;
using ModelCore.Security.MembershipManagement;
using System.Data.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Interfaces;
using TaskCenter.Properties;

namespace TaskCenter.Core.Handlers
{
    public class LoginHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IElementaryService _elementaryService;
        private readonly ILogger<LoginHandler> _logger;
        /// <inheritdoc />
        public LoginHandler(
            IElementaryService elementaryService)
        {
            _elementaryService = elementaryService;
            _unitOfWork = _elementaryService.UnitOfWork;
            _logger = _elementaryService.LoggerFactory.CreateLogger<LoginHandler>();
        }

        /// <inheritdoc />
        public async Task<LoginResultDto?> AuthenticateAsync(LoginDto loginDto, HttpContext context)
        {
            try
            {
                // Hash the password (you should use a proper password hashing library like BCrypt)
                var hashedPassword = loginDto.Password?.ComputeSHA256Hash() ?? string.Empty;
                _logger.LogInformation("Login attempt - Id: {Id}, HashedPassword: {HashedPassword}", loginDto.Id, hashedPassword);

                UserProfileManager manager = new UserProfileManager(new GenericDbContext<ApplicationDbContext>(_unitOfWork.Context));
                var user = UserProfileFactory.CreateInstance(manager, loginDto.Id ?? "", loginDto.Password ?? "");

                if (user == null)
                {
                    _logger.LogWarning("Authentication failed for user: {Id} - User not found or password mismatch", loginDto.Id);
                    return null;
                }

                var currentUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}";

                // TODO: Create AutoMapper instance and map UserProfile to UserProfileDto
                //var userDto = _mapper.Map<UserProfileDto>(user);

                return await SignIn(context, user.Entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication for user: {Id}", loginDto.Id);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<LoginResultDto?> AuthenticateAsync(String pid, HttpContext context)
        {
            try
            {
                UserProfile? user = _unitOfWork.Context.UserProfile.Where(u => u.PID == pid)
                    .FirstOrDefault();

                if (user == null)
                {
                    _logger.LogWarning("Authentication failed for user: {PID}", pid);
                    return null;
                }

                return await SignIn(context, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication for user: {PID}", pid);
                throw;
            }
        }

        private async Task<LoginResultDto?> SignIn(HttpContext context, UserProfile user)
        {
            var (token, identity) = await GenerateJwtTokenAsync(user);
            var refreshToken = GenerateRefreshToken(user);

            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(
                    new ClaimsIdentity(identity.Claims,
                        CookieAuthenticationDefaults.AuthenticationScheme)));

            var userDto = user.ToDto();

            return new LoginResultDto
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
                User = userDto,
            };
        }

        private static string HashRefreshToken(string refreshToken)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(refreshToken));
            return Convert.ToBase64String(hashedBytes);
        }

        /// <inheritdoc />
        public async Task<LoginResultDto?> RefreshTokenAsync(string refreshToken, HttpContext context)
        {
            try
            {
                _logger.LogInformation("Refresh token requested");

                // First validate the JWT structure (signature, issuer, audience, lifetime)
                var isValid = ValidateToken(refreshToken, out ClaimsPrincipal principal);
                if (!isValid)
                {
                    _logger.LogWarning("Invalid JWT token structure");
                    return null;
                }

                // 必須是 refresh token，避免 access token 被拿來換新 token
                var tokenType = principal.Claims.FirstOrDefault(c => c.Type == "token_type")?.Value;
                if (!string.Equals(tokenType, "refresh", StringComparison.Ordinal))
                {
                    _logger.LogWarning("Provided token is not a refresh token");
                    return null;
                }

                var userProfileIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userProfileIdClaim) || !int.TryParse(userProfileIdClaim, out var userProfileId))
                {
                    _logger.LogWarning("Invalid or missing UserProfileId in refresh token");
                    return null;
                }

                // Get user profile
                var user = _unitOfWork.Context.UserProfile.Where(u => u.UID == userProfileId).FirstOrDefault();
                if (user == null)
                {
                    _logger.LogWarning("User not found for refresh token: {UserProfileId}", userProfileId);
                    return null;
                }

                // Generate new tokens (rotate refresh token)
                var (token, identity) = await GenerateJwtTokenAsync(user);
                var newRefreshToken = GenerateRefreshToken(user);

                var userDto = user.ToDto();

                return new LoginResultDto
                {
                    AccessToken = token!,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
                    User = userDto,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                throw;
            }
        }

        /// <inheritdoc />
        public bool ValidateToken(string token, out ClaimsPrincipal principal)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(AppSettings.Default.Jwt.SecurityKey!);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = AppSettings.Default.Jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = AppSettings.Default.Jwt.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                return validatedToken != null;
            }
            catch
            {
                principal = null!;
                return false;
            }
        }

        private async Task<(string Token, ClaimsIdentity Identity)> GenerateJwtTokenAsync(UserProfile user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(AppSettings.Default.Jwt.SecurityKey!);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UID.ToString()),
                new(ClaimTypes.Name, user.PID ?? string.Empty),
                new(ClaimTypes.GivenName, user.UserName ?? string.Empty),
                new("pid", user.PID ?? string.Empty),
            };


            var claimsIdentity = new ClaimsIdentity(claims);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
                Issuer = AppSettings.Default.Jwt.Issuer,
                Audience = AppSettings.Default.Jwt.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return (tokenHandler.WriteToken(token), claimsIdentity);
        }

        /// <summary>
        /// 產生 refresh token。為了讓 <see cref="RefreshTokenAsync"/> 能離線驗證（本系統未保存
        /// refresh token），refresh token 本身就是一個帶有 token_type=refresh 標記、效期較長的 JWT。
        /// </summary>
        private string GenerateRefreshToken(UserProfile user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(AppSettings.Default.Jwt.SecurityKey!);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UID.ToString()),
                new("token_type", "refresh"),
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(GetRefreshTokenExpirationMinutes()),
                Issuer = AppSettings.Default.Jwt.Issuer,
                Audience = AppSettings.Default.Jwt.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        private int GetTokenExpirationMinutes()
        {
            return AppSettings.Default.Jwt.ExpirationInMinutes;
        }

        private int GetRefreshTokenExpirationMinutes()
        {
            return AppSettings.Default.Jwt.RefreshExpirationInMinutes;
        }

        /// <inheritdoc />
        public async Task<bool> LogoutAsync(HttpContext context)
        {
            try
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return false;
            }
        }
    }

}
