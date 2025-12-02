/*
 * 预警规则任务状态枚举
 */
using System.ComponentModel;

namespace HDPro.Entity.DomainModels.OrderCollaboration.Enums
{
    /// <summary>
    /// 预警规则任务状态枚举
    /// </summary>
    public enum AlertRuleTaskStatus
    {
        /// <summary>
        /// 暂停 - 定时任务存在但暂停执行
        /// </summary>
        [Description("暂停")]
        Paused = 0,

        /// <summary>
        /// 启用 - 定时任务运行中
        /// </summary>
        [Description("启用")]
        Enabled = 1
    }
}
