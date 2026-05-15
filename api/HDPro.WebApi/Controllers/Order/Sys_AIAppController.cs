/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹Sys_AIAppController编写
 */
using Microsoft.AspNetCore.Mvc;
using HDPro.Core.Controllers.Basic;
using HDPro.Entity.AttributeManager;
using HDPro.CY.Order.IServices;
namespace HDPro.CY.Order.Controllers
{
    [Route("api/Sys_AIApp")]
    [PermissionTable(Name = "Sys_AIApp")]
    public partial class Sys_AIAppController : ApiBaseController<ISys_AIAppService>
    {
        public Sys_AIAppController(ISys_AIAppService service)
        : base(service)
        {
        }
    }
}

