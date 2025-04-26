/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *Repository提供数据库操作，如果要增加数据库操作请在当前目录下Partial文件夹Demo_ProductRepository编写代码
 */
using HDPro.DbTest.IRepositories;
using HDPro.Core.BaseProvider;
using HDPro.Core.EFDbContext;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.DbTest.Repositories
{
    public partial class Demo_ProductRepository : RepositoryBase<Demo_Product> , IDemo_ProductRepository
    {
    public Demo_ProductRepository(SysDbContext dbContext)
    : base(dbContext)
    {

    }
    public static IDemo_ProductRepository Instance
    {
      get {  return AutofacContainerModule.GetService<IDemo_ProductRepository>(); } }
    }
}
