using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HDPro.Core.CacheManager;
using HDPro.Core.Configuration;
using HDPro.Core.Controllers.Basic;
using HDPro.Core.Extensions;
using HDPro.Core.ManageUser;
using HDPro.Core.Utilities;
using HDPro.Entity.DomainModels;
using HDPro.Sys.IRepositories;
using HDPro.Sys.IServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using HDPro.Core.Enums;
using System.Net;

namespace HDPro.WebApi.Controllers.Auth
{
    [Route("api/sso")]
    [ApiController]
    public class SSOController : VolController
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClientHelper _httpClientHelper;
        private readonly ISys_UserService _userService;
        private readonly ISys_UserRepository _userRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<SSOController> _logger;

        public SSOController(
            IConfiguration configuration,
            HttpClientHelper httpClientHelper,
            ISys_UserService userService,
            ISys_UserRepository userRepository,
            ICacheService cacheService,
            ILogger<SSOController> logger)
        {
            _configuration = configuration;
            _httpClientHelper = httpClientHelper;
            _userService = userService;
            _userRepository = userRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        /// <summary>
        /// 获取统一认证的授权URL
        /// </summary>
        /// <returns>返回授权URL</returns>
        [HttpGet("auth-url")]
        [AllowAnonymous]
        public IActionResult GetAuthUrl()
        {
            try
            {
                var ssoConfig = AppSetting.SSOConfig;
                if (string.IsNullOrEmpty(ssoConfig.ServerUrl) || 
                    string.IsNullOrEmpty(ssoConfig.ClientId) || 
                    string.IsNullOrEmpty(ssoConfig.RedirectUri))
                {
                    _logger.LogError("SSO配置缺失，请检查配置文件");
                    return Json(new WebResponseContent().Error("SSO配置缺失，请联系管理员"));
                }

                // 构建授权URL
                string authUrl = $"{ssoConfig.ServerUrl.TrimEnd('/')}{ssoConfig.AuthorizeEndpoint}" +
                                 $"?client_id={ssoConfig.ClientId}" +
                                 $"&response_type=code" +
                                 $"&redirect_uri={Uri.EscapeDataString(ssoConfig.RedirectUri)}";

                _logger.LogInformation("生成SSO授权URL: {Url}", authUrl);
                return Json(new WebResponseContent().OKData(authUrl));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取SSO授权URL出错");
                return Json(new WebResponseContent().Error($"获取SSO授权URL出错: {ex.Message}"));
            }
        }

        /// <summary>
        /// 处理SSO回调，根据code获取token并登录
        /// </summary>
        /// <param name="request">SSO登录请求</param>
        /// <returns>登录结果，包含本系统的token</returns>
        [HttpPost("ssologin")]
        [AllowAnonymous]
        public async Task<IActionResult> SSOLogin([FromBody] SSOLoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Code))
                {
                    return Json(new WebResponseContent().Error("缺少授权码"));
                }

                var ssoConfig = AppSetting.SSOConfig;
                if (string.IsNullOrEmpty(ssoConfig.ServerUrl) ||
                    string.IsNullOrEmpty(ssoConfig.ClientId) ||
                    string.IsNullOrEmpty(ssoConfig.ClientSecret))
                {
                    _logger.LogError("SSO配置缺失，请检查配置文件");
                    return Json(new WebResponseContent().Error("SSO配置缺失，请联系管理员"));
                }

