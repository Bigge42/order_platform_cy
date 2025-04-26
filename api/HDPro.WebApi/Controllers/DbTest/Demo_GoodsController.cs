/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹Demo_GoodsController编写
 */
using Microsoft.AspNetCore.Mvc;
using HDPro.Core.Controllers.Basic;
using HDPro.Entity.AttributeManager;
using HDPro.DbTest.IServices;
namespace HDPro.DbTest.Controllers
{
    [Route("api/Demo_Goods")]
    [PermissionTable(Name = "Demo_Goods")]
    public partial class Demo_GoodsController : ApiBaseController<IDemo_GoodsService>
    {
        public Demo_GoodsController(IDemo_GoodsService service)
        : base(service)
        {
        }
    }
}

