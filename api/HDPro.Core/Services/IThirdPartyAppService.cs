using System.Threading.Tasks;
using HDPro.Entity.DomainModels;

namespace HDPro.Core.Services
{
    /// <summary>
    /// 第三方应用服务接口
    /// </summary>
    public interface IThirdPartyAppService
    {
        /// <summary>
        /// 验证AppId和AppSecret
        /// </summary>
        /// <param name="appId">应用ID</param>
        /// <param name="appSecret">应用密钥</param>
        /// <returns>第三方应用信息，验证失败返回null</returns>
        Task<Sys_ThirdPartyApp> ValidateAppAsync(string appId, string appSecret);

        /// <summary>
        /// 检查应用权限
        /// </summary>
        /// <param name="appId">应用ID</param>
        /// <param name="permission">权限标识</param>
        /// <returns>是否有权限</returns>
        Task<bool> CheckPermissionAsync(string appId, string permission);

        /// <summary>
        /// 增加访问次数
        /// </summary>
        /// <param name="appId">应用ID</param>
        /// <returns></returns>
        Task IncrementAccessCountAsync(string appId);

        /// <summary>
        /// 生成AppId
        /// </summary>
        /// <param name="prefix">前缀，如"CY1jOrd"</param>
        /// <returns>生成的AppId</returns>
        string GenerateAppId(string prefix = "CY1jOrd");

        /// <summary>
        /// 生成AppSecret
        /// </summary>
        /// <param name="length">长度，默认32位</param>
        /// <returns>生成的AppSecret</returns>
        string GenerateAppSecret(int length = 32);

        /// <summary>
        /// 验证AppSecret
        /// </summary>
        /// <param name="inputSecret">输入的密钥</param>
        /// <param name="storedSecret">存储的密钥</param>
        /// <returns>是否匹配</returns>
        bool VerifyAppSecret(string inputSecret, string storedSecret);
    }
} 