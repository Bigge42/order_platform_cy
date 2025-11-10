/*
 *Author：hmf
 *Contact：461857658@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下vw_OCP_Tech_BOM_Status_MonthlyService与Ivw_OCP_Tech_BOM_Status_MonthlyService中编写
 */
using HDPro.CY.Order.IRepositories;
using HDPro.CY.Order.IServices;
using HDPro.CY.Order.Services;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.CY.Order.Services
{
    public partial class vw_OCP_Tech_BOM_Status_MonthlyService : CYOrderServiceBase<vw_OCP_Tech_BOM_Status_Monthly, Ivw_OCP_Tech_BOM_Status_MonthlyRepository>
    , Ivw_OCP_Tech_BOM_Status_MonthlyService, IDependency
    {
    public static Ivw_OCP_Tech_BOM_Status_MonthlyService Instance
    {
      get { return AutofacContainerModule.GetService<Ivw_OCP_Tech_BOM_Status_MonthlyService>(); } }
    }
 } 