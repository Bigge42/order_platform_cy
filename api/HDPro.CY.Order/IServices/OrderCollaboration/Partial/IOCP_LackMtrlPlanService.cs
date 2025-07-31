/*
*所有关于OCP_LackMtrlPlan类的业务代码接口应在此处编写
*/
using HDPro.Core.BaseProvider;
using HDPro.Entity.DomainModels;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
namespace HDPro.CY.Order.IServices
{
    public partial interface IOCP_LackMtrlPlanService
    {
        /// <summary>
        /// 设置默认方案
        /// </summary>
        /// <param name="computeId">要设置为默认的方案ID</param>
        /// <returns>操作结果</returns>
        WebResponseContent SetDefaultPlan(long computeId);

        /// <summary>
        /// 取消默认方案
        /// </summary>
        /// <param name="computeId">要取消默认的方案ID</param>
        /// <returns>操作结果</returns>
        WebResponseContent CancelDefaultPlan(long computeId);
    }
 }
