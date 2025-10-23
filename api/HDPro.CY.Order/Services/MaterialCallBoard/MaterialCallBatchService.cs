using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HDPro.Core.Utilities;
using HDPro.Entity.DomainModels;                 // MaterialCallBoard 实体
using HDPro.CY.Order.IRepositories;             // IMaterialCallBoardRepository
using HDPro.CY.Order.IServices.MaterialCallBoard;
using HDPro.CY.Order.Models.MaterialCallBoardDtos;

namespace HDPro.CY.Order.Services.MaterialCallBoard
{
    /// <summary>
    /// MaterialCallBoard 批量导入的独立服务实现
    /// </summary>
    public class MaterialCallBatchService : IMaterialCallBatchService
    {
        private readonly IMaterialCallBoardRepository _repo;

        public MaterialCallBatchService(IMaterialCallBoardRepository repo)
        {
            _repo = repo;
        }

        public async Task<WebResponseContent> BatchUpsertAsync(List<MaterialCallBoardBatchDto> data)
        {
            if (data == null || data.Count == 0)
                return WebResponseContent.Instance.Error("请求体不能为空");

            // 1) 基础校验
            var errors = new List<string>();
            for (int i = 0; i < data.Count; i++)
            {
                var d = data[i];
                if (string.IsNullOrWhiteSpace(d.WorkOrderNo)) errors.Add($"第{i + 1}条：工单号不能为空");
                if (string.IsNullOrWhiteSpace(d.PlanTrackNo)) errors.Add($"第{i + 1}条：计划跟踪号不能为空");
                if (string.IsNullOrWhiteSpace(d.ProductCode)) errors.Add($"第{i + 1}条：产品编号不能为空");
                if (string.IsNullOrWhiteSpace(d.CallerName)) errors.Add($"第{i + 1}条：叫料人不能为空");
            }
            if (errors.Count > 0)
                return WebResponseContent.Instance.Error(string.Join("；", errors));

            // 2) 同一批内 WorkOrderNo 去重，后者覆盖前者
            var map = new Dictionary<string, MaterialCallBoardBatchDto>(StringComparer.OrdinalIgnoreCase);
            foreach (var x in data) map[x.WorkOrderNo] = x;

            // 3) 事务执行批量 Upsert
            // 说明：IMaterialCallBoardRepository 通常继承通用仓储，具备 Find / AddRange / SaveChanges / DbContextBeginTransaction
            var result = _repo.DbContextBeginTransaction(() =>
            {
                int updated = 0, inserted = 0;

                var final = map.Values.ToList();
                var workNos = final.Select(x => x.WorkOrderNo).ToList();

                // 已存在记录
                var exists = _repo.Find(e => workNos.Contains(e.WorkOrderNo)).ToList();

                foreach (var e in exists)
                {
                    var dto = map[e.WorkOrderNo];
                    e.PlanTrackNo = dto.PlanTrackNo;
                    e.ProductCode = dto.ProductCode;
                    e.CallerName = dto.CallerName;
                    e.CalledAt = dto.CalledAt ?? DateTime.Now;
                    updated++;
                    final.RemoveAll(v => v.WorkOrderNo.Equals(e.WorkOrderNo, StringComparison.OrdinalIgnoreCase));
                }

                if (final.Count > 0)
                {
                    var news = final.Select(dto => new MaterialCallBoard
                    {
                        WorkOrderNo = dto.WorkOrderNo,
                        PlanTrackNo = dto.PlanTrackNo,
                        ProductCode = dto.ProductCode,
                        CallerName = dto.CallerName,
                        CalledAt = dto.CalledAt ?? DateTime.Now
                    }).ToList();

                    _repo.AddRange(news);
                    inserted = news.Count;
                }

                _repo.SaveChanges();
                return WebResponseContent.Instance.OK($"批量导入成功，更新 {updated} 条，新增 {inserted} 条");
            });

            // 因为仓储事务是同步委托，这里用 Task.FromResult 包装即可
            return await Task.FromResult(result);
        }
    }
}
