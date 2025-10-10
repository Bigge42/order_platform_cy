using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using HDPro.Core.Utilities;
using HDPro.CY.Order.IServices.WZ;

namespace HDPro.CY.Order.Controllers
{
    /// <summary>
    /// 循环仓库看板手动更新接口
    /// </summary>
    [Route("api/Xhckkb")]
    [ApiController]
    public class XhckkbController : ControllerBase
    {
        private readonly IXhckkbService _xhckkbService;
        private readonly ILogger<XhckkbController> _logger;

        public XhckkbController(IXhckkbService xhckkbService, ILogger<XhckkbController> logger)
        {
            _xhckkbService = xhckkbService;
            _logger = logger;
        }

        /// <summary>
        /// 手动触发循环仓库看板数据更新
        /// </summary>
        [HttpPost("ManualUpdate")]
        public async Task<IActionResult> ManualUpdate()
        {
            _logger.LogInformation("收到手动更新循环仓库看板数据的请求...");
            try
            {
                // 调用服务层执行更新操作
                WebResponseContent result = await _xhckkbService.ManualUpdateXhckkbAsync();
                _logger.LogInformation("循环仓库看板数据更新操作完成，Success={Success}", result.Status);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ManualUpdate 接口调用过程中出现异常");
                // 返回统一的错误响应
                return Ok(new WebResponseContent().Error($"更新过程中出现异常: {ex.Message}"));
            }
        }
    }
}
