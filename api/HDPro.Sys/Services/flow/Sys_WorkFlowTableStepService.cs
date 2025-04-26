/*
 *Author：jxx
 *Contact：461857658@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下Sys_WorkFlowTableStepService与ISys_WorkFlowTableStepService中编写
 */
using HDPro.Sys.IRepositories;
using HDPro.Sys.IServices;
using HDPro.Core.BaseProvider;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.Sys.Services
{
    public partial class Sys_WorkFlowTableStepService : ServiceBase<Sys_WorkFlowTableStep, ISys_WorkFlowTableStepRepository>
    , ISys_WorkFlowTableStepService, IDependency
    {
    public Sys_WorkFlowTableStepService(ISys_WorkFlowTableStepRepository repository)
    : base(repository)
    {
    Init(repository);
    }
    public static ISys_WorkFlowTableStepService Instance
    {
      get { return AutofacContainerModule.GetService<ISys_WorkFlowTableStepService>(); } }
    }
 }
