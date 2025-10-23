/*
 *所有关于MaterialCallBoard类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*MaterialCallBoardService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using HDPro.CY.Order.Services;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;
using HDPro.Entity.DomainModels.MaterialCallBoard.dto;
using System.Collections.Generic;
using System.Linq;
using HDPro.Core.Utilities;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.CY.Order.IRepositories;

namespace HDPro.CY.Order.Services
{
    public partial class MaterialCallBoardService
    {
        private readonly IMaterialCallBoardRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public MaterialCallBoardService(
            IMaterialCallBoardRepository dbRepository,
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
        /// 可在此处添加MaterialCallBoard特有的初始化代码
        /// </summary>
        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
            // 在此处添加MaterialCallBoard特有的初始化逻辑
        }

        /// <summary>
        /// 重写CY.Order项目通用数据验证方法
        /// 可在此处添加MaterialCallBoard特有的数据验证逻辑
        /// </summary>
        /// <param name="entity">要验证的实体</param>
        /// <returns>验证结果</returns>
        protected override WebResponseContent ValidateCYOrderEntity(MaterialCallBoard entity)
        {
            var response = base.ValidateCYOrderEntity(entity);

            // 在此处添加MaterialCallBoard特有的数据验证逻辑

            return response;
        }

        /// <summary>
        /// 批量新增或更新 MaterialCallBoard 数据
        /// </summary>
        /// <param name="payload">外部系统传入的数据集合</param>
        /// <returns>写入结果</returns>
        public async Task<WebResponseContent> BatchUpsertAsync(IEnumerable<MaterialCallBoardBatchDto> payload)
        {
            if (payload == null)
            {
                return WebResponseContent.Instance.Error("请求数据不能为空");
            }

            var items = payload.Where(item => item != null).ToList();
            if (!items.Any())
            {
                return WebResponseContent.Instance.Error("请求数据不能为空");
            }

            var validationErrors = new List<string>();
            for (int i = 0; i < items.Count; i++)
            {
                var current = items[i];
                var rowIndex = i + 1;

                if (string.IsNullOrWhiteSpace(current.WorkOrderNo))
                {
                    validationErrors.Add($"第{rowIndex}条数据的工单号不能为空");
                }
                if (string.IsNullOrWhiteSpace(current.PlanTrackNo))
                {
                    validationErrors.Add($"第{rowIndex}条数据的计划跟踪号不能为空");
                }
                if (string.IsNullOrWhiteSpace(current.ProductCode))
                {
                    validationErrors.Add($"第{rowIndex}条数据的产品编号不能为空");
                }
                if (string.IsNullOrWhiteSpace(current.CallerName))
                {
                    validationErrors.Add($"第{rowIndex}条数据的叫料人不能为空");
                }
                if (current.CalledAt == default)
                {
                    validationErrors.Add($"第{rowIndex}条数据的叫料时间不能为空");
                }
            }

            var duplicateWorkOrders = items
                .Where(item => !string.IsNullOrWhiteSpace(item.WorkOrderNo))
                .GroupBy(item => item.WorkOrderNo.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            if (duplicateWorkOrders.Any())
            {
                validationErrors.Add($"存在重复的工单号: {string.Join(", ", duplicateWorkOrders)}");
            }

            if (validationErrors.Any())
            {
                return WebResponseContent.Instance.ErrorData("数据校验失败", validationErrors);
            }

            var normalizedItems = new Dictionary<string, MaterialCallBoardBatchDto>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in items)
            {
                normalizedItems[item.WorkOrderNo.Trim()] = NormalizePayload(item);
            }

            var workOrderNos = normalizedItems.Keys.ToList();

            return await Task.Run(() => _repository.DbContextBeginTransaction(() =>
            {
                var response = new WebResponseContent();

                try
                {
                    var existingEntities = _repository.DbContext.Set<MaterialCallBoard>()
                        .Where(entity => workOrderNos.Contains(entity.WorkOrderNo))
                        .ToList();

                    var existingMap = existingEntities.ToDictionary(entity => entity.WorkOrderNo, StringComparer.OrdinalIgnoreCase);

                    var toInsert = new List<MaterialCallBoard>();
                    var toUpdate = new List<MaterialCallBoard>();

                    foreach (var kvp in normalizedItems)
                    {
                        var key = kvp.Key;
                        var dto = kvp.Value;

                        if (existingMap.TryGetValue(key, out var entity))
                        {
                            entity.PlanTrackNo = dto.PlanTrackNo;
                            entity.ProductCode = dto.ProductCode;
                            entity.CallerName = dto.CallerName;
                            entity.CalledAt = dto.CalledAt;
                            toUpdate.Add(entity);
                        }
                        else
                        {
                            toInsert.Add(new MaterialCallBoard
                            {
                                WorkOrderNo = key,
                                PlanTrackNo = dto.PlanTrackNo,
                                ProductCode = dto.ProductCode,
                                CallerName = dto.CallerName,
                                CalledAt = dto.CalledAt
                            });
                        }
                    }

                    if (toUpdate.Any())
                    {
                        _repository.UpdateRange(toUpdate, new[]
                        {
                            nameof(MaterialCallBoard.PlanTrackNo),
                            nameof(MaterialCallBoard.ProductCode),
                            nameof(MaterialCallBoard.CallerName),
                            nameof(MaterialCallBoard.CalledAt)
                        });
                    }

                    if (toInsert.Any())
                    {
                        _repository.AddRange(toInsert);
                    }

                    if (toInsert.Any() || toUpdate.Any())
                    {
                        _repository.SaveChanges();
                    }

                    return response.OK($"批量写入成功，新增 {toInsert.Count} 条，更新 {toUpdate.Count} 条");
                }
                catch (Exception ex)
                {
                    return response.Error($"批量写入失败：{ex.Message}");
                }
            }));
        }

        private static MaterialCallBoardBatchDto NormalizePayload(MaterialCallBoardBatchDto source)
        {
            return new MaterialCallBoardBatchDto
            {
                WorkOrderNo = source.WorkOrderNo?.Trim(),
                PlanTrackNo = source.PlanTrackNo?.Trim(),
                ProductCode = source.ProductCode?.Trim(),
                CallerName = source.CallerName?.Trim(),
                CalledAt = source.CalledAt
            };
        }
    }
}
