using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using HDPro.Entity.AttributeManager;
using HDPro.Entity.SystemModels;
using HDPro.Core.Utilities;
using HDPro.Core.Configuration;
using HDPro.CY.Order.Services.K3Cloud;
using HDPro.CY.Order.IRepositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HDPro.Core.Controllers.Basic;
using HDPro.Core.ManageUser;

namespace HDPro.CY.Order.Controllers
{
    /// <summary>
    /// BOM查询控制器
    /// 提供BOM展开、物料查询、图纸查询等功能
    /// </summary>
    [Route("api/BomQuery")]
    [ApiController]
    [PermissionTable(Name = "BomQuery")]
    public class BomQueryController : VolController
    {
        private readonly IK3CloudService _k3CloudService;
        private readonly IOCP_MaterialRepository _materialRepository;
        private readonly HttpClientHelper _httpClientHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<BomQueryController> _logger;

        public BomQueryController(
            IK3CloudService k3CloudService,
            IOCP_MaterialRepository materialRepository,
            HttpClientHelper httpClientHelper,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment webHostEnvironment,
            ILogger<BomQueryController> logger)
        {
            _k3CloudService = k3CloudService;
            _materialRepository = materialRepository;
            _httpClientHelper = httpClientHelper;
            _httpContextAccessor = httpContextAccessor;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        #region BOM展开接口

        /// <summary>
        /// 根据物料编码查询BOM展开结果
        /// </summary>
        /// <param name="materialNumber">物料编码</param>
        /// <returns>BOM展开结果</returns>
        [HttpGet("ExpandBom")]
        public async Task<IActionResult> ExpandBom([FromQuery] string materialNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(materialNumber))
                {
                    return Json(new WebResponseContent().Error("物料编码不能为空"));
                }

                _logger.LogInformation("开始BOM展开查询，物料编码: {MaterialNumber}", materialNumber);

                var result = await _k3CloudService.ExpandBomAsync(materialNumber);

                if (result.IsSuccess && result.Data != null && result.Data.Count > 0)
                {
                    _logger.LogInformation("BOM展开查询成功，物料编码: {MaterialNumber}，返回 {Count} 条数据",
                        materialNumber, result.Data.Count);

                    return Json(new WebResponseContent().OK(result.Message, result.Data));
                }
                else
                {
                    _logger.LogWarning("BOM展开查询未返回数据，物料编码: {MaterialNumber}，原因: {Message}，尝试从物料表查询",
                        materialNumber, result.Message);

                    // BOM展开失败或无数据，从物料表查询当前物料作为根节点返回
                    var material = await _materialRepository.FindAsyncFirst(m => m.MaterialCode == materialNumber);

                    if (material == null)
                    {
                        _logger.LogWarning("物料表中也未找到该物料，物料编码: {MaterialNumber}", materialNumber);
                        return Json(new WebResponseContent().Error($"未找到物料信息：{materialNumber}"));
                    }

                    // 构建根节点BOM项
                    var rootBomItem = new HDPro.CY.Order.Services.K3Cloud.Models.BomExpandItemDto
                    {
                        BomLevel = 0,
                        Number = material.MaterialCode,
                        Name = material.MaterialName,
                        Numerator = 1,
                        Denominator = 1,
                        Specification = material.SpecModel,
                        ParentEntryId = "",
                        EntryId = "1",
                        UnitNumber = material.BasicUnit,
                        UnitName = material.BasicUnit
                    };

                    var bomList = new List<HDPro.CY.Order.Services.K3Cloud.Models.BomExpandItemDto> { rootBomItem };

                    _logger.LogInformation("从物料表构建根节点成功，物料编码: {MaterialNumber}，物料名称: {MaterialName}",
                        materialNumber, material.MaterialName);

                    return Json(new WebResponseContent().OK("查询成功（该物料无BOM结构）", bomList));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BOM展开查询发生异常，物料编码: {MaterialNumber}", materialNumber);
                return Json(new WebResponseContent().Error($"BOM展开查询异常：{ex.Message}"));
            }
        }

        #endregion

        #region 物料查询接口

