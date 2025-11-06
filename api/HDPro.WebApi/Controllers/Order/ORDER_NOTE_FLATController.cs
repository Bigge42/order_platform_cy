/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹ORDER_NOTE_FLATController编写
 */
using Microsoft.AspNetCore.Mvc;
using HDPro.Core.Controllers.Basic;
using HDPro.Entity.AttributeManager;
using HDPro.CY.Order.IServices;
namespace HDPro.CY.Order.Controllers
{
    [Route("api/ORDER_NOTE_FLAT")]
    [PermissionTable(Name = "ORDER_NOTE_FLAT")]
    public partial class ORDER_NOTE_FLATController : ApiBaseController<IORDER_NOTE_FLATService>
    {
        public ORDER_NOTE_FLATController(IORDER_NOTE_FLATService service)
        : base(service)
        {
        }
    }
}

