/*
 *Author：jxx
 *Contact：461857658@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下Sys_DbServiceService与ISys_DbServiceService中编写
 */
using HDPro.Sys.IRepositories;
using HDPro.Sys.IServices;
using HDPro.Core.BaseProvider;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.Sys.Services
{
    public partial class Sys_DbServiceService : ServiceBase<Sys_DbService, ISys_DbServiceRepository>
    , ISys_DbServiceService, IDependency
    {
    public Sys_DbServiceService(ISys_DbServiceRepository repository)
    : base(repository)
    {
    Init(repository);
    }
    public static ISys_DbServiceService Instance
    {
      get { return AutofacContainerModule.GetService<ISys_DbServiceService>(); } }
    }
 }
