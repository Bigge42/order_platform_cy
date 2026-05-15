using HDPro.Core.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HDPro.CY.Order.IServices
{
    public partial interface ISys_AIAppService
    {
        Task<WebResponseContent> GetPlatformAppsAsync(string keyword = null);

        Task<WebResponseContent> SyncFromPlatformAsync(List<string> platformAppIds);

        Task<WebResponseContent> CreatePlatformAppKeyAsync(string platformAppId);
    }
}
