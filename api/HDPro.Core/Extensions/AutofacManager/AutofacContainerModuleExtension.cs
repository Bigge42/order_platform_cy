using Autofac;
using Autofac.Extensions.DependencyInjection;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using HDPro.Core.CacheManager;
using HDPro.Core.Configuration;
using HDPro.Core.Const;
using HDPro.Core.Dapper;
using HDPro.Core.DBManager;
using HDPro.Core.EFDbContext;
using HDPro.Core.Enums;
using HDPro.Core.Extensions.AutofacManager;
//using HDPro.Core.KafkaManager.IService;
//using HDPro.Core.KafkaManager.Service;
using HDPro.Core.ManageUser;
using HDPro.Core.ObjectActionValidator;
using HDPro.Core.Services;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.EntityFrameworkCore.Internal;

namespace HDPro.Core.Extensions
{
    public static class AutofacContainerModuleExtension
    { 
        //  private static bool _isMysql = false;
        public static IServiceCollection AddModule(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddSession();
            //services.AddMemoryCache();
            //初始化配置文件
            AppSetting.Init(services, configuration);
            Type baseType = typeof(IDependency);
            var compilationLibrary = DependencyContext.Default
                .RuntimeLibraries
                .Where(x => !x.Serviceable
                && x.Type == "project")
                .ToList();
            var count1 = compilationLibrary.Count;
            List<Assembly> assemblyList = new List<Assembly>();

            foreach (var _compilation in compilationLibrary)
            {
                try
                {
                    assemblyList.Add(AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(_compilation.Name)));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(_compilation.Name + ex.Message);
                }
            }
            //插件开发
            //try
            //{
            //    var provider = services.BuildServiceProvider();
            //    IWebHostEnvironment webHostEnvironment = provider.GetRequiredService<IWebHostEnvironment>();
            //    string rootPath = (webHostEnvironment.ContentRootPath + "\\plugs").ReplacePath();
            //    foreach (var item in Directory.GetFiles(rootPath).Where(x => x.EndsWith(".dll")))
            //    {
            //        string path = ($"{item}").ReplacePath();
            //        AssemblyName assemblyName = Assembly.LoadFrom(path).GetName(); ;
            //        assemblyList.Add(AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName));
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"解析类库异常：{ex.Message + ex.StackTrace}");
            //}
            foreach (var _compilation in compilationLibrary)
            {
                var types = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(_compilation.Name)).GetTypes();

                var implementedInterfaces = types.Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Length > 0)
                    .Where(type => baseType.IsAssignableFrom(type) && !type.IsAbstract)
                    .Select(t => (serviceType: t.GetInterfaces(), implementationType: t))
                    .ToList();

                foreach (var (serviceType, implementationType) in implementedInterfaces)
                {
                    if (serviceType.Any(x => x == typeof(IDbContextDependencies)))
                    {
                        services.AddScoped(implementationType);
                    }
                    else
                    {
                        services.AddScoped(serviceType[0], implementationType);
                    }

                }
            }
            services.AddScoped<UserContext>();
            services.AddScoped<ActionObserver>();
            services.AddScoped<ObjectModelValidatorState>();

            services.AddSingleton(typeof(ICacheService), AppSetting.UseRedis ? typeof(RedisCacheService) : typeof(MemoryCacheService));
            if (AppSetting.UseRedis)
            {
                services.AddSingleton<RedisCacheService>();
            }
            if (DBType.Name == DbCurrentType.PgSql.ToString())
            {
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
            }
            DapperParseGuidTypeHandler.InitParseGuid();
            DbCache.Init();
            //kafka注入
            //if (AppSetting.Kafka.UseConsumer)
            //    builder.RegisterType<KafkaConsumer<string, string>>().As<IKafkaConsumer<string, string>>().SingleInstance();
            //if (AppSetting.Kafka.UseProducer)
            //    builder.RegisterType<KafkaProducer<string, string>>().As<IKafkaProducer<string, string>>().SingleInstance();
            return services;
        }

    }
}
