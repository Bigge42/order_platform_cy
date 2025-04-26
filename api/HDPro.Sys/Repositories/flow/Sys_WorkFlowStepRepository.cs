/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *Repository提供数据库操作，如果要增加数据库操作请在当前目录下Partial文件夹Sys_WorkFlowStepRepository编写代码
 */
using HDPro.Sys.IRepositories;
using HDPro.Core.BaseProvider;
using HDPro.Core.EFDbContext;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.Sys.Repositories
{
    public partial class Sys_WorkFlowStepRepository : RepositoryBase<Sys_WorkFlowStep> , ISys_WorkFlowStepRepository
    {
    public Sys_WorkFlowStepRepository(SysDbContext dbContext)
    : base(dbContext)
    {

    }
    public static ISys_WorkFlowStepRepository Instance
    {
      get {  return AutofacContainerModule.GetService<ISys_WorkFlowStepRepository>(); } }
    }
}
