/*
 *所有关于OCP_TechManagement类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*OCP_TechManagementService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using HDPro.CY.Order.Services;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;
using System.Linq;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
using HDPro.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.CY.Order.IRepositories;
using System.Threading.Tasks;
using System.Text;
using System.Xml;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using HDPro.Core.Configuration;
using HDPro.Core.Enums;
using System;
using HDPro.CY.Order.Services.Common;
using HDPro.CY.Order.IServices;

namespace HDPro.CY.Order.Services
{
    /// <summary>
    /// TC系统物料信息模型
    /// </summary>
    public class TCMaterialInfo
    {
        /// <summary>
        /// 物料ID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        [JsonProperty("创建日期")]
        public string CreateDate { get; set; }

        /// <summary>
        /// 所有者（负责人）
        /// </summary>
        [JsonProperty("所有者")]
        public string Owner { get; set; }

        /// <summary>
        /// 发布日期
        /// </summary>
        [JsonProperty("发布日期")]
        public string PublishDate { get; set; }

        /// <summary>
        /// 获取负责人姓名（从所有者字段中提取）
        /// </summary>
        public string ResponsiblePerson
        {
            get
            {
                if (string.IsNullOrEmpty(Owner))
                    return string.Empty;

                // 从 "陈林 (011660)" 格式中提取姓名
                var parts = Owner.Split(' ');
                return parts.Length > 0 ? parts[0] : Owner;
            }
        }

        /// <summary>
        /// 负责人真实姓名（从所有者字段中提取）
        /// </summary>
        public string OwnerUserTrueName
        {
            get
            {
                if (string.IsNullOrEmpty(Owner))
                    return string.Empty;

                // 从 "陈林 (011660)" 格式中提取姓名部分
                var parts = Owner.Split(' ');
                return parts.Length > 0 ? parts[0].Trim() : Owner;
            }
        }

        /// <summary>
        /// 负责人用户名/工号（从所有者字段中提取）
        /// </summary>
        public string OwnerUserName
        {
            get
            {
                if (string.IsNullOrEmpty(Owner))
                    return string.Empty;

                // 从 "陈林 (011660)" 格式中提取工号部分
                var startIndex = Owner.IndexOf('(');
                var endIndex = Owner.IndexOf(')');

                if (startIndex >= 0 && endIndex > startIndex)
                {
                    return Owner.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();
                }

                return string.Empty;
            }
        }
    }

    public partial class OCP_TechManagementService
    {
        private readonly IOCP_TechManagementRepository _repository;//访问数据库
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OCP_TechManagementService> _logger;
        private readonly TCSystemOptions _tcOptions;
        private readonly IOCP_AlertRulesRepository _alertRulesRepository;

        [ActivatorUtilitiesConstructor]
        public OCP_TechManagementService(
            IOCP_TechManagementRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            ILogger<OCP_TechManagementService> logger,
            IOptions<TCSystemOptions> tcOptions,
            IOCP_AlertRulesRepository alertRulesRepository
            )
        : base(dbRepository, httpContextAccessor)
        {
            _repository = dbRepository;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _tcOptions = tcOptions.Value;
            _alertRulesRepository = alertRulesRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        /// <summary>
        /// 重写CY.Order项目特有的初始化逻辑
        /// 可在此处添加OCP_TechManagement特有的初始化代码
        /// </summary>
        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
            // 在此处添加OCP_TechManagement特有的初始化逻辑
        }

        /// <summary>
        /// 根据物料编码获取技术管理负责人
        /// 调用TC系统WebService接口
        /// </summary>
        /// <param name="materialCode">物料编码</param>
        /// <returns>负责人信息</returns>
        public async Task<WebResponseContent> GetTechManagerByMaterialCodeAsync(string materialCode)
        {
            if (string.IsNullOrWhiteSpace(materialCode))
            {
                return WebResponseContent.Instance.Error("物料编码不能为空");
            }

            // 验证TC系统配置
            if (!_tcOptions.IsValid())
            {
                _logger.LogError("TC系统配置无效，请检查配置文件中的TC节点配置");
                return WebResponseContent.Instance.Error("TC系统配置无效");
            }

            try
            {
                _logger.LogInformation($"开始调用TC系统获取技术管理负责人，物料编码: {materialCode}");

                // 从配置文件获取TC系统WebService接口地址
                var tcServiceUrl = _tcOptions.WebServiceUrl;

                // 构建SOAP请求XML
                var soapRequest = BuildSoapRequest(materialCode);

                using var httpClient = _httpClientFactory.CreateClient();

                // 设置请求超时
                httpClient.Timeout = _tcOptions.TimeoutSpan;

                // 设置请求头
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("SOAPAction", "");

                // 创建请求内容
                var content = new StringContent(soapRequest, Encoding.UTF8, "application/xml");

                _logger.LogInformation($"发送SOAP请求到TC系统: {tcServiceUrl}，超时时间: {_tcOptions.Timeout}秒");
                _logger.LogDebug($"SOAP请求内容: {soapRequest}");

                // 发送POST请求
                var response = await httpClient.PostAsync(tcServiceUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"TC系统响应状态码: {response.StatusCode}");
                _logger.LogDebug($"TC系统响应内容: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    // 解析SOAP响应
                    var materialInfo = ParseSoapResponse(responseContent);

                    if (materialInfo != null)
                    {
                        _logger.LogInformation($"成功获取技术管理信息，物料编码: {materialCode}，负责人: {materialInfo.Owner}");
                        return WebResponseContent.Instance.OKData(materialInfo);
                    }
                    else
                    {
                        _logger.LogWarning($"TC系统返回空的物料信息，物料编码: {materialCode}");
                        return WebResponseContent.Instance.Error("未找到对应的物料信息");
                    }
                }
                else
                {
                    _logger.LogError($"TC系统调用失败，状态码: {response.StatusCode}，响应: {responseContent}");
                    return WebResponseContent.Instance.Error($"TC系统调用失败: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"调用TC系统获取技术管理负责人异常，物料编码: {materialCode}");
                return WebResponseContent.Instance.Error($"获取技术管理负责人失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 构建SOAP请求XML
        /// </summary>
        /// <param name="materialCode">物料编码</param>
        /// <returns>SOAP请求XML字符串</returns>
        private string BuildSoapRequest(string materialCode)
        {
            var soapEnvelope = $@"<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""
                                    xmlns:ns=""http://wsdl.webservice.nancal.com/"">
    <soap:Body>
        <ns:receiveErpCode>
            <value>{materialCode}</value>
        </ns:receiveErpCode>
    </soap:Body>
</soap:Envelope>";

            return soapEnvelope;
        }

        /// <summary>
        /// 批量获取技术负责人信息
        /// </summary>
        /// <param name="materialCodes">物料编码列表</param>
        /// <returns>物料编码与技术负责人信息的字典</returns>
        private async Task<Dictionary<string, TCMaterialInfo>> GetTechManagerBatchAsync(List<string> materialCodes)
        {
            var result = new Dictionary<string, TCMaterialInfo>();

            if (materialCodes == null || !materialCodes.Any())
            {
                return result;
            }

            _logger.LogInformation($"开始批量获取技术负责人信息，物料编码数量: {materialCodes.Count}");

            // 为了避免单次请求过多物料编码导致超时，分批处理
            const int batchSize = 10; // 每批处理10个物料编码
            var batches = materialCodes
                .Select((code, index) => new { code, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.code).ToList())
                .ToList();

            foreach (var batch in batches)
            {
                try
                {
                    // 并发处理当前批次的物料编码
                    var tasks = batch.Select(async materialCode =>
                    {
                        try
                        {
                            var response = await GetTechManagerByMaterialCodeAsync(materialCode);
                            if (response.Status && response.Data is TCMaterialInfo techInfo)
                            {
                                return new { MaterialCode = materialCode, TechInfo = techInfo };
                            }
                            else
                            {
                                _logger.LogWarning($"获取物料编码 {materialCode} 的技术负责人失败: {response.Message}");
                                return null;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"获取物料编码 {materialCode} 的技术负责人异常");
                            return null;
                        }
                    });

                    var batchResults = await Task.WhenAll(tasks);

                    // 将成功获取的结果添加到字典中
                    foreach (var batchResult in batchResults.Where(r => r != null))
                    {
                        if (batchResult != null)
                        {
                            result[batchResult.MaterialCode] = batchResult.TechInfo;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"批量处理技术负责人信息异常，当前批次物料编码: {string.Join(",", batch)}");
                }
            }

            _logger.LogInformation($"批量获取技术负责人信息完成，成功获取 {result.Count}/{materialCodes.Count} 条");

            return result;
        }

        /// <summary>
        /// 解析SOAP响应XML，提取物料信息
        /// </summary>
        /// <param name="soapResponse">SOAP响应XML</param>
        /// <returns>物料信息对象</returns>
        private TCMaterialInfo? ParseSoapResponse(string soapResponse)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(soapResponse);

                // 根据实际的SOAP响应格式解析
                // 响应格式：
                // <soap:Envelope>
                //   <soap:Body>
                //     <ns2:receiveErpCodeResponse>
                //       <return>[{"ID":"04003000129","Name":"阀杆","创建日期":"2020-12-06 15:58","所有者":"陈林 (011660)","发布日期":"2021-2-03 13:54"}]</return>
                //     </ns2:receiveErpCodeResponse>
                //   </soap:Body>
                // </soap:Envelope>

                var namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
                namespaceManager.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
                namespaceManager.AddNamespace("ns2", "http://wsdl.webservice.nancal.com/");

                // 查找return节点
                var returnNode = xmlDoc.SelectSingleNode("//soap:Body//return", namespaceManager);

                if (returnNode == null)
                {
                    // 尝试不使用命名空间
                    returnNode = xmlDoc.SelectSingleNode("//return");
                }

                if (returnNode != null)
                {
                    var jsonContent = returnNode.InnerText?.Trim();
                    _logger.LogDebug($"从SOAP响应中提取的JSON内容: {jsonContent}");

                    if (!string.IsNullOrEmpty(jsonContent))
                    {
                        // 解析JSON数组
                        var materialList = JsonConvert.DeserializeObject<List<TCMaterialInfo>>(jsonContent);

                        if (materialList != null && materialList.Count > 0)
                        {
                            var materialInfo = materialList[0]; // 取第一个结果
                            _logger.LogDebug($"成功解析物料信息: ID={materialInfo.ID}, Name={materialInfo.Name}, Owner={materialInfo.Owner}");
                            return materialInfo;
                        }
                        else
                        {
                            _logger.LogWarning("JSON解析结果为空或无数据");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("return节点内容为空");
                    }
                }
                else
                {
                    _logger.LogWarning($"无法在SOAP响应中找到return节点，响应内容: {soapResponse}");
                }

                return null;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, $"JSON解析异常，响应内容: {soapResponse}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"解析SOAP响应异常，响应内容: {soapResponse}");
                return null;
            }
        }

        /// <summary>
        /// 重写CY.Order项目通用数据验证方法
        /// 可在此处添加OCP_TechManagement特有的数据验证逻辑
        /// </summary>
        /// <param name="entity">要验证的实体</param>
        /// <returns>验证结果</returns>
        protected override WebResponseContent ValidateCYOrderEntity(OCP_TechManagement entity)
        {
            var response = base.ValidateCYOrderEntity(entity);
            
            // 在此处添加OCP_TechManagement特有的数据验证逻辑
            
            return response;
        }

        //查询(主表合计)
        public override PageGridData<OCP_TechManagement> GetPageData(PageDataOptions options)
        {
            //此处是从前台提交的原生的查询条件，这里可以自己过滤
            string 字段 = "";
            QueryRelativeList = (List<SearchParameters> parameters) =>
            {
                // 添加默认查询条件：HasBOM=0
                //bool hasHasBOMCondition = parameters.Any(p => p.Name == "HasBOM");
                //if (!hasHasBOMCondition)
                //{
                //    parameters.Add(new SearchParameters
                //    {
                //        Name = "HasBOM",
                //        Value = "0",
                //        DisplayType = "selectList"
                //    });
                //}
            };

            // 添加物料编码和订单状态过滤条件
            QueryRelativeExpression = (IQueryable<OCP_TechManagement> queryable) =>
            {
                // 只查询物料编码以010、BJ或WX开头的记录，且订单状态为"正常"
                queryable = queryable.Where(x => (x.MaterialNumber.StartsWith("010") ||
                                                  x.MaterialNumber.StartsWith("BJ") ||
                                                  x.MaterialNumber.StartsWith("WX")) &&
                                                 x.OrderStatus == "正常");
                return queryable;
            };

            //查询完成后，在返回页面前可对查询的数据进行操作
            GetPageDataOnExecuted = (PageGridData<OCP_TechManagement> grid) =>
            {
                //可对查询的结果的数据操作
                List<OCP_TechManagement> list = grid.rows;

                // 根据物料编码获取技术负责人信息
                if (list != null && list.Any())
                {
                    // 计算每条记录的超期天数
                    foreach (var record in list)
                    {
                        record.OverdueDays = CalculateTechManagementOverdueDays(record);
                        record.IsSpecialContract = record.StandardDays.HasValue && record.StandardDays > 3 ? 1 : 0;
                    }
                    // 获取所有不为空的物料编码
                    var materialCodes = list
                        .Where(x => !string.IsNullOrWhiteSpace(x.MaterialNumber))
                        .Select(x => x.MaterialNumber)
                        .Distinct()
                        .ToList();

                    if (materialCodes.Any())
                    {
                        // 批量获取技术负责人信息
                        var techManagerDict = GetTechManagerBatchAsync(materialCodes).GetAwaiter().GetResult();

                        // 为每条记录设置技术负责人信息
                        foreach (var record in list)
                        {
                            if (!string.IsNullOrWhiteSpace(record.MaterialNumber) &&
                                techManagerDict.TryGetValue(record.MaterialNumber, out var techInfo))
                            {
                                // 将技术负责人信息设置到Designer字段
                                record.Designer = techInfo.OwnerUserTrueName;
                            }
                        }
                    }

                    // 应用预警标记
                    ApplyAlertWarningToData(list);
                }
            };

            //EF:查询table界面显示合计（需要与前端开发文档上的【table显示合计】一起使用）
            SummaryExpress = (IQueryable<OCP_TechManagement> queryable) =>
            {
                return queryable.GroupBy(x => 1).Select(x => new
                {
                    //注意大小写和数据库字段大小写一样
                    SalesQty = x.Sum(o => o.SalesQty ?? 0),
                    StandardDays = x.Sum(o => o.StandardDays ?? 0),
                    OverdueDays = x.Sum(o => o.OverdueDays ?? 0)
                })
                .FirstOrDefault();
            };
            return base.GetPageData(options);
        }

        /// <summary>
        /// 计算技术管理超期天数
        /// 计算规则：是否有BOM=否，并且当前时间大于要求完工日期，计算超期多少天
        /// </summary>
        /// <param name="record">技术管理记录</param>
        /// <returns>超期天数</returns>
        private int CalculateTechManagementOverdueDays(OCP_TechManagement record)
        {
            try
            {
                // 如果有BOM，则不计算超期天数
                if (record.HasBOM.HasValue && record.HasBOM.Value != 0)
                {
                    return 0;
                }

                // 如果没有要求完工日期，无法计算超期天数
                if (!record.RequiredFinishTime.HasValue)
                {
                    return 0;
                }

                DateTime requiredFinishTime = record.RequiredFinishTime.Value;
                DateTime currentTime = DateTime.Now;

                // 当前时间大于要求完工日期，计算超期天数
                if (currentTime > requiredFinishTime)
                {
                    int daysDiff = (int)(currentTime.Date - requiredFinishTime.Date).TotalDays;
                    return Math.Max(0, daysDiff);
                }

                // 未超期
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "计算技术管理超期天数时发生异常，技术ID：{TechID}", record.TechID);
                return 0;
            }
        }

        /// <summary>
        /// 批量根据物料编码获取技术管理负责人
        /// </summary>
        /// <param name="materialCodes">物料编码列表</param>
        /// <returns>批量负责人信息</returns>
        public async Task<WebResponseContent> BatchGetTechManagerByMaterialCodeAsync(List<string> materialCodes)
        {
            if (materialCodes == null || !materialCodes.Any())
            {
                return WebResponseContent.Instance.Error("物料编码列表不能为空");
            }

            // 去重并过滤空值
            var validMaterialCodes = materialCodes.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
            if (!validMaterialCodes.Any())
            {
                return WebResponseContent.Instance.Error("没有有效的物料编码");
            }

            try
            {
                _logger.LogInformation($"开始批量获取技术管理负责人，物料编码数量: {validMaterialCodes.Count}");

                // 调用现有的批量获取方法
                var techManagerDict = await GetTechManagerBatchAsync(validMaterialCodes);

                // 构建响应结果
                var results = new List<object>();
                foreach (var materialCode in validMaterialCodes)
                {
                    if (techManagerDict.TryGetValue(materialCode, out var techInfo))
                    {
                        results.Add(new
                        {
                            MaterialCode = materialCode,
                            Found = true,
                            ResponsibleLoginName = techInfo.OwnerUserName,
                            ResponsibleName = techInfo.OwnerUserTrueName,
                            MaterialInfo = techInfo,
                            ErrorMessage = (string)null
                        });
                    }
                    else
                    {
                        results.Add(new
                        {
                            MaterialCode = materialCode,
                            Found = false,
                            ResponsibleLoginName = (string)null,
                            ResponsibleName = (string)null,
                            MaterialInfo = (TCMaterialInfo)null,
                            ErrorMessage = "未找到对应的技术负责人信息"
                        });
                    }
                }

                var response = new
                {
                    TotalCount = validMaterialCodes.Count,
                    SuccessCount = techManagerDict.Count,
                    FailedCount = validMaterialCodes.Count - techManagerDict.Count,
                    Results = results,
                    QueryTime = DateTime.Now
                };

                _logger.LogInformation($"批量获取技术管理负责人完成，成功: {techManagerDict.Count}/{validMaterialCodes.Count}");

                return WebResponseContent.Instance.OK("批量获取技术管理负责人成功", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"批量获取技术管理负责人异常，物料编码数量: {validMaterialCodes.Count}");
                return WebResponseContent.Instance.Error($"批量获取技术管理负责人失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取近14天的BOM未搭建数量统计
        /// </summary>
        /// <returns>近14天每日BOM未搭建数量统计数据</returns>
        public async Task<List<object>> GetLast14DaysBomUnbuiltTrend()
        {
            try
            {
                _logger.LogInformation("开始获取近14天BOM未搭建数量统计");

                // 计算近14天的日期范围
                var endDate = DateTime.Now.Date;
                var startDate = endDate.AddDays(-13); // 包含今天共14天

                // 获取近14天的BOM未搭建数据，只统计订单状态为"正常"的记录
                var bomUnbuiltData = await _repository.FindAsIQueryable(x => x.CreateDate >= startDate && x.CreateDate <= endDate.AddDays(1))
                    .Where(x => x.HasBOM == 0) // HasBOM=0表示BOM未搭建
                    .Where(x => x.OrderStatus == "正常") // 只统计订单状态为"正常"的记录
                    .Where(x => x.MaterialNumber.StartsWith("010") || 
                               x.MaterialNumber.StartsWith("BJ") || 
                               x.MaterialNumber.StartsWith("WX")) // 只统计特定物料编码开头的记录
                    .Select(x => new
                    {
                        Date = x.CreateDate.Value.Date,
                        TechID = x.TechID
                    })
                    .ToListAsync();

                // 按日期分组统计
                var dailyStats = new List<object>();

                for (int i = 0; i < 14; i++)
                {
                    var currentDate = startDate.AddDays(i);
                    
                    // 统计当天的BOM未搭建数量
                    var dayCount = bomUnbuiltData.Count(x => x.Date == currentDate);

                    dailyStats.Add(new
                    {
                        date = currentDate.ToString("yyyy.MM.dd"),
                        BOM未搭建数量 = dayCount
                    });
                }

                _logger.LogInformation($"获取近14天BOM未搭建数量统计完成，共统计 {bomUnbuiltData.Count} 条记录（仅统计订单状态为正常的记录）");

                return dailyStats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取近14天BOM未搭建数量统计异常");
                throw; // 重新抛出异常，让控制器处理
            }
        }

        /// <summary>
        /// 获取BOM搭建情况统计
        /// </summary>
        /// <returns>BOM搭建情况统计数据</returns>
        public async Task<object[]> GetBomBuildStatusSummary()
        {
            try
            {
                _logger.LogInformation("开始获取BOM搭建情况统计");

                // 查询所有符合条件的技术管理记录，只统计订单状态为"正常"的记录
                var allRecords = await _repository.FindAsIQueryable(x => true)
                    .Where(x => x.OrderStatus == "正常") // 只统计订单状态为"正常"的记录
                    .Where(x => x.MaterialNumber.StartsWith("010") || 
                               x.MaterialNumber.StartsWith("BJ") || 
                               x.MaterialNumber.StartsWith("WX")) // 只统计特定物料编码开头的记录
                    .Select(x => new
                    {
                        HasBOM = x.HasBOM
                    })
                    .ToListAsync();

                // 统计已搭建和未搭建的数量
                var builtCount = allRecords.Count(x => x.HasBOM == 1); // HasBOM=1表示已搭建
                var unbuiltCount = allRecords.Count(x => x.HasBOM == 0); // HasBOM=0表示未搭建

                // 构建返回结果
                var result = new object[]
                {
                    new { value = builtCount, name = "已搭建" },
                    new { value = unbuiltCount, name = "未搭建" }
                };

                _logger.LogInformation($"获取BOM搭建情况统计完成，已搭建: {builtCount}，未搭建: {unbuiltCount}（仅统计订单状态为正常的记录）");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取BOM搭建情况统计异常");
                throw; // 重新抛出异常，让控制器处理
            }
        }

        /// <summary>
        /// 应用预警标记到数据列表
        /// </summary>
        /// <param name="list">数据列表</param>
        private void ApplyAlertWarningToData(List<OCP_TechManagement> list)
        {
            try
            {
                // 获取当前页面的预警规则
                
                var rules = _alertRulesRepository.FindAsync(x =>
                    x.AlertPage == "OCP_TechManagement" &&
                    x.TaskStatus == 1).GetAwaiter().GetResult();

                if (rules != null && rules.Any())
                {
                    // 应用预警标记
                    AlertWarningHelper.ApplyAlertWarning(list, rules.ToList(), _logger);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "应用预警标记时发生异常");
                // 不抛出异常,避免影响正常查询
            }
        }
  }
}