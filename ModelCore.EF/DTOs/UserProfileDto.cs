using ModelCore.DataEntity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ModelCore.DTOs
{
    public static class UserProfileMappingExtensions
    {
        public static UserProfileDto ToDto(this UserProfile user)
        {
            // 取使用者的主要角色（第一筆 UserRole），供前端判斷選單與顯示用。
            var currentRole = user.UserRole?.FirstOrDefault();

            return new()
            {
                UID = user.UID,
                UserName = user.UserName,
                PID = user.PID,
                Password = user.Password,
                ContactTitle = user.ContactTitle,
                Address = user.Address,
                City = user.City,
                Region = user.Region,
                PostalCode = user.PostalCode,
                Country = user.Country,
                MobilePhone = user.MobilePhone,
                Phone = user.Phone,
                Phone2 = user.Phone2,
                Fax = user.Fax,
                EMail = user.EMail,
                Expiration = user.Expiration,
                Creator = user.Creator,
                AuthID = user.AuthID,
                LevelID = user.LevelID,
                ThemeName = user.ThemeName,
                Password2 = user.Password2,
                MailID = user.MailID,
                RoleID = currentRole?.RoleID,
                RoleName = currentRole?.UserRoleDefinition?.Role,
            };
        }
    }

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

        /// <summary>主要角色 ID（取自第一筆 UserRole），前端據此決定選單。</summary>
        public int? RoleID { get; set; }

        /// <summary>主要角色名稱（UserRoleDefinition.UserRoleDefinition），僅供顯示。</summary>
        public string? RoleName { get; set; }

        // Navigation / related entities kept as in the entity definitions
        public UserProfileDto? Auth { get; set; }

        public List<UserRoleDto> UserRole { get; set; } = new List<UserRoleDto>();

    }
}
