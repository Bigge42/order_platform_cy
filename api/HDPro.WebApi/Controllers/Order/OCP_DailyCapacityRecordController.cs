using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HDPro.CY.Order.IServices;
using HDPro.Core.Controllers.Basic;
using HDPro.Entity.AttributeManager;

namespace HDPro.CY.Order.Controllers
{
    [Route("api/OCP_DailyCapacityRecord")]
    [PermissionTable(Name = "OCP_DailyCapacityRecord")]
    public partial class OCP_DailyCapacityRecordController : ApiBaseController<IOCP_DailyCapacityRecordService>
    {
        private readonly IOCP_DailyCapacityRecordService _service;
        public OCP_DailyCapacityRecordController(IOCP_DailyCapacityRecordService service)
            : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// 手动同步指定日期范围的产能数据
        /// </summary>
        /// <param name="FSTARTDATE">起始日期(yyyy-MM-dd)</param>
        /// <param name="FENDDATE">结束日期(yyyy-MM-dd)</param>
        [HttpPost("Sync")]
        public async Task<IActionResult> SyncDailyCapacity([FromQuery] string FSTARTDATE, [FromQuery] string FENDDATE)
        {
            var result = await _service.SyncCapacityData(FSTARTDATE, FENDDATE);
            return Ok(result);
        }

        /// <summary>
        /// 获取产能记录列表（支持按年/产线/阀门大类筛选）
        /// </summary>
        /// <param name="year">年份，如2025</param>
        /// <param name="productionLine">生产线名称</param>
        /// <param name="valveCategory">阀门大类名称</param>
        [HttpGet("List")]
        public async Task<IActionResult> GetDailyCapacityList(int? year, string productionLine, string valveCategory)
        {
            var data = await _service.GetRecords(year, productionLine, valveCategory);
            return Ok(data);
        }
    }
}
