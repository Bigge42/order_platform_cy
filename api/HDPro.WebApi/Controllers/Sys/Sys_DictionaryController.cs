using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HDPro.Core.Controllers.Basic;
using HDPro.Core.Extensions;
using HDPro.Core.Filters;
using HDPro.Sys.IServices;

namespace HDPro.Sys.Controllers
{
    [Route("api/Sys_Dictionary")]
    public partial class Sys_DictionaryController : ApiBaseController<ISys_DictionaryService>
    {
        public Sys_DictionaryController(ISys_DictionaryService service)
        : base("System", "System", "Sys_Dictionary", service)
        {
        }
    }
}
