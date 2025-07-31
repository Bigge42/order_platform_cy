/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹OCP_CurrentProcessBatchInfoController编写
 */
using Microsoft.AspNetCore.Mvc;
using HDPro.Core.Controllers.Basic;
using HDPro.Entity.AttributeManager;
using HDPro.CY.Order.IServices;
namespace HDPro.CY.Order.Controllers
{
    [Route("api/OCP_CurrentProcessBatchInfo")]
    [PermissionTable(Name = "OCP_CurrentProcessBatchInfo")]
    public partial class OCP_CurrentProcessBatchInfoController : ApiBaseController<IOCP_CurrentProcessBatchInfoService>
    {
        public OCP_CurrentProcessBatchInfoController(IOCP_CurrentProcessBatchInfoService service)
        : base(service)
        {
        }
    }
}

