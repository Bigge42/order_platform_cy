using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using HDPro.CY.Order.IServices.MaterialCallBoard;
using HDPro.CY.Order.Models.MaterialCallBoardDtos;

namespace HDPro.WebApi.Controllers.Order
{
    /// <summary>
    /// MaterialCallBoard ������������������������������������������ͻ��
    /// ·�ɱ���������һ�£�/api/MaterialCallBoard/batch-upsert
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialCallBoardController : ControllerBase
    {
        private readonly IMaterialCallBatchService _materialCallBatchService;

        public MaterialCallBoardController(IMaterialCallBatchService materialCallBatchService)
        {
            _materialCallBatchService = materialCallBatchService;
        }

        /// <summary>
        /// ������������ MaterialCallBoard ���ݣ��޼�Ȩ��
        /// </summary>
        [HttpPost("batch-upsert")]
        [AllowAnonymous]
        public async Task<IActionResult> BatchUpsertAsync([FromBody] List<MaterialCallBoardBatchDto> payload)
        {
            if (payload == null || payload.Count == 0)
                return BadRequest(new { status = false, message = "�����岻��Ϊ��" });

            var res = await _materialCallBatchService.BatchUpsertAsync(payload);
            if (res.Status) return Ok(res);
            return BadRequest(res);
        }
    }
}
