/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹V_XhckkbRecord_MaterialController编写
 */
using Microsoft.AspNetCore.Mvc;
using HDPro.Core.Controllers.Basic;
using HDPro.Entity.AttributeManager;
using HDPro.CY.Order.IServices;
namespace HDPro.CY.Order.Controllers
{
    [Route("api/V_XhckkbRecord_Material")]
    [PermissionTable(Name = "V_XhckkbRecord_Material")]
    public partial class V_XhckkbRecord_MaterialController : ApiBaseController<IV_XhckkbRecord_MaterialService>
    {
        public V_XhckkbRecord_MaterialController(IV_XhckkbRecord_MaterialService service)
        : base(service)
        {
        }
    }
}

