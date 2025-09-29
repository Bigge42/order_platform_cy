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
        /// 停止 - 不创建定时任务
        /// </summary>
        [Description("停止")]
        Stopped = 0,

        /// <summary>
        /// 启用 - 创建并启动定时任务
        /// </summary>
        [Description("启用")]
        Enabled = 1,

        /// <summary>
        /// 暂停 - 创建定时任务但暂停执行
        /// </summary>
        [Description("暂停")]
        Paused = 2
    }
}
