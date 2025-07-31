using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace HDPro.Core.Utilities
{
    /// <summary>
    /// 安全文件读取器 - 专门用于读取可能被其他进程占用的文件
    /// </summary>
    public class SafeFileReader
    {
        private readonly ILogger<SafeFileReader> _logger;
        private const int MaxRetryAttempts = 3;
        private const int RetryDelayMs = 100;

        public SafeFileReader(ILogger<SafeFileReader> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 安全读取文件所有行
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="encoding">编码格式，默认UTF8</param>
        /// <returns>文件内容行数组</returns>
        public async Task<string[]> ReadAllLinesAsync(string filePath, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            
            for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
            {
                try
                {
                    return await ReadAllLinesInternalAsync(filePath, encoding);
                }
                catch (IOException ex) when (ex.Message.Contains("being used by another process") && attempt < MaxRetryAttempts)
                {
                    _logger.LogWarning($"文件被占用，第 {attempt} 次重试读取: {filePath}");
                    await Task.Delay(RetryDelayMs * attempt); // 递增延迟
                }
                catch (UnauthorizedAccessException ex) when (attempt < MaxRetryAttempts)
                {
                    _logger.LogWarning($"文件访问被拒绝，第 {attempt} 次重试读取: {filePath}");
                    await Task.Delay(RetryDelayMs * attempt);
                }
            }

            // 最后一次尝试，如果失败则抛出异常
            try
            {
                return await ReadAllLinesInternalAsync(filePath, encoding);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"经过 {MaxRetryAttempts} 次重试后仍无法读取文件: {filePath}");
                throw new InvalidOperationException($"无法读取文件 {Path.GetFileName(filePath)}，文件可能正在被使用或访问被拒绝", ex);
            }
        }

        /// <summary>
        /// 安全读取文件字节
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件字节数组</returns>
        public async Task<byte[]> ReadAllBytesAsync(string filePath)
        {
            for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
            {
                try
                {
                    return await ReadAllBytesInternalAsync(filePath);
                }
                catch (IOException ex) when (ex.Message.Contains("being used by another process") && attempt < MaxRetryAttempts)
                {
                    _logger.LogWarning($"文件被占用，第 {attempt} 次重试读取: {filePath}");
                    await Task.Delay(RetryDelayMs * attempt);
                }
                catch (UnauthorizedAccessException ex) when (attempt < MaxRetryAttempts)
                {
                    _logger.LogWarning($"文件访问被拒绝，第 {attempt} 次重试读取: {filePath}");
                    await Task.Delay(RetryDelayMs * attempt);
                }
            }

            // 最后一次尝试
            try
            {
                return await ReadAllBytesInternalAsync(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"经过 {MaxRetryAttempts} 次重试后仍无法读取文件: {filePath}");
                throw new InvalidOperationException($"无法读取文件 {Path.GetFileName(filePath)}，文件可能正在被使用或访问被拒绝", ex);
            }
        }

        /// <summary>
        /// 同步版本 - 安全读取文件所有行
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="encoding">编码格式，默认UTF8</param>
        /// <returns>文件内容行数组</returns>
        public string[] ReadAllLines(string filePath, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            
            for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
            {
                try
                {
                    return ReadAllLinesInternal(filePath, encoding);
                }
                catch (IOException ex) when (ex.Message.Contains("being used by another process") && attempt < MaxRetryAttempts)
                {
                    _logger.LogWarning($"文件被占用，第 {attempt} 次重试读取: {filePath}");
                    Thread.Sleep(RetryDelayMs * attempt);
                }
                catch (UnauthorizedAccessException ex) when (attempt < MaxRetryAttempts)
                {
                    _logger.LogWarning($"文件访问被拒绝，第 {attempt} 次重试读取: {filePath}");
                    Thread.Sleep(RetryDelayMs * attempt);
                }
            }

            // 最后一次尝试
            try
            {
                return ReadAllLinesInternal(filePath, encoding);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"经过 {MaxRetryAttempts} 次重试后仍无法读取文件: {filePath}");
                throw new InvalidOperationException($"无法读取文件 {Path.GetFileName(filePath)}，文件可能正在被使用或访问被拒绝", ex);
            }
        }

        /// <summary>
        /// 同步版本 - 安全读取文件字节
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件字节数组</returns>
        public byte[] ReadAllBytes(string filePath)
        {
            for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
            {
                try
                {
                    return ReadAllBytesInternal(filePath);
                }
                catch (IOException ex) when (ex.Message.Contains("being used by another process") && attempt < MaxRetryAttempts)
                {
                    _logger.LogWarning($"文件被占用，第 {attempt} 次重试读取: {filePath}");
                    Thread.Sleep(RetryDelayMs * attempt);
                }
                catch (UnauthorizedAccessException ex) when (attempt < MaxRetryAttempts)
                {
                    _logger.LogWarning($"文件访问被拒绝，第 {attempt} 次重试读取: {filePath}");
                    Thread.Sleep(RetryDelayMs * attempt);
                }
            }

            // 最后一次尝试
            try
            {
                return ReadAllBytesInternal(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"经过 {MaxRetryAttempts} 次重试后仍无法读取文件: {filePath}");
                throw new InvalidOperationException($"无法读取文件 {Path.GetFileName(filePath)}，文件可能正在被使用或访问被拒绝", ex);
            }
        }

        #region 私有方法

        private async Task<string[]> ReadAllLinesInternalAsync(string filePath, Encoding encoding)
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var reader = new StreamReader(fileStream, encoding);
            
            var lines = new List<string>();
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                lines.Add(line);
            }
            
            return lines.ToArray();
        }

        private async Task<byte[]> ReadAllBytesInternalAsync(string filePath)
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var memoryStream = new MemoryStream();
            
            await fileStream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        private string[] ReadAllLinesInternal(string filePath, Encoding encoding)
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var reader = new StreamReader(fileStream, encoding);
            
            var lines = new List<string>();
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                lines.Add(line);
            }
            
            return lines.ToArray();
        }

        private byte[] ReadAllBytesInternal(string filePath)
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var memoryStream = new MemoryStream();
            
            fileStream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        #endregion
    }
} 