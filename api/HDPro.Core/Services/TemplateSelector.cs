/*
 * 模板选择器
 * 根据项目命名空间自动选择合适的代码生成模板
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace HDPro.Core.Services
{
    /// <summary>
    /// 模板选择器
    /// 用于根据项目命名空间自动选择合适的代码生成模板
    /// </summary>
    public class TemplateSelector
    {
        private readonly Dictionary<string, TemplateConfig> _templateMappings;
        private readonly string _templatesPath;
        private readonly bool _autoDetectProject;
        private readonly bool _fallbackToDefault;

        public TemplateSelector()
        {
            LoadConfiguration();
            _templateMappings = LoadTemplateMappings();
            _templatesPath = "Template/Services/";
            _autoDetectProject = true;
            _fallbackToDefault = true;
        }

        /// <summary>
        /// 根据命名空间获取服务模板路径
        /// </summary>
        /// <param name="namespaceName">命名空间名称</param>
        /// <returns>模板文件路径</returns>
        public string GetServiceTemplate(string namespaceName)
        {
            var config = GetTemplateConfig(namespaceName);
            return Path.Combine(_templatesPath, config.ServiceTemplate);
        }

        /// <summary>
        /// 根据命名空间获取服务Partial模板路径
        /// </summary>
        /// <param name="namespaceName">命名空间名称</param>
        /// <returns>模板文件路径</returns>
        public string GetServicePartialTemplate(string namespaceName)
        {
            var config = GetTemplateConfig(namespaceName);
            return Path.Combine(_templatesPath, config.ServicePartialTemplate);
        }

        /// <summary>
        /// 获取模板配置信息
        /// </summary>
        /// <param name="namespaceName">命名空间名称</param>
        /// <returns>模板配置</returns>
        public TemplateConfig GetTemplateConfig(string namespaceName)
        {
            if (string.IsNullOrEmpty(namespaceName))
            {
                return _templateMappings["default"];
            }

            // 精确匹配
            if (_templateMappings.ContainsKey(namespaceName))
            {
                return _templateMappings[namespaceName];
            }

            // 模糊匹配（检查是否包含关键字）
            foreach (var mapping in _templateMappings)
            {
                if (mapping.Key != "default" && namespaceName.Contains(mapping.Key))
                {
                    return mapping.Value;
                }
            }

            // 返回默认配置
            return _templateMappings["default"];
        }

        /// <summary>
        /// 获取所有可用的模板配置
        /// </summary>
        /// <returns>模板配置字典</returns>
        public Dictionary<string, TemplateConfig> GetAllTemplateConfigs()
        {
            return new Dictionary<string, TemplateConfig>(_templateMappings);
        }

        /// <summary>
        /// 检查模板文件是否存在
        /// </summary>
        /// <param name="templatePath">模板文件路径</param>
        /// <returns>是否存在</returns>
        public bool TemplateExists(string templatePath)
        {
            return File.Exists(templatePath);
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        private void LoadConfiguration()
        {
            // 这里可以从配置文件或数据库加载配置
            // 暂时使用硬编码配置
        }

        /// <summary>
        /// 加载模板映射配置
        /// </summary>
        /// <returns>模板映射字典</returns>
        private Dictionary<string, TemplateConfig> LoadTemplateMappings()
        {
            var mappings = new Dictionary<string, TemplateConfig>();

            // CY.Order项目专用模板
            mappings["HDPro.CY.Order"] = new TemplateConfig
            {
                ServiceTemplate = "CYOrderServiceBase.html",
                ServicePartialTemplate = "CYOrderServiceBasePartial.html",
                Description = "CY.Order项目专用服务模板"
            };

            // 系统模块模板
            mappings["HDPro.Sys"] = new TemplateConfig
            {
                ServiceTemplate = "ServiceBase.html",
                ServicePartialTemplate = "ServiceBasePartial.html",
                Description = "系统默认服务模板"
            };

            // MES模块模板
            mappings["HDPro.MES"] = new TemplateConfig
            {
                ServiceTemplate = "ServiceBase.html",
                ServicePartialTemplate = "ServiceBasePartial.html",
                Description = "MES项目服务模板"
            };

            // 默认模板
            mappings["default"] = new TemplateConfig
            {
                ServiceTemplate = "ServiceBase.html",
                ServicePartialTemplate = "ServiceBasePartial.html",
                Description = "默认服务模板"
            };

            return mappings;
        }
    }

    /// <summary>
    /// 模板配置类
    /// </summary>
    public class TemplateConfig
    {
        /// <summary>
        /// 服务模板文件名
        /// </summary>
        public string ServiceTemplate { get; set; }

        /// <summary>
        /// 服务Partial模板文件名
        /// </summary>
        public string ServicePartialTemplate { get; set; }

        /// <summary>
        /// 模板描述
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// 模板选择器扩展方法
    /// </summary>
    public static class TemplateSelectorExtensions
    {
        /// <summary>
        /// 根据类型获取命名空间
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>命名空间</returns>
        public static string GetProjectNamespace(this Type type)
        {
            var namespaceParts = type.Namespace?.Split('.');
            if (namespaceParts != null && namespaceParts.Length >= 2)
            {
                return $"{namespaceParts[0]}.{namespaceParts[1]}";
            }
            return type.Namespace;
        }

        /// <summary>
        /// 从程序集名称获取项目命名空间
        /// </summary>
        /// <param name="assemblyName">程序集名称</param>
        /// <returns>项目命名空间</returns>
        public static string GetProjectNamespaceFromAssembly(string assemblyName)
        {
            // 从程序集名称推断项目命名空间
            // 例如：HDPro.CY.Order.dll -> HDPro.CY.Order
            return assemblyName.Replace(".dll", "").Replace(".exe", "");
        }
    }
} 