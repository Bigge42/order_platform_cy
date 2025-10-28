using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HDPro.CY.Order.IServices.MaterialCallBoard;

namespace HDPro.WebApi.Controllers.Order.MaterialCallBoard
{
    [Route("api/MaterialCall/WorkOrdersSet")]
    [ApiController]
    public class MaterialCallWmsSyncController : ControllerBase
    {
        private readonly IMaterialCallWmsSyncService _svc;
        public MaterialCallWmsSyncController(IMaterialCallWmsSyncService svc) => _svc = svc;

        /// <summary>�ڲ����� WMS �� ˢ�°��������գ���ɾ�����Ͽ��壩</summary>
        [HttpPost("wms-sync")]
        [AllowAnonymous]
        public async Task<IActionResult> SyncFromWms()
        {
            var res = await _svc.SyncSnapshotFromWmsAsync(false);
            return res.Status ? Ok(res) : BadRequest(res);
        }

        /// <summary>�ڲ����� WMS �� ˢ�°��������� �� ������������ɾ�����Ͽ���ȱϯ��</summary>
        [HttpPost("wms-sync-prune")]
        [AllowAnonymous]
        public async Task<IActionResult> SyncFromWmsAndPrune()
        {
            var res = await _svc.SyncSnapshotFromWmsAsync(true);
            return res.Status ? Ok(res) : BadRequest(res);
        }
    }
}
