using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using HDPro.Core.Controllers.Basic;
using HDPro.Core.Utilities;
using HDPro.Entity.AttributeManager;
using HDPro.Entity.DomainModels;
using HDPro.Sys.IRepositories;

namespace HDPro.WebApi.Controllers.DataView
{
    /// <summary>
    /// 所有接口路由都要以api/dataview开头
    /// </summary>
    [Route("api/dataview/test")]

    public class DataViewTestController : VolController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISys_UserRepository _userRepository;
        public DataViewTestController(IHttpContextAccessor httpContextAccessor, ISys_UserRepository userRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
        }
         
        [Route("Text1"), HttpGet,HttpPost]
        public async Task<IActionResult> Text1()
        {
            await Task.CompletedTask;
            return Json(new { value=DateTime.Now});
        }
        [Route("data1"), HttpGet]
        public async Task<IActionResult> Data1()
        {
            await Task.CompletedTask;
            return Json(new { });
        } 
    }
}
