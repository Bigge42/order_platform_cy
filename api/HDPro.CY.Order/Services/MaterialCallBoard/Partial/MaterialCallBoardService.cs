/*
 *所有关于MaterialCallBoard类的业务代码应在此处编写
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HDPro.Core.Utilities;
using HDPro.CY.Order.IRepositories;
using HDPro.CY.Order.IServices;
using HDPro.CY.Order.Models.MaterialCallBoardDtos;
using HDPro.Entity.DomainModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HDPro.CY.Order.Services
{
    public partial class MaterialCallBoardService
    {
        private readonly IMaterialCallBoardRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public MaterialCallBoardService(
            IMaterialCallBoardRepository repository,
            IHttpContextAccessor httpContextAccessor)
            : base(repository, httpContextAccessor)
        {
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<WebResponseContent> BatchUpsertAsync(IEnumerable<MaterialCallBoardBatchDto> entries)
        {
            if (entries == null)
            {
                return WebResponseContent.Instance.Error("请求体不能为空");
            }

            var indexed = entries
                .Select((item, index) => new { Item = item, Index = index + 1 })
                .ToList();

            if (indexed.Count == 0)
            {
                return WebResponseContent.Instance.OK("没有可处理的数据");
            }

            var errors = new List<string>();

            foreach (var entry in indexed)
            {
                if (entry.Item == null)
                {
                    errors.Add($"第{entry.Index}条数据不能为空");
                    continue;
                }

                entry.Item.WorkOrderNo = entry.Item.WorkOrderNo?.Trim();
                entry.Item.PlanTrackNo = entry.Item.PlanTrackNo?.Trim();
                entry.Item.ProductCode = entry.Item.ProductCode?.Trim();
                entry.Item.CallerName = entry.Item.CallerName?.Trim();

                if (string.IsNullOrWhiteSpace(entry.Item.WorkOrderNo))
                {
                    errors.Add($"第{entry.Index}条数据的工单号不能为空");
                }

                if (string.IsNullOrWhiteSpace(entry.Item.PlanTrackNo))
                {
                    errors.Add($"第{entry.Index}条数据的计划跟踪号不能为空");
                }

                if (string.IsNullOrWhiteSpace(entry.Item.ProductCode))
                {
                    errors.Add($"第{entry.Index}条数据的产品编号不能为空");
                }

                if (string.IsNullOrWhiteSpace(entry.Item.CallerName))
                {
                    errors.Add($"第{entry.Index}条数据的叫料人不能为空");
                }

                entry.Item.CalledAt ??= DateTime.Now;
            }

            if (errors.Count > 0)
            {
                return WebResponseContent.Instance.Error(string.Join("；", errors));
            }

            var deduped = indexed
                .Where(x => x.Item != null)
                .GroupBy(x => x.Item.WorkOrderNo!, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.Last().Item)
                .ToList();

            var workOrderNos = deduped.Select(x => x.WorkOrderNo!).ToList();

            var dbContext = _repository.DbContext;
            var dbSet = dbContext.Set<MaterialCallBoard>();

            List<MaterialCallBoard> existingEntities = await dbSet
                .Where(x => workOrderNos.Contains(x.WorkOrderNo))
                .ToListAsync();

            var existingLookup = existingEntities.ToDictionary(x => x.WorkOrderNo, StringComparer.OrdinalIgnoreCase);

            var toInsert = new List<MaterialCallBoard>();
            var updatedCount = 0;

            foreach (var dto in deduped)
            {
                if (existingLookup.TryGetValue(dto.WorkOrderNo!, out var entity))
                {
                    entity.PlanTrackNo = dto.PlanTrackNo!;
                    entity.ProductCode = dto.ProductCode!;
                    entity.CallerName = dto.CallerName!;
                    entity.CalledAt = dto.CalledAt!.Value;
                    updatedCount++;
                }
                else
                {
                    toInsert.Add(new MaterialCallBoard
                    {
                        WorkOrderNo = dto.WorkOrderNo!,
                        PlanTrackNo = dto.PlanTrackNo!,
                        ProductCode = dto.ProductCode!,
                        CallerName = dto.CallerName!,
                        CalledAt = dto.CalledAt!.Value
                    });
                }
            }

            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                if (toInsert.Count > 0)
                {
                    await dbSet.AddRangeAsync(toInsert);
                }

                if (updatedCount > 0 || toInsert.Count > 0)
                {
                    await dbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return WebResponseContent.Instance.OK($"批量导入成功，更新 {updatedCount} 条，新增 {toInsert.Count} 条");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return WebResponseContent.Instance.Error($"批量导入失败: {ex.Message}");
            }
        }
    }
}
