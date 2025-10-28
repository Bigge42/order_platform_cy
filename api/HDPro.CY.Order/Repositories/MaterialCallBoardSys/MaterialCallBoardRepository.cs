using HDPro.Core.BaseProvider;
using HDPro.Core.EFDbContext;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.CY.Order.IRepositories;
using MCBEntity = HDPro.Entity.DomainModels.MaterialCallBoard;

namespace HDPro.CY.Order.Repositories
{
    public partial class MaterialCallBoardRepository
        : RepositoryBase<MCBEntity>, IMaterialCallBoardRepository
    {
        public MaterialCallBoardRepository(ServiceDbContext dbContext) : base(dbContext) { }

        public static IMaterialCallBoardRepository Instance
            => AutofacContainerModule.GetService<IMaterialCallBoardRepository>();
    }
}
