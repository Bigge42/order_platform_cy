/*
 *所有关于OCP_NegotiationReply类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*OCP_NegotiationReplyService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using HDPro.Core.ManageUser;

namespace HDPro.CY.Order.Services
{
    public partial class OCP_NegotiationReplyService
    {
        private readonly IOCP_NegotiationReplyRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public OCP_NegotiationReplyService(
            IOCP_NegotiationReplyRepository dbRepository,
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
        /// 可在此处添加OCP_NegotiationReply特有的初始化代码
        /// </summary>
        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
            // 在此处添加OCP_NegotiationReply特有的初始化逻辑
        }

        /// <summary>
        /// 重写CY.Order项目通用数据验证方法
        /// 可在此处添加OCP_NegotiationReply特有的数据验证逻辑
        /// </summary>
        /// <param name="entity">要验证的实体</param>
        /// <returns>验证结果</returns>
        protected override WebResponseContent ValidateCYOrderEntity(OCP_NegotiationReply entity)
        {
            var response = base.ValidateCYOrderEntity(entity);
            
            // 在此处添加OCP_NegotiationReply特有的数据验证逻辑
            
            return response;
        }

        /// <summary>
        /// 添加协商回复记录
        /// </summary>
        /// <param name="negotiationReply">协商回复实体</param>
        /// <returns>操作结果</returns>
        public async Task<WebResponseContent> AddReplyAsync(OCP_NegotiationReply negotiationReply)
        {
            var response = new WebResponseContent();
            
            try
            {
                // 1. 数据验证
                if (negotiationReply == null)
                {
                    return response.Error("协商回复数据不能为空");
                }

                if (negotiationReply.NegotiationID <= 0)
                {
                    return response.Error("协商ID不能为空");
                }

                // 2. 业务验证 - 检查协商记录是否存在
                var negotiationExists = await _repository.DbContext.Set<OCP_Negotiation>()
                    .AnyAsync(n => n.NegotiationID == negotiationReply.NegotiationID);
                
                if (!negotiationExists)
                {
                    return response.Error("指定的协商记录不存在");
                }

                // 3. 设置默认值
                negotiationReply.SetCreateDefaultVal();
                if (negotiationReply.ReplyTime == null)
                {
                    negotiationReply.ReplyTime = DateTime.Now;
                }

                // 4. 实体验证
                var validationResult = ValidateCYOrderEntity(negotiationReply);
                if (!validationResult.Status)
                {
                    return validationResult;
                }

                // 5. 使用事务保存数据并更新协商状态
                using (var transaction = await _repository.DbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 添加回复记录
                        await _repository.AddAsync(negotiationReply);
                        await _repository.SaveChangesAsync();
                        
                        // 获取当前用户信息
                        var currentUser = UserContext.Current;
                        var userInfo = currentUser?.UserInfo;
                        
                        // 根据传入的协商状态更新协商记录状态
                        var negotiation = await _repository.DbContext.Set<OCP_Negotiation>()
                            .FirstOrDefaultAsync(n => n.NegotiationID == negotiationReply.NegotiationID);
                        
                        if (negotiation != null)
                        {
                            // 根据传入的协商状态参数更新状态
                            if (!string.IsNullOrWhiteSpace(negotiationReply.NegotiationStatus))
                            {
                                negotiation.NegotiationStatus = negotiationReply.NegotiationStatus;
                            }
                            else
                            {
                                // 如果没有传入状态，默认设置为已同意
                                negotiation.NegotiationStatus = BusinessConstants.NegotiationStatus.Approved;
                            }
                            
                            negotiation.ModifyDate = DateTime.Now;
                            negotiation.Modifier = userInfo?.UserTrueName ?? "系统";
                            _repository.DbContext.Set<OCP_Negotiation>().Update(negotiation);
                            // 保存协商状态更新
                            await _repository.DbContext.SaveChangesAsync();
                        }
                        
                        await transaction.CommitAsync();
                        
                        // 记录操作日志
                        LogCYOrderOperation("AddReply", negotiationReply, 
                            $"添加协商回复成功，协商ID：{negotiationReply.NegotiationID}，回复ID：{negotiationReply.ReplyID}，协商状态已更新为：{negotiation?.NegotiationStatus ?? "未更新"}");
                        
                        response.OK("协商回复添加成功，协商状态已更新");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }

                if (response.Status)
                {
                    response.Data = new { 
                        replyId = negotiationReply.ReplyID,
                        negotiationId = negotiationReply.NegotiationID,
                        replyTime = negotiationReply.ReplyTime
                    };
                }

                return response;
            }
            catch (Exception ex)
            {
                Logger.Error($"添加协商回复失败：{ex.Message}");
                return response.Error($"添加协商回复失败：{ex.Message}");
            }
        }
  }
} 