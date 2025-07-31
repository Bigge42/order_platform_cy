/*
 *所有关于OCP_LackMtrlPlan类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*OCP_LackMtrlPlanService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using HDPro.CY.Order.Services;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;
using System.Linq;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
using HDPro.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.CY.Order.IRepositories;
using ServiceStack;
using HDPro.Utilities.Extensions;
using System;
using System.Collections.Generic;
using HDPro.Core.Services;

namespace HDPro.CY.Order.Services
{
    public partial class OCP_LackMtrlPlanService
    {
        private readonly IOCP_LackMtrlPlanRepository _repository;//访问数据库
        private readonly WebResponseContent webResponse = new WebResponseContent();

        [ActivatorUtilitiesConstructor]
        public OCP_LackMtrlPlanService(
            IOCP_LackMtrlPlanRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
            )
        : base(dbRepository, httpContextAccessor)
        {
            _repository = dbRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        /// <summary>
        /// 重写CY.Order项目特有的初始化逻辑
        /// 可在此处添加OCP_LackMtrlPlan特有的初始化代码
        /// </summary>
        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
            // 在此处添加OCP_LackMtrlPlan特有的初始化逻辑
        }

        /// <summary>
        /// 重写CY.Order项目通用数据验证方法
        /// 可在此处添加OCP_LackMtrlPlan特有的数据验证逻辑
        /// </summary>
        /// <param name="entity">要验证的实体</param>
        /// <returns>验证结果</returns>
        protected override WebResponseContent ValidateCYOrderEntity(OCP_LackMtrlPlan entity)
        {
            var response = base.ValidateCYOrderEntity(entity);

            // 在此处添加OCP_LackMtrlPlan特有的数据验证逻辑

            return response;
        }

        public override WebResponseContent Add(SaveModel saveDataModel)
        {

            //此方法中已开启了事务，如果在此方法中做其他数据库操作，请不要再开启事务
            // 在保存数据库后的操作，此时已进行数据提交，但未提交事务，如果返回false，则会回滚提交

            AddOnExecuted = (OCP_LackMtrlPlan plan, object list) =>
            {
                string filter = plan.Filter;
                var data = View_OrderTrackingService.Instance.GetPageData(new PageDataOptions() { Page = 1, Export = true, Wheres = filter });
                if (data != null && data.rows?.Count > 0)
                {
                    var mtoNos = data.rows.Where(p => !p.MtoNo.IsNullOrEmptyOrWhiteSpace()).Select(p => p.MtoNo).ToList();
                    List<OCP_LackMtrlPool> lackMtrlPools = new List<OCP_LackMtrlPool>();
                    foreach (var mtoNo in mtoNos)
                    {
                        var lackMtrlPool = new OCP_LackMtrlPool();
                        lackMtrlPool.MtoNo = mtoNo;
                        lackMtrlPool.ComputeID = plan.ComputeID;
                        lackMtrlPool.Status = 0;
                        lackMtrlPool.SetCreateDefaultVal();
                        lackMtrlPools.Add(lackMtrlPool);
                    }
                    if (lackMtrlPools.Count > 0)
                    {
                        repository.DbContext.Set<OCP_LackMtrlPool>().AddRange(lackMtrlPools);
                        repository.DbContext.SaveChanges();
                    }
                }
                //保存到数据库后，这里可以再查询数据库写业务操作
                //注意EF版,这里如果是执行的sql，只能使用ef执行sql，如:repository.DbContext.Database.xx
                return webResponse.OK("已新建成功,台AddOnExecuted方法返回的消息");
            };

            var res = base.Add(saveDataModel);
            return res;
        }

        /// <summary>
        /// 设置默认方案
        /// </summary>
        /// <param name="computeId">要设置为默认的方案ID</param>
        /// <returns>操作结果</returns>
        public WebResponseContent SetDefaultPlan(long computeId)
        {
            return repository.DbContextBeginTransaction(() =>
            {
                var response = new WebResponseContent();
                try
                {
                    // 检查方案是否存在
                    var targetPlan = repository.FindFirst(x => x.ComputeID == computeId);
                    if (targetPlan == null)
                    {
                        return response.Error("指定的方案不存在");
                    }

                    // 先将所有方案的默认状态设置为0（非默认）
                    var allPlans = repository.Find(x => x.IsDefault == 1).ToList();
                    foreach (var plan in allPlans)
                    {
                        plan.IsDefault = 0;
                        plan.SetModifyDefaultVal();
                    }

                    // 设置指定方案为默认方案
                    targetPlan.IsDefault = 1;
                    targetPlan.SetModifyDefaultVal();

                    // 保存更改
                    if (allPlans.Count > 0)
                    {
                        repository.UpdateRange(allPlans);
                    }
                    repository.Update(targetPlan);
                    repository.SaveChanges();

                    return response.OK($"方案\"{targetPlan.PlanName}\"已设置为默认方案");
                }
                catch (Exception ex)
                {
                    return response.Error($"设置默认方案失败：{ex.Message}");
                }
            });
        }

        /// <summary>
        /// 取消默认方案
        /// </summary>
        /// <param name="computeId">要取消默认的方案ID</param>
        /// <returns>操作结果</returns>
        public WebResponseContent CancelDefaultPlan(long computeId)
        {
            return repository.DbContextBeginTransaction(() =>
            {
                var response = new WebResponseContent();
                try
                {
                    // 检查方案是否存在
                    var targetPlan = repository.FindFirst(x => x.ComputeID == computeId);
                    if (targetPlan == null)
                    {
                        return response.Error("指定的方案不存在");
                    }

                    // 检查是否为默认方案
                    if (targetPlan.IsDefault != 1)
                    {
                        return response.Error("该方案不是默认方案");
                    }

                    // 取消默认状态
                    targetPlan.IsDefault = 0;
                    targetPlan.SetModifyDefaultVal();

                    repository.Update(targetPlan);
                    repository.SaveChanges();

                    return response.OK($"方案\"{targetPlan.PlanName}\"的默认状态已取消");
                }
                catch (Exception ex)
                {
                    return response.Error($"取消默认方案失败：{ex.Message}");
                }
            });
        }

        /// <summary>
        /// 根据运算ID删除相关的队列和运算结果数据
        /// </summary>
        /// <param name="computeIds">运算ID列表</param>
        /// <returns>删除结果</returns>
        private WebResponseContent DeleteRelatedDataByComputeIds(List<long> computeIds)
        {
            if (computeIds == null || computeIds.Count == 0)
            {
                return webResponse.OK("无需删除相关数据");
            }

            try
            {
                // 使用EF Core 7的BatchDelete批量删除缺料方案队列
                var poolDeleteCount = repository.DbContext.Set<OCP_LackMtrlPool>()
                    .Where(x => computeIds.Contains(x.ComputeID))
                    .ExecuteDelete();

                Logger.Info($"批量删除缺料方案队列记录 {poolDeleteCount} 条");

                // 使用EF Core 7的BatchDelete批量删除缺料运算结果
                var resultDeleteCount = repository.DbContext.Set<OCP_LackMtrlResult>()
                    .Where(x => computeIds.Contains(x.ComputeID ?? 0))
                    .ExecuteDelete();

                Logger.Info($"批量删除缺料运算结果记录 {resultDeleteCount} 条");

                Logger.Info($"成功删除缺料方案相关数据，运算ID：{string.Join("、", computeIds)}");
                return webResponse.OK($"删除成功，同时清理了 {poolDeleteCount} 条队列记录和 {resultDeleteCount} 条运算结果记录");
            }
            catch (Exception ex)
            {
                Logger.Error(HDPro.Core.Enums.LoggerType.Error, $"删除缺料方案相关数据异常：{ex.Message}", ex.ToString());
                return webResponse.Error($"删除相关数据失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="keys">删除的行的主键</param>
        /// <param name="delList">删除时是否将明细也删除</param>
        /// <returns></returns>
        public override WebResponseContent Del(object[] keys, bool delList = true)
        {
            //删除前可以做一些查询判断
            //将keys转换为表的主键类型一致，用于下面的查询
            var ids = keys.Select(s => Convert.ToInt64(s)).ToList();

            // 检查是否存在运算未完成的方案
            var uncompletedPlans = repository.FindAsIQueryable(x => ids.Contains(x.ComputeID))
                .Join(repository.DbContext.Set<OCP_LackMtrlPool>(),
                      plan => plan.ComputeID,
                      pool => pool.ComputeID,
                      (plan, pool) => new { plan.ComputeID, plan.PlanName, pool.Status })
                .Where(x => x.Status != 2) // Status != 2 表示运算未完成
                .Select(x => new { x.ComputeID, x.PlanName })
                .Distinct()
                .ToList();

            if (uncompletedPlans.Count > 0)
            {
                var planNames = string.Join("、", uncompletedPlans.Select(p => p.PlanName));
                return webResponse.Error($"无法删除运算未完成的方案：{planNames}。只有运算完成后的方案才能删除。");
            }

            //删除后处理 - 删除相关的队列和运算结果
            DelOnExecuted = _keys =>
            {
                try
                {
                    // _keys就是computeIds，直接转换为long类型
                    var computeIds = _keys.Select(s => Convert.ToInt64(s)).ToList();

                    if (computeIds.Count > 0)
                    {
                        return DeleteRelatedDataByComputeIds(computeIds);
                    }

                    return webResponse.OK();
                }
                catch (Exception ex)
                {
                    Logger.Error(HDPro.Core.Enums.LoggerType.Error, $"删除缺料方案后处理异常：{ex.Message}", ex.ToString());
                    return webResponse.Error($"删除相关数据失败：{ex.Message}");
                }
            };

            return base.Del(keys, delList);
        }
    }
}