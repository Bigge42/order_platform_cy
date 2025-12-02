# 物料主数据新增字段部署指南

## 概述

本次更新为物料主数据表OCP_Material新增5个字段，用于存储从金蝶K3Cloud同步的物料扩展信息。

**新增字段**：
1. 法兰标准 (FlangeStandard)
2. 阀体材质 (BodyMaterial)
3. 阀内件材质 (TrimMaterial)
4. 法兰密封面型式 (FlangeSealType)
5. TC发布人 (TCReleaser)

## 部署步骤

### 1. 数据库迁移

#### 1.1 备份数据库
```sql
-- 在执行任何修改前，请先备份数据库
BACKUP DATABASE [YourDatabaseName] 
TO DISK = 'D:\Backup\YourDatabaseName_Before_Material_Fields_20241116.bak'
WITH FORMAT, INIT, NAME = 'Full Backup Before Material Fields Update';
```

#### 1.2 执行迁移脚本
1. 打开SQL Server Management Studio
2. 连接到目标数据库服务器
3. 打开文件：`api/Database/Migrations/20241116_Add_Material_Fields.sql`
4. 修改脚本第一行的数据库名称：
   ```sql
   USE [YourDatabaseName]  -- 替换为实际的数据库名称
   ```
5. 执行脚本（F5）
6. 检查执行结果，确保所有字段都成功添加

#### 1.3 验证字段
```sql
-- 验证新字段是否成功添加
SELECT 
    COLUMN_NAME AS '字段名',
    DATA_TYPE AS '数据类型',
    CHARACTER_MAXIMUM_LENGTH AS '最大长度',
    IS_NULLABLE AS '可空'
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'OCP_Material'
AND COLUMN_NAME IN ('FlangeStandard', 'BodyMaterial', 'TrimMaterial', 'FlangeSealType', 'TCReleaser')
ORDER BY ORDINAL_POSITION
```

预期结果应显示5个新字段，数据类型和长度如下：
- FlangeStandard: nvarchar(2000), 可空
- BodyMaterial: nvarchar(2000), 可空
- TrimMaterial: nvarchar(2000), 可空
- FlangeSealType: nvarchar(2000), 可空
- TCReleaser: nvarchar(200), 可空

### 2. 后端部署

#### 2.1 编译代码
```bash
cd api
dotnet build --configuration Release
```

#### 2.2 检查编译结果
确保没有编译错误，特别注意以下文件：
- HDPro.Entity.dll
- HDPro.CY.Order.dll

#### 2.3 部署到服务器
1. 停止IIS应用程序池或后端服务
2. 备份现有的dll文件
3. 复制新编译的dll文件到服务器
4. 启动IIS应用程序池或后端服务

#### 2.4 验证服务启动
- 检查应用程序日志，确保没有启动错误
- 访问健康检查接口（如果有）

### 3. 前端部署

#### 3.1 构建前端
```bash
cd web.vite
npm run build
```

#### 3.2 部署到服务器
1. 备份现有的前端文件
2. 复制dist目录到服务器
3. 清除浏览器缓存

### 4. 数据同步

#### 4.1 执行物料同步
1. 登录系统
2. 进入"系统管理" -> "数据同步" -> "物料同步"
3. 点击"同步物料数据"按钮
4. 等待同步完成

**注意**：
- 首次同步可能需要较长时间（取决于物料数量）
- 建议在业务低峰期执行
- 可以先同步少量数据测试

#### 4.2 验证同步结果
```sql
-- 查询新增字段的数据
SELECT TOP 10
    MaterialCode AS '物料编码',
    MaterialName AS '物料名称',
    FlangeStandard AS '法兰标准',
    BodyMaterial AS '阀体材质',
    TrimMaterial AS '阀内件材质',
    FlangeSealType AS '法兰密封面型式',
    TCReleaser AS 'TC发布人',
    ModifyDate AS '修改时间'
FROM OCP_Material
WHERE FlangeStandard IS NOT NULL 
   OR BodyMaterial IS NOT NULL
   OR TrimMaterial IS NOT NULL
   OR FlangeSealType IS NOT NULL
   OR TCReleaser IS NOT NULL
ORDER BY ModifyDate DESC
```

### 5. 功能验证

#### 5.1 BOM查询验证
1. 登录系统
2. 进入"订单协同" -> "BOM查询"
3. 输入物料编码（选择一个有新字段数据的物料）
4. 点击"查询"
5. 检查物料信息区域是否显示新字段：
   - ✅ 法兰标准
   - ✅ 法兰密封面型式
   - ✅ 阀体材质
   - ✅ 阀内件材质
   - ✅ TC发布人

#### 5.2 数据完整性验证
- 检查字段值是否正确
- 检查是否有乱码
- 检查空值处理是否正常

## 回滚方案

如果部署后发现问题，可以按以下步骤回滚：

### 1. 回滚数据库
```sql
-- 删除新增的字段（谨慎操作！）
ALTER TABLE [dbo].[OCP_Material] DROP COLUMN [FlangeStandard]
ALTER TABLE [dbo].[OCP_Material] DROP COLUMN [BodyMaterial]
ALTER TABLE [dbo].[OCP_Material] DROP COLUMN [TrimMaterial]
ALTER TABLE [dbo].[OCP_Material] DROP COLUMN [FlangeSealType]
ALTER TABLE [dbo].[OCP_Material] DROP COLUMN [TCReleaser]
```

或者恢复数据库备份：
```sql
RESTORE DATABASE [YourDatabaseName] 
FROM DISK = 'D:\Backup\YourDatabaseName_Before_Material_Fields_20241116.bak'
WITH REPLACE
```

### 2. 回滚后端
1. 停止服务
2. 恢复备份的dll文件
3. 启动服务

### 3. 回滚前端
1. 恢复备份的前端文件
2. 清除浏览器缓存

## 常见问题

### Q1: 数据库迁移脚本执行失败
**A**: 检查以下几点：
- 数据库名称是否正确
- 是否有足够的权限
- OCP_Material表是否存在
- 字段是否已经存在

### Q2: 物料同步后新字段仍然为空
**A**: 检查以下几点：
- 金蝶K3Cloud中是否存在对应字段
- 字段名称是否正确（F_BLN_Flbz等）
- 查看同步日志，是否有错误信息
- 检查K3CloudService中的FieldKeys是否包含新字段

### Q3: 前端显示undefined或null
**A**: 检查以下几点：
- 后端API是否返回了新字段
- 前端字段绑定是否正确（camelCase）
- 浏览器缓存是否已清除

### Q4: 编译错误
**A**: 检查以下几点：
- .NET SDK版本是否正确
- 依赖包是否完整
- 代码语法是否正确

## 联系支持

如果遇到问题，请联系技术支持团队，并提供以下信息：
- 错误信息截图
- 相关日志文件
- 执行的操作步骤
- 环境信息（数据库版本、.NET版本等）

