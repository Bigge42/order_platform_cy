/*
*所有关于OCP_TechManagement类的业务代码接口应在此处编写
*/
using HDPro.Core.BaseProvider;
using HDPro.Entity.DomainModels;
using HDPro.Core.Utilities;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HDPro.CY.Order.IServices
{
    public partial interface IOCP_TechManagementService
    {
        /// <summary>
        /// 根据物料编码获取技术管理负责人
        /// </summary>
        /// <param name="materialCode">物料编码</param>
        /// <returns>负责人信息</returns>
        Task<WebResponseContent> GetTechManagerByMaterialCodeAsync(string materialCode);

        /// <summary>
        /// 批量根据物料编码获取技术管理负责人
        /// </summary>
        /// <param name="materialCodes">物料编码列表</param>
        /// <returns>批量负责人信息</returns>
        Task<WebResponseContent> BatchGetTechManagerByMaterialCodeAsync(List<string> materialCodes);

        /// <summary>
        /// 获取近14天的BOM未搭建数量统计
        /// </summary>
        /// <returns>近14天每日BOM未搭建数量统计数据</returns>
        Task<List<object>> GetLast14DaysBomUnbuiltTrend();

        /// <summary>
        /// 获取BOM搭建情况统计
        /// </summary>
        /// <returns>BOM搭建情况统计数据</returns>
        Task<object[]> GetBomBuildStatusSummary();
    }
 }
