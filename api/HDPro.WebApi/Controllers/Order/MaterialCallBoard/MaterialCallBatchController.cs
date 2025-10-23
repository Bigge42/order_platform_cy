using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using HDPro.CY.Order.IServices.MaterialCallBoard;     // ← 确保是这个
using HDPro.CY.Order.Models.MaterialCallBoardDtos;    // ← 确保是这个

namespace HDPro.WebApi.Controllers.Order.MaterialCallBoard
{
    [Route("api/MaterialCallBoard")]
    [ApiController]
    public class MaterialCallBatchController : ControllerBase
    {
        private readonly IMaterialCallBatchService _svc;
        public MaterialCallBatchController(IMaterialCallBatchService svc) => _svc = svc;

        [HttpPost("batch-upsert")]
        [AllowAnonymous]
        public async Task<IActionResult> BatchUpsertAsync([FromBody] List<MaterialCallBoardBatchDto> payload)
        {
            if (payload == null || payload.Count == 0)
                return BadRequest(new { status = false, message = "请求体不能为空" });

            var res = await _svc.BatchUpsertAsync(payload);
            return res.Status ? Ok(res) : BadRequest(res);
        }
    }
}
