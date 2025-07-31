/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *Repository提供数据库操作，如果要增加数据库操作请在当前目录下Partial文件夹View_OrderTrackingRepository编写代码
 */
using HDPro.CY.Order.IRepositories;
using HDPro.Core.BaseProvider;
using HDPro.Core.EFDbContext;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.CY.Order.Repositories
{
    public partial class View_OrderTrackingRepository : RepositoryBase<View_OrderTracking> , IView_OrderTrackingRepository
    {
    public View_OrderTrackingRepository(ServiceDbContext dbContext)
    : base(dbContext)
    {

    }
    public static IView_OrderTrackingRepository Instance
    {
      get {  return AutofacContainerModule.GetService<IView_OrderTrackingRepository>(); } }
    }
}
