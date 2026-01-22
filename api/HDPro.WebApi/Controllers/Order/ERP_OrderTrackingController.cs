/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹ERP_OrderTrackingController编写
 */
using Microsoft.AspNetCore.Mvc;
using HDPro.Core.Controllers.Basic;
using HDPro.Entity.AttributeManager;
using HDPro.CY.Order.IServices;
namespace HDPro.CY.Order.Controllers
{
    [Route("api/ERP_OrderTracking")]
    [PermissionTable(Name = "ERP_OrderTracking")]
    public partial class ERP_OrderTrackingController : ApiBaseController<IERP_OrderTrackingService>
    {
        public ERP_OrderTrackingController(IERP_OrderTrackingService service)
        : base(service)
        {
        }
    }
}

