using System.Threading.Tasks;
using HDPro.Core.Utilities;
using HDPro.Entity.DomainModels;

namespace HDPro.CY.Order.IServices.WZ
{
    /// <summary>
    /// 循环仓库看板手动更新服务接口
    /// </summary>
    public interface IXhckkbService
    {
        /// <summary>
        /// 手动触发从接口获取数据并更新 XhckkbRecord 表
        /// </summary>
        /// <returns>操作结果（成功或错误信息）</returns>
        Task<WebResponseContent> ManualUpdateXhckkbAsync();
    }
}
