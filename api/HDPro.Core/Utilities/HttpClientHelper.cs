using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HDPro.Core.Utilities
{
    /// <summary>
    /// HTTP客户端帮助类
    /// </summary>
    public class HttpClientHelper
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpClientHelper> _logger;

        public HttpClientHelper(HttpClient httpClient, ILogger<HttpClientHelper> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// 发送GET请求
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <returns>响应结果</returns>
        public async Task<T> GetAsync<T>(string url) where T : class
        {
            try
            {
                _logger.LogInformation("发送GET请求: {Url}", url);
                
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("GET请求响应: {StatusCode}, 内容: {Content}", 
                    response.StatusCode, content);

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<T>(content);
                }
                
                _logger.LogWarning("GET请求失败: {StatusCode}, 内容: {Content}", 
                    response.StatusCode, content);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GET请求异常: {Url}", url);
                throw;
            }
        }

        /// <summary>
        /// 发送POST请求
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="data">请求数据</param>
        /// <returns>响应结果</returns>
        public async Task<T> PostAsync<T>(string url, object data) where T : class
        {
            try
            {
                _logger.LogInformation("发送POST请求: {Url}, 数据: {Data}", url, JsonConvert.SerializeObject(data));
                
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("POST请求响应: {StatusCode}, 内容: {Content}", 
                    response.StatusCode, responseContent);

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<T>(responseContent);
                }
                
                _logger.LogWarning("POST请求失败: {StatusCode}, 内容: {Content}", 
                    response.StatusCode, responseContent);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "POST请求异常: {Url}", url);
                throw;
            }
        }

        /// <summary>
        /// 发送POST请求并返回字符串
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="data">请求数据</param>
        /// <returns>响应字符串</returns>
        public async Task<string> PostStringAsync(string url, object data)
        {
            try
            {
                _logger.LogInformation("发送POST字符串请求: {Url}, 数据: {Data}", url, JsonConvert.SerializeObject(data));
                
                HttpContent content = null;
                if (data != null)
                {
                    var json = JsonConvert.SerializeObject(data);
                    content = new StringContent(json, Encoding.UTF8, "application/json");
                }
                
                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("POST字符串请求响应: {StatusCode}, 内容: {Content}", 
                    response.StatusCode, responseContent);

                return responseContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "POST字符串请求异常: {Url}", url);
                throw;
            }
        }
    }
} 