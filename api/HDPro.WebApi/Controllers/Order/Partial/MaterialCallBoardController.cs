/*
 *接口编写处...
 *如果接口需要做Action的权限验证，请在Action上使用属性
 *如: [ApiActionPermission("MaterialCallBoard",Enums.ActionPermissionOptions.Search)]
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HDPro.CY.Order.IServices.MaterialCallBoard;
using HDPro.CY.Order.Models.MaterialCallBoardDtos;
using HDPro.Core.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace HDPro.CY.Order.Controllers
{
    public partial class MaterialCallBoardController
    {
        private readonly IMaterialCallBoardService _service;
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public MaterialCallBoardController(
            IMaterialCallBoardService service,
            IHttpContextAccessor httpContextAccessor)
            : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 批量插入或更新 MaterialCallBoard 数据
        /// </summary>
        /// <param name="payload">批量数据</param>
        /// <returns>操作结果</returns>
        [HttpPost("batch-upsert")]
        public async Task<IActionResult> BatchUpsertAsync([FromBody] List<MaterialCallBoardBatchDto> payload)
        {
            try
            {
                var result = await _service.BatchUpsertAsync(payload);
                if (result.Status)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(WebResponseContent.Instance.Error($"批量导入失败: {ex.Message}"));
            }
        }
    }
}
