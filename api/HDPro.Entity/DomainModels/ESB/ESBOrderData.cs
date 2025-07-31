/*
 * ESB平台数据模型
 * 统一管理所有ESB接口相关的数据模型
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDPro.Entity.DomainModels.ESB
{
    /// <summary>
    /// ESB订单数据模型
    /// 对应ESB平台销售订单查询接口返回的数据结构
    /// </summary>
    public class ESBOrderData
    {
        /// <summary>
        /// 销售订单主键
        /// </summary>
        public long? FID { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public string FMODIFYDATE { get; set; }

        /// <summary>
        /// 单据状态
        /// </summary>
        public string FDOCUMENTSTATUS { get; set; }

        /// <summary>
        /// 合同签订日期
        /// </summary>
        public string F_BLN_CONTRACTSIGNDATE { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public string FCREATEDATE { get; set; }

        /// <summary>
        /// 销售合同号
        /// </summary>
        public string F_BLN_CONTACTNONAME { get; set; }

        /// <summary>
        /// 作废状态
        /// </summary>
        public string FCANCELSTATUS { get; set; }

        /// <summary>
        /// 用户合同号
        /// </summary>
        public string F_BLN_YHHTH { get; set; }

        /// <summary>
        /// 关闭状态
        /// </summary>
        public string FCLOSESTATUS { get; set; }

        /// <summary>
        /// 紧急等级
        /// </summary>
        public string FCUSTUN { get; set; }

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
        /// 使用单位
        /// </summary>
        public string F_BLN_USEUNIT1NAME { get; set; }

        /// <summary>
        /// 销售单据号
        /// </summary>
        public string FBILLNO { get; set; }

        /// <summary>
        /// 合同类型
        /// </summary>
        public string F_ORA_ASSISTANTNAME { get; set; }

        /// <summary>
        /// 销售员
        /// </summary>
        public string XSYNAME { get; set; }

        /// <summary>
        /// 客户
        /// </summary>
        public string FCUSTName { get; set; }

        /// <summary>
        /// 订单明细金额
        /// </summary>
        public object FAMOUNT { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string F_BLN_PROJECTNAME { get; set; }

        /// <summary>
        /// 业务终止
        /// </summary>
        public string FMRPTERMINATESTATUS { get; set; }

        /// <summary>
        /// 实际开工时间
        /// </summary>
        public string FSTARTDATE { get; set; }

        /// <summary>
        /// 业务冻结
        /// </summary>
        public string FMRPFREEZESTATUS { get; set; }

        /// <summary>
        /// 明细主键
        /// </summary>
        public long? FENTRYID { get; set; }

        /// <summary>
        /// 客户要货日期
        /// </summary>
        public string F_ORA_DATETIME { get; set; }

        /// <summary>
        /// 计划跟踪号订单状态
        /// </summary>
        public string FMTONOSTATUS { get; set; }

        /// <summary>
        /// 计划确认时间
        /// </summary>
        public string FPLANCONFIRMDATE { get; set; }

        /// <summary>
        /// 订单数量
        /// </summary>
        public object FQTY { get; set; }

        /// <summary>
        /// 订单审核时间
        /// </summary>
        public string XSDDAPPROVEDATE { get; set; }

        /// <summary>
        /// 计划跟踪号
        /// </summary>
        public string FMTONO { get; set; }

        /// <summary>
        /// 回复交货日期
        /// </summary>
        public string F_BLN_HFJHRQ { get; set; }

        /// <summary>
        /// BOM创建时间
        /// </summary>
        public string BOMCREATEDATE { get; set; }

        /// <summary>
        /// 计划开工时间
        /// </summary>
        public string FPLANSTARTDATE { get; set; }

        /// <summary>
        /// 完成数量（已入库数量）
        /// </summary>
        public object FINSTOCKFQTY { get; set; }

        /// <summary>
        /// 排产日期
        /// </summary>
        public string F_ORA_DATE1 { get; set; }

        /// <summary>
        /// 整机规格型号
        /// </summary>
        public string FSPECIFICATION { get; set; }

        /// <summary>
        /// 物料ID
        /// </summary>
        public long? FMATERIALID { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string FNUMBER { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        public string FNAME { get; set; }

        /// <summary>
        /// 关联总任务单据编号
        /// </summary>
        public string FZRWBILLNO { get; set; }

        /// <summary>
        /// 中标日期
        /// </summary>
        public string open_bid_date { get; set; }

        /// <summary>
        /// 最近入库日期
        /// </summary>
        public string FRKDATE { get; set; }

        /// <summary>
        /// 最近出库日期
        /// </summary>
        public string XSCKFDATE { get; set; }

        /// <summary>
        /// 出库数量
        /// </summary>
        public decimal? XSCKQTY { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public string FDDSTATUS { get; set; }

        /// <summary>
        /// 未完数量
        /// </summary>
        public decimal? FWWQTY { get; set; }

        /// <summary>
        /// 运算日期
        /// </summary>
        public string FDeliveryDate { get; set; }
    }
}