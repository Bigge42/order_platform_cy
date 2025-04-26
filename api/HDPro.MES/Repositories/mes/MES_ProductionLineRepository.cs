/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *Repository提供数据库操作，如果要增加数据库操作请在当前目录下Partial文件夹MES_ProductionLineRepository编写代码
 */
using HDPro.MES.IRepositories;
using HDPro.Core.BaseProvider;
using HDPro.Core.EFDbContext;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.MES.Repositories
{
    public partial class MES_ProductionLineRepository : RepositoryBase<MES_ProductionLine> , IMES_ProductionLineRepository
    {
    public MES_ProductionLineRepository(ServiceDbContext dbContext)
    : base(dbContext)
    {

    }
    public static IMES_ProductionLineRepository Instance
    {
      get {  return AutofacContainerModule.GetService<IMES_ProductionLineRepository>(); } }
    }
}
