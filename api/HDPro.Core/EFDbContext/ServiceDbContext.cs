using HDPro.Core.DBManager;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.SystemModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;


namespace HDPro.Core.EFDbContext
{
    public class ServiceDbContext : BaseDbContext, IDependency
    {
        private string dbServiceId { get; set; }
        protected override string ConnectionString
        {
            get
            {
                if (dbServiceId != null)
                {
                    return DBServerProvider.GetConnectionString(dbServiceId);
                }
                return DBServerProvider.ServiceConnectingString;
            }
        }
        public ServiceDbContext() : base() { }

        public ServiceDbContext(string dbServiceId) : base()
        {
            this.dbServiceId = dbServiceId;
        }


        public ServiceDbContext(DbContextOptions<BaseDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.UseDbType(optionsBuilder, ConnectionString);
            //默认禁用实体跟踪
            optionsBuilder = optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder, typeof(ServiceEntity));
        }
    }
}
