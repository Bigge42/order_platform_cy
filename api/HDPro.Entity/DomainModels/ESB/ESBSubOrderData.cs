using System;

namespace HDPro.Entity.DomainModels.ESB
{
    /// <summary>
    /// 委外订单头ESB数据实体 - 对应接口 SearchReqorder
    /// </summary>
    public class ESBSubOrderData
    {
        /// <summary>
        /// 采购订单主键
        /// 必填字段
        /// </summary>
        public int FID { get; set; }

        /// <summary>
        /// 采购订单据号
        /// 可选字段，最大长度50
        /// </summary>
        public string FBILLNO { get; set; }

        /// <summary>
        /// 催货单位任务时间月份
        /// 可选字段，最大长度20
        /// 注：由于一个采购订单可能会对应多个合同，进行了合并，用逗号分隔，目前为''后续会合并
        /// </summary>
        public string FCUSTUNMONTH { get; set; }

        /// <summary>
        /// 催货单位任务时间周
        /// 可选字段，最大长度20
        /// 注：由于一个采购订单可能会对应多个合同，进行了合并，用逗号分隔，目前为''后续会合并
        /// </summary>
        public string FCUSTUNWEEK { get; set; }

        /// <summary>
        /// 催货单位紧急等级
        /// 可选字段，最大长度20
        /// 注：由于一个采购订单可能会对应多个合同，进行了合并，用逗号分隔，目前为''后续会合并
        /// </summary>
        public string FCUSTUNEMER { get; set; }

        /// <summary>
        /// 业务类型
        /// 可选字段，最大长度20
        /// </summary>
        public string FBILLTYPENAME { get; set; }

        /// <summary>
        /// 供应商ID
        /// 可选字段
        /// </summary>
        public int? FSUPPLIERID { get; set; }

        /// <summary>
        /// 采购负责人
        /// 可选字段，最大长度50
        /// </summary>
        public string FCREATENAME { get; set; }
    }

    /// <summary>
    /// 委外订单明细ESB数据实体 - 对应接口 SearchPoorEntry
    /// </summary>
    public class ESBSubOrderDetailData
    {
        /// <summary>
        /// 采购订单明细主键
        /// 必填字段
        /// </summary>
        public int FENTRYID { get; set; }

        /// <summary>
        /// 采购订单主键
        /// 可选字段
        /// </summary>
        public int? FID { get; set; }

        /// <summary>
        /// 行号
        /// 可选字段
        /// </summary>
        public int? FSEQ { get; set; }

        /// <summary>
        /// 计划跟踪号
        /// 可选字段，最大长度255
        /// </summary>
        public string FMTONO { get; set; }

        /// <summary>
        /// 物料ID
        /// 可选字段
        /// </summary>
        public int? FMATERIALID { get; set; }

        /// <summary>
        /// 催货单位任务时间月份
        /// 可选字段，最大长度20
        /// 注：由于一个采购订单可能会对应多个合同，进行了合并，用逗号分隔，目前为''后续会合并
        /// </summary>
        public string FCUSTUNMONTH { get; set; }

        /// <summary>
        /// 催货单位任务时间周
        /// 可选字段，最大长度20
        /// 注：由于一个采购订单可能会对应多个合同，进行了合并，用逗号分隔，目前为''后续会合并
        /// </summary>
        public string FCUSTUNWEEK { get; set; }

        /// <summary>
        /// 催货单位紧急等级
        /// 可选字段，最大长度20
        /// 注：由于一个采购订单可能会对应多个合同，进行了合并，用逗号分隔，目前为''后续会合并
        /// </summary>
        public string FCUSTUNEMER { get; set; }

        /// <summary>
        /// 供应商ID
        /// 可选字段
        /// </summary>
        public int? FSUPPLIERID { get; set; }

        /// <summary>
        /// 送货单号
        /// 可选字段，最大长度1000
        /// 注：由于存在分批送货，一个采购订单明细可能会对应多个送货单，目前进行了合并，用逗号分隔
        /// </summary>
        public string F_BLN_DELIVERYNO { get; set; }

        /// <summary>
        /// 采购数量
        /// 可选字段，精度(23,10)
        /// </summary>
        public decimal? FQTY { get; set; }

        /// <summary>
        /// 要求交货日期
        /// 可选字段，最大长度120
        /// </summary>
        public string FDELIVERYDATE { get; set; }

        /// <summary>
        /// 回复交货日期
        /// 可选字段，最大长度120
        /// </summary>
        public string F_BLN_SPDATE { get; set; }

        /// <summary>
        /// 入库数量
        /// 可选字段，精度(23,10)
        /// </summary>
        public decimal? FREALQTY { get; set; }

        /// <summary>
        /// 未完数量
        /// 可选字段，精度(23,10)
        /// 注：采购数量-入库数量，目前有部分历史数据超额入库，会导致负数出现，后续调整
        /// </summary>
        public decimal? FWWQTY { get; set; }

        /// <summary>
        /// 委外订单单据号
        /// 可选字段，最大长度50
        /// </summary>
        public string FWWBILLNO { get; set; }

        /// <summary>
        /// 委外领料日期
        /// 可选字段，最大长度120
        /// </summary>
        public string F_BLN_LLRQ { get; set; }
    }

