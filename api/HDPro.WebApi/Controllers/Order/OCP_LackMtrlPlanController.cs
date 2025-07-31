/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹OCP_LackMtrlPlanController编写
 */
using Microsoft.AspNetCore.Mvc;
using HDPro.Core.Controllers.Basic;
using HDPro.Entity.AttributeManager;
using HDPro.CY.Order.IServices;
namespace HDPro.CY.Order.Controllers
{
    [Route("api/OCP_LackMtrlPlan")]
    [PermissionTable(Name = "OCP_LackMtrlPlan")]
    public partial class OCP_LackMtrlPlanController : ApiBaseController<IOCP_LackMtrlPlanService>
    {
        public OCP_LackMtrlPlanController(IOCP_LackMtrlPlanService service)
        : base(service)
        {
        }
    }
}

