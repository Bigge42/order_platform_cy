using HDPro.CY.Order.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace HDPro.CY.Order.Controllers
{
    public partial class Sys_AIAppController
    {
        private readonly ISys_AIAppService _service;
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public Sys_AIAppController(
            ISys_AIAppService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }
    }
}
