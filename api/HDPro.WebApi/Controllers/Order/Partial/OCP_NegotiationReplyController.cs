/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("OCP_NegotiationReply",Enums.ActionPermissionOptions.Search)]
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
    public partial class OCP_NegotiationReplyController
    {
        private readonly IOCP_NegotiationReplyService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public OCP_NegotiationReplyController(
            IOCP_NegotiationReplyService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 添加协商回复记录
        /// </summary>
        /// <param name="request">协商回复请求数据</param>
        /// <returns>操作结果</returns>
        [HttpPost("AddReply")]
        public async Task<IActionResult> AddReplyAsync([FromBody] AddNegotiationReplyRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new HDPro.Core.Utilities.WebResponseContent().Error("请求数据不能为空"));
                }

                // 创建协商回复实体
                var negotiationReply = new OCP_NegotiationReply
                {
                    NegotiationID = request.NegotiationID,
                    ReplyContent = request.ReplyContent,
                    ReplyPersonName = request.ReplyPersonName,
                    ReplyPersonPhone = request.ReplyPersonPhone,
                    ReplyTime = request.ReplyTime,
                    ReplyProgress = request.ReplyProgress,
                    ReplyDeliveryDate = request.ReplyDeliveryDate,
                    NegotiationStatus = request.NegotiationStatus,
                    Remarks = request.Remarks
                };

                // 调用服务方法
                var result = await _service.AddReplyAsync(negotiationReply);
                
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
                return BadRequest(new HDPro.Core.Utilities.WebResponseContent().Error($"添加协商回复失败: {ex.Message}"));
            }
        }
    }

    /// <summary>
    /// 添加协商回复请求模型
    /// </summary>
    public class AddNegotiationReplyRequest
    {
        /// <summary>
        /// 协商ID
        /// </summary>
        public long NegotiationID { get; set; }

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
        /// 协商状态
        /// </summary>
        public string NegotiationStatus { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }
    }
}
