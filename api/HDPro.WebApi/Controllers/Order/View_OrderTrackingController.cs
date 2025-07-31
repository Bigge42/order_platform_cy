/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹View_OrderTrackingController编写
 */
using Microsoft.AspNetCore.Mvc;
using HDPro.Core.Controllers.Basic;
using HDPro.Entity.AttributeManager;
using HDPro.CY.Order.IServices;
namespace HDPro.CY.Order.Controllers
{
    [Route("api/View_OrderTracking")]
    [PermissionTable(Name = "View_OrderTracking")]
    public partial class View_OrderTrackingController : ApiBaseController<IView_OrderTrackingService>
    {
        public View_OrderTrackingController(IView_OrderTrackingService service)
        : base(service)
        {
        }
    }
}

