/*
*所有关于OCP_AlertRules类的业务代码接口应在此处编写
*/
using HDPro.Core.BaseProvider;
using HDPro.Entity.DomainModels;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HDPro.CY.Order.IServices
{
    public partial interface IOCP_AlertRulesService
    {
        /// <summary>
        /// 执行所有预警规则检查
        /// </summary>
        /// <returns>执行结果</returns>
        Task<WebResponseContent> ExecuteAllAlertRulesAsync(bool logToQuartz = true);

        /// <summary>
        /// 执行指定预警规则
        /// </summary>
        /// <param name="ruleId">规则ID</param>
        /// <returns>执行结果</returns>
        Task<WebResponseContent> ExecuteAlertRuleByIdAsync(long ruleId, bool logToQuartz = true);

        /// <summary>
        /// 测试预警规则（不发送通知，只返回查询结果）
        /// </summary>
        /// <param name="ruleId">规则ID</param>
        /// <returns>测试结果</returns>
        Task<WebResponseContent> TestAlertRuleAsync(long ruleId);

        /// <summary>
        /// 更新预警规则任务状态
        /// </summary>
        /// <param name="ruleId">规则ID</param>
        /// <param name="taskStatus">任务状态</param>
        /// <returns>更新结果</returns>
        Task<WebResponseContent> UpdateTaskStatusAsync(long ruleId, int taskStatus);
    }
 }
