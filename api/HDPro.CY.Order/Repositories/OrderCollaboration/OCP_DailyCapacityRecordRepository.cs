using HDPro.Entity.DomainModels;
using HDPro.CY.Order.IRepositories;
using HDPro.Core.BaseProvider;
using HDPro.Core.EFDbContext;

namespace HDPro.CY.Order.Repositories
{
    public partial class OCP_DailyCapacityRecordRepository : RepositoryBase<OCP_DailyCapacityRecord>, IOCP_DailyCapacityRecordRepository
    {
        public OCP_DailyCapacityRecordRepository(ServiceDbContext dbContext)
            : base(dbContext)
        {
        }
    }
}
