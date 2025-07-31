/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("OCP_UrgentOrderReply",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.Entity.DomainModels;
using HDPro.CY.Order.IServices;
using HDPro.Core.Filters;

namespace HDPro.CY.Order.Controllers
{
    public partial class OCP_UrgentOrderReplyController
    {
        private readonly IOCP_UrgentOrderReplyService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public OCP_UrgentOrderReplyController(
            IOCP_UrgentOrderReplyService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 添加催单回复记录
        /// </summary>
        /// <param name="request">催单回复请求数据</param>
        /// <returns>操作结果</returns>
        [HttpPost("AddReply")]
        public async Task<IActionResult> AddReplyAsync([FromBody] AddUrgentOrderReplyRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new HDPro.Core.Utilities.WebResponseContent().Error("请求数据不能为空"));
                }

                // 创建催单回复实体
                var urgentOrderReply = new OCP_UrgentOrderReply
                {
                    UrgentOrderID = request.UrgentOrderID,
                    ReplyContent = request.ReplyContent,
                    ReplyPersonName = request.ReplyPersonName,
                    ReplyPersonPhone = request.ReplyPersonPhone,
                    ReplyTime = request.ReplyTime,
                    ReplyProgress = request.ReplyProgress,
                    ReplyDeliveryDate = request.ReplyDeliveryDate,
                    Remarks = request.Remarks
                };

                // 调用服务方法
                var result = await _service.AddReplyAsync(urgentOrderReply);
                
                if (result.Status)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new HDPro.Core.Utilities.WebResponseContent().Error($"添加催单回复失败: {ex.Message}"));
            }
        }
    }

    /// <summary>
    /// 添加催单回复请求模型
    /// </summary>
    public class AddUrgentOrderReplyRequest
    {
        /// <summary>
        /// 催单ID
        /// </summary>
        public long UrgentOrderID { get; set; }

        /// <summary>
        /// 回复内容
        /// </summary>
        public string ReplyContent { get; set; }

        /// <summary>
        /// 回复人名称
        /// </summary>
        public string ReplyPersonName { get; set; }

        /// <summary>
        /// 回复人电话
        /// </summary>
        public string ReplyPersonPhone { get; set; }

        /// <summary>
        /// 回复时间
        /// </summary>
        public DateTime? ReplyTime { get; set; }

        /// <summary>
        /// 回复进度
        /// </summary>
        public string ReplyProgress { get; set; }

        /// <summary>
        /// 回复交期
        /// </summary>
        public DateTime? ReplyDeliveryDate { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }
    }
}