                // 第一步：使用授权码获取访问令牌
                var tokenResponse = await GetAccessTokenAsync(request.Code);
                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    return Json(new WebResponseContent().Error("无法获取访问令牌"));
                }

                // 第二步：使用访问令牌获取用户信息
                var userInfo = await GetUserInfoAsync(tokenResponse.AccessToken);
                if (userInfo == null)
                {
                    return Json(new WebResponseContent().Error("无法获取用户信息"));
                }

                // 第三步：使用用户信息在系统中登录用户
                var loginResult = await LoginUserAsync(userInfo);
                
                return Json(loginResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SSO回调处理失败: {ErrorMessage}", ex.Message);
                return Json(new WebResponseContent().Error($"SSO回调处理失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 获取访问令牌
        /// </summary>
        private async Task<TokenResponse> GetAccessTokenAsync(string code)
        {
            try
            {
                var ssoConfig = AppSetting.SSOConfig;
                
                // 构建带参数的URL（与Postman请求一致）
                string tokenUrl = $"{ssoConfig.ServerUrl.TrimEnd('/')}{ssoConfig.TokenEndpoint}" +
                                  $"?client_id={ssoConfig.ClientId}" +
                                  $"&client_secret={ssoConfig.ClientSecret}" +
                                  $"&grant_type=authorization_code" +
                                  $"&redirect_uri={Uri.EscapeDataString(ssoConfig.RedirectUri)}" +
                                  $"&code={code}";

                _logger.LogInformation("请求SSO访问令牌，URL: {Url}", tokenUrl);

                // 发送POST请求获取访问令牌
                var responseStr = await _httpClientHelper.PostStringAsync(tokenUrl, null);
                var response = JsonConvert.DeserializeObject<TokenResponse>(responseStr);
                
                if (response != null && !string.IsNullOrEmpty(response.AccessToken))
                {
                    _logger.LogInformation("成功获取SSO访问令牌");
                    return response;
                }
                else
                {
                    _logger.LogWarning("获取SSO访问令牌失败，响应内容为空或无效");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取SSO访问令牌时出错");
                throw;
            }
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        private async Task<SSOUserInfo> GetUserInfoAsync(string accessToken)
        {
            try
            {
                var ssoConfig = AppSetting.SSOConfig;

                // 构建带参数的URL（与Postman请求一致）
                string userInfoUrl = $"{ssoConfig.ServerUrl.TrimEnd('/')}{ssoConfig.UserInfoEndpoint}" +
                                  $"?access_token={accessToken}";

                _logger.LogInformation("请求SSO用户信息，URL: {Url}", userInfoUrl);
                
                // 发送请求获取用户信息
                var responseString = await _httpClientHelper.PostStringAsync(userInfoUrl, null);
                
                if (!string.IsNullOrEmpty(responseString))
                {
                    // 尝试解析返回的JSON数据
                    var ssoResponse = JsonConvert.DeserializeObject<SSOResponse>(responseString);
                    if (ssoResponse != null && ssoResponse.Success && ssoResponse.Data != null)
                    {
                        // 提取需要的用户数据字段
                        var userInfo = new SSOUserInfo
                        {
                            UserId = ssoResponse.Data.UserId,
                            User = ssoResponse.Data.User,
                            UserName = ssoResponse.Data.Username,
                            RealName = ssoResponse.Data.RealName,
                            DisplayName = ssoResponse.Data.DisplayName,
                            Gender = ssoResponse.Data.Gender?.ToString(),
                            Mobile = ssoResponse.Data.Mobile,
                            Email = ssoResponse.Data.Email,
                            Department = ssoResponse.Data.Department,
                            Title = ssoResponse.Data.Title,
                            State = ssoResponse.Data.State,
                            OriginalData = responseString
                        };

                        _logger.LogInformation("成功获取SSO用户信息，用户名: {UserName}", userInfo.UserName);
                        return userInfo;
                    }
                }
                
                _logger.LogWarning("获取SSO用户信息失败，响应内容为空或无效");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取SSO用户信息时出错");
                throw;
            }
        }

        /// <summary>
        /// 根据SSO用户信息登录用户
        /// </summary>
        private async Task<WebResponseContent> LoginUserAsync(SSOUserInfo userInfo)
        {
            WebResponseContent response = new WebResponseContent();
            
            try
            {
                // 查找是否已存在该用户
                var user = await _userRepository.FindFirstAsync(x => x.UserName == userInfo.User);
                
                if (user == null)
                {
                    return response.Error(ResponseType.LoginError);
                }

                // 为用户生成JWT令牌
                UserInfo userTokenInfo = new UserInfo
                {
                    User_Id = user.User_Id,
                    UserName = user.UserName,
                    Role_Id = user.Role_Id ?? 0
                };

                int expireMinutes = AppSetting.ExpMinutes;
                string token = JwtHelper.IssueJwt(userTokenInfo, expireMinutes);

                // 更新用户token
                user.Token = token;
                _userRepository.Update(user, x => x.Token, true);

                // 准备返回数据
                string accessToken = null;
                if (AppSetting.FileAuth)
                {
                    expireMinutes = expireMinutes + 30;
                    string dt = DateTime.Now.AddMinutes(expireMinutes).ToString("yyyy-MM-dd HH:mm");
                    accessToken = $"{user.User_Id}_{dt}".EncryptDES(AppSetting.Secret.User);
                    _cacheService.Add(accessToken, dt, expireMinutes);
                }

                var responseData = new
                {
                    token,
                    userName = user.UserTrueName,
                    userId = user.User_Id,
                    img = user.HeadImageUrl,
                    accessToken
                };
                UserContext.Current.LogOut(user.User_Id);
                
                _logger.LogInformation("SSO登录成功，用户名: {UserName}", userInfo.UserName);
                return response.OKData(responseData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SSO用户登录或创建失败: {ErrorMessage}", ex.Message);
                return response.Error($"登录失败: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// SSO回调请求
    /// </summary>
    public class SSOLoginRequest
    {
        /// <summary>
        /// 授权码
        /// </summary>
        public string Code { get; set; }
    }

    /// <summary>
    /// 令牌响应
    /// </summary>
    public class TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        
        [JsonProperty("scope")]
        public string Scope { get; set; }
    }

    /// <summary>
    /// SSO用户信息
    /// </summary>
    public class SSOUserInfo
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string User { get; set; }
        public string RealName { get; set; }
        public string DisplayName { get; set; }
        public string Gender { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string Title { get; set; }
        public string State { get; set; }
        public string OriginalData { get; set; }
    }

    /// <summary>
    /// SSO响应结果
    /// </summary>
    public class SSOResponse
    {
        /// <summary>
        /// 状态码
        /// </summary>
        [JsonProperty("code")]
        public int Code { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        [JsonProperty("success")]
        public bool Success { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// 数据内容
        /// </summary>
        [JsonProperty("data")]
        public SSOUserData Data { get; set; }
    }

    /// <summary>
    /// SSO用户数据
    /// </summary>
    public class SSOUserData
    {
        /// <summary>
        /// 生日
        /// </summary>
        [JsonProperty("birthday")]
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        [JsonProperty("gender")]
        public int? Gender { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// 部门ID
        /// </summary>
        [JsonProperty("departmentId")]
        public string DepartmentId { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        [JsonProperty("mobile")]
        public string Mobile { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        [JsonProperty("createdate")]
        public string CreateDate { get; set; }

        /// <summary>
        /// 职位
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [JsonProperty("userId")]
        public string UserId { get; set; }

        /// <summary>
        /// 在线票据
        /// </summary>
        [JsonProperty("online_ticket")]
        public string OnlineTicket { get; set; }

        /// <summary>
        /// 员工编号
        /// </summary>
        [JsonProperty("employeeNumber")]
        public string EmployeeNumber { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        [JsonProperty("realname")]
        public string RealName { get; set; }

        /// <summary>
        /// 机构
        /// </summary>
        [JsonProperty("institution")]
        public string Institution { get; set; }

        /// <summary>
        /// 随机ID
        /// </summary>
        [JsonProperty("randomId")]
        public string RandomId { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [JsonProperty("state")]
        public string State { get; set; }

        /// <summary>
        /// 部门
        /// </summary>
        [JsonProperty("department")]
        public string Department { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [JsonProperty("user")]
        public string User { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; set; }
    }
} 