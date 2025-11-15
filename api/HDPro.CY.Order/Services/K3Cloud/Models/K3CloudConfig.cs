/*
 * K3Cloud配置模型
 */
namespace HDPro.CY.Order.Services.K3Cloud.Models
{
    /// <summary>
    /// K3Cloud配置模型
    /// </summary>
    public class K3CloudConfig
    {
        /// <summary>
        /// K3Cloud服务器地址
        /// </summary>
        public string ServerUrl { get; set; }

        /// <summary>
        /// 账套ID
        /// </summary>
        public string AcctId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// 应用ID
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// 应用密钥
        /// </summary>
        public string AppSecret { get; set; }

        /// <summary>
        /// 语言ID，默认2052（中文）
        /// </summary>
        public int Lcid { get; set; } = 2052;

        /// <summary>
        /// 登录接口地址
        /// </summary>
        public string LoginUrl => $"{ServerUrl}/k3cloud/Kingdee.BOS.WebApi.ServicesStub.AuthService.LoginByAppSecret.common.kdsvc";

        /// <summary>
        /// 查询接口地址
        /// </summary>
        public string QueryUrl => $"{ServerUrl}/k3cloud/Haodee.K3.WebApi.ServicesStub.DynamicFormService.ExecuteBillQuery.common.kdsvc";

        /// <summary>
        /// BOM展开接口地址
        /// </summary>
        public string BomExpandUrl => $"{ServerUrl}/k3cloud/Haodee.K3.WebApi.ServicesStub.BomExpandService.Expand.common.kdsvc";
    }
}