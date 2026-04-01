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
using CommonLib.Core.DataWork;




namespace ModelCore.ModelTemplate
{
	/// <summary>
	/// UserManager ªººK­n´y­z¡C
	/// </summary>
    public partial class EIVOGenericManager<TEntity> : GenericManager<ApplicationDbContext,TEntity>
        where TEntity:class,new()
	{
        public EIVOGenericManager() : base() { }
        public EIVOGenericManager(GenericManager<ApplicationDbContext> manager) : base(manager) { }
    }

   

   
    
}
