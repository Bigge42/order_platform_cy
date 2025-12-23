using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HDPro.CY.Order.IServices.MaterialCallBoard;
using HDPro.Core.Filters;

namespace HDPro.WebApi.Controllers.Order.MaterialCallBoard
{
    [Route("api/MaterialCall/WorkOrdersSet")]
    [ApiController]
    public class MaterialCallWmsSyncController : ControllerBase
    {
        private readonly IMaterialCallWmsSyncService _svc;
        public MaterialCallWmsSyncController(IMaterialCallWmsSyncService svc) => _svc = svc;

        /// <summary>内部调用 WMS → 刷新白名单快照（不删除叫料看板）</summary>
        [HttpPost("wms-sync")]
        [AllowAnonymous]
        public async Task<IActionResult> SyncFromWms()
        {
            var res = await _svc.SyncSnapshotFromWmsAsync(false);
            return res.Status ? Ok(res) : BadRequest(res);
        }

        /// <summary>内部调用 WMS → 刷新白名单快照 → 立即按白名单删除叫料看板缺席项</summary>
        [HttpPost("wms-sync-prune")]
        [AllowAnonymous]
        public async Task<IActionResult> SyncFromWmsAndPrune()
        {
            var res = await _svc.SyncSnapshotFromWmsAsync(true);
            return res.Status ? Ok(res) : BadRequest(res);
        }
        [HttpPost("wms-sync-prune_task"),ApiTask]
        [AllowAnonymous]
        public async Task<IActionResult> SyncFromWmsAndPruneTask()
        {
            var res = await _svc.SyncSnapshotFromWmsAsync(true);
            return res.Status ? Ok(res) : BadRequest(res);
        }
    }
}
