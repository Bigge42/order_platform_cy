/*
 * 数据库迁移脚本
 * 日期: 2024-11-16
 * 说明: 为OCP_Material表添加5个新字段
 * 字段说明:
 *   1. FlangeStandard - 法兰标准 (对应金蝶字段: F_BLN_Flbz)
 *   2. BodyMaterial - 阀体材质 (对应金蝶字段: F_BLN_Ftcz)
 *   3. TrimMaterial - 阀内件材质 (对应金蝶字段: F_BLN_Fljcz)
 *   4. FlangeSealType - 法兰密封面型式 (对应金蝶字段: F_BLN_Flmfmxs)
 *   5. TCReleaser - TC发布人 (对应金蝶字段: F_TC_RELEASER)
 */

USE [YourDatabaseName]  -- 请替换为实际的数据库名称
GO

-- 检查表是否存在
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OCP_Material]') AND type in (N'U'))
BEGIN
    PRINT '错误: OCP_Material表不存在'
    RETURN
END
GO

PRINT '开始为OCP_Material表添加新字段...'
GO

-- 1. 添加法兰标准字段
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[OCP_Material]') AND name = 'FlangeStandard')
BEGIN
    ALTER TABLE [dbo].[OCP_Material]
    ADD [FlangeStandard] NVARCHAR(2000) NULL
    PRINT '✓ 已添加字段: FlangeStandard (法兰标准)'
END
ELSE
BEGIN
    PRINT '× 字段已存在: FlangeStandard'
END
GO

-- 2. 添加阀体材质字段
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[OCP_Material]') AND name = 'BodyMaterial')
BEGIN
    ALTER TABLE [dbo].[OCP_Material]
    ADD [BodyMaterial] NVARCHAR(2000) NULL
    PRINT '✓ 已添加字段: BodyMaterial (阀体材质)'
END
ELSE
BEGIN
    PRINT '× 字段已存在: BodyMaterial'
END
GO

-- 3. 添加阀内件材质字段
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[OCP_Material]') AND name = 'TrimMaterial')
BEGIN
    ALTER TABLE [dbo].[OCP_Material]
    ADD [TrimMaterial] NVARCHAR(2000) NULL
    PRINT '✓ 已添加字段: TrimMaterial (阀内件材质)'
END
ELSE
BEGIN
    PRINT '× 字段已存在: TrimMaterial'
END
GO

-- 4. 添加法兰密封面型式字段
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[OCP_Material]') AND name = 'FlangeSealType')
BEGIN
    ALTER TABLE [dbo].[OCP_Material]
    ADD [FlangeSealType] NVARCHAR(2000) NULL
    PRINT '✓ 已添加字段: FlangeSealType (法兰密封面型式)'
END
ELSE
BEGIN
    PRINT '× 字段已存在: FlangeSealType'
END
GO

-- 5. 添加TC发布人字段
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[OCP_Material]') AND name = 'TCReleaser')
BEGIN
    ALTER TABLE [dbo].[OCP_Material]
    ADD [TCReleaser] NVARCHAR(200) NULL
    PRINT '✓ 已添加字段: TCReleaser (TC发布人)'
END
ELSE
BEGIN
    PRINT '× 字段已存在: TCReleaser'
END
GO

PRINT '数据库迁移完成！'
PRINT '请执行物料同步以从金蝶获取这些字段的数据。'
GO

-- 查询验证
SELECT 
    COLUMN_NAME AS '字段名',
    DATA_TYPE AS '数据类型',
    CHARACTER_MAXIMUM_LENGTH AS '最大长度',
    IS_NULLABLE AS '可空'
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'OCP_Material'
AND COLUMN_NAME IN ('FlangeStandard', 'BodyMaterial', 'TrimMaterial', 'FlangeSealType', 'TCReleaser')
ORDER BY ORDINAL_POSITION
GO

