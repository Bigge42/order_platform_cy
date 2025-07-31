/*
 *所有关于Sys_ThirdPartyApp类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*Sys_ThirdPartyAppService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using HDPro.Core.BaseProvider;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;
using System.Linq;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
using HDPro.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.Sys.IRepositories;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HDPro.Core.ManageUser;
using HDPro.Core.Services;

namespace HDPro.Sys.Services
{
    public partial class Sys_ThirdPartyAppService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISys_ThirdPartyAppRepository _repository;//访问数据库
        private readonly IThirdPartyAppService _thirdPartyAppService;//第三方应用核心服务

        [ActivatorUtilitiesConstructor]
        public Sys_ThirdPartyAppService(
            ISys_ThirdPartyAppRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            IThirdPartyAppService thirdPartyAppService
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            _thirdPartyAppService = thirdPartyAppService;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        /// <summary>
        /// 创建第三方应用
        /// </summary>
        /// <param name="appName">应用名称</param>
        /// <param name="description">应用描述</param>
        /// <param name="allowedIPs">允许的IP地址，多个用逗号分隔</param>
        /// <param name="expireTime">过期时间</param>
        /// <param name="prefix">AppId前缀</param>
        /// <returns></returns>
        public async Task<WebResponseContent> CreateThirdPartyAppAsync(
            string appName, 
            string description = "", 
            string allowedIPs = "", 
            DateTime? expireTime = null,
            string prefix = "CY1jOrd")
        {
            var response = new WebResponseContent();
            
            try
            {
                // 使用核心服务生成AppId和AppSecret
                var appId = _thirdPartyAppService.GenerateAppId(prefix);
                var appSecret = _thirdPartyAppService.GenerateAppSecret();

                var app = new Sys_ThirdPartyApp
                {
                    AppId = appId,
                    AppSecret = appSecret,
                    AppName = appName,
                    Description = description,
                    IsEnabled = 1,
                    AllowedIPs = allowedIPs,
                    ExpireTime = expireTime,
                    AccessCount = 0,
                    CreateTime = DateTime.Now,
                    CreateId = UserContext.Current?.UserId,
                    Creator = UserContext.Current?.UserName
                };

                await _repository.AddAsync(app);
                await _repository.SaveChangesAsync();

                response.OK("第三方应用创建成功", new
                {
                    appId = appId,
                    appSecret = appSecret,
                    appName = appName,
                    note = "请妥善保管AppSecret，系统不会再次显示完整密钥"
                });
            }
            catch (Exception ex)
            {
                response.Error($"创建第三方应用失败：{ex.Message}");
            }

            return response;
        }

        /// <summary>
        /// 重新生成AppSecret
        /// </summary>
        /// <param name="appId">应用ID</param>
        /// <returns></returns>
        public async Task<WebResponseContent> RegenerateAppSecretAsync(string appId)
        {
            var response = new WebResponseContent();
            
            try
            {
                var app = await _repository.FindFirstAsync(x => x.AppId == appId);
                if (app == null)
                {
                    return response.Error("应用不存在");
                }

                // 使用核心服务生成新的AppSecret
                var newAppSecret = _thirdPartyAppService.GenerateAppSecret();
                app.AppSecret = newAppSecret;
                app.ModifyTime = DateTime.Now;

                _repository.Update(app, true);

                response.OK("AppSecret重新生成成功", new
                {
                    appId = appId,
                    appSecret = newAppSecret,
                    note = "请妥善保管新的AppSecret，旧的AppSecret将立即失效"
                });
            }
            catch (Exception ex)
            {
                response.Error($"重新生成AppSecret失败：{ex.Message}");
            }

            return response;
        }

        /// <summary>
        /// 启用/禁用第三方应用
        /// </summary>
        /// <param name="appId">应用ID</param>
        /// <param name="enabled">是否启用</param>
        /// <returns></returns>
        public async Task<WebResponseContent> SetAppEnabledAsync(string appId, bool enabled)
        {
            var response = new WebResponseContent();
            
            try
            {
                var app = await _repository.FindFirstAsync(x => x.AppId == appId);
                if (app == null)
                {
                    return response.Error("应用不存在");
                }

                app.IsEnabled = enabled ? 1 : 0;
                app.ModifyTime = DateTime.Now;

                _repository.Update(app, true);

                response.OK($"应用已{(enabled ? "启用" : "禁用")}");
            }
            catch (Exception ex)
            {
                response.Error($"操作失败：{ex.Message}");
            }

            return response;
        }
  }
}
