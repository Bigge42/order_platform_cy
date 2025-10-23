using HDPro.Entity.DomainModels;

namespace HDPro.CY.Order.Repositories
{
    public partial class MaterialCallBoardRepository : RepositoryBase<MaterialCallBoard>, IMaterialCallBoardRepository
    {
        public MaterialCallBoardRepository(CYOrderContext dbContext) : base(dbContext)
        {
        }
    }
}
