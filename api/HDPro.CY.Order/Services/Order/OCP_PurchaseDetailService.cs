/*
 *Author：jxx
 *Contact：461857658@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下OCP_PurchaseDetailService与IOCP_PurchaseDetailService中编写
 */
using HDPro.CY.Order.IRepositories;
using HDPro.CY.Order.IServices;
using HDPro.Core.BaseProvider;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.CY.Order.Services
{
    public partial class OCP_PurchaseDetailService : CYOrderServiceBase<OCP_PurchaseDetail, IOCP_PurchaseDetailRepository>
    , IOCP_PurchaseDetailService, IDependency
    {
    public static IOCP_PurchaseDetailService Instance
    {
      get { return AutofacContainerModule.GetService<IOCP_PurchaseDetailService>(); } }
    }
 }
