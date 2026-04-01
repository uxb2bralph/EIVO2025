using ModelCore.DataEntity;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ModelCore.DTOs
{
    public class UserProfileDto
    {
        public int UID { get; set; }

        public string? UserName { get; set; }

        public string PID { get; set; } = null!;

        public string? Password { get; set; }

        public string? ContactTitle { get; set; }

        public string? Address { get; set; }

        public string? City { get; set; }

        public string? Region { get; set; }

        public string? PostalCode { get; set; }

        public string? Country { get; set; }

        public string? MobilePhone { get; set; }

        public string? Phone { get; set; }

        public string? Phone2 { get; set; }

        public string? Fax { get; set; }

        public string? EMail { get; set; }

        public DateTime? Expiration { get; set; }

        public int? Creator { get; set; }

        public int? AuthID { get; set; }

        public int? LevelID { get; set; }

        public string? ThemeName { get; set; }

        public string? Password2 { get; set; }

        public string? MailID { get; set; }

        // Navigation / related entities kept as in the entity definitions
        public UserProfileDto? Auth { get; set; }

        public List<UserRoleDto> UserRole { get; set; } = new List<UserRoleDto>();

    }
}
