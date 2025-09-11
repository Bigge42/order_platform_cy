/*
 *所有关于MES_SpecialPrintRequest类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*MES_SpecialPrintRequestService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using HDPro.Core.BaseProvider;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;
using System.Linq;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
using HDPro.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.MES.IRepositories;
using HDPro.Core.Services;

namespace HDPro.MES.Services
{
    public partial class MES_SpecialPrintRequestService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMES_SpecialPrintRequestRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public MES_SpecialPrintRequestService(
            IMES_SpecialPrintRequestRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        /// <summary>
        /// 添加自定义打印資料记录
        /// </summary>
        /// <param name="requestEntity">请求实体</param>
        /// <returns>操作结果</returns>
        public async Task<WebResponseContent> AddReplyAsync(MES_SpecialPrintRequest requestEntity)
        {
            var response = new WebResponseContent();

            try
            {
                // 1. 数据验证
                if (requestEntity == null)
                {
                    return response.Error("推送数据不能为空");
                }

                if (requestEntity.RetrospectCode ==null)
                {
                    return response.Error("产品编码不能为空");
                }

                // 2. 业务验证 - 检查特殊打印资料是否存在
                var requestEntityExists = await _repository.DbContext.Set<MES_SpecialPrintRequest>()
                    .AnyAsync(u => u.RetrospectCode == requestEntity.RetrospectCode);

                if (requestEntityExists)
                {
                    //return response.Error("指定的自定义打印信息已存在");
                }

                // 3. 设置默认值
                requestEntity.SetCreateDefaultVal();
                if (requestEntity.Creator == null)
                {
                    requestEntity.Creator = "系统";
                    requestEntity.CreateID = 1;
                }

                //// 4. 实体验证
                //var validationResult = ValidateCYOrderEntity(requestEntity);
                //if (!validationResult.Status)
                //{
                //    return validationResult;
                //}

                // 5. 使用事务保存数据并更新时间
                using (var transaction = await _repository.DbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 查找已存在的推送记录
                        var existSPprint = await _repository.DbContext.Set<MES_SpecialPrintRequest>()
                            .FirstOrDefaultAsync(u => u.RetrospectCode == requestEntity.RetrospectCode );

                        if (existSPprint != null)
                        {
                            // existSPprint不为null时，用requestEntity的值更新existSPprint的所有字段
                            existSPprint.PositionNumber = requestEntity.PositionNumber;
                            existSPprint.ProductModel = requestEntity.ProductModel;
                            existSPprint.NominalDiameter = requestEntity.NominalDiameter;
                            existSPprint.NominalPressure = requestEntity.NominalPressure;
                            existSPprint.ValveBodyMaterial = requestEntity.ValveBodyMaterial;
                            existSPprint.ActuatorModel = requestEntity.ActuatorModel;
                            existSPprint.FailPosition = requestEntity.FailPosition;
                            existSPprint.AirSupplyPressure = requestEntity.AirSupplyPressure;
                            existSPprint.OperatingTemperature = requestEntity.OperatingTemperature;
                            existSPprint.RatedStroke = requestEntity.RatedStroke;
                            existSPprint.FlowCharacteristic = requestEntity.FlowCharacteristic;
                            existSPprint.FlowCoefficient = requestEntity.FlowCoefficient;
                            existSPprint.UDF_Key1 = requestEntity.UDF_Key1;
                            existSPprint.UDF_Value1 = requestEntity.UDF_Value1;
                            existSPprint.UDF_Key2 = requestEntity.UDF_Key2;
                            existSPprint.UDF_Value2 = requestEntity.UDF_Value2;
                            existSPprint.UDF_Key3 = requestEntity.UDF_Key3;
                            existSPprint.UDF_Value3 = requestEntity.UDF_Value3;
                            existSPprint.UDF_Key4 = requestEntity.UDF_Key4;
                            existSPprint.UDF_Value4 = requestEntity.UDF_Value4;
                            existSPprint.UDF_Key5 = requestEntity.UDF_Key5;
                            existSPprint.UDF_Value5 = requestEntity.UDF_Value5;
                            existSPprint.UDF_Key6 = requestEntity.UDF_Key6;
                            existSPprint.UDF_Value6 = requestEntity.UDF_Value6;
                            existSPprint.UDF_Key7 = requestEntity.UDF_Key7;
                            existSPprint.UDF_Value7 = requestEntity.UDF_Value7;
                            existSPprint.UDF_Key8 = requestEntity.UDF_Key8;
                            existSPprint.UDF_Value8 = requestEntity.UDF_Value8;
                            existSPprint.UDF_Key9 = requestEntity.UDF_Key9;
                            existSPprint.UDF_Value9 = requestEntity.UDF_Value9;
                            existSPprint.UDF_Key10 = requestEntity.UDF_Key10;
                            existSPprint.UDF_Value10 = requestEntity.UDF_Value10;
                            existSPprint.UDF_Key11 = requestEntity.UDF_Key11;
                            existSPprint.UDF_Value11 = requestEntity.UDF_Value11;
                            existSPprint.UDF_Key12 = requestEntity.UDF_Key12;
                            existSPprint.UDF_Value12 = requestEntity.UDF_Value12;
                            existSPprint.UDF_Key13 = requestEntity.UDF_Key13;
                            existSPprint.UDF_Value13 = requestEntity.UDF_Value13;
                            existSPprint.UDF_Key14 = requestEntity.UDF_Key14;
                            existSPprint.UDF_Value14 = requestEntity.UDF_Value14;
                            existSPprint.UDF_Key15 = requestEntity.UDF_Key15;
                            existSPprint.UDF_Value15 = requestEntity.UDF_Value15;

                            existSPprint.ModifyDate = DateTime.Now;
                            existSPprint.ModifyID = 1;
                            existSPprint.Modifier =  "系统";
                            _repository.DbContext.Set<MES_SpecialPrintRequest>().Update(existSPprint);
                        }
                        else 
                        {
                            // 保存新的推送记录
                            await _repository.AddAsync(requestEntity);
                        }
                            
                        await _repository.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }

                // 记录操作日志
                //LogCYOrderOperation("AddReply", requestEntity,
                //    $"添加特殊打印资料成功，识别ID：{requestEntity.udf1_key}，显示值：{requestEntity.udf1_value}，测试更新");

                response.OK("特殊打印资料添加成功");
                if (response.Status)
                {
                    response.Data = new
                    {
                        RetrospectCode = requestEntity.RetrospectCode
                    };
                }

                return response;
            }
            catch (Exception ex)
            {
                Logger.Error($"添加特殊打印数据失败：{ex.Message}");
                return response.Error($"添加特殊打印数据失败：{ex.Message}");
            }
        }

        ///// <summary>
        ///// 重写CY.Order项目通用数据验证方法
        ///// 可在此处添加OCP_requestEntity特有的数据验证逻辑
        ///// </summary>
        ///// <param name="entity">要验证的实体</param>
        ///// <returns>验证结果</returns>
        //protected override WebResponseContent ValidateCYOrderEntity(MES_SpecialPrintRequest entity)
        //{
        //    var response = base.ValidateCYOrderEntity(entity);

        //    // 在此处添加OCP_requestEntity特有的数据验证逻辑

        //    return response;
        //}
    }
}
