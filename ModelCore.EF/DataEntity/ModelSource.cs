using CommonLib.Core.DataWork;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModelCore.DataEntity
{
    public partial class ModelSource<TEntity> : ModelSource<ApplicationDbContext, TEntity>
        where TEntity : class, new()
    {
        public ModelSource() : base() { }
        public ModelSource(GenericDbContext<ApplicationDbContext> manager) : base(manager) { }

        //public Naming.DataResultMode ResultModel
        //{
        //    get;
        //    set;
        //}

    }

    public partial class ModelSourceInquiry<TEntity> : ModelSourceInquiry<ApplicationDbContext, TEntity>
        where TEntity : class, new()
    {
        protected object? _viewModel;

    }

    public class ModelSource : GenericDbContext<ApplicationDbContext>
    {

        public ModelSource() : base() { }
        public ModelSource(GenericDbContext<ApplicationDbContext> manager) : base(manager) { }

    }
}
