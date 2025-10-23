using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using HDPro.CY.Order.IServices.MaterialCallBoard;
using HDPro.CY.Order.Models.MaterialCallBoardDtos;

namespace HDPro.WebApi.Controllers.Order
{
    // 注意：此类必须是 partial，且命名空间、类名与主控制器完全一致
    public partial class MaterialCallBoardController
    {
        /// <summary>
        /// 批量新增/更新叫料看板
        /// </summary>
        [HttpPost("batch-upsert")]
        public async Task<IActionResult> BatchUpsert([FromBody] List<MaterialCallBoardBatchDto> rows)
        {
            if (rows == null || rows.Count == 0)
                return BadRequest("payload is empty.");

            // Service 属性来自主控制器继承的基类（ApiBaseController<TService>）
            var result = await Service.BatchUpsertAsync(rows);

            // 某些项目里 WebResponseContent 有 Status/Message；这里兼容性写法：
            // 若你的返回值没有 Status 字段，也可以直接 return Ok(result);
            if (result is null) return BadRequest("service returned null.");
            var statusProp = result.GetType().GetProperty("Status");
            if (statusProp != null && statusProp.PropertyType == typeof(bool))
            {
                bool ok = (bool)statusProp.GetValue(result)!;
                return ok ? Ok(result) : BadRequest(result);
            }
            return Ok(result);
        }
    }
}
