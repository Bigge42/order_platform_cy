/*
 *Author：hmf
 *Contact：461857658@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下OCP_BusinessTypeResponsibleService与IOCP_BusinessTypeResponsibleService中编写
 */
using HDPro.CY.Order.IRepositories;
using HDPro.CY.Order.IServices;
using HDPro.CY.Order.Services;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.CY.Order.Services
{
    public partial class OCP_BusinessTypeResponsibleService : CYOrderServiceBase<OCP_BusinessTypeResponsible, IOCP_BusinessTypeResponsibleRepository>
    , IOCP_BusinessTypeResponsibleService, IDependency
    {
    public static IOCP_BusinessTypeResponsibleService Instance
    {
      get { return AutofacContainerModule.GetService<IOCP_BusinessTypeResponsibleService>(); } }
    }
 } 