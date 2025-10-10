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
using System.Text;

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
            _logger.LogInformation("开始手动更新循环仓库看板数据(硬编码)…");

            try
            {
                // ① 硬编码 ESB 地址
                // 已确认该接口必须 POST 才能返回数据
                const string apiUrl = "http://10.11.0.101:8003/gateway/DataCenter/XHCKKBSearch";
                _logger.LogInformation("调用接口(POST): {Url}", apiUrl);

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromMinutes(5);

                // ② 以 application/json POST 一个空对象 {}（很多 ESB 要求有 JSON 体）
                var content = new StringContent("{}", Encoding.UTF8, "application/json");
                var resp = await client.PostAsync(apiUrl, content);
                var respText = await resp.Content.ReadAsStringAsync();

                _logger.LogInformation("接口响应: {StatusCode}, 内容前200字: {Text}",
                    resp.StatusCode,
                    respText != null && respText.Length > 200 ? respText.Substring(0, 200) : respText);

                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogError("接口调用失败，状态码: {Status}, 返回: {Body}", resp.StatusCode, respText);
                    return responseContent.Error($"接口调用失败，状态码: {resp.StatusCode}");
                }

                // ③ 反序列化：兼容两种格式（纯数组 或 包一层 data）
                List<XhckkbRecord> newRecords = null;
                try
                {
                    var trimmed = (respText ?? "").TrimStart();
                    if (trimmed.StartsWith("["))
                    {
                        newRecords = JsonConvert.DeserializeObject<List<XhckkbRecord>>(respText);
                    }
                    else
                    {
                        dynamic wrapper = JsonConvert.DeserializeObject(respText);
                        if (wrapper != null && wrapper.data != null)
                        {
                            newRecords = JsonConvert.DeserializeObject<List<XhckkbRecord>>(wrapper.data.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ESB JSON 解析失败。");
                    return responseContent.Error("接口返回数据格式不正确（JSON 解析失败）");
                }

                if (newRecords == null)
                {
                    _logger.LogWarning("接口返回数据为空或不匹配。");
                    return responseContent.Error("接口返回数据为空或格式不正确");
                }

                _logger.LogInformation("接口返回 {Count} 条记录，准备写库。", newRecords.Count);

                // ④ 清空并写入（事务保证原子性）
                using var tran = _dbContext.Database.BeginTransaction();
                try
                {
                    _logger.LogInformation("清空表 XhckkbRecord…");
                    _dbContext.Database.ExecuteSqlRaw("DELETE FROM XhckkbRecord");

                    _dbContext.Set<XhckkbRecord>().AddRange(newRecords);
                    _dbContext.SaveChanges();

                    tran.Commit();
                    _logger.LogInformation("写入完成，共插入 {Count} 条。", newRecords.Count);
                    return responseContent.OK($"更新成功，插入 {newRecords.Count} 条数据");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    _logger.LogError(ex, "写库失败，已回滚。");
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
