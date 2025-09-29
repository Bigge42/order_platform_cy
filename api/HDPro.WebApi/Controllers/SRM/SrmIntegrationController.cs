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
using HDPro.MES.IServices;

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
        private readonly IMES_SpecialPrintRequestService _SpecialPrintRequestService;
        public SrmIntegrationController(
            IOCP_UrgentOrderReplyService urgentOrderReplyService,
            IOCP_NegotiationReplyService negotiationReplyService,
            IMES_SpecialPrintRequestService specialPrintRequestService
            )
        {
            _urgentOrderReplyService = urgentOrderReplyService;
            _negotiationReplyService = negotiationReplyService;
            _SpecialPrintRequestService = specialPrintRequestService;
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

        /// <summary>
        /// 创建自定义打印资料记录
        /// 需要在请求头中传入 X-App-Id 和 X-App-Secret
        /// 需要应用具有 "negotiation:reply" 权限
        /// </summary>
        /// <param name="request">协商回复请求</param>
        /// <returns></returns>
        [HttpPost("TestGetUDFinfo")]
        [ThirdPartyApi]
        public async Task<IActionResult> TestGetUDFinfo([FromBody] TestRequestClass request)
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

                if (string.IsNullOrWhiteSpace(request.RetrospectCode))
                {
                    return BadRequest(new { success = false, message = "产品编码不能为空" });
                }

                // 创建自定义打印资料实体
                var requestEntity = new MES_SpecialPrintRequest
                {
                    RetrospectCode = request.RetrospectCode,
                    ProductModel = request.ProductModel,
                    NominalDiameter = request.NominalDiameter,
                    NominalPressure = request.NominalPressure,
                    ValveBodyMaterial = request.ValveBodyMaterial,
                    ActuatorModel = request.ActuatorModel,
                    FailPosition = request.FailPosition,
                    AirSupplyPressure = request.AirSupplyPressure,
                    OperatingTemperature = request.OperatingTemperature,
                    RatedStroke = request.RatedStroke,
                    FlowCharacteristic = request.FlowCharacteristic,
                    FlowCoefficient = request.FlowCoefficient,
                    UDF_Key1 = request.UDF_Key1,
                    UDF_Value1 = request.UDF_Value1,
                    UDF_Key2 = request.UDF_Key2,
                    UDF_Value2 = request.UDF_Value2,
                    UDF_Key3 = request.UDF_Key3,
                    UDF_Value3 = request.UDF_Value3,
                    UDF_Key4 = request.UDF_Key4,
                    UDF_Value4 = request.UDF_Value4,
                    UDF_Key5 = request.UDF_Key5,
                    UDF_Value5 = request.UDF_Value5,
                    UDF_Key6 = request.UDF_Key6,
                    UDF_Value6 = request.UDF_Value6,
                    UDF_Key7 = request.UDF_Key7,
                    UDF_Value7 = request.UDF_Value7,
                    UDF_Key8 = request.UDF_Key8,
                    UDF_Value8 = request.UDF_Value8,
                    UDF_Key9 = request.UDF_Key9,
                    UDF_Value9 = request.UDF_Value9,
                    UDF_Key10 = request.UDF_Key10,
                    UDF_Value10 = request.UDF_Value10,
                    UDF_Key11 = request.UDF_Key11,
                    UDF_Value11 = request.UDF_Value11,
                    UDF_Key12 = request.UDF_Key12,
                    UDF_Value12 = request.UDF_Value12,
                    UDF_Key13 = request.UDF_Key13,
                    UDF_Value13 = request.UDF_Value13,
                    UDF_Key14 = request.UDF_Key14,
                    UDF_Value14 = request.UDF_Value14,
                    UDF_Key15 = request.UDF_Key15,
                    UDF_Value15 = request.UDF_Value15,

                    CreateDate = DateTime.Now,
                };

                // 保存到数据库
                var result = await _SpecialPrintRequestService.AddReplyAsync(requestEntity);

                if (result.Status)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "特殊打印资料创建成功",
                        data = new
                        {
                            RetrospectCode = request.RetrospectCode
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
                        message = $"特殊打印资料创建失败：{result.Message}",
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


    /// <summary>
    /// 创建特殊打印推送请求
    /// </summary>
    public class TestRequestClass
    {
        /// <summary>
        /// 产品编码
        /// </summary>
        [Required(ErrorMessage ="RetrospectCode字段不能为空")]
        public string RetrospectCode { get; set; }

        /// <summary>
        ///产品型号
        /// </summary>
        public string ProductModel { get; set; }

        /// <summary>
        ///公称通径
        /// </summary>
        public string NominalDiameter { get; set; }

        /// <summary>
        ///公称压力
        /// </summary>
        public string NominalPressure { get; set; }

        /// <summary>
        ///阀体材质
        /// </summary>
        public string ValveBodyMaterial { get; set; }

        /// <summary>
        ///执行机构
        /// </summary>
        public string ActuatorModel { get; set; }

        /// <summary>
        ///故障位
        /// </summary>
        public string FailPosition { get; set; }

        /// <summary>
        ///气源压力
        /// </summary>
        public string AirSupplyPressure { get; set; }

        /// <summary>
        ///工作温度
        /// </summary>
        public string OperatingTemperature { get; set; }

        /// <summary>
        ///额定行程
        /// </summary>
        public string RatedStroke { get; set; }

        /// <summary>
        ///流量特性
        /// </summary>
        public string FlowCharacteristic { get; set; }

        /// <summary>
        ///流量系数(CV值)
        /// </summary>
        public string FlowCoefficient { get; set; }

        /// <summary>
        ///自定义字段1
        /// </summary>
        public string UDF_Key1 { get; set; }

        /// <summary>
        ///自定义值1
        /// </summary>
        public string UDF_Value1 { get; set; }

        /// <summary>
        ///自定义字段2
        public string UDF_Key2 { get; set; }

        /// <summary>
        ///自定义值2
        /// </summary>
        public string UDF_Value2 { get; set; }

        /// <summary>
        ///自定义字段3
        /// </summary>
        public string UDF_Key3 { get; set; }

        /// <summary>
        ///自定义值3
        /// </summary>
        public string UDF_Value3 { get; set; }

        /// <summary>
        ///自定义字段4
        /// </summary>
        public string UDF_Key4 { get; set; }

        /// <summary>
        ///自定义值4
        /// </summary>
        public string UDF_Value4 { get; set; }

        /// <summary>
        ///自定义字段5
        /// </summary>
        public string UDF_Key5 { get; set; }

        /// <summary>
        ///自定义值5
        /// </summary>
        public string UDF_Value5 { get; set; }

        /// <summary>
        ///自定义字段6
        /// </summary>
        public string UDF_Key6 { get; set; }

        /// <summary>
        ///自定义值6
        /// </summary>
        public string UDF_Value6 { get; set; }

        /// <summary>
        ///自定义字段7
        /// </summary>
        public string UDF_Key7 { get; set; }

        /// <summary>
        ///自定义值7
        /// </summary>
        public string UDF_Value7 { get; set; }

        /// <summary>
        ///自定义字段8
        /// </summary>
        public string UDF_Key8 { get; set; }

        /// <summary>
        ///自定义值8
        /// </summary>
        public string UDF_Value8 { get; set; }

        /// <summary>
        ///自定义字段9
        /// </summary>
        public string UDF_Key9 { get; set; }

        /// <summary>
        ///自定义值9
        /// </summary>
        public string UDF_Value9 { get; set; }

        /// <summary>
        ///自定义字段10
        /// </summary>
        public string UDF_Key10 { get; set; }

        /// <summary>
        ///自定义值10
        /// </summary>
        public string UDF_Value10 { get; set; }

        /// <summary>
        ///自定义字段11
        /// </summary>
        public string UDF_Key11 { get; set; }

        /// <summary>
        ///自定义值11
        /// </summary>
        public string UDF_Value11 { get; set; }

        /// <summary>
        ///自定义字段12
        /// </summary>
        public string UDF_Key12 { get; set; }

        /// <summary>
        ///自定义值12
        /// </summary>
        public string UDF_Value12 { get; set; }

        /// <summary>
        ///自定义字段13
        /// </summary>

        public string UDF_Key13 { get; set; }

        /// <summary>
        ///自定义值13
        /// </summary>

        public string UDF_Value13 { get; set; }

        /// <summary>
        ///自定义字段14
        /// </summary>

        public string UDF_Key14 { get; set; }

        /// <summary>
        ///自定义值14
        /// </summary>
        public string UDF_Value14 { get; set; }

        /// <summary>
        ///自定义字段15
        /// </summary>

        public string UDF_Key15 { get; set; }

        /// <summary>
        ///自定义值15
        /// </summary>

        public string UDF_Value15 { get; set; }
    }

        
}