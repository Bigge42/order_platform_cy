/*
 * OA审批人功能使用示例
 * 演示如何在OA流程中设置指定负责人作为审批人
 */
using HDPro.CY.Order.IServices.OA;
using HDPro.CY.Order.Services.OA;
using HDPro.Core.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HDPro.CY.Order.Services.OA
{
    /// <summary>
    /// OA审批人功能使用示例
    /// </summary>
    public class OAApproverUsageExample
    {
        private readonly IOAIntegrationService _oaIntegrationService;
        private readonly ILogger<OAApproverUsageExample> _logger;

        public OAApproverUsageExample(
            IOAIntegrationService oaIntegrationService,
            ILogger<OAApproverUsageExample> logger)
        {
            _oaIntegrationService = oaIntegrationService;
            _logger = logger;
        }

        /// <summary>
        /// 示例1：发起带有指定审批人的OA流程
        /// </summary>
        /// <param name="loginName">发起人登录名</param>
        /// <param name="assignedPersonCode">指定负责人工号</param>
        /// <param name="orderData">订单数据</param>
        /// <returns></returns>
        public async Task<WebResponseContent> StartProcessWithApproverAsync(
            string loginName,
            string assignedPersonCode,
            OrderProcessData orderData)
        {
            try
            {
                _logger.LogInformation($"开始发起带有指定审批人的OA流程 - 发起人: {loginName}, 指定负责人: {assignedPersonCode}");

                // 构建智能OA流程数据
                var processData = new SmartOAProcessData
                {
                    DataSource = "协同平台",
                    SupplierCode = orderData.SupplierCode,
                    SupplierName = orderData.SupplierName,
                    OrderNo = orderData.OrderNo,
                    SourceOrderNo = orderData.SourceOrderNo,
                    DeliveryStatus = orderData.DeliveryStatus,
                    OrderDate = orderData.OrderDate?.ToString("yyyy-MM-dd"),
                    OrderRemark = orderData.OrderRemark,
                    MaterialName = orderData.MaterialName,
                    MaterialCode = orderData.MaterialCode,
                    Specification = orderData.Specification,
                    DrawingNo = orderData.DrawingNo,
                    Material = orderData.Material,
                    Unit = orderData.Unit,
                    PurchaseQuantity = orderData.PurchaseQuantity?.ToString(),
                    ProductionCycle = orderData.ProductionCycle,
                    Shortage = orderData.Shortage?.ToString(),
                    PlanTrackingNo = orderData.PlanTrackingNo,
                    FirstRequiredDeliveryDate = orderData.FirstRequiredDeliveryDate?.ToString("yyyy-MM-dd"),
                    ExecutiveOrganization = orderData.ExecutiveOrganization ?? "1",
                    ChangedDeliveryDate = orderData.ChangedDeliveryDate?.ToString("yyyy-MM-dd"),
                    OrderEntryLineNo = orderData.OrderEntryLineNo,
                    SupplierExceptionReply = orderData.SupplierExceptionReply,
                    AssignedResponsiblePerson = assignedPersonCode // 设置指定负责人工号
                };

                // 发起流程（系统会自动根据指定负责人工号获取对应的OA人员ID作为审批人）
                var result = await _oaIntegrationService.SmartStartProcessAsync(loginName, processData);

                if (result.Status)
                {
                    _logger.LogInformation($"带有指定审批人的OA流程发起成功 - 订单号: {orderData.OrderNo}, 指定负责人: {assignedPersonCode}");
                    
                    // 可以从result.Data中获取流程信息
                    if (result.Data is OAProcessData processInfo)
                    {
                        _logger.LogInformation($"流程ID: {processInfo.ProcessId}, 主题: {processInfo.Subject}");
                    }
                }
                else
                {
                    _logger.LogError($"带有指定审批人的OA流程发起失败 - 订单号: {orderData.OrderNo}, 指定负责人: {assignedPersonCode}, 错误: {result.Message}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"发起带有指定审批人的OA流程时发生异常 - 订单号: {orderData.OrderNo}, 指定负责人: {assignedPersonCode}");
                return new WebResponseContent().Error($"发起流程异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 示例2：协商流程中设置指定负责人作为审批人
        /// </summary>
        /// <param name="loginName">发起人登录名</param>
        /// <param name="negotiationData">协商数据</param>
        /// <returns></returns>
        public async Task<WebResponseContent> StartNegotiationProcessWithApproverAsync(
            string loginName,
            NegotiationProcessData negotiationData)
        {
            try
            {
                _logger.LogInformation($"开始发起协商OA流程 - 发起人: {loginName}, 协商ID: {negotiationData.NegotiationId}, 指定负责人: {negotiationData.AssignedResponsiblePerson}");

                // 构建协商OA流程数据
                var processData = new SmartOAProcessData
                {
                    DataSource = "协同平台",
                    NegotiationNo = negotiationData.NegotiationId.ToString(),
                    SupplierCode = negotiationData.SupplierCode,
                    SupplierName = negotiationData.SupplierName,
                    OrderNo = negotiationData.OrderNo,
                    SourceOrderNo = negotiationData.SourceOrderNo,
                    DeliveryStatus = "协商中",
                    OrderDate = negotiationData.OrderDate?.ToString("yyyy-MM-dd"),
                    OrderRemark = negotiationData.NegotiationReason,
                    MaterialName = negotiationData.MaterialName,
                    MaterialCode = negotiationData.MaterialCode,
                    Specification = negotiationData.Specification,
                    PlanTrackingNo = negotiationData.PlanTrackingNo,
                    FirstRequiredDeliveryDate = negotiationData.OriginalDeliveryDate?.ToString("yyyy-MM-dd"),
                    ChangedDeliveryDate = negotiationData.ProposedDeliveryDate?.ToString("yyyy-MM-dd"),
                    ExecutiveOrganization = negotiationData.ExecutiveOrganization ?? "1",
                    SupplierExceptionReply = negotiationData.NegotiationContent,
                    AssignedResponsiblePerson = negotiationData.AssignedResponsiblePerson // 设置指定负责人工号
                };

                // 发起协商流程
                var result = await _oaIntegrationService.SmartStartProcessAsync(loginName, processData);

                if (result.Status)
                {
                    _logger.LogInformation($"协商OA流程发起成功 - 协商ID: {negotiationData.NegotiationId}, 指定负责人: {negotiationData.AssignedResponsiblePerson}");
                }
                else
                {
                    _logger.LogError($"协商OA流程发起失败 - 协商ID: {negotiationData.NegotiationId}, 指定负责人: {negotiationData.AssignedResponsiblePerson}, 错误: {result.Message}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"发起协商OA流程时发生异常 - 协商ID: {negotiationData.NegotiationId}, 指定负责人: {negotiationData.AssignedResponsiblePerson}");
                return new WebResponseContent().Error($"发起协商流程异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 示例3：直接使用表单数据发起流程（手动设置审批人ID）
        /// </summary>
        /// <param name="loginName">发起人登录名</param>
        /// <param name="assignedPersonCode">指定负责人工号</param>
        /// <param name="formData">表单数据</param>
        /// <returns></returns>
        public async Task<WebResponseContent> StartProcessWithManualApproverAsync(
            string loginName,
            string assignedPersonCode,
            OrderFormData formData)
        {
            try
            {
                _logger.LogInformation($"开始手动设置审批人并发起OA流程 - 发起人: {loginName}, 指定负责人: {assignedPersonCode}");

                // 1. 先获取指定负责人的OA人员ID
                string approverUserId = null;
                if (!string.IsNullOrEmpty(assignedPersonCode))
                {
                    var userResult = await _oaIntegrationService.GetShareholderOAUserByCodeAsync(assignedPersonCode);
                    if (userResult.Status && userResult.Data is ShareholderOAUserInfo userInfo)
                    {
                        approverUserId = userInfo.Id.ToString();
                        _logger.LogInformation($"成功获取指定负责人OA人员ID - 工号: {assignedPersonCode}, 人员ID: {approverUserId}, 姓名: {userInfo.Name}");
                    }
                    else
                    {
                        _logger.LogWarning($"未能获取指定负责人OA人员ID - 工号: {assignedPersonCode}, 错误: {userResult.Message}");
                    }
                }

                // 2. 构建表单数据（根据配置选择股份OA或原有OA）
                if (true) // 假设使用股份OA
                {
                    // 获取发起人员工号（管理员账号使用默认值015206）
                    var currentUserEmployeeCode = GetInitiatorEmployeeCode();

                    var shareholderFormData = new ShareholderOAFormData
                    {
                        DataSource = formData.DataSource,
                        SupplierCode = formData.SupplierCode,
                        SupplierName = formData.SupplierName,
                        OrderNo = formData.OrderNo,
                        SourceOrderNo = formData.SourceOrderNo,
                        DeliveryStatus = formData.DeliveryStatus,
                        OrderDate = formData.OrderDate,
                        OrderRemark = formData.OrderRemark,
                        MaterialName = formData.MaterialName,
                        MaterialCode = formData.MaterialCode,
                        Specification = formData.Specification,
                        DrawingNo = formData.DrawingNo,
                        Material = formData.Material,
                        Unit = formData.Unit,
                        PurchaseQuantity = formData.PurchaseQuantity,
                        ProductionCycle = formData.ProductionCycle,
                        Shortage = formData.Shortage,
                        PlanTrackingNo = formData.PlanTrackingNo,
                        FirstRequiredDeliveryDate = formData.FirstRequiredDeliveryDate,
                        ExecutiveOrganization = formData.ExecutiveOrganization,
                        ChangedDeliveryDate = formData.ChangedDeliveryDate,
                        OrderEntryLineNo = formData.OrderEntryLineNo,
                        SupplierExceptionReply = formData.SupplierExceptionReply,
                        ApproverUserId = approverUserId ?? "", // 设置审批人ID
                        InitiatorEmployeeCode = currentUserEmployeeCode // 设置发起人员工号
                    };

                    // 发起股份OA流程
                    var result = await _oaIntegrationService.GetShareholderTokenAndStartProcessAsync(loginName, shareholderFormData);

                    if (result.Status)
                    {
                        _logger.LogInformation($"股份OA流程发起成功 - 订单号: {formData.OrderNo}, 审批人ID: {approverUserId}");
                    }
                    else
                    {
                        _logger.LogError($"股份OA流程发起失败 - 订单号: {formData.OrderNo}, 审批人ID: {approverUserId}, 错误: {result.Message}");
                    }

                    return result;
                }
                else
                {
                    // 原有OA流程的处理逻辑
                    var originalFormData = new OAFormData
                    {
                        DataSource = formData.DataSource,
                        SupplierCode = formData.SupplierCode,
                        SupplierName = formData.SupplierName,
                        OrderNo = formData.OrderNo,
                        SourceOrderNo = formData.SourceOrderNo,
                        DeliveryStatus = formData.DeliveryStatus,
                        OrderDate = formData.OrderDate,
                        OrderRemark = formData.OrderRemark,
                        MaterialName = formData.MaterialName,
                        MaterialCode = formData.MaterialCode,
                        Specification = formData.Specification,
                        DrawingNo = formData.DrawingNo,
                        Material = formData.Material,
                        Unit = formData.Unit,
                        PurchaseQuantity = formData.PurchaseQuantity,
                        Shortage = formData.Shortage,
                        PlanTrackingNo = formData.PlanTrackingNo,
                        FirstRequiredDeliveryDate = formData.FirstRequiredDeliveryDate,
                        ExecutiveOrganization = formData.ExecutiveOrganization,
                        ChangedDeliveryDate = formData.ChangedDeliveryDate,
                        SupplierExceptionReply = formData.SupplierExceptionReply,
                        ApproverUserId = approverUserId ?? "" // 设置审批人ID
                    };

                    return await _oaIntegrationService.GetTokenAndStartProcessAsync(loginName, originalFormData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"手动设置审批人并发起OA流程时发生异常 - 订单号: {formData.OrderNo}, 指定负责人: {assignedPersonCode}");
                return new WebResponseContent().Error($"发起流程异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 示例4：验证指定负责人是否存在于OA系统中
        /// </summary>
        /// <param name="employeeCode">人员工号</param>
        /// <returns></returns>
        public async Task<WebResponseContent> ValidateAssignedPersonInOAAsync(string employeeCode)
        {
            try
            {
                _logger.LogInformation($"开始验证指定负责人是否存在于OA系统中 - 工号: {employeeCode}");

                var result = await _oaIntegrationService.GetShareholderOAUserByCodeAsync(employeeCode);

                if (result.Status && result.Data is ShareholderOAUserInfo userInfo)
                {
                    _logger.LogInformation($"指定负责人存在于OA系统中 - 工号: {employeeCode}, 人员ID: {userInfo.Id}, 姓名: {userInfo.Name}, 部门: {userInfo.OrgDepartmentName}");
                    
                    return new WebResponseContent
                    {
                        Status = true,
                        Message = "指定负责人验证成功",
                        Data = new
                        {
                            EmployeeCode = employeeCode,
                            UserId = userInfo.Id,
                            UserName = userInfo.Name,
                            Department = userInfo.OrgDepartmentName,
                            Position = userInfo.OrgPostName,
                            IsValid = userInfo.IsValid,
                            IsLoginable = userInfo.IsLoginable
                        }
                    };
                }
                else
                {
                    _logger.LogWarning($"指定负责人不存在于OA系统中 - 工号: {employeeCode}, 错误: {result.Message}");
                    
                    return new WebResponseContent
                    {
                        Status = false,
                        Message = $"指定负责人不存在于OA系统中: {result.Message}",
                        Data = new { EmployeeCode = employeeCode }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"验证指定负责人时发生异常 - 工号: {employeeCode}");
                return new WebResponseContent().Error($"验证异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 示例5：使用动态Token获取方式查询股份OA人员信息（新增方法）
        /// </summary>
        /// <param name="employeeCode">人员工号</param>
        /// <returns>查询结果</returns>
        public async Task<WebResponseContent> GetUserInfoWithDynamicTokenAsync(string employeeCode)
        {
            try
            {
                _logger.LogInformation($"开始使用动态Token获取方式查询人员信息 - 工号: {employeeCode}");
                
                // 使用新的GetShareholderTokenAndQueryUserAsync方法
                // 该方法会自动获取Token然后查询人员信息
                var result = await _oaIntegrationService.GetShareholderTokenAndQueryUserAsync(employeeCode);
                
                if (result.Status && result.Data is ShareholderOAUserInfo userInfo)
                {
                    _logger.LogInformation($"动态Token方式查询成功 - 工号: {employeeCode}, 人员ID: {userInfo.Id}, 姓名: {userInfo.Name}");
                    
                    return new WebResponseContent
                    {
                        Status = true,
                        Message = "动态Token方式查询成功",
                        Data = new
                        {
                            EmployeeCode = employeeCode,
                            UserId = userInfo.Id,
                            UserName = userInfo.Name,
                            Department = userInfo.OrgDepartmentName,
                            Position = userInfo.OrgPostName,
                            LoginName = userInfo.LoginName,
                            EmailAddress = userInfo.EmailAddress,
                            TelNumber = userInfo.TelNumber,
                            IsValid = userInfo.IsValid,
                            IsLoginable = userInfo.IsLoginable,
                            QueryMethod = "动态Token获取"
                        }
                    };
                }
                else
                {
                    _logger.LogWarning($"动态Token方式查询失败 - 工号: {employeeCode}, 错误: {result.Message}");
                    return new WebResponseContent().Error($"动态Token方式查询失败: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"动态Token方式查询异常 - 工号: {employeeCode}");
                return new WebResponseContent().Error($"动态Token方式查询异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 示例6：对比两种Token获取方式的性能和可靠性
        /// </summary>
        /// <param name="employeeCode">人员工号</param>
        /// <returns>对比结果</returns>
        public async Task<WebResponseContent> CompareTokenMethodsAsync(string employeeCode)
        {
            try
            {
                _logger.LogInformation($"开始对比两种Token获取方式 - 工号: {employeeCode}");
                
                var startTime = DateTime.Now;
                var comparisonResult = new
                {
                    EmployeeCode = employeeCode,
                    StartTime = startTime,
                    Method1 = new { Name = "GetShareholderOAUserByCodeAsync（内部动态获取）", Success = false, Duration = 0.0, Message = "", Data = (object)null },
                    Method2 = new { Name = "GetShareholderTokenAndQueryUserAsync（专门方法）", Success = false, Duration = 0.0, Message = "", Data = (object)null },
                    TotalDuration = 0.0,
                    Recommendation = ""
                };
                
                // 方式1：使用原有方法（内部动态获取Token）
                _logger.LogInformation("=== 测试方式1：GetShareholderOAUserByCodeAsync（内部动态获取Token）===");
                var method1Start = DateTime.Now;
                var result1 = await _oaIntegrationService.GetShareholderOAUserByCodeAsync(employeeCode);
                var method1Duration = (DateTime.Now - method1Start).TotalMilliseconds;
                
                // 方式2：使用新的专门方法
                _logger.LogInformation("=== 测试方式2：GetShareholderTokenAndQueryUserAsync（专门方法）===");
                var method2Start = DateTime.Now;
                var result2 = await _oaIntegrationService.GetShareholderTokenAndQueryUserAsync(employeeCode);
                var method2Duration = (DateTime.Now - method2Start).TotalMilliseconds;
                
                var totalDuration = (DateTime.Now - startTime).TotalMilliseconds;
                
                // 分析结果
                string recommendation;
                if (result1.Status && result2.Status)
                {
                    recommendation = "两种方式都成功，建议使用专门方法（方式2）以获得更好的代码可读性";
                }
                else if (result1.Status && !result2.Status)
                {
                    recommendation = "方式1成功，方式2失败，建议使用方式1并检查方式2的实现";
                }
                else if (!result1.Status && result2.Status)
                {
                    recommendation = "方式2成功，方式1失败，建议使用方式2";
                }
                else
                {
                    recommendation = "两种方式都失败，请检查网络连接和配置";
                }
                
                _logger.LogInformation($"对比测试完成 - 方式1: {(result1.Status ? "成功" : "失败")}({method1Duration}ms), 方式2: {(result2.Status ? "成功" : "失败")}({method2Duration}ms)");
                
                return new WebResponseContent
                {
                    Status = true,
                    Message = "Token获取方式对比完成",
                    Data = new
                    {
                        EmployeeCode = employeeCode,
                        StartTime = startTime,
                        Method1 = new 
                        { 
                            Name = "GetShareholderOAUserByCodeAsync（内部动态获取）", 
                            Success = result1.Status, 
                            Duration = method1Duration, 
                            Message = result1.Message,
                            Data = result1.Data
                        },
                        Method2 = new 
                        { 
                            Name = "GetShareholderTokenAndQueryUserAsync（专门方法）", 
                            Success = result2.Status, 
                            Duration = method2Duration, 
                            Message = result2.Message,
                            Data = result2.Data
                        },
                        TotalDuration = totalDuration,
                        Recommendation = recommendation
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"对比测试异常 - 工号: {employeeCode}");
                return new WebResponseContent().Error($"对比测试异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 示例7：演示优化后的SmartStartProcessAsync共享Token机制
        /// </summary>
        /// <param name="loginName">发起人登录名</param>
        /// <param name="assignedPersonCode">指定负责人工号</param>
        /// <param name="orderData">订单数据</param>
        /// <returns>流程发起结果</returns>
        public async Task<WebResponseContent> DemonstrateOptimizedSmartStartProcessAsync(
            string loginName,
            string assignedPersonCode,
            OrderProcessData orderData)
        {
            try
            {
                _logger.LogInformation($"开始演示优化后的SmartStartProcessAsync共享Token机制 - 发起人: {loginName}, 指定负责人: {assignedPersonCode}");

                // 构建智能OA流程数据
                var processData = new SmartOAProcessData
                {
                    DataSource = "协同平台",
                    SupplierCode = orderData.SupplierCode,
                    SupplierName = orderData.SupplierName,
                    OrderNo = orderData.OrderNo,
                    SourceOrderNo = orderData.SourceOrderNo,
                    DeliveryStatus = orderData.DeliveryStatus,
                    OrderDate = orderData.OrderDate?.ToString("yyyy-MM-dd"),
                    OrderRemark = orderData.OrderRemark,
                    MaterialName = orderData.MaterialName,
                    MaterialCode = orderData.MaterialCode,
                    Specification = orderData.Specification,
                    DrawingNo = orderData.DrawingNo,
                    Material = orderData.Material,
                    Unit = orderData.Unit,
                    PurchaseQuantity = orderData.PurchaseQuantity?.ToString(),
                    ProductionCycle = orderData.ProductionCycle,
                    Shortage = orderData.Shortage?.ToString(),
                    PlanTrackingNo = orderData.PlanTrackingNo,
                    FirstRequiredDeliveryDate = orderData.FirstRequiredDeliveryDate?.ToString("yyyy-MM-dd"),
                    ExecutiveOrganization = orderData.ExecutiveOrganization ?? "1",
                    ChangedDeliveryDate = orderData.ChangedDeliveryDate?.ToString("yyyy-MM-dd"),
                    OrderEntryLineNo = orderData.OrderEntryLineNo,
                    SupplierExceptionReply = orderData.SupplierExceptionReply,
                    AssignedResponsiblePerson = assignedPersonCode // 设置指定负责人工号
                };

                _logger.LogInformation("=== 优化后的SmartStartProcessAsync流程说明 ===");
                _logger.LogInformation("1. 对于股份OA流程：先获取一次Token，然后用于人员查询和流程发起");
                _logger.LogInformation("2. 对于原有OA流程：人员查询使用股份OA，流程发起使用原有OA");
                _logger.LogInformation("3. 优化效果：股份OA流程减少一次Token获取，提高性能");

                // 记录开始时间
                var startTime = DateTime.Now;

                // 发起流程（系统会自动使用优化后的Token共享机制）
                var result = await _oaIntegrationService.SmartStartProcessAsync(loginName, processData);

                // 记录结束时间
                var endTime = DateTime.Now;
                var duration = endTime - startTime;

                if (result.Status)
                {
                    _logger.LogInformation($"优化后的SmartStartProcessAsync执行成功 - 订单号: {orderData.OrderNo}, 指定负责人: {assignedPersonCode}, 耗时: {duration.TotalMilliseconds}ms");
                    
                    // 可以从result.Data中获取流程信息
                    if (result.Data is OAProcessData processInfo)
                    {
                        _logger.LogInformation($"流程ID: {processInfo.ProcessId}, 主题: {processInfo.Subject}");
                    }
                    
                    return new WebResponseContent
                    {
                        Status = true,
                        Message = "优化后的SmartStartProcessAsync执行成功",
                        Data = new
                        {
                            OrderNo = orderData.OrderNo,
                            AssignedResponsiblePerson = assignedPersonCode,
                            Duration = duration.TotalMilliseconds,
                            ProcessId = (result.Data as OAProcessData)?.ProcessId,
                            OptimizationNote = "股份OA流程使用共享Token机制，减少一次Token获取调用"
                        }
                    };
                }
                else
                {
                    _logger.LogError($"优化后的SmartStartProcessAsync执行失败 - 订单号: {orderData.OrderNo}, 指定负责人: {assignedPersonCode}, 错误: {result.Message}, 耗时: {duration.TotalMilliseconds}ms");
                    return new WebResponseContent().Error($"优化后的SmartStartProcessAsync执行失败: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"演示优化后的SmartStartProcessAsync时发生异常 - 订单号: {orderData.OrderNo}, 指定负责人: {assignedPersonCode}");
                return new WebResponseContent().Error($"演示异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取发起人员工号
        /// 对于管理员账号（admin、cyadmin），使用默认工号 015206
        /// 对于普通用户，使用当前登录用户的工号
        /// </summary>
        /// <returns>发起人员工号</returns>
        private string GetInitiatorEmployeeCode()
        {
            try
            {
                var currentUser = HDPro.Core.ManageUser.UserContext.Current;
                var currentUserName = currentUser?.UserInfo?.UserName;
                
                // 如果无法获取用户信息，使用默认值
                if (string.IsNullOrEmpty(currentUserName))
                {
                    _logger.LogWarning("无法获取当前用户信息，使用默认发起人员工号: 015206");
                    return "015206";
                }

                // 管理员账号映射到默认工号
                if (currentUserName.Equals("admin", StringComparison.OrdinalIgnoreCase) || 
                    currentUserName.Equals("cyadmin", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("管理员账号 {UserName} 使用默认发起人员工号: 015206", currentUserName);
                    return "015206";
                }

                // 普通用户使用实际工号
                _logger.LogInformation("用户 {UserName} 使用发起人员工号: {EmployeeCode}", currentUserName, currentUserName);
                return currentUserName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取发起人员工号时发生异常，使用默认值: 015206");
                return "015206";
            }
        }
    }

    /// <summary>
    /// 订单流程数据模型
    /// </summary>
    public class OrderProcessData
    {
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string OrderNo { get; set; }
        public string SourceOrderNo { get; set; }
        public string DeliveryStatus { get; set; }
        public DateTime? OrderDate { get; set; }
        public string OrderRemark { get; set; }
        public string MaterialName { get; set; }
        public string MaterialCode { get; set; }
        public string Specification { get; set; }
        public string DrawingNo { get; set; }
        public string Material { get; set; }
        public string Unit { get; set; }
        public decimal? PurchaseQuantity { get; set; }
        public string ProductionCycle { get; set; }
        public decimal? Shortage { get; set; }
        public string PlanTrackingNo { get; set; }
        public DateTime? FirstRequiredDeliveryDate { get; set; }
        public string ExecutiveOrganization { get; set; }
        public DateTime? ChangedDeliveryDate { get; set; }
        public int? OrderEntryLineNo { get; set; }
        public string SupplierExceptionReply { get; set; }
    }

    /// <summary>
    /// 协商流程数据模型
    /// </summary>
    public class NegotiationProcessData
    {
        public long NegotiationId { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string OrderNo { get; set; }
        public string SourceOrderNo { get; set; }
        public DateTime? OrderDate { get; set; }
        public string NegotiationReason { get; set; }
        public string MaterialName { get; set; }
        public string MaterialCode { get; set; }
        public string Specification { get; set; }
        public string PlanTrackingNo { get; set; }
        public DateTime? OriginalDeliveryDate { get; set; }
        public DateTime? ProposedDeliveryDate { get; set; }
        public string ExecutiveOrganization { get; set; }
        public string NegotiationContent { get; set; }
        public string AssignedResponsiblePerson { get; set; }
    }

    /// <summary>
    /// 表单数据模型
    /// </summary>
    public class OrderFormData
    {
        public string DataSource { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string OrderNo { get; set; }
        public string SourceOrderNo { get; set; }
        public string DeliveryStatus { get; set; }
        public string OrderDate { get; set; }
        public string OrderRemark { get; set; }
        public string MaterialName { get; set; }
        public string MaterialCode { get; set; }
        public string Specification { get; set; }
        public string DrawingNo { get; set; }
        public string Material { get; set; }
        public string Unit { get; set; }
        public string PurchaseQuantity { get; set; }
        public string ProductionCycle { get; set; }
        public string Shortage { get; set; }
        public string PlanTrackingNo { get; set; }
        public string FirstRequiredDeliveryDate { get; set; }
        public string ExecutiveOrganization { get; set; }
        public string ChangedDeliveryDate { get; set; }
        public int? OrderEntryLineNo { get; set; }
        public string SupplierExceptionReply { get; set; }
    }
} 