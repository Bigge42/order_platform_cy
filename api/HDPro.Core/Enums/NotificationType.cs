using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDPro.Core.Enums
{
    /// <summary>
    /// 通知类型
    /// </summary>
    public enum NotificationType
    {
        审批 = 1,
        通知 = 2,
        系统 = 3,
        催单=4,
        协商=5
    }
    /// <summary>
    /// 通知对象类型
    /// </summary>
    public enum NotificationTarget
    {
        用户 = 1,
        角色 = 2,
        部门 = 3,
        岗位 = 4
    }
}
