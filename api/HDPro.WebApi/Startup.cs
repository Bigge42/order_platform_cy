using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using Swashbuckle.AspNetCore.SwaggerGen;
using HDPro.Core.Configuration;
using HDPro.Core.Extensions;
using HDPro.Core.Filters;
using HDPro.Core.Language;
//using HDPro.Core.KafkaManager.IService;
//using HDPro.Core.KafkaManager.Service;
using HDPro.Core.Middleware;
using HDPro.Core.ObjectActionValidator;
using HDPro.Core.Quartz;
using HDPro.Core.Utilities.PDFHelper;
using HDPro.Core.WorkFlow;
using HDPro.Entity.DomainModels;
using HDPro.WebApi.Controllers.Hubs;
using HDPro.Core.Controllers.DynamicController;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using HDPro.Core.Print;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using HDPro.Core.Controllers.Basic;
using HDPro.WebApi.Filters;
using HDPro.WebApi.Options;

namespace HDPro.WebApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<FixedTokenAuthOptions>(Configuration.GetSection("FixedTokenAuth"));
            services.AddScoped<FixedTokenAuthorizeFilter>();
        }
        public void ConfigureContainer()
        {
            //Services.AddModule(builder, Configuration);
            //初始化流程表，表里面必须有AuditStatus字段
            WorkFlowContainer.Instance
                .Use<MES_ProductionReporting, MES_ProductionReportingDetail>(
                 "生产报工",
                    filterFields: x => new {x.ReportingNumber, x.AcceptedQuantity, x.RejectedQuantity, x.Total, x.ReportedBy, x.ReportingTime },
                    //审批界面显示表数据字段
                    formFields: x => new { x.ReportedBy, x.ReportingNumber, x.ReportingTime, x.AcceptedQuantity, x.RejectedQuantity, x.Total },
                    //明细表显示的数据
                    formDetailFields: x => new { x.MaterialCode, x.MaterialName, x.MaterialSpecification, x.ReportedQuantity, x.RejectedQuantity, x.ReportHour }
                )
                .Use<Demo_Order, Demo_OrderList>("订单管理",
                    //过滤条件字段
                    filterFields: x => new { x.OrderDate, x.OrderNo, x.OrderStatus, x.OrderType, x.Creator, x.CustomerId },
                    //审批界面显示表数据字段
                    formFields: x => new { x.OrderDate, x.OrderNo, x.OrderStatus, x.Remark },
                    //明细表显示的数据
                    formDetailFields: x => new { x.GoodsName, x.GoodsCode, x.Qty, x.Price, x.Specs, x.Img, x.CreateDate },
                    //defaultAduitStatus: AuditStatus.草稿,
                    //可编辑的字段(字段必须在上面的formFields内)
                    editFields: x => new { x.OrderDate, x.OrderNo, x.OrderStatus, x.OrderType }
                )
                .Use<Demo_Product>(
                    "产品管理",
                    filterFields: x => new { x.Creator, x.Price, x.ProductCode, x.ProductName },
                    formFields: x => new { x.ProductCode, x.ProductName, x.Remark, x.Price, x.Creator, x.CreateDate }
                    )
                .Run();

            /**********************************打印配置****************************************************/
            PrintContainer.Instance
                 /*****************[全国城市]单表打印*****************/
                 .Use<MES_Material>(
                   //主表配置
                   name: "物料管理",
                   //主表可以打印的字段
                   printFields: x => new { x.MaterialCode, x.MaterialName, x.MaterialType, x.CatalogID, x.Img }
                 )
                   .Use<MES_ProductionReporting, MES_ProductionReportingDetail>(
                 "生产报工",
                    printFields: x => new {x.ReportingNumber,x.ReportingTime, x.AcceptedQuantity, x.RejectedQuantity, x.ReportedBy, x.Total },
                    //明细表配置
                   detailName: "报工明细",
                    //明细表显示的数据
                    detailPrintFields: x => new { x.MaterialCode, x.MaterialName, x.MaterialSpecification, x.ReportedQuantity, x.RejectedQuantity, x.ReportHour }
                )
               .Use<MES_ProductionOrder, MES_ProductionPlanDetail>(
                 "生产订单",
                    printFields: x => new { x.OrderNumber, x.OrderQty, x.OrderStatus,x.LV, x.OrderDate },
                   //明细表配置
                   detailName: "订单明细",
                    //明细表显示的数据
                    detailPrintFields: x => new { x.MaterialCode, x.MaterialName, x.MaterialSpecification, x.PlanQuantity, x.Unit}
                )
                  .Use<MES_Process, MES_ProcessRoute>(
                 "工序管理",
                    printFields: x => new { x.ProcessCode, x.ProcessName, x.ProcessType, x.ProcessStatus },
                   //明细表配置
                   detailName: "工艺路线",
                    //明细表显示的数据
                    detailPrintFields: x => new { x.ProductName, x.RouteSequence, x.RouteResponsible, x.PreProcessID, x.NextProcessID }
                )
                 /*****************[全国城市]单表打印*****************/
                 .Use<Sys_Region>(
                   //主表配置
                   name: "地址打印管理",
                   //主表可以打印的字段
                   printFields: x => new { x.name, x.code, x.mername, x.pinyin, x.Lat, x.Lng }
                 )
                 //主从表同时打印(注意Use第一个参数是主表，第二个明细表)
                 .Use<Demo_Order, Demo_OrderList>(
                   //主表配置
                   name: "订单打印管理",
                   //主表可以打印的字段
                   printFields: x => new { x.OrderNo, x.OrderStatus, x.OrderType, x.OrderDate, x.TotalPrice, x.TotalQty },

                   //明细表配置
                   detailName: "订单详情",
                   //明细表可以打印的字段
                   detailPrintFields: c => new { c.GoodsCode, c.GoodsName, c.Specs, c.Price, c.Qty }
                 )
                 /*****************[订单表]打印配置(主从表明细表(一对一))*****************/
                 .Use<Demo_Order, Demo_OrderList>(
                   //主表配置
                   name: "订单打印管理",
                   //主表可以打印的字段
                   printFields: x => new { x.OrderNo, x.OrderStatus, x.OrderType, x.OrderDate, x.TotalPrice, x.TotalQty },
                   //主表自定义打印的字段,没有就填null;需要在:PrintCustom类QueryResult字自定义返回这些字段的值
                   customFields: new List<CustomField>() {
                            new CustomField() { Name="自定义1",   Field="test1"  } ,
                            new CustomField() {  Name="自定义2",  Field="test2" }
                    },
                   //明细表配置
                   detail: new PrintDetailOptions<Demo_OrderList>()
                   {
                       Name = "订单详情",
                       PrintFields = c => new { c.GoodsCode, c.GoodsName, c.Img, c.Specs, c.Price, c.Qty },
                       //明细表自定义的字段,没有就填null;需要在:PrintCustom类QueryResult字自定义返回这些字段的值
                       CustomFields = new List<CustomField>() {
                            new CustomField() { Name="明细自定义1",   Field="test1"  } ,
                            new CustomField() { Name="明细自定义2",  Field="test2" }
                       }
                   }
                 )
                 /*****************[产品表]打印配置(一对多打印)*****************/
                 .Use<Demo_Product, Demo_ProductSize, Demo_ProductColor>(
                       //主表配置
                       name: "一对多打印测试",
                       //主表可以打印的字段
                       printFields: x => new { x.ProductName, x.ProductCode, x.AuditStatus, x.Price, x.Remark, x.CreateDate },
                       //主表自定义打印的字段,没有就填null;需要在:PrintCustom类QueryResult字自定义返回这些字段的值
                       customFields: null,//配置同上
                                          //明细表[产品尺寸]打印配置
                       detail1: new PrintDetailOptions<Demo_ProductSize>()
                       {
                           Name = "产品尺寸",
                           //明细表打印的字段
                           PrintFields = x => new { x.ProductId, x.Size, x.Unit, x.Remark, x.Creator, x.CreateDate },
                           //明细表自定义的字段,需要在:PrintCustom类QueryResult字自定义返回这些字段的值
                           CustomFields = null //自定义字段同上配置一样
                       },
                       //明细表[产品颜色]打印配置
                       detail2: new PrintDetailOptions<Demo_ProductColor>()
                       {
                           Name = "产品颜色",
                           //明细表打印的字段
                           PrintFields = x => new { x.ProductId, x.Img, x.Color, x.Qty, x.Unit, x.Remark, x.Creator, x.CreateDate },
                           //明细表自定义的字段需要在:PrintCustom类QueryResult字自定义返回这些字段的值
                           CustomFields = null //自定义字段同上配置一样
                       }
                 )
                 /*****************[MES自定义打印数据]单表打印*****************/
                 .Use<MES_SpecialPrintRequest>(
                   //主表配置
                   name: "MOM自定义打印资料",
                   //主表可以打印的字段
                   printFields: x => new { x.RetrospectCode,x.ProductModel,x.NominalDiameter,x.NominalPressure,x.ValveBodyMaterial,x.ActuatorModel,x.FailPosition,x.AirSupplyPressure,x.OperatingTemperature,x.RatedStroke,x.FlowCharacteristic, x.FlowCoefficient,x.UDF_Key1,x.UDF_Value1, x.UDF_Key2,x.UDF_Value2,x.UDF_Key3,x.UDF_Value3,x.UDF_Key4,x.UDF_Value4,x.UDF_Key5,x.UDF_Value5,x.UDF_Key6,x.UDF_Value6,x.UDF_Key7,x.UDF_Value7,x.UDF_Key8,x.UDF_Value8,x.UDF_Key9,x.UDF_Value9,x.UDF_Key10,x.UDF_Value10, x.UDF_Key11,x.UDF_Value11,x.UDF_Key12,x.UDF_Value12,x.UDF_Key13,x.UDF_Value13,x.UDF_Key14,x.UDF_Value14,x.UDF_Key15,x.UDF_Value15 }
                 )
                 ;
        }
    }
}
