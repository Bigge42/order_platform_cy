using System;
using System.ComponentModel.DataAnnotations;

namespace HDPro.Entity.DomainModels.ESB
{
    /// <summary>
    /// 金工车间业务ESB数据交换模型
    /// </summary>
    public class ESBMetalworkData
    {
        /// <summary>
        /// 操作类型（SAVE-保存, UPDATE-更新, DELETE-删除）
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// 单据类型（JGPrdMO-金工生产订单, JGPrdMODetail-金工生产订单明细, JGUnFinishTrack-金工未完工跟踪）
        /// </summary>
        public string BillType { get; set; }

        /// <summary>
        /// 生产订单相关数据
        /// </summary>
        public ESBJGPrdMOData JGPrdMO { get; set; }

        /// <summary>
        /// 生产订单明细相关数据
        /// </summary>
        public ESBJGPrdMODetailData JGPrdMODetail { get; set; }

        /// <summary>
        /// 未完工跟踪相关数据
        /// </summary>
        public ESBJGUnFinishTrackData JGUnFinishTrack { get; set; }
    }

    /// <summary>
    /// 金工生产订单ESB数据模型
    /// </summary>
    public class ESBJGPrdMOData
    {
        /// <summary>
        /// 生产订单主键 - 对应实体 FID
        /// 必填字段
        /// </summary>
        public long FID { get; set; }

        /// <summary>
        /// 生产订单号 - 对应实体 ProductionOrderNo
        /// 可选字段，最大长度50
        /// </summary>
        public string FBILLNO { get; set; }

        /// <summary>
        /// 催货单位任务时间月份 - 对应实体 PlanTaskMonth
        /// 可选字段，最大长度20
        /// </summary>
        public string FCUSTUNMONTH { get; set; }

        /// <summary>
        /// 催货单位任务时间周 - 对应实体 PlanTaskWeek
        /// 可选字段，最大长度20
        /// </summary>
        public string FCUSTUNWEEK { get; set; }

        /// <summary>
        /// 催货单位紧急等级 - 对应实体 Urgency
        /// 可选字段，最大长度20
        /// </summary>
        public string FCUSTUNEMER { get; set; }

        /// <summary>
        /// 生产订单审核时间 - 对应实体 MOAuditDate
        /// 可选字段，最大长度120
        /// </summary>
        public string FAPPROVEDATE { get; set; }

        /// <summary>
        /// 生产订单单据类型 - 对应实体 ProductionType
        /// 可选字段，最大长度50
        /// </summary>
        public string FBILLTYPENAME { get; set; }
    }

    /// <summary>
    /// 金工生产订单明细ESB数据模型
    /// </summary>
    public class ESBJGPrdMODetailData
    {
        /// <summary>
        /// 生产订单明细主键
        /// 必填字段
        /// </summary>
        public int FENTRYID { get; set; }

        /// <summary>
        /// 生产订单主键
        /// 可选字段
        /// </summary>
        public int? FID { get; set; }

        /// <summary>
        /// 行号
        /// 可选字段
        /// </summary>
        public int? FSEQ { get; set; }

        /// <summary>
        /// 生产订单状态
        /// 可选字段，最大长度30
        /// </summary>
        public string FSTATUSNAME { get; set; }

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
        /// </summary>
        public string FCUSTUNMONTH { get; set; }

        /// <summary>
        /// 催货单位任务时间周
        /// 可选字段，最大长度20
        /// </summary>
        public string FCUSTUNWEEK { get; set; }

        /// <summary>
        /// 催货单位紧急等级
        /// 可选字段，最大长度20
        /// </summary>
        public string FCUSTUNEMER { get; set; }

        /// <summary>
        /// 计划完工时间
        /// 可选字段，最大长度120
        /// </summary>
        public string FPLANFINISHDATE { get; set; }

        /// <summary>
        /// 实际完工时间
        /// 可选字段，最大长度120
        /// </summary>
        public string FFINISHDATE { get; set; }

        /// <summary>
        /// 领料日期
        /// 可选字段，最大长度120
        /// </summary>
        public string F_BLN_LLRQ { get; set; }

        /// <summary>
        /// 生产数量
        /// 可选字段，精度(23,10)
        /// </summary>
        public decimal? FQTY { get; set; }

        /// <summary>
        /// 入库数量
        /// 可选字段，精度(23,10)
        /// </summary>
        public decimal? FSTOCKINQUAAUXQTY { get; set; }

        /// <summary>
        /// 未入库数量
        /// 可选字段，精度(23,10)
        /// </summary>
        public decimal? FNOTINSTOCKQTY { get; set; }
    }

    /// <summary>
    /// 金工未完工跟踪ESB数据模型
    /// </summary>
    public class ESBJGUnFinishTrackData
    {
        /// <summary>
        /// 主键，由生产订单明细主键+MES工单ID组成，中间由'+'连接
        /// 必填字段，最大长度100
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 生产订单明细主键
        /// 必填字段
        /// </summary>
        public int FENTRYID { get; set; }

        /// <summary>
        /// 计划跟踪号
        /// 可选字段，最大长度255
        /// </summary>
        public string FMTONO { get; set; }

        /// <summary>
        /// 生产订单号
        /// 可选字段，最大长度50
        /// </summary>
        public string FBILLNO { get; set; }

        /// <summary>
        /// MES工单号
        /// 可选字段，最大长度50
        /// </summary>
        public string WorkOrder_Code { get; set; }

        /// <summary>
        /// 物料ID
        /// 可选字段
        /// </summary>
        public int? FMATERIALID { get; set; }

        /// <summary>
        /// 催货单位任务时间月份
        /// 可选字段，最大长度20
        /// </summary>
        public string FCUSTUNMONTH { get; set; }

        /// <summary>
        /// 催货单位任务时间周
        /// 可选字段，最大长度20
        /// </summary>
        public string FCUSTUNWEEK { get; set; }

        /// <summary>
        /// 催货单位紧急等级
        /// 可选字段，最大长度20
        /// </summary>
        public string FCUSTUNEMER { get; set; }

        /// <summary>
        /// 生产订单实际开工时间
        /// 可选字段，最大长度120
        /// </summary>
        public string FSTARTDATE { get; set; }

        /// <summary>
        /// MES当前工序
        /// 可选字段，最大长度50
        /// </summary>
        public string FMESCurrentProcedure { get; set; }

        /// <summary>
        /// MES叫料日期
        /// 可选字段，最大长度120
        /// </summary>
        public string FMESJLRQ { get; set; }

        /// <summary>
        /// 生产数量
        /// 可选字段，精度(23,10)
        /// </summary>
        public decimal? FQTY { get; set; }

        /// <summary>
        /// 入库数量
        /// 可选字段，精度(23,10)
        /// </summary>
        public decimal? FSTOCKINQUAAUXQTY { get; set; }

        /// <summary>
        /// MES批次号
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

        /// <summary>
        /// 工序委外发出日期
        /// 可选字段，最大长度120
        /// </summary>
        public string GXWWFCDATE { get; set; }

        /// <summary>
        /// 工序入库日期
        /// 可选字段，最大长度120
        /// </summary>
        public string GXWWRKDATE { get; set; }

        /// <summary>
        /// 当前工序状态
        /// 可选字段，最大长度30
        /// </summary>
        public string ProcessStateName { get; set; }
    }
}