/*
*所有关于WZ_OrderCycleBase类的业务代码接口应在此处编写
*/
using HDPro.Core.BaseProvider;
using HDPro.Entity.DomainModels;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using HDPro.CY.Order.Models.OrderCycleBaseDtos;
namespace HDPro.CY.Order.IServices
{
    public partial interface IWZ_OrderCycleBaseService
    {
        Task<int> SyncFromOrderTrackingAsync(CancellationToken cancellationToken = default);

        Task<int> BatchCallValveRuleServiceAsync(List<OrderRuleDto> items, CancellationToken cancellationToken = default);
    }
 }
