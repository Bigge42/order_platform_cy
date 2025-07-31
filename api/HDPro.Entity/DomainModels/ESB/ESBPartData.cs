using System;

namespace HDPro.Entity.DomainModels.ESB
{
    /// <summary>
    /// 部件未完跟踪ESB数据实体 - 对应接口 SearchPartProgress
    /// </summary>
    public class ESBPartProgressData
    {
        /// <summary>
        /// 主键 - 对应实体 ID
        /// 必填字段，最大长度100
        /// 主键，由生产订单明细主键+MES工单ID组成，中间由'+'连接
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 生产订单明细主键 - 对应实体 FENTRYID
        /// 必填字段
        /// </summary>
        public long FENTRYID { get; set; }

        /// <summary>
        /// 计划跟踪号 - 对应实体 FMTONO
        /// 可选字段，最大长度255
        /// </summary>
        public string FMTONO { get; set; }

        /// <summary>
        /// 生产订单号 - 对应实体 FBILLNO
        /// 可选字段，最大长度50
        /// </summary>
        public string FBILLNO { get; set; }

        /// <summary>
        /// MES工单号 - 对应实体 WorkOrder_Code
        /// 可选字段，最大长度50
        /// </summary>
        public string WorkOrder_Code { get; set; }

        /// <summary>
        /// 物料ID - 对应实体 FMATERIALID
        /// 可选字段
        /// </summary>
        public long? FMATERIALID { get; set; }

        /// <summary>
        /// 催货单位任务时间月份 - 对应实体 FCUSTUNMONTH
        /// 可选字段，最大长度20
        /// </summary>
        public string FCUSTUNMONTH { get; set; }

        /// <summary>
        /// 催货单位任务时间周 - 对应实体 FCUSTUNWEEK
        /// 可选字段，最大长度20
        /// </summary>
        public string FCUSTUNWEEK { get; set; }

        /// <summary>
        /// 催货单位紧急等级 - 对应实体 FCUSTUNEMER
        /// 可选字段，最大长度20
        /// </summary>
        public string FCUSTUNEMER { get; set; }

        /// <summary>
        /// 生产订单实际开工时间 - 对应实体 FSTARTDATE
        /// 可选字段，最大长度120
        /// </summary>
        public string FSTARTDATE { get; set; }

        /// <summary>
        /// MES当前工序 - 对应实体 FMESCurrentProcedure
        /// 可选字段，最大长度50
        /// </summary>
        public string FMESCurrentProcedure { get; set; }

        /// <summary>
        /// MES领料时间 - 对应实体 LLTIME
        /// 可选字段，最大长度120
        /// </summary>
        public string LLTIME { get; set; }

        /// <summary>
        /// MES预装时间 - 对应实体 YZTIME
        /// 可选字段，最大长度120
        /// </summary>
        public string YZTIME { get; set; }

        /// <summary>
        /// MES阀体部件装配时间 - 对应实体 FTBJZPTIME
        /// 可选字段，最大长度120
        /// </summary>
        public string FTBJZPTIME { get; set; }

        /// <summary>
        /// MES强压泄漏试验时间 - 对应实体 QYXLTIME
        /// 可选字段，最大长度120
        /// </summary>
        public string QYXLTIME { get; set; }

        /// <summary>
        /// MES检验时间 - 对应实体 JYTIME
        /// 可选字段，最大长度120
        /// </summary>
        public string JYTIME { get; set; }

        /// <summary>
        /// 生产数量 - 对应实体 FQTY
        /// 可选字段，精度(23,10)
        /// </summary>
        public decimal? FQTY { get; set; }

        /// <summary>
        /// 入库数量 - 对应实体 FSTOCKINQUAAUXQTY
        /// 可选字段，精度(23,10)
        /// </summary>
        public decimal? FSTOCKINQUAAUXQTY { get; set; }

        /// <summary>
        /// MES批次号 - 对应实体 Retrospect_Code
        /// 可选字段，最大长度200
        /// </summary>
        public string Retrospect_Code { get; set; }

        /// <summary>
        /// 排产年月/关联总任务单据编号
        /// 可选字段，最大长度120
        /// </summary>
        public string FZRWBILLNO { get; set; }

        /// <summary>
        /// 生产订单状态
        /// 可选字段，最大长度30
        /// </summary>
        public string FSTATUSNAME { get; set; }

        /// <summary>
        /// 执行状态（异常，准备中，执行中，已完成）
        /// 可选字段，最大长度30
        /// </summary>
        public string ExecuteStateNAME { get; set; }

        /// <summary>
        /// 计划完工时间
        /// 可选字段，最大长度120
        /// </summary>
        public string FSCPLANFINISHDATE { get; set; }

        /// <summary>
        /// 实际完工时间
        /// 可选字段，最大长度120
        /// </summary>
        public string FSCFINISHDATE { get; set; }
    }

