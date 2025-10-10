namespace HDPro.Entity.DomainModels.ESB
{
    /// <summary>
    /// 产线产能ESB数据模型 (对应ESB接口返回)
    /// </summary>
    public class ESBDailyCapacityRecordData
    {
        /// <summary>
        /// 阀门大类（来源字段：F_ORA_FMLB）
        /// </summary>
        public string F_ORA_FMLB { get; set; }

        /// <summary>
        /// 生产线（来源字段：F_ORA_SCX）
        /// </summary>
        public string F_ORA_SCX { get; set; }

        /// <summary>
        /// 排产日期（来源字段：F_ORA_DATE1）
        /// </summary>
        public string F_ORA_DATE1 { get; set; }

        /// <summary>
        /// 产量数量（来源字段：FQTY）
        /// </summary>
        public decimal? FQTY { get; set; }

        // 如果接口返回其他字段但与本功能无关，可不在此定义
    }
}
