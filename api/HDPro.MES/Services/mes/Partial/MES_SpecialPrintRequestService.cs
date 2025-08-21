/*
 *所有关于MES_SpecialPrintRequest类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*MES_SpecialPrintRequestService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using HDPro.Core.BaseProvider;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;
using System.Linq;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
using HDPro.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.MES.IRepositories;
using HDPro.Core.Services;

namespace HDPro.MES.Services
{
    public partial class MES_SpecialPrintRequestService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMES_SpecialPrintRequestRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public MES_SpecialPrintRequestService(
            IMES_SpecialPrintRequestRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        /// <summary>
        /// 添加特殊打印資料记录
        /// </summary>
        /// <param name="requestEntity">请求实体</param>
        /// <returns>操作结果</returns>
        public async Task<WebResponseContent> AddReplyAsync(MES_SpecialPrintRequest requestEntity)
        {
            var response = new WebResponseContent();

            try
            {
                // 1. 数据验证
                if (requestEntity == null)
                {
                    return response.Error("推送数据不能为空");
                }

                if (requestEntity.udf1_key ==null)
                {
                    return response.Error("自定义字段1不能为空");
                }

                // 2. 业务验证 - 检查特殊打印资料是否存在
                var requestEntityExists = await _repository.DbContext.Set<MES_SpecialPrintRequest>()
                    .AnyAsync(u => u.udf1_key == requestEntity.udf1_key);

                if (requestEntityExists)
                {
                    //return response.Error("指定的自定义打印信息已存在");
                }

                // 3. 设置默认值
                requestEntity.SetCreateDefaultVal();
                if (requestEntity.udf2_key == null)
                {
                    requestEntity.udf2_key = "测试空字段2";
                }

                //// 4. 实体验证
                //var validationResult = ValidateCYOrderEntity(requestEntity);
                //if (!validationResult.Status)
                //{
                //    return validationResult;
                //}

                // 5. 使用事务保存数据并更新时间
                using (var transaction = await _repository.DbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 查找已存在的推送记录
                        var existSPprint = await _repository.DbContext.Set<MES_SpecialPrintRequest>()
                            .FirstOrDefaultAsync(u => u.udf1_key == requestEntity.udf1_key );

                        if (existSPprint != null)
                        {
                            existSPprint.udf1_value = requestEntity.udf1_value;
                            existSPprint.udf2_key = requestEntity.udf2_key;
                            existSPprint.udf2_value = requestEntity.udf2_value;
                            //existSPprint. = HDPro.Core.ManageUser.UserContext.Current?.UserInfo?.UserTrueName ?? "系统";

                            _repository.DbContext.Set<MES_SpecialPrintRequest>().Update(existSPprint);
                        }
                        else 
                        {
                            // 保存新的推送记录
                            await _repository.AddAsync(requestEntity);
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
                //LogCYOrderOperation("AddReply", requestEntity,
                //    $"添加特殊打印资料成功，识别ID：{requestEntity.udf1_key}，显示值：{requestEntity.udf1_value}，测试更新");

                response.OK("特殊打印资料添加成功");
                if (response.Status)
                {
                    response.Data = new
                    {
                        udf1_value = requestEntity.udf1_value,
                        udf2_key = requestEntity.udf2_value,
                        udf2_value = requestEntity.udf2_value
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

        ///// <summary>
        ///// 重写CY.Order项目通用数据验证方法
        ///// 可在此处添加OCP_requestEntity特有的数据验证逻辑
        ///// </summary>
        ///// <param name="entity">要验证的实体</param>
        ///// <returns>验证结果</returns>
        //protected override WebResponseContent ValidateCYOrderEntity(MES_SpecialPrintRequest entity)
        //{
        //    var response = base.ValidateCYOrderEntity(entity);

        //    // 在此处添加OCP_requestEntity特有的数据验证逻辑

        //    return response;
        //}
    }
}
