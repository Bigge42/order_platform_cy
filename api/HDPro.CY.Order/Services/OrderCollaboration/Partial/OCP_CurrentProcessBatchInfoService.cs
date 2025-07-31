/*
 *所有关于OCP_CurrentProcessBatchInfo类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*OCP_CurrentProcessBatchInfoService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using HDPro.CY.Order.Services;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;
using System.Linq;
using HDPro.Core.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.CY.Order.IRepositories;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.SalesManagement;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace HDPro.CY.Order.Services
{
    public partial class OCP_CurrentProcessBatchInfoService
    {
        private readonly IOCP_CurrentProcessBatchInfoRepository _repository;//访问数据库
        private readonly ILogger<OCP_CurrentProcessBatchInfoService> _logger;

        [ActivatorUtilitiesConstructor]
        public OCP_CurrentProcessBatchInfoService(
            IOCP_CurrentProcessBatchInfoRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<OCP_CurrentProcessBatchInfoService> logger
            )
        : base(dbRepository, httpContextAccessor)
        {
            _repository = dbRepository;
            _logger = logger;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        /// <summary>
        /// 重写CY.Order项目特有的初始化逻辑
        /// 可在此处添加OCP_CurrentProcessBatchInfo特有的初始化代码
        /// </summary>
        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
            // 在此处添加OCP_CurrentProcessBatchInfo特有的初始化逻辑
        }

        /// <summary>
        /// 重写CY.Order项目通用数据验证方法
        /// 可在此处添加OCP_CurrentProcessBatchInfo特有的数据验证逻辑
        /// </summary>
        /// <param name="entity">要验证的实体</param>
        /// <returns>验证结果</returns>
        protected override WebResponseContent ValidateCYOrderEntity(OCP_CurrentProcessBatchInfo entity)
        {
            var response = base.ValidateCYOrderEntity(entity);

            // 在此处添加OCP_CurrentProcessBatchInfo特有的数据验证逻辑

            return response;
        }
         //查询(主表合计)
        public override PageGridData<OCP_CurrentProcessBatchInfo> GetPageData(PageDataOptions options)
        {
            try
            {
                // 在查询之前，先尝试同步ESB数据
                var mtoNoFromOptions = ExtractMtoNoFromOptions(options);
                if (!string.IsNullOrWhiteSpace(mtoNoFromOptions))
                {
                    SyncBatchInfoBeforeQuery(mtoNoFromOptions);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "查询前同步ESB数据时发生异常，继续执行查询操作");
            }

            return base.GetPageData(options);
        }

        /// <summary>
        /// 从PageDataOptions参数中提取MtoNo
        /// </summary>
        /// <param name="options">查询选项</param>
        /// <returns>计划跟踪号</returns>
        private string? ExtractMtoNoFromOptions(PageDataOptions options)
        {
            try
            {
                // 先检查Filter属性中的查询条件
                if (options?.Filter?.Any() == true)
                {
                    var mtoNoCondition = options.Filter.FirstOrDefault(w =>
                        string.Equals(w.Name, "MtoNo", StringComparison.OrdinalIgnoreCase));

                    if (!string.IsNullOrWhiteSpace(mtoNoCondition?.Value))
                    {
                        _logger?.LogInformation("从Filter查询条件中提取到MtoNo: {MtoNo}", mtoNoCondition.Value);
                        return mtoNoCondition.Value;
                    }
                }

                // 如果Filter为空，尝试从JSON格式的Wheres字符串中解析
                if (!string.IsNullOrEmpty(options?.Wheres))
                {
                    try
                    {
                        var whereConditions = JsonConvert.DeserializeObject<dynamic[]>(options.Wheres);
                        if (whereConditions != null)
                        {
                            foreach (var condition in whereConditions)
                            {
                                if (condition?.name?.ToString().Equals("MtoNo", StringComparison.OrdinalIgnoreCase) == true)
                                {
                                    var mtoNo = condition.value?.ToString();
                                    if (!string.IsNullOrWhiteSpace(mtoNo))
                                    {
                                       // _logger?.LogInformation("从Wheres JSON查询条件中提取到MtoNo: {MtoNo}", mtoNo);
                                        return mtoNo;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "解析Wheres JSON格式的查询条件时发生异常");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "提取MtoNo时发生异常");
            }

            return null;
        }

        /// <summary>
        /// 根据MtoNo同步批次信息
        /// </summary>
        /// <param name="mtoNo">计划跟踪号</param>
        private void SyncBatchInfoBeforeQuery(string mtoNo)
        {
            try
            {
                _logger?.LogInformation("开始为MtoNo={MtoNo}同步ESB批次信息数据", mtoNo);

                // 获取SalesBatchInfoESBSyncService实例并调用同步方法
                var syncService = SalesBatchInfoESBSyncService.Instance;
                if (syncService != null)
                {
                     var result =  syncService.SyncByPlanTrackingNo(mtoNo).GetAwaiter().GetResult();
                    if (result.Status)
                    {
                        _logger?.LogInformation("MtoNo={MtoNo}的ESB批次信息同步成功", mtoNo);
                    }
                    else
                    {
                        _logger?.LogWarning("MtoNo={MtoNo}的ESB批次信息同步失败: {Message}", mtoNo, result.Message);
                    }
                }
                else
                {
                    _logger?.LogWarning("无法获取SalesBatchInfoESBSyncService实例");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "同步MtoNo={MtoNo}的ESB批次信息时发生异常", mtoNo);
            }
        }
  }
} 