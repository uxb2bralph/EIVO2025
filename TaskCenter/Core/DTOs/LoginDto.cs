using ModelCore.DTOs;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TaskCenter.Core.DTOs
{
    public class LoginDto
    {
        public string? Id { get; set; } = string.Empty;
        public string? Password { get; set; } = string.Empty;
        public bool? RememberMe { get; set; }
    }

    public class LoginResultDto
    {
        [JsonPropertyName("redirectUrl")]
        public string? RedirectUrl { get; set; }

        /// <summary>
        /// JWT access token
        /// </summary>
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Refresh token
        /// </summary>
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Token expiration date
        /// </summary>
        [JsonPropertyName("expiresAt")]
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// User information
        /// </summary>
        [JsonPropertyName("user")]
        public UserProfileDto User { get; set; } = new();

        /// <summary>
        /// 依使用者角色解析出的側邊選單群組（供前端 DefaultLayout 直接渲染）。
        /// </summary>
        [JsonPropertyName("menuGroups")]
        public List<MenuGroupDto> MenuGroups { get; set; } = new();
    }
}
