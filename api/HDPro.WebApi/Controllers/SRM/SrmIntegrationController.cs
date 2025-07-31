using Microsoft.AspNetCore.Mvc;
using HDPro.Core.Filters;
using HDPro.CY.Order.IServices;
using HDPro.Entity.DomainModels;
using HDPro.Core.Utilities;
using System;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using HDPro.Core.Controllers.Basic;

namespace HDPro.WebApi.Controllers.SRM
{
    /// <summary>
    /// SRM系统集成控制器
    /// 提供催单回复和协商回复接口，用于与SRM系统对接
    /// </summary>
    [Route("api/srm")]
    [ApiController]
    public class SrmIntegrationController : VolController
    {
        private readonly IOCP_UrgentOrderReplyService _urgentOrderReplyService;
        private readonly IOCP_NegotiationReplyService _negotiationReplyService;

        public SrmIntegrationController(
            IOCP_UrgentOrderReplyService urgentOrderReplyService,
            IOCP_NegotiationReplyService negotiationReplyService)
        {
            _urgentOrderReplyService = urgentOrderReplyService;
            _negotiationReplyService = negotiationReplyService;
        }

        /// <summary>
        /// 创建催单回复记录
        /// 需要在请求头中传入 X-App-Id 和 X-App-Secret
        /// 需要应用具有 "urgent:reply" 权限
        /// </summary>
        /// <param name="request">催单回复请求</param>
        /// <returns></returns>
        [HttpPost("urgent-order-reply")]
        [ThirdPartyApi]
        public async Task<IActionResult> CreateUrgentOrderReply([FromBody] CreateUrgentOrderReplyRequest request)
        {
            try
            {
                // 获取当前第三方应用信息
                var app = HttpContext.Items["ThirdPartyApp"] as Sys_ThirdPartyApp;
                
                // 数据验证
                if (request == null)
                {
                    return BadRequest(new { success = false, message = "请求数据不能为空" });
                }

                if (request.UrgentOrderID <= 0)
                {
                    return BadRequest(new { success = false, message = "催单ID不能为空" });
                }

                if (string.IsNullOrWhiteSpace(request.ReplyContent))
                {
                    return BadRequest(new { success = false, message = "回复内容不能为空" });
                }

                if (string.IsNullOrWhiteSpace(request.ReplyPersonName))
                {
                    return BadRequest(new { success = false, message = "回复人名称不能为空" });
                }

                // 创建催单回复实体
                var urgentOrderReply = new OCP_UrgentOrderReply
                {
                    UrgentOrderID = request.UrgentOrderID,
                    ReplyContent = request.ReplyContent,
                    ReplyPersonName = request.ReplyPersonName,
                    ReplyPersonPhone = request.ReplyPersonPhone,
                    ReplyTime = request.ReplyTime ?? DateTime.Now,
                    ReplyProgress = request.ReplyProgress,
                    ReplyDeliveryDate = request.ReplyDeliveryDate,
                    Remarks = request.Remarks,
                    CreateDate = DateTime.Now,
                    Creator = $"SRM-{app?.AppName}"
                };

                // 保存到数据库
                var result = await _urgentOrderReplyService.AddReplyAsync(urgentOrderReply);

                if (result.Status)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "催单回复创建成功",
                        data = new
                        {
                            replyId = urgentOrderReply.ReplyID,
                            urgentOrderId = urgentOrderReply.UrgentOrderID,
                            replyTime = urgentOrderReply.ReplyTime,
                            replyPersonName = urgentOrderReply.ReplyPersonName
                        },
                        appName = app?.AppName,
                        timestamp = DateTime.Now
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"催单回复创建失败：{result.Message}",
                        timestamp = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"服务器内部错误：{ex.Message}",
                    timestamp = DateTime.Now
                });
            }
        }

        /// <summary>
        /// 创建协商回复记录
        /// 需要在请求头中传入 X-App-Id 和 X-App-Secret
        /// 需要应用具有 "negotiation:reply" 权限
        /// </summary>
        /// <param name="request">协商回复请求</param>
        /// <returns></returns>
        [HttpPost("negotiation-reply")]
        [ThirdPartyApi]
        public async Task<IActionResult> CreateNegotiationReply([FromBody] CreateNegotiationReplyRequest request)
        {
            try
            {
                // 获取当前第三方应用信息
                var app = HttpContext.Items["ThirdPartyApp"] as Sys_ThirdPartyApp;
                
                // 数据验证
                if (request == null)
                {
                    return BadRequest(new { success = false, message = "请求数据不能为空" });
                }

                if (request.NegotiationID <= 0)
                {
                    return BadRequest(new { success = false, message = "协商ID不能为空" });
                }

                // 创建协商回复实体
                var negotiationReply = new OCP_NegotiationReply
                {
                    NegotiationID = request.NegotiationID,
                    ReplyContent = request.ReplyContent,
                    ReplyPersonName = request.ReplyPersonName,
                    ReplyPersonPhone = request.ReplyPersonPhone,
                    ReplyTime = request.ReplyTime ?? DateTime.Now,
                    ReplyProgress = request.ReplyProgress,
                    ReplyDeliveryDate = request.ReplyDeliveryDate,
                    Remarks = request.Remarks,
                    CreateDate = DateTime.Now,
                    Creator = $"SRM-{app?.AppName}"
                };

                // 保存到数据库
                var result = await _negotiationReplyService.AddReplyAsync(negotiationReply);

                if (result.Status)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "协商回复创建成功",
                        data = new
                        {
                            replyId = negotiationReply.ReplyID,
                            negotiationId = negotiationReply.NegotiationID,
                            replyTime = negotiationReply.ReplyTime,
                            replyPersonName = negotiationReply.ReplyPersonName
                        },
                        appName = app?.AppName,
                        timestamp = DateTime.Now
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"协商回复创建失败：{result.Message}",
                        timestamp = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"服务器内部错误：{ex.Message}",
                    timestamp = DateTime.Now
                });
            }
        }

        /// <summary>
        /// 获取SRM集成API使用说明
        /// </summary>
        /// <returns></returns>
        [HttpGet("usage")]
        public IActionResult GetUsage()
        {
            return Ok(new
            {
                title = "SRM系统集成API使用说明",
                version = "1.0",
                authentication = new
                {
                    method = "Header认证",
                    headers = new
                    {
                        appId = "X-App-Id: 您的应用ID",
                        appSecret = "X-App-Secret: 您的应用密钥"
                    }
                },
                apis = new object[]
                {
                    new
                    {
                        name = "创建催单回复",
                        url = "/api/srm/urgent-order-reply",
                        method = "POST",
                        permission = "urgent:reply",
                        description = "用于SRM系统创建催单回复记录",
                        requestBody = new
                        {
                            urgentOrderID = "催单ID（必填）",
                            replyContent = "回复内容（必填）",
                            replyPersonName = "回复人名称（必填）",
                            replyPersonPhone = "回复人电话（可选）",
                            replyTime = "回复时间（可选，默认当前时间）",
                            replyProgress = "回复进度（可选）",
                            replyDeliveryDate = "回复交期（可选）",
                            remarks = "备注（可选）"
                        }
                    },
                    new
                    {
                        name = "创建协商回复",
                        url = "/api/srm/negotiation-reply",
                        method = "POST",
                        permission = "negotiation:reply",
                        description = "用于SRM系统创建协商回复记录",
                        requestBody = new
                        {
                            negotiationID = "协商ID（必填）",
                            replyContent = "回复内容（可选）",
                            replyPersonName = "回复人名称（可选）",
                            replyPersonPhone = "回复人电话（可选）",
                            replyTime = "回复时间（可选，默认当前时间）",
                            replyProgress = "回复进度（可选）",
                            replyDeliveryDate = "回复交期（可选）",
                            remarks = "备注（可选）"
                        }
                    }
                },
                examples = new object[]
                {
                    new
                    {
                        name = "催单回复示例",
                        url = "/api/srm/urgent-order-reply",
                        method = "POST",
                        headers = new
                        {
                            XAppId = "CY1jOrdSRM20250606123456",
                            XAppSecret = "D4MteF8mDJRdFOL9O>ORDJH*ITAGD4RASK",
                            ContentType = "application/json"
                        },
                        body = new
                        {
                            urgentOrderID = 1001,
                            replyContent = "已安排生产，预计3天内完成",
                            replyPersonName = "张三",
                            replyPersonPhone = "13800138000",
                            replyProgress = "生产中",
                            replyDeliveryDate = "2025-06-10T10:00:00",
                            remarks = "优先处理"
                        }
                    },
                    new
                    {
                        name = "协商回复示例",
                        url = "/api/srm/negotiation-reply",
                        method = "POST",
                        headers = new
                        {
                            XAppId = "CY1jOrdSRM20250606123456",
                            XAppSecret = "D4MteF8mDJRdFOL9O>ORDJH*ITAGD4RASK",
                            ContentType = "application/json"
                        },
                        body = new
                        {
                            negotiationID = 2001,
                            replyContent = "同意价格调整方案",
                            replyPersonName = "李四",
                            replyPersonPhone = "13900139000",
                            replyProgress = "已确认",
                            remarks = "按新价格执行"
                        }
                    }
                },
                errorCodes = new object[]
                {
                    new { code = 400, message = "请求参数错误" },
                    new { code = 401, message = "AppId或AppSecret无效" },
                    new { code = 403, message = "权限不足或IP不在白名单" },
                    new { code = 500, message = "服务器内部错误" }
                },
                timestamp = DateTime.Now
            });
        }
    }

    /// <summary>
    /// 创建催单回复请求
    /// </summary>
    public class CreateUrgentOrderReplyRequest
    {
        /// <summary>
        /// 催单ID
        /// </summary>
        [Required(ErrorMessage = "催单ID不能为空")]
        public long UrgentOrderID { get; set; }

        /// <summary>
        /// 回复内容
        /// </summary>
        [Required(ErrorMessage = "回复内容不能为空")]
        [MaxLength(1000, ErrorMessage = "回复内容不能超过1000个字符")]
        public string ReplyContent { get; set; }

        /// <summary>
        /// 回复人名称
        /// </summary>
        [Required(ErrorMessage = "回复人名称不能为空")]
        [MaxLength(50, ErrorMessage = "回复人名称不能超过50个字符")]
        public string ReplyPersonName { get; set; }

        /// <summary>
        /// 回复人电话
        /// </summary>
        [MaxLength(50, ErrorMessage = "回复人电话不能超过50个字符")]
        public string ReplyPersonPhone { get; set; }

        /// <summary>
        /// 回复时间
        /// </summary>
        public DateTime? ReplyTime { get; set; }

        /// <summary>
        /// 回复进度
        /// </summary>
        [MaxLength(100, ErrorMessage = "回复进度不能超过100个字符")]
        public string ReplyProgress { get; set; }

        /// <summary>
        /// 回复交期
        /// </summary>
        public DateTime? ReplyDeliveryDate { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [MaxLength(500, ErrorMessage = "备注不能超过500个字符")]
        public string Remarks { get; set; }
    }

    /// <summary>
    /// 创建协商回复请求
    /// </summary>
    public class CreateNegotiationReplyRequest
    {
        /// <summary>
        /// 协商ID
        /// </summary>
        [Required(ErrorMessage = "协商ID不能为空")]
        public long NegotiationID { get; set; }

        /// <summary>
        /// 回复内容
        /// </summary>
        [MaxLength(1000, ErrorMessage = "回复内容不能超过1000个字符")]
        public string ReplyContent { get; set; }

        /// <summary>
        /// 回复人名称
        /// </summary>
        [MaxLength(50, ErrorMessage = "回复人名称不能超过50个字符")]
        public string ReplyPersonName { get; set; }

        /// <summary>
        /// 回复人电话
        /// </summary>
        [MaxLength(50, ErrorMessage = "回复人电话不能超过50个字符")]
        public string ReplyPersonPhone { get; set; }

        /// <summary>
        /// 回复时间
        /// </summary>
        public DateTime? ReplyTime { get; set; }

        /// <summary>
        /// 回复进度
        /// </summary>
        [MaxLength(100, ErrorMessage = "回复进度不能超过100个字符")]
        public string ReplyProgress { get; set; }

        /// <summary>
        /// 回复交期
        /// </summary>
        public DateTime? ReplyDeliveryDate { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [MaxLength(500, ErrorMessage = "备注不能超过500个字符")]
        public string Remarks { get; set; }
    }
}