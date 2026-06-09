using HDPro.Entity.SystemModels;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HDPro.Entity.DomainModels
{
    [Entity(TableCnName = "排产日期预测核对表", TableName = "WZ_OrderCycleSchedulePredictionReview", DBServer = "ServiceDbContext")]
    public class WZ_OrderCycleSchedulePredictionReview : ServiceEntity
    {
        [Key]
        [Display(Name = "Id")]
        [Column(TypeName = "int")]
        [Editable(true)]
        [Required]
        public int Id { get; set; }

        [Display(Name = "远端预测ID")]
        [Column(TypeName = "bigint")]
        [Editable(true)]
        public long? PredictionResultId { get; set; }

        [Display(Name = "排产基础ID")]
        [Column(TypeName = "int")]
        [Editable(true)]
        public int? OrderCycleBaseId { get; set; }

        [Display(Name = "输入指纹")]
        [MaxLength(64)]
        [Column(TypeName = "nvarchar(64)")]
        [Editable(true)]
        public string InputFingerprint { get; set; }

        [Display(Name = "批次号")]
        [MaxLength(64)]
        [Column(TypeName = "nvarchar(64)")]
        [Editable(true)]
        public string RequestBatchNo { get; set; }

        [Display(Name = "产品名称")]
        [MaxLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        [Editable(true)]
        public string ProductName { get; set; }

        [Display(Name = "规格型号")]
        [MaxLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        [Editable(true)]
        public string SpecModel { get; set; }

        [Display(Name = "阀门类别")]
        [MaxLength(2000)]
        [Column(TypeName = "nvarchar(2000)")]
        [Editable(true)]
        public string ValveCategory { get; set; }

        [Display(Name = "公称通径")]
        [MaxLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        [Editable(true)]
        public string NominalDiameter { get; set; }

        [Display(Name = "公称压力")]
        [MaxLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        [Editable(true)]
        public string NominalPressure { get; set; }

        [Display(Name = "生产线")]
        [MaxLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        [Editable(true)]
        public string ProductionLine { get; set; }

        [Display(Name = "固定周期")]
        [Column(TypeName = "int")]
        [Editable(true)]
        public int? FixedCycleDays { get; set; }

        [Display(Name = "推测排产日期")]
        [Column(TypeName = "date")]
        [Editable(true)]
        public DateTime? PredictedScheduleDate { get; set; }

        [Display(Name = "标准交货日期")]
        [Column(TypeName = "date")]
        [Editable(true)]
        public DateTime? StandardDeliveryDate { get; set; }

        [Display(Name = "置信度")]
        [DisplayFormat(DataFormatString = "18,6")]
        [Column(TypeName = "decimal(18,6)")]
        [Editable(true)]
        public decimal? ConfidenceScore { get; set; }

        [Display(Name = "匹配规则数")]
        [Column(TypeName = "int")]
        [Editable(true)]
        public int? MatchedRuleCount { get; set; }

        [Display(Name = "使用字段")]
        [Column(TypeName = "nvarchar(max)")]
        [Editable(true)]
        public string UsedFieldsJson { get; set; }

        [Display(Name = "候选结果")]
        [Column(TypeName = "nvarchar(max)")]
        [Editable(true)]
        public string CandidateSuggestionsJson { get; set; }

        [Display(Name = "失败原因")]
        [MaxLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        [Editable(true)]
        public string FailureReason { get; set; }

        [Display(Name = "失败说明")]
        [MaxLength(500)]
        [Column(TypeName = "nvarchar(500)")]
        [Editable(true)]
        public string FailureMessage { get; set; }

        [Display(Name = "核对状态")]
        [MaxLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        [Editable(true)]
        public string ReviewStatus { get; set; }

        [Display(Name = "核对备注")]
        [MaxLength(500)]
        [Column(TypeName = "nvarchar(500)")]
        [Editable(true)]
        public string ReviewRemark { get; set; }

        [Display(Name = "首次发现时间")]
        [Column(TypeName = "datetime")]
        [Editable(true)]
        public DateTime? FirstSeenAt { get; set; }

        [Display(Name = "最近发现时间")]
        [Column(TypeName = "datetime")]
        [Editable(true)]
        public DateTime? LastSeenAt { get; set; }

        [Display(Name = "发现次数")]
        [Column(TypeName = "int")]
        [Editable(true)]
        public int SeenCount { get; set; }

        [Display(Name = "是否当前有效")]
        [Column(TypeName = "bit")]
        [Editable(true)]
        public bool IsActive { get; set; }
    }
}
