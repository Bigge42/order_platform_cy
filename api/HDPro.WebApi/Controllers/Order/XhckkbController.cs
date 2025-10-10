using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using HDPro.Core.Utilities;
using HDPro.CY.Order.IServices.WZ;

namespace HDPro.CY.Order.Controllers
{
    /// <summary>
    /// ѭ���ֿ⿴���ֶ����½ӿ�
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
        /// �ֶ�����ѭ���ֿ⿴�����ݸ���
        /// </summary>
        [HttpPost("ManualUpdate")]
        public async Task<IActionResult> ManualUpdate()
        {
            _logger.LogInformation("�յ��ֶ�����ѭ���ֿ⿴�����ݵ�����...");
            try
            {
                // ���÷����ִ�и��²���
                WebResponseContent result = await _xhckkbService.ManualUpdateXhckkbAsync();
                _logger.LogInformation("ѭ���ֿ⿴�����ݸ��²�����ɣ�Success={Success}", result.Status);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ManualUpdate �ӿڵ��ù����г����쳣");
                // ����ͳһ�Ĵ�����Ӧ
                return Ok(new WebResponseContent().Error($"���¹����г����쳣: {ex.Message}"));
            }
        }
    }
}
