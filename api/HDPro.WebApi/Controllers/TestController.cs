using Microsoft.AspNetCore.Mvc;
using HDPro.Core.Utilities;

namespace HDPro.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        /// <summary>
        /// 测试中文编码
        /// </summary>
        /// <returns></returns>
        [HttpGet("encoding")]
        public IActionResult TestEncoding()
        {
            return Ok(new 
            { 
                message = "中文编码测试成功", 
                status = true, 
                data = new 
                {
                    chinese = "这是中文测试",
                    english = "This is English test",
                    mixed = "混合文本 Mixed Text 测试"
                }
            });
        }

        /// <summary>
        /// 测试授权失败响应
        /// </summary>
        /// <returns></returns>
        [HttpGet("unauthorized")]
        public IActionResult TestUnauthorized()
        {
            return Unauthorized(new 
            { 
                message = "授权未通过", 
                status = false, 
                code = 401 
            });
        }
    }
} 