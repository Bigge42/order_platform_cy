/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *Repository提供数据库操作，如果要增加数据库操作请在当前目录下Partial文件夹vw_OCP_Tech_BOM_Status_MonthlyRepository编写代码
 */
using HDPro.CY.Order.IRepositories;
using HDPro.Core.BaseProvider;
using HDPro.Core.EFDbContext;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.CY.Order.Repositories
{
    public partial class vw_OCP_Tech_BOM_Status_MonthlyRepository : RepositoryBase<vw_OCP_Tech_BOM_Status_Monthly> , Ivw_OCP_Tech_BOM_Status_MonthlyRepository
    {
    public vw_OCP_Tech_BOM_Status_MonthlyRepository(ServiceDbContext dbContext)
    : base(dbContext)
    {

    }
    public static Ivw_OCP_Tech_BOM_Status_MonthlyRepository Instance
    {
      get {  return AutofacContainerModule.GetService<Ivw_OCP_Tech_BOM_Status_MonthlyRepository>(); } }
    }
}
