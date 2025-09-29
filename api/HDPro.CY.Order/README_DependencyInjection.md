# HDPro.CY.Order 依赖注入配置说明

## 概述

本项目使用Autofac作为依赖注入容器，结合.NET Core的内置DI容器。服务通过实现`IDependency`接口实现自动注册。

## 自动注册机制

### IDependency接口

所有需要自动注册的服务都需要实现`IDependency`接口：

```csharp
using HDPro.Core.Extensions.AutofacManager;

public interface IMyService : IDependency
{
    // 服务方法定义
}

public class MyService : IMyService, IDependency
{
    // 服务实现
}
```

### 自动注册规则

- 实现了`IDependency`接口的类会被自动注册为Scoped生命周期
- 接口和实现类会自动建立映射关系
- 支持多个接口实现

## SRM集成服务配置

### 服务接口

```csharp
// api/HDPro.CY.Order/IServices/SRM/ISRMIntegrationService.cs
public interface ISRMIntegrationService : IDependency
{
    Task<WebResponseContent> PushOrderAskAsync(List<OrderAskData> orderAskList, string operatorName = null);
    Task<WebResponseContent> PushSingleOrderAskAsync(OrderAskData orderAskData, string operatorName = null);
    Task<WebResponseContent> ValidateConnectionAsync();
    SRMConfig GetSRMConfig();
}
```

### 服务实现

```csharp
// api/HDPro.CY.Order/Services/SRM/SRMIntegrationService.cs
public class SRMIntegrationService : ISRMIntegrationService, IDependency
{
    private readonly ILogger<SRMIntegrationService> _logger;
    private readonly HttpClient _httpClient;
    
    public SRMIntegrationService(
        ILogger<SRMIntegrationService> logger,
        HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }
    
    // 服务实现...
}
```

### HttpClient配置

在`Program.cs`中配置HttpClient：

```csharp
// 添加集成服务的HttpClient配置
builder.Services.AddHttpClient<HDPro.CY.Order.IServices.SRM.ISRMIntegrationService, HDPro.CY.Order.Services.SRM.SRMIntegrationService>();
```

## 扩展方法配置（可选）

### ServiceCollectionExtensions

为了更好的组织和管理服务注册，可以使用扩展方法：

```csharp
// api/HDPro.CY.Order/Extensions/ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSRMIntegrationServices(this IServiceCollection services)
    {
        // 注册HttpClient
        services.AddHttpClient<ISRMIntegrationService, SRMIntegrationService>();
        
        // 注册SRM集成服务（可选，因为已有自动注册）
        services.AddScoped<ISRMIntegrationService, SRMIntegrationService>();
        
        return services;
    }
}
```

### 使用扩展方法

在`Program.cs`中调用：

```csharp
// 使用扩展方法注册服务
builder.Services.AddSRMIntegrationServices();
```

## 控制器中使用

### 构造函数注入

```csharp
[ApiController]
[Route("api/[controller]")]
public class SRMController : ControllerBase
{
    private readonly ISRMIntegrationService _srmIntegrationService;
    private readonly ILogger<SRMController> _logger;

    public SRMController(
        ISRMIntegrationService srmIntegrationService,
        ILogger<SRMController> logger)
    {
        _srmIntegrationService = srmIntegrationService;
        _logger = logger;
    }
    
    // 控制器方法...
}
```

## 生命周期管理

### 服务生命周期

- **Scoped**: 每个HTTP请求创建一个实例（默认）
- **Singleton**: 应用程序生命周期内只有一个实例
- **Transient**: 每次请求都创建新实例

### HttpClient生命周期

- HttpClient通过`AddHttpClient`注册，自动管理生命周期
- 避免手动创建HttpClient实例
- 支持配置超时、重试等策略

## 配置验证

### 服务注册验证

可以通过以下方式验证服务是否正确注册：

1. **启动日志检查**: 查看应用启动时的依赖注入日志
2. **API测试**: 调用相关API接口验证服务可用性
3. **调试模式**: 在构造函数中设置断点验证注入

### 常见问题

1. **循环依赖**: 避免服务间的循环引用
2. **生命周期不匹配**: 确保依赖服务的生命周期兼容
3. **接口未实现**: 确保所有接口都有对应的实现类

## 最佳实践

### 1. 接口设计

- 使用接口定义服务契约
- 接口应该职责单一
- 避免过大的接口

### 2. 依赖管理

- 通过构造函数注入依赖
- 避免服务定位器模式
- 使用工厂模式处理复杂创建逻辑

### 3. 配置管理

- 将配置信息从appsettings.json读取
- 使用强类型配置类
- 验证配置的完整性

### 4. 错误处理

- 在服务层处理业务异常
- 记录详细的错误日志
- 返回友好的错误信息

## 示例代码

### 完整的服务注册示例

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// 基础服务注册
builder.Services.AddControllers();
builder.Services.AddHttpClient();

// 集成服务HttpClient配置
builder.Services.AddHttpClient<ISRMIntegrationService, SRMIntegrationService>();
builder.Services.AddHttpClient<IOAIntegrationService, OAIntegrationService>();

// 自定义服务注册（可选）
builder.Services.AddSRMIntegrationServices();

var app = builder.Build();

// 中间件配置...
app.Run();
```

### 服务使用示例

```csharp
// 在控制器中使用
[HttpPost("push-order-ask")]
public async Task<WebResponseContent> PushOrderAsk([FromBody] PushOrderAskRequest request)
{
    return await _srmIntegrationService.PushOrderAskAsync(request.OrderAskList, request.Operator);
}
```

## 总结

通过实现`IDependency`接口和配置HttpClient，SRM集成服务已经完成了依赖注入配置。服务会自动注册到DI容器中，可以在控制器和其他服务中通过构造函数注入使用。 