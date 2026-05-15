/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹Sys_RoleAIAppController编写
 */
using Microsoft.AspNetCore.Mvc;
using HDPro.Core.Controllers.Basic;
using HDPro.Entity.AttributeManager;
using HDPro.CY.Order.IServices;
namespace HDPro.CY.Order.Controllers
{
    [Route("api/Sys_RoleAIApp")]
    [PermissionTable(Name = "Sys_RoleAIApp")]
    public partial class Sys_RoleAIAppController : ApiBaseController<ISys_RoleAIAppService>
    {
        public Sys_RoleAIAppController(ISys_RoleAIAppService service)
        : base(service)
        {
        }
    }
}

