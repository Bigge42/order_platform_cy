/*
 * K3Cloud客户数据模型
 */
namespace HDPro.CY.Order.Services.K3Cloud.Models
{
    /// <summary>
    /// K3Cloud客户数据模型
    /// </summary>
    public class K3CloudCustomerData
    {
        /// <summary>
        /// 客户编号 - FNumber
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// 客户名称 - FName
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 客户ID - FCustomerId
        /// </summary>
        public long? FCustomerId { get; set; }

        /// <summary>
        /// 数据状态 - FDocumentStatus
        /// </summary>
        public string FDocumentStatus { get; set; }

        /// <summary>
        /// 禁用状态 - FForbidStatus
        /// </summary>
        public string FForbidStatus { get; set; }

        /// <summary>
        /// 创建人 - FCreatorId.FName
        /// </summary>
        public string FCreatorName { get; set; }

        /// <summary>
        /// 修改人 - FModifierId.FName
        /// </summary>
        public string FModifierName { get; set; }

        /// <summary>
        /// 创建日期 - FCreateDate
        /// </summary>
        public string FCreateDate { get; set; }

        /// <summary>
        /// 修改日期 - FModifyDate
        /// </summary>
        public string FModifyDate { get; set; }

        /// <summary>
        /// 销售员 - FSalesman.FName
        /// </summary>
        public string FSalesmanName { get; set; }

        /// <summary>
        /// 通讯地址 - FBaseInfo.FAddress
        /// </summary>
        public string FAddress { get; set; }

        /// <summary>
        /// 统一社会信用代码 - FBaseInfo.FSOCIALCRECODE
        /// </summary>
        public string FSOCIALCRECODE { get; set; }

        /// <summary>
        /// 联系人电话 - FLocationInfo.FLocTel
        /// </summary>
        public string FLocTel { get; set; }

        /// <summary>
        /// 联系人手机 - FLocationInfo.FLocMobile
        /// </summary>
        public string FLocMobile { get; set; }

        /// <summary>
        /// 联系人 - FCustomerContact.FContact
        /// </summary>
        public string FContact { get; set; }

        /// <summary>
        /// 联系人电话 - FCustomerContact.FTel
        /// </summary>
        public string FContactTel { get; set; }

        /// <summary>
        /// 联系人手机 - FCustomerContact.FMobile
        /// </summary>
        public string FContactMobile { get; set; }
    }
} 