    /// <summary>
    /// 委外进度追踪ESB数据实体 - 对应接口 SearchPoorProgress
    /// 根据最新接口字段更新 - 2024年12月
    /// </summary>
    public class ESBSubOrderProgressData
    {
        /// <summary>
        /// 采购订单明细主键
        /// 必填字段
        /// </summary>
        public int FENTRYID { get; set; }

        /// <summary>
        /// 采购订单主键
        /// 可选字段
        /// </summary>
        public int? FID { get; set; }

        /// <summary>
        /// 计划跟踪号
        /// 可选字段，最大长度255
        /// </summary>
        public string FMTONO { get; set; }

        /// <summary>
        /// 采购单据号
        /// 可选字段，最大长度30
        /// </summary>
        public string FBILLNO { get; set; }

        /// <summary>
        /// 催货单位任务时间月份
        /// 可选字段，最大长度20
        /// 注：由于一个采购订单可能会对应多个合同，进行了合并，用逗号分隔，目前为''后续会合并
        /// </summary>
        public string FCUSTUNMONTH { get; set; }

        /// <summary>
        /// 催货单位任务时间周
        /// 可选字段，最大长度20
        /// 注：由于一个采购订单可能会对应多个合同，进行了合并，用逗号分隔，目前为''后续会合并
        /// </summary>
        public string FCUSTUNWEEK { get; set; }

        /// <summary>
        /// 催货单位紧急等级
        /// 可选字段，最大长度20
        /// 注：由于一个采购订单可能会对应多个合同，进行了合并，用逗号分隔，目前为''后续会合并
        /// </summary>
        public string FCUSTUNEMER { get; set; }

        /// <summary>
        /// 物料ID
        /// 可选字段
        /// </summary>
        public int? FMATERIALID { get; set; }

        /// <summary>
        /// 供应商ID
        /// 可选字段
        /// </summary>
        public int? FSUPPLIERID { get; set; }

        /// <summary>
        /// 委外订单号
        /// 可选字段，最大长度50
        /// </summary>
        public string FWWBILLNO { get; set; }

        /// <summary>
        /// 委外领料日期
        /// 可选字段，最大长度120
        /// </summary>
        public string F_BLN_LLRQ { get; set; }

        /// <summary>
        /// 委外订单创建时间
        /// 可选字段，最大长度120
        /// </summary>
        public string FWWCREATEDATE { get; set; }

        /// <summary>
        /// 委外订单审核时间
        /// 可选字段，最大长度120
        /// </summary>
        public string FWWAPPROVEDATE { get; set; }

        /// <summary>
        /// 采购订单创建时间
        /// 可选字段，最大长度120
        /// </summary>
        public string FCGDDCREATEDATE { get; set; }

        /// <summary>
        /// 采购订单审核时间
        /// 可选字段，最大长度120
        /// </summary>
        public string FCGDDAPPROVEDATE { get; set; }

        /// <summary>
        /// 要求交货日期
        /// 可选字段，最大长度120
        /// </summary>
        public string FDELIVERYDATE { get; set; }

        /// <summary>
        /// 回复交货日期
        /// 可选字段，最大长度120
        /// </summary>
        public string F_BLN_SPDATE { get; set; }

        /// <summary>
        /// 最近送货日期
        /// 可选字段，最大长度120
        /// </summary>
        public string prt_dt { get; set; }

        /// <summary>
        /// 累计送货数量
        /// 可选字段，精度(23,10)
        /// </summary>
        public decimal? qty_send { get; set; }

        /// <summary>
        /// 最近到货日期
        /// 可选字段，最大长度120
        /// </summary>
        public string rcv_dt { get; set; }

        /// <summary>
        /// 累计到货数量
        /// 可选字段，精度(23,10)
        /// </summary>
        public decimal? qty_rcvd { get; set; }

        /// <summary>
        /// 最近质检日期
        /// 可选字段，最大长度120
        /// </summary>
        public string chk_dt { get; set; }

        /// <summary>
        /// 累计合格数
        /// 可选字段，精度(23,10)
        /// </summary>
        public decimal? qty_ok { get; set; }

        /// <summary>
        /// 最近前处理日期
        /// 可选字段，最大长度120
        /// </summary>
        public string stock_dt { get; set; }

        /// <summary>
        /// 累计入库数量
        /// 可选字段，精度(23,10)
        /// </summary>
        public decimal? FREALQTY { get; set; }

        /// <summary>
        /// 最近入库日期
        /// 可选字段，最大长度120
        /// </summary>
        public string FRKAPPROVEDATE { get; set; }

        /// <summary>
        /// 行号
        /// 可选字段
        /// </summary>
        public int? FSEQ { get; set; }

        /// <summary>
        /// 送货单号
        /// 可选字段，最大长度1000
        /// </summary>
        public string F_BLN_DELIVERYNO { get; set; }

        /// <summary>
        /// 采购数量
        /// 可选字段，精度(23,10)
        /// </summary>
        public decimal? FQTY { get; set; }

        /// <summary>
        /// 订单状态
        /// 可选字段，最大长度20
        /// 值：正常，关闭(头和行，作废，终止)，冻结
        /// </summary>
        public string FCGSTATUS { get; set; }

        /// <summary>
        /// 委外订单下达日期
        /// 可选字段，最大长度120
        /// </summary>
        public string FCONVEYDATE { get; set; }

        /// <summary>
        /// 备注
        /// 可选字段，最大长度1000
        /// </summary>
        public string F_BLN_BZ { get; set; }
    }
} 