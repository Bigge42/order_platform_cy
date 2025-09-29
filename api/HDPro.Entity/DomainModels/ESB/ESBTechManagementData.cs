using System;

namespace HDPro.Entity.DomainModels.ESB
{
    /// <summary>
    /// 技术管理ESB数据模型
    /// </summary>
    public class ESBTechManagementData
    {
        /// <summary>
        /// 销售订单明细主键
        /// </summary>
        public long FENTRYID { get; set; }

        /// <summary>
        /// 销售订单号
        /// </summary>
        public string FBILLNO { get; set; }

        /// <summary>
        /// 销售合同号
        /// </summary>
        public string F_BLN_CONTACTNONAME { get; set; }

        /// <summary>
        /// 计划跟踪号
        /// </summary>
        public string FMTONO { get; set; }

        /// <summary>
        /// 物料ID
        /// </summary>
        public long? FMATERIALID { get; set; }

        /// <summary>
        /// 订单冻结状态
        /// </summary>
        public string FMRPFREEZESTATUSNAME { get; set; }

        /// <summary>
        /// 订单终止状态
        /// </summary>
        public string FMRPTERMINATESTATUSNAME { get; set; }

        /// <summary>
        /// 订单关闭状态
        /// </summary>
        public string FMRPCLOSESTATUSNAME { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public string FDDSTATUS { get; set; }

        /// <summary>
        /// 数量（销售数量）
        /// </summary>
        public decimal? FQTY { get; set; }

        /// <summary>
        /// 催货单位任务时间月份
        /// </summary>
        public string FCUSTUNMONTH { get; set; }

        /// <summary>
        /// 催货单位任务时间周
        /// </summary>
        public string FCUSTUNWEEK { get; set; }

        /// <summary>
        /// 催货单位紧急等级
        /// </summary>
        public string FCUSTUNEMER { get; set; }

        /// <summary>
        /// BOM创建时间
        /// </summary>
        public string BOMCREATEDATE { get; set; }

        /// <summary>
        /// 是否有BOM
        /// </summary>
        public string FISBOM { get; set; }

        /// <summary>
        /// 计划要求完工日期
        /// </summary>
        public string FPLANFINISHDATE { get; set; }

        /// <summary>
        /// 销售订单回复交货日期
        /// </summary>
        public string F_BLN_HFJHRQ { get; set; }

        /// <summary>
        /// 关联总任务单据号
        /// </summary>
        public string FZRWBILLNO { get; set; }

        /// <summary>
        /// 标准天数
        /// </summary>
        public decimal? FBZTS { get; set; }

        /// <summary>
        /// 客户要货日期
        /// </summary>
        public string F_ORA_DATETIME { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string F_BLN_BZ { get; set; }

        /// <summary>
        /// 修改日期
        /// </summary>
        public string FMODIFYDATE { get; set; }
        /// <summary>
        /// 评审意见
        /// </summary>
        public string FPSYJ { get; set; }
    }
}