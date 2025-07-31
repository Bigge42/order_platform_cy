/*
 * SRM集成服务接口
 * 定义与SRM系统集成的服务契约
 */
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Core.Utilities;
using HDPro.CY.Order.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HDPro.CY.Order.IServices.SRM
{
    /// <summary>
    /// SRM集成服务接口
    /// 提供与SRM系统集成的服务方法
    /// </summary>
    public interface ISRMIntegrationService : IDependency
    {
        /// <summary>
        /// 推送催单数据到SRM系统
        /// </summary>
        /// <param name="orderAskList">催单数据列表</param>
        /// <param name="operatorName">操作人</param>
        /// <returns>推送结果</returns>
        Task<WebResponseContent> PushOrderAskAsync(List<OrderAskData> orderAskList, string operatorName = null);

        /// <summary>
        /// 推送单个催单数据到SRM系统
        /// </summary>
        /// <param name="orderAskData">催单数据</param>
        /// <param name="operatorName">操作人</param>
        /// <returns>推送结果</returns>
        Task<WebResponseContent> PushSingleOrderAskAsync(OrderAskData orderAskData, string operatorName = null);

        /// <summary>
        /// 验证SRM连接状态
        /// </summary>
        /// <returns>连接状态</returns>
        Task<WebResponseContent> ValidateConnectionAsync();

        /// <summary>
        /// 获取SRM配置信息
        /// </summary>
        /// <returns>配置信息</returns>
        SRMConfig GetSRMConfig();
    }
} 