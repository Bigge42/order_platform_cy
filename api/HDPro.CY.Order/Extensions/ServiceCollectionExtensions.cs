/*
 * 服务集合扩展类
 * 用于配置集成服务的依赖注入
 */
using HDPro.CY.Order.IServices.OA;
using HDPro.CY.Order.IServices.SRM;
using HDPro.CY.Order.Services.OA;
using HDPro.CY.Order.Services.SRM;
using Microsoft.Extensions.DependencyInjection;

namespace HDPro.CY.Order.Extensions
{
    /// <summary>
    /// 服务集合扩展类
    /// 提供依赖注入配置方法
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加OA集成服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddOAIntegrationServices(this IServiceCollection services)
        {
            // 注册HttpClient
            services.AddHttpClient<IOAIntegrationService, OAIntegrationService>();
            
            // 注册OA集成服务
            services.AddScoped<IOAIntegrationService, OAIntegrationService>();
            
            return services;
        }

        /// <summary>
        /// 添加SRM集成服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddSRMIntegrationServices(this IServiceCollection services)
        {
            // 注册HttpClient
            services.AddHttpClient<ISRMIntegrationService, SRMIntegrationService>();
            
            // 注册SRM集成服务
            services.AddScoped<ISRMIntegrationService, SRMIntegrationService>();
            
            return services;
        }

        /// <summary>
        /// 添加所有集成服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddAllIntegrationServices(this IServiceCollection services)
        {
            // 添加OA集成服务
            services.AddOAIntegrationServices();
            
            // 添加SRM集成服务
            services.AddSRMIntegrationServices();
            
            return services;
        }
    }
} 