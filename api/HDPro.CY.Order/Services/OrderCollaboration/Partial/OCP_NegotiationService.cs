/*
 *所有关于OCP_Negotiation类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*OCP_NegotiationService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using System;
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
using HDPro.CY.Order.IServices.OA;
using System.Threading.Tasks;
using System.Collections.Generic;
using HDPro.Utilities;
using HDPro.CY.Order.Models;
using HDPro.CY.Order.Services.OrderCollaboration.Common;
using HDPro.Utilities.Extensions;
using HDPro.Core.SignalR;
using HDPro.Core.Enums;

namespace HDPro.CY.Order.Services
{
    public partial class OCP_NegotiationService
    {
        private readonly IOCP_NegotiationRepository _repository;//访问数据库
        private readonly IOCP_SubOrderUnFinishTrackRepository _subOrderRepository;
        private readonly IOCP_POUnFinishTrackRepository _purchaseRepository;
        private readonly IOCP_MaterialRepository _materialRepository;
        private readonly IOAIntegrationService _oaIntegrationService;
        private readonly IMessageService _messageService;//SignalR消息服务

        [ActivatorUtilitiesConstructor]
        public OCP_NegotiationService(
            IOCP_NegotiationRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            IOCP_SubOrderUnFinishTrackRepository subOrderRepository,
            IOCP_POUnFinishTrackRepository purchaseRepository,
            IOCP_MaterialRepository materialRepository,
            IOAIntegrationService oaIntegrationService,
            IMessageService messageService
            )
        : base(dbRepository, httpContextAccessor)
        {
            _repository = dbRepository;
            _subOrderRepository = subOrderRepository;
            _purchaseRepository = purchaseRepository;
            _materialRepository = materialRepository;
            _oaIntegrationService = oaIntegrationService;
            _messageService = messageService;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        /// <summary>
        /// 重写CY.Order项目特有的初始化逻辑
        /// 可在此处添加OCP_Negotiation特有的初始化代码
        /// </summary>
        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
            // 在此处添加OCP_Negotiation特有的初始化逻辑
        }

        /// <summary>
        /// 重写CY.Order项目通用数据验证方法
        /// 可在此处添加OCP_Negotiation特有的数据验证逻辑
        /// </summary>
        /// <param name="entity">要验证的实体</param>
        /// <returns>验证结果</returns>
        protected override WebResponseContent ValidateCYOrderEntity(OCP_Negotiation entity)
        {
            var response = base.ValidateCYOrderEntity(entity);
            
            // 在此处添加OCP_Negotiation特有的数据验证逻辑
            
            return response;
        }

        public override WebResponseContent Add(SaveModel saveDataModel)
        {
            // 在保存前设置默认状态
            if (saveDataModel?.MainData != null)
            {
                var negotiation = saveDataModel.MainData.DicToEntity<OCP_Negotiation>();
                if (negotiation != null && string.IsNullOrEmpty(negotiation.NegotiationStatus))
                {
                    saveDataModel.MainData["NegotiationStatus"] = BusinessConstants.NegotiationStatus.Pending;
                }
            }
     // 在保存数据库前的操作，所有数据都验证通过了，这一步执行完就执行数据库保存
            AddOnExecuting = (OCP_Negotiation order, object list) =>
            {
                if(order.AssignedResPerson.IsNullOrEmptyOrWhiteSpace())
                {
                    order.AssignedResPerson=order.DefaultResPerson;
                    order.AssignedResPersonName=order.DefaultResPersonName;
        
                }
                order.NegotiationStatus = BusinessConstants.NegotiationStatus.Pending;
                
                // 根据业务类型和业务主键读取物料相关信息
                PopulateMaterialInfoFromTrackingTable(order);
                
                return WebResponseContent.Instance.OK();
            };

            var res = base.Add(saveDataModel);
            
            //这里在保存成功后做一些其他操作
            if (res.Status)
            {
                var negotiation = saveDataModel.MainData.DicToEntity<OCP_Negotiation>();
                if (negotiation != null)
                {
                    // 判断业务类型：委外和采购使用OA发流程，其他业务类型发OA消息
                    if (BusinessConstants.RequiresOAProcess(negotiation.BusinessType))
                    {
                        // 在Task.Run之前预先读取所有需要的数据
                        OCP_SubOrderUnFinishTrack subOrderTrackingData = null;
                        OCP_POUnFinishTrack purchaseTrackingData = null;
                        OCP_Material materialData = null;

                        // 根据业务类型预加载对应的跟踪数据
                        if (negotiation.BusinessType == BusinessConstants.BusinessType.OutSourcing && !string.IsNullOrEmpty(negotiation.BusinessKey))
                        {
                            // 预加载委外跟踪数据
                            if (int.TryParse(negotiation.BusinessKey, out int entryId))
                            {
                                subOrderTrackingData = _subOrderRepository.FindFirst(x => x.FENTRYID == entryId);

                                // 如果找到跟踪记录，获取对应的物料信息
                                if (subOrderTrackingData != null && subOrderTrackingData.MaterialID.HasValue)
                                {
                                    materialData = _materialRepository.FindFirst(x => x.MaterialID == subOrderTrackingData.MaterialID.Value);
                                }
                            }
                        }
                        else if (negotiation.BusinessType == BusinessConstants.BusinessType.Purchase && !string.IsNullOrEmpty(negotiation.BusinessKey))
                        {
                            // 预加载采购跟踪数据
                            if (int.TryParse(negotiation.BusinessKey, out int entryId))
                            {
                                purchaseTrackingData = _purchaseRepository.FindFirst(x => x.FENTRYID == entryId);

                                // 如果找到跟踪记录，通过物料编码获取对应的物料信息
                                if (purchaseTrackingData != null && !string.IsNullOrEmpty(purchaseTrackingData.MaterialCode))
                                {
                                    materialData = _materialRepository.FindFirst(x => x.MaterialCode == purchaseTrackingData.MaterialCode);
                                }
                            }
                        }

                        // 在Task.Run之前获取用户信息，避免上下文丢失
                        var (currentUserLoginName, currentUserDisplayName) = MessageHelper.GetCurrentUserInfo();
                        var currentUserId = HDPro.Core.ManageUser.UserContext.Current?.UserId;

                        // 委外和采购业务：发起OA流程，传入预加载的数据
                        Task.Run(async () =>
                        {
                            try
                            {
                                // 1. 发送SignalR实时通知
                                await SendSignalRNotificationAsync(negotiation, currentUserLoginName, currentUserDisplayName, currentUserId);
                                
                                // 2. 发起OA流程
                                await StartOAProcessAsync(negotiation, subOrderTrackingData, purchaseTrackingData, materialData, currentUserLoginName, currentUserDisplayName);
                            }
                            catch (System.Exception ex)
                            {
                                MessageHelper.LogIntegrationException("OA流程", "Negotiation", negotiation.NegotiationID, negotiation.BusinessType, ex);
                            }
                        });
                    }
                    else
                    {
                        // 在Task.Run之前获取用户信息，避免上下文丢失
                        var (currentUserLoginName, currentUserDisplayName) = MessageHelper.GetCurrentUserInfo();
                        var currentUserId = HDPro.Core.ManageUser.UserContext.Current?.UserId;
                        
                        // 其他业务类型：发送通知消息
                        Task.Run(async () =>
                        {
                            try
                            {
                                // 1. 发送SignalR实时通知
                                await SendSignalRNotificationAsync(negotiation, currentUserLoginName, currentUserDisplayName, currentUserId);
                                
                                // 2. 发送OA消息通知
                                await SendOANegotiationNotificationAsync(negotiation, currentUserLoginName, currentUserDisplayName);
                            }
                            catch (System.Exception ex)
                            {
                                MessageHelper.LogIntegrationException("通知消息", "Negotiation", negotiation.NegotiationID, negotiation.BusinessType, ex);
                            }
                        });
                    }
                }
            }
            return res;
        }













        /// <summary>
        /// 获取协商消息统计数据
        /// </summary>
        /// <returns>协商统计数据</returns>
        public async Task<MessageStatisticsDto> GetNegotiationStatisticsAsync()
        {
            try
            {
                // 使用当前Repository的DbContext统一查询，避免多个Repository的并发访问问题
                var dbContext = _repository.DbContext;
                
                // 权限控制：只统计当前用户有权限查看的协商
                var currentUser = HDPro.Core.ManageUser.UserContext.Current;
                var currentUserId = currentUser?.UserId ?? 0;
                var currentUserName = currentUser?.UserInfo?.UserName ?? "";
                
                IQueryable<OCP_Negotiation> authorizedNegotiations;
                
                // 超级管理员可以查看所有数据
                if (currentUser?.IsSuperAdmin == true)
                {
                    authorizedNegotiations = dbContext.Set<OCP_Negotiation>();
                    HDLogHelper.Log("Negotiation_Statistics_SuperAdmin", 
                        $"超级管理员统计协商数据 - 用户ID: {currentUserId}, 无权限限制", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                }
                else
                {
                    authorizedNegotiations = dbContext.Set<OCP_Negotiation>()
                        .Where(x => x.CreateID == currentUserId || 
                                   x.AssignedResPerson == currentUserName ||
                                   x.DefaultResPerson == currentUserName);
                    HDLogHelper.Log("Negotiation_Statistics_Permission", 
                        $"协商统计权限过滤 - 用户ID: {currentUserId}, 用户名: {currentUserName}", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                }
                
                // 发送消息数（当前用户有权限查看的协商记录）
                var sentCount = await authorizedNegotiations.CountAsync();
                
                // 获取当前用户有权限查看的协商ID列表
                var authorizedNegotiationIds = await authorizedNegotiations
                    .Select(x => x.NegotiationID)
                    .ToListAsync();
                
                // 已回复消息数（有回复记录且用户有权限查看的协商）
                var repliedNegotiationIds = await dbContext.Set<OCP_NegotiationReply>()
                    .Where(r => authorizedNegotiationIds.Contains(r.NegotiationID))
                    .Select(x => x.NegotiationID)
                    .Distinct()
                    .ToListAsync();
                
                var repliedCount = repliedNegotiationIds.Count;
                
                // 待回复消息数（没有回复记录且用户有权限查看的协商）
                var pendingCount = sentCount - repliedCount;
                
                // 已超期消息数（根据协商日期和当前时间计算，假设超过7天为超期）
                var overdueCount = 0;
                var sevenDaysAgo = System.DateTime.Now.AddDays(-7);
                
                var overdueNegotiations = await authorizedNegotiations
                    .Where(x => x.CreateDate.HasValue && 
                               x.CreateDate.Value < sevenDaysAgo)
                    .Select(x => x.NegotiationID)
                    .ToListAsync();
                
                // 从超期的协商中排除已回复的
                overdueCount = overdueNegotiations.Count(x => !repliedNegotiationIds.Contains(x));

                return new MessageStatisticsDto
                {
                    SentCount = sentCount,
                    PendingCount = pendingCount,
                    OverdueCount = overdueCount,
                    RepliedCount = repliedCount
                };
            }
            catch (System.Exception ex)
            {
                HDLogHelper.Log("Negotiation_Statistics_Error", $"获取协商统计数据异常: {ex.Message}", "OrderCollaboration");
                throw;
            }
        }

        /// <summary>
        /// 更新协商状态
        /// </summary>
        /// <param name="negotiationID">协商ID</param>
        /// <param name="newStatus">新状态</param>
        /// <returns>更新结果</returns>
        public async Task<WebResponseContent> UpdateNegotiationStatusAsync(long negotiationID, string newStatus)
        {
            try
            {
                // 获取当前用户信息
                var currentUser = HDPro.Core.ManageUser.UserContext.Current;
                var userInfo = currentUser?.UserInfo;
                
                using (var transaction = await _repository.DbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var negotiation = await _repository.DbContext.Set<OCP_Negotiation>()
                            .FirstOrDefaultAsync(n => n.NegotiationID == negotiationID);
                        
                        if (negotiation == null)
                        {
                            return new WebResponseContent().Error("协商记录不存在");
                        }

                        var oldStatus = negotiation.NegotiationStatus;
                        negotiation.NegotiationStatus = newStatus;
                        negotiation.ModifyDate = System.DateTime.Now;
                        negotiation.Modifier = userInfo?.UserTrueName ?? "系统";
                        
                        await _repository.DbContext.SaveChangesAsync();
                        await transaction.CommitAsync();

                        HDLogHelper.Log("Negotiation_UpdateStatus_Success", 
                            $"协商状态更新成功 - 协商ID: {negotiationID}, 状态: {oldStatus} → {newStatus}", 
                            BusinessConstants.LogCategory.OrderCollaboration);

                        return new WebResponseContent().OK("协商状态更新成功");
                    }
                    catch (System.Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
            catch (System.Exception ex)
            {
                HDLogHelper.Log("Negotiation_UpdateStatus_Error", $"更新协商状态异常: {ex.Message}", BusinessConstants.LogCategory.OrderCollaboration);
                return new WebResponseContent().Error($"更新协商状态异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 批量更新协商状态
        /// </summary>
        /// <param name="negotiationIDs">协商ID列表</param>
        /// <param name="newStatus">新状态</param>
        /// <returns>更新结果</returns>
        public async Task<WebResponseContent> BatchUpdateNegotiationStatusAsync(List<long> negotiationIDs, string newStatus)
        {
            try
            {
                // 获取当前用户信息
                var currentUser = HDPro.Core.ManageUser.UserContext.Current;
                var userInfo = currentUser?.UserInfo;
                
                using (var transaction = await _repository.DbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var negotiations = await _repository.DbContext.Set<OCP_Negotiation>()
                            .Where(n => negotiationIDs.Contains(n.NegotiationID))
                            .ToListAsync();
                        
                        if (!negotiations.Any())
                        {
                            return new WebResponseContent().Error("没有找到需要更新的协商记录");
                        }

                        foreach (var negotiation in negotiations)
                        {
                            negotiation.NegotiationStatus = newStatus;
                            negotiation.ModifyDate = System.DateTime.Now;
                            negotiation.Modifier = userInfo?.UserTrueName ?? "系统";
                        }
                        
                        await _repository.DbContext.SaveChangesAsync();
                        await transaction.CommitAsync();

                        HDLogHelper.Log("Negotiation_BatchUpdateStatus_Success", 
                            $"批量更新协商状态成功 - 更新数量: {negotiations.Count}, 新状态: {newStatus}", 
                            BusinessConstants.LogCategory.OrderCollaboration);

                        return new WebResponseContent().OK($"成功更新{negotiations.Count}条协商记录状态");
                    }
                    catch (System.Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
            catch (System.Exception ex)
            {
                HDLogHelper.Log("Negotiation_BatchUpdateStatus_Error", $"批量更新协商状态异常: {ex.Message}", BusinessConstants.LogCategory.OrderCollaboration);
                return new WebResponseContent().Error($"批量更新协商状态异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取协商状态选项
        /// </summary>
        /// <returns>协商状态选项</returns>
        public List<object> GetNegotiationStatusOptions()
        {
            return new List<object>
            {
                new { value = BusinessConstants.NegotiationStatus.Pending, text = "协商中" },
                new { value = BusinessConstants.NegotiationStatus.Approved, text = "已同意" },
                new { value = BusinessConstants.NegotiationStatus.Rejected, text = "拒绝" }
            };
        }

        /// <summary>
        /// 获取消息状态过滤选项
        /// </summary>
        /// <returns>消息状态过滤选项列表</returns>
        public List<object> GetMessageStatusFilterOptions()
        {
            return new List<object>
            {
                new { Value = "sent", Text = "已发送消息", Description = "所有协商记录", Count = 0 },
                new { Value = "pending", Text = "待回复消息", Description = "没有回复记录的协商", Count = 0 },
                new { Value = "overdue", Text = "已超期消息", Description = "没有回复且超过7天的协商", Count = 0 },
                new { Value = "replied", Text = "已回复消息", Description = "有回复记录的协商", Count = 0 }
            };
        }

        /// <summary>
        /// 获取按业务类型统计的协商消息数据
        /// </summary>
        /// <param name="messageStatus">消息状态过滤参数（可选）：sent-已发送消息, pending-待回复消息, overdue-已超期消息, replied-已回复消息</param>
        /// <returns>按业务类型统计的协商消息数据</returns>
        public async Task<BusinessTypeMessageStatisticsDto> GetNegotiationStatisticsByBusinessTypeAsync(string messageStatus = null)
        {
            try
            {
                var dbContext = _repository.DbContext;
                
                // 权限控制：只统计当前用户有权限查看的协商
                var currentUser = HDPro.Core.ManageUser.UserContext.Current;
                var currentUserId = currentUser?.UserId ?? 0;
                var currentUserName = currentUser?.UserInfo?.UserName ?? "";
                
                // 获取当前用户有权限查看的所有协商记录
                List<OCP_Negotiation> allNegotiations;
                
                // 超级管理员可以查看所有数据
                if (currentUser?.IsSuperAdmin == true)
                {
                    allNegotiations = await dbContext.Set<OCP_Negotiation>().ToListAsync();
                    HDLogHelper.Log("Negotiation_BusinessTypeStatistics_SuperAdmin", 
                        $"超级管理员按业务类型统计协商 - 用户ID: {currentUserId}, 无权限限制", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                }
                else
                {
                    allNegotiations = await dbContext.Set<OCP_Negotiation>()
                        .Where(x => x.CreateID == currentUserId || 
                                   x.AssignedResPerson == currentUserName ||
                                   x.DefaultResPerson == currentUserName)
                        .ToListAsync();
                    HDLogHelper.Log("Negotiation_BusinessTypeStatistics_Permission", 
                        $"协商按业务类型统计权限过滤 - 用户ID: {currentUserId}, 用户名: {currentUserName}", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                }
                
                // 获取当前用户有权限查看的协商ID列表
                var authorizedNegotiationIds = allNegotiations.Select(x => x.NegotiationID).ToList();
                
                // 获取所有回复记录的协商ID（只包含用户有权限查看的）
                var repliedNegotiationIds = await dbContext.Set<OCP_NegotiationReply>()
                    .Where(r => authorizedNegotiationIds.Contains(r.NegotiationID))
                    .Select(x => x.NegotiationID)
                    .Distinct()
                    .ToListAsync();

                // 根据MessageStatus参数过滤协商数据
                List<OCP_Negotiation> filteredNegotiations = allNegotiations;
                
                if (!string.IsNullOrWhiteSpace(messageStatus))
                {
                    switch (messageStatus.ToLower())
                    {
                        case "sent":
                            // 已发送消息：返回所有协商记录，无需额外过滤
                            break;

                        case "pending":
                            // 待回复消息：没有回复记录的协商
                            filteredNegotiations = allNegotiations
                                .Where(n => !repliedNegotiationIds.Contains(n.NegotiationID))
                                .ToList();
                            break;

                        case "overdue":
                            // 已超期消息：没有回复且超过7天的协商
                            var sevenDaysAgo = System.DateTime.Now.AddDays(-7);
                            filteredNegotiations = allNegotiations
                                .Where(n => !repliedNegotiationIds.Contains(n.NegotiationID) &&
                                           n.CreateDate.HasValue && 
                                           n.CreateDate.Value < sevenDaysAgo)
                                .ToList();
                            break;

                        case "replied":
                            // 已回复消息：有回复记录的协商
                            filteredNegotiations = allNegotiations
                                .Where(n => repliedNegotiationIds.Contains(n.NegotiationID))
                                .ToList();
                            break;

                        default:
                            // 默认返回所有记录
                            break;
                    }
                    
                    HDLogHelper.Log("Negotiation_BusinessTypeStatistics_MessageStatusFilter", 
                        $"协商按业务类型统计消息状态过滤 - 状态: {messageStatus}, 过滤后数量: {filteredNegotiations.Count}", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                }

                // 获取所有预定义的业务类型
                var allBusinessTypes = BusinessConstants.GetAllBusinessTypes();
                
                // 按业务类型分组统计现有数据
                var existingBusinessTypeGroups = filteredNegotiations
                    .GroupBy(x => x.BusinessType ?? "未知")
                    .ToDictionary(g => g.Key, g => g.ToList());

                var businessTypeList = new List<BusinessTypeStatisticsDto>();
                int totalSent = 0, totalPending = 0, totalOverdue = 0, totalReplied = 0;

                // 遍历所有预定义的业务类型，确保每个业务类型都有统计数据
                foreach (var businessTypeDefine in allBusinessTypes)
                {
                    var businessTypeCode = businessTypeDefine.Code;
                    var businessTypeName = businessTypeDefine.Name;
                    
                    // 获取该业务类型的协商记录，如果没有则为空列表
                    var negotiations = existingBusinessTypeGroups.ContainsKey(businessTypeCode) 
                        ? existingBusinessTypeGroups[businessTypeCode] 
                        : new List<OCP_Negotiation>();
                    
                    var sentCount = negotiations.Count;
                    var repliedCount = negotiations.Count(n => repliedNegotiationIds.Contains(n.NegotiationID));
                    var pendingCount = sentCount - repliedCount;
                    
                    // 计算超期数量（7天前创建且未回复的）
                    var sevenDaysAgo = System.DateTime.Now.AddDays(-7);
                    var overdueCount = negotiations.Count(n => 
                        n.CreateDate.HasValue && 
                        n.CreateDate.Value < sevenDaysAgo && 
                        !repliedNegotiationIds.Contains(n.NegotiationID));

                    var businessTypeStat = new BusinessTypeStatisticsDto
                    {
                        BusinessTypeCode = businessTypeCode,
                        BusinessTypeName = businessTypeName,
                        SentCount = sentCount,
                        PendingCount = pendingCount,
                        OverdueCount = overdueCount,
                        RepliedCount = repliedCount
                    };
                    
                    businessTypeList.Add(businessTypeStat);
                    
                    // 累计总数
                    totalSent += sentCount;
                    totalPending += pendingCount;
                    totalOverdue += overdueCount;
                    totalReplied += repliedCount;
                }

                // 处理未知业务类型（不在预定义列表中的）
                foreach (var kvp in existingBusinessTypeGroups)
                {
                    var businessTypeCode = kvp.Key;
                    var negotiations = kvp.Value;
                    
                    // 如果不在预定义业务类型中，则添加到统计中
                    if (!allBusinessTypes.Any(bt => bt.Code == businessTypeCode))
                    {
                        var sentCount = negotiations.Count;
                        var repliedCount = negotiations.Count(n => repliedNegotiationIds.Contains(n.NegotiationID));
                        var pendingCount = sentCount - repliedCount;
                        
                        var sevenDaysAgo = System.DateTime.Now.AddDays(-7);
                        var overdueCount = negotiations.Count(n => 
                            n.CreateDate.HasValue && 
                            n.CreateDate.Value < sevenDaysAgo && 
                            !repliedNegotiationIds.Contains(n.NegotiationID));

                        var businessTypeStat = new BusinessTypeStatisticsDto
                        {
                            BusinessTypeCode = businessTypeCode,
                            BusinessTypeName = MessageHelper.GetBusinessTypeText(businessTypeCode),
                            SentCount = sentCount,
                            PendingCount = pendingCount,
                            OverdueCount = overdueCount,
                            RepliedCount = repliedCount
                        };
                        
                        businessTypeList.Add(businessTypeStat);
                        
                        // 累计总数
                        totalSent += sentCount;
                        totalPending += pendingCount;
                        totalOverdue += overdueCount;
                        totalReplied += repliedCount;
                    }
                }

                // 创建全部消息统计
                var allMessagesStat = new BusinessTypeStatisticsDto
                {
                    BusinessTypeCode = "ALL",
                    BusinessTypeName = "全部消息",
                    SentCount = totalSent,
                    PendingCount = totalPending,
                    OverdueCount = totalOverdue,
                    RepliedCount = totalReplied
                };

                return new BusinessTypeMessageStatisticsDto
                {
                    AllMessages = allMessagesStat,
                    BusinessTypeList = businessTypeList,
                    StatisticsTime = System.DateTime.Now
                };
            }
            catch (System.Exception ex)
            {
                HDLogHelper.Log("Negotiation_BusinessTypeStatistics_Error", $"获取协商按业务类型统计数据异常: {ex.Message}", BusinessConstants.LogCategory.OrderCollaboration);
                throw;
            }
        }

        public override PageGridData<OCP_Negotiation> GetPageData(PageDataOptions options)
        {
            //options.Value可以从前台查询的方法提交一些其他参数放到value里面
            //前端提交方式，见文档：[生成页面文档->searchBefore方法]
            object extraValue = options.Value;

            //此处是从前台提交的原生的查询条件，这里可以自己过滤
            string messageStatus = ""; // 消息状态过滤参数
            QueryRelativeList = (List<SearchParameters> parameters) =>
            {
                foreach (var item in parameters)
                {
                    //如果需要取出某个查询值，并单独使用
                    if (item.Name == "MessageStatus" || item.Name == "messageStatus")
                    {
                        messageStatus = item.Value;
                        //清空字段值后框架不会使用此字段
                        item.Value = null;
                    }
                }
            };

            //此处与上面QuerySql只需要实现其中一个就可以了
            //查询前可以自已设定查询表达式的条件
            QueryRelativeExpression = (IQueryable<OCP_Negotiation> queryable) =>
            {
                // 权限控制：当前登录用户只能看到自己发起的或被指定为负责人的协商
                var currentUser = HDPro.Core.ManageUser.UserContext.Current;
                if (currentUser?.UserInfo != null)
                {
                    // 超级管理员不进行权限过滤，可以查看所有数据
                    if (!currentUser.IsSuperAdmin)
                    {
                        var currentUserId = currentUser.UserId;
                        var currentUserName = currentUser.UserInfo.UserName ?? "";
                        
                        // 只能查看：1. 自己创建的协商  2. 指定自己为负责人的协商
                        queryable = queryable.Where(x => 
                            x.CreateID == currentUserId || 
                            x.AssignedResPerson == currentUserName ||
                            x.DefaultResPerson == currentUserName);
                        
                        HDLogHelper.Log("Negotiation_Permission_Filter", 
                            $"协商权限过滤 - 用户ID: {currentUserId}, 用户名: {currentUserName}", 
                            BusinessConstants.LogCategory.OrderCollaboration);
                    }
                    else
                    {
                        HDLogHelper.Log("Negotiation_FullAccess",
                            $"超级管理员访问协商 - 用户ID: {currentUser.UserId}, 角色: [{string.Join(",", currentUser.RoleIds)}], 跳过权限过滤",
                            BusinessConstants.LogCategory.OrderCollaboration);
                    }
                }

                //或者直接写查询条件
                //queryable=queryable.Where(x=>条件)

                // 根据消息状态进行过滤
                if (!string.IsNullOrWhiteSpace(messageStatus))
                {
                    var dbContext = _repository.DbContext;
                    
                    // 获取所有有回复记录的协商ID
                    var repliedNegotiationIds = dbContext.Set<OCP_NegotiationReply>()
                        .Select(r => r.NegotiationID)
                        .Distinct();

                    switch (messageStatus.ToLower())
                    {
                        case "sent":
                            // 已发送消息：返回所有协商记录，无需额外过滤
                            break;

                        case "pending":
                            // 待回复消息：没有回复记录的协商
                            queryable = queryable.Where(n => !repliedNegotiationIds.Contains(n.NegotiationID));
                            break;

                        case "overdue":
                            // 已超期消息：没有回复且超过7天的协商（协商没有指定回复时间，使用固定7天规则）
                            var sevenDaysAgo = System.DateTime.Now.AddDays(-7);
                            queryable = queryable.Where(n => 
                                !repliedNegotiationIds.Contains(n.NegotiationID) &&
                                n.CreateDate.HasValue && 
                                n.CreateDate.Value < sevenDaysAgo);
                            break;

                        case "replied":
                            // 已回复消息：有回复记录的协商
                            queryable = queryable.Where(n => repliedNegotiationIds.Contains(n.NegotiationID));
                            break;

                        default:
                            // 默认返回所有记录
                            break;
                    }
                }

                //查看生成的sql语句：Console.Write(queryable.ToQueryString())
                return queryable;
            };

            var res = base.GetPageData(options);

            return res;
        }

        /// <summary>
        /// 根据业务类型和业务主键读取物料相关信息
        /// </summary>
        /// <param name="negotiation">协商实体</param>
        private void PopulateMaterialInfoFromTrackingTable(OCP_Negotiation negotiation)
        {
            if (string.IsNullOrWhiteSpace(negotiation.BusinessType) || string.IsNullOrWhiteSpace(negotiation.BusinessKey))
            {
                HDLogHelper.Log("Negotiation_MaterialInfo_Populate_Error", 
                    $"协商ID: {negotiation.NegotiationID}, 业务类型或业务主键为空，无法读取物料信息", 
                    BusinessConstants.LogCategory.OrderCollaboration);
                return;
            }

            try
            {
                var dbContext = _repository.DbContext;
                
                // 解析业务主键为FENTRYID
                if (!int.TryParse(negotiation.BusinessKey, out int entryId))
                {
                    HDLogHelper.Log("Negotiation_MaterialInfo_Populate_Error", 
                        $"协商ID: {negotiation.NegotiationID}, 业务主键格式错误: {negotiation.BusinessKey}", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                    return;
                }

                string materialNumber = null;
                string materialName = null;
                string specification = null;

                // 根据业务类型查询对应的跟踪表
                switch (negotiation.BusinessType)
                {
                    case BusinessConstants.BusinessType.OutSourcing: // 委外
                        var subOrderTracking = dbContext.Set<OCP_SubOrderUnFinishTrack>()
                            .FirstOrDefault(x => x.FENTRYID == entryId);
                        if (subOrderTracking != null)
                        {
                            materialNumber = subOrderTracking.MaterialNumber;
                            materialName = subOrderTracking.MaterialName;
                            specification = subOrderTracking.Specification;
                        }
                        break;

                    case BusinessConstants.BusinessType.Purchase: // 采购
                        var purchaseTracking = dbContext.Set<OCP_POUnFinishTrack>()
                            .FirstOrDefault(x => x.FENTRYID == entryId);
                        if (purchaseTracking != null)
                        {
                            materialNumber = purchaseTracking.MaterialCode; // 采购跟踪表使用MaterialCode字段
                            materialName = purchaseTracking.MaterialName;
                            specification = purchaseTracking.Specification;
                        }
                        break;

                    case BusinessConstants.BusinessType.Technology: // 技术
                        var techManagement = dbContext.Set<OCP_TechManagement>()
                            .FirstOrDefault(x => x.TechID == entryId);
                        if (techManagement != null)
                        {
                            materialNumber = techManagement.MaterialNumber;
                            materialName = techManagement.MaterialName;
                            // 技术管理表中没有Specification字段，暂时留空
                            specification = null;
                        }
                        break;

                    case BusinessConstants.BusinessType.Component: // 部件
                        var partTracking = dbContext.Set<OCP_PartUnFinishTracking>()
                            .FirstOrDefault(x => x.FENTRYID == entryId);
                        if (partTracking != null)
                        {
                            materialNumber = partTracking.MaterialCode; // 部件跟踪表使用MaterialCode字段
                            materialName = partTracking.MaterialName;
                            specification = partTracking.Specification;
                        }
                        break;

                    case BusinessConstants.BusinessType.Metalwork: // 金工
                        var jgTracking = dbContext.Set<OCP_JGUnFinishTrack>()
                            .FirstOrDefault(x => x.FENTRYID == entryId);
                        if (jgTracking != null)
                        {
                            materialNumber = jgTracking.MaterialNumber;
                            materialName = jgTracking.MaterialName;
                            specification = jgTracking.Specification;
                        }
                        break;

                    case BusinessConstants.BusinessType.Sales: // 销售
                        var orderTracking = dbContext.Set<OCP_OrderTracking>()
                            .FirstOrDefault(x => x.SOEntryID == entryId);
                        if (orderTracking != null)
                        {
                            materialNumber = orderTracking.MaterialNumber;
                            materialName = orderTracking.MaterialName;
                            specification = orderTracking.TopSpecification; // 销售跟踪表使用TopSpecification字段
                        }
                        break;

                    case BusinessConstants.BusinessType.Assembly: // 装配
                    case BusinessConstants.BusinessType.Planning: // 计划
                        // 如果有对应的跟踪表，可以在这里添加逻辑
                        HDLogHelper.Log("Negotiation_MaterialInfo_BusinessTypeNotSupported", 
                            $"协商ID: {negotiation.NegotiationID}, 业务类型: {negotiation.BusinessType} 暂不支持自动读取物料信息", 
                            BusinessConstants.LogCategory.OrderCollaboration);
                        break;

                    default:
                        HDLogHelper.Log("Negotiation_MaterialInfo_UnknownBusinessType", 
                            $"协商ID: {negotiation.NegotiationID}, 未知的业务类型: {negotiation.BusinessType}", 
                            BusinessConstants.LogCategory.OrderCollaboration);
                        break;
                }

                // 设置物料信息
                if (!string.IsNullOrWhiteSpace(materialNumber))
                {
                    negotiation.MaterialNumber = materialNumber;
                    negotiation.MaterialName = materialName;
                    negotiation.Specification = specification;
                    
                    HDLogHelper.Log("Negotiation_MaterialInfo_Populated", 
                        $"协商ID: {negotiation.NegotiationID}, 从{negotiation.BusinessType}跟踪表读取物料信息 - 物料编号: {materialNumber}, 物料名称: {materialName}, 规格: {specification}", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                }
                else
                {
                    HDLogHelper.Log("Negotiation_MaterialInfo_NotFound", 
                        $"协商ID: {negotiation.NegotiationID}, {negotiation.BusinessType}跟踪表中未找到业务主键: {negotiation.BusinessKey} 的记录", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                }
            }
            catch (Exception ex)
            {
                HDLogHelper.Log("Negotiation_MaterialInfo_Populate_Exception", 
                    $"从跟踪表读取物料信息异常 - 协商ID: {negotiation.NegotiationID}, 业务类型: {negotiation.BusinessType}, 业务主键: {negotiation.BusinessKey}, 错误: {ex.Message}", 
                    BusinessConstants.LogCategory.OrderCollaboration);
            }
        }




        /// <summary>
        /// 发送SignalR实时通知消息
        /// </summary>
        /// <param name="negotiation">协商实体</param>
        /// <param name="senderLoginName">发送者登录名（可选，用于异步调用时传入）</param>
        /// <param name="senderName">发送者显示名（可选，用于异步调用时传入）</param>
        /// <param name="senderId">发送者ID（可选，用于异步调用时传入）</param>
        /// <returns></returns>
        private async Task SendSignalRNotificationAsync(OCP_Negotiation negotiation, string senderLoginName = null, string senderName = null, int? senderId = null)
        {
            try
            {
                // 如果没有传入用户信息，尝试从当前上下文获取（同步调用时）
                if (string.IsNullOrEmpty(senderLoginName) || string.IsNullOrEmpty(senderName))
                {
                    var (currentLoginName, currentDisplayName) = MessageHelper.GetCurrentUserInfo();
                    senderLoginName = currentLoginName;
                    senderName = currentDisplayName;
                    senderId = HDPro.Core.ManageUser.UserContext.Current?.UserId;
                }
                
                // 确定接收者列表
                var receiverLoginNames = MessageHelper.DetermineReceivers(
                    negotiation.AssignedResPerson, 
                    negotiation.DefaultResPerson, 
                    negotiation.NegotiationID, 
                    "Negotiation");

                // 构建消息内容
                var messageTitle = BuildNegotiationSignalRTitle(negotiation);
                var messageContent = BuildNegotiationSignalRContent(negotiation, senderName);

                // 获取接收者用户ID列表
                var receiverUserIds = await GetUserIdsByLoginNamesAsync(receiverLoginNames);

                if (receiverUserIds.Any())
                {
                    // 构建SignalR消息数据
                    var channelData = new MessageChannelData()
                    {
                        UserIds = receiverUserIds,
                        Code = $"NEGOTIATION_{negotiation.NegotiationID}",
                        MessageNotification = new MessageNotification()
                        {
                            NotificationId = Guid.NewGuid(),
                            Title = messageTitle,
                            Content = messageContent,
                            NotificationType = NotificationType.协商,
                            BusinessFunction = "订单协商",
                            TableName = "OCP_Negotiation",
                            TableKey = negotiation.NegotiationID.ToString(),
                            LinkUrl = $"/order/negotiation/detail/{negotiation.NegotiationID}",
                            LinkType = "page",
                            Level = GetNegotiationNotificationLevel(negotiation.NegotiationType),
                            Creator = senderName,
                            CreateID = senderId
                        }
                    };

                    // 发送SignalR消息
                    MessageHelper.LogIntegrationStart("SignalR通知", "Negotiation", negotiation.NegotiationID, negotiation.BusinessType, senderName, receiverLoginNames);
                    
                    await _messageService.SendMessageAsync(channelData);
                    
                    MessageHelper.LogIntegrationSuccess("SignalR通知", "Negotiation", negotiation.NegotiationID, negotiation.BillNo, negotiation.BusinessType, receiverLoginNames);
                }
                else
                {
                    HDLogHelper.Log("Negotiation_SignalR_NoReceivers", 
                        $"协商SignalR通知未找到有效接收者 - 协商ID: {negotiation.NegotiationID}, 接收者登录名: {string.Join(",", receiverLoginNames)}", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                }
            }
            catch (System.Exception ex)
            {
                MessageHelper.LogIntegrationException("SignalR通知", "Negotiation", negotiation.NegotiationID, negotiation.BusinessType, ex);
            }
        }

        /// <summary>
        /// 构建协商SignalR消息标题
        /// </summary>
        /// <param name="negotiation">协商实体</param>
        /// <returns>SignalR消息标题</returns>
        private string BuildNegotiationSignalRTitle(OCP_Negotiation negotiation)
        {
            var businessTypeText = MessageHelper.GetBusinessTypeText(negotiation.BusinessType);
            var negotiationTypeText = negotiation.NegotiationType ?? "协商";
            
            return $"【协商通知】{businessTypeText}订单{negotiationTypeText}";
        }

        /// <summary>
        /// 构建协商SignalR消息简短内容
        /// </summary>
        /// <param name="negotiation">协商实体</param>
        /// <param name="senderName">发送者名称</param>
        /// <returns>SignalR消息简短内容</returns>
        private string BuildNegotiationSignalRContent(OCP_Negotiation negotiation, string senderName)
        {
            var businessTypeText = MessageHelper.GetBusinessTypeText(negotiation.BusinessType);
            var negotiationTypeText = negotiation.NegotiationType ?? "协商";
            
            return $"单据编号：{negotiation.BillNo ?? "未设置"} | " +
                   $"业务类型：{businessTypeText} | " +
                   $"协商类型：{negotiationTypeText} | " +
                   $"交货日期：{negotiation.DeliveryDate?.ToString("yyyy-MM-dd") ?? "未设置"} | " +
                   $"协商人：{senderName}";
        }

        /// <summary>
        /// 获取协商通知级别
        /// </summary>
        /// <param name="negotiationType">协商类型</param>
        /// <returns>通知级别</returns>
        private string GetNegotiationNotificationLevel(string negotiationType)
        {
            return negotiationType switch
            {
                "交期协商" => "warning",
                "质量协商" => "error",
                "技术协商" => "warning",
                _ => "info"
            };
        }

        /// <summary>
        /// 根据登录名获取用户ID列表
        /// </summary>
        /// <param name="loginNames">登录名列表</param>
        /// <returns>用户ID列表</returns>
        private async Task<List<int>> GetUserIdsByLoginNamesAsync(List<string> loginNames)
        {
            try
            {
                if (loginNames == null || !loginNames.Any())
                    return new List<int>();

                var dbContext = _repository.DbContext;
                var userIds = await dbContext.Set<Sys_User>()
                    .Where(u => loginNames.Contains(u.UserName))
                    .Select(u => u.User_Id)
                    .ToListAsync();

                return userIds;
            }
            catch (System.Exception ex)
            {
                HDLogHelper.Log("Negotiation_GetUserIds_Error", 
                    $"根据登录名获取用户ID失败: {ex.Message}, 登录名: {string.Join(",", loginNames ?? new List<string>())}", 
                    BusinessConstants.LogCategory.OrderCollaboration);
                return new List<int>();
            }
        }

        /// <summary>
        /// 获取当前用户协商情况统计（管理员可以看到所有人的统计）
        /// </summary>
        /// <returns>当前用户协商情况统计数据</returns>
        public async Task<List<object>> GetUserNegotiationStatisticsAsync()
        {
            try
            {
                var currentUser = HDPro.Core.ManageUser.UserContext.Current;
                if (currentUser?.UserInfo == null)
                {
                    return GetDefaultNegotiationStatisticsResult();
                }

                var currentUserId = currentUser.UserId;
                var currentUserName = currentUser.UserInfo.UserName ?? "";
                var currentTime = DateTime.Now;

                var dbContext = _repository.DbContext;

                // 获取所有有回复记录的协商ID
                var repliedNegotiationIds = await dbContext.Set<OCP_NegotiationReply>()
                    .Select(r => r.NegotiationID)
                    .Distinct()
                    .ToListAsync();

                // 根据用户权限获取协商数据
                List<OCP_Negotiation> sentNegotiations;
                List<OCP_Negotiation> receivedNegotiations;

                if (currentUser.IsSuperAdmin)
                {
                    // 超级管理员可以查看所有协商统计
                    sentNegotiations = await dbContext.Set<OCP_Negotiation>()
                        .ToListAsync();
                    
                    // 管理员的"被协商单"统计为空，因为管理员查看的是全局统计
                    receivedNegotiations = new List<OCP_Negotiation>();
                    
                    HDLogHelper.Log("Negotiation_AdminStatistics",
                        $"超级管理员查看全局协商统计 - 用户ID: {currentUserId}, 角色: [{string.Join(",", currentUser.RoleIds)}]",
                        BusinessConstants.LogCategory.OrderCollaboration);
                }
                else
                {
                    // 普通用户只能查看与自己相关的协商
                    sentNegotiations = await dbContext.Set<OCP_Negotiation>()
                        .Where(x => x.CreateID == currentUserId)
                        .ToListAsync();

                    receivedNegotiations = await dbContext.Set<OCP_Negotiation>()
                        .Where(x => x.AssignedResPerson == currentUserName || x.DefaultResPerson == currentUserName)
                        .Where(x => x.CreateID != currentUserId) // 排除自己发出的协商
                        .ToListAsync();
                }

                // 计算发出协商的统计
                var sentTotal = sentNegotiations.Count;
                var sentPending = sentNegotiations.Count(n => !repliedNegotiationIds.Contains(n.NegotiationID));
                var sentReplied = sentNegotiations.Count(n => repliedNegotiationIds.Contains(n.NegotiationID));
                
                // 计算发出协商中的超期数量（超过7天未回复）
                var sevenDaysAgo = currentTime.AddDays(-7);
                var sentOverdue = sentNegotiations.Count(n => 
                    !repliedNegotiationIds.Contains(n.NegotiationID) &&
                    n.CreateDate.HasValue && 
                    n.CreateDate.Value < sevenDaysAgo);

                // 计算被协商单的统计
                var receivedPending = receivedNegotiations.Count(n => !repliedNegotiationIds.Contains(n.NegotiationID));
                
                // 计算被协商单中的超时数量（超过7天未回复）
                var receivedOverdue = receivedNegotiations.Count(n => 
                    !repliedNegotiationIds.Contains(n.NegotiationID) &&
                    n.CreateDate.HasValue && 
                    n.CreateDate.Value < sevenDaysAgo);

                // 构建返回数据，格式与催单统计保持一致
                var result = new List<object>
                {
                    new { name = "协商总数", value = sentTotal },
                    new { name = "协商待回复", value = sentPending },
                    new { name = "协商已超期", value = sentOverdue },
                    new { name = "协商已回复", value = sentReplied },
                    new { name = "被协商单待回复", value = receivedPending },
                    new { name = "被协商单已超时", value = receivedOverdue }
                };

                // 记录统计日志
                var statisticsType = currentUser.IsSuperAdmin ? "全局协商统计" : "用户协商统计";
                MessageHelper.LogIntegrationSuccess(statisticsType, "Negotiation", currentUserId, 
                    currentUserName, "统计查询", null);

                if (currentUser.IsSuperAdmin)
                {
                    HDLogHelper.Log("Negotiation_AdminStatistics_Detail", 
                        $"管理员全局协商统计详情 - 用户ID: {currentUserId}, 用户名: {currentUserName}, " +
                        $"全部协商总数: {sentTotal}, 全部待回复: {sentPending}, 全部已超期: {sentOverdue}, 全部已回复: {sentReplied}", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                }
                else
                {
                    HDLogHelper.Log("Negotiation_UserStatistics_Detail", 
                        $"用户协商统计详情 - 用户ID: {currentUserId}, 用户名: {currentUserName}, " +
                        $"协商总数: {sentTotal}, 协商待回复: {sentPending}, 协商已超期: {sentOverdue}, 协商已回复: {sentReplied}, " +
                        $"被协商单待回复: {receivedPending}, 被协商单已超时: {receivedOverdue}", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                }

                return result;
            }
            catch (Exception ex)
            {
                // 记录异常日志
                MessageHelper.LogIntegrationException("用户协商统计", "Negotiation", 
                    HDPro.Core.ManageUser.UserContext.Current?.UserId ?? 0, "统计查询", ex);
                
                return GetDefaultNegotiationStatisticsResult();
            }
        }

        /// <summary>
        /// 获取默认协商统计结果（异常情况下返回）
        /// </summary>
        /// <returns>默认统计结果</returns>
        private List<object> GetDefaultNegotiationStatisticsResult()
        {
            return new List<object>
            {
                new { name = "协商总数", value = 0 },
                new { name = "协商待回复", value = 0 },
                new { name = "协商已超期", value = 0 },
                new { name = "协商已回复", value = 0 },
                new { name = "被协商单待回复", value = 0 },
                new { name = "被协商单已超时", value = 0 }
            };
        }
    }
}