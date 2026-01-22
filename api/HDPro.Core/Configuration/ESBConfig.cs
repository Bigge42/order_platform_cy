using System;
using System.Collections.Generic;
using System.Text;

namespace HDPro.Core.Configuration
{
    /// <summary>
    /// ESB平台配置
    /// </summary>
    public class ESBConfig
    {
        /// <summary>
        /// ESB基础地址
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// 默认超时时间（分钟）
        /// </summary>
        public int DefaultTimeoutMinutes { get; set; } = 15;

        /// <summary>
        /// 长时间运行接口超时时间（分钟）
        /// </summary>
        public int LongRunningTimeoutMinutes { get; set; } = 30;

        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// 重试延迟时间（秒）
        /// </summary>
        public int RetryDelaySeconds { get; set; } = 30;

        /// <summary>
        /// 默认同步天数
        /// </summary>
        public int DefaultSyncDays { get; set; } = 5;

        /// <summary>
        /// 批量处理大小
        /// </summary>
        public int BatchSize { get; set; } = 1000;

        /// <summary>
        /// 数据库查询批量大小
        /// </summary>
        public int QueryBatchSize { get; set; } = 500;

        /// <summary>
        /// 单次事务最大处理时间（分钟）
        /// </summary>
        public int TransactionTimeoutMinutes { get; set; } = 10;

        /// <summary>
        /// 接口配置
        /// </summary>
        public ESBApiConfig Apis { get; set; } = new ESBApiConfig();
    }

    /// <summary>
    /// ESB接口配置
    /// </summary>
    public class ESBApiConfig
    {
        // 销售管理相关服务
        public string SalesOrderListESBSyncService { get; set; } = "/gateway/DataCenter/SearchERPSalOrderList";
        public string SalesOrderDetailESBSyncService { get; set; } = "/gateway/SearchERPSalOrderEntry";
        public string SalesBatchInfoESBSyncService { get; set; } = "/gateway/SearchERPSalOrderEntry";
        
        // 订单跟踪服务
        public string OrderTrackingESBSyncService { get; set; } = "/gateway/DataCenter/SearchSalOder";

        // ERP订单跟踪明细同步服务
        public string ERPOrderTrackingESBSyncService { get; set; } = "DDPTSeacherSalOrderEntry";
        
        // 缺料管理服务
        public string LackMtrlResultESBSyncService { get; set; } = "/gateway/DataCenter/SearchQLCal";
        
        // 采购管理相关服务
        public string PurchaseOrderESBSyncService { get; set; } = "/SearchPoor";
        public string PurchaseOrderDetailESBSyncService { get; set; } = "/SearchPoorEntry";
        public string PurchaseOrderUnFinishTrackESBSyncService { get; set; } = "/SearchPoorProgress";
        
        // 部件生产相关服务
        public string PartPrdMOESBSyncService { get; set; } = "/SearchBujianMo";
        public string PartPrdMODetailESBSyncService { get; set; } = "/SearchBujianMOEntry";
        public string PartUnFinishTrackESBSyncService { get; set; } = "/SearchBujianMoProgress";
        
        // 整机生产相关服务
        public string WholeUnitPrdMOESBSyncService { get; set; } = "/SearchZhuangpeiMo";
        public string WholeUnitPrdMODetailESBSyncService { get; set; } = "/SearchZhuangpeiMOEntry";
        public string WholeUnitTrackingESBSyncService { get; set; } = "/SearchZhuangpeiMoProgress";
        
        // 金工生产相关服务
        public string MetalworkPrdMOESBSyncService { get; set; } = "/SearchJingongMo";
        public string MetalworkPrdMODetailESBSyncService { get; set; } = "/SearchJingongMOEntry";
        public string MetalworkUnFinishTrackESBSyncService { get; set; } = "/SearchJingongMoProgress";
        
        // 技术管理服务
        public string TechManagementESBSyncService { get; set; } = "/SearchBOMCreateDate";
        
        // 子订单相关服务
        public string SubOrderESBSyncService { get; set; } = "/SearchSubOrder";
        public string SubOrderDetailESBSyncService { get; set; } = "/SearchSubOrderEntry";
        public string SubOrderUnFinishTrackESBSyncService { get; set; } = "/SearchSubOrderProgress";

        /// <summary>
        /// 根据服务名称获取API路径
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <returns>API路径</returns>
        public string GetApiPath(string serviceName)
        {
            var property = this.GetType().GetProperty(serviceName);
            return property?.GetValue(this)?.ToString() ?? string.Empty;
        }
    }
} 
