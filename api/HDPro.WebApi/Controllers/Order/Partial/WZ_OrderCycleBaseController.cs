/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("WZ_OrderCycleBase",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HDPro.CY.Order.IServices;
using HDPro.Core.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HDPro.CY.Order.Controllers
{
    public partial class WZ_OrderCycleBaseController
    {
        /// <summary>
        /// 从订单跟踪表同步数据到订单周期基础表
        /// </summary>
        /// <returns>同步的记录数量</returns>
        [HttpPost("sync-from-order-tracking")]
        [AllowAnonymous]
        public async Task<IActionResult> SyncFromOrderTracking()
        {
            try
            {
                var result = await Service.SyncFromOrderTrackingAsync();
                return JsonNormal(result);
            }
            catch (Exception ex)
            {
                return JsonNormal(new WebResponseContent().Error($"同步失败：{ex.Message}"));
            }
        }

        /// <summary>
        /// 批量调用阀门规则服务，回填订单周期信息
        /// </summary>
        /// <returns>回填批量结果摘要</returns>
        [HttpPost("batch-call-valve-rule-service")]
        [AllowAnonymous]
        public async Task<IActionResult> BatchCallValveRuleService()
        {
            try
            {
                var result = await Service.BatchCallValveRuleServiceAsync();
                return JsonNormal(result);
            }
            catch (Exception ex)
            {
                return JsonNormal(new WebResponseContent().Error($"规则服务调用失败：{ex.Message}"));
            }
        }

        /// <summary>
        /// 导入Excel并覆盖现有数据
        /// </summary>
        /// <param name="file">Excel文件</param>
        /// <returns>导入结果</returns>
        [HttpPost("Import")]
        [AllowAnonymous]
        [OverrideActionFilters]
        [ApiExplorerSettings(IgnoreApi = false)]
        public override ActionResult Import([FromForm] List<IFormFile> fileInput)
        {
            try
            {
                var file = fileInput?.FirstOrDefault();
                var result = Service.ImportAsync(file).GetAwaiter().GetResult();
                return JsonNormal(result);
            }
            catch (Exception ex)
            {
                return JsonNormal(new WebResponseContent().Error($"导入失败：{ex.Message}"));
            }
        }
    }
}
