/*
 *所有关于OCP_UrgentOrder类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*OCP_UrgentOrderService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using HDPro.CY.Order.IServices.SRM;
using HDPro.CY.Order.IServices.OA;
using HDPro.CY.Order.IServices;
using HDPro.CY.Order.Models;
using System.Threading.Tasks;
using HDPro.Utilities;
using HDPro.CY.Order.Services.OrderCollaboration.Common;
using System.Collections.Generic;
using HDPro.Utilities.Extensions;
using HDPro.Core.Enums;
using HDPro.CY.Order.Services.OA;
using HDPro.Core.SignalR;

namespace HDPro.CY.Order.Services
{
    public partial class OCP_UrgentOrderService
    {
        private readonly IOCP_UrgentOrderRepository _repository;//访问数据库
        private readonly ISRMIntegrationService _srmIntegrationService;//SRM集成服务
        private readonly IOAIntegrationService _oaIntegrationService;//OA集成服务
        private readonly IOCP_PurchaseSupplierMappingService _purchaseSupplierMappingService;//供应商映射服务
        private readonly IOCP_TechManagementService _techManagementService;//技术管理服务
        private readonly IMessageService _messageService;//SignalR消息服务
        private readonly WebResponseContent webResponse = WebResponseContent.Instance;

        [ActivatorUtilitiesConstructor]
        public OCP_UrgentOrderService(
            IOCP_UrgentOrderRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            ISRMIntegrationService srmIntegrationService,
            IOAIntegrationService oaIntegrationService,
            IOCP_PurchaseSupplierMappingService purchaseSupplierMappingService,
            IOCP_TechManagementService techManagementService,
            IMessageService messageService
            )
        : base(dbRepository, httpContextAccessor)
        {
            _repository = dbRepository;
            _srmIntegrationService = srmIntegrationService;
            _oaIntegrationService = oaIntegrationService;
            _purchaseSupplierMappingService = purchaseSupplierMappingService;
            _techManagementService = techManagementService;
            _messageService = messageService;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        /// <summary>
        /// 重写CY.Order项目特有的初始化逻辑
        /// 可在此处添加OCP_UrgentOrder特有的初始化代码
        /// </summary>
        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
            // 在此处添加OCP_UrgentOrder特有的初始化逻辑
        }
      
        public override PageGridData<OCP_UrgentOrder> GetPageData(PageDataOptions options)
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
            QueryRelativeExpression = (IQueryable<OCP_UrgentOrder> queryable) =>
            {
                //当前用户只能操作自己(与下级角色)创建的数据,如:查询、删除、修改等操作
                //IQueryable<int> userQuery = RoleContext.GetCurrentAllChildUser();
                //queryable = queryable.Where(x => x.CreateID == UserContext.Current.UserId || userQuery.Contains(x.CreateID ?? 0));

                // 权限控制：当前登录用户只能看到自己发起的或被指定为负责人的催单
                var currentUser = HDPro.Core.ManageUser.UserContext.Current;
                if (currentUser?.UserInfo != null)
                {
                    // 超级管理员不进行权限过滤，可以查看所有数据
                    if (!currentUser.IsSuperAdmin)
                    {
                        var currentUserId = currentUser.UserId;
                        var currentUserName = currentUser.UserInfo.UserName ?? "";
                        
                        // 只能查看：1. 自己创建的催单  2. 指定自己为负责人的催单
                        queryable = queryable.Where(x => 
                            x.CreateID == currentUserId || 
                            x.AssignedResPerson == currentUserName ||
                            x.DefaultResPerson == currentUserName);
                        
                        HDLogHelper.Log("UrgentOrder_Permission_Filter", 
                            $"催单权限过滤 - 用户ID: {currentUserId}, 用户名: {currentUserName}", 
                            BusinessConstants.LogCategory.OrderCollaboration);
                    }
                    else
                    {
                        HDLogHelper.Log("UrgentOrder_FullAccess",
                            $"超级管理员访问催单 - 用户ID: {currentUser.UserId}, 角色: [{string.Join(",", currentUser.RoleIds)}], 跳过权限过滤",
                            BusinessConstants.LogCategory.OrderCollaboration);
                    }
                }

                //或者直接写查询条件
                //queryable=queryable.Where(x=>条件)

                // 根据消息状态进行过滤
                if (!string.IsNullOrWhiteSpace(messageStatus))
                {
                    var dbContext = _repository.DbContext;
                    
                    // 获取所有有回复记录的催单ID
                    var repliedUrgentOrderIds = dbContext.Set<OCP_UrgentOrderReply>()
                        .Select(r => r.UrgentOrderID)
                        .Distinct();

                    switch (messageStatus.ToLower())
                    {
                        case "sent":
                            // 已发送消息：返回所有催单记录，无需额外过滤
                            break;

                        case "pending":
                            // 待回复消息：没有回复记录的催单
                            queryable = queryable.Where(u => !repliedUrgentOrderIds.Contains(u.UrgentOrderID));
                            break;

                        case "overdue":
                            // 已超期消息：没有回复且超过指定回复时间的催单
                            queryable = queryable.Where(u => 
                                !repliedUrgentOrderIds.Contains(u.UrgentOrderID) &&
                                u.AssignedReplyTime.HasValue && 
                                u.TimeUnit.HasValue && 
                                u.CreateDate.HasValue);
                            
                            // 注意：由于EF Core无法直接转换复杂的时间计算，这里先筛选出基础条件
                            // 具体的超期判断会在后续的数据处理中进行
                            break;

                        case "replied":
                            // 已回复消息：有回复记录的催单
                            queryable = queryable.Where(u => repliedUrgentOrderIds.Contains(u.UrgentOrderID));
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
            
            // 对于超期消息，需要在内存中进行进一步过滤
            if (!string.IsNullOrWhiteSpace(messageStatus) && 
                (messageStatus.ToLower() == "overdue" || messageStatus == "已超期消息"))
            {
                var currentTime = DateTime.Now;
                var dbContext = _repository.DbContext;
                
                // 获取所有有回复记录的催单ID
                var repliedUrgentOrderIds = dbContext.Set<OCP_UrgentOrderReply>()
                    .Select(r => r.UrgentOrderID)
                    .Distinct()
                    .ToList();

                // 在内存中过滤超期的记录
                res.rows = res.rows.Where(u => 
                {
                    // 检查是否已回复
                    if (repliedUrgentOrderIds.Contains(u.UrgentOrderID))
                        return false;

                    // 检查是否有指定回复时间
                    if (!u.AssignedReplyTime.HasValue || !u.TimeUnit.HasValue || !u.CreateDate.HasValue)
                        return false;

                    // 计算截止时间
                    var deadline = CalculateDeadline(u.CreateDate.Value, u.AssignedReplyTime.Value, u.TimeUnit.Value);
                    
                    // 判断是否超期
                    return currentTime > deadline;
                }).ToList();
                
                // 更新总记录数
                res.total = res.rows.Count;
            }
            return res;
        }
        /// <summary>
        /// 重写CY.Order项目通用数据验证方法
        /// 可在此处添加OCP_UrgentOrder特有的数据验证逻辑
        /// </summary>
        /// <param name="entity">要验证的实体</param>
        /// <returns>验证结果</returns>
        protected override WebResponseContent ValidateCYOrderEntity(OCP_UrgentOrder entity)
        {
            var response = base.ValidateCYOrderEntity(entity);
            
            // 在此处添加OCP_UrgentOrder特有的数据验证逻辑
            
            return response;
        }

        public override WebResponseContent Add(SaveModel saveDataModel)
        {
                // 在保存数据库前的操作，所有数据都验证通过了，这一步执行完就执行数据库保存
            AddOnExecuting = (OCP_UrgentOrder order, object list) =>
            {
                if(order.AssignedResPerson.IsNullOrEmptyOrWhiteSpace())
                {
                    order.AssignedResPerson=order.DefaultResPerson;
                    order.AssignedResPersonName=order.DefaultResPersonName;
        
                }
                order.UrgentStatus = BusinessConstants.UrgentOrderStatus.Pending;
                order.IsSendSRM = 0; // 初始化为未推送SRM
                
                // 根据业务类型和业务主键读取物料相关信息
                PopulateMaterialInfoFromTrackingTable(order);
                
                return webResponse.OK();
            };

            var res = base.Add(saveDataModel);
            
            //这里在保存成功后做一些其他操作
            if (res.Status)
            {
                var urgentOrder = saveDataModel.MainData.DicToEntity<OCP_UrgentOrder>();
                if (urgentOrder != null)
                {
                    // 在Task.Run之前获取用户信息，避免上下文丢失
                    var (currentUserLoginName, currentUserDisplayName) = MessageHelper.GetCurrentUserInfo();
                    var currentUserId = HDPro.Core.ManageUser.UserContext.Current?.UserId;
                    
                    // 所有业务类型：发送通知消息
                    Task.Run(async () =>
                    {
                        try
                        {
                            // 1. 发送SignalR实时通知
                            await SendSignalRNotificationAsync(urgentOrder, currentUserLoginName, currentUserDisplayName, currentUserId);
                            
                            // 2. 发送OA消息通知
                            await SendOANotificationAsync(urgentOrder, currentUserLoginName, currentUserDisplayName);
                        }
                        catch (System.Exception ex)
                        {
                            MessageHelper.LogIntegrationException("通知消息", "UrgentOrder", urgentOrder.UrgentOrderID, urgentOrder.BusinessType, ex);
                        }
                    });
                    // 判断业务类型：委外和采购还需要发送SRM消息
                    // 增加条件：发起人（当前用户）所在部门为物资供应中心
                    var requiresSRMIntegration = BusinessConstants.RequiresSRMIntegration(urgentOrder.BusinessType);
                    var isInMaterialSupplyCenter = BusinessConstants.IsCurrentUserInMaterialSupplyCenter();
                    var (loginName, displayName) = MessageHelper.GetCurrentUserInfo();

                    // 记录SRM集成条件检查日志
                    HDLogHelper.Log("SRM_Integration_Check",
                        $"SRM集成条件检查 - 催单ID: {urgentOrder.UrgentOrderID}, 业务类型: {urgentOrder.BusinessType}, " +
                        $"发起人: {displayName}, 需要SRM集成: {requiresSRMIntegration}, 是否属于物资供应中心: {isInMaterialSupplyCenter}",
                        BusinessConstants.LogCategory.OrderCollaboration);

                    if (requiresSRMIntegration && isInMaterialSupplyCenter)
                    {
                        // 委外和采购业务且发起人属于物资供应中心：发送SRM消息
                        Task.Run(async () =>
                        {
                            try
                            {
                                //1. 发送SRM消息
                                await ProcessUrgentOrderWithSRMAsync(urgentOrder);
                            }
                            catch (System.Exception ex)
                            {
                                MessageHelper.LogIntegrationException("集成服务", "UrgentOrder", urgentOrder.UrgentOrderID, urgentOrder.BusinessType, ex);
                            }
                        });
                    }
                    else
                    {

                    }
                }
            }
            return res;
        }

        public async Task<WebResponseContent> BatchAddAsync(List<OCP_UrgentOrder> urgentOrders)
        {
            if (urgentOrders == null || !urgentOrders.Any())
            {
                return webResponse.Error("催单列表不能为空");
            }

            var successCount = 0;
            var failedItems = new List<object>();
            var currentTime = DateTime.Now;
            var currentUser = HDPro.Core.ManageUser.UserContext.Current;
            var currentUserId = currentUser?.UserId ?? 1;
            var currentUserName = currentUser?.UserInfo?.UserTrueName ?? "系统";

            using (var transaction = _repository.DbContext.Database.BeginTransaction())
            {
                try
                {
                    foreach (var order in urgentOrders)
                    {
                        try
                        {
                            // 设置基础信息
                            if (string.IsNullOrWhiteSpace(order.AssignedResPerson))
                            {
                                order.AssignedResPerson = order.DefaultResPerson;
                                order.AssignedResPersonName = order.DefaultResPersonName;
                            }
                            
                            order.UrgentStatus = BusinessConstants.UrgentOrderStatus.Pending;
                            order.IsSendSRM = 0; // 初始化为未推送SRM
                            order.CreateID = currentUserId;
                            order.CreateDate = currentTime;
                            order.Creator = currentUserName;
                            
                            // 根据业务类型和业务主键读取物料相关信息
                            PopulateMaterialInfoFromTrackingTable(order);
                            
                            // 添加到数据库
                            _repository.Add(order);
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            failedItems.Add(new 
                            { 
                                BillNo = order.BillNo,
                                Seq = order.Seq,
                                Error = ex.Message 
                            });
                            HDLogHelper.Log("UrgentOrder_BatchAdd_ItemError", 
                                $"批量添加催单单项失败 - 单据号: {order.BillNo}, 行号: {order.Seq}, 错误: {ex.Message}", 
                                BusinessConstants.LogCategory.OrderCollaboration);
                        }
                    }

                    // 保存所有成功的记录
                    if (successCount > 0)
                    {
                        await _repository.SaveChangesAsync();
                        transaction.Commit();
                        
                        HDLogHelper.Log("UrgentOrder_BatchAdd_Success", 
                            $"批量添加催单成功 - 成功数量: {successCount}, 失败数量: {failedItems.Count}", 
                            BusinessConstants.LogCategory.OrderCollaboration);
                    }
                    else
                    {
                        transaction.Rollback();
                        return webResponse.ErrorData("所有催单添加失败",failedItems);
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    HDLogHelper.Log("UrgentOrder_BatchAdd_TransactionError", 
                        $"批量添加催单事务失败: {ex.Message}", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                    return webResponse.Error($"批量添加催单失败: {ex.Message}");
                }
            }
            var isInMaterialSupplyCenter = BusinessConstants.IsCurrentUserInMaterialSupplyCenter();
            // 在Task.Run之前获取用户信息，避免上下文丢失
            var (currentUserLoginName, currentUserDisplayName) = MessageHelper.GetCurrentUserInfo();
            
            // 异步处理消息发送（不影响主流程）
            var addedOrders = urgentOrders.Take(successCount).ToList();
            _ = Task.Run(async () =>
            {
                try
                {
                    await ProcessBatchNotificationsAsync(addedOrders, isInMaterialSupplyCenter, currentUserLoginName, currentUserDisplayName, currentUserId);
                }
                catch (Exception ex)
                {
                    HDLogHelper.Log("UrgentOrder_BatchAdd_NotificationError", 
                        $"批量添加催单后台通知处理异常: {ex.Message}", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                }
            });

            var result = new
            {
                SuccessCount = successCount,
                FailedCount = failedItems.Count,
                FailedItems = failedItems
            };

            if (failedItems.Any())
            {
                return webResponse.OK($"批量添加完成，成功 {successCount} 条，失败 {failedItems.Count} 条", result);
            }
            else
            {
                return webResponse.OK($"批量添加成功，共添加 {successCount} 条催单", result);
            }
        }

        /// <summary>
        /// 处理需要SRM集成的催单（委外和采购且发起人属于物资供应中心）
        /// </summary>
        /// <param name="urgentOrder">催单实体</param>
        /// <returns></returns>
        private async Task ProcessUrgentOrderWithSRMAsync(OCP_UrgentOrder urgentOrder)
        {
            var (loginName, displayName) = MessageHelper.GetCurrentUserInfo();

            // 1. 发送SRM消息
            var orderAskData = new OrderAskData
            {
                AskId = urgentOrder.UrgentOrderID.ToString(),
                OrderNo = urgentOrder.BillNo ?? "",
                OrderLine = int.TryParse(urgentOrder.Seq, out int line) ? line : 1,
                UrgencyLevel = urgentOrder.UrgencyLevel ?? "",
                ServiceType = MessageHelper.GetUrgentTypeText(urgentOrder.UrgentType), // 根据催单类型传递stype参数
                Times = "1", // 第一次催单
                Remark = urgentOrder.UrgentContent ?? urgentOrder.Remarks ?? ""
            };

            MessageHelper.LogIntegrationStart("SRM推送", "UrgentOrder", urgentOrder.UrgentOrderID, urgentOrder.BusinessType, displayName);

            var srmResult = await _srmIntegrationService.PushSingleOrderAskAsync(orderAskData, displayName);

            if (srmResult.Status)
            {
                // SRM推送成功，更新标记
                await UpdateUrgentOrderSRMStatusAsync(urgentOrder.UrgentOrderID, 1);
                MessageHelper.LogIntegrationSuccess("SRM推送", "UrgentOrder", urgentOrder.UrgentOrderID, urgentOrder.BillNo, urgentOrder.BusinessType);
            }
            else
            {
                MessageHelper.LogIntegrationError("SRM推送", "UrgentOrder", urgentOrder.UrgentOrderID, urgentOrder.BillNo, urgentOrder.BusinessType, srmResult.Message);
            }
        }

        /// <summary>
        /// 批量处理通知消息
        /// </summary>
        /// <param name="urgentOrders">催单列表</param>
        /// <param name="isInMaterialSupplyCenter">是否在物资供应中心</param>
        /// <param name="senderLoginName">发送者登录名</param>
        /// <param name="senderDisplayName">发送者显示名</param>
        /// <param name="senderId">发送者ID</param>
        /// <returns></returns>
        private async Task ProcessBatchNotificationsAsync(List<OCP_UrgentOrder> urgentOrders, bool isInMaterialSupplyCenter, string senderLoginName, string senderDisplayName, int? senderId)
        {
            if (urgentOrders == null || !urgentOrders.Any())
                return;
            
            foreach (var urgentOrder in urgentOrders)
            {
                try
                {
                    // 1. 发送SignalR实时通知（所有业务类型）
                    await SendSignalRNotificationAsync(urgentOrder, senderLoginName, senderDisplayName, senderId);
                    
                    // 2. 发送OA消息通知（所有业务类型）
                    await SendOANotificationAsync(urgentOrder, senderLoginName, senderDisplayName);
                    
                    // 3. 判断业务类型：委外和采购还需要发送SRM消息
                    var requiresSRMIntegration = BusinessConstants.RequiresSRMIntegration(urgentOrder.BusinessType);
                   

                    if (requiresSRMIntegration && isInMaterialSupplyCenter)
                    {
                        await ProcessUrgentOrderWithSRMAsync(urgentOrder);
                    }
                }
                catch (Exception ex)
                {
                    HDLogHelper.Log("UrgentOrder_BatchNotification_Error", 
                        $"批量通知处理失败 - 催单ID: {urgentOrder.UrgentOrderID}, 单据号: {urgentOrder.BillNo}, 错误: {ex.Message}", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                }
            }
        }

        /// <summary>
        /// 发送SignalR实时通知消息
        /// </summary>
        /// <param name="urgentOrder">催单实体</param>
        /// <returns></returns>
        private async Task SendSignalRNotificationAsync(OCP_UrgentOrder urgentOrder, string senderLoginName = null, string senderName = null, int? senderId = null)
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
                    urgentOrder.AssignedResPerson, 
                    urgentOrder.DefaultResPerson, 
                    urgentOrder.UrgentOrderID, 
                    "UrgentOrder");

                // 构建消息内容
                var messageTitle = MessageHelper.BuildUrgentOrderSignalRTitle(urgentOrder);
                var messageContent = MessageHelper.BuildUrgentOrderSignalRContent(urgentOrder, senderName);

                // 获取接收者用户ID列表
                var receiverUserIds = await GetUserIdsByLoginNamesAsync(receiverLoginNames);

                if (receiverUserIds.Any())
                {
                    // 构建SignalR消息数据
                    var channelData = new MessageChannelData()
                    {
                        UserIds = receiverUserIds,
                        Code = $"URGENT_ORDER_{urgentOrder.UrgentOrderID}",
                        MessageNotification = new MessageNotification()
                        {
                            NotificationId = Guid.NewGuid(),
                            Title = messageTitle,
                            Content = messageContent,
                            NotificationType = NotificationType.催单,
                            BusinessFunction = "订单催单",
                            TableName = "OCP_UrgentOrder",
                            TableKey = urgentOrder.UrgentOrderID.ToString(),
                            LinkUrl = $"/order/urgent/detail/{urgentOrder.UrgentOrderID}",
                            LinkType = "page",
                            Level = MessageHelper.GetUrgencyLevelText(urgentOrder.UrgencyLevel),
                            Creator = senderName,
                            CreateID = senderId
                        }
                    };

                    // 发送SignalR消息
                    MessageHelper.LogIntegrationStart("SignalR通知", "UrgentOrder", urgentOrder.UrgentOrderID, urgentOrder.BusinessType, senderName, receiverLoginNames);
                    
                    await _messageService.SendMessageAsync(channelData);
                    
                    MessageHelper.LogIntegrationSuccess("SignalR通知", "UrgentOrder", urgentOrder.UrgentOrderID, urgentOrder.BillNo, urgentOrder.BusinessType, receiverLoginNames);
                }
                else
                {
                    HDLogHelper.Log("UrgentOrder_SignalR_NoReceivers", 
                        $"催单SignalR通知未找到有效接收者 - 催单ID: {urgentOrder.UrgentOrderID}, 接收者登录名: {string.Join(",", receiverLoginNames)}", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                }
            }
            catch (System.Exception ex)
            {
                MessageHelper.LogIntegrationException("SignalR通知", "UrgentOrder", urgentOrder.UrgentOrderID, urgentOrder.BusinessType, ex);
            }
        }

        /// <summary>
        /// 发送OA通知消息
        /// </summary>
        /// <param name="urgentOrder">催单实体</param>
        /// <param name="senderLoginName">发送者登录名（可选，用于异步调用时传入）</param>
        /// <param name="senderName">发送者显示名（可选，用于异步调用时传入）</param>
        /// <returns></returns>
        private async Task SendOANotificationAsync(OCP_UrgentOrder urgentOrder, string senderLoginName = null, string senderName = null)
        {
            try
            {
                // 如果没有传入用户信息，尝试从当前上下文获取（同步调用时）
                if (string.IsNullOrEmpty(senderLoginName) || string.IsNullOrEmpty(senderName))
                {
                    var (currentLoginName, currentDisplayName) = MessageHelper.GetCurrentUserInfo();
                    senderLoginName = currentLoginName;
                    senderName = currentDisplayName;
                }
                
                // 确定接收者列表
                var receiverLoginNames = MessageHelper.DetermineReceivers(
                    urgentOrder.AssignedResPerson, 
                    urgentOrder.DefaultResPerson, 
                    urgentOrder.UrgentOrderID, 
                    "UrgentOrder");

                // 构建消息内容
                var messageContent = MessageHelper.BuildUrgentOrderMessage(urgentOrder, senderName);

                // 同时发送原OA和股份OA消息
                MessageHelper.LogIntegrationStart("双OA系统通知", "UrgentOrder", urgentOrder.UrgentOrderID, urgentOrder.BusinessType, senderName, receiverLoginNames);

                var result = await _oaIntegrationService.SendToBothOASystemsAsync(
                    senderLoginName,
                    receiverLoginNames,
                    messageContent);

                if (result.Status)
                {
                    MessageHelper.LogIntegrationSuccess("双OA系统通知", "UrgentOrder", urgentOrder.UrgentOrderID, urgentOrder.BillNo, urgentOrder.BusinessType, receiverLoginNames);
                    
                    // 记录详细发送结果
                    if (result.Data is BothOASystemsResult bothResult)
                    {
                        HDLogHelper.Log("UrgentOrder_BothOA_Detail", $"催单双OA发送详情 - 催单ID: {urgentOrder.UrgentOrderID}, " +
                            $"原OA: {(bothResult.OriginalOA?.Success == true ? "成功" : "失败")} - {bothResult.OriginalOA?.Message}, " +
                            $"股份OA: {(bothResult.ShareholderOA?.Success == true ? "成功" : "失败")} - {bothResult.ShareholderOA?.Message}", 
                            BusinessConstants.LogCategory.OrderCollaboration);
                    }
                }
                else
                {
                    MessageHelper.LogIntegrationError("双OA系统通知", "UrgentOrder", urgentOrder.UrgentOrderID, urgentOrder.BillNo, urgentOrder.BusinessType, result.Message);
                }
            }
            catch (System.Exception ex)
            {
                MessageHelper.LogIntegrationException("OA通知", "UrgentOrder", urgentOrder.UrgentOrderID, urgentOrder.BusinessType, ex);
            }
        }



        /// <summary>
        /// 发送订单相关的OA消息（通用方法）
        /// </summary>
        /// <param name="senderLoginName">发送者登录名</param>
        /// <param name="receiverLoginNames">接收者登录名列表</param>
        /// <param name="orderNo">订单号</param>
        /// <param name="orderType">订单类型</param>
        /// <param name="messageType">消息类型</param>
        /// <param name="customContent">自定义内容</param>
        /// <returns></returns>
        public async Task<WebResponseContent> SendOrderOAMessageAsync(
            string senderLoginName,
            List<string> receiverLoginNames,
            string orderNo,
            string orderType,
            string messageType,
            string customContent = null)
        {
            try
            {
                HDLogHelper.Log("Order_OA_Start", $"开始发送订单OA消息 - 发送者: {senderLoginName}, 订单号: {orderNo}, 消息类型: {messageType}", "OrderCollaboration");
                
                var result = await _oaIntegrationService.SendOrderMessageAsync(
                    senderLoginName, 
                    orderNo, 
                    orderType, 
                    messageType, 
                    receiverLoginNames, 
                    customContent);

                if (result.Status)
                {
                    HDLogHelper.Log("Order_OA_Success", $"订单OA消息发送成功 - 订单号: {orderNo}, 消息类型: {messageType}, 接收者: {string.Join(",", receiverLoginNames)}", "OrderCollaboration");
                }
                else
                {
                    HDLogHelper.Log("Order_OA_Error", $"订单OA消息发送失败 - 订单号: {orderNo}, 消息类型: {messageType}, 错误信息: {result.Message}", "OrderCollaboration");
                }

                return result;
            }
            catch (System.Exception ex)
            {
                HDLogHelper.Log("Order_OA_Exception", $"发送订单OA消息异常 - 订单号: {orderNo}, 消息类型: {messageType}, 错误详情: {ex.Message}", "OrderCollaboration");
                return new WebResponseContent().Error($"发送OA消息异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取催单消息统计数据
        /// </summary>
        /// <returns>催单统计数据</returns>
        public async Task<MessageStatisticsDto> GetUrgentOrderStatisticsAsync()
        {
            try
            {
                // 使用当前Repository的DbContext统一查询，避免多个Repository的并发访问问题
                var dbContext = _repository.DbContext;

                // 权限控制：只统计当前用户有权限查看的催单
                var currentUser = HDPro.Core.ManageUser.UserContext.Current;
                var currentUserId = currentUser?.UserId ?? 0;
                var currentUserName = currentUser?.UserInfo?.UserName ?? "";

                IQueryable<OCP_UrgentOrder> authorizedUrgentOrders;

                // 超级管理员可以查看所有数据
                if (currentUser?.IsSuperAdmin == true)
                {
                    authorizedUrgentOrders = dbContext.Set<OCP_UrgentOrder>();
                    HDLogHelper.Log("UrgentOrder_Statistics_SuperAdmin",
                        $"超级管理员统计催单数据 - 用户ID: {currentUserId}, 无权限限制",
                        BusinessConstants.LogCategory.OrderCollaboration);
                }
                else
                {
                    authorizedUrgentOrders = dbContext.Set<OCP_UrgentOrder>()
                        .Where(x => x.CreateID == currentUserId ||
                                   x.AssignedResPerson == currentUserName ||
                                   x.DefaultResPerson == currentUserName);
                    HDLogHelper.Log("UrgentOrder_Statistics_Permission",
                        $"催单统计权限过滤 - 用户ID: {currentUserId}, 用户名: {currentUserName}",
                        BusinessConstants.LogCategory.OrderCollaboration);
                }

                // 发送消息数（当前用户有权限查看的催单记录）
                var sentCount = await authorizedUrgentOrders.CountAsync();

                // 获取当前用户有权限查看的催单ID列表
                var authorizedUrgentOrderIds = await authorizedUrgentOrders
                    .Select(x => x.UrgentOrderID)
                    .ToListAsync();

                // 已回复消息数（有回复记录且用户有权限查看的催单）
                var repliedUrgentOrderIds = await dbContext.Set<OCP_UrgentOrderReply>()
                    .Where(r => authorizedUrgentOrderIds.Contains(r.UrgentOrderID))
                    .Select(x => x.UrgentOrderID)
                    .Distinct()
                    .ToListAsync();

                var repliedCount = repliedUrgentOrderIds.Count;

                // 待回复消息数（没有回复记录且用户有权限查看的催单）
                var pendingCount = sentCount - repliedCount;

                // 已超期消息数（需要根据指定回复时间和创建时间计算）
                var overdueCount = 0;
                var urgentOrders = await authorizedUrgentOrders
                    .Where(x => x.AssignedReplyTime.HasValue &&
                               x.TimeUnit.HasValue &&
                               x.CreateDate.HasValue)
                    .ToListAsync();

                foreach (var order in urgentOrders)
                {
                    if (order.AssignedReplyTime.HasValue && order.TimeUnit.HasValue && order.CreateDate.HasValue)
                    {
                        // 检查是否已回复
                        var hasReply = repliedUrgentOrderIds.Contains(order.UrgentOrderID);
                        if (!hasReply)
                        {
                            // 计算截止时间
                            var deadline = CalculateDeadline(order.CreateDate.Value, order.AssignedReplyTime.Value, order.TimeUnit.Value);
                            if (DateTime.Now > deadline)
                            {
                                overdueCount++;
                            }
                        }
                    }
                }

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
                HDLogHelper.Log("UrgentOrder_Statistics_Error", $"获取催单统计数据异常: {ex.Message}", "OrderCollaboration");
                throw;
            }
        }


        /// <summary>
        /// 计算回复截止时间
        /// </summary>
        /// <param name="createDate">创建时间</param>
        /// <param name="assignedReplyTime">指定回复时间</param>
        /// <param name="timeUnit">时间单位 1-小时 2-天 3-周</param>
        /// <returns>截止时间</returns>
        private DateTime CalculateDeadline(DateTime createDate, decimal assignedReplyTime, int timeUnit)
        {
            switch (timeUnit)
            {
                case 1: // 小时
                    return createDate.AddHours((double)assignedReplyTime);
                case 2: // 天
                    return createDate.AddDays((double)assignedReplyTime);
                case 3: // 周
                    return createDate.AddDays((double)assignedReplyTime * 7);
                default:
                    return createDate;
            }
        }

        /// <summary>
        /// 更新催单状态
        /// </summary>
        /// <param name="urgentOrderId">催单ID</param>
        /// <param name="status">新状态</param>
        /// <returns>操作结果</returns>
        public async Task<WebResponseContent> UpdateUrgentOrderStatusAsync(long urgentOrderId, string status)
        {
            try
            {
                var urgentOrder = await _repository.FindAsIQueryable(x => x.UrgentOrderID == urgentOrderId).FirstOrDefaultAsync();
                if (urgentOrder == null)
                {
                    return new WebResponseContent().Error("催单记录不存在");
                }

                urgentOrder.UrgentStatus = status;
                urgentOrder.ModifyDate = DateTime.Now;
                urgentOrder.ModifyID = HDPro.Core.ManageUser.UserContext.Current?.UserInfo?.User_Id ?? 1;
                urgentOrder.Modifier = HDPro.Core.ManageUser.UserContext.Current?.UserInfo?.UserTrueName ?? "系统";

                _repository.Update(urgentOrder);
                await _repository.SaveChangesAsync();

                HDLogHelper.Log("UrgentOrder_Status_Update", $"催单状态更新成功 - 催单ID: {urgentOrderId}, 新状态: {status}", "OrderCollaboration");

                return new WebResponseContent().OK($"催单状态已更新为：{status}");
            }
            catch (Exception ex)
            {
                HDLogHelper.Log("UrgentOrder_Status_Update_Error", $"催单状态更新失败 - 催单ID: {urgentOrderId}, 新状态: {status}, 错误: {ex.Message}", "OrderCollaboration");
                return new WebResponseContent().Error($"催单状态更新失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 批量更新催单状态
        /// </summary>
        /// <param name="urgentOrderIds">催单ID列表</param>
        /// <param name="status">新状态</param>
        /// <returns>操作结果</returns>
        public async Task<WebResponseContent> BatchUpdateUrgentOrderStatusAsync(List<long> urgentOrderIds, string status)
        {
            if (urgentOrderIds == null || !urgentOrderIds.Any())
            {
                return new WebResponseContent().Error("催单ID列表不能为空");
            }

            try
            {
                var urgentOrders = await _repository.FindAsIQueryable(x => urgentOrderIds.Contains(x.UrgentOrderID)).ToListAsync();
                if (!urgentOrders.Any())
                {
                    return new WebResponseContent().Error("未找到任何催单记录");
                }

                var currentTime = DateTime.Now;
                var currentUserId = HDPro.Core.ManageUser.UserContext.Current?.UserInfo?.User_Id ?? 1;
                var currentUserName = HDPro.Core.ManageUser.UserContext.Current?.UserInfo?.UserTrueName ?? "系统";

                foreach (var urgentOrder in urgentOrders)
                {
                    urgentOrder.UrgentStatus = status;
                    urgentOrder.ModifyDate = currentTime;
                    urgentOrder.ModifyID = currentUserId;
                    urgentOrder.Modifier = currentUserName;
                }

                _repository.UpdateRange(urgentOrders);
                await _repository.SaveChangesAsync();

                HDLogHelper.Log("UrgentOrder_Status_BatchUpdate", $"批量更新催单状态成功 - 催单数量: {urgentOrders.Count}, 新状态: {status}, 催单IDs: {string.Join(",", urgentOrderIds)}", "OrderCollaboration");

                return new WebResponseContent().OK($"成功更新 {urgentOrders.Count} 条催单状态为：{status}");
            }
            catch (Exception ex)
            {
                HDLogHelper.Log("UrgentOrder_Status_BatchUpdate_Error", $"批量更新催单状态失败 - 催单数量: {urgentOrderIds.Count}, 新状态: {status}, 错误: {ex.Message}", "OrderCollaboration");
                return new WebResponseContent().Error($"批量更新催单状态失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 获取催单状态选项
        /// </summary>
        /// <returns>状态选项列表</returns>
        public List<object> GetUrgentOrderStatusOptions()
        {
            return new List<object>
            {
                new { Value = BusinessConstants.UrgentOrderStatus.Pending, Text = "催单中" },
                new { Value = BusinessConstants.UrgentOrderStatus.Replied, Text = "已回复" }
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
                new { Value = "sent", Text = "已发送消息", Description = "所有催单记录", Count = 0 },
                new { Value = "pending", Text = "待回复消息", Description = "没有回复记录的催单", Count = 0 },
                new { Value = "overdue", Text = "已超期消息", Description = "没有回复且超过指定回复时间的催单", Count = 0 },
                new { Value = "replied", Text = "已回复消息", Description = "有回复记录的催单", Count = 0 }
            };
        }

        /// <summary>
        /// 更新催单SRM推送状态
        /// </summary>
        /// <param name="urgentOrderId">催单ID</param>
        /// <param name="isSendSRM">是否已推送SRM（0-否，1-是）</param>
        /// <returns>操作结果</returns>
        public async Task<WebResponseContent> UpdateUrgentOrderSRMStatusAsync(long urgentOrderId, int isSendSRM)
        {
            try
            {
                var urgentOrder = await _repository.FindAsIQueryable(x => x.UrgentOrderID == urgentOrderId).FirstOrDefaultAsync();
                if (urgentOrder == null)
                {
                    return new WebResponseContent().Error("催单记录不存在");
                }

                urgentOrder.IsSendSRM = isSendSRM;
                urgentOrder.ModifyDate = DateTime.Now;
                urgentOrder.ModifyID = HDPro.Core.ManageUser.UserContext.Current?.UserInfo?.User_Id ?? 1;
                urgentOrder.Modifier = HDPro.Core.ManageUser.UserContext.Current?.UserInfo?.UserTrueName ?? "系统";

                _repository.Update(urgentOrder);
                await _repository.SaveChangesAsync();

                var statusText = isSendSRM == 1 ? "已推送" : "未推送";
                HDLogHelper.Log("UrgentOrder_SRM_Status_Update", 
                    $"催单SRM推送状态更新成功 - 催单ID: {urgentOrderId}, SRM状态: {statusText}", 
                    BusinessConstants.LogCategory.OrderCollaboration);

                return new WebResponseContent().OK($"催单SRM推送状态已更新为：{statusText}");
            }
            catch (Exception ex)
            {
                HDLogHelper.Log("UrgentOrder_SRM_Status_Update_Error", 
                    $"催单SRM推送状态更新失败 - 催单ID: {urgentOrderId}, SRM状态: {isSendSRM}, 错误: {ex.Message}", 
                    BusinessConstants.LogCategory.OrderCollaboration);
                return new WebResponseContent().Error($"催单SRM推送状态更新失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 获取按业务类型统计的催单消息数据
        /// </summary>
        /// <param name="messageStatus">消息状态过滤参数（可选）：sent-已发送消息, pending-待回复消息, overdue-已超期消息, replied-已回复消息</param>
        /// <returns>按业务类型统计的催单消息数据</returns>
        public async Task<BusinessTypeMessageStatisticsDto> GetUrgentOrderStatisticsByBusinessTypeAsync(string messageStatus = null)
        {
            try
            {
                var dbContext = _repository.DbContext;
                
                // 权限控制：只统计当前用户有权限查看的催单
                var currentUser = HDPro.Core.ManageUser.UserContext.Current;
                var currentUserId = currentUser?.UserId ?? 0;
                var currentUserName = currentUser?.UserInfo?.UserName ?? "";
                
                // 获取当前用户有权限查看的所有催单记录
                List<OCP_UrgentOrder> allUrgentOrders;
                
                // 超级管理员可以查看所有数据
                if (currentUser?.IsSuperAdmin == true)
                {
                    allUrgentOrders = await dbContext.Set<OCP_UrgentOrder>().ToListAsync();
                    HDLogHelper.Log("UrgentOrder_BusinessTypeStatistics_SuperAdmin", 
                        $"超级管理员按业务类型统计催单 - 用户ID: {currentUserId}, 无权限限制", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                }
                else
                {
                    allUrgentOrders = await dbContext.Set<OCP_UrgentOrder>()
                        .Where(x => x.CreateID == currentUserId || 
                                   x.AssignedResPerson == currentUserName ||
                                   x.DefaultResPerson == currentUserName)
                        .ToListAsync();
                    HDLogHelper.Log("UrgentOrder_BusinessTypeStatistics_Permission", 
                        $"催单按业务类型统计权限过滤 - 用户ID: {currentUserId}, 用户名: {currentUserName}", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                }
                
                // 获取当前用户有权限查看的催单ID列表
                var authorizedUrgentOrderIds = allUrgentOrders.Select(x => x.UrgentOrderID).ToList();
                
                // 获取所有回复记录的催单ID（只包含用户有权限查看的）
                var repliedUrgentOrderIds = await dbContext.Set<OCP_UrgentOrderReply>()
                    .Where(r => authorizedUrgentOrderIds.Contains(r.UrgentOrderID))
                    .Select(x => x.UrgentOrderID)
                    .Distinct()
                    .ToListAsync();

                // 根据MessageStatus参数过滤催单数据
                List<OCP_UrgentOrder> filteredUrgentOrders = allUrgentOrders;
                
                if (!string.IsNullOrWhiteSpace(messageStatus))
                {
                    switch (messageStatus.ToLower())
                    {
                        case "sent":
                            // 已发送消息：返回所有催单记录，无需额外过滤
                            break;

                        case "pending":
                            // 待回复消息：没有回复记录的催单
                            filteredUrgentOrders = allUrgentOrders
                                .Where(u => !repliedUrgentOrderIds.Contains(u.UrgentOrderID))
                                .ToList();
                            break;

                        case "overdue":
                            // 已超期消息：没有回复且超过指定回复时间的催单
                            var now = DateTime.Now;
                            filteredUrgentOrders = allUrgentOrders
                                .Where(u => !repliedUrgentOrderIds.Contains(u.UrgentOrderID) &&
                                           u.CreateDate.HasValue &&
                                           u.AssignedReplyTime.HasValue &&
                                           u.TimeUnit.HasValue)
                                .Where(u => {
                                    var createTime = u.CreateDate.Value;
                                    var replyTime = u.AssignedReplyTime.Value;
                                    var timeUnit = u.TimeUnit.Value;
                                    
                                    DateTime deadlineTime = timeUnit switch
                                    {
                                        1 => createTime.AddHours((double)replyTime), // 小时
                                        2 => createTime.AddDays((double)replyTime),  // 天
                                        3 => createTime.AddDays((double)replyTime * 7), // 周
                                        _ => createTime.AddDays((double)replyTime) // 默认按天计算
                                    };
                                    
                                    return now > deadlineTime;
                                })
                                .ToList();
                            break;

                        case "replied":
                            // 已回复消息：有回复记录的催单
                            filteredUrgentOrders = allUrgentOrders
                                .Where(u => repliedUrgentOrderIds.Contains(u.UrgentOrderID))
                                .ToList();
                            break;

                        default:
                            // 默认返回所有记录
                            break;
                    }
                    
                    HDLogHelper.Log("UrgentOrder_BusinessTypeStatistics_MessageStatusFilter", 
                        $"催单按业务类型统计消息状态过滤 - 状态: {messageStatus}, 过滤后数量: {filteredUrgentOrders.Count}", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                }

                // 获取所有预定义的业务类型
                var allBusinessTypes = BusinessConstants.GetAllBusinessTypes();
                
                // 按业务类型分组统计现有数据
                var existingBusinessTypeGroups = filteredUrgentOrders
                    .GroupBy(x => x.BusinessType ?? "未知")
                    .ToDictionary(g => g.Key, g => g.ToList());

                var businessTypeList = new List<BusinessTypeStatisticsDto>();
                int totalSent = 0, totalPending = 0, totalOverdue = 0, totalReplied = 0;

                // 遍历所有预定义的业务类型，确保每个业务类型都有统计数据
                foreach (var businessTypeDefine in allBusinessTypes)
                {
                    var businessTypeCode = businessTypeDefine.Code;
                    var businessTypeName = businessTypeDefine.Name;
                    
                    // 获取该业务类型的催单记录，如果没有则为空列表
                    var urgentOrders = existingBusinessTypeGroups.ContainsKey(businessTypeCode) 
                        ? existingBusinessTypeGroups[businessTypeCode] 
                        : new List<OCP_UrgentOrder>();
                    
                    var sentCount = urgentOrders.Count;
                    var repliedCount = urgentOrders.Count(u => repliedUrgentOrderIds.Contains(u.UrgentOrderID));
                    var pendingCount = sentCount - repliedCount;
                    
                    // 计算超期数量（根据指定回复时间计算）
                    var overdueCount = 0;
                    foreach (var order in urgentOrders)
                    {
                        if (order.AssignedReplyTime.HasValue && order.TimeUnit.HasValue && order.CreateDate.HasValue)
                        {
                            // 检查是否已回复
                            var hasReply = repliedUrgentOrderIds.Contains(order.UrgentOrderID);
                            if (!hasReply)
                            {
                                // 计算截止时间
                                var deadline = CalculateDeadline(order.CreateDate.Value, order.AssignedReplyTime.Value, order.TimeUnit.Value);
                                if (DateTime.Now > deadline)
                                {
                                    overdueCount++;
                                }
                            }
                        }
                    }

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
                    var urgentOrders = kvp.Value;
                    
                    // 如果不在预定义业务类型中，则添加到统计中
                    if (!allBusinessTypes.Any(bt => bt.Code == businessTypeCode))
                    {
                        var sentCount = urgentOrders.Count;
                        var repliedCount = urgentOrders.Count(u => repliedUrgentOrderIds.Contains(u.UrgentOrderID));
                        var pendingCount = sentCount - repliedCount;
                        
                        // 计算超期数量（根据指定回复时间计算）
                        var overdueCount = 0;
                        foreach (var order in urgentOrders)
                        {
                            if (order.AssignedReplyTime.HasValue && order.TimeUnit.HasValue && order.CreateDate.HasValue)
                            {
                                // 检查是否已回复
                                var hasReply = repliedUrgentOrderIds.Contains(order.UrgentOrderID);
                                if (!hasReply)
                                {
                                    // 计算截止时间
                                    var deadline = CalculateDeadline(order.CreateDate.Value, order.AssignedReplyTime.Value, order.TimeUnit.Value);
                                    if (DateTime.Now > deadline)
                                    {
                                        overdueCount++;
                                    }
                                }
                            }
                        }

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
                    StatisticsTime = DateTime.Now
                };
            }
            catch (System.Exception ex)
            {
                HDLogHelper.Log("UrgentOrder_BusinessTypeStatistics_Error", $"获取催单按业务类型统计数据异常: {ex.Message}", BusinessConstants.LogCategory.OrderCollaboration);
                throw;
            }
        }

        /// <summary>
        /// 根据业务类型和业务主键查询催单和协商信息
        /// </summary>
        /// <param name="businessType">业务类型</param>
        /// <param name="businessKey">业务主键</param>
        /// <returns>业务关联的催单和协商信息</returns>
        public async Task<WebResponseContent> GetBusinessOrderCollaborationAsync(string businessType, string businessKey)
        {
            try
            {
                HDLogHelper.Log("GetBusinessOrderCollaboration_Start", 
                    $"开始查询业务协同信息 - 业务类型: {businessType}, 业务主键: {businessKey}", 
                    BusinessConstants.LogCategory.OrderCollaboration);

                // 参数验证
                if (string.IsNullOrWhiteSpace(businessType))
                {
                    return new WebResponseContent().Error("业务类型不能为空");
                }

                if (string.IsNullOrWhiteSpace(businessKey))
                {
                    return new WebResponseContent().Error("业务主键不能为空");
                }

                // 获取当前用户信息
                var currentUser = HDPro.Core.ManageUser.UserContext.Current;
                var currentUserName = currentUser?.UserInfo?.UserName ?? "";
                var currentUserId = currentUser?.UserId ?? 0;

                HDLogHelper.Log("GetBusinessOrderCollaboration_User", 
                    $"当前用户: {currentUserName} (ID: {currentUserId})", 
                    BusinessConstants.LogCategory.OrderCollaboration);

                var dbContext = _repository.DbContext;

                // 查询催单信息 - 超级管理员可以查看所有数据，普通用户只能查看自己是发起人或指定负责人的催单
                List<OCP_UrgentOrder> urgentOrders;
                if (currentUser?.IsSuperAdmin == true)
                {
                    urgentOrders = await dbContext.Set<OCP_UrgentOrder>()
                        .Where(u => u.BusinessType == businessType && u.BusinessKey == businessKey)
                        .OrderByDescending(u => u.CreateDate)
                        .ToListAsync();
                    
                    HDLogHelper.Log("UrgentOrder_BusinessQuery_SuperAdmin", 
                        $"超级管理员查询业务催单 - 业务类型: {businessType}, 业务主键: {businessKey}, 无权限限制", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                }
                else
                {
                    urgentOrders = await dbContext.Set<OCP_UrgentOrder>()
                        .Where(u => u.BusinessType == businessType && u.BusinessKey == businessKey &&
                                   (u.CreateID == currentUserId || u.AssignedResPerson == currentUserName))
                        .OrderByDescending(u => u.CreateDate)
                        .ToListAsync();
                    
                    HDLogHelper.Log("UrgentOrder_BusinessQuery_Permission", 
                        $"普通用户查询业务催单 - 业务类型: {businessType}, 业务主键: {businessKey}, 用户ID: {currentUserId}, 用户名: {currentUserName}", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                }

                // 查询协商信息 - 超级管理员可以查看所有数据，普通用户只能查看自己是发起人或指定负责人的协商
                List<OCP_Negotiation> negotiations;
                if (currentUser?.IsSuperAdmin == true)
                {
                    negotiations = await dbContext.Set<OCP_Negotiation>()
                        .Where(n => n.BusinessType == businessType && n.BusinessKey == businessKey)
                        .OrderByDescending(n => n.CreateDate)
                        .ToListAsync();
                    
                    HDLogHelper.Log("Negotiation_BusinessQuery_SuperAdmin", 
                        $"超级管理员查询业务协商 - 业务类型: {businessType}, 业务主键: {businessKey}, 无权限限制", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                }
                else
                {
                    negotiations = await dbContext.Set<OCP_Negotiation>()
                        .Where(n => n.BusinessType == businessType && n.BusinessKey == businessKey &&
                                   (n.CreateID == currentUserId || n.AssignedResPerson == currentUserName))
                        .OrderByDescending(n => n.CreateDate)
                        .ToListAsync();
                    
                    HDLogHelper.Log("Negotiation_BusinessQuery_Permission", 
                        $"普通用户查询业务协商 - 业务类型: {businessType}, 业务主键: {businessKey}, 用户ID: {currentUserId}, 用户名: {currentUserName}", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                }

                // 获取催单ID列表
                var urgentOrderIds = urgentOrders.Select(u => u.UrgentOrderID).ToList();

                // 获取协商ID列表
                var negotiationIds = negotiations.Select(n => n.NegotiationID).ToList();

                // 查询催单回复
                var urgentOrderReplies = urgentOrderIds.Any() 
                    ? await dbContext.Set<OCP_UrgentOrderReply>()
                        .Where(r => urgentOrderIds.Contains(r.UrgentOrderID))
                        .OrderByDescending(r => r.ReplyTime)
                        .ToListAsync()
                    : new List<OCP_UrgentOrderReply>();

                // 查询协商回复
                var negotiationReplies = negotiationIds.Any()
                    ? await dbContext.Set<OCP_NegotiationReply>()
                        .Where(r => negotiationIds.Contains(r.NegotiationID))
                        .OrderByDescending(r => r.ReplyTime)
                        .ToListAsync()
                    : new List<OCP_NegotiationReply>();

                // 构建催单DTO列表
                var urgentOrderDtos = urgentOrders.Select(u => {
                    var replies = urgentOrderReplies
                        .Where(r => r.UrgentOrderID == u.UrgentOrderID)
                        .Select(r => new UrgentOrderReplyDto
                        {
                            ReplyID = r.ReplyID,
                            UrgentOrderID = r.UrgentOrderID,
                            ReplyContent = r.ReplyContent,
                            ReplyPersonName = r.ReplyPersonName,
                            ReplyPersonPhone = r.ReplyPersonPhone,
                            ReplyTime = r.ReplyTime,
                            ReplyProgress = r.ReplyProgress,
                            ReplyDeliveryDate = r.ReplyDeliveryDate,
                            Remarks = r.Remarks,
                            CreateDate = r.CreateDate,
                            Creator = r.Creator
                        }).ToList();

                    // 判断是否可回复：当前用户是指定负责人且没有回复记录
                    bool canReply = u.AssignedResPerson == currentUserName && !replies.Any();

                    return new UrgentOrderDto
                    {
                        UrgentOrderID = u.UrgentOrderID,
                        BusinessType = u.BusinessType,
                        BusinessKey = u.BusinessKey,
                        BillNo = u.BillNo,
                        Seq = u.Seq,
                        UrgentType = u.UrgentType,
                        UrgentContent = u.UrgentContent,
                        UrgencyLevel = u.UrgencyLevel,
                        AssignedReplyTime = u.AssignedReplyTime,
                        TimeUnit = u.TimeUnit,
                        UrgentStatus = u.UrgentStatus,
                        DefaultResPerson = u.DefaultResPerson,
                        DefaultResPersonName = u.DefaultResPersonName,
                        AssignedResPerson = u.AssignedResPerson,
                        AssignedResPersonName = u.AssignedResPersonName,
                        PlanTraceNo = u.PlanTraceNo,
                        CreateDate = u.CreateDate,
                        Creator = u.Creator,
                        Remarks = u.Remarks,
                        CanReply = canReply,
                        Replies = replies
                    };
                }).ToList();

                // 构建协商DTO列表
                var negotiationDtos = negotiations.Select(n => {
                    var replies = negotiationReplies
                        .Where(r => r.NegotiationID == n.NegotiationID)
                        .Select(r => new NegotiationReplyDto
                        {
                            ReplyID = r.ReplyID,
                            NegotiationID = r.NegotiationID,
                            ReplyContent = r.ReplyContent,
                            ReplyPersonName = r.ReplyPersonName,
                            ReplyPersonPhone = r.ReplyPersonPhone,
                            ReplyTime = r.ReplyTime,
                            ReplyProgress = r.ReplyProgress,
                            ReplyDeliveryDate = r.ReplyDeliveryDate,
                            Remarks = r.Remarks,
                            CreateDate = r.CreateDate,
                            Creator = r.Creator
                        }).ToList();

                    // 判断是否可回复：当前用户是指定负责人且没有回复记录
                    bool canReply = n.AssignedResPerson == currentUserName && !replies.Any();

                    return new NegotiationDto
                    {
                        NegotiationID = n.NegotiationID,
                        BusinessType = n.BusinessType,
                        BusinessKey = n.BusinessKey,
                        BillNo = n.BillNo,
                        Seq = n.Seq,
                        NegotiationType = n.NegotiationType,
                        NegotiationReason = n.NegotiationReason,
                        NegotiationContent = n.NegotiationContent,
                        NegotiationDate = n.NegotiationDate,
                        DeliveryDate = n.DeliveryDate,
                        NegotiationStatus = n.NegotiationStatus,
                        DefaultResPerson = n.DefaultResPerson,
                        DefaultResPersonName = n.DefaultResPersonName,
                        AssignedResPerson = n.AssignedResPerson,
                        AssignedResPersonName = n.AssignedResPersonName,
                        PlanTraceNo = n.PlanTraceNo,
                        CreateDate = n.CreateDate,
                        Creator = n.Creator,
                        Remarks = n.Remarks,
                        CanReply = canReply,
                        Replies = replies
                    };
                }).ToList();

                // 计算汇总信息
                var repliedUrgentOrderIds = urgentOrderReplies.Select(r => r.UrgentOrderID).Distinct().ToList();
                var repliedNegotiationIds = negotiationReplies.Select(r => r.NegotiationID).Distinct().ToList();

                var summary = new BusinessCollaborationSummaryDto
                {
                    TotalUrgentOrders = urgentOrders.Count,
                    RepliedUrgentOrders = repliedUrgentOrderIds.Count,
                    PendingUrgentOrders = urgentOrders.Count - repliedUrgentOrderIds.Count,
                    TotalNegotiations = negotiations.Count,
                    RepliedNegotiations = repliedNegotiationIds.Count,
                    PendingNegotiations = negotiations.Count - repliedNegotiationIds.Count,
                    LastActivityTime = GetLastActivityTime(urgentOrders, negotiations, urgentOrderReplies, negotiationReplies)
                };

                // 构建响应数据
                var result = new BusinessOrderCollaborationDto
                {
                    BusinessType = businessType,
                    BusinessKey = businessKey,
                    UrgentOrders = urgentOrderDtos,
                    Negotiations = negotiationDtos,
                    Summary = summary
                };

                HDLogHelper.Log("GetBusinessOrderCollaboration_Success", 
                    $"查询业务协同信息成功 - 业务类型: {businessType}, 业务主键: {businessKey}, " +
                    $"催单数量: {urgentOrders.Count}, 协商数量: {negotiations.Count}", 
                    BusinessConstants.LogCategory.OrderCollaboration);

                return new WebResponseContent().OK("查询业务协同信息成功", result);
            }
            catch (System.Exception ex)
            {
                HDLogHelper.Log("GetBusinessOrderCollaboration_Error", 
                    $"查询业务协同信息异常 - 业务类型: {businessType}, 业务主键: {businessKey}, 错误: {ex.Message}", 
                    BusinessConstants.LogCategory.OrderCollaboration);
                return new WebResponseContent().Error($"查询业务协同信息异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取最后活动时间
        /// </summary>
        /// <param name="urgentOrders">催单列表</param>
        /// <param name="negotiations">协商列表</param>
        /// <param name="urgentOrderReplies">催单回复列表</param>
        /// <param name="negotiationReplies">协商回复列表</param>
        /// <returns>最后活动时间</returns>
        private DateTime? GetLastActivityTime(
            List<OCP_UrgentOrder> urgentOrders,
            List<OCP_Negotiation> negotiations,
            List<OCP_UrgentOrderReply> urgentOrderReplies,
            List<OCP_NegotiationReply> negotiationReplies)
        {
            var times = new List<DateTime?>();

            // 添加催单创建时间
            times.AddRange(urgentOrders.Select(u => u.CreateDate));

            // 添加协商创建时间
            times.AddRange(negotiations.Select(n => n.CreateDate));

            // 添加催单回复时间
            times.AddRange(urgentOrderReplies.Select(r => r.ReplyTime));

            // 添加协商回复时间
            times.AddRange(negotiationReplies.Select(r => r.ReplyTime));

            // 过滤空值并返回最大时间
            var validTimes = times.Where(t => t.HasValue).Select(t => t.Value);
            return validTimes.Any() ? validTimes.Max() : (DateTime?)null;
        }

        /// <summary>
        /// 根据业务类型和业务主键读取物料相关信息
        /// </summary>
        /// <param name="urgentOrder">催单实体</param>
        private void PopulateMaterialInfoFromTrackingTable(OCP_UrgentOrder urgentOrder)
        {
            if (string.IsNullOrWhiteSpace(urgentOrder.BusinessType) || string.IsNullOrWhiteSpace(urgentOrder.BusinessKey))
            {
                HDLogHelper.Log("UrgentOrder_MaterialInfo_Populate_Error", 
                    $"催单ID: {urgentOrder.UrgentOrderID}, 业务类型或业务主键为空，无法读取物料信息", 
                    BusinessConstants.LogCategory.OrderCollaboration);
                return;
            }

            try
            {
                var dbContext = _repository.DbContext;
                
                // 解析业务主键为FENTRYID
                if (!int.TryParse(urgentOrder.BusinessKey, out int entryId))
                {
                    HDLogHelper.Log("UrgentOrder_MaterialInfo_Populate_Error", 
                        $"催单ID: {urgentOrder.UrgentOrderID}, 业务主键格式错误: {urgentOrder.BusinessKey}", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                    return;
                }

                string materialNumber = null;
                string materialName = null;
                string specification = null;

                // 根据业务类型查询对应的跟踪表
                switch (urgentOrder.BusinessType)
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
                        HDLogHelper.Log("UrgentOrder_MaterialInfo_BusinessTypeNotSupported", 
                            $"催单ID: {urgentOrder.UrgentOrderID}, 业务类型: {urgentOrder.BusinessType} 暂不支持自动读取物料信息", 
                            BusinessConstants.LogCategory.OrderCollaboration);
                        break;

                    default:
                        HDLogHelper.Log("UrgentOrder_MaterialInfo_UnknownBusinessType", 
                            $"催单ID: {urgentOrder.UrgentOrderID}, 未知的业务类型: {urgentOrder.BusinessType}", 
                            BusinessConstants.LogCategory.OrderCollaboration);
                        break;
                }

                // 设置物料信息
                if (!string.IsNullOrWhiteSpace(materialNumber))
                {
                    urgentOrder.MaterialNumber = materialNumber;
                    urgentOrder.MaterialName = materialName;
                    urgentOrder.Specification = specification;
                    
                    HDLogHelper.Log("UrgentOrder_MaterialInfo_Populated", 
                        $"催单ID: {urgentOrder.UrgentOrderID}, 从{urgentOrder.BusinessType}跟踪表读取物料信息 - 物料编号: {materialNumber}, 物料名称: {materialName}, 规格: {specification}", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                }
                else
                {
                    HDLogHelper.Log("UrgentOrder_MaterialInfo_NotFound", 
                        $"催单ID: {urgentOrder.UrgentOrderID}, {urgentOrder.BusinessType}跟踪表中未找到业务主键: {urgentOrder.BusinessKey} 的记录", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                }
            }
            catch (Exception ex)
            {
                HDLogHelper.Log("UrgentOrder_MaterialInfo_Populate_Exception", 
                    $"从跟踪表读取物料信息异常 - 催单ID: {urgentOrder.UrgentOrderID}, 业务类型: {urgentOrder.BusinessType}, 业务主键: {urgentOrder.BusinessKey}, 错误: {ex.Message}", 
                    BusinessConstants.LogCategory.OrderCollaboration);
            }
        }

        /// <summary>
        /// 批量获取默认负责人
        /// </summary>
        /// <param name="request">批量获取请求</param>
        /// <returns>默认负责人信息列表</returns>
        public async Task<WebResponseContent> BatchGetDefaultResponsibleAsync(BatchGetDefaultResponsibleRequest request)
        {
            if (request == null || request.BusinessItems == null || !request.BusinessItems.Any())
            {
                return webResponse.Error("请求参数不能为空");
            }

            try
            {
                var response = new BatchGetDefaultResponsibleResponse();
                var dbContext = _repository.DbContext;

                // 按业务类型分组处理
                var groupedItems = request.BusinessItems.GroupBy(x => x.BusinessType).ToList();

                foreach (var group in groupedItems)
                {
                    var businessType = group.Key;
                    var items = group.ToList();

                    switch (businessType?.ToUpper())
                    {
                        case "PO": // 采购
                        case "WW": // 委外
                            await ProcessPurchaseAndOutsourcingItemsAsync(items, response, dbContext);
                            break;

                        case "JS": // 技术
                            await ProcessTechnologyItemsAsync(items, response);
                            break;

                        default: // 其他业务类型
                            await ProcessOtherBusinessTypeItemsAsync(items, businessType, response, dbContext);
                            break;
                    }
                }

                HDLogHelper.Log("BatchGetDefaultResponsible_Success", 
                    $"批量获取默认负责人成功 - 请求数量: {request.BusinessItems.Count}, 成功数量: {response.ResponsibleInfos.Count(x => x.Found)}", 
                    BusinessConstants.LogCategory.OrderCollaboration);

                return webResponse.OK("批量获取默认负责人成功", response);
            }
            catch (Exception ex)
            {
                HDLogHelper.Log("BatchGetDefaultResponsible_Error", 
                    $"批量获取默认负责人失败: {ex.Message}", 
                    BusinessConstants.LogCategory.OrderCollaboration);
                return webResponse.Error($"批量获取默认负责人失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理采购和委外业务类型
        /// </summary>
        /// <param name="items">业务项目列表</param>
        /// <param name="response">响应对象</param>
        /// <param name="dbContext">数据库上下文</param>
        /// <returns></returns>
        private async Task ProcessPurchaseAndOutsourcingItemsAsync(List<BusinessItem> items, BatchGetDefaultResponsibleResponse response, Microsoft.EntityFrameworkCore.DbContext dbContext)
        {
            try
            {
                // 构建批量请求
                var supplierRequests = new List<GetSupplierResponsibleRequest>();
                foreach (var item in items)
                {
                    if (int.TryParse(item.BusinessKey, out int businessId))
                    {
                        supplierRequests.Add(new GetSupplierResponsibleRequest
                        {
                            BusinessType = item.BusinessType,
                            BusinessId = businessId
                        });
                    }
                }

                // 批量调用供应商映射服务
                var batchResult = await _purchaseSupplierMappingService.BatchGetSupplierResponsibleAsync(supplierRequests);
                
                if (batchResult.Status && batchResult.Data is List<SupplierResponsibleResponse> supplierResponses)
                {
                    // 创建查找字典
                    var supplierDict = supplierResponses.ToDictionary(
                        x => $"{x.BusinessType}_{x.BusinessId}", 
                        x => x);

                    // 处理每个业务项目
                    foreach (var item in items)
                    {
                        var responsibleInfo = new DefaultResponsibleInfo
                        {
                            BusinessType = item.BusinessType,
                            BusinessKey = item.BusinessKey,
                            DataSource = "供应商映射"
                        };

                        if (!int.TryParse(item.BusinessKey, out int businessId))
                        {
                            responsibleInfo.Found = false;
                            responsibleInfo.ErrorMessage = "业务主键格式错误";
                        }
                        else
                        {
                            var key = $"{item.BusinessType}_{businessId}";
                            if (supplierDict.TryGetValue(key, out var supplierResponse))
                            {
                                responsibleInfo.Found = !string.IsNullOrWhiteSpace(supplierResponse.ResponsiblePersonLoginName);
                                responsibleInfo.DefaultResponsibleLoginName = supplierResponse.ResponsiblePersonLoginName;
                                responsibleInfo.DefaultResponsibleName = supplierResponse.ResponsiblePersonName;
                                if (!responsibleInfo.Found)
                                {
                                    responsibleInfo.ErrorMessage = supplierResponse.Remarks ?? "未找到供应商负责人";
                                }
                            }
                            else
                            {
                                responsibleInfo.Found = false;
                                responsibleInfo.ErrorMessage = "未找到对应的供应商信息";
                            }
                        }

                        response.ResponsibleInfos.Add(responsibleInfo);
                    }
                }
                else
                {
                    // 批量调用失败，逐个处理
                    foreach (var item in items)
                    {
                        var responsibleInfo = new DefaultResponsibleInfo
                        {
                            BusinessType = item.BusinessType,
                            BusinessKey = item.BusinessKey,
                            DataSource = "供应商映射",
                            Found = false,
                            ErrorMessage = batchResult.Message ?? "批量获取供应商负责人失败"
                        };
                        response.ResponsibleInfos.Add(responsibleInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                HDLogHelper.Log("BatchGetDefaultResponsible_SupplierError", 
                    $"批量获取供应商负责人异常: {ex.Message}", 
                    BusinessConstants.LogCategory.OrderCollaboration);

                // 异常情况下为所有项目添加错误信息
                foreach (var item in items)
                {
                    response.ResponsibleInfos.Add(new DefaultResponsibleInfo
                    {
                        BusinessType = item.BusinessType,
                        BusinessKey = item.BusinessKey,
                        DataSource = "供应商映射",
                        Found = false,
                        ErrorMessage = $"获取供应商负责人异常: {ex.Message}"
                    });
                }
            }
        }

        /// <summary>
        /// 处理技术业务类型
        /// </summary>
        /// <param name="items">业务项目列表</param>
        /// <param name="response">响应对象</param>
        /// <returns></returns>
        private async Task ProcessTechnologyItemsAsync(List<BusinessItem> items, BatchGetDefaultResponsibleResponse response)
        {
            try
            {
                var dbContext = _repository.DbContext;
                
                // 收集需要查询的业务主键
                var techIds = new List<long>();
                var itemsWithMaterialCode = new List<BusinessItem>();
                var itemsNeedMaterialCode = new List<BusinessItem>();

                foreach (var item in items)
                {
                    if (!string.IsNullOrWhiteSpace(item.MaterialCode))
                    {
                        // 已有物料编码的项目
                        itemsWithMaterialCode.Add(item);
                    }
                    else if (long.TryParse(item.BusinessKey, out long techId))
                    {
                        // 需要从技术管理表获取物料编码的项目
                        techIds.Add(techId);
                        itemsNeedMaterialCode.Add(item);
                    }
                    else
                    {
                        // 业务主键格式错误
                        response.ResponsibleInfos.Add(new DefaultResponsibleInfo
                        {
                            BusinessType = item.BusinessType,
                            BusinessKey = item.BusinessKey,
                            DataSource = "技术管理",
                            Found = false,
                            ErrorMessage = "业务主键格式错误，无法解析为技术ID"
                        });
                    }
                }

                // 从技术管理表批量获取物料编码
                var techMaterialDict = new Dictionary<long, string>();
                if (techIds.Any())
                {
                    var techManagements = await dbContext.Set<OCP_TechManagement>()
                        .Where(x => techIds.Contains(x.TechID))
                        .Select(x => new { x.TechID, x.MaterialNumber })
                        .ToListAsync();

                    foreach (var tech in techManagements)
                    {
                        if (!string.IsNullOrWhiteSpace(tech.MaterialNumber))
                        {
                            techMaterialDict[tech.TechID] = tech.MaterialNumber;
                        }
                    }
                }

                // 为需要物料编码的项目填充物料编码
                foreach (var item in itemsNeedMaterialCode)
                {
                    if (long.TryParse(item.BusinessKey, out long techId) && 
                        techMaterialDict.TryGetValue(techId, out string materialCode))
                    {
                        item.MaterialCode = materialCode;
                        itemsWithMaterialCode.Add(item);
                    }
                    else
                    {
                        // 未找到对应的物料编码
                        response.ResponsibleInfos.Add(new DefaultResponsibleInfo
                        {
                            BusinessType = item.BusinessType,
                            BusinessKey = item.BusinessKey,
                            DataSource = "技术管理",
                            Found = false,
                            ErrorMessage = $"未找到技术ID {item.BusinessKey} 对应的物料编码"
                        });
                    }
                }

                // 收集所有有效的物料编码
                var materialCodes = itemsWithMaterialCode
                    .Where(x => !string.IsNullOrWhiteSpace(x.MaterialCode))
                    .Select(x => x.MaterialCode)
                    .Distinct()
                    .ToList();

                if (materialCodes.Any())
                {
                    // 批量调用技术管理服务
                    var batchResult = await _techManagementService.BatchGetTechManagerByMaterialCodeAsync(materialCodes);
                    
                    if (batchResult.Status && batchResult.Data != null)
                    {
                        // 解析批量结果
                        var batchData = batchResult.Data as dynamic;
                        var results = batchData?.Results as IEnumerable<dynamic>;
                        
                        // 创建查找字典
                        var techDict = new Dictionary<string, dynamic>();
                        if (results != null)
                        {
                            foreach (var result in results)
                            {
                                var materialCode = result.MaterialCode?.ToString();
                                if (!string.IsNullOrWhiteSpace(materialCode))
                                {
                                    techDict[materialCode] = result;
                                }
                            }
                        }

                        // 处理每个有物料编码的业务项目
                        foreach (var item in itemsWithMaterialCode)
                        {
                            var responsibleInfo = new DefaultResponsibleInfo
                            {
                                BusinessType = item.BusinessType,
                                BusinessKey = item.BusinessKey,
                                DataSource = "技术管理"
                            };

                            if (techDict.TryGetValue(item.MaterialCode, out var techResult))
                            {
                                var found = techResult.Found;
                                if (found == true)
                                {
                                    responsibleInfo.Found = true;
                                    responsibleInfo.DefaultResponsibleLoginName = techResult.ResponsibleLoginName?.ToString();
                                    responsibleInfo.DefaultResponsibleName = techResult.ResponsibleName?.ToString();
                                }
                                else
                                {
                                    responsibleInfo.Found = false;
                                    responsibleInfo.ErrorMessage = techResult.ErrorMessage?.ToString() ?? "未找到技术负责人";
                                }
                            }
                            else
                            {
                                responsibleInfo.Found = false;
                                responsibleInfo.ErrorMessage = "未找到对应的技术负责人信息";
                            }

                            response.ResponsibleInfos.Add(responsibleInfo);
                        }
                    }
                    else
                    {
                        // 批量调用失败，为所有项目添加错误信息
                        foreach (var item in itemsWithMaterialCode)
                        {
                            response.ResponsibleInfos.Add(new DefaultResponsibleInfo
                            {
                                BusinessType = item.BusinessType,
                                BusinessKey = item.BusinessKey,
                                DataSource = "技术管理",
                                Found = false,
                                ErrorMessage = batchResult.Message ?? "批量获取技术负责人失败"
                            });
                        }
                    }
                }

                HDLogHelper.Log("BatchGetDefaultResponsible_TechProcess", 
                    $"技术业务类型处理完成 - 总数: {items.Count}, 从技术表获取物料编码: {techMaterialDict.Count}, 最终处理: {itemsWithMaterialCode.Count}", 
                    BusinessConstants.LogCategory.OrderCollaboration);
            }
            catch (Exception ex)
            {
                HDLogHelper.Log("BatchGetDefaultResponsible_TechError", 
                    $"批量获取技术负责人异常: {ex.Message}", 
                    BusinessConstants.LogCategory.OrderCollaboration);

                // 异常情况下为所有项目添加错误信息
                foreach (var item in items)
                {
                    response.ResponsibleInfos.Add(new DefaultResponsibleInfo
                    {
                        BusinessType = item.BusinessType,
                        BusinessKey = item.BusinessKey,
                        DataSource = "技术管理",
                        Found = false,
                        ErrorMessage = $"获取技术负责人异常: {ex.Message}"
                    });
                }
            }
        }

        /// <summary>
        /// 处理其他业务类型
        /// </summary>
        /// <param name="items">业务项目列表</param>
        /// <param name="businessType">业务类型</param>
        /// <param name="response">响应对象</param>
        /// <param name="dbContext">数据库上下文</param>
        /// <returns></returns>
        private async Task ProcessOtherBusinessTypeItemsAsync(List<BusinessItem> items, string businessType, BatchGetDefaultResponsibleResponse response, Microsoft.EntityFrameworkCore.DbContext dbContext)
        {
            // 从业务类型配置表获取默认负责人
            var businessTypeConfig = await dbContext.Set<OCP_BusinessTypeResponsible>()
                .Where(x => x.BusinessType == businessType)
                .FirstOrDefaultAsync();

            foreach (var item in items)
            {
                var responsibleInfo = new DefaultResponsibleInfo
                {
                    BusinessType = item.BusinessType,
                    BusinessKey = item.BusinessKey,
                    DataSource = "业务类型配置"
                };

                if (businessTypeConfig != null)
                {
                    responsibleInfo.Found = true;
                    responsibleInfo.DefaultResponsibleLoginName = businessTypeConfig.DefaultResponsibleLoginName;
                    responsibleInfo.DefaultResponsibleName = businessTypeConfig.DefaultResponsibleName;
                }
                else
                {
                    responsibleInfo.Found = false;
                    responsibleInfo.ErrorMessage = $"未找到业务类型 '{businessType}' 的默认负责人配置";
                }

                response.ResponsibleInfos.Add(responsibleInfo);
            }
        }
        #region SignalR消息发送方法

        /// <summary>
        /// 根据登录名获取用户ID列表
        /// </summary>
        /// <param name="loginNames">登录名列表</param>
        /// <returns>用户ID列表</returns>
        private async Task<List<int>> GetUserIdsByLoginNamesAsync(List<string> loginNames)
        {
            if (loginNames == null || !loginNames.Any())
            {
                return new List<int>();
            }

            try
            {
                using var context = new HDPro.Core.EFDbContext.SysDbContext();
                var userIds = await context.Set<Sys_User>()
                    .Where(u => loginNames.Contains(u.UserName) && u.Enable == 1)
                    .Select(u => u.User_Id)
                    .ToListAsync();

                return userIds;
            }
            catch (System.Exception ex)
            {
                HDLogHelper.Log("UrgentOrder_GetUserIds_Error", 
                    $"根据登录名获取用户ID失败 - 登录名: {string.Join(",", loginNames)}, 错误: {ex.Message}", 
                    BusinessConstants.LogCategory.OrderCollaboration);
                return new List<int>();
            }
        }

        #endregion

        /// <summary>
        /// 获取当前用户催单情况统计
        /// </summary>
        /// <returns>当前用户催单情况统计数据</returns>
        public async Task<List<object>> GetUserUrgentOrderStatisticsAsync()
        {
            try
            {
                var currentUser = HDPro.Core.ManageUser.UserContext.Current;
                if (currentUser?.UserInfo == null)
                {
                    return GetDefaultStatisticsResult();
                }

                var currentUserId = currentUser.UserId;
                var currentUserName = currentUser.UserInfo.UserName ?? "";
                var currentTime = DateTime.Now;

                var dbContext = _repository.DbContext;

                // 获取所有有回复记录的催单ID
                var repliedUrgentOrderIds = await dbContext.Set<OCP_UrgentOrderReply>()
                    .Select(r => r.UrgentOrderID)
                    .Distinct()
                    .ToListAsync();

                // 根据用户权限获取催单数据
                List<OCP_UrgentOrder> sentUrgentOrders;
                List<OCP_UrgentOrder> receivedUrgentOrders;

                if (currentUser.IsSuperAdmin)
                {
                    // 超级管理员可以查看所有催单统计
                    sentUrgentOrders = await dbContext.Set<OCP_UrgentOrder>()
                        .ToListAsync();
                    
                    // 管理员的"被催单"统计为空，因为管理员查看的是全局统计
                    receivedUrgentOrders = new List<OCP_UrgentOrder>();
                    
                    HDLogHelper.Log("UrgentOrder_AdminStatistics",
                        $"超级管理员查看全局催单统计 - 用户ID: {currentUserId}, 角色: [{string.Join(",", currentUser.RoleIds)}]",
                        BusinessConstants.LogCategory.OrderCollaboration);
                }
                else
                {
                    // 普通用户只能查看与自己相关的催单
                    sentUrgentOrders = await dbContext.Set<OCP_UrgentOrder>()
                        .Where(x => x.CreateID == currentUserId)
                        .ToListAsync();

                    receivedUrgentOrders = await dbContext.Set<OCP_UrgentOrder>()
                        .Where(x => x.AssignedResPerson == currentUserName || x.DefaultResPerson == currentUserName)
                        .Where(x => x.CreateID != currentUserId) // 排除自己发出的催单
                        .ToListAsync();
                }

                // 计算发出催单的统计
                var sentTotal = sentUrgentOrders.Count;
                var sentPending = sentUrgentOrders.Count(u => !repliedUrgentOrderIds.Contains(u.UrgentOrderID));
                var sentReplied = sentUrgentOrders.Count(u => repliedUrgentOrderIds.Contains(u.UrgentOrderID));
                
                // 计算发出催单中的超期数量（参照MessageHelper的时间单位处理）
                var sentOverdue = sentUrgentOrders.Count(u => 
                {
                    if (repliedUrgentOrderIds.Contains(u.UrgentOrderID))
                        return false;

                    return IsUrgentOrderOverdue(u, currentTime);
                });

                // 计算被催单的统计
                var receivedPending = receivedUrgentOrders.Count(u => !repliedUrgentOrderIds.Contains(u.UrgentOrderID));
                
                // 计算被催单中的超时数量（参照MessageHelper的时间单位处理）
                var receivedOverdue = receivedUrgentOrders.Count(u => 
                {
                    if (repliedUrgentOrderIds.Contains(u.UrgentOrderID))
                        return false;

                    return IsUrgentOrderOverdue(u, currentTime);
                });

                // 根据用户权限构建返回数据
                List<object> result;
                
                // 管理员和普通用户都返回相同格式的统计数据
                result = new List<object>
                {
                    new { name = "催单总数", value = sentTotal },
                    new { name = "催单待回复", value = sentPending },
                    new { name = "催单已超期", value = sentOverdue },
                    new { name = "催单已回复", value = sentReplied },
                    new { name = "被催单待回复", value = receivedPending },
                    new { name = "被催单已超时", value = receivedOverdue }
                };

                // 记录统计日志（参照MessageHelper的日志记录方式）
                var statisticsType = currentUser.IsSuperAdmin ? "全局催单统计" : "用户催单统计";
                MessageHelper.LogIntegrationSuccess(statisticsType, "UrgentOrder", currentUserId, 
                    currentUserName, "统计查询", null);

                if (currentUser.IsSuperAdmin)
                {
                    HDLogHelper.Log("UrgentOrder_AdminStatistics_Detail", 
                        $"管理员全局催单统计详情 - 用户ID: {currentUserId}, 用户名: {currentUserName}, " +
                        $"全部催单总数: {sentTotal}, 全部待回复: {sentPending}, 全部已超期: {sentOverdue}, 全部已回复: {sentReplied}", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                }
                else
                {
                    HDLogHelper.Log("UrgentOrder_UserStatistics_Detail", 
                        $"用户催单统计详情 - 用户ID: {currentUserId}, 用户名: {currentUserName}, " +
                        $"催单总数: {sentTotal}, 催单待回复: {sentPending}, 催单已超期: {sentOverdue}, 催单已回复: {sentReplied}, " +
                        $"被催单待回复: {receivedPending}, 被催单已超时: {receivedOverdue}", 
                        BusinessConstants.LogCategory.OrderCollaboration);
                }

                return result;
            }
            catch (Exception ex)
            {
                // 参照MessageHelper的异常日志记录方式
                MessageHelper.LogIntegrationException("用户催单统计", "UrgentOrder", 
                    HDPro.Core.ManageUser.UserContext.Current?.UserId ?? 0, "统计查询", ex);
                
                return GetDefaultStatisticsResult();
            }
        }

        /// <summary>
        /// 判断催单是否超期（参照MessageHelper的时间单位处理方式）
        /// </summary>
        /// <param name="urgentOrder">催单实体</param>
        /// <param name="currentTime">当前时间</param>
        /// <returns>是否超期</returns>
        private bool IsUrgentOrderOverdue(OCP_UrgentOrder urgentOrder, DateTime currentTime)
        {
            if (!urgentOrder.AssignedReplyTime.HasValue || !urgentOrder.TimeUnit.HasValue || !urgentOrder.CreateDate.HasValue)
                return false;

            var deadline = CalculateDeadlineByTimeUnit(urgentOrder.CreateDate.Value, 
                (int)urgentOrder.AssignedReplyTime.Value, urgentOrder.TimeUnit.Value);
            
            return currentTime > deadline;
        }

        /// <summary>
        /// 根据时间单位计算截止时间（参照MessageHelper的GetTimeUnitText方法）
        /// </summary>
        /// <param name="createDate">创建时间</param>
        /// <param name="assignedReplyTime">指定回复时间</param>
        /// <param name="timeUnit">时间单位：1-分钟，2-小时，3-天</param>
        /// <returns>截止时间</returns>
        private DateTime CalculateDeadlineByTimeUnit(DateTime createDate, int assignedReplyTime, int timeUnit)
        {
            return timeUnit switch
            {
                1 => createDate.AddMinutes(assignedReplyTime), // 分钟
                2 => createDate.AddHours(assignedReplyTime),   // 小时
                3 => createDate.AddDays(assignedReplyTime),    // 天
                _ => createDate.AddHours(assignedReplyTime)    // 默认按小时计算
            };
        }

        /// <summary>
        /// 获取默认统计结果
        /// </summary>
        /// <returns>默认统计数据</returns>
        private List<object> GetDefaultStatisticsResult()
        {
            return new List<object>
            {
                new { name = "催单总数", value = 0 },
                new { name = "催单待回复", value = 0 },
                new { name = "催单已超期", value = 0 },
                new { name = "催单已回复", value = 0 },
                new { name = "被催单待回复", value = 0 },
                new { name = "被催单已超时", value = 0 }
            };
        }

    }
}