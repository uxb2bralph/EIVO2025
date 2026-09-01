using ModelCore.DataEntity;
using ModelCore.DTOs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ModelCore.DataEntityWrapper
{
    public class UserProfileWrapper : EntityWrapper<UserProfile>
    {
        public UserProfileWrapper(UserProfile entity) : base(entity)
        {

        }

        public int? RoleIndex { get; set; }
        public String? CurrentSiteMenu { get; set; }
        public UserRole? CurrentUserRole { get; private set; }
        public void DetermineUserRole()
        {
            RoleIndex = Entity.UserRole.Any() ? 0 : -1;
            if (RoleIndex >= 0)
            {
                CurrentUserRole = Entity.UserRole?.ElementAt(RoleIndex.Value);
            }
        }

        /// <summary>
        /// 以指定的角色索引還原目前角色；索引無效時退回 <see cref="DetermineUserRole()"/> 的預設行為。
        /// 供背景作業以自己的 DbContext 重新載入使用者時，沿用請求當下所選的角色。
        /// </summary>
        public void DetermineUserRole(int? roleIndex)
        {
            if (roleIndex.HasValue && roleIndex.Value >= 0 && Entity.UserRole.Count > roleIndex.Value)
            {
                RoleIndex = roleIndex;
                CurrentUserRole = Entity.UserRole.ElementAt(roleIndex.Value);
                return;
            }

            DetermineUserRole();
        }

        protected internal Hashtable? _values;
        public object? this[object index]
        {
            get
            {
                return _values?[index];
            }
            set
            {
                if (_values == null)
                {
                    _values = new Hashtable();
                }

                _values[index] = value;
            }
        }
    }
}
