using System.Collections.Generic;
using System.Threading.Tasks;
using HDPro.Core.Filters;
using HDPro.CY.Order.IServices;
using HDPro.CY.Order.Models.MaterialCallBoardDtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace HDPro.CY.Order.Controllers
{
    /// <summary>
    /// MaterialCallBoard 业务扩展接口
    /// </summary>
    public partial class MaterialCallBoardController
    {
        private readonly IMaterialCallBoardService _service;

        [ActivatorUtilitiesConstructor]
        public MaterialCallBoardController(
            IMaterialCallBoardService service)
            : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// 批量上传叫料看板数据
        /// </summary>
        /// <param name="payload">待上传数据</param>
        [HttpPost("batch-upload")]
        [ApiActionPermission()]
        public async Task<IActionResult> BatchUploadAsync([FromBody] List<MaterialCallBoardBatchDto> payload)
        {
            var result = await _service.BatchUpsertAsync(payload);
            if (!result.Status)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
