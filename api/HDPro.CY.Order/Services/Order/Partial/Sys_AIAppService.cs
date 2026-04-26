using HDPro.CY.Order.IRepositories;
using HDPro.Core.Utilities;
using HDPro.Entity.DomainModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace HDPro.CY.Order.Services
{
    public partial class Sys_AIAppService
    {
        private readonly ISys_AIAppRepository _repository;

        [ActivatorUtilitiesConstructor]
        public Sys_AIAppService(
            ISys_AIAppRepository dbRepository,
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

        protected override WebResponseContent ValidateCYOrderEntity(Sys_AIApp entity)
        {
            var response = base.ValidateCYOrderEntity(entity);
            if (!response.Status)
            {
                return response;
            }

            if (entity.SortNo < 0)
            {
                return response.Error("排序号不能小于0");
            }

            if (entity.Status != 0 && entity.Status != 1)
            {
                return response.Error("状态只能为启用或停用");
            }

            return response;
        }
    }
}
