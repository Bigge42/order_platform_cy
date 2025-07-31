/*
 *所有关于OCP_UrgentOrderReply类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*OCP_UrgentOrderReplyService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using System;
using System.Threading.Tasks;
using HDPro.Core.Services;
using HDPro.CY.Order.Services.OrderCollaboration.Common;
using HDPro.Core.UserManager;

namespace HDPro.CY.Order.Services
{
    public partial class OCP_UrgentOrderReplyService
    {
        private readonly IOCP_UrgentOrderReplyRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public OCP_UrgentOrderReplyService(
            IOCP_UrgentOrderReplyRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
            )
        : base(dbRepository, httpContextAccessor)
        {
            _repository = dbRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        /// <summary>
        /// 重写CY.Order项目特有的初始化逻辑
        /// 可在此处添加OCP_UrgentOrderReply特有的初始化代码
        /// </summary>
        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
            // 在此处添加OCP_UrgentOrderReply特有的初始化逻辑
        }

        /// <summary>
        /// 重写CY.Order项目通用数据验证方法
        /// 可在此处添加OCP_UrgentOrderReply特有的数据验证逻辑
        /// </summary>
        /// <param name="entity">要验证的实体</param>
        /// <returns>验证结果</returns>
        protected override WebResponseContent ValidateCYOrderEntity(OCP_UrgentOrderReply entity)
        {
            var response = base.ValidateCYOrderEntity(entity);
            
            // 在此处添加OCP_UrgentOrderReply特有的数据验证逻辑
            
            return response;
        }

        /// <summary>
        /// 添加催单回复记录
        /// </summary>
        /// <param name="urgentOrderReply">催单回复实体</param>
        /// <returns>操作结果</returns>
        public async Task<WebResponseContent> AddReplyAsync(OCP_UrgentOrderReply urgentOrderReply)
        {
            var response = new WebResponseContent();
            
            try
            {
                // 1. 数据验证
                if (urgentOrderReply == null)
                {
                    return response.Error("催单回复数据不能为空");
                }

                if (urgentOrderReply.UrgentOrderID <= 0)
                {
                    return response.Error("催单ID不能为空");
                }

                // 2. 业务验证 - 检查催单记录是否存在
                var urgentOrderExists = await _repository.DbContext.Set<OCP_UrgentOrder>()
                    .AnyAsync(u => u.UrgentOrderID == urgentOrderReply.UrgentOrderID);
                
                if (!urgentOrderExists)
                {
                    return response.Error("指定的催单记录不存在");
                }

                // 3. 设置默认值
                urgentOrderReply.SetCreateDefaultVal();
                if (urgentOrderReply.ReplyTime == null)
                {
                    urgentOrderReply.ReplyTime = DateTime.Now;
                }

                // 4. 实体验证
                var validationResult = ValidateCYOrderEntity(urgentOrderReply);
                if (!validationResult.Status)
                {
                    return validationResult;
                }

                // 5. 使用事务保存数据并更新催单状态
                using (var transaction = await _repository.DbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 保存回复记录
                        await _repository.AddAsync(urgentOrderReply);
                        
                        // 更新催单状态为"已回复"
                        var urgentOrder = await _repository.DbContext.Set<OCP_UrgentOrder>()
                            .FirstOrDefaultAsync(u => u.UrgentOrderID == urgentOrderReply.UrgentOrderID);
                        
                        if (urgentOrder != null)
                        {
                            urgentOrder.UrgentStatus = BusinessConstants.UrgentOrderStatus.Replied;
                            urgentOrder.ModifyDate = DateTime.Now;
                            urgentOrder.ModifyID = HDPro.Core.ManageUser.UserContext.Current?.UserInfo?.User_Id ?? 1;
                            urgentOrder.Modifier = HDPro.Core.ManageUser.UserContext.Current?.UserInfo?.UserTrueName ?? "系统";
                            
                            _repository.DbContext.Set<OCP_UrgentOrder>().Update(urgentOrder);
                        }
                        
                        await _repository.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }

                // 记录操作日志
                LogCYOrderOperation("AddReply", urgentOrderReply,
                    $"添加催单回复成功，催单ID：{urgentOrderReply.UrgentOrderID}，回复ID：{urgentOrderReply.ReplyID}，催单状态已更新为已回复");

                response.OK("催单回复添加成功");
                if (response.Status)
                {
                    response.Data = new { 
                        replyId = urgentOrderReply.ReplyID,
                        urgentOrderId = urgentOrderReply.UrgentOrderID,
                        replyTime = urgentOrderReply.ReplyTime
                    };
                }

                return response;
            }
            catch (Exception ex)
            {
                Logger.Error($"添加催单回复失败：{ex.Message}");
                return response.Error($"添加催单回复失败：{ex.Message}");
            }
        }
  }
} 