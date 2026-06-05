IF COL_LENGTH(N'dbo.ERP_OrderTracking', N'F_ORA_DATE1') IS NULL
BEGIN
    ALTER TABLE [dbo].[ERP_OrderTracking] ADD [F_ORA_DATE1] DATE NULL;
    PRINT N'已添加 ERP_OrderTracking.F_ORA_DATE1 排产日期字段。';
END
ELSE
BEGIN
    PRINT N'ERP_OrderTracking.F_ORA_DATE1 已存在，跳过。';
END
