using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Quartz.Impl;
using Quartz;
using HDPro.Core.CacheManager;
using HDPro.Core.Configuration;
using HDPro.Core.Dapper;
using HDPro.Core.Filters;
using HDPro.Core.Middleware;
using HDPro.Core.ObjectActionValidator;
using HDPro.Core.Quartz;
using HDPro.Core.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;
using HDPro.Core.Controllers.Basic;
using HDPro.Core.Language;
using HDPro.WebApi.Controllers.Hubs;
using System.Net;
using HDPro.WebApi;
using HDPro.Core.SignalR;
using HDPro.Core.Utilities;
using HDPro.CY.Order.Services.OrderCollaboration.ESB;
using NLog;
using NLog.Web;
using HDPro.CY.Order.IServices.MaterialCallBoard;
using HDPro.CY.Order.Services.MaterialCallBoard;
using HDPro.CY.Order.IRepositories;
using HDPro.CY.Order.Repositories;


// 早期初始化NLog以便捕获所有日志
var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("应用程序启动初始化");

var builder = WebApplication.CreateBuilder(args);

// 设置全局编码为UTF-8
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
Console.OutputEncoding = Encoding.UTF8;


builder.Services.AddHttpClient("WZ", client =>
{
    client.Timeout = TimeSpan.FromMinutes(10); // 按需调大/调小
});
// 配置NLog作为日志提供程序
builder.Logging.ClearProviders();
builder.Host.UseNLog();

builder.Services.AddModule(builder.Configuration);
// 添加ESB服务注册
builder.Services.AddESBServices();
// 添加后台服务
builder.Services.AddHostedService<BackgroundMessageService>();
// 添加预警规则初始化后台服务
builder.Services.AddHostedService<HDPro.CY.Order.Services.OrderCollaboration.AlertRulesInitializer>();
// 添加消息服务
builder.Services.AddSingleton<IMessageService, MessageService>();
// 添加消息通道
builder.Services.AddSingleton<MessageChannel>();
// 添加安全文件读取器
builder.Services.AddScoped<SafeFileReader>();
// 添加预警相关服务
builder.Services.AddScoped<HDPro.CY.Order.Services.OrderCollaboration.AlertRulesSchedulerService>();
builder.Services.AddScoped<HDPro.CY.Order.Services.OrderCollaboration.AlertRulesLogService>();
builder.Services
    .AddControllers()
        //https://learn.microsoft.com/zh-cn/aspnet/core/web-api/jsonpatch?view=aspnetcore-8.0
        // 需要安装Microsoft.AspNetCore.Mvc.NewtonsoftJson包
        .AddNewtonsoftJson(op =>
        {
            op.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            op.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            op.SerializerSettings.Converters.Add(new LongCovert());
            // 确保中文字符正确序列化
            op.SerializerSettings.StringEscapeHandling = Newtonsoft.Json.StringEscapeHandling.Default;
        });
DapperParseGuidTypeHandler.InitParseGuid();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
          .AddJwtBearer(options =>
          {
              options.TokenValidationParameters = new TokenValidationParameters
              {
                  SaveSigninToken = true,// 保存token,后台验证token是否有效(必要)
                  ValidateIssuer = true,// 是否验证Issuer
                  ValidateAudience = true,// 是否验证Audience
                  ValidateLifetime = true,// 是否验证失效时间
                  ValidateIssuerSigningKey = true,// 是否验证SecurityKey
                  ValidAudience = AppSetting.Secret.Audience,//Audience
                  ValidIssuer = AppSetting.Secret.Issuer,//Issuer，这两项和前面签发jwt的设置一致
                  IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSetting.Secret.JWT))
              };
              options.Events = new JwtBearerEvents()
              {
                  OnChallenge = context =>
                  {
                      context.HandleResponse();
                      context.Response.Clear();
                      context.Response.ContentType = "application/json; charset=utf-8";
                      context.Response.StatusCode = 401;
                      context.Response.WriteAsync(new { message = "授权未通过", status = false, code = 401 }.Serialize(), Encoding.UTF8);
                      return Task.CompletedTask;
                  }
              };
          });
