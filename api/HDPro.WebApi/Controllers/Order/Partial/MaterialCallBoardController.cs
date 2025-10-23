/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("MaterialCallBoard",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.Entity.DomainModels;
using HDPro.Entity.DomainModels.MaterialCallBoard.dto;
using HDPro.Core.Utilities;
using HDPro.CY.Order.IServices;

namespace HDPro.CY.Order.Controllers
{
    public partial class MaterialCallBoardController
    {
        private readonly IMaterialCallBoardService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public MaterialCallBoardController(
            IMaterialCallBoardService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
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
