using System.Collections.Generic;
using System.Threading.Tasks;
using HDPro.Core.Utilities;
using HDPro.Entity.DomainModels.ESB;

namespace HDPro.CY.Order.IServices.OrderCollaboration.ESB
{
    /// <summary>
    /// ESB删除数据查询服务接口
    /// 用于查询、存储删除数据并执行业务表删除逻辑
    /// </summary>
    public interface IQueryDeletedDataService
    {
        /// <summary>
        /// 查询、存储并删除单个业务类型的数据（完整流程）
        /// </summary>
        /// <param name="businessType">业务类型（DDGZ、BOMDJJD、DDJDCX、CGGZ、WWGZ、ZJGZ、BJGZ、JGGZ）</param>
        /// <returns>完整流程处理结果</returns>
        Task<WebResponseContent> ProcessDeletedDataAsync(string businessType);

        /// <summary>
        /// 查询、存储并删除所有业务类型的数据（完整流程）
        /// </summary>
        /// <returns>完整流程处理结果</returns>
        Task<WebResponseContent> ProcessAllDeletedDataAsync();

        /// <summary>
        /// 获取业务类型统计信息
        /// </summary>
        /// <returns>业务类型统计信息</returns>
        WebResponseContent GetBusinessTypeStatistics();

        /// <summary>
        /// 验证业务类型参数
        /// </summary>
        /// <param name="businessType">业务类型</param>
        /// <returns>验证结果</returns>
        WebResponseContent ValidateBusinessType(string businessType);
    }
}