/*
 * 代码生成器服务
 * 集成模板选择功能，支持根据项目自动选择合适的模板
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using HDPro.Core.Utilities;

namespace HDPro.Core.Services
{
    /// <summary>
    /// 代码生成器服务
    /// 支持根据项目命名空间自动选择合适的模板
    /// </summary>
    public class CodeGeneratorService
    {
        private readonly TemplateSelector _templateSelector;
        private readonly string _templateBasePath;

        public CodeGeneratorService()
        {
            _templateSelector = new TemplateSelector();
            _templateBasePath = Path.Combine(AppContext.BaseDirectory, "Template");
        }

        /// <summary>
        /// 生成服务类代码
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="namespaceName">命名空间</param>
        /// <param name="startName">项目前缀</param>
        /// <returns>生成的代码</returns>
        public string GenerateServiceCode(string tableName, string namespaceName, string startName = "HDPro")
        {
            try
            {
                var templatePath = _templateSelector.GetServiceTemplate(namespaceName);
                var fullTemplatePath = Path.Combine(_templateBasePath, templatePath);

                if (!File.Exists(fullTemplatePath))
                {
                    Logger.Error($"模板文件不存在: {fullTemplatePath}");
                    // 使用默认模板
                    templatePath = _templateSelector.GetServiceTemplate("default");
                    fullTemplatePath = Path.Combine(_templateBasePath, templatePath);
                }

                var templateContent = File.ReadAllText(fullTemplatePath, Encoding.UTF8);
                var generatedCode = ReplaceTemplateVariables(templateContent, tableName, namespaceName, startName);

                Logger.Info($"使用模板 {templatePath} 为 {namespaceName}.{tableName} 生成服务类代码");
                return generatedCode;
            }
            catch (Exception ex)
            {
                Logger.Error($"生成服务类代码失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 生成服务Partial类代码
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="namespaceName">命名空间</param>
        /// <param name="startName">项目前缀</param>
        /// <returns>生成的代码</returns>
        public string GenerateServicePartialCode(string tableName, string namespaceName, string startName = "HDPro")
        {
            try
            {
                var templatePath = _templateSelector.GetServicePartialTemplate(namespaceName);
                var fullTemplatePath = Path.Combine(_templateBasePath, templatePath);

                if (!File.Exists(fullTemplatePath))
                {
                    Logger.Error($"模板文件不存在: {fullTemplatePath}");
                    // 使用默认模板
                    templatePath = _templateSelector.GetServicePartialTemplate("default");
                    fullTemplatePath = Path.Combine(_templateBasePath, templatePath);
                }

                var templateContent = File.ReadAllText(fullTemplatePath, Encoding.UTF8);
                var generatedCode = ReplaceTemplateVariables(templateContent, tableName, namespaceName, startName);

                Logger.Info($"使用模板 {templatePath} 为 {namespaceName}.{tableName} 生成服务Partial类代码");
                return generatedCode;
            }
            catch (Exception ex)
            {
                Logger.Error($"生成服务Partial类代码失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 获取可用的模板列表
        /// </summary>
        /// <returns>模板配置列表</returns>
        public Dictionary<string, TemplateConfig> GetAvailableTemplates()
        {
            return _templateSelector.GetAllTemplateConfigs();
        }

        /// <summary>
        /// 根据命名空间获取推荐的模板配置
        /// </summary>
        /// <param name="namespaceName">命名空间</param>
        /// <returns>模板配置</returns>
        public TemplateConfig GetRecommendedTemplate(string namespaceName)
        {
            return _templateSelector.GetTemplateConfig(namespaceName);
        }

        /// <summary>
        /// 替换模板变量
        /// </summary>
        /// <param name="templateContent">模板内容</param>
        /// <param name="tableName">表名</param>
        /// <param name="namespaceName">命名空间</param>
        /// <param name="startName">项目前缀</param>
        /// <returns>替换后的内容</returns>
        private string ReplaceTemplateVariables(string templateContent, string tableName, string namespaceName, string startName)
        {
            var result = templateContent;

            // 替换基本变量
            result = result.Replace("{TableName}", tableName);
            result = result.Replace("{Namespace}", namespaceName);
            result = result.Replace("{StartName}", startName);

            // 替换日期时间
            result = result.Replace("{DateTime}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            result = result.Replace("{Date}", DateTime.Now.ToString("yyyy-MM-dd"));
            result = result.Replace("{Year}", DateTime.Now.Year.ToString());

            // 替换作者信息（可以从配置中获取）
            result = result.Replace("{Author}", "Code Generator");
            result = result.Replace("{Contact}", "system@company.com");

            return result;
        }

        /// <summary>
        /// 验证生成的代码语法
        /// </summary>
        /// <param name="code">生成的代码</param>
        /// <returns>验证结果</returns>
        public CodeValidationResult ValidateGeneratedCode(string code)
        {
            var result = new CodeValidationResult { IsValid = true };

            try
            {
                // 基本语法检查
                if (string.IsNullOrWhiteSpace(code))
                {
                    result.IsValid = false;
                    result.Errors.Add("生成的代码为空");
                    return result;
                }

                // 检查基本C#语法结构
                if (!code.Contains("namespace") || !code.Contains("class"))
                {
                    result.IsValid = false;
                    result.Errors.Add("生成的代码缺少基本的命名空间或类定义");
                }

                // 检查大括号匹配
                var openBraces = Regex.Matches(code, @"\{").Count;
                var closeBraces = Regex.Matches(code, @"\}").Count;
                if (openBraces != closeBraces)
                {
                    result.IsValid = false;
                    result.Errors.Add($"大括号不匹配：开括号{openBraces}个，闭括号{closeBraces}个");
                }

                // 检查using语句
                if (!code.Contains("using"))
                {
                    result.Warnings.Add("代码中没有using语句，可能缺少必要的引用");
                }

                Logger.Info($"代码验证完成，结果：{(result.IsValid ? "通过" : "失败")}");
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"代码验证过程中发生错误：{ex.Message}");
                Logger.Error($"代码验证失败：{ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// 保存生成的代码到文件
        /// </summary>
        /// <param name="code">代码内容</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="overwrite">是否覆盖现有文件</param>
        /// <returns>保存结果</returns>
        public bool SaveCodeToFile(string code, string filePath, bool overwrite = false)
        {
            try
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (File.Exists(filePath) && !overwrite)
                {
                    Logger.Info($"文件已存在且不允许覆盖：{filePath}");
                    return false;
                }

                File.WriteAllText(filePath, code, Encoding.UTF8);
                Logger.Info($"代码已保存到文件：{filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"保存代码到文件失败：{ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// 代码验证结果
    /// </summary>
    public class CodeValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    /// <summary>
    /// 代码生成选项
    /// </summary>
    public class CodeGenerationOptions
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// 项目前缀
        /// </summary>
        public string StartName { get; set; } = "HDPro";

        /// <summary>
        /// 是否生成Partial类
        /// </summary>
        public bool GeneratePartial { get; set; } = true;

        /// <summary>
        /// 输出目录
        /// </summary>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// 是否覆盖现有文件
        /// </summary>
        public bool OverwriteExisting { get; set; } = false;

        /// <summary>
        /// 自定义模板路径（可选）
        /// </summary>
        public string CustomTemplatePath { get; set; }
    }
} 