using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonLib.DataAccess;

namespace ModelCore.DocumentFlowManagement
{
    public static partial class ExtensionMethods
    {
    }

    public partial class FlowEntityDataContext
    {
        public FlowEntityDataContext() :
                base(global::ModelCore.Properties.AppSettings.Default.ConnectionString, mappingSource)
        {
            OnCreated();
        }
    }

    public partial class FlowEntityManager<TEntity> : GenericManager<FlowEntityDataContext, TEntity>
        where TEntity : class,new()
    {
        public FlowEntityManager() : base() { }
        public FlowEntityManager(GenericManager<FlowEntityDataContext> manager) : base(manager) { }
    }

}
