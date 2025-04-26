/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹MES_ProcessReportController编写
 */
using Microsoft.AspNetCore.Mvc;
using HDPro.Core.Controllers.Basic;
using HDPro.Entity.AttributeManager;
using HDPro.MES.IServices;
namespace HDPro.MES.Controllers
{
    [Route("api/MES_ProcessReport")]
    [PermissionTable(Name = "MES_ProcessReport")]
    public partial class MES_ProcessReportController : ApiBaseController<IMES_ProcessReportService>
    {
        public MES_ProcessReportController(IMES_ProcessReportService service)
        : base(service)
        {
        }
    }
}

