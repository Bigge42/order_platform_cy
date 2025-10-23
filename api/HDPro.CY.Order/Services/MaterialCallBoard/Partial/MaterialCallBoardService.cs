/*
 *所有关于MaterialCallBoard类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*MaterialCallBoardService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using HDPro.Core.Utilities;
using HDPro.CY.Order.IRepositories;
using HDPro.CY.Order.Models.MaterialCallBoardDtos;
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
            }

            if (validationErrors.Any())
            {
                return Task.FromResult(WebResponseContent.Instance.Error(string.Join("；", validationErrors)));
            }

            using var tran = _repository.DbContextBeginTransaction();
            try
            {
                var existing = _repository
                    .FindAsIQueryable(x => items.Select(i => i.WorkOrderNo).Contains(x.WorkOrderNo), true)
                    .ToDictionary(x => x.WorkOrderNo, x => x);

                var toInsert = new List<MaterialCallBoard>();

                foreach (var dto in items)
                {
                    if (existing.TryGetValue(dto.WorkOrderNo, out var entity))
                    {
                        UpdateEntity(entity, dto);
                        _repository.Update(entity, true);
                    }
                    else
                    {
                        var newEntity = CreateEntity(dto);
                        toInsert.Add(newEntity);
                    }
                }

                if (toInsert.Any())
                {
                    _repository.AddRange(toInsert, true);
                }

                tran.Commit();
                return Task.FromResult(WebResponseContent.Instance.OK("批量导入成功"));
            }
            catch (Exception ex)
            {
                tran.Rollback();
                return Task.FromResult(WebResponseContent.Instance.Error($"批量导入失败: {ex.Message}"));
            }
        }

        private static MaterialCallBoardBatchDto NormalizePayload(MaterialCallBoardBatchDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new MaterialCallBoardBatchDto
            {
                WorkOrderNo = dto.WorkOrderNo?.Trim(),
                PlanTrackNo = dto.PlanTrackNo?.Trim(),
                ProductCode = dto.ProductCode?.Trim(),
                CallerName = dto.CallerName?.Trim(),
                CalledAt = dto.CalledAt == default ? DateTime.Now : dto.CalledAt
            };
        }

        private static void UpdateEntity(MaterialCallBoard target, MaterialCallBoardBatchDto source)
        {
            target.PlanTrackNo = source.PlanTrackNo;
            target.ProductCode = source.ProductCode;
            target.CallerName = source.CallerName;
            target.CalledAt = source.CalledAt;
        }

        private static MaterialCallBoard CreateEntity(MaterialCallBoardBatchDto source)
        {
            return new MaterialCallBoard
            {
                WorkOrderNo = source.WorkOrderNo,
                PlanTrackNo = source.PlanTrackNo,
                ProductCode = source.ProductCode,
                CallerName = source.CallerName,
                CalledAt = source.CalledAt
            };
        }
    }
}
