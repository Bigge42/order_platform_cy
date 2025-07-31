/*
 * ESB平台缺料计算结果数据模型
 * 对应ESB平台缺料计算结果查询接口返回的数据结构
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
    /// ESB缺料计算结果数据模型
    /// 对应ESB平台缺料计算结果查询接口返回的数据结构
    /// </summary>
    public class ESBLackMtrlData
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 销售合同号
        /// </summary>
        public string F_BLN_CONTACTNO { get; set; }

        /// <summary>
        /// 销售单据号
        /// </summary>
        public string FSALBILLNO { get; set; }

        /// <summary>
        /// 计划跟踪号
        /// </summary>
        public string FMOMTONO { get; set; }

        /// <summary>
        /// 整机物料ID（关联物料表取整机信息）
        /// </summary>
        public int? FZJMATERIALID { get; set; }

        /// <summary>
        /// 催货单位
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
        /// 物料ID（关联物料表取缺料物料信息）
        /// </summary>
        public int? FMATERIALID { get; set; }

        /// <summary>
        /// 需求数量
        /// </summary>
        public decimal? FNEEDQTY { get; set; }

        /// <summary>
        /// 库存数量
        /// </summary>
        public decimal? Finvqty { get; set; }

        /// <summary>
        /// 已下计划（在途\在制）
        /// </summary>
        public decimal? ZTZZqty { get; set; }

        /// <summary>
        /// 未下计划数量
        /// </summary>
        public decimal? FWXJHQTY { get; set; }

        /// <summary>
        /// 单据编号（在途在制的对应单据号：采购单据号、委外单据号、生产订单号）
        /// </summary>
        public string FBILLNO { get; set; }

        /// <summary>
        /// 采购数量
        /// </summary>
        public decimal? FQTY { get; set; }

        /// <summary>
        /// 供应商
        /// </summary>
        public string FWORKSHOPNAME { get; set; }

        /// <summary>
        /// 负责人
        /// </summary>
        public string FPURCHASERNAME { get; set; }

        /// <summary>
        /// 计划开始日期
        /// </summary>
        public string FJHSTRATDATE { get; set; }

        /// <summary>
        /// 计划完成日期
        /// </summary>
        public string FJHENDDATE { get; set; }

        /// <summary>
        /// 采购申请审核日期
        /// </summary>
        public object FCGSQDATE { get; set; }

        /// <summary>
        /// 委外订单下达日期
        /// </summary>
        public string FWWXDDATE { get; set; }

        /// <summary>
        /// 领料日期
        /// </summary>
        public string F_BLN_LLRQ { get; set; }

        /// <summary>
        /// 生产实际开工日期
        /// </summary>
        public string FSCSTARTDATE { get; set; }

        /// <summary>
        /// 生产订单创建日期
        /// </summary>
        public string FSCCREATEDATE { get; set; }

        /// <summary>
        /// 生产计划完工日期
        /// </summary>
        public string FSCPLANFINISHDATE { get; set; }

        /// <summary>
        /// 采购订单创建日期
        /// </summary>
        public string FCGCREATEDATE { get; set; }

        /// <summary>
        /// 采购订单审核日期
        /// </summary>
        public string FCGAPPROVEDATE { get; set; }

        /// <summary>
        /// 计划交货日期
        /// </summary>
        public string FJHJHRQ { get; set; }

        /// <summary>
        /// 运算ID
        /// </summary>
        public int? ComputeID { get; set; }

        /// <summary>
        /// 单据分类（分为标准采购、标准委外、金工车间、其他）
        /// </summary>
        public string FBILLNOCLASS { get; set; }

        /// <summary>
        /// 排产年月（关联总任务单据号）
        /// </summary>
        public string FZRWBILLNO { get; set; }

    }
} 