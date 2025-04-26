using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using HDPro.Core.Controllers.Basic;
using HDPro.Core.Enums;
using HDPro.Core.Filters;
using HDPro.Entity.AttributeManager;
using HDPro.Entity.DomainModels;
using HDPro.Sys.IServices;

namespace HDPro.Sys.Controllers
{
    [Route("api/Sys_Role")]
    [PermissionTable(Name = "Sys_Role")]
    public partial class Sys_RoleController : ApiBaseController<ISys_RoleService>
    {
        public Sys_RoleController(ISys_RoleService service)
        : base("System", "System", "Sys_Role", service)
        {

        }
    }
}


