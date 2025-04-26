using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HDPro.Core.Controllers.Basic;
using HDPro.Core.Enums;
using HDPro.Core.Filters;
using HDPro.Entity.DomainModels;
using HDPro.Sys.IServices;

namespace HDPro.Sys.Controllers
{
    [Route("api/menu")]
    [ApiController, JWTAuthorize()]
    public partial class Sys_MenuController : ApiBaseController<ISys_MenuService>
    {
        private ISys_MenuService _service { get; set; }
        public Sys_MenuController(ISys_MenuService service) :
            base("System", "System", "Sys_Menu", service)
        {
            _service = service;
        } 
    }
}
