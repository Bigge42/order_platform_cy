/*
 *所有关于WZ_OrderCycleBase类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*WZ_OrderCycleBaseService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using HDPro.CY.Order.IRepositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HDPro.CY.Order.Services
{
    public partial class WZ_OrderCycleBaseService
    {
        private readonly IWZ_OrderCycleBaseRepository _repository;//访问数据库
        private readonly IOCP_OrderTrackingRepository _orderTrackingRepository;
        private readonly IOCP_MaterialRepository _materialRepository;

        [ActivatorUtilitiesConstructor]
        public WZ_OrderCycleBaseService(
            IWZ_OrderCycleBaseRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            IOCP_OrderTrackingRepository orderTrackingRepository,
            IOCP_MaterialRepository materialRepository
            )
        : base(dbRepository, httpContextAccessor)
        {
            _repository = dbRepository;
            _orderTrackingRepository = orderTrackingRepository;
            _materialRepository = materialRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        /// <summary>
        /// 重写CY.Order项目特有的初始化逻辑
        /// 可在此处添加WZ_OrderCycleBase特有的初始化代码
        /// </summary>
        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
            // 在此处添加WZ_OrderCycleBase特有的初始化逻辑
        }

        /// <summary>
        /// 重写CY.Order项目通用数据验证方法
        /// 可在此处添加WZ_OrderCycleBase特有的数据验证逻辑
        /// </summary>
        /// <param name="entity">要验证的实体</param>
        /// <returns>验证结果</returns>
        protected override WebResponseContent ValidateCYOrderEntity(WZ_OrderCycleBase entity)
        {
            var response = base.ValidateCYOrderEntity(entity);

            // 在此处添加WZ_OrderCycleBase特有的数据验证逻辑

            return response;
        }

        /// <summary>
        /// 从订单跟踪表同步数据到订单周期基础表
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>同步的总行数</returns>
        public async Task<int> SyncFromOrderTrackingAsync(CancellationToken cancellationToken = default)
        {
            var orderTrackingContext = _orderTrackingRepository?.DbContext
                ?? throw new InvalidOperationException("订单跟踪仓储未正确初始化");
            var materialContext = _materialRepository?.DbContext
                ?? throw new InvalidOperationException("物料仓储未正确初始化");
            var orderCycleContext = _repository?.DbContext
                ?? throw new InvalidOperationException("订单周期仓储未正确初始化");

            var orderTrackingList = await orderTrackingContext.Set<OCP_OrderTracking>()
                .AsNoTracking()
                .Where(p => p.PrdScheduleDate == null)
                .ToListAsync(cancellationToken);

            if (orderTrackingList.Count == 0)
            {
                return 0;
            }

            var materialNumbers = orderTrackingList.Where(p => !string.IsNullOrWhiteSpace(p.MaterialNumber))
                .Select(p => p.MaterialNumber)
                .Distinct()
                .ToList();

            var materialDict = materialNumbers.Count == 0
                ? new Dictionary<string, OCP_Material>(StringComparer.OrdinalIgnoreCase)
                : (await materialContext.Set<OCP_Material>()
                    .AsNoTracking()
                    .Where(p => materialNumbers.Contains(p.MaterialCode))
                    .ToListAsync(cancellationToken))
                    .GroupBy(p => p.MaterialCode ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var salesOrderNos = orderTrackingList.Where(p => !string.IsNullOrWhiteSpace(p.SOBillNo))
                .Select(p => p.SOBillNo)
                .Distinct()
                .ToList();

            var planTrackingNos = orderTrackingList.Where(p => !string.IsNullOrWhiteSpace(p.MtoNo))
                .Select(p => p.MtoNo)
                .Distinct()
                .ToList();

            var existingRecords = await orderCycleContext.Set<WZ_OrderCycleBase>()
                .Where(p => salesOrderNos.Contains(p.SalesOrderNo) && planTrackingNos.Contains(p.PlanTrackingNo))
                .ToListAsync(cancellationToken);

            var existingDict = new Dictionary<string, WZ_OrderCycleBase>(StringComparer.OrdinalIgnoreCase);
            foreach (var record in existingRecords)
            {
                var key = $"{record.SalesOrderNo}__{record.PlanTrackingNo}";
                if (existingDict.ContainsKey(key))
                {
                    continue;
                }

                existingDict[key] = record;
            }

            var toInsert = new List<WZ_OrderCycleBase>();
            var updatedCount = 0;

            foreach (var orderTracking in orderTrackingList)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (orderTracking == null || string.IsNullOrWhiteSpace(orderTracking.SOBillNo) || string.IsNullOrWhiteSpace(orderTracking.MtoNo))
                {
                    continue;
                }

                materialDict.TryGetValue(orderTracking.MaterialNumber ?? string.Empty, out var materialInfo);

                var key = $"{orderTracking.SOBillNo}__{orderTracking.MtoNo}";

                if (existingDict.TryGetValue(key, out var existing))
                {
                    MapFields(orderTracking, materialInfo, existing);
                    updatedCount++;
                    continue;
                }

                var newEntity = new WZ_OrderCycleBase();
                MapFields(orderTracking, materialInfo, newEntity);
                toInsert.Add(newEntity);
            }

            if (toInsert.Count > 0)
            {
                _repository.AddRange(toInsert);
            }

            if (updatedCount == 0 && toInsert.Count == 0)
            {
                return 0;
            }

            await orderCycleContext.SaveChangesAsync(cancellationToken);

            return updatedCount + toInsert.Count;
        }

        private static void MapFields(OCP_OrderTracking orderTracking, OCP_Material materialInfo, WZ_OrderCycleBase target)
        {
            target.SalesOrderNo = orderTracking.SOBillNo;
            target.PlanTrackingNo = orderTracking.MtoNo;

            target.OrderApprovedDate = orderTracking.OrderAuditDate;
            target.ReplyDeliveryDate = orderTracking.ReplyDeliveryDate;
            target.RequestedDeliveryDate = orderTracking.DeliveryDate;
            target.MaterialCode = orderTracking.MaterialNumber;

            if (materialInfo != null)
            {
                target.InnerMaterial = materialInfo.Material;
                target.FlangeConnection = materialInfo.FlangeConnection;
                target.BonnetForm = materialInfo.PackingForm;
                target.FlowCharacteristic = materialInfo.FlowCharacteristic;
                target.Actuator = materialInfo.ActuatorModel;
                target.SealFaceForm = materialInfo.DrawingNo;
                target.ProductName = materialInfo.ProductModel;
                target.NominalDiameter = materialInfo.NominalDiameter;
                target.NominalPressure = materialInfo.NominalPressure;
            }
        }
    }
}