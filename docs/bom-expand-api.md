# BOM展开接口文档

## 概述

本文档描述了K3CloudService中新增的BOM展开查询接口，用于查询物料的BOM结构并展开所有层级的子物料信息。

## 接口信息

### K3Cloud服务端接口

**接口地址**: `Haodee.K3.WebApi.ServicesStub.BomExpandService.Expand.common.kdsvc`

**完整URL**: `http://{ServerUrl}/k3cloud/Haodee.K3.WebApi.ServicesStub.BomExpandService.Expand.common.kdsvc`

### Web API接口

**接口地址**: `GET /api/K3CloudSync/ExpandBom`

**请求参数**:
- `materialNumber` (string, 必填): 物料编码

## 数据模型

### BomExpandRequestDto (请求DTO)

```csharp
public class BomExpandRequestDto
{
    /// <summary>
    /// 物料编码
    /// </summary>
    public string MaterialNumber { get; set; }
}
```

### BomExpandItemDto (BOM项DTO)

```csharp
public class BomExpandItemDto
{
    /// <summary>
    /// BOM层级(0表示顶层)
    /// </summary>
    public int BomLevel { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    public string Number { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 分子(用量计算)
    /// </summary>
    public decimal Numerator { get; set; }

    /// <summary>
    /// 分母(用量计算)
    /// </summary>
    public decimal Denominator { get; set; }

    /// <summary>
    /// 规格型号
    /// </summary>
    public string Specification { get; set; }

    /// <summary>
    /// 父级分录ID
    /// </summary>
    public string ParentEntryId { get; set; }

    /// <summary>
    /// 分录ID
    /// </summary>
    public string EntryId { get; set; }

    /// <summary>
    /// 单位编码
    /// </summary>
    public string UnitNumber { get; set; }

    /// <summary>
    /// 单位名称
    /// </summary>
    public string UnitName { get; set; }
}
```

### ServiceResult<T> (服务结果)

```csharp
public class ServiceResult<T>
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 返回消息
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 数据
    /// </summary>
    public T Data { get; set; }
}
```

## 使用示例

### 1. 在Service中调用

```csharp
// 注入IK3CloudService
private readonly IK3CloudService _k3CloudService;

public YourService(IK3CloudService k3CloudService)
{
    _k3CloudService = k3CloudService;
}

// 调用BOM展开方法
public async Task<ServiceResult<List<BomExpandItemDto>>> GetBomStructure(string materialNumber)
{
    var result = await _k3CloudService.ExpandBomAsync(materialNumber);
    return result;
}
```

### 2. HTTP请求示例

**请求**:
```http
GET /api/K3CloudSync/ExpandBom?materialNumber=aaaa
```

**成功响应**:
```json
{
    "status": true,
    "message": "BOM展开成功",
    "data": [
        {
            "bomLevel": 0,
            "number": "aaaa",
            "name": "产品A",
            "numerator": 1.0,
            "denominator": 1.0,
            "specification": "标准型",
            "parentEntryId": "",
            "entryId": "1001",
            "unitNumber": "Pcs",
            "unitName": "个"
        }
    ]
}
```

## 修改的文件列表

1. **api\HDPro.CY.Order\Services\K3Cloud\Models\K3CloudConfig.cs**
   - 新增 `BomExpandUrl` 属性

2. **api\HDPro.CY.Order\Services\K3Cloud\Models\BomExpandModels.cs** (新建)
   - 定义 `BomExpandRequestDto`
   - 定义 `BomExpandItemDto`
   - 定义 `ServiceResult<T>`

3. **api\HDPro.CY.Order\Services\K3Cloud\IK3CloudService.cs**
   - 新增 `ExpandBomAsync` 方法签名

4. **api\HDPro.CY.Order\Services\K3Cloud\K3CloudService.cs**
   - 实现 `ExpandBomAsync` 方法

5. **api\HDPro.WebApi\Controllers\Order\K3CloudSyncController.cs**
   - 新增 `ExpandBom` Web API接口

## 技术要点

1. **自动登录**: 方法内部会自动检查会话状态，必要时自动登录
2. **参数验证**: 自动验证物料编码不能为空
3. **错误处理**: 完善的异常处理和日志记录
4. **依赖注入**: 通过实现 `IDependency` 接口自动注册到DI容器

## 错误信息说明

- "物料编码不能为空！" - 未提供物料编码
- "上下文丢失，请重新登录！" - 会话已过期，登录失败
- "未在ERP中找到该物料编码" - 物料不存在
- "未在ERP中找到该物料的BOM" - 该物料没有BOM
- "BOM展开结果为空" - BOM展开失败

