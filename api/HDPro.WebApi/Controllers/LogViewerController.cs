using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using HDPro.Core.Controllers.Basic;
using HDPro.Core.ManageUser;
using HDPro.Core.Utilities;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HDPro.WebApi.Controllers
{
    /// <summary>
    /// 日志查看器控制器 - 提供安全的日志文件访问
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 需要授权才能访问
    public class LogViewerController : ControllerBase
    {
        private readonly ILogger<LogViewerController> _logger;
        private readonly SafeFileReader _safeFileReader;
        private readonly string _logsPath;

        public LogViewerController(ILogger<LogViewerController> logger, SafeFileReader safeFileReader, IWebHostEnvironment environment)
        {
            _logger = logger;
            _safeFileReader = safeFileReader;
            // 在调试模式下，日志文件位于运行目录下的logs文件夹
            // 例如：D:\WorkSpace\Web\Vol.Pro\Haodee.CY.Order.OCP\api\HDPro.WebApi\bin\Debug\net8.0\logs
            _logsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        }



        /// <summary>
        /// 获取日志文件列表（包括子目录）
        /// </summary>
        /// <returns></returns>
        [HttpGet("files")]
        public IActionResult GetLogFiles()
        {
            try
            {
                _logger.LogInformation($"开始获取日志文件列表，用户：{GetCurrentUserName()}，日志路径：{_logsPath}");

                // 检查用户权限（只有管理员可以查看日志）
                if (!IsAdminUser())
                {
                    _logger.LogWarning($"用户 {GetCurrentUserName()} 尝试访问日志文件但权限不足");
                    return Forbid("只有管理员可以查看日志文件");
                }

                if (!Directory.Exists(_logsPath))
                {
                    _logger.LogWarning($"日志目录不存在：{_logsPath}");
                    return Ok(new { 
                        files = new string[0], 
                        message = "日志目录不存在"
                    });
                }

                // 递归获取logs目录及其子目录中的所有日志文件
                var allLogFiles = GetLogFilesRecursively(_logsPath);
                var files = allLogFiles
                    .Select(f => new
                    {
                        name = GetRelativeFileName(f, _logsPath),
                        fullPath = f,
                        size = new FileInfo(f).Length,
                        lastModified = new FileInfo(f).LastWriteTime,
                        type = GetLogFileType(Path.GetFileName(f)),
                        directory = GetRelativeDirectory(f, _logsPath)
                    })
                    .OrderBy(f => f.directory)
                    .ThenByDescending(f => f.lastModified)
                    .ToList();

                _logger.LogInformation($"用户 {GetCurrentUserName()} 查看了日志文件列表，找到 {files.Count} 个文件");

                return Ok(new { files, count = files.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取日志文件列表失败");
                return StatusCode(500, new { 
                    message = "获取日志文件列表失败", 
                    error = ex.Message
                });
            }
        }



        /// <summary>
        /// 下载日志文件
        /// </summary>
        /// <param name="fileName">文件名（支持子目录路径）</param>
        /// <returns></returns>
        [HttpGet("download/{*fileName}")]
        public IActionResult DownloadLogFile(string fileName)
        {
            try
            {
                _logger.LogInformation($"开始下载日志文件：{fileName}，用户：{GetCurrentUserName()}");

                // 检查用户权限
                if (!IsAdminUser())
                {
                    _logger.LogWarning($"用户 {GetCurrentUserName()} 尝试下载日志文件但权限不足");
                    return Forbid("只有管理员可以下载日志文件");
                }

                // 验证文件路径安全性（支持子目录）
                if (!IsValidLogFilePath(fileName))
                {
                    _logger.LogWarning($"无效的文件路径：{fileName}");
                    return BadRequest("无效的文件路径");
                }

                var filePath = Path.Combine(_logsPath, fileName.Replace('/', Path.DirectorySeparatorChar));
                _logger.LogInformation($"尝试访问文件：{filePath}");
                
                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning($"日志文件不存在：{filePath}");
                    return NotFound($"日志文件不存在：{filePath}");
                }

                _logger.LogInformation($"用户 {GetCurrentUserName()} 下载了日志文件: {fileName}");

                // 使用SafeFileReader安全读取文件
                var fileBytes = _safeFileReader.ReadAllBytes(filePath);
                return File(fileBytes, "text/plain", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"下载日志文件失败: {fileName}");
                return StatusCode(500, new { 
                    message = "下载日志文件失败", 
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 查看日志文件内容（分页）
        /// </summary>
        /// <param name="fileName">文件名（支持子目录路径）</param>
        /// <param name="page">页码（从1开始）</param>
        /// <param name="pageSize">每页行数</param>
        /// <param name="search">搜索关键词</param>
        /// <returns></returns>
        [HttpGet("view/{*fileName}")]
        public IActionResult ViewLogFile(string fileName, int page = 1, int pageSize = 100, string search = "")
        {
            try
            {
                _logger.LogInformation($"开始查看日志文件：{fileName}，页码：{page}，用户：{GetCurrentUserName()}");

                // 检查用户权限
                if (!IsAdminUser())
                {
                    _logger.LogWarning($"用户 {GetCurrentUserName()} 尝试查看日志文件但权限不足");
                    return Forbid("只有管理员可以查看日志文件");
                }

                // 验证文件路径安全性（支持子目录）
                if (!IsValidLogFilePath(fileName))
                {
                    _logger.LogWarning($"无效的文件路径：{fileName}");
                    return BadRequest("无效的文件路径");
                }

                var filePath = Path.Combine(_logsPath, fileName.Replace('/', Path.DirectorySeparatorChar));
                _logger.LogInformation($"尝试访问文件：{filePath}");
                
                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning($"日志文件不存在：{filePath}");
                    return NotFound($"日志文件不存在：{filePath}");
                }

                // 限制页面大小
                pageSize = Math.Min(pageSize, 1000);
                page = Math.Max(page, 1);

                _logger.LogInformation($"开始读取文件内容：{filePath}");
                
                // 使用SafeFileReader安全读取文件
                var lines = _safeFileReader.ReadAllLines(filePath);
                _logger.LogInformation($"文件读取完成，总行数：{lines.Length}");
                
                // 搜索过滤
                if (!string.IsNullOrEmpty(search))
                {
                    lines = lines.Where(line => line.Contains(search, StringComparison.OrdinalIgnoreCase)).ToArray();
                    _logger.LogInformation($"搜索过滤后行数：{lines.Length}");
                }

                // 分页
                var totalLines = lines.Length;
                var totalPages = (int)Math.Ceiling((double)totalLines / pageSize);
                var startIndex = (page - 1) * pageSize;
                var pageLines = lines.Skip(startIndex).Take(pageSize).ToArray();

                _logger.LogInformation($"用户 {GetCurrentUserName()} 查看了日志文件: {fileName}, 页码: {page}");

                return Ok(new
                {
                    fileName,
                    page,
                    pageSize,
                    totalLines,
                    totalPages,
                    search,
                    lines = pageLines,
                    fileInfo = new
                    {
                        size = new FileInfo(filePath).Length,
                        lastModified = new FileInfo(filePath).LastWriteTime
                    },
                    debugInfo = new
                    {
                        filePath = filePath,
                        logsPath = _logsPath
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查看日志文件失败: {fileName}");
                return StatusCode(500, new { 
                    message = "查看日志文件失败", 
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 获取日志文件的最新内容（实时日志）
        /// </summary>
        /// <param name="fileName">文件名（支持子目录路径）</param>
        /// <param name="lines">获取最后几行，默认50行</param>
        /// <returns></returns>
        [HttpGet("tail/{*fileName}")]
        public IActionResult TailLogFile(string fileName, int lines = 50)
        {
            try
            {
                _logger.LogInformation($"开始获取实时日志：{fileName}，行数：{lines}，用户：{GetCurrentUserName()}");

                // 检查用户权限
                if (!IsAdminUser())
                {
                    _logger.LogWarning($"用户 {GetCurrentUserName()} 尝试查看实时日志但权限不足");
                    return Forbid("只有管理员可以查看日志文件");
                }

                // 验证文件路径安全性（支持子目录）
                if (!IsValidLogFilePath(fileName))
                {
                    _logger.LogWarning($"无效的文件路径：{fileName}");
                    return BadRequest("无效的文件路径");
                }

                var filePath = Path.Combine(_logsPath, fileName.Replace('/', Path.DirectorySeparatorChar));
                _logger.LogInformation($"尝试访问文件：{filePath}");
                
                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning($"日志文件不存在：{filePath}");
                    return NotFound($"日志文件不存在：{filePath}");
                }

                // 限制行数
                lines = Math.Min(lines, 1000);

                // 使用SafeFileReader安全读取文件
                var allLines = _safeFileReader.ReadAllLines(filePath);
                var tailLines = allLines.TakeLast(lines).ToArray();

                return Ok(new
                {
                    fileName,
                    requestedLines = lines,
                    actualLines = tailLines.Length,
                    totalLines = allLines.Length,
                    lines = tailLines,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取日志文件尾部内容失败: {fileName}");
                return StatusCode(500, new { 
                    message = "获取日志文件尾部内容失败", 
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 清理旧日志文件
        /// </summary>
        /// <param name="daysToKeep">保留天数</param>
        /// <returns></returns>
        [HttpDelete("cleanup")]
        public IActionResult CleanupOldLogs(int daysToKeep = 30)
        {
            try
            {
                // 检查用户权限（只有超级管理员可以清理日志）
                if (!IsSuperAdminUser())
                {
                    return Forbid("只有超级管理员可以清理日志文件");
                }

                if (!Directory.Exists(_logsPath))
                {
                    return Ok(new { message = "日志目录不存在", deletedFiles = 0 });
                }

                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                var files = Directory.GetFiles(_logsPath, "*.log");
                var deletedFiles = new List<string>();

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.LastWriteTime < cutoffDate)
                    {
                        System.IO.File.Delete(file);
                        deletedFiles.Add(Path.GetFileName(file));
                    }
                }

                _logger.LogWarning($"用户 {GetCurrentUserName()} 清理了 {deletedFiles.Count} 个旧日志文件，保留天数: {daysToKeep}");

                return Ok(new
                {
                    message = $"清理完成，删除了 {deletedFiles.Count} 个文件",
                    deletedFiles,
                    daysToKeep,
                    cutoffDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理旧日志文件失败");
                return StatusCode(500, new { message = "清理旧日志文件失败", error = ex.Message });
            }
        }

        #region 私有方法

        /// <summary>
        /// 检查是否为管理员用户
        /// </summary>
        /// <returns></returns>
        private bool IsAdminUser()
        {
            try
            {
                var userInfo = UserContext.Current?.UserInfo;
                if (userInfo == null) 
                {
                    _logger.LogWarning("UserContext.Current.UserInfo 为空");
                    return false;
                }

                // 检查是否为超级管理员或具有日志查看权限
                var isAdmin = userInfo.Role_Id == 1 || // 假设1为超级管理员角色ID
                       userInfo.UserTrueName?.Contains("admin", StringComparison.OrdinalIgnoreCase) == true ||
                       User.IsInRole("Admin") ||
                       User.IsInRole("LogViewer");

                _logger.LogInformation($"用户权限检查：用户={userInfo.UserTrueName}, 角色ID={userInfo.Role_Id}, 是否管理员={isAdmin}");
                return isAdmin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查用户权限时发生异常");
                return false;
            }
        }

        /// <summary>
        /// 检查是否为超级管理员用户
        /// </summary>
        /// <returns></returns>
        private bool IsSuperAdminUser()
        {
            try
            {
                var userInfo = UserContext.Current?.UserInfo;
                if (userInfo == null) return false;

                // 只有超级管理员可以清理日志
                return userInfo.Role_Id == 1 || // 假设1为超级管理员角色ID
                       User.IsInRole("SuperAdmin");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取当前用户名
        /// </summary>
        /// <returns></returns>
        private string GetCurrentUserName()
        {
            try
            {
                return UserContext.Current?.UserInfo?.UserTrueName ?? 
                       User.Identity?.Name ?? 
                       "未知用户";
            }
            catch
            {
                return "未知用户";
            }
        }

        /// <summary>
        /// 验证日志文件名是否安全（仅文件名，不包含路径）
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool IsValidLogFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            // 检查文件名是否包含危险字符
            var invalidChars = Path.GetInvalidFileNameChars();
            if (fileName.Any(c => invalidChars.Contains(c)))
                return false;

            // 检查是否为相对路径攻击
            if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
                return false;

            // 只允许.log文件
            if (!fileName.EndsWith(".log", StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        /// <summary>
        /// 验证日志文件路径是否安全（支持子目录）
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private bool IsValidLogFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            // 检查是否为相对路径攻击
            if (filePath.Contains(".."))
                return false;

            // 统一路径分隔符
            var normalizedPath = filePath.Replace('\\', '/');

            // 检查路径是否包含危险字符
            var invalidPathChars = Path.GetInvalidPathChars();
            if (normalizedPath.Any(c => invalidPathChars.Contains(c)))
                return false;

            // 只允许.log文件
            if (!normalizedPath.EndsWith(".log", StringComparison.OrdinalIgnoreCase))
                return false;

            // 检查文件是否在logs目录或其子目录中
            var fullPath = Path.Combine(_logsPath, normalizedPath.Replace('/', Path.DirectorySeparatorChar));
            var normalizedFullPath = Path.GetFullPath(fullPath);
            var normalizedLogsPath = Path.GetFullPath(_logsPath);
            
            if (!normalizedFullPath.StartsWith(normalizedLogsPath, StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        /// <summary>
        /// 获取日志文件类型
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string GetLogFileType(string fileName)
        {
            if (fileName.Contains("error", StringComparison.OrdinalIgnoreCase))
                return "错误日志";
            if (fileName.Contains("info", StringComparison.OrdinalIgnoreCase))
                return "信息日志";
            if (fileName.Contains("own", StringComparison.OrdinalIgnoreCase))
                return "应用程序日志";
            if (fileName.Contains("all", StringComparison.OrdinalIgnoreCase))
                return "所有日志";
            return "日志文件";
        }

        /// <summary>
        /// 递归获取指定目录及其子目录中的所有日志文件
        /// </summary>
        /// <param name="directoryPath">目录路径</param>
        /// <returns>日志文件路径列表</returns>
        private List<string> GetLogFilesRecursively(string directoryPath)
        {
            var logFiles = new List<string>();
            
            try
            {
                // 获取当前目录中的日志文件
                var currentDirFiles = Directory.GetFiles(directoryPath, "*.log", SearchOption.TopDirectoryOnly);
                logFiles.AddRange(currentDirFiles);

                // 递归获取子目录中的日志文件
                var subDirectories = Directory.GetDirectories(directoryPath);
                foreach (var subDir in subDirectories)
                {
                    logFiles.AddRange(GetLogFilesRecursively(subDir));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"获取目录 {directoryPath} 中的日志文件时发生异常");
            }

            return logFiles;
        }

        /// <summary>
        /// 获取相对于基础目录的文件名
        /// </summary>
        /// <param name="fullPath">完整文件路径</param>
        /// <param name="basePath">基础目录路径</param>
        /// <returns>相对文件名</returns>
        private string GetRelativeFileName(string fullPath, string basePath)
        {
            try
            {
                var relativePath = Path.GetRelativePath(basePath, fullPath);
                return relativePath.Replace('\\', '/'); // 统一使用正斜杠
            }
            catch
            {
                return Path.GetFileName(fullPath);
            }
        }

        /// <summary>
        /// 获取相对于基础目录的目录名
        /// </summary>
        /// <param name="fullPath">完整文件路径</param>
        /// <param name="basePath">基础目录路径</param>
        /// <returns>相对目录名</returns>
        private string GetRelativeDirectory(string fullPath, string basePath)
        {
            try
            {
                var fileDir = Path.GetDirectoryName(fullPath);
                if (string.IsNullOrEmpty(fileDir))
                    return "";

                var relativePath = Path.GetRelativePath(basePath, fileDir);
                return relativePath == "." ? "根目录" : relativePath.Replace('\\', '/');
            }
            catch
            {
                return "未知目录";
            }
        }

        #endregion
    }
} 