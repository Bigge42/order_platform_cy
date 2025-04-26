/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹Sys_RegionController编写
 */
using Microsoft.AspNetCore.Mvc;
using HDPro.Core.Controllers.Basic;
using HDPro.Entity.AttributeManager;
using HDPro.Sys.IServices;
namespace HDPro.Sys.Controllers
{
    [Route("api/Sys_Region")]
    [PermissionTable(Name = "Sys_Region")]
    public partial class Sys_RegionController : ApiBaseController<ISys_RegionService>
    {
        public Sys_RegionController(ISys_RegionService service)
        : base(service)
        {
        }
    }
}

