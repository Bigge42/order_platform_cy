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
        /// �ֶ�ͬ��ָ�����ڷ�Χ�Ĳ�������
        /// </summary>
        /// <param name="FSTARTDATE">��ʼ����(yyyy-MM-dd)</param>
        /// <param name="FENDDATE">��������(yyyy-MM-dd)</param>
        [HttpPost("Sync")]
        public async Task<IActionResult> SyncDailyCapacity([FromQuery] string FSTARTDATE, [FromQuery] string FENDDATE)
        {
            var result = await _service.SyncCapacityData(FSTARTDATE, FENDDATE);
            return Ok(result);
        }

        /// <summary>
        /// ��ȡ���ܼ�¼�б�֧�ְ���/����/���Ŵ���ɸѡ��
        /// </summary>
        /// <param name="year">��ݣ���2025</param>
        /// <param name="productionLine">����������</param>
        /// <param name="valveCategory">���Ŵ�������</param>
        [HttpGet("List")]
        public async Task<IActionResult> GetDailyCapacityList(int? year, string productionLine, string valveCategory)
        {
            var data = await _service.GetRecords(year, productionLine, valveCategory);
            return Ok(data);
        }
    }
}
