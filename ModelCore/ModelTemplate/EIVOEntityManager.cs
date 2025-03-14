using System;
using System.Data;
using System.Linq;
using System.Data.Linq;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;

using ModelCore.DataEntity;
using ModelCore.Properties;
using CommonLib.Utility;
using CommonLib.DataAccess;




namespace ModelCore.ModelTemplate
{
	/// <summary>
	/// UserManager ªººK­n´y­z¡C
	/// </summary>
    public partial class EIVOGenericManager<TEntity> : GenericManager<EIVOEntityDataContext,TEntity>
        where TEntity:class,new()
	{
        public EIVOGenericManager() : base() { }
        public EIVOGenericManager(GenericManager<EIVOEntityDataContext> manager) : base(manager) { }
    }

   

   
    
}