    /// <summary>
    /// 部件生产订单头ESB数据实体 - 对应接口 SearchPrdMO
    /// </summary>
    public class ESBPartPrdMOData
    {
        /// <summary>
        /// 生产订单主键 - 对应实体 FID
        /// 必填字段
        /// </summary>
        public long FID { get; set; }

        /// <summary>
        /// 生产订单号 - 对应实体 FBILLNO
        /// 可选字段，最大长度50
        /// </summary>
        public string FBILLNO { get; set; }

        /// <summary>
        /// 催货单位任务时间月份 - 对应实体 FCUSTUNMONTH
        /// 可选字段，最大长度20
        /// </summary>
        public string FCUSTUNMONTH { get; set; }

        /// <summary>
        /// 催货单位任务时间周 - 对应实体 FCUSTUNWEEK
        /// 可选字段，最大长度20
        /// </summary>
        public string FCUSTUNWEEK { get; set; }

        /// <summary>
        /// 催货单位紧急等级 - 对应实体 FCUSTUNEMER
        /// 可选字段，最大长度20
        /// </summary>
        public string FCUSTUNEMER { get; set; }

        /// <summary>
        /// 生产订单审核时间 - 对应实体 FAPPROVEDATE
        /// 可选字段，最大长度120
        /// </summary>
        public string FAPPROVEDATE { get; set; }

        /// <summary>
        /// 生产订单单据类型 - 对应实体 FBILLTYPENAME
        /// 可选字段，最大长度50
        /// </summary>
        public string FBILLTYPENAME { get; set; }


    }

    /// <summary>
    /// 部件生产订单明细ESB数据实体 - 对应接口 SearchPrdMOEntry
    /// </summary>
    public class ESBPartPrdMODetailData
    {
        /// <summary>
        /// 生产订单明细主键 - 对应实体 FENTRYID
        /// 必填字段
        /// </summary>
        public long FENTRYID { get; set; }

        /// <summary>
        /// 生产订单主键 - 对应实体 FID
        /// 可选字段
        /// </summary>
        public long? FID { get; set; }

        /// <summary>
        /// 行号 - 对应实体 FSEQ
        /// 可选字段
        /// </summary>
        public int? FSEQ { get; set; }

        /// <summary>
        /// 生产订单状态 - 对应实体 FSTATUSNAME
        /// 可选字段，最大长度30
        /// </summary>
        public string FSTATUSNAME { get; set; }

        /// <summary>
        /// 计划跟踪号 - 对应实体 FMTONO
        /// 可选字段，最大长度255
        /// </summary>
        public string FMTONO { get; set; }

        /// <summary>
        /// 物料ID - 对应实体 FMATERIALID
        /// 可选字段
        /// </summary>
        public long? FMATERIALID { get; set; }

        /// <summary>
        /// 催货单位任务时间月份 - 对应实体 FCUSTUNMONTH
        /// 可选字段，最大长度20
        /// 注：由于总任务暂时没有录入计划跟踪号，而且销售订单号，因为目前该字段设置为空字符串
        /// </summary>
        public string FCUSTUNMONTH { get; set; }

        /// <summary>
        /// 催货单位任务时间周 - 对应实体 FCUSTUNWEEK
        /// 可选字段，最大长度20
        /// 注：由于总任务暂时没有录入计划跟踪号，而且销售订单号，因为目前该字段设置为空字符串
        /// </summary>
        public string FCUSTUNWEEK { get; set; }

        /// <summary>
        /// 催货单位紧急等级 - 对应实体 FCUSTUNEMER
        /// 可选字段，最大长度20
        /// 注：由于总任务暂时没有录入计划跟踪号，而且销售订单号，因为目前该字段设置为空字符串
        /// </summary>
        public string FCUSTUNEMER { get; set; }

        /// <summary>
        /// 计划完工时间 - 对应实体 FPLANFINISHDATE
        /// 可选字段，最大长度120
        /// </summary>
        public string FPLANFINISHDATE { get; set; }

        /// <summary>
        /// 实际完工时间 - 对应实体 FFINISHDATE
        /// 可选字段，最大长度120
        /// </summary>
        public string FFINISHDATE { get; set; }

        /// <summary>
        /// 领料日期 - 对应实体 F_BLN_LLRQ
        /// 可选字段，最大长度120
        /// </summary>
        public string F_BLN_LLRQ { get; set; }

        /// <summary>
        /// 生产数量 - 对应实体 FQTY
        /// 可选字段，精度(23,10)
        /// </summary>
        public decimal? FQTY { get; set; }

        /// <summary>
        /// 入库数量 - 对应实体 FSTOCKINQUAAUXQTY
        /// 可选字段，精度(23,10)
        /// </summary>
        public decimal? FSTOCKINQUAAUXQTY { get; set; }

        /// <summary>
        /// 未入库数量 - 对应实体 FNOTINSTOCKQTY
        /// 可选字段，精度(23,10)
        /// </summary>
        public decimal? FNOTINSTOCKQTY { get; set; }


    }
} 