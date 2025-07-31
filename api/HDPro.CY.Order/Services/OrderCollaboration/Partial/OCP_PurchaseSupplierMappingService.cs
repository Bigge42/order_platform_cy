/*
 *所有关于OCP_PurchaseSupplierMapping类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*OCP_PurchaseSupplierMappingService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using HDPro.CY.Order.Models;
using HDPro.CY.Order.Services.OrderCollaboration.Common;
using System.Threading.Tasks;
using System;

namespace HDPro.CY.Order.Services
{
    public partial class OCP_PurchaseSupplierMappingService
    {
        private readonly IOCP_PurchaseSupplierMappingRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public OCP_PurchaseSupplierMappingService(
            IOCP_PurchaseSupplierMappingRepository dbRepository,
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
        /// 可在此处添加OCP_PurchaseSupplierMapping特有的初始化代码
        /// </summary>
        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
            // 在此处添加OCP_PurchaseSupplierMapping特有的初始化逻辑
        }

        /// <summary>
        /// 重写CY.Order项目通用数据验证方法
        /// 可在此处添加OCP_PurchaseSupplierMapping特有的数据验证逻辑
        /// </summary>
        /// <param name="entity">要验证的实体</param>
        /// <returns>验证结果</returns>
        protected override WebResponseContent ValidateCYOrderEntity(OCP_PurchaseSupplierMapping entity)
        {
            var response = base.ValidateCYOrderEntity(entity);
            
            // 在此处添加OCP_PurchaseSupplierMapping特有的数据验证逻辑
            
            return response;
        }

        /// <summary>
        /// 根据业务类型和业务主键获取供应商负责人信息
        /// </summary>
        /// <param name="request">查询请求</param>
        /// <returns>供应商负责人信息</returns>
        public async Task<WebResponseContent> GetSupplierResponsibleAsync(GetSupplierResponsibleRequest request)
        {
            var response = new WebResponseContent();
            
            try
            {
                if (request == null)
                {
                    return response.Error("请求参数不能为空");
                }

                if (string.IsNullOrWhiteSpace(request.BusinessType))
                {
                    return response.Error("业务类型不能为空");
                }

                if (request.BusinessId <= 0)
                {
                    return response.Error("业务主键必须大于0");
                }

                var result = new SupplierResponsibleResponse
                {
                    BusinessType = request.BusinessType,
                    BusinessId = request.BusinessId,
                    QueryTime = DateTime.Now
                };

                string supplierCode = null;
                string supplierName = null;

                // 根据业务类型获取供应商编码
                if (request.BusinessType.Equals(BusinessConstants.BusinessType.Purchase, StringComparison.OrdinalIgnoreCase))
                {
                    // 从采购跟踪表获取供应商信息
                    var purchaseTrack = await _repository.DbContext.Set<OCP_POUnFinishTrack>()
                        .Where(x => x.TrackID == request.BusinessId)
                        .FirstOrDefaultAsync();
                    
                    if (purchaseTrack != null)
                    {
                        supplierCode = purchaseTrack.SupplierCode;
                        supplierName = purchaseTrack.SupplierName;
                    }
                }
                else if (request.BusinessType.Equals(BusinessConstants.BusinessType.OutSourcing, StringComparison.OrdinalIgnoreCase))
                {
                    // 从委外跟踪表获取供应商信息
                    var subOrderTrack = await _repository.DbContext.Set<OCP_SubOrderUnFinishTrack>()
                        .Where(x => x.TrackID == request.BusinessId)
                        .FirstOrDefaultAsync();
                    
                    if (subOrderTrack != null)
                    {
                        supplierCode = subOrderTrack.SupplierCode;
                        supplierName = subOrderTrack.SupplierName;
                    }
                }
                else
                {
                    return response.Error($"不支持的业务类型: {request.BusinessType}，支持的类型：PO(采购)、WW(委外)");
                }

                if (string.IsNullOrWhiteSpace(supplierCode))
                {
                    result.Remarks = $"未找到业务类型为 {request.BusinessType}，主键为 {request.BusinessId} 的记录";
                    return response.OK(null, result);
                }

                // 根据供应商编码查找供应商映射关系
                var supplierMapping = await _repository.DbContext.Set<OCP_PurchaseSupplierMapping>()
                    .Where(x => x.Code == supplierCode)
                    .FirstOrDefaultAsync();

                if (supplierMapping != null)
                {
                    // 找到供应商映射，使用映射中的负责人信息
                    result.ResponsiblePersonName = supplierMapping.BusinessPersonName;
                    result.ResponsiblePersonLoginName = supplierMapping.BusinessPersonLoginName;
                    result.HasSupplierMapping = true;
                    result.IsDefaultResponsible = false;
                }
                else
                {
                    // 未找到供应商映射，使用业务类型默认负责人
                    // 将业务类型常量转换为中文名称
                    string businessTypeName = GetBusinessTypeName(request.BusinessType);
                    
                    var businessTypeResponsible = await _repository.DbContext.Set<OCP_BusinessTypeResponsible>()
                        .Where(x => x.BusinessType == request.BusinessType)
                        .FirstOrDefaultAsync();

                    if (businessTypeResponsible != null)
                    {
                        result.ResponsiblePersonName = businessTypeResponsible.DefaultResponsibleName;
                        result.ResponsiblePersonLoginName = businessTypeResponsible.DefaultResponsibleLoginName;
                        result.HasSupplierMapping = false;
                        result.IsDefaultResponsible = true;
                        result.Remarks = $"未找到供应商的专属负责人，使用业务类型 {businessTypeName} 的默认负责人";
                    }
                    else
                    {
                        result.HasSupplierMapping = false;
                        result.IsDefaultResponsible = false;
                        result.Remarks = $"未找到供应商的专属负责人，也未找到业务类型 {businessTypeName} 的默认负责人";
                    }
                }

                return response.OK(null, result);
            }
            catch (Exception ex)
            {
                return response.Error($"获取供应商负责人信息时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 将业务类型常量转换为中文名称
        /// </summary>
        /// <param name="businessType">业务类型常量</param>
        /// <returns>业务类型中文名称</returns>
        private string GetBusinessTypeName(string businessType)
        {
            return businessType switch
            {
                BusinessConstants.BusinessType.Purchase => "采购",
                BusinessConstants.BusinessType.OutSourcing => "委外",
                BusinessConstants.BusinessType.Sales => "销售",
                BusinessConstants.BusinessType.Technology => "技术",
                BusinessConstants.BusinessType.Component => "部件",
                BusinessConstants.BusinessType.Metalwork => "金工",
                BusinessConstants.BusinessType.Assembly => "装配",
                BusinessConstants.BusinessType.Planning => "计划",
                _ => businessType ?? "未知"
            };
        }

        /// <summary>
        /// 批量获取供应商负责人信息
        /// </summary>
        /// <param name="requests">批量查询请求列表</param>
        /// <returns>批量供应商负责人信息</returns>
        public async Task<WebResponseContent> BatchGetSupplierResponsibleAsync(List<GetSupplierResponsibleRequest> requests)
        {
            var response = new WebResponseContent();
            
            try
            {
                if (requests == null || !requests.Any())
                {
                    return response.Error("请求参数不能为空");
                }

                var results = new List<SupplierResponsibleResponse>();
                var dbContext = _repository.DbContext;

                // 按业务类型分组处理，提高查询效率
                var groupedRequests = requests.GroupBy(x => x.BusinessType).ToList();

                foreach (var group in groupedRequests)
                {
                    var businessType = group.Key;
                    var groupRequests = group.ToList();
                    var businessIds = groupRequests.Select(x => x.BusinessId).ToList();

                    // 根据业务类型批量获取供应商信息
                    if (businessType.Equals(BusinessConstants.BusinessType.Purchase, StringComparison.OrdinalIgnoreCase))
                    {
                        // 批量查询采购跟踪表
                        var purchaseTracks = await dbContext.Set<OCP_POUnFinishTrack>()
                            .Where(x => x.FENTRYID!=null&&businessIds.Contains(x.FENTRYID.Value))
                            .ToListAsync();

                        await ProcessPurchaseTrackingResults(groupRequests, purchaseTracks, results, dbContext);
                    }
                    else if (businessType.Equals(BusinessConstants.BusinessType.OutSourcing, StringComparison.OrdinalIgnoreCase))
                    {
                        // 批量查询委外跟踪表
                        var subOrderTracks = await dbContext.Set<OCP_SubOrderUnFinishTrack>()
                           .Where(x => x.FENTRYID != null && businessIds.Contains(x.FENTRYID.Value))
                            .ToListAsync();

                        await ProcessSubOrderTrackingResults(groupRequests, subOrderTracks, results, dbContext);
                    }
                    else
                    {
                        // 不支持的业务类型
                        foreach (var req in groupRequests)
                        {
                            results.Add(new SupplierResponsibleResponse
                            {
                                BusinessType = req.BusinessType,
                                BusinessId = req.BusinessId,
                                QueryTime = DateTime.Now,
                                HasSupplierMapping = false,
                                IsDefaultResponsible = false,
                                Remarks = $"不支持的业务类型: {req.BusinessType}，支持的类型：PO(采购)、WW(委外)"
                            });
                        }
                    }
                }

                return response.OK("批量获取供应商负责人信息成功", results);
            }
            catch (Exception ex)
            {
                return response.Error($"批量获取供应商负责人信息时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理采购跟踪结果
        /// </summary>
        private async Task ProcessPurchaseTrackingResults(
            List<GetSupplierResponsibleRequest> requests,
            List<OCP_POUnFinishTrack> purchaseTracks,
            List<SupplierResponsibleResponse> results,
            Microsoft.EntityFrameworkCore.DbContext dbContext)
        {
            // 获取所有供应商编码
            var supplierCodes = purchaseTracks.Where(x => !string.IsNullOrWhiteSpace(x.SupplierCode))
                .Select(x => x.SupplierCode).Distinct().ToList();

            // 批量查询供应商映射
            var supplierMappings = await dbContext.Set<OCP_PurchaseSupplierMapping>()
                .Where(x => supplierCodes.Contains(x.Code))
                .ToListAsync();

            // 获取业务类型默认负责人
            var businessTypeName = GetBusinessTypeName(BusinessConstants.BusinessType.Purchase);
            var businessTypeResponsible = await dbContext.Set<OCP_BusinessTypeResponsible>()
                .Where(x => x.BusinessType == BusinessConstants.BusinessType.Purchase)
                .FirstOrDefaultAsync();

            foreach (var request in requests)
            {
                var result = new SupplierResponsibleResponse
                {
                    BusinessType = request.BusinessType,
                    BusinessId = request.BusinessId,
                    QueryTime = DateTime.Now
                };

                var purchaseTrack = purchaseTracks.FirstOrDefault(x => x.FENTRYID == request.BusinessId);
                if (purchaseTrack == null || string.IsNullOrWhiteSpace(purchaseTrack.SupplierCode))
                {
                    result.Remarks = $"未找到业务类型为 {request.BusinessType}，主键为 {request.BusinessId} 的记录";
                    result.HasSupplierMapping = false;
                    result.IsDefaultResponsible = false;
                }
                else
                {
                    var supplierMapping = supplierMappings.FirstOrDefault(x => x.Code == purchaseTrack.SupplierCode);
                    if (supplierMapping != null)
                    {
                        result.ResponsiblePersonName = supplierMapping.BusinessPersonName;
                        result.ResponsiblePersonLoginName = supplierMapping.BusinessPersonLoginName;
                        result.HasSupplierMapping = true;
                        result.IsDefaultResponsible = false;
                    }
                    else if (businessTypeResponsible != null)
                    {
                        result.ResponsiblePersonName = businessTypeResponsible.DefaultResponsibleName;
                        result.ResponsiblePersonLoginName = businessTypeResponsible.DefaultResponsibleLoginName;
                        result.HasSupplierMapping = false;
                        result.IsDefaultResponsible = true;
                        result.Remarks = $"未找到供应商的专属负责人，使用业务类型 {businessTypeName} 的默认负责人";
                    }
                    else
                    {
                        result.HasSupplierMapping = false;
                        result.IsDefaultResponsible = false;
                        result.Remarks = $"未找到供应商的专属负责人，也未找到业务类型 {businessTypeName} 的默认负责人";
                    }
                }

                results.Add(result);
            }
        }

        /// <summary>
        /// 处理委外跟踪结果
        /// </summary>
        private async Task ProcessSubOrderTrackingResults(
            List<GetSupplierResponsibleRequest> requests,
            List<OCP_SubOrderUnFinishTrack> subOrderTracks,
            List<SupplierResponsibleResponse> results,
            Microsoft.EntityFrameworkCore.DbContext dbContext)
        {
            // 获取所有供应商编码
            var supplierCodes = subOrderTracks.Where(x => !string.IsNullOrWhiteSpace(x.SupplierCode))
                .Select(x => x.SupplierCode).Distinct().ToList();

            // 批量查询供应商映射
            var supplierMappings = await dbContext.Set<OCP_PurchaseSupplierMapping>()
                .Where(x => supplierCodes.Contains(x.Code))
                .ToListAsync();

            // 获取业务类型默认负责人
            var businessTypeName = GetBusinessTypeName(BusinessConstants.BusinessType.OutSourcing);
            var businessTypeResponsible = await dbContext.Set<OCP_BusinessTypeResponsible>()
                .Where(x => x.BusinessType == BusinessConstants.BusinessType.OutSourcing)
                .FirstOrDefaultAsync();

            foreach (var request in requests)
            {
                var result = new SupplierResponsibleResponse
                {
                    BusinessType = request.BusinessType,
                    BusinessId = request.BusinessId,
                    QueryTime = DateTime.Now
                };

                var subOrderTrack = subOrderTracks.FirstOrDefault(x => x.TrackID == request.BusinessId);
                if (subOrderTrack == null || string.IsNullOrWhiteSpace(subOrderTrack.SupplierCode))
                {
                    result.Remarks = $"未找到业务类型为 {request.BusinessType}，主键为 {request.BusinessId} 的记录";
                    result.HasSupplierMapping = false;
                    result.IsDefaultResponsible = false;
                }
                else
                {
                    var supplierMapping = supplierMappings.FirstOrDefault(x => x.Code == subOrderTrack.SupplierCode);
                    if (supplierMapping != null)
                    {
                        result.ResponsiblePersonName = supplierMapping.BusinessPersonName;
                        result.ResponsiblePersonLoginName = supplierMapping.BusinessPersonLoginName;
                        result.HasSupplierMapping = true;
                        result.IsDefaultResponsible = false;
                    }
                    else if (businessTypeResponsible != null)
                    {
                        result.ResponsiblePersonName = businessTypeResponsible.DefaultResponsibleName;
                        result.ResponsiblePersonLoginName = businessTypeResponsible.DefaultResponsibleLoginName;
                        result.HasSupplierMapping = false;
                        result.IsDefaultResponsible = true;
                        result.Remarks = $"未找到供应商的专属负责人，使用业务类型 {businessTypeName} 的默认负责人";
                    }
                    else
                    {
                        result.HasSupplierMapping = false;
                        result.IsDefaultResponsible = false;
                        result.Remarks = $"未找到供应商的专属负责人，也未找到业务类型 {businessTypeName} 的默认负责人";
                    }
                }

                results.Add(result);
            }
        }
  }
} 