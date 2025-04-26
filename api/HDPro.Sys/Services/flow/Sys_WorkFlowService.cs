/*
 *Author：jxx
 *Contact：461857658@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下Sys_WorkFlowService与ISys_WorkFlowService中编写
 */
using HDPro.Sys.IRepositories;
using HDPro.Sys.IServices;
using HDPro.Core.BaseProvider;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.Sys.Services
{
    public partial class Sys_WorkFlowService : ServiceBase<Sys_WorkFlow, ISys_WorkFlowRepository>
    , ISys_WorkFlowService, IDependency
    {
    public Sys_WorkFlowService(ISys_WorkFlowRepository repository)
    : base(repository)
    {
    Init(repository);
    }
    public static ISys_WorkFlowService Instance
    {
      get { return AutofacContainerModule.GetService<ISys_WorkFlowService>(); } }
    }
 }
