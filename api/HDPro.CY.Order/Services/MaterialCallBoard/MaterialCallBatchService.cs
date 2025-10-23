using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HDPro.Core.Utilities;
using HDPro.CY.Order.IRepositories;
using HDPro.CY.Order.IServices.MaterialCallBoard;
using HDPro.CY.Order.Models.MaterialCallBoardDtos;

namespace HDPro.CY.Order.Services.MaterialCallBoard
{
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

            var errors = new List<string>();
            for (int i = 0; i < data.Count; i++)
            {
                var d = data[i];
                if (d == null) { errors.Add($"第{i + 1}条：数据为空"); continue; }
                if (string.IsNullOrWhiteSpace(d.WorkOrderNo)) errors.Add($"第{i + 1}条：工单号不能为空");
                if (string.IsNullOrWhiteSpace(d.PlanTrackNo)) errors.Add($"第{i + 1}条：计划跟踪号不能为空");
                if (string.IsNullOrWhiteSpace(d.ProductCode)) errors.Add($"第{i + 1}条：产品编号不能为空");
                if (string.IsNullOrWhiteSpace(d.CallerName)) errors.Add($"第{i + 1}条：叫料人不能为空");
                d.CalledAt ??= DateTime.Now;
            }
            if (errors.Count > 0) return WebResponseContent.Instance.Error(string.Join("；", errors));

            try
            {
                var (inserted, updated) = await _repo.BulkUpsertAsync(data);
                return WebResponseContent.Instance.OK($"批量导入成功，更新 {updated} 条，新增 {inserted} 条");
            }
            catch (Exception ex)
            {
                return WebResponseContent.Instance.Error("批量导入失败：" + ex.Message);
            }
        }
    }
}
