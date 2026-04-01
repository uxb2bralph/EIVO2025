using CommonLib.Core.DataWork;
using Microsoft.EntityFrameworkCore;
using ModelCore.Properties;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModelCore.TurnkeyModel
{
    public partial class TurnkeyDbContext
    {
        private readonly CommonLib.DataAccess.SqlLogger? _logWriter;
        public TurnkeyDbContext()
        {
            if (CommonLib.Core.Properties.AppSettings.Default.SqlLog)
            {
                _logWriter = new CommonLib.DataAccess.SqlLogger { IgnoreSelect = CommonLib.Core.Properties.AppSettings.Default.SqlLogIgnoreSelect };
            }
        }

        private bool _useLazyLoadingProxies = true;
        public TurnkeyDbContext(DbContextOptions options, bool useLazyLoadingProxies = true)
            : base(options)
        {
            _useLazyLoadingProxies = useLazyLoadingProxies;
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(AppSettings.Default.ConnectionString,
                        sqlOptions => sqlOptions.CommandTimeout((int)TimeSpan.FromMinutes(30).TotalSeconds))
                    .LogTo((sql) =>
                    {
                        _logWriter?.WriteLine(sql);
                    });

            if (_useLazyLoadingProxies)
                optionsBuilder
                    .UseLazyLoadingProxies(); // << 開啟 Lazy Loading Proxy
        }

        public override void Dispose()
        {
            base.Dispose();
            _logWriter?.Dispose();
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Seller>()
            //    .Property(o => o.Id)
            //    .HasDefaultValueSql("NEXT VALUE FOR OrgCompanyID");
        }

    }
}
