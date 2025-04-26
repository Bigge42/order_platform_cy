/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *Repository提供数据库操作，如果要增加数据库操作请在当前目录下Partial文件夹MES_MaterialCatalogRepository编写代码
 */
using HDPro.MES.IRepositories;
using HDPro.Core.BaseProvider;
using HDPro.Core.EFDbContext;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.MES.Repositories
{
    public partial class MES_MaterialCatalogRepository : RepositoryBase<MES_MaterialCatalog> , IMES_MaterialCatalogRepository
    {
    public MES_MaterialCatalogRepository(ServiceDbContext dbContext)
    : base(dbContext)
    {

    }
    public static IMES_MaterialCatalogRepository Instance
    {
      get {  return AutofacContainerModule.GetService<IMES_MaterialCatalogRepository>(); } }
    }
}