        /// <summary>
        /// 根据物料编码查询物料基本数据
        /// </summary>
        /// <param name="materialCode">物料编码</param>
        /// <returns>物料基本数据</returns>
        [HttpGet("GetMaterial")]
        public async Task<IActionResult> GetMaterial([FromQuery] string materialCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(materialCode))
                {
                    return Json(new WebResponseContent().Error("物料编码不能为空"));
                }

                _logger.LogInformation("开始查询物料数据，物料编码: {MaterialCode}", materialCode);

                // 从数据库查询物料信息
                var material = await _materialRepository.FindAsyncFirst(m => m.MaterialCode == materialCode);
                
                if (material == null)
                {
                    _logger.LogWarning("未找到物料数据，物料编码: {MaterialCode}", materialCode);
                    return Json(new WebResponseContent().Error("未找到该物料信息"));
                }

                _logger.LogInformation("查询物料数据成功，物料编码: {MaterialCode}，物料名称: {MaterialName}", 
                    materialCode, material.MaterialName);
                
                return Json(new WebResponseContent().OK("查询成功", material));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询物料数据发生异常，物料编码: {MaterialCode}", materialCode);
                return Json(new WebResponseContent().Error($"查询物料数据异常：{ex.Message}"));
            }
        }

        #endregion

        #region 图纸查询接口

        /// <summary>
        /// 查询图纸预览URL
        /// </summary>
        /// <param name="materialCode">物料编码</param>
        /// <returns>图纸预览URL</returns>
        [HttpGet]
        [Route("GetDrawing")]
        public async Task<IActionResult> GetDrawing(string materialCode)
        {
            var result = await QueryDrawingInternal(materialCode);

            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }

            // 标准接口返回基础信息
            return Json(new
            {
                success = true,
                data = new
                {
                    materialCode = result.MaterialCode,
                    previewUrl = result.PreviewUrl,
                    downloadUrl = result.DownloadUrl,
                    lastModified = result.LastModified
                }
            });
        }

        /// <summary>
        /// 内部图纸查询方法
        /// </summary>
        private async Task<DrawingQueryResult> QueryDrawingInternal(string materialCode)
        {
            var result = new DrawingQueryResult
            {
                Success = false,
                MaterialCode = materialCode,
                RequestUser = UserContext.Current.UserName,
                RequestUserId = UserContext.Current.UserId
            };

            try
            {
                if (string.IsNullOrWhiteSpace(materialCode))
                {
                    result.Message = "物料编码不能为空";
                    return result;
                }

                _logger.LogInformation("开始查询图纸，物料编码: {MaterialCode}, 用户: {User}",
                    materialCode, result.RequestUser);

                // 获取图纸API地址
                var drawingApiUrl = AppSetting.AppUrls?.DrawingApiUrl;
                if (string.IsNullOrWhiteSpace(drawingApiUrl))
                {
                    result.Message = "图纸API地址未配置";
                    _logger.LogError("图纸API地址未配置");
                    return result;
                }

                // 调用第三方图纸API
                var apiUrl = $"{drawingApiUrl}?code={materialCode}";
                _logger.LogInformation("调用图纸API: {ApiUrl}", apiUrl);

                var apiResponse = await _httpClientHelper.GetAsync<DrawingApiResponse>(apiUrl, 30);

                if (apiResponse == null)
                {
                    result.Message = "图纸API返回为空";
                    _logger.LogWarning("图纸API返回为空，物料编码: {MaterialCode}", materialCode);
                    return result;
                }

                // 检查响应代码（200表示成功）
                if (apiResponse.Code != 200)
                {
                    result.Message = apiResponse.Msg ?? "图纸API查询失败";
                    _logger.LogWarning("图纸API查询失败，物料编码: {MaterialCode}，Code: {Code}，原因: {Message}",
                        materialCode, apiResponse.Code, result.Message);
                    return result;
                }

                if (apiResponse.Data == null)
                {
                    result.Message = "未找到该物料的图纸";
                    _logger.LogWarning("未找到图纸，物料编码: {MaterialCode}", materialCode);
                    return result;
                }

                // 获取图纸数据
                var drawingData = apiResponse.Data;
                if (drawingData.Url == null || string.IsNullOrWhiteSpace(drawingData.Url.Url))
                {
                    result.Message = "未找到图纸信息";
                    _logger.LogWarning("图纸URL为空，物料编码: {MaterialCode}", materialCode);
                    return result;
                }

                // 获取PDF文件URL
                var pdfUrl = drawingData.Url.Url;
                _logger.LogInformation("找到图纸PDF，物料编码: {MaterialCode}，URL: {PdfUrl}",
                    materialCode, pdfUrl);

                // 下载并缓存PDF
                var cachedPath = await DownloadAndCachePdf(pdfUrl, materialCode);
                if (string.IsNullOrWhiteSpace(cachedPath))
                {
                    result.Message = "PDF文件下载失败";
                    return result;
                }

                // 构建预览URL
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                result.PreviewUrl = $"{baseUrl}/{cachedPath}";
                result.DownloadUrl = result.PreviewUrl;
                result.LastModified = drawingData.Url.LastModified;
                result.Success = true;
                result.Message = "查询成功";

                _logger.LogInformation("图纸查询成功，物料编码: {MaterialCode}，预览URL: {PreviewUrl}",
                    materialCode, result.PreviewUrl);

                return result;
            }
            catch (Exception ex)
            {
                result.Message = $"查询图纸异常：{ex.Message}";
                _logger.LogError(ex, "查询图纸发生异常，物料编码: {MaterialCode}", materialCode);
                return result;
            }
        }

        /// <summary>
        /// 下载并缓存PDF文件
        /// </summary>
        private async Task<string> DownloadAndCachePdf(string pdfUrl, string materialCode)
        {
            try
            {
                // 缓存目录
                var cacheDir = Path.Combine(_webHostEnvironment.WebRootPath, "cache", "drawings");
                if (!Directory.Exists(cacheDir))
                {
                    Directory.CreateDirectory(cacheDir);
                }

                // 使用MD5生成文件名，避免特殊字符问题
                var fileName = $"{GetMD5Hash(materialCode)}.pdf";
                var filePath = Path.Combine(cacheDir, fileName);
                var relativePath = $"cache/drawings/{fileName}";

                // 检查缓存是否存在且未过期（24小时）
                if (System.IO.File.Exists(filePath))
                {
                    var fileInfo = new FileInfo(filePath);
                    if (DateTime.Now - fileInfo.LastWriteTime < TimeSpan.FromHours(24))
                    {
                        _logger.LogInformation("使用缓存的PDF文件，物料编码: {MaterialCode}", materialCode);
                        return relativePath;
                    }
                }

                _logger.LogInformation("开始下载PDF文件，物料编码: {MaterialCode}，URL: {PdfUrl}",
                    materialCode, pdfUrl);

                // 下载PDF文件（5分钟超时）
                using (var httpClient = new System.Net.Http.HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromMinutes(5);
                    var pdfBytes = await httpClient.GetByteArrayAsync(pdfUrl);
                    await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);
                }

                _logger.LogInformation("PDF文件下载成功，物料编码: {MaterialCode}，保存路径: {FilePath}",
                    materialCode, filePath);

                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "下载PDF文件失败，物料编码: {MaterialCode}，URL: {PdfUrl}",
                    materialCode, pdfUrl);
                return null;
            }
        }

        /// <summary>
        /// 计算MD5哈希
        /// </summary>
        private string GetMD5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(bytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        #endregion
    }

    #region DTO模型

    /// <summary>
    /// 第三方图纸接口响应模型
    /// </summary>
    public class DrawingApiResponse
    {
        /// <summary>
        /// 响应消息
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 响应代码
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 响应数据
        /// </summary>
        public DrawingData Data { get; set; }
    }

    /// <summary>
    /// 图纸数据模型
    /// </summary>
    public class DrawingData
    {
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 图纸URL信息
        /// </summary>
        public DrawingUrl Url { get; set; }

        /// <summary>
        /// 背景图URL信息
        /// </summary>
        public DrawingUrl BGurl { get; set; }
    }

    /// <summary>
    /// 图纸URL模型
    /// </summary>
    public class DrawingUrl
    {
        /// <summary>
        /// 图纸下载URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public string LastModified { get; set; }
    }

    /// <summary>
    /// 图纸查询结果模型
    /// </summary>
    public class DrawingQueryResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string MaterialCode { get; set; }
        public string PreviewUrl { get; set; }
        public string DownloadUrl { get; set; }
        public string LastModified { get; set; }
        public string RequestUser { get; set; }
        public int? RequestUserId { get; set; }
    }

    #endregion
}

