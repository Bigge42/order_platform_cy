IF OBJECT_ID(N'[dbo].[WZ_OrderCycleSchedulePredictionReview]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[WZ_OrderCycleSchedulePredictionReview](
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_WZ_OrderCycleSchedulePredictionReview] PRIMARY KEY,
        [PredictionResultId] BIGINT NULL,
        [OrderCycleBaseId] INT NULL,
        [InputFingerprint] NVARCHAR(64) NOT NULL,
        [RequestBatchNo] NVARCHAR(64) NULL,
        [ProductName] NVARCHAR(200) NULL,
        [SpecModel] NVARCHAR(200) NULL,
        [ValveCategory] NVARCHAR(2000) NULL,
        [NominalDiameter] NVARCHAR(50) NULL,
        [NominalPressure] NVARCHAR(50) NULL,
        [ProductionLine] NVARCHAR(50) NULL,
        [FixedCycleDays] INT NULL,
        [PredictedScheduleDate] DATE NULL,
        [StandardDeliveryDate] DATE NULL,
        [ConfidenceScore] DECIMAL(18,6) NULL,
        [MatchedRuleCount] INT NULL,
        [UsedFieldsJson] NVARCHAR(MAX) NULL,
        [CandidateSuggestionsJson] NVARCHAR(MAX) NULL,
        [FailureReason] NVARCHAR(200) NULL,
        [FailureMessage] NVARCHAR(500) NULL,
        [ReviewStatus] NVARCHAR(50) NOT NULL CONSTRAINT [DF_WZ_OrderCycleSchedulePredictionReview_ReviewStatus] DEFAULT(N'待核对'),
        [ReviewRemark] NVARCHAR(500) NULL,
        [FirstSeenAt] DATETIME NOT NULL CONSTRAINT [DF_WZ_OrderCycleSchedulePredictionReview_FirstSeenAt] DEFAULT(GETDATE()),
        [LastSeenAt] DATETIME NOT NULL CONSTRAINT [DF_WZ_OrderCycleSchedulePredictionReview_LastSeenAt] DEFAULT(GETDATE()),
        [SeenCount] INT NOT NULL CONSTRAINT [DF_WZ_OrderCycleSchedulePredictionReview_SeenCount] DEFAULT(1),
        [IsActive] BIT NOT NULL CONSTRAINT [DF_WZ_OrderCycleSchedulePredictionReview_IsActive] DEFAULT(1)
    );

    PRINT N'已创建 WZ_OrderCycleSchedulePredictionReview 排产日期预测核对表。';
END
ELSE
BEGIN
    PRINT N'WZ_OrderCycleSchedulePredictionReview 已存在，跳过建表。';
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_WZ_OrderCycleSchedulePredictionReview_InputFingerprint'
      AND object_id = OBJECT_ID(N'dbo.WZ_OrderCycleSchedulePredictionReview')
)
BEGIN
    CREATE UNIQUE INDEX [UX_WZ_OrderCycleSchedulePredictionReview_InputFingerprint]
        ON [dbo].[WZ_OrderCycleSchedulePredictionReview]([InputFingerprint]);

    PRINT N'已创建 WZ_OrderCycleSchedulePredictionReview 输入指纹唯一索引。';
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_WZ_OrderCycleSchedulePredictionReview_IsActive_LastSeenAt'
      AND object_id = OBJECT_ID(N'dbo.WZ_OrderCycleSchedulePredictionReview')
)
BEGIN
    CREATE INDEX [IX_WZ_OrderCycleSchedulePredictionReview_IsActive_LastSeenAt]
        ON [dbo].[WZ_OrderCycleSchedulePredictionReview]([IsActive], [LastSeenAt] DESC);

    PRINT N'已创建 WZ_OrderCycleSchedulePredictionReview 导出查询索引。';
END;
