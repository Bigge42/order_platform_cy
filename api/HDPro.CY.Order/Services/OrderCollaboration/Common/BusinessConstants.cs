using System.Collections.Generic;
using System.Linq;

namespace HDPro.CY.Order.Services.OrderCollaboration.Common
{
    /// <summary>
    /// 控制阀订单智能管理业务常量
    /// </summary>
    public static class BusinessConstants
    {
        /// <summary>
        /// 业务类型
        /// </summary>
        public static class BusinessType
        {
            /// <summary>
            /// 委外
            /// </summary>
            public const string OutSourcing = "WW";
            
            /// <summary>
            /// 采购
            /// </summary>
            public const string Purchase = "PO";
            
            /// <summary>
            /// 销售
            /// </summary>
            public const string Sales = "SO";
            
            /// <summary>
            /// 技术
            /// </summary>
            public const string Technology = "JS";
            
            /// <summary>
            /// 部件
            /// </summary>
            public const string Component = "BJ";
            
            /// <summary>
            /// 金工
            /// </summary>
            public const string Metalwork = "JG";
            
            /// <summary>
            /// 装配
            /// </summary>
            public const string Assembly = "ZP";
            
            /// <summary>
            /// 计划
            /// </summary>
            public const string Planning = "JH";
        }

        /// <summary>
        /// 消息类型
        /// </summary>
        public static class MessageType
        {
            /// <summary>
            /// 催单消息
            /// </summary>
            public const string UrgentOrder = "催单通知";
            
            /// <summary>
            /// 协商消息
            /// </summary>
            public const string Negotiation = "协商通知";
            
            /// <summary>
            /// 交期协商消息
            /// </summary>
            public const string DeliveryNegotiation = "交期协商通知";
        }

        /// <summary>
        /// 催单状态
        /// </summary>
        public static class UrgentOrderStatus
        {
            /// <summary>
            /// 催单中（默认状态）
            /// </summary>
            public const string Pending = "催单中";
            
            /// <summary>
            /// 已回复
            /// </summary>
            public const string Replied = "已回复";
        }

        /// <summary>
        /// 协商状态常量
        /// </summary>
        public static class NegotiationStatus
        {
            /// <summary>
            /// 协商中（默认状态）
            /// </summary>
            public const string Pending = "协商中";

            /// <summary>
            /// 已同意
            /// </summary>
            public const string Approved = "已同意";

            /// <summary>
            /// 拒绝
            /// </summary>
            public const string Rejected = "拒绝";
        }

        /// <summary>
        /// 默认配置
        /// </summary>
        public static class DefaultConfig
        {
            /// <summary>
            /// 默认管理员账号
            /// </summary>
            public const string DefaultAdminUser = "admin";
            
            /// <summary>
            /// 系统用户名
            /// </summary>
            public const string SystemUser = "system";
            
            /// <summary>
            /// 系统用户显示名
            /// </summary>
            public const string SystemUserName = "系统";
            
            /// <summary>
            /// 默认数据源
            /// </summary>
            public const string DefaultDataSource = "协同平台";
            
            /// <summary>
            /// 默认执行机构
            /// </summary>
            public const string DefaultExecutiveOrganization = "1";
        }

        /// <summary>
        /// 日志分类
        /// </summary>
        public static class LogCategory
        {
            /// <summary>
            /// 控制阀订单智能管理
            /// </summary>
            public const string OrderCollaboration = "OrderCollaboration";
        }

        /// <summary>
        /// 部门相关常量
        /// </summary>
        public static class Department
        {
            /// <summary>
            /// 物资供应中心部门名称
            /// </summary>
            public const string MaterialSupplyCenter = "物资供应中心";
        }

        /// <summary>
        /// 检查是否为需要SRM集成的业务类型
        /// </summary>
        /// <param name="businessType">业务类型</param>
        /// <returns>是否需要SRM集成</returns>
        public static bool RequiresSRMIntegration(string businessType)
        {
            return businessType == BusinessType.OutSourcing || businessType == BusinessType.Purchase;
        }

        /// <summary>
        /// 检查当前用户是否属于物资供应中心
        /// </summary>
        /// <returns>是否属于物资供应中心</returns>
        public static bool IsCurrentUserInMaterialSupplyCenter()
        {
            try
            {
                var currentUser = HDPro.Core.ManageUser.UserContext.Current;
                if (currentUser?.UserInfo?.DeptIds == null || !currentUser.UserInfo.DeptIds.Any())
                {
                    return false;
                }

                // 获取用户所在的部门信息
                var userDeptIds = currentUser.UserInfo.DeptIds;
                var allDepartments = HDPro.Core.UserManager.DepartmentContext.GetAllDept();

                // 检查用户所在的部门是否包含"物资供应中心"
                var userDepartments = allDepartments.Where(d => userDeptIds.Contains(d.id)).ToList();

                return userDepartments.Any(d => d.value != null && d.value.Contains(Department.MaterialSupplyCenter));
            }
            catch (System.Exception)
            {
                // 如果出现异常，默认返回false，不影响主流程
                return false;
            }
        }

        /// <summary>
        /// 检查是否为需要OA流程的业务类型
        /// </summary>
        /// <param name="businessType">业务类型</param>
        /// <returns>是否需要OA流程</returns>
        public static bool RequiresOAProcess(string businessType)
        {
            return businessType == BusinessType.OutSourcing || businessType == BusinessType.Purchase;
        }

        /// <summary>
        /// 获取所有业务类型选项
        /// </summary>
        /// <returns>业务类型选项列表</returns>
        public static List<(string Code, string Name)> GetAllBusinessTypes()
        {
            return new List<(string Code, string Name)>
            {
                (BusinessType.OutSourcing, "委外"),
                (BusinessType.Purchase, "采购"),
                (BusinessType.Sales, "销售"),
                (BusinessType.Technology, "技术"),
                (BusinessType.Component, "部件"),
                (BusinessType.Metalwork, "金工"),
                (BusinessType.Assembly, "装配"),
                (BusinessType.Planning, "计划")
            };
        }
    }
} 