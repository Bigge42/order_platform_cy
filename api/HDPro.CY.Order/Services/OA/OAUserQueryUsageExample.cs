/*
 * 股份OA人员查询使用示例
 * 演示如何根据人员工号获取股份OA人员信息
 */
using HDPro.CY.Order.IServices.OA;
using HDPro.CY.Order.Services.OA;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HDPro.CY.Order.Services.OA
{
    /// <summary>
    /// 股份OA人员查询使用示例
    /// </summary>
    public class OAUserQueryUsageExample
    {
        private readonly IOAIntegrationService _oaIntegrationService;
        private readonly ILogger<OAUserQueryUsageExample> _logger;

        public OAUserQueryUsageExample(
            IOAIntegrationService oaIntegrationService,
            ILogger<OAUserQueryUsageExample> logger)
        {
            _oaIntegrationService = oaIntegrationService;
            _logger = logger;
        }

        /// <summary>
        /// 示例1：根据人员工号获取股份OA人员信息
        /// </summary>
        /// <param name="employeeCode">人员工号</param>
        /// <returns></returns>
        public async Task<bool> GetUserInfoByCodeExample(string employeeCode)
        {
            try
            {
                _logger.LogInformation("开始查询股份OA人员信息 - 工号: {EmployeeCode}", employeeCode);

                // 调用股份OA人员查询接口
                var result = await _oaIntegrationService.GetShareholderOAUserByCodeAsync(employeeCode);

                if (result.Status)
                {
                    var userInfo = result.Data as ShareholderOAUserInfo;
                    if (userInfo != null)
                    {
                        _logger.LogInformation("查询成功 - 人员信息:");
                        _logger.LogInformation("  人员ID: {UserId}", userInfo.Id);
                        _logger.LogInformation("  姓名: {UserName}", userInfo.Name);
                        _logger.LogInformation("  工号: {EmployeeCode}", userInfo.Code);
                        _logger.LogInformation("  登录名: {LoginName}", userInfo.LoginName);
                        _logger.LogInformation("  部门: {Department}", userInfo.OrgDepartmentName);
                        _logger.LogInformation("  岗位: {Position}", userInfo.OrgPostName);
                        _logger.LogInformation("  级别: {Level}", userInfo.OrgLevelName);
                        _logger.LogInformation("  电话: {Phone}", userInfo.TelNumber);
                        _logger.LogInformation("  邮箱: {Email}", userInfo.EmailAddress);
                        _logger.LogInformation("  是否有效: {IsValid}", userInfo.Valid);
                        _logger.LogInformation("  是否可登录: {IsLoginable}", userInfo.IsLoginable);

                        return true;
                    }
                }
                else
                {
                    _logger.LogWarning("查询失败: {Message}", result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询股份OA人员信息时发生异常");
            }

            return false;
        }

        /// <summary>
        /// 示例2：使用自定义Token查询人员信息
        /// </summary>
        /// <param name="employeeCode">人员工号</param>
        /// <param name="customToken">自定义Token</param>
        /// <returns></returns>
        public async Task<bool> GetUserInfoWithCustomTokenExample(string employeeCode, string customToken)
        {
            try
            {
                _logger.LogInformation("使用自定义Token查询股份OA人员信息 - 工号: {EmployeeCode}", employeeCode);

                // 使用自定义Token调用股份OA人员查询接口
                var result = await _oaIntegrationService.GetShareholderOAUserByCodeAsync(employeeCode, customToken);

                if (result.Status)
                {
                    var userInfo = result.Data as ShareholderOAUserInfo;
                    if (userInfo != null)
                    {
                        _logger.LogInformation("使用自定义Token查询成功 - 人员ID: {UserId}, 姓名: {UserName}", 
                            userInfo.Id, userInfo.Name);
                        return true;
                    }
                }
                else
                {
                    _logger.LogWarning("使用自定义Token查询失败: {Message}", result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "使用自定义Token查询股份OA人员信息时发生异常");
            }

            return false;
        }

        /// <summary>
        /// 示例3：批量查询多个人员信息
        /// </summary>
        /// <param name="employeeCodes">人员工号列表</param>
        /// <returns></returns>
        public async Task<int> BatchGetUserInfoExample(string[] employeeCodes)
        {
            var successCount = 0;

            try
            {
                _logger.LogInformation("开始批量查询股份OA人员信息 - 总数: {TotalCount}", employeeCodes.Length);

                foreach (var employeeCode in employeeCodes)
                {
                    var result = await _oaIntegrationService.GetShareholderOAUserByCodeAsync(employeeCode);

                    if (result.Status)
                    {
                        var userInfo = result.Data as ShareholderOAUserInfo;
                        if (userInfo != null)
                        {
                            _logger.LogInformation("批量查询成功 - 工号: {EmployeeCode}, 姓名: {UserName}, 人员ID: {UserId}", 
                                employeeCode, userInfo.Name, userInfo.Id);
                            successCount++;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("批量查询失败 - 工号: {EmployeeCode}, 错误: {Message}", 
                            employeeCode, result.Message);
                    }

                    // 避免请求过于频繁，添加延迟
                    await Task.Delay(100);
                }

                _logger.LogInformation("批量查询完成 - 成功: {SuccessCount}, 失败: {FailCount}", 
                    successCount, employeeCodes.Length - successCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量查询股份OA人员信息时发生异常");
            }

            return successCount;
        }

        /// <summary>
        /// 示例4：验证人员是否存在并获取基本信息
        /// </summary>
        /// <param name="employeeCode">人员工号</param>
        /// <returns>返回人员基本信息，如果不存在则返回null</returns>
        public async Task<(long? UserId, string UserName, string Department)> ValidateUserExistsExample(string employeeCode)
        {
            try
            {
                _logger.LogInformation("验证人员是否存在 - 工号: {EmployeeCode}", employeeCode);

                var result = await _oaIntegrationService.GetShareholderOAUserByCodeAsync(employeeCode);

                if (result.Status)
                {
                    var userInfo = result.Data as ShareholderOAUserInfo;
                    if (userInfo != null && userInfo.Valid && userInfo.IsLoginable)
                    {
                        _logger.LogInformation("人员存在且有效 - 工号: {EmployeeCode}, 姓名: {UserName}", 
                            employeeCode, userInfo.Name);
                        return (userInfo.Id, userInfo.Name, userInfo.OrgDepartmentName);
                    }
                    else if (userInfo != null)
                    {
                        _logger.LogWarning("人员存在但无效 - 工号: {EmployeeCode}, 有效性: {IsValid}, 可登录: {IsLoginable}", 
                            employeeCode, userInfo.Valid, userInfo.IsLoginable);
                    }
                }
                else
                {
                    _logger.LogWarning("人员不存在 - 工号: {EmployeeCode}, 错误: {Message}", 
                        employeeCode, result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证人员是否存在时发生异常 - 工号: {EmployeeCode}", employeeCode);
            }

            return (null, null, null);
        }

        /// <summary>
        /// 示例5：获取人员详细属性信息
        /// </summary>
        /// <param name="employeeCode">人员工号</param>
        /// <returns></returns>
        public async Task<bool> GetUserDetailPropertiesExample(string employeeCode)
        {
            try
            {
                _logger.LogInformation("获取人员详细属性信息 - 工号: {EmployeeCode}", employeeCode);

                var result = await _oaIntegrationService.GetShareholderOAUserByCodeAsync(employeeCode);

                if (result.Status)
                {
                    var userInfo = result.Data as ShareholderOAUserInfo;
                    if (userInfo != null && userInfo.Properties != null)
                    {
                        _logger.LogInformation("人员详细属性信息:");
                        _logger.LogInformation("  基本信息 - 姓名: {Name}, 性别: {Gender}", 
                            userInfo.Name, userInfo.Properties.Gender == 1 ? "男" : "女");
                        _logger.LogInformation("  联系方式 - 电话: {Phone}, 办公电话: {OfficePhone}, 邮箱: {Email}", 
                            userInfo.Properties.TelNumber, userInfo.Properties.OfficeNumber, userInfo.Properties.EmailAddress);
                        _logger.LogInformation("  工作信息 - 入职日期: {HireDate}, 学历: {Degree}", 
                            userInfo.Properties.HireDate, userInfo.Properties.Degree);
                        _logger.LogInformation("  地址信息 - 地址: {Address}, 邮编: {PostalCode}", 
                            userInfo.Properties.Address, userInfo.Properties.PostalCode);
                        _logger.LogInformation("  社交信息 - 微信: {Weixin}, 微博: {Weibo}", 
                            userInfo.Properties.Weixin, userInfo.Properties.Weibo);

                        return true;
                    }
                }
                else
                {
                    _logger.LogWarning("获取人员详细属性信息失败: {Message}", result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取人员详细属性信息时发生异常");
            }

            return false;
        }

        /// <summary>
        /// 示例6：演示常见的使用场景
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CommonUsageScenarioExample()
        {
            try
            {
                _logger.LogInformation("开始演示常见使用场景");

                // 场景1：根据示例中的工号查询人员信息
                var employeeCode = "015206";
                var result = await GetUserInfoByCodeExample(employeeCode);

                if (result)
                {
                    _logger.LogInformation("场景1完成：成功获取工号 {EmployeeCode} 的人员信息", employeeCode);
                }

                // 场景2：验证多个常见工号
                var commonCodes = new[] { "015206", "000001", "000002", "999999" };
                var validCount = 0;

                foreach (var code in commonCodes)
                {
                    var (userId, userName, department) = await ValidateUserExistsExample(code);
                    if (userId.HasValue)
                    {
                        validCount++;
                        _logger.LogInformation("有效人员 - 工号: {Code}, 姓名: {Name}, 部门: {Dept}", 
                            code, userName, department);
                    }
                }

                _logger.LogInformation("场景2完成：验证了 {TotalCount} 个工号，其中 {ValidCount} 个有效", 
                    commonCodes.Length, validCount);

                return validCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "演示常见使用场景时发生异常");
            }

            return false;
        }
    }
} 