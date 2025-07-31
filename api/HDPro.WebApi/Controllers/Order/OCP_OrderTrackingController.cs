/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹OCP_OrderTrackingController编写
 */
using Microsoft.AspNetCore.Mvc;
using HDPro.Core.Controllers.Basic;
using HDPro.Entity.AttributeManager;
using HDPro.CY.Order.IServices;
using System.Threading.Tasks;
using HDPro.Core.Utilities;
using HDPro.Core.Filters;
using System;
using HDPro.Core.Filters;

namespace HDPro.CY.Order.Controllers
{
    [Route("api/OCP_OrderTracking")]
    [PermissionTable(Name = "OCP_OrderTracking")]
    public partial class OCP_OrderTrackingController : ApiBaseController<IOCP_OrderTrackingService>
    {
        public OCP_OrderTrackingController(IOCP_OrderTrackingService service)
        : base(service)
        {
        }
     
    }
}

