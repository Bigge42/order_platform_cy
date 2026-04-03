using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using HDPro.Entity.AttributeManager;
using HDPro.Entity.SystemModels;
using HDPro.Core.Utilities;
using HDPro.Core.Configuration;
using HDPro.CY.Order.Services.K3Cloud;
using HDPro.CY.Order.Services.K3Cloud.Models;
using HDPro.CY.Order.IRepositories;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using HDPro.Core.Controllers.Basic;
using HDPro.Core.ManageUser;
using HDPro.Entity.DomainModels;

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
        private static readonly HashSet<string> ExportAuthorizedUsers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "cyadmin",
            "002166",
            "013675",
            "004356"
        };

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
        #region 物料信息导出

        [HttpPost("ExportCurrentMaterialInfo")]
        public async Task<IActionResult> ExportCurrentMaterialInfo([FromBody] MaterialExportRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.MaterialCode))
                {
                    return Json(new WebResponseContent().Error("物料编码不能为空"));
                }

                if (!HasExportPermission())
                {
                    return BuildExportPermissionDeniedResult("导出当前物料信息", request.MaterialCode);
                }

                var materialCode = request.MaterialCode.Trim();
                var material = await _materialRepository.FindAsyncFirst(m => m.MaterialCode == materialCode);
                if (material == null)
                {
                    return Json(new WebResponseContent().Error($"未找到物料信息：{materialCode}"));
                }

                var rows = await BuildExportRowsAsync(new List<BomExpandItemDto> { CreateRootBomItem(material) });
                return BuildMaterialExportFile(rows, $"{materialCode}_物料信息_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "导出当前物料信息失败，物料编码: {MaterialCode}", request?.MaterialCode);
                return Json(new WebResponseContent().Error($"导出当前物料信息失败：{ex.Message}"));
            }
        }

        [HttpPost("ExportBomMaterialInfo")]
        public async Task<IActionResult> ExportBomMaterialInfo([FromBody] BomMaterialExportRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.MaterialNumber))
                {
                    return Json(new WebResponseContent().Error("查询物料编码不能为空"));
                }

                if (!HasExportPermission())
                {
                    return BuildExportPermissionDeniedResult("导出BOM全部物料信息", request.MaterialNumber);
                }

                var materialNumber = request.MaterialNumber.Trim();
                var bomItems = await GetBomItemsForExportAsync(materialNumber);
                if (bomItems.Count == 0)
                {
                    return Json(new WebResponseContent().Error($"未找到可导出的BOM物料：{materialNumber}"));
                }

                var rows = await BuildExportRowsAsync(bomItems);
                return BuildMaterialExportFile(rows, $"{materialNumber}_BOM物料信息_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "导出BOM全部物料信息失败，查询物料编码: {MaterialNumber}", request?.MaterialNumber);
                return Json(new WebResponseContent().Error($"导出BOM全部物料信息失败：{ex.Message}"));
            }
        }

        private async Task<List<BomExpandItemDto>> GetBomItemsForExportAsync(string materialNumber)
        {
            var result = await _k3CloudService.ExpandBomAsync(materialNumber);
            if (result.IsSuccess && result.Data != null && result.Data.Count > 0)
            {
                return result.Data;
            }

            var material = await _materialRepository.FindAsyncFirst(m => m.MaterialCode == materialNumber);
            if (material == null)
            {
                return new List<BomExpandItemDto>();
            }

            return new List<BomExpandItemDto> { CreateRootBomItem(material) };
        }

        private bool HasExportPermission()
        {
            var currentUserName = UserContext.Current?.UserName?.Trim();
            return !string.IsNullOrWhiteSpace(currentUserName) && ExportAuthorizedUsers.Contains(currentUserName);
        }

        private IActionResult BuildExportPermissionDeniedResult(string actionName, string materialCode)
        {
            var currentUserName = UserContext.Current?.UserName?.Trim() ?? string.Empty;
            _logger.LogWarning("用户 {UserName} 尝试{ActionName}但无权限，目标物料: {MaterialCode}",
                currentUserName, actionName, materialCode ?? string.Empty);

            return Json(new WebResponseContent().Error("当前账号无导出权限"));
        }

        private static BomExpandItemDto CreateRootBomItem(OCP_Material material)
        {
            return new BomExpandItemDto
            {
                BomLevel = 0,
                Number = material.MaterialCode,
                Name = material.MaterialName,
                Numerator = 1,
                Denominator = 1,
                Specification = material.SpecModel,
                ParentEntryId = string.Empty,
                EntryId = "1",
                UnitNumber = material.BasicUnit,
                UnitName = material.BasicUnit
            };
        }

        private async Task<List<MaterialExportRow>> BuildExportRowsAsync(List<BomExpandItemDto> bomItems)
        {
            var validBomItems = bomItems?
                .Where(item => !string.IsNullOrWhiteSpace(item?.Number))
                .ToList() ?? new List<BomExpandItemDto>();

            if (validBomItems.Count == 0)
            {
                return new List<MaterialExportRow>();
            }

            var materialCodes = validBomItems
                .Select(item => item.Number.Trim())
                .Distinct()
                .ToList();

            var materials = await _materialRepository.FindAsync(m => materialCodes.Contains(m.MaterialCode));
            var materialMap = materials
                .GroupBy(m => m.MaterialCode)
                .ToDictionary(group => group.Key, group => group.First());

            var bomMap = validBomItems
                .Where(item => !string.IsNullOrWhiteSpace(item.EntryId))
                .GroupBy(item => item.EntryId)
                .ToDictionary(group => group.Key, group => group.First());

            return validBomItems.Select(item =>
            {
                materialMap.TryGetValue(item.Number.Trim(), out var material);
                var parentMaterialCode = string.Empty;
                if (!string.IsNullOrWhiteSpace(item.ParentEntryId) && bomMap.TryGetValue(item.ParentEntryId, out var parentItem))
                {
                    parentMaterialCode = parentItem.Number;
                }

                return new MaterialExportRow
                {
                    BomLevel = item.BomLevel,
                    ParentMaterialCode = parentMaterialCode,
                    MaterialCode = item.Number,
                    BomMaterialName = item.Name,
                    Specification = material?.SpecModel ?? item.Specification,
                    Numerator = item.Numerator,
                    Denominator = item.Denominator,
                    UnitNumber = item.UnitNumber,
                    UnitName = item.UnitName,
                    MaterialName = material?.MaterialName ?? item.Name,
                    NominalDiameter = material?.NominalDiameter,
                    NominalPressure = material?.NominalPressure,
                    Cv = material?.CV,
                    FlangeStandard = material?.FlangeStandard,
                    FlangeSealType = material?.FlangeSealType,
                    BodyMaterial = material?.BodyMaterial,
                    TrimMaterial = material?.TrimMaterial,
                    FlowCharacteristic = material?.FlowCharacteristic,
                    PackingForm = material?.PackingForm,
                    FlangeConnection = material?.FlangeConnection,
                    ActuatorModel = material?.ActuatorModel,
                    ActuatorStroke = material?.ActuatorStroke,
                    DrawingNo = material?.DrawingNo,
                    Material = material?.Material,
                    TcReleaser = material?.TCReleaser
                };
            }).ToList();
        }

        private IActionResult BuildMaterialExportFile(List<MaterialExportRow> rows, string fileName)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("物料信息");

            var columns = new List<MaterialExportColumn>
            {
                new("BOM层级", row => row.BomLevel, 12),
                new("父级物料编码", row => row.ParentMaterialCode, 20),
                new("物料编码", row => row.MaterialCode, 20),
                new("BOM名称", row => row.BomMaterialName, 28),
                new("规格型号", row => row.Specification, 24),
                new("分子", row => row.Numerator, 10),
                new("分母", row => row.Denominator, 10),
                new("单位编码", row => row.UnitNumber, 12),
                new("单位名称", row => row.UnitName, 12),
                new("物料名称", row => row.MaterialName, 28),
                new("公称通径", row => row.NominalDiameter, 16),
                new("公称压力", row => row.NominalPressure, 16),
                new("CV", row => row.Cv, 12),
                new("法兰标准", row => row.FlangeStandard, 20),
                new("法兰密封面形式", row => row.FlangeSealType, 22),
                new("阀体材质", row => row.BodyMaterial, 18),
                new("阀内件材质", row => row.TrimMaterial, 18),
                new("流量特性", row => row.FlowCharacteristic, 18),
                new("填料形式", row => row.PackingForm, 18),
                new("法兰连接方式", row => row.FlangeConnection, 20),
                new("执行机构型号", row => row.ActuatorModel, 20),
                new("执行机构行程", row => row.ActuatorStroke, 18),
                new("图号", row => row.DrawingNo, 20),
                new("材质", row => row.Material, 18),
                new("TC发布人", row => row.TcReleaser, 18)
            };

            for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
            {
                var cell = worksheet.Cells[1, columnIndex + 1];
                cell.Value = columns[columnIndex].Header;
                cell.Style.Font.Bold = true;
                cell.Style.Font.Color.SetColor(Color.White);
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(64, 158, 255));
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Column(columnIndex + 1).Width = columns[columnIndex].Width;
            }

            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
                {
                    var cell = worksheet.Cells[rowIndex + 2, columnIndex + 1];
                    cell.Value = columns[columnIndex].ValueSelector(rows[rowIndex]) ?? string.Empty;
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cell.Style.WrapText = true;
                }
            }

            using (var range = worksheet.Cells[1, 1, Math.Max(rows.Count + 1, 2), columns.Count])
            {
                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            worksheet.Row(1).Height = 22;
            worksheet.View.FreezePanes(2, 1);
            worksheet.Cells[1, 1, Math.Max(rows.Count + 1, 2), columns.Count].AutoFilter = true;

            var fileBytes = package.GetAsByteArray();
            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        #endregion

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

                // 检查缓存是否存在且未过期（1分钟）
                if (System.IO.File.Exists(filePath))
                {
                    var fileInfo = new FileInfo(filePath);
                    if (DateTime.Now - fileInfo.LastWriteTime < TimeSpan.FromMinutes(1))
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

    public class MaterialExportRequest
    {
        public string? MaterialCode { get; set; }
    }

    public class BomMaterialExportRequest
    {
        public string? MaterialNumber { get; set; }
    }

    public class MaterialExportRow
    {
        public int BomLevel { get; set; }
        public string? ParentMaterialCode { get; set; }
        public string? MaterialCode { get; set; }
        public string? BomMaterialName { get; set; }
        public string? Specification { get; set; }
        public decimal Numerator { get; set; }
        public decimal Denominator { get; set; }
        public string? UnitNumber { get; set; }
        public string? UnitName { get; set; }
        public string? MaterialName { get; set; }
        public string? NominalDiameter { get; set; }
        public string? NominalPressure { get; set; }
        public string? Cv { get; set; }
        public string? FlangeStandard { get; set; }
        public string? FlangeSealType { get; set; }
        public string? BodyMaterial { get; set; }
        public string? TrimMaterial { get; set; }
        public string? FlowCharacteristic { get; set; }
        public string? PackingForm { get; set; }
        public string? FlangeConnection { get; set; }
        public string? ActuatorModel { get; set; }
        public string? ActuatorStroke { get; set; }
        public string? DrawingNo { get; set; }
        public string? Material { get; set; }
        public string? TcReleaser { get; set; }
    }

    public class MaterialExportColumn
    {
        public MaterialExportColumn(string header, Func<MaterialExportRow, object?> valueSelector, double width)
        {
            Header = header;
            ValueSelector = valueSelector;
            Width = width;
        }

        public string Header { get; }
        public Func<MaterialExportRow, object?> ValueSelector { get; }
        public double Width { get; }
    }

    #endregion
}
