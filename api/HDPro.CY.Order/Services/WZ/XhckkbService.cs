using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using HDPro.Core.Utilities;
using HDPro.Core.EFDbContext;
using HDPro.Core.Extensions.AutofacManager;
using Microsoft.EntityFrameworkCore;
using HDPro.CY.Order.IServices.WZ;
using HDPro.Entity.DomainModels.OrderCollaboration;

namespace HDPro.CY.Order.Services.WZ
{
    /// <summary>
    /// 循环仓库看板手动更新服务实现
    /// </summary>
    public class XhckkbService : IXhckkbService, IDependency
    {
        private readonly ServiceDbContext _dbContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<XhckkbService> _logger;

        // 注入 SQLServer数据库上下文、HTTP客户端工厂、日志记录器
        public XhckkbService(ServiceDbContext dbContext, IHttpClientFactory httpClientFactory, ILogger<XhckkbService> logger)
        {
            _dbContext = dbContext;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// 手动更新循环仓库看板数据：调用接口获取最新数据，清空原有表数据并插入新数据。
        /// </summary>
        public async Task<WebResponseContent> ManualUpdateXhckkbAsync()
        {
            var responseContent = new WebResponseContent();
            _logger.LogInformation("开始手动更新循环仓库看板数据...");

            try
            {
                // 1. 调用循环仓库看板第三方接口获取数据
                string apiUrl = "http://10.11.0.101:8003/gateway/DataCenter/XHCKKBSearch";
                _logger.LogInformation("调用接口: {Url}", apiUrl);
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromMinutes(5);  // 可根据需要设置超时时间

                // 发起GET请求（若接口需要POST请求或参数，可调整为PostAsync）
                HttpResponseMessage resp = await client.GetAsync(apiUrl);
                string respJson = await resp.Content.ReadAsStringAsync();
                _logger.LogInformation("接口响应: {StatusCode}, 内容长度: {Length}", resp.StatusCode, respJson.Length);

                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogError("接口调用失败，状态码: {Status}, 返回内容: {Content}", resp.StatusCode, respJson);
                    return responseContent.Error($"接口调用失败，状态码: {resp.StatusCode}");
                }

                // 反序列化JSON为数据列表
                List<XhckkbRecord> newRecords = JsonConvert.DeserializeObject<List<XhckkbRecord>>(respJson);
                if (newRecords == null)
                {
                    _logger.LogWarning("接口返回数据为空或反序列化失败。");
                    return responseContent.Error("接口返回数据为空或格式不正确");
                }

                _logger.LogInformation("接口返回 {Count} 条记录，将更新数据库。", newRecords.Count);

                // 2. 清空原表数据并插入新数据，使用事务保证原子性
                using var transaction = _dbContext.Database.BeginTransaction();
                try
                {
                    // 清空表数据
                    _logger.LogInformation("清空表 XhckkbRecord 原有数据...");
                    _dbContext.Database.ExecuteSqlRaw("DELETE FROM XhckkbRecord");

                    // 批量写入新数据
                    _dbContext.Set<XhckkbRecord>().AddRange(newRecords);
                    _dbContext.SaveChanges();

                    // 提交事务
                    transaction.Commit();
                    _logger.LogInformation("循环仓库看板数据更新成功，共插入 {Count} 条记录。", newRecords.Count);
                    return responseContent.OK($"更新成功，插入 {newRecords.Count} 条数据");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "更新 XhckkbRecord 表时发生异常，正在回滚...");
                    transaction.Rollback();
                    return responseContent.Error($"数据更新失败: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调用循环仓库看板接口过程中出现异常。");
                return responseContent.Error($"接口调用异常: {ex.Message}");
            }
        }
    }
}
