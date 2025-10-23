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
using HDPro.Core.Utilities;
using HDPro.CY.Order.IRepositories;
using HDPro.CY.Order.Services.MaterialCallBoard.Models;
using HDPro.Entity.DomainModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        /// 批量新增或更新 MaterialCallBoard 数据
        /// </summary>
        /// <param name="payload">外部系统传入的数据集合</param>
        /// <returns>写入结果</returns>
        public Task<WebResponseContent> BatchUpsertAsync(IEnumerable<MaterialCallBoardBatchDto> payload)
        {
            if (payload == null)
            {
                return Task.FromResult(WebResponseContent.Instance.Error("请求数据不能为空"));
            }

            var items = payload
                .Where(item => item != null)
                .Select(NormalizePayload)
                .ToList();
            if (!items.Any())
            {
                return Task.FromResult(WebResponseContent.Instance.Error("请求数据不能为空"));
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
                return Task.FromResult(WebResponseContent.Instance.ErrorData("数据校验失败", validationErrors));
            }

            var workOrderNos = items.Select(item => item.WorkOrderNo).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            var response = _repository.DbContextBeginTransaction(() =>
            {
                try
                {
                    var existingEntities = _repository.DbContext.Set<MaterialCallBoard>()
                        .Where(entity => workOrderNos.Contains(entity.WorkOrderNo))
                        .ToList();

                    var existingMap = existingEntities.ToDictionary(entity => entity.WorkOrderNo, StringComparer.OrdinalIgnoreCase);

                    var toInsert = new List<MaterialCallBoard>();
                    var toUpdate = new List<MaterialCallBoard>();

                    foreach (var dto in items)
                    {
                        var workOrderNo = dto.WorkOrderNo;

                        if (existingMap.TryGetValue(workOrderNo, out var entity))
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
                                WorkOrderNo = workOrderNo,
                                PlanTrackNo = dto.PlanTrackNo,
                                ProductCode = dto.ProductCode,
                                CallerName = dto.CallerName,
                                CalledAt = dto.CalledAt
                            });
                        }
                    }

                    if (toInsert.Any())
                    {
                        _repository.AddRange(toInsert);
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

                    if (toInsert.Any() || toUpdate.Any())
                    {
                        _repository.SaveChanges();
                    }

                    return WebResponseContent.Instance.OK($"批量写入成功，新增 {toInsert.Count} 条，更新 {toUpdate.Count} 条");
                }
                catch (Exception ex)
                {
                    return WebResponseContent.Instance.Error($"批量写入失败：{ex.Message}");
                }
            });

            return Task.FromResult(response);
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
