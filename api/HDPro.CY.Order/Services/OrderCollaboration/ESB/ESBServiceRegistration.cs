using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.SubOrder;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.Purchase;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.Part;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.WholeUnit;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.Metalwork;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.TechManagement;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.SalesManagement;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.OrderTracking;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.LackMaterial;
using HDPro.CY.Order.IServices.OrderCollaboration.ESB;

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB
{
    /// <summary>
    /// ESB服务依赖注入注册配置
    /// </summary>
    public static class ESBServiceRegistration
    {
        /// <summary>
        /// 注册所有ESB相关服务到DI容器
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddESBServices(this IServiceCollection services)
        {
            // 注册ESB基础服务 - 使用工厂模式解决ILogger依赖问题
            services.AddScoped<ESBBaseService>(provider =>
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("ESBBaseService");
                return new ESBBaseService(httpClientFactory, logger, loggerFactory);
            });

            // 注册委外业务领域服务
            services.AddScoped<SubOrderESBSyncService>();
            services.AddScoped<SubOrderDetailESBSyncService>();
            services.AddScoped<SubOrderUnFinishTrackESBSyncService>();
            services.AddScoped<SubOrderESBSyncCoordinator>();

            // 注册采购业务领域服务
            services.AddScoped<PurchaseOrderESBSyncService>();
            services.AddScoped<PurchaseOrderDetailESBSyncService>();
            services.AddScoped<PurchaseOrderUnFinishTrackESBSyncService>();
            services.AddScoped<PurchaseESBSyncCoordinator>();

            // 注册部件业务领域服务
            services.AddScoped<PartPrdMOESBSyncService>();
            services.AddScoped<PartPrdMODetailESBSyncService>();
            services.AddScoped<PartUnFinishTrackESBSyncService>();
            services.AddScoped<PartESBSyncCoordinator>();

            // 注册整机业务领域服务
            services.AddScoped<WholeUnitPrdMOESBSyncService>();
            services.AddScoped<WholeUnitPrdMODetailESBSyncService>();
            services.AddScoped<WholeUnitTrackingESBSyncService>();
            services.AddScoped<WholeUnitESBSyncCoordinator>();

            // 注册金工车间业务领域服务
            services.AddScoped<MetalworkPrdMOESBSyncService>();
            services.AddScoped<MetalworkPrdMODetailESBSyncService>();
            services.AddScoped<MetalworkUnFinishTrackESBSyncService>();
            services.AddScoped<MetalworkESBSyncCoordinator>();

            // 注册技术管理业务领域服务
            services.AddScoped<TechManagementESBSyncService>();
            services.AddScoped<TechManagementESBSyncCoordinator>();

            // 注册销售管理业务领域服务
            services.AddScoped<SalesOrderListESBSyncService>();
            services.AddScoped<SalesOrderDetailESBSyncService>();
            services.AddScoped<SalesBatchInfoESBSyncService>();
            services.AddScoped<SalesManagementESBSyncCoordinator>();

            // 注册订单跟踪业务领域服务
            services.AddScoped<OrderTrackingESBSyncService>();
            services.AddScoped<OrderTrackingESBSyncCoordinator>();

            // 注册订单进度监造查询服务
            services.AddScoped<OrderProgressQueryService>();

            // 注册物料管理业务领域服务
            services.AddScoped<LackMtrlResultESBSyncService>();
            services.AddScoped<LackMtrlResultESBSyncCoordinator>();

            // 注册删除数据查询服务
            services.AddScoped<QueryDeletedDataService>();
            services.AddScoped<IQueryDeletedDataService, QueryDeletedDataService>();

            // 注册主协调器
            services.AddScoped<ESBMasterCoordinator>();
            return services;
        }

    }
} 