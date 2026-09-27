using CommonLib.Core.DataWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace ModelCore.DataEntity
{
    public partial class ApplicationDbContext
    {
        private readonly CommonLib.DataAccess.SqlLogger? _logWriter;

        private readonly String? _defaultConnectionString = ModelCore.Properties.AppSettings.Default.ConnectionString;
        public ApplicationDbContext()
        {
            if (CommonLib.Core.Properties.AppSettings.Default.SqlLog)
            {
                _logWriter = new CommonLib.DataAccess.SqlLogger { IgnoreSelect = CommonLib.Core.Properties.AppSettings.Default.SqlLogIgnoreSelect };
            }
        }
        public ApplicationDbContext(String? defaultConnectionString) : this()
        {
            _defaultConnectionString = defaultConnectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(_defaultConnectionString,
                sqlOptions => sqlOptions.CommandTimeout((int)TimeSpan.FromMinutes(30).TotalSeconds))
            .LogTo((sql) =>
            {
                _logWriter?.WriteLine(sql);
            })
            .UseLazyLoadingProxies(); // << 開啟 Lazy Loading Proxy

        public override void Dispose()
        {
            base.Dispose();
            _logWriter?.Dispose();
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {

        }
   
    }

}
