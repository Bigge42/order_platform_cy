SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.t_material_bom_creator', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.t_material_bom_creator
    (
        MaterialNumber NVARCHAR(100) NOT NULL,
        TCBomCreator   NVARCHAR(100) NULL,
        CreatedAt      DATETIME2(0) NOT NULL CONSTRAINT DF_t_material_bom_creator_CreatedAt DEFAULT SYSDATETIME(),
        UpdatedAt      DATETIME2(0) NOT NULL CONSTRAINT DF_t_material_bom_creator_UpdatedAt DEFAULT SYSDATETIME()
    );
END
ELSE
BEGIN
    IF COL_LENGTH(N'dbo.t_material_bom_creator', N'TCBomCreator') IS NULL
    BEGIN
        ALTER TABLE dbo.t_material_bom_creator ADD TCBomCreator NVARCHAR(100) NULL;
    END;

    IF COL_LENGTH(N'dbo.t_material_bom_creator', N'CreatedAt') IS NULL
    BEGIN
        ALTER TABLE dbo.t_material_bom_creator
        ADD CreatedAt DATETIME2(0) NOT NULL
            CONSTRAINT DF_t_material_bom_creator_CreatedAt DEFAULT SYSDATETIME();
    END;

    IF COL_LENGTH(N'dbo.t_material_bom_creator', N'UpdatedAt') IS NULL
    BEGIN
        ALTER TABLE dbo.t_material_bom_creator
        ADD UpdatedAt DATETIME2(0) NOT NULL
            CONSTRAINT DF_t_material_bom_creator_UpdatedAt DEFAULT SYSDATETIME();
    END;
END;
GO

IF OBJECT_ID(N'dbo.t_material_bom_creator_task', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.t_material_bom_creator_task
    (
        ID             BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_t_material_bom_creator_task PRIMARY KEY,
        MaterialNumber NVARCHAR(100) NOT NULL,
        Status         NVARCHAR(20) NOT NULL CONSTRAINT DF_t_material_bom_creator_task_Status DEFAULT N'Pending',
        RetryCount     INT NOT NULL CONSTRAINT DF_t_material_bom_creator_task_RetryCount DEFAULT 0,
        BatchNo        UNIQUEIDENTIFIER NULL,
        LastError      NVARCHAR(1000) NULL,
        LastAttemptAt  DATETIME2(0) NULL,
        CreatedAt      DATETIME2(0) NOT NULL CONSTRAINT DF_t_material_bom_creator_task_CreatedAt DEFAULT SYSDATETIME(),
        UpdatedAt      DATETIME2(0) NOT NULL CONSTRAINT DF_t_material_bom_creator_task_UpdatedAt DEFAULT SYSDATETIME(),
        CONSTRAINT UQ_t_material_bom_creator_task_MaterialNumber UNIQUE(MaterialNumber),
        CONSTRAINT CK_t_material_bom_creator_task_Status CHECK (Status IN (N'Pending', N'Running', N'Success', N'Failed', N'NoResult'))
    );
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.t_material_bom_creator')
      AND name = N'IX_t_material_bom_creator_MaterialNumber'
)
BEGIN
    CREATE INDEX IX_t_material_bom_creator_MaterialNumber
    ON dbo.t_material_bom_creator(MaterialNumber)
    INCLUDE (TCBomCreator);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.t_material_bom_creator_task')
      AND name = N'IX_t_material_bom_creator_task_Status'
)
BEGIN
    CREATE INDEX IX_t_material_bom_creator_task_Status
    ON dbo.t_material_bom_creator_task(Status, RetryCount, CreatedAt)
    INCLUDE (MaterialNumber, BatchNo);
END;
GO

IF COL_LENGTH(N'dbo.OCP_OrderTracking', N'SOBillNo') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.OCP_OrderTracking')
          AND name = N'IX_OCP_OrderTracking_SOBillNo_OrderDates'
   )
BEGIN
    CREATE INDEX IX_OCP_OrderTracking_SOBillNo_OrderDates
    ON dbo.OCP_OrderTracking(SOBillNo)
    INCLUDE (OrderAuditDate, OrderCreateDate);
END;
GO

IF COL_LENGTH(N'dbo.OCP_TechManagement', N'OrderAuditDate') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.OCP_TechManagement')
          AND name = N'IX_OCP_TechManagement_OrderAuditDate'
   )
BEGIN
    CREATE INDEX IX_OCP_TechManagement_OrderAuditDate
    ON dbo.OCP_TechManagement(OrderAuditDate);
END;
GO

IF COL_LENGTH(N'dbo.OCP_TechManagement', N'SOBillNo') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.OCP_TechManagement')
          AND name = N'IX_OCP_TechManagement_SOBillNo'
   )
BEGIN
    CREATE INDEX IX_OCP_TechManagement_SOBillNo
    ON dbo.OCP_TechManagement(SOBillNo);
END;
GO

IF COL_LENGTH(N'dbo.OCP_TechManagement', N'MaterialNumber') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.OCP_TechManagement')
          AND name = N'IX_OCP_TechManagement_MaterialNumber'
   )
BEGIN
    CREATE INDEX IX_OCP_TechManagement_MaterialNumber
    ON dbo.OCP_TechManagement(MaterialNumber);
END;
GO

