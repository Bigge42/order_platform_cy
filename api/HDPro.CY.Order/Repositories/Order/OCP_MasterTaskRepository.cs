/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *Repository提供数据库操作，如果要增加数据库操作请在当前目录下Partial文件夹OCP_MasterTaskRepository编写代码
 */
using HDPro.CY.Order.IRepositories;
using HDPro.Core.BaseProvider;
using HDPro.Core.EFDbContext;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.CY.Order.Repositories
{
    public partial class OCP_MasterTaskRepository : RepositoryBase<OCP_MasterTask> , IOCP_MasterTaskRepository
    {
    public OCP_MasterTaskRepository(ServiceDbContext dbContext)
    : base(dbContext)
    {

    }
    public static IOCP_MasterTaskRepository Instance
    {
      get {  return AutofacContainerModule.GetService<IOCP_MasterTaskRepository>(); } }
    }
}
