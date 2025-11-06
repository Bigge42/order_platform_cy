/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *Repository提供数据库操作，如果要增加数据库操作请在当前目录下Partial文件夹ORDER_NOTE_FLATRepository编写代码
 */
using HDPro.CY.Order.IRepositories;
using HDPro.Core.BaseProvider;
using HDPro.Core.EFDbContext;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.CY.Order.Repositories
{
    public partial class ORDER_NOTE_FLATRepository : RepositoryBase<ORDER_NOTE_FLAT> , IORDER_NOTE_FLATRepository
    {
    public ORDER_NOTE_FLATRepository(ServiceDbContext dbContext)
    : base(dbContext)
    {

    }
    public static IORDER_NOTE_FLATRepository Instance
    {
      get {  return AutofacContainerModule.GetService<IORDER_NOTE_FLATRepository>(); } }
    }
}
