/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹Sys_LanguageController编写
 */
using Microsoft.AspNetCore.Mvc;
using HDPro.Core.Controllers.Basic;
using HDPro.Entity.AttributeManager;
using HDPro.Sys.IServices;
namespace HDPro.Sys.Controllers
{
    [Route("api/Sys_Language")]
    [PermissionTable(Name = "Sys_Language")]
    public partial class Sys_LanguageController : ApiBaseController<ISys_LanguageService>
    {
        public Sys_LanguageController(ISys_LanguageService service)
        : base(service)
        {
        }
    }
}

