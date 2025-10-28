using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HDPro.Core.Utilities;
using HDPro.CY.Order.IRepositories.MaterialCallBoard;
using HDPro.CY.Order.IServices.MaterialCallBoard;
using HDPro.CY.Order.Models.MaterialCallBoardDtos;

namespace HDPro.CY.Order.Services.MaterialCallBoard
{
    public class MaterialCallWorkOrderSetService : IMaterialCallWorkOrderSetService
    {
        private readonly IMaterialCallWorkOrderSetRepository _repo;
        public MaterialCallWorkOrderSetService(IMaterialCallWorkOrderSetRepository repo) => _repo = repo;

        public async Task<WebResponseContent> RefreshSnapshotAsync(List<MaterialCallWorkOrderCodeDto> list)
        {
            if (list == null || list.Count == 0)
                return WebResponseContent.Instance.Error("请求体不能为空（需要全量 WorkOrderCode 列表）");

            var codes = new List<string>(list.Count);
            var errors = new List<string>();
            for (int i = 0; i < list.Count; i++)
            {
                var x = list[i];
                if (x == null || string.IsNullOrWhiteSpace(x.WorkOrderCode))
                {
                    errors.Add($"第{i + 1}条：WorkOrderCode 不能为空");
                    continue;
                }
                codes.Add(x.WorkOrderCode.Trim());
            }
            if (errors.Any())
                return WebResponseContent.Instance.Error(string.Join("；", errors));

            try
            {
                var (total, matched) = await _repo.RefreshSnapshotAsync(codes);
                return WebResponseContent.Instance.OK($"快照写入完成：白名单 {total} 条，其中看板已存在 {matched} 条");
            }
            catch (Exception ex)
            {
                return WebResponseContent.Instance.Error("快照写入失败：" + ex.Message);
            }
        }

        public async Task<WebResponseContent> PruneAsync()
        {
            try
            {
                var deleted = await _repo.PruneMaterialCallBoardAsync();
                return WebResponseContent.Instance.OK($"删除完成：移除 {deleted} 条不在白名单中的叫料记录");
            }
            catch (Exception ex)
            {
                return WebResponseContent.Instance.Error("删除失败：" + ex.Message);
            }
        }
    }
}
