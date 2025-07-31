/*
 * OA集成服务控制器
 * 提供OA系统集成的API接口
 */
using HDPro.Core.Controllers.Basic;
using HDPro.Core.Utilities;
using HDPro.CY.Order.IServices.OA;
using HDPro.CY.Order.Services.OA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using HDPro.CY.Order.IServices;
using HDPro.Entity.DomainModels;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using HDPro.Core.Filters;

namespace HDPro.WebApi.Controllers.Order
{
    /// <summary>
    /// OA集成服务控制器
    /// 提供与OA系统集成的API接口
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OAController : VolController
    {
        private readonly IOAIntegrationService _oaIntegrationService;
        private readonly IOCP_NegotiationReplyService _negotiationReplyService;
        private readonly ILogger<OAController> _logger;

        public OAController(
            IOAIntegrationService oaIntegrationService,
            IOCP_NegotiationReplyService negotiationReplyService,
            ILogger<OAController> logger)
        {
            _oaIntegrationService = oaIntegrationService;
            _negotiationReplyService = negotiationReplyService;
            _logger = logger;
        }

        /// <summary>
        /// 获取OA Token
        /// </summary>
        /// <param name="loginName">登录名</param>
        /// <returns>Token信息</returns>
        [HttpPost("token")]
        public async Task<WebResponseContent> GetToken([FromBody] string loginName)
        {
            _logger.LogInformation($"正在获取OA Token，登录名: {loginName}");
            return await _oaIntegrationService.GetTokenAsync(loginName);
        }

        /// <summary>
        /// 发送股份OA消息
        /// </summary>
        /// <param name="request">消息发送请求</param>
        /// <returns>发送结果</returns>
        [HttpPost("message")]
        public async Task<WebResponseContent> SendMessage([FromBody] SendMessageRequest request)
        {
            _logger.LogInformation($"正在发送股份OA消息，发送者: {request.SenderLoginName}");

            return await _oaIntegrationService.GetShareholderTokenAndSendMessageAsync(
                request.SenderLoginName,
                request.ReceiverLoginNames,
                request.Content,
                request.Urls);
        }

        /// <summary>
        /// 发送订单消息
        /// </summary>
        /// <param name="request">订单消息请求</param>
        /// <returns>发送结果</returns>
        [HttpPost("order-message")]
        public async Task<WebResponseContent> SendOrderMessage([FromBody] SendOrderMessageRequest request)
        {
            _logger.LogInformation($"正在发送订单消息，订单号: {request.OrderNo}");

            return await _oaIntegrationService.SendOrderMessageAsync(
                request.SenderLoginName,
                request.OrderNo,
                request.OrderType,
                request.MessageType,
                request.ReceiverLoginNames,
                request.CustomContent);
        }

        /// <summary>
        /// 发送股份OA消息
        /// </summary>
        /// <param name="request">消息发送请求</param>
        /// <returns>发送结果</returns>
        [HttpPost("shareholder-message")]
        public async Task<WebResponseContent> SendShareholderMessage([FromBody] SendMessageRequest request)
        {
            _logger.LogInformation($"正在发送股份OA消息，发送者: {request.SenderLoginName}");

            return await _oaIntegrationService.GetShareholderTokenAndSendMessageAsync(
                request.SenderLoginName,
                request.ReceiverLoginNames,
                request.Content,
                request.Urls);
        }

        /// <summary>
        /// 发起OA流程
        /// </summary>
        /// <param name="request">流程发起请求</param>
        /// <returns>流程发起结果</returns>
        [HttpPost("start-process")]
        public async Task<WebResponseContent> StartProcess([FromBody] StartProcessRequest request)
        {
            _logger.LogInformation($"正在发起OA流程，发起人: {request.LoginName}");
            
            return await _oaIntegrationService.GetTokenAndStartProcessAsync(
                request.LoginName,
                request.FormData);
        }

        /// <summary>
        /// 发起订单跟踪流程
        /// </summary>
        /// <param name="request">订单跟踪流程请求</param>
        /// <returns>流程发起结果</returns>
        [HttpPost("start-order-tracking")]
        public async Task<WebResponseContent> StartOrderTracking([FromBody] StartOrderTrackingRequest request)
        {
            _logger.LogInformation($"正在发起订单跟踪流程，订单号: {request.OrderData.OrderNo}");
            
            // 将业务数据转换为OA表单数据格式
            var formData = new OAFormData
            {
                DataSource = request.OrderData.DataSource ?? "协同平台", // 数据来源：SRM/协同平台
                SupplierCode = request.OrderData.SupplierCode,
                SupplierName = request.OrderData.SupplierName,
                OrderNo = request.OrderData.OrderNo,
                SourceOrderNo = request.OrderData.SourceOrderNo,
                DeliveryStatus = request.OrderData.DeliveryStatus,
                OrderDate = request.OrderData.OrderDate?.ToString("yyyy-MM-dd"),
                OrderRemark = request.OrderData.OrderRemark,
                MaterialName = request.OrderData.MaterialName,
                MaterialCode = request.OrderData.MaterialCode,
                Specification = request.OrderData.Specification,
                DrawingNo = request.OrderData.DrawingNo,
                Material = request.OrderData.Material,
                Unit = request.OrderData.Unit,
                PurchaseQuantity = request.OrderData.PurchaseQuantity?.ToString(),
                Shortage = request.OrderData.Shortage?.ToString(),
                PlanTrackingNo = request.OrderData.PlanTrackingNo,
                FirstRequiredDeliveryDate = request.OrderData.DeliveryDate?.ToString("yyyy-MM-dd"), // 第一次要求交期（变更前交期）
                ExecutiveOrganization = request.OrderData.ExecutiveOrganization ?? "1", // 执行机构
                ChangedDeliveryDate = request.OrderData.ChangedDeliveryDate?.ToString("yyyy-MM-dd"), // 变更的交期
                SupplierExceptionReply = request.OrderData.SupplierExceptionReply
            };
            
            return await _oaIntegrationService.GetTokenAndStartProcessAsync(
                request.LoginName,
                formData);
        }

        /// <summary>
        /// 创建协商回复记录（供OA系统调用）
        /// 需要在请求头中传入 X-App-Id 和 X-App-Secret
        /// 需要应用具有 "negotiation:reply" 权限
        /// </summary>
        /// <param name="request">协商回复请求</param>
        /// <returns>创建结果</returns>
        [HttpPost("negotiation-reply")]
        [ThirdPartyApi]
        public async Task<IActionResult> CreateNegotiationReply([FromBody] CreateOANegotiationReplyRequest request)
        {
            try
            {
                // 获取当前第三方应用信息
                var app = HttpContext.Items["ThirdPartyApp"] as Sys_ThirdPartyApp;
                
                _logger.LogInformation($"OA系统创建协商回复，协商ID: {request.NegotiationID}，应用: {app?.AppName}");
                
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
                    NegotiationStatus = request.NegotiationStatus, // 协商状态
                    Remarks = request.Remarks,
                    CreateDate = DateTime.Now,
                    Creator = $"OA-{app?.AppName}"
                };

                // 保存到数据库
                var result = await _negotiationReplyService.AddReplyAsync(negotiationReply);

                if (result.Status)
                {
                    _logger.LogInformation($"OA协商回复创建成功，回复ID: {negotiationReply.ReplyID}");
                    
                    return Ok(new
                    {
                        success = true,
                        message = "协商回复创建成功",
                        data = new
                        {
                            replyId = negotiationReply.ReplyID,
                            negotiationId = negotiationReply.NegotiationID,
                            replyTime = negotiationReply.ReplyTime,
                            replyPersonName = negotiationReply.ReplyPersonName,
                            negotiationStatus = negotiationReply.NegotiationStatus
                        },
                        appName = app?.AppName,
                        timestamp = DateTime.Now
                    });
                }
                else
                {
                    _logger.LogError($"OA协商回复创建失败: {result.Message}");
                    
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
                _logger.LogError(ex, $"OA协商回复创建时发生异常，协商ID: {request?.NegotiationID}");
                
                return StatusCode(500, new
                {
                    success = false,
                    message = $"服务器内部错误：{ex.Message}",
                    timestamp = DateTime.Now
                });
            }
        }

        /// <summary>
        /// 根据人员工号获取股份OA人员信息
        /// </summary>
        /// <param name="employeeCode">人员工号</param>
        /// <param name="token">认证Token（可选）</param>
        /// <returns>人员信息</returns>
        [HttpGet("shareholder-user/{employeeCode}")]
        public async Task<WebResponseContent> GetShareholderUserByCode(
            [FromRoute] string employeeCode,
            [FromQuery] string token = null)
        {
            _logger.LogInformation($"正在获取股份OA人员信息，工号: {employeeCode}");

            if (string.IsNullOrEmpty(employeeCode))
            {
                return WebResponseContent.Instance.Error("人员工号不能为空");
            }

            return await _oaIntegrationService.GetShareholderOAUserByCodeAsync(employeeCode, token);
        }

        /// <summary>
        /// 批量获取股份OA人员信息
        /// </summary>
        /// <param name="request">批量查询请求</param>
        /// <returns>人员信息列表</returns>
        [HttpPost("shareholder-users")]
        public async Task<WebResponseContent> GetShareholderUsersBatch([FromBody] BatchGetUsersRequest request)
        {
            _logger.LogInformation($"正在批量获取股份OA人员信息，工号数量: {request.EmployeeCodes?.Count ?? 0}");

            if (request == null || request.EmployeeCodes == null || !request.EmployeeCodes.Any())
            {
                return WebResponseContent.Instance.Error("人员工号列表不能为空");
            }

            var results = new List<object>();
            var successCount = 0;
            var failCount = 0;

            foreach (var employeeCode in request.EmployeeCodes)
            {
                try
                {
                    var result = await _oaIntegrationService.GetShareholderOAUserByCodeAsync(employeeCode, request.Token);
                    
                    if (result.Status)
                    {
                        successCount++;
                        results.Add(new
                        {
                            EmployeeCode = employeeCode,
                            Success = true,
                            Data = result.Data,
                            Message = result.Message
                        });
                    }
                    else
                    {
                        failCount++;
                        results.Add(new
                        {
                            EmployeeCode = employeeCode,
                            Success = false,
                            Data = (object)null,
                            Message = result.Message
                        });
                    }
                }
                catch (Exception ex)
                {
                    failCount++;
                    results.Add(new
                    {
                        EmployeeCode = employeeCode,
                        Success = false,
                        Data = (object)null,
                        Message = $"查询异常: {ex.Message}"
                    });
                }

                // 避免请求过于频繁
                await Task.Delay(50);
            }

            var response = new WebResponseContent();
            response.Status = true;
            response.Message = $"批量查询完成，成功: {successCount}，失败: {failCount}";
            response.Data = new
            {
                TotalCount = request.EmployeeCodes.Count,
                SuccessCount = successCount,
                FailCount = failCount,
                Results = results
            };
            return response;
        }

        /// <summary>
        /// 获取OA集成API使用说明
        /// </summary>
        /// <returns></returns>
        [HttpGet("usage")]
        public IActionResult GetUsage()
        {
            return Ok(new
            {
                title = "OA系统集成API使用说明",
                version = "1.0",
                authentication = new
                {
                    method = "Header认证",
                    headers = new
                    {
                        appId = "X-App-Id: OAASKBOX20250618459083",
                        appSecret = "X-App-Secret: vRlzWSI9OM>n7U1xbyNIDjRMvdYszwQT"
                    },
                    note = "协商回复接口需要第三方应用认证"
                },
                apis = new object[]
                {
                    new
                    {
                        name = "获取OA Token",
                        url = "/api/oa/token",
                        method = "POST",
                        description = "获取OA系统访问令牌",
                        requestBody = "string - 登录名"
                    },
                    new
                    {
                        name = "发送OA消息",
                        url = "/api/oa/message",
                        method = "POST",
                        description = "向OA系统发送消息"
                    },
                    new
                    {
                        name = "发送订单消息",
                        url = "/api/oa/order-message",
                        method = "POST",
                        description = "发送订单相关消息到OA系统"
                    },
                    new
                    {
                        name = "发送股份OA消息",
                        url = "/api/oa/shareholder-message",
                        method = "POST",
                        description = "向股份OA系统发送消息"
                    },
                    new
                    {
                        name = "发起OA流程",
                        url = "/api/oa/start-process",
                        method = "POST",
                        description = "在OA系统中发起流程"
                    },
                    new
                    {
                        name = "发起订单跟踪流程",
                        url = "/api/oa/start-order-tracking",
                        method = "POST",
                        description = "发起订单跟踪流程"
                    },
                    new
                    {
                        name = "根据工号获取股份OA人员信息",
                        url = "/api/oa/shareholder-user/{employeeCode}",
                        method = "GET",
                        description = "根据人员工号获取股份OA人员详细信息"
                    },
                    new
                    {
                        name = "批量获取股份OA人员信息",
                        url = "/api/oa/shareholder-users",
                        method = "POST",
                        description = "批量根据人员工号获取股份OA人员信息"
                    },
                    new
                    {
                        name = "创建协商回复",
                        url = "/api/oa/negotiation-reply",
                        method = "POST",
                        description = "创建协商回复记录（包含协商状态）",
                        authentication = "需要Header认证",
                        permission = "negotiation:reply",
                        requestBody = new
                        {
                            negotiationID = "协商ID（必填）",
                            replyContent = "回复内容（可选）",
                            replyPersonName = "回复人名称（可选）",
                            replyPersonPhone = "回复人电话（可选）",
                            replyTime = "回复时间（可选，默认当前时间）",
                            replyProgress = "回复进度（可选）",
                            replyDeliveryDate = "回复交期（可选）",
                            negotiationStatus = "协商状态（可选）",
                            remarks = "备注（可选）"
                        }
                    }
                },
                examples = new object[]
                {
                    new
                    {
                        name = "协商回复示例",
                        url = "/api/oa/negotiation-reply",
                        method = "POST",
                        headers = new
                        {
                            XAppId = "OAASKBOX20250618459083",
                            XAppSecret = "vRlzWSI9OM>n7U1xbyNIDjRMvdYszwQT",
                            ContentType = "application/json"
                        },
                        body = new
                        {
                            negotiationID = 2001,
                            replyContent = "已审核通过价格调整方案",
                            replyPersonName = "OA审核员",
                            replyPersonPhone = "13800138000",
                            replyProgress = "已审核",
                            negotiationStatus = "已通过",
                            remarks = "流程审核完成"
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
    /// 发送消息请求模型
    /// </summary>
    public class SendMessageRequest
    {
        /// <summary>
        /// 发送者登录名
        /// </summary>
        public string SenderLoginName { get; set; }

        /// <summary>
        /// 接收者登录名列表
        /// </summary>
        public List<string> ReceiverLoginNames { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// URL列表（可选）
        /// </summary>
        public List<string> Urls { get; set; }
    }

    /// <summary>
    /// 发送订单消息请求模型
    /// </summary>
    public class SendOrderMessageRequest
    {
        /// <summary>
        /// 发送者登录名
        /// </summary>
        public string SenderLoginName { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 订单类型
        /// </summary>
        public string OrderType { get; set; }

        /// <summary>
        /// 消息类型
        /// </summary>
        public string MessageType { get; set; }

        /// <summary>
        /// 接收者登录名列表
        /// </summary>
        public List<string> ReceiverLoginNames { get; set; }

        /// <summary>
        /// 自定义内容（可选）
        /// </summary>
        public string CustomContent { get; set; }
    }

    /// <summary>
    /// 发起流程请求模型
    /// </summary>
    public class StartProcessRequest
    {
        /// <summary>
        /// 发起人登录名
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// 表单数据
        /// </summary>
        public OAFormData FormData { get; set; }
    }

    /// <summary>
    /// 发起订单跟踪流程请求模型
    /// </summary>
    public class StartOrderTrackingRequest
    {
        /// <summary>
        /// 发起人登录名
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// 订单数据
        /// </summary>
        public OrderTrackingData OrderData { get; set; }
    }

    /// <summary>
    /// 创建OA协商回复请求模型
    /// </summary>
    public class CreateOANegotiationReplyRequest
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
        /// 协商状态
        /// </summary>
        [MaxLength(50, ErrorMessage = "协商状态不能超过50个字符")]
        public string NegotiationStatus { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [MaxLength(500, ErrorMessage = "备注不能超过500个字符")]
        public string Remarks { get; set; }
    }

    /// <summary>
    /// 批量获取用户信息请求模型
    /// </summary>
    public class BatchGetUsersRequest
    {
        /// <summary>
        /// 人员工号列表
        /// </summary>
        public List<string> EmployeeCodes { get; set; }

        /// <summary>
        /// 认证Token（可选）
        /// </summary>
        public string Token { get; set; }
    }
} 