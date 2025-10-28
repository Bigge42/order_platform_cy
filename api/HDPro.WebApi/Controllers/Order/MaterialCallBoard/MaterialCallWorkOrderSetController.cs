using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using HDPro.CY.Order.IServices.MaterialCallBoard;
using HDPro.CY.Order.Models.MaterialCallBoardDtos;

namespace HDPro.WebApi.Controllers.Order.MaterialCallBoard
{
    [Route("api/MaterialCall/WorkOrdersSet")]
    [ApiController]
    public class MaterialCallWorkOrderSetController : ControllerBase
    {
        private readonly IMaterialCallWorkOrderSetService _svc;
        public MaterialCallWorkOrderSetController(IMaterialCallWorkOrderSetService svc) => _svc = svc;

        /// <summary>
        /// ȫ������д�루���ǰ��������ϣ������� ExistsInMCB��
        /// </summary>
        [HttpPost("snapshot")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshSnapshot([FromBody] List<MaterialCallWorkOrderCodeDto> payload)
        {
            var res = await _svc.RefreshSnapshotAsync(payload);
            return res.Status ? Ok(res) : BadRequest(res);
        }

        /// <summary>
        /// ��������ɾ�����Ͽ����С�ȱϯ���� WorkOrderNo�����������е�����û�еģ������κβ�����
        /// </summary>
        [HttpPost("prune")]
        [AllowAnonymous]
        public async Task<IActionResult> Prune()
        {
            var res = await _svc.PruneAsync();
            return res.Status ? Ok(res) : BadRequest(res);
        }
    }
}
