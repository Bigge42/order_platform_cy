/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *Repository提供数据库操作，如果要增加数据库操作请在当前目录下Partial文件夹Sys_GroupRepository编写代码
 */
using HDPro.Sys.IRepositories;
using HDPro.Core.BaseProvider;
using HDPro.Core.EFDbContext;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.Sys.Repositories
{
    public partial class Sys_GroupRepository : RepositoryBase<Sys_Group> , ISys_GroupRepository
    {
    public Sys_GroupRepository(SysDbContext dbContext)
    : base(dbContext)
    {

    }
    public static ISys_GroupRepository Instance
    {
      get {  return AutofacContainerModule.GetService<ISys_GroupRepository>(); } }
    }
}
