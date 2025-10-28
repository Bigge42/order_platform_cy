using HDPro.Core.BaseProvider;
using HDPro.Core.Extensions.AutofacManager;
using MCBEntity = HDPro.Entity.DomainModels.MaterialCallBoard;

namespace HDPro.CY.Order.IRepositories
{
    public partial interface IMaterialCallBoardRepository : IDependency, IRepository<MCBEntity> { }
}