CREATE OR ALTER VIEW dbo.vw_OCP_Tech_BOM_Status_Monthly
AS
SELECT
    tm.PlanTraceNo AS PlanTraceNo,
    tm.MaterialNumber AS MaterialCode,
    tm.ProductModel AS ProductModel,
    tm.SOBillNo AS OrderNo,

    COALESCE(ot.OrderCreateDate, tm.OrderCreateDate) AS OrderDate,
    COALESCE(ot.OrderAuditDate, tm.OrderAuditDate) AS OrderAuditDate,

    tm.BOMCreateDate AS BomCreateDate,
    CASE
        WHEN tm.HasBOM = 1 AND tm.BOMCreateDate IS NOT NULL
        THEN DATEDIFF(DAY, tm.BOMCreateDate, GETDATE())
        ELSE NULL
    END AS BomAgeDays,
    CASE
        WHEN tm.BOMCreateDate IS NOT NULL
             AND COALESCE(ot.OrderAuditDate, tm.OrderAuditDate) IS NOT NULL
        THEN DATEDIFF(DAY, COALESCE(ot.OrderAuditDate, tm.OrderAuditDate), tm.BOMCreateDate)
        ELSE NULL
    END AS BomDelayDays,
    CASE
        WHEN (tm.HasBOM = 0 OR tm.BOMCreateDate IS NULL)
             AND COALESCE(ot.OrderAuditDate, tm.OrderAuditDate) IS NOT NULL
        THEN DATEDIFF(DAY, COALESCE(ot.OrderAuditDate, tm.OrderAuditDate), GETDATE())
        ELSE 0
    END AS BomMissingDays,

    mbc.TCBomCreator AS TCBomCreator,

    tm.MaterialName AS MaterialName,
    tm.NominalDiameter AS NominalDiameter,
    tm.FBOMFINISHEDNAME AS FBOMFINISHEDNAME,
    tm.NominalPressure AS NominalPressure,

    om.CV AS CV,

    CAST(NULL AS NVARCHAR(100)) AS FlangeStandard,
    CAST(NULL AS NVARCHAR(100)) AS SealFaceForm,
    CAST(NULL AS NVARCHAR(100)) AS BodyMaterial,
    CAST(NULL AS NVARCHAR(100)) AS TrimMaterial,

    tm.FlowCharacteristic AS FlowCharacteristic,
    tm.PackingForm AS PackingForm,
    tm.FlangeConnection AS FlangeConnection,
    tm.ActuatorModel AS ActuatorModel,
    tm.ActuatorStroke AS ActuatorStroke,
    tm.Remarks AS Remarks,

    tm.SalesQty AS Qty,
    tm.ReplyDeliveryDate AS ReplyDeliveryDate
FROM dbo.OCP_TechManagement AS tm
OUTER APPLY (
    SELECT TOP (1)
        tr.OrderAuditDate,
        tr.OrderCreateDate
    FROM dbo.OCP_OrderTracking AS tr
    WHERE tr.SOBillNo = tm.SOBillNo
    ORDER BY
        ISNULL(tr.OrderAuditDate, CONVERT(DATETIME, '19000101', 112)) DESC,
        ISNULL(tr.OrderCreateDate, CONVERT(DATETIME, '19000101', 112)) DESC
) AS ot
LEFT JOIN dbo.OCP_Material AS om
    ON tm.MaterialID = om.MaterialID
OUTER APPLY (
    SELECT TOP (1)
        creator.TCBomCreator
    FROM dbo.t_material_bom_creator AS creator
    WHERE creator.MaterialNumber = tm.MaterialNumber
    ORDER BY
        CASE WHEN NULLIF(LTRIM(RTRIM(ISNULL(creator.TCBomCreator, N''))), N'') IS NULL THEN 1 ELSE 0 END,
        creator.UpdatedAt DESC
) AS mbc;
GO

CREATE OR ALTER PROCEDURE dbo.usp_OCP_EnqueueMissingTCBomCreatorTasks
    @BatchLimit INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Limit INT = ISNULL(NULLIF(@BatchLimit, 0), 2147483647);

    ;WITH MissingCreator AS
    (
        SELECT TOP (@Limit)
            MaterialNumber = LTRIM(RTRIM(tm.MaterialNumber))
        FROM dbo.OCP_TechManagement AS tm
        OUTER APPLY (
            SELECT TOP (1)
                creator.TCBomCreator
            FROM dbo.t_material_bom_creator AS creator
            WHERE creator.MaterialNumber = tm.MaterialNumber
            ORDER BY
                CASE WHEN NULLIF(LTRIM(RTRIM(ISNULL(creator.TCBomCreator, N''))), N'') IS NULL THEN 1 ELSE 0 END,
                creator.UpdatedAt DESC
        ) AS mbc
        WHERE NULLIF(LTRIM(RTRIM(ISNULL(tm.MaterialNumber, N''))), N'') IS NOT NULL
          AND NULLIF(LTRIM(RTRIM(ISNULL(mbc.TCBomCreator, N''))), N'') IS NULL
        GROUP BY LTRIM(RTRIM(tm.MaterialNumber))
        ORDER BY LTRIM(RTRIM(tm.MaterialNumber))
    )
    MERGE dbo.t_material_bom_creator_task AS target
    USING MissingCreator AS source
        ON target.MaterialNumber = source.MaterialNumber
    WHEN NOT MATCHED THEN
        INSERT (MaterialNumber, Status, RetryCount, CreatedAt, UpdatedAt)
        VALUES (source.MaterialNumber, N'Pending', 0, SYSDATETIME(), SYSDATETIME())
    WHEN MATCHED
         AND target.Status IN (N'Failed', N'NoResult')
         AND target.RetryCount < 5 THEN
        UPDATE SET
            Status = N'Pending',
            LastError = NULL,
            UpdatedAt = SYSDATETIME();

    SELECT PendingCount = COUNT(1)
    FROM dbo.t_material_bom_creator_task
    WHERE Status = N'Pending';
END;
GO
