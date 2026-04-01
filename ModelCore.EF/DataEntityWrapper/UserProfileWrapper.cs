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
