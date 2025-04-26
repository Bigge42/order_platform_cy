/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *Repository提供数据库操作，如果要增加数据库操作请在当前目录下Partial文件夹Demo_ProductSizeRepository编写代码
 */
using HDPro.DbTest.IRepositories;
using HDPro.Core.BaseProvider;
using HDPro.Core.EFDbContext;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.DbTest.Repositories
{
    public partial class Demo_ProductSizeRepository : RepositoryBase<Demo_ProductSize> , IDemo_ProductSizeRepository
    {
    public Demo_ProductSizeRepository(SysDbContext dbContext)
    : base(dbContext)
    {

    }
    public static IDemo_ProductSizeRepository Instance
    {
      get {  return AutofacContainerModule.GetService<IDemo_ProductSizeRepository>(); } }
    }
}
