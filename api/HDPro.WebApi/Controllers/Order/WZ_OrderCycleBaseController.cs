/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹WZ_OrderCycleBaseController编写
 */
using Microsoft.AspNetCore.Mvc;
using HDPro.Core.Controllers.Basic;
using HDPro.Entity.AttributeManager;
using HDPro.CY.Order.IServices;
namespace HDPro.CY.Order.Controllers
{
    [Route("api/WZ_OrderCycleBase")]
    [PermissionTable(Name = "WZ_OrderCycleBase")]
    public partial class WZ_OrderCycleBaseController : ApiBaseController<IWZ_OrderCycleBaseService>
    {
        public WZ_OrderCycleBaseController(IWZ_OrderCycleBaseService service)
        : base(service)
        {
        }
    }
}

