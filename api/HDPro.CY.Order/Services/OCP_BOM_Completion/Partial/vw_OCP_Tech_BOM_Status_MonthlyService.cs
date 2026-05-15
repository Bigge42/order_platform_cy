using HDPro.Core.Utilities;
using HDPro.CY.Order.IRepositories;
using HDPro.Entity.DomainModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HDPro.CY.Order.Services
{
    public partial class vw_OCP_Tech_BOM_Status_MonthlyService
    {
        private readonly Ivw_OCP_Tech_BOM_Status_MonthlyRepository _repository;

        [ActivatorUtilitiesConstructor]
        public vw_OCP_Tech_BOM_Status_MonthlyService(
            Ivw_OCP_Tech_BOM_Status_MonthlyRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
            )
        : base(dbRepository, httpContextAccessor)
        {
            _repository = dbRepository;
        }

        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
        }

        protected override WebResponseContent ValidateCYOrderEntity(vw_OCP_Tech_BOM_Status_Monthly entity)
        {
            return base.ValidateCYOrderEntity(entity);
        }

        public override PageGridData<vw_OCP_Tech_BOM_Status_Monthly> GetPageData(PageDataOptions options)
        {
            QueryRelativeList = (List<SearchParameters> parameters) =>
            {
                if (parameters == null)
                {
                    return;
                }

                bool hasAuditDateFilter = parameters.Any(p =>
                    string.Equals(p.Name, nameof(vw_OCP_Tech_BOM_Status_Monthly.OrderAuditDate), StringComparison.OrdinalIgnoreCase));

                if (hasAuditDateFilter)
                {
                    return;
                }

                DateTime start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                DateTime end = start.AddMonths(1).AddSeconds(-1);
                parameters.Add(new SearchParameters
                {
                    Name = nameof(vw_OCP_Tech_BOM_Status_Monthly.OrderAuditDate),
                    Value = start.ToString("yyyy-MM-dd"),
                    DisplayType = "thanorequal"
                });
                parameters.Add(new SearchParameters
                {
                    Name = nameof(vw_OCP_Tech_BOM_Status_Monthly.OrderAuditDate),
                    Value = end.ToString("yyyy-MM-dd HH:mm:ss"),
                    DisplayType = "lessorequal"
                });
            };

            return base.GetPageData(options);
        }

        public async Task<WebResponseContent> SyncOrderDatesAsync()
        {
            var response = new WebResponseContent();
            const string sql = @";WITH LatestOrderDates AS
(
    SELECT
        ot.SOBillNo,
        ot.OrderAuditDate,
        ot.OrderCreateDate,
        rn = ROW_NUMBER() OVER
        (
            PARTITION BY ot.SOBillNo
            ORDER BY
                ISNULL(ot.OrderAuditDate, CONVERT(DATETIME, '19000101', 112)) DESC,
                ISNULL(ot.OrderCreateDate, CONVERT(DATETIME, '19000101', 112)) DESC
        )
    FROM dbo.OCP_OrderTracking AS ot
)
UPDATE tm
SET
    tm.OrderAuditDate = ot.OrderAuditDate,
    tm.OrderCreateDate = ot.OrderCreateDate
FROM dbo.OCP_TechManagement AS tm
JOIN LatestOrderDates AS ot
    ON ot.SOBillNo = tm.SOBillNo
   AND ot.rn = 1
WHERE ISNULL(tm.OrderAuditDate, CONVERT(DATETIME, '19000101', 112)) <> ISNULL(ot.OrderAuditDate, CONVERT(DATETIME, '19000101', 112))
   OR ISNULL(tm.OrderCreateDate, CONVERT(DATETIME, '19000101', 112)) <> ISNULL(ot.OrderCreateDate, CONVERT(DATETIME, '19000101', 112));";

            int affectedRows = await _repository.DbContext.Database.ExecuteSqlRawAsync(sql);
            return response.OK("Order dates synchronized.", new { affectedRows }, false);
        }

        public async Task<WebResponseContent> EnqueueMissingBomCreatorsAsync()
        {
            var response = new WebResponseContent();
            const string sql = "EXEC dbo.usp_OCP_EnqueueMissingTCBomCreatorTasks @BatchLimit = NULL;";
            await _repository.DbContext.Database.ExecuteSqlRawAsync(sql);
            return response.OK("Missing BOM creator tasks enqueued.", null, false);
        }
    }
}
