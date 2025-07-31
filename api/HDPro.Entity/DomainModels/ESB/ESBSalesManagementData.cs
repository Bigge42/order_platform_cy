/*
 * 销售管理ESB数据模型
 * 对应销售管理相关的ESB接口数据结构
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HDPro.Entity.DomainModels.ESB
{
    #region 查询销售表当前批次信息接口数据模型

    /// <summary>
    /// ESB查询销售表当前批次信息请求参数
    /// 对应接口：SearchMOMProcedure
    /// </summary>
    public class ESBSalesBatchInfoRequest
    {
        /// <summary>
        /// 计划跟踪号
        /// </summary>
        [Required]
        public string ERPMTONO { get; set; }
    }

    /// <summary>
    /// ESB查询销售表当前批次信息响应数据
    /// 对应接口：SearchMOMProcedure
    /// </summary>
    public class ESBSalesBatchInfoData
    {
        /// <summary>
        /// 计划跟踪号
        /// </summary>
        public string ERPMTONO { get; set; }

        /// <summary>
        /// 生产订单号
        /// </summary>
        public string FMOBILLNO { get; set; }

        /// <summary>
        /// 批次号
        /// </summary>
        public string Retrospect_Code { get; set; }

        /// <summary>
        /// 当前工序名称
        /// </summary>
        public string OperationName { get; set; }

        /// <summary>
        /// 位号
        /// </summary>
        public string Position_Num { get; set; }

        /// <summary>
        /// 当前工序执行状态
        /// </summary>
        public string ProcessStateName { get; set; }
    }

    #endregion

    #region 查询销售表主表销售订单列表接口数据模型

    /// <summary>
    /// ESB查询销售订单列表请求参数
    /// 对应接口：SearchERPSalOrderList
    /// </summary>
    public class ESBSalesOrderListRequest
    {
        /// <summary>
        /// 时间范围开始时间
        /// </summary>
        [Required]
        public string FSTARTDATE { get; set; }

        /// <summary>
        /// 时间范围结束时间
        /// </summary>
        [Required]
        public string FENDDATE { get; set; }
    }

    /// <summary>
    /// ESB查询销售订单列表响应数据
    /// 对应接口：SearchERPSalOrderList
    /// </summary>
    public class ESBSalesOrderListData
    {
        /// <summary>
        /// 销售订单主键
        /// </summary>
        [Required]
        public int FID { get; set; }

        /// <summary>
        /// 订单录入日期
        /// </summary>
        public string FDATE { get; set; }

        /// <summary>
        /// 销售订单号
        /// </summary>
        public string FSALBILLNO { get; set; }

        /// <summary>
        /// 销售合同号
        /// </summary>
        public string F_BLN_CONTACTNONAME { get; set; }

        /// <summary>
        /// 用户合同号
        /// </summary>
        public string F_BLN_YHHTH { get; set; }

        /// <summary>
        /// 客户
        /// </summary>
        public string FCUSTName { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public string FSTATE { get; set; }

        /// <summary>
        /// 合同类型
        /// </summary>
        public string F_ORA_ASSISTANTNAME { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string F_ORA_XMMC { get; set; }

        /// <summary>
        /// 销售员
        /// </summary>
        public string XSYNAME { get; set; }
    }

    #endregion

    #region 查询销售表子表销售订单明细接口数据模型

    /// <summary>
    /// ESB查询销售订单明细请求参数
    /// 对应接口：SearchERPSalOrderEntry
    /// </summary>
    public class ESBSalesOrderDetailRequest
    {
        /// <summary>
        /// 销售订单号
        /// </summary>
        [Required]
        public string FSALBILLNO { get; set; }
    }

    /// <summary>
    /// ESB查询销售订单明细响应数据
    /// 对应接口：SearchERPSalOrderEntry
    /// </summary>
    public class ESBSalesOrderDetailData
    {
        /// <summary>
        /// 销售订单明细主键
        /// </summary>
        [Required]
        public int FENTRYID { get; set; }

        /// <summary>
        /// 销售订单号
        /// </summary>
        public string FSALBILLNO { get; set; }

        /// <summary>
        /// 计划跟踪号
        /// </summary>
        public string FMTONO { get; set; }

        /// <summary>
        /// 生产订单号
        /// </summary>
        public string FMOBILLNO { get; set; }

        /// <summary>
        /// 生产订单状态
        /// </summary>
        public string FWORKSTATE { get; set; }

        /// <summary>
        /// 订货数量
        /// </summary>
        public decimal? FQTY { get; set; }

        /// <summary>
        /// 位号
        /// </summary>
        public string F_BLN_WH { get; set; }

        /// <summary>
        /// 物料ID
        /// </summary>
        public int? FMATERIALID { get; set; }

        /// <summary>
        /// 入库数量
        /// </summary>
        public decimal? FRKREALQTY { get; set; }

        /// <summary>
        /// 待入库数量
        /// </summary>
        public decimal? FSQTY { get; set; }

        /// <summary>
        /// 出库数量
        /// </summary>
        public decimal? FCKREALQTY { get; set; }

        /// <summary>
        /// 销售合同号
        /// </summary>
        public string F_BLN_CONTACTNONAME { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string F_ORA_XMMC { get; set; }

        /// <summary>
        /// 回复交货日期
        /// </summary>
        public string F_BLN_HFJHRQ { get; set; }

        /// <summary>
        /// 客户要货日期
        /// </summary>
        public string F_ORA_DATETIME { get; set; }

        /// <summary>
        /// 排产日期
        /// </summary>
        public string F_ORA_DATE1 { get; set; }

        /// <summary>
        /// 价税合计
        /// </summary>
        public decimal? FALLAMOUNT { get; set; }

        /// <summary>
        /// 目录价税合计
        /// </summary>
        public decimal? F_BLN_ZQJSHJ { get; set; }
    }

    #endregion
} 