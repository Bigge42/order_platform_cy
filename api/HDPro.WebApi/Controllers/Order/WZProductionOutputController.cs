using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using HDPro.CY.Order.IServices.WZ;
using HDPro.Entity.DomainModels.OrderCollaboration;
// ���������Ŀʹ��Ȩ�ޱ��/������������밴�����룺
// using HDPro.Core.Filters;
// using HDPro.Core.Controllers; ��

namespace HDPro.CY.Order.Controllers.WZ
{
    [ApiController]
    [Route("api/WZ/ProductionOutput")]
    // [PermissionTable(Name = "WZ_ProductionOutput")] // ����Ȩ�޿��ƣ����迪��
    public class WZProductionOutputController : ControllerBase
    {
        private readonly IWZProductionOutputService _service;

        public WZProductionOutputController(IWZProductionOutputService service)
        {
            _service = service;
        }

        /// <summary>
        /// ��ѯ�������塢���ߡ�ʱ�䷶Χ��ȡÿ�ղ���
        /// GET /api/WZ/ProductionOutput?valveCategory=ֱͨ��&productionLine=����1&start=2025-01-01&end=2025-12-31
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<WZ_ProductionOutput>>> Get(
            [FromQuery] string valveCategory,
            [FromQuery] string productionLine,
            [FromQuery(Name = "start")] DateTime startDate,
            [FromQuery(Name = "end")] DateTime endDate,
            CancellationToken ct = default)
        {
            var list = await _service.GetAsync(valveCategory, productionLine, startDate, endDate, ct);
            return Ok(list);
        }

        public sealed class DateRangeDto
        {
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
        }

        /// <summary>
        /// �ֶ�ˢ�£���ղ��ؽ����棨������Ա���ã�
        /// POST /api/WZ/ProductionOutput/refresh
        /// body: { "start":"2025-08-11", "end":"2025-08-12" }
        /// </summary>
        [HttpPost("refresh")]
        public async Task<ActionResult<object>> Refresh([FromBody] DateRangeDto dto, CancellationToken ct = default)
        {
            var count = await _service.RefreshAsync(dto.Start, dto.End, ct);
            return Ok(new { inserted = count, range = $"{dto.Start:yyyy-MM-dd}~{dto.End:yyyy-MM-dd}" });
        }
    }
}
