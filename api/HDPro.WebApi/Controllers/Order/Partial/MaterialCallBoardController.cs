using System.Collections.Generic;
using System.Threading.Tasks;
using HDPro.Core.Filters;
using HDPro.CY.Order.IServices;
using HDPro.CY.Order.Services.MaterialCallBoardModels;
using Microsoft.AspNetCore.Mvc;

namespace HDPro.WebApi.Controllers.Order
{
    /// <summary>
    /// MaterialCallBoard 业务扩展接口
    /// </summary>
    public partial class MaterialCallBoardController
    {
        private readonly IMaterialCallBoardService _service;

        public MaterialCallBoardController(IMaterialCallBoardService service)
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

        /// <summary>
        /// 批量新增或更新 MaterialCallBoard 数据
        /// </summary>
        /// <param name="payload">外部系统传入的数据集合</param>
        /// <returns>处理结果</returns>
        [HttpPost("batch-upload")]
        public async Task<IActionResult> BatchUploadAsync([FromBody] List<MaterialCallBoardBatchDto> payload)
        {
            if (payload == null)
            {
                return Json(WebResponseContent.Instance.Error("请求数据不能为空"));
            }

            var result = await _service.BatchUpsertAsync(payload);
            return Json(result);
        }
    }
}
