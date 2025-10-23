using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using HDPro.CY.Order.IServices.MaterialCallBoard;
using HDPro.CY.Order.Models.MaterialCallBoardDtos;

namespace HDPro.WebApi.Controllers.Order
{
    /// <summary>
    /// MaterialCallBoard 批量导入控制器（独立命名，避免与代码生成器冲突）
    /// 路由保持与需求一致：/api/MaterialCallBoard/batch-upsert
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
        /// 批量插入或更新 MaterialCallBoard 数据（无鉴权）
        /// </summary>
        [HttpPost("batch-upsert")]
        [AllowAnonymous]
        public async Task<IActionResult> BatchUpsertAsync([FromBody] List<MaterialCallBoardBatchDto> payload)
        {
            if (payload == null || payload.Count == 0)
                return BadRequest(new { status = false, message = "请求体不能为空" });

            var res = await _materialCallBatchService.BatchUpsertAsync(payload);
            if (res.Status) return Ok(res);
            return BadRequest(res);
        }
    }
}
