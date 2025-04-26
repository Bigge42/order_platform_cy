using HDPro.Core.BaseProvider;
using HDPro.Core.Utilities;
using HDPro.Entity.DomainModels;
using System.Threading.Tasks;

namespace HDPro.Sys.IServices
{
    public partial interface ISys_UserService
    {

        Task<WebResponseContent> Login(LoginInfo loginInfo, bool verificationCode = true);
        Task<WebResponseContent> ReplaceToken();
        Task<WebResponseContent> ModifyPwd(string oldPwd, string newPwd);
        Task<WebResponseContent> GetCurrentUserInfo();
    }
}

