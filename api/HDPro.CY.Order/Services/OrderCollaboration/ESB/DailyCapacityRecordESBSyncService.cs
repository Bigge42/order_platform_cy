using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HDPro.Entity.DomainModels;
using HDPro.Entity.DomainModels.ESB;
using HDPro.CY.Order.IRepositories;
using HDPro.Core.Utilities;
using HDPro.CY.Order.Services.OrderCollaboration.ESB;
using HDPro.Core.Extensions.AutofacManager;

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB
{
    /// <summary>
    /// 每日产能记录ESB同步服务实现
    /// 对接ESB接口：/gateway/DataCenter/CXCNSJ 
    /// 将ESB数据同步到 OCP_DailyCapacityRecord 表
    /// </summary>
    public class DailyCapacityRecordESBSyncService
        : ESBSyncServiceBase<OCP_DailyCapacityRecord, ESBDailyCapacityRecordData, IOCP_DailyCapacityRecordRepository>, IDependency
    {
        public DailyCapacityRecordESBSyncService(
            IOCP_DailyCapacityRecordRepository repository,
            ESBBaseService esbService,
            ILogger<DailyCapacityRecordESBSyncService> logger)
            : base(repository, esbService, logger)
        {
            // 构造函数通过依赖注入获取仓储和ESB基础服务
        }

        /// <summary>
        /// 获取ESB接口配置名称（用于定位ESB API路径）
        /// </summary>
        protected override string GetESBApiConfigName()
        {
            // 返回当前服务类名，对应配置中的键
            return nameof(DailyCapacityRecordESBSyncService);
        }

        /// <summary>
        /// 获取操作类型描述（用于日志）
        /// </summary>
        protected override string GetOperationType()
        {
            return "每日产能记录";
        }

        /// <summary>
        /// 验证单条ESB数据的有效性
        /// </summary>
        protected override bool ValidateESBData(ESBDailyCapacityRecordData esbData)
        {
            // 基本字段非空验证
            if (string.IsNullOrWhiteSpace(esbData.F_ORA_FMLB) ||
                string.IsNullOrWhiteSpace(esbData.F_ORA_SCX) ||
                string.IsNullOrWhiteSpace(esbData.F_ORA_DATE1))
            {
                ESBLogger?.LogWarning($"产能记录数据缺少必要字段: 类别={esbData.F_ORA_FMLB}, 产线={esbData.F_ORA_SCX}, 日期={esbData.F_ORA_DATE1}");
                return false;
            }
            // 数量字段验证
            if (esbData.FQTY == null)
            {
                ESBLogger?.LogWarning($"产能记录数据数量为空: 日期={esbData.F_ORA_DATE1}, 产线={esbData.F_ORA_SCX}, 类别={esbData.F_ORA_FMLB}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取用于查询现有记录的键值
        /// </summary>
        protected override object GetEntityKey(ESBDailyCapacityRecordData esbData)
        {
            // 使用日期+产线+阀门类别组合作为唯一键
            string dateStr = esbData.F_ORA_DATE1;
            // 规范化日期格式（确保一致性，例如 "yyyy-MM-dd"）
            if (DateTime.TryParse(esbData.F_ORA_DATE1, out DateTime dt))
            {
                dateStr = dt.ToString("yyyy-MM-dd");
            }
            return $"{dateStr}|{esbData.F_ORA_SCX}|{esbData.F_ORA_FMLB}";
        }

        /// <summary>
        /// 查询指定键值的现有记录集合
        /// </summary>
        protected override async Task<List<OCP_DailyCapacityRecord>> QueryExistingRecords(List<object> keys)
        {
            // 将keys转成字符串集合（格式为 "日期|产线|类别"）
            var keySet = keys.Select(k => k.ToString()).ToList();
            // 查询数据库中具有匹配组合键的记录
            return await Task.Run(() =>
                _repository.FindAsIQueryable(record =>
                    keySet.Contains($"{record.ProductionDate:yyyy-MM-dd}|{record.ProductionLine}|{record.ValveCategory}")
                ).ToList()
            );
        }

        /// <summary>
        /// 判断现有记录是否匹配ESB数据
        /// </summary>
        protected override bool IsEntityMatch(OCP_DailyCapacityRecord entity, ESBDailyCapacityRecordData esbData)
        {
            // 以日期、产线、类别匹配判断同一记录
            if (!DateTime.TryParse(esbData.F_ORA_DATE1, out DateTime dataDate))
                return false;
            // 注意将日期时间去除时间部分再比较
            return entity.ProductionDate.Date == dataDate.Date
                && entity.ProductionLine == esbData.F_ORA_SCX
                && entity.ValveCategory == esbData.F_ORA_FMLB;
        }

        /// <summary>
        /// 将ESB数据映射到实体对象
        /// </summary>
        protected override void MapESBDataToEntity(ESBDailyCapacityRecordData esbData, OCP_DailyCapacityRecord entity)
        {
            // 字段映射：将ESB数据赋值给实体属性
            if (DateTime.TryParse(esbData.F_ORA_DATE1, out DateTime prodDate))
            {
                // 取日期部分
                entity.ProductionDate = prodDate.Date;
            }
            else
            {
                entity.ProductionDate = DateTime.ParseExact(esbData.F_ORA_DATE1, "yyyy-MM-dd", null);
            }
            entity.ProductionLine = esbData.F_ORA_SCX;
            entity.ValveCategory = esbData.F_ORA_FMLB;
            entity.Quantity = esbData.FQTY.HasValue ? Convert.ToInt32(esbData.FQTY.Value) : 0;
            // 审计字段（创建者/修改者）将在批量处理时统一由基类设置
        }

        /// <summary>
        /// 执行批量数据库操作（插入或更新）
        /// </summary>
        protected override async Task<WebResponseContent> ExecuteBatchOperations(List<OCP_DailyCapacityRecord> toUpdate, List<OCP_DailyCapacityRecord> toInsert)
        {
            // 如果没有任何数据需要处理
            if (!toUpdate.Any() && !toInsert.Any())
            {
                return new WebResponseContent().OK("无产能数据需要同步更新");
            }
            // 使用事务批量提交插入和更新
            return await Task.Run(() => _repository.DbContextBeginTransaction(() =>
            {
                var response = new WebResponseContent();
                try
                {
                    if (toUpdate.Any())
                    {
                        _repository.UpdateRange(toUpdate, false);
                    }
                    if (toInsert.Any())
                    {
                        _repository.AddRange(toInsert, false);
                    }
                    _repository.SaveChanges();
                    int total = toUpdate.Count + toInsert.Count;
                    return response.OK($"同步成功：更新{toUpdate.Count}条，新增{toInsert.Count}条，合计{total}条产能记录。");
                }
                catch (Exception ex)
                {
                    // 发生异常则回滚
                    return response.Error($"同步产能数据失败：{ex.Message}");
                }
            }));
        }
    }
}
