using ModelCore.DTOs;

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
        public string? RedirectUrl { get; set; }

        /// <summary>
        /// JWT access token
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Refresh token
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Token expiration date
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// User information
        /// </summary>
        public UserProfileDto User { get; set; } = new();
    }
}
