/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹MES_ProductInboundController编写
 */
using Microsoft.AspNetCore.Mvc;
using HDPro.Core.Controllers.Basic;
using HDPro.Entity.AttributeManager;
using HDPro.MES.IServices;
namespace HDPro.MES.Controllers
{
    [Route("api/MES_ProductInbound")]
    [PermissionTable(Name = "MES_ProductInbound")]
    public partial class MES_ProductInboundController : ApiBaseController<IMES_ProductInboundService>
    {
        public MES_ProductInboundController(IMES_ProductInboundService service)
        : base(service)
        {
        }
    }
}

