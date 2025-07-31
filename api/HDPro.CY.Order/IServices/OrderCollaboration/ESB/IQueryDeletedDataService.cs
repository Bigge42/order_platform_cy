using System.Collections.Generic;
using System.Threading.Tasks;
using HDPro.Core.Utilities;
using HDPro.Entity.DomainModels.ESB;

namespace HDPro.CY.Order.IServices.OrderCollaboration.ESB
{
    /// <summary>
    /// ESB删除数据查询服务接口
    /// 用于查询订单协同平台各业务领域的删除数据主键
    /// </summary>
    public interface IQueryDeletedDataService
    {
        /// <summary>
        /// 查询单个业务类型的删除数据
        /// </summary>
        /// <param name="businessType">业务类型（DDGZ、BOMDJJD、DDJDCX、CGGZ、WWGZ、ZJGZ、BJGZ、JGGZ）</param>
        /// <returns>删除数据查询结果</returns>
        Task<WebResponseContent> QueryDeletedDataAsync(string businessType);

        /// <summary>
        /// 批量查询所有业务类型的删除数据
        /// </summary>
        /// <returns>所有业务类型的删除数据查询结果</returns>
        Task<WebResponseContent> QueryAllDeletedDataAsync();

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