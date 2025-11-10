/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹vw_OCP_Tech_BOM_Status_MonthlyController编写
 */
using Microsoft.AspNetCore.Mvc;
using HDPro.Core.Controllers.Basic;
using HDPro.Entity.AttributeManager;
using HDPro.CY.Order.IServices;
namespace HDPro.CY.Order.Controllers
{
    [Route("api/vw_OCP_Tech_BOM_Status_Monthly")]
    [PermissionTable(Name = "vw_OCP_Tech_BOM_Status_Monthly")]
    public partial class vw_OCP_Tech_BOM_Status_MonthlyController : ApiBaseController<Ivw_OCP_Tech_BOM_Status_MonthlyService>
    {
        public vw_OCP_Tech_BOM_Status_MonthlyController(Ivw_OCP_Tech_BOM_Status_MonthlyService service)
        : base(service)
        {
        }
    }
}

