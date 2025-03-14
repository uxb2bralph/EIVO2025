using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CommonLib.DataAccess;
using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Resource;
using ModelCore.Schema.EIVO;
using CommonLib.Utility;

namespace ModelCore.Models.ViewModel
{
    public partial class A0101ViewModelValidator<TEntity> : A0401ViewModelValidator<TEntity>
        where TEntity : class, new()
    {
        public A0101ViewModelValidator(ModelSource<TEntity> mgr, Organization owner) : base(mgr, owner)
        {

        }

    }
}
