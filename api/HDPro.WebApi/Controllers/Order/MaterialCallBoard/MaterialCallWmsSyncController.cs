using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HDPro.CY.Order.IServices.MaterialCallBoard;
using HDPro.Core.Filters;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HDPro.WebApi.Controllers.Order.MaterialCallBoard
{
    [Route("api/MaterialCall/WorkOrdersSet")]
    [ApiController]
    public class MaterialCallWmsSyncController : ControllerBase
    {
        private readonly IMaterialCallWmsSyncService _svc;
        private readonly ILogger<MaterialCallWmsSyncController> _logger;

        public MaterialCallWmsSyncController(
            IMaterialCallWmsSyncService svc,
            ILogger<MaterialCallWmsSyncController> logger)
        {
            _svc = svc;
            _logger = logger;
        }

        /// <summary>内部调用 WMS → 刷新白名单快照（不删除叫料看板）</summary>
        [HttpPost("wms-sync")]
        [AllowAnonymous]
        public async Task<IActionResult> SyncFromWms()
        {
            var res = await _svc.SyncSnapshotFromWmsAsync(false);
            if (!res.Status)
            {
                _logger.LogWarning("WMS 同步失败返回 400，payload={Payload}", SerializePayload(res));
                return BadRequest(res);
            }

            return Ok(res);
        }

        /// <summary>内部调用 WMS → 刷新白名单快照 → 立即按白名单删除叫料看板缺席项</summary>
        [HttpPost("wms-sync-prune")]
        [AllowAnonymous]
        public async Task<IActionResult> SyncFromWmsAndPrune()
        {
            var res = await _svc.SyncSnapshotFromWmsAsync(true);
            if (!res.Status)
            {
                _logger.LogWarning("WMS 同步+prune 失败返回 400，payload={Payload}", SerializePayload(res));
                return BadRequest(res);
            }

            return Ok(res);
        }

        private static string SerializePayload(object res)
        {
            try
            {
                return JsonSerializer.Serialize(res);
            }
            catch
            {
                return "<无法序列化>";
            }
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
