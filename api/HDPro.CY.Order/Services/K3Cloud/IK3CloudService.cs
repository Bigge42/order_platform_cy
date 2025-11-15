/*
 * K3Cloud服务接口
 */
using HDPro.CY.Order.Services.K3Cloud.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HDPro.CY.Order.Services.K3Cloud
{
    /// <summary>
    /// K3Cloud服务接口
    /// </summary>
    public interface IK3CloudService
    {
        /// <summary>
        /// 登录K3Cloud
        /// </summary>
        /// <returns>登录响应</returns>
        Task<K3CloudLoginResponse> LoginAsync();

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <typeparam name="T">返回数据类型</typeparam>
        /// <param name="request">查询请求</param>
        /// <returns>查询响应</returns>
        Task<K3CloudQueryResponse<T>> ExecuteQueryAsync<T>(K3CloudQueryRequest request);

        /// <summary>
        /// 获取物料总数
        /// </summary>
        /// <param name="filterString">过滤条件</param>
        /// <returns>物料总数</returns>
        Task<int> GetMaterialCountAsync(string filterString = null);

        /// <summary>
        /// 分页获取物料数据
        /// </summary>
        /// <param name="pageIndex">页索引（从0开始）</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="orderString">排序字段</param>
        /// <returns>物料数据列表</returns>
        Task<K3CloudQueryResponse<K3CloudMaterialData>> GetMaterialsAsync(
            int pageIndex = 0, 
            int pageSize = 1000, 
            string filterString = null, 
            string orderString = "FNumber");

        /// <summary>
        /// 获取供应商总数
        /// </summary>
        /// <param name="filterString">过滤条件</param>
        /// <returns>供应商总数</returns>
        Task<int> GetSupplierCountAsync(string filterString = null);

        /// <summary>
        /// 分页获取供应商数据
        /// </summary>
        /// <param name="pageIndex">页索引（从0开始）</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="orderString">排序字段</param>
        /// <returns>供应商数据列表</returns>
        Task<K3CloudQueryResponse<K3CloudSupplierData>> GetSuppliersAsync(
            int pageIndex = 0, 
            int pageSize = 1000, 
            string filterString = null, 
            string orderString = "FNumber");

        /// <summary>
        /// 获取客户总数
        /// </summary>
        /// <param name="filterString">过滤条件</param>
        /// <returns>客户总数</returns>
        Task<int> GetCustomerCountAsync(string filterString = null);

        /// <summary>
        /// 分页获取客户数据
        /// </summary>
        /// <param name="pageIndex">页索引（从0开始）</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="orderString">排序字段</param>
        /// <returns>客户数据列表</returns>
        Task<K3CloudQueryResponse<K3CloudCustomerData>> GetCustomersAsync(
            int pageIndex = 0,
            int pageSize = 1000,
            string filterString = null,
            string orderString = "FNumber");

        /// <summary>
        /// BOM展开
        /// </summary>
        /// <param name="materialNumber">物料编码</param>
        /// <returns>BOM展开结果</returns>
        Task<ServiceResult<List<BomExpandItemDto>>> ExpandBomAsync(string materialNumber);
    }
}