/*
 *Author：hmf
 *Contact：461857658@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下OCP_LackMtrlPoolService与IOCP_LackMtrlPoolService中编写
 */
using HDPro.CY.Order.IRepositories;
using HDPro.CY.Order.IServices;
using HDPro.CY.Order.Services;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.CY.Order.Services
{
    public partial class OCP_LackMtrlPoolService : CYOrderServiceBase<OCP_LackMtrlPool, IOCP_LackMtrlPoolRepository>
    , IOCP_LackMtrlPoolService, IDependency
    {
    public static IOCP_LackMtrlPoolService Instance
    {
      get { return AutofacContainerModule.GetService<IOCP_LackMtrlPoolService>(); } }
    }
 } 