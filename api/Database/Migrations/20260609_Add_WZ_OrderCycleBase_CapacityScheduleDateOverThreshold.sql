IF COL_LENGTH(N'dbo.WZ_OrderCycleBase', N'CapacityScheduleDateOverThreshold') IS NULL
BEGIN
    ALTER TABLE [dbo].[WZ_OrderCycleBase]
        ADD [CapacityScheduleDateOverThreshold] BIT NOT NULL
            CONSTRAINT [DF_WZ_OrderCycleBase_CapacityScheduleDateOverThreshold] DEFAULT (0);

    PRINT N'已添加 WZ_OrderCycleBase.CapacityScheduleDateOverThreshold 排产优化日期超阈值标记字段。';
END
ELSE
BEGIN
    PRINT N'WZ_OrderCycleBase.CapacityScheduleDateOverThreshold 已存在，跳过。';
END
