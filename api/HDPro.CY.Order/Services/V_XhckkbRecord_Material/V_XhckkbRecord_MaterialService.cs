/*
 *Author：hmf
 *Contact：461857658@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下V_XhckkbRecord_MaterialService与IV_XhckkbRecord_MaterialService中编写
 */
using HDPro.CY.Order.IRepositories;
using HDPro.CY.Order.IServices;
using HDPro.CY.Order.Services;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.CY.Order.Services
{
    public partial class V_XhckkbRecord_MaterialService : CYOrderServiceBase<V_XhckkbRecord_Material, IV_XhckkbRecord_MaterialRepository>
    , IV_XhckkbRecord_MaterialService, IDependency
    {
    public static IV_XhckkbRecord_MaterialService Instance
    {
      get { return AutofacContainerModule.GetService<IV_XhckkbRecord_MaterialService>(); } }
    }
 } 