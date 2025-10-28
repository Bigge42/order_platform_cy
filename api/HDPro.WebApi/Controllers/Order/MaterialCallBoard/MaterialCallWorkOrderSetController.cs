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
        /// 全量快照写入（覆盖白名单集合，并计算 ExistsInMCB）
        /// </summary>
        [HttpPost("snapshot")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshSnapshot([FromBody] List<MaterialCallWorkOrderCodeDto> payload)
        {
            var res = await _svc.RefreshSnapshotAsync(payload);
            return res.Status ? Ok(res) : BadRequest(res);
        }

        /// <summary>
        /// 按白名单删除叫料看板中“缺席”的 WorkOrderNo（白名单里有但看板没有的，不做任何操作）
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
