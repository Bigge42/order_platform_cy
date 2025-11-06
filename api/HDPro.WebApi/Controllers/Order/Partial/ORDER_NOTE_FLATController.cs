using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.Entity.DomainModels;
using HDPro.CY.Order.IServices;
using HDPro.Core.Utilities;
using Microsoft.AspNetCore.Authorization;

namespace HDPro.CY.Order.Controllers
{
    public partial class ORDER_NOTE_FLATController
    {
        private readonly IORDER_NOTE_FLATService _service;
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public ORDER_NOTE_FLATController(IORDER_NOTE_FLATService service, IHttpContextAccessor httpContextAccessor)
            : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 调用 ERP 接口，同步销售订单备注到本地
        /// </summary>
        [HttpPost("syncErpNotes")]
        [AllowAnonymous]
        public async Task<WebResponseContent> SyncErpNotes()
        {
            // 调用服务层方法执行同步
            return await _service.SyncErpNotes();
        }

        /// <summary>
        /// 提交备注拆分结果，更新指定字段并重置变更标记
        /// </summary>
        [HttpPost("updateNoteSplit")]
        [AllowAnonymous]
        public WebResponseContent UpdateNoteSplit([FromBody] NoteSplitUpdateModel model)
        {
            // 调用服务层方法更新拆分后的备注
            return _service.UpdateNoteSplit(
                model.source_type,
                model.source_entry_id,
                model.note_body_actuator,
                model.note_accessory_debug,
                model.note_pressure_leak,
                model.note_packing
            );
        }

        /// <summary>
        /// 获取内部备注和原始备注内容
        /// </summary>
        [HttpGet("{sourceType}/{sourceEntryId}/note")]
        [AllowAnonymous]
        public WebResponseContent GetNote(string sourceType, long sourceEntryId)
        {
            // 调用服务层方法获取备注字段
            return _service.GetNote(sourceType, sourceEntryId);
        }
    }

    /// <summary>
    /// 更新备注拆分接口的请求模型
    /// </summary>
    public class NoteSplitUpdateModel
    {
        public string source_type { get; set; }
        public long source_entry_id { get; set; }
        public string note_body_actuator { get; set; }
        public string note_accessory_debug { get; set; }
        public string note_pressure_leak { get; set; }
        public string note_packing { get; set; }
    }
}
