/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("OCP_AlertRules",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.Entity.DomainModels;
using HDPro.CY.Order.IServices;
using HDPro.Core.Utilities;
using System.Linq;
using HDPro.CY.Order.IRepositories;

namespace HDPro.CY.Order.Controllers
{
    public partial class OCP_AlertRulesController
    {
        private readonly IOCP_AlertRulesRepository _repository;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public OCP_AlertRulesController(
            IOCP_AlertRulesService service, IOCP_AlertRulesRepository repository,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 根据页面名称获取预警规则
        /// </summary>
        /// <param name="pageName">页面名称(如:OCP_TechManagement)</param>
        /// <returns>预警规则列表</returns>
        [HttpGet("GetAlertRulesByPage")]
        public async Task<WebResponseContent> GetAlertRulesByPage(string pageName)
        {
            var response = new WebResponseContent();
            try
            {
                if (string.IsNullOrWhiteSpace(pageName))
                {
                    return response.Error("页面名称不能为空");
                }

                // 查询该页面的所有启用状态的预警规则
                var rules = await _repository.FindAsync(x =>
                    x.AlertPage == pageName &&
                    x.TaskStatus == 1); // 1=启用状态

                if (rules == null || !rules.Any())
                {
                    return response.OK(null, new List<OCP_AlertRules>());
                }

                // 返回规则列表
                return response.OK(null, rules.ToList());
            }
            catch (Exception ex)
            {
                return response.Error($"获取预警规则失败: {ex.Message}");
            }
        }
    }
}