//builder.Services.AddCors();
//builder.Services.AddCors(options =>
//{
//    options.AddDefaultPolicy(
//        builder =>
//        {
//            builder.AllowAnyOrigin()
//                         .SetPreflightMaxAge(TimeSpan.FromSeconds(2520))
//                          .AllowAnyHeader().AllowAnyMethod();
//        });
//});
builder.Services.AddCors(options =>
{
    options.AddPolicy("cors", builder =>
    {
        builder.SetIsOriginAllowed(_ => true)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HDPro.core.api", Version = "v1" });
    var security = new Dictionary<string, IEnumerable<string>> { { AppSetting.Secret.Issuer, new string[] { } }};
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Description = "JWT授权token前面需要加上字段Bearer和一个空格，如Bearer token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    { { new OpenApiSecurityScheme{  Reference = new OpenApiReference {  Type = ReferenceType.SecurityScheme,  Id = "Bearer" }}, new string[] { }  } });
})
 .AddControllers()
.ConfigureApiBehaviorOptions(options =>
{
    options.SuppressConsumesConstraintForFormFileParameters = true;
    options.SuppressInferBindingSourcesForParameters = true;
    options.SuppressModelStateInvalidFilter = true;
    options.SuppressMapClientErrors = true;
    options.ClientErrorMapping[404].Link =
        "https://*/404";
});
builder.Services.AddSignalR();
builder.Services.AddHttpClient()
.AddHttpContextAccessor()
.AddMemoryCache()
.AddTransient<HttpResultfulJob>()
.AddTransient<HDPro.CY.Order.Services.OrderCollaboration.AlertRulesJob>()
.AddTransient<HDPro.CY.Order.Services.OrderCollaboration.SingleAlertRuleJob>()
.AddSingleton<ISchedulerFactory, StdSchedulerFactory>()
.AddSingleton<Quartz.Spi.IJobFactory, IOCJobFactory>()
.AddSingleton<RedisCacheService>()
.AddScoped<HDPro.Core.Utilities.HttpClientHelper>();

// 添加集成服务的HttpClient配置
builder.Services.AddHttpClient<HDPro.CY.Order.IServices.SRM.ISRMIntegrationService, HDPro.CY.Order.Services.SRM.SRMIntegrationService>();
builder.Services.AddHttpClient<HDPro.CY.Order.IServices.OA.IOAIntegrationService, HDPro.CY.Order.Services.OA.OAIntegrationService>();

builder.Services.AddMvc(options =>
{
    options.Filters.Add(typeof(ApiAuthorizeFilter));
    options.Filters.Add(typeof(ActionExecuteFilter));
});
var startup = new Startup(builder.Configuration);

startup.ConfigureContainer();
builder.Services.UseMethodsModelParameters().UseMethodsGeneralParameters();
builder.Services.AddSingleton<IObjectModelValidator>(new NullObjectModelValidator());
//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.WebHost.UseUrls("http://*:9200");
builder.Services.AddScoped<IMaterialCallBoardRepository, MaterialCallBoardRepository>();
builder.Services.AddScoped<IMaterialCallBatchService, MaterialCallBatchService>();
builder.Services.Configure<FormOptions>(x =>
{
    x.MultipartBodyLengthLimit = 1024 * 1024 * 100;
}).Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 1024 * 1024 * 100;
}).Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 1024 * 1024 * 100;
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
//else
//{
    // 定时任务，如果不需要定时执行定时任务，请将此处放到else里面
    app.UseQuartz(app.Environment);
//}
app.UseLanguagePack().UseMiddleware<LanguageMiddleWare>();
app.UseMiddleware<ExceptionHandlerMiddleWare>();
app.UseDefaultFiles();
app.UseStaticFiles().UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true
});
app.Use(HttpRequestMiddleware.Context);

string _uploadPath = (app.Environment.ContentRootPath + "/Upload").ReplacePath();

if (!Directory.Exists(_uploadPath))
{
    Directory.CreateDirectory(_uploadPath);
}

app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(
    Path.Combine(Directory.GetCurrentDirectory(), @"Upload")),
    RequestPath = "/Upload",
    OnPrepareResponse = (Microsoft.AspNetCore.StaticFiles.StaticFileResponseContext staticFile) =>{}
});
// 设置HttpContext
app.UseStaticHttpContext();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
        options.DocumentTitle = "HDPro API 文档";
        options.DefaultModelsExpandDepth(-1);
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });
}
app.UseCors("cors");
app.UseCors();
// 使用 HTTPS 重定向
//app.UseHttpsRedirection();
// 使用路由
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapHub<MessageHub>("/hub/message");
app.MapControllers();

try
{
    logger.Debug("应用程序启动完成");
    app.Run();
}
catch (Exception exception)
{
    // NLog: 捕获设置错误
    logger.Error(exception, "应用程序因异常停止");
    throw;
}
finally
{
    // 确保在应用程序退出前刷新和停止内部计时器/线程
    NLog.LogManager.Shutdown();
}