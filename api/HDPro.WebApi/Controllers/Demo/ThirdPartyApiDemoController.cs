using Microsoft.AspNetCore.Mvc;
using HDPro.Core.Filters;
using HDPro.Core.Services;
using System.Threading.Tasks;
using HDPro.Entity.DomainModels;
using System;

namespace HDPro.WebApi.Controllers.Demo
{
    /// <summary>
    /// 第三方API权限验证示例控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ThirdPartyApiDemoController : ControllerBase
    {
        private readonly IThirdPartyAppService _thirdPartyAppService;

        public ThirdPartyApiDemoController(IThirdPartyAppService thirdPartyAppService)
        {
            _thirdPartyAppService = thirdPartyAppService;
        }

        /// <summary>
        /// 基础第三方API接口示例
        /// 需要在请求头中传入 X-App-Id 和 X-App-Secret
        /// </summary>
        /// <returns></returns>
        [HttpGet("basic")]
        [ThirdPartyApi]
        public IActionResult BasicApi()
        {
            // 获取当前第三方应用信息
            var app = HttpContext.Items["X-App-Id"] as Sys_ThirdPartyApp;
            var appId = HttpContext.Items["X-App-Secret"] as string;

            return Ok(new
            {
                message = "第三方API调用成功",
                appName = app?.AppName,
                appId = appId,
                timestamp = DateTime.Now
            });
        }

        /// <summary>
        /// 带权限验证的第三方API接口示例
        /// 需要在请求头中传入 X-App-Id 和 X-App-Secret
        /// 并且应用需要有 "order:read" 权限
        /// </summary>
        /// <returns></returns>
        [HttpGet("order/list")]
        [ThirdPartyApi("order:read")]
        public IActionResult GetOrderList()
        {
            var app = HttpContext.Items["X-App-Id"] as Sys_ThirdPartyApp;
            
            return Ok(new
            {
                message = "订单列表获取成功",
                appName = app?.AppName,
                data = new[]
                {
                    new { orderId = "ORD001", orderName = "测试订单1", amount = 1000 },
                    new { orderId = "ORD002", orderName = "测试订单2", amount = 2000 }
                },
                timestamp = DateTime.Now
            });
        }

        /// <summary>
        /// 不验证IP白名单的第三方API接口示例
        /// </summary>
        /// <returns></returns>
        [HttpGet("public")]
        [ThirdPartyApi(ValidateIP = false)]
        public IActionResult PublicApi()
        {
            var app = HttpContext.Items["X-App-Id"] as Sys_ThirdPartyApp;
            
            return Ok(new
            {
                message = "公共API调用成功（不验证IP）",
                appName = app?.AppName,
                timestamp = DateTime.Now
            });
        }

        /// <summary>
        /// 不验证过期时间的第三方API接口示例
        /// </summary>
        /// <returns></returns>
        [HttpGet("no-expire-check")]
        [ThirdPartyApi(ValidateExpire = false)]
        public IActionResult NoExpireCheckApi()
        {
            var app = HttpContext.Items["X-App-Id"] as Sys_ThirdPartyApp;
            
            return Ok(new
            {
                message = "API调用成功（不验证过期时间）",
                appName = app?.AppName,
                timestamp = DateTime.Now
            });
        }

        /// <summary>
        /// 生成新的AppId和AppSecret
        /// 这个接口通常只对管理员开放，这里仅作演示
        /// </summary>
        /// <param name="prefix">AppId前缀</param>
        /// <returns></returns>
        [HttpPost("generate-credentials")]
        public IActionResult GenerateCredentials(string prefix = "CY1jOrd")
        {
            var appId = _thirdPartyAppService.GenerateAppId(prefix);
            var appSecret = _thirdPartyAppService.GenerateAppSecret();

            return Ok(new
            {
                message = "凭据生成成功",
                appId = appId,
                appSecret = appSecret,
                note = "请妥善保管AppSecret，系统不会再次显示完整密钥",
                timestamp = DateTime.Now
            });
        }

        /// <summary>
        /// 获取API使用说明
        /// </summary>
        /// <returns></returns>
        [HttpGet("usage")]
        public IActionResult GetUsage()
        {
            return Ok(new
            {
                title = "第三方API使用说明",
                authentication = new
                {
                    method = "Header认证",
                    headers = new
                    {
                        appId = "X-App-Id: 您的应用ID",
                        appSecret = "X-App-Secret: 您的应用密钥"
                    }
                },
                optionalHeaders = new
                {
                    timestamp = "X-Timestamp: Unix时间戳（用于签名验证）",
                    signature = "X-Signature: HMAC-SHA256签名（可选，增强安全性）"
                },
                signatureGeneration = new
                {
                    algorithm = "HMAC-SHA256",
                    signString = "HTTP方法 + 路径 + 查询参数 + 时间戳 + AppSecret",
                    example = "GET/api/ThirdPartyApiDemo/basic?timestamp=1640995200CY1jOrdASKBOX20250606123456"
                },
                examples = new object []
                {
                    new
                    {
                        name = "基础API调用",
                        url = "/api/ThirdPartyApiDemo/basic",
                        method = "GET",
                        headers = new
                        {
                            XAppId = "CY1jOrdASKBOX20250606",
                            XAppSecret = "D4MteF8mDJRdFOL9O>ORDJH*ITAGD4RASK"
                        }
                    },
                    new
                    {
                        name = "带权限验证的API调用",
                        url = "/api/ThirdPartyApiDemo/order/list",
                        method = "GET",
                        permission = "order:read",
                        headers = new
                        {
                            XAppId = "CY1jOrdASKBOX20250606",
                            XAppSecret = "D4MteF8mDJRdFOL9O>ORDJH*ITAGD4RASK"
                        }
                    }
                }
            });
        }
    }
} 