/*
 *Author：hmf
 *Contact：461857658@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下Sys_RoleAIAppService与ISys_RoleAIAppService中编写
 */
using HDPro.CY.Order.IRepositories;
using HDPro.CY.Order.IServices;
using HDPro.CY.Order.Services;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.CY.Order.Services
{
    public partial class Sys_RoleAIAppService : CYOrderServiceBase<Sys_RoleAIApp, ISys_RoleAIAppRepository>
    , ISys_RoleAIAppService, IDependency
    {
    public static ISys_RoleAIAppService Instance
    {
      get { return AutofacContainerModule.GetService<ISys_RoleAIAppService>(); } }
    }
 } 