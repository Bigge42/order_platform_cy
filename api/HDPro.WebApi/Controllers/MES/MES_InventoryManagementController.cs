/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹MES_InventoryManagementController编写
 */
using Microsoft.AspNetCore.Mvc;
using HDPro.Core.Controllers.Basic;
using HDPro.Entity.AttributeManager;
using HDPro.MES.IServices;
namespace HDPro.MES.Controllers
{
    [Route("api/MES_InventoryManagement")]
    [PermissionTable(Name = "MES_InventoryManagement")]
    public partial class MES_InventoryManagementController : ApiBaseController<IMES_InventoryManagementService>
    {
        public MES_InventoryManagementController(IMES_InventoryManagementService service)
        : base(service)
        {
        }
    }
}

