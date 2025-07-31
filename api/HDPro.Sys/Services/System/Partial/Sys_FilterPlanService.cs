/*
 *所有关于Sys_FilterPlan类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*Sys_FilterPlanService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using HDPro.Core.BaseProvider;
using HDPro.Core.DBManager;
using HDPro.Core.Extensions;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Core.Utilities;
using HDPro.Entity.DomainModels;
using HDPro.Entity.DomainModels.System.dto;
using HDPro.Sys.IRepositories;
using HDPro.Utilities.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Threading.Tasks;

namespace HDPro.Sys.Services
{
    public partial class Sys_FilterPlanService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISys_FilterPlanRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public Sys_FilterPlanService(
            ISys_FilterPlanRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        public async Task<WebResponseContent> AddOrUpdatePlan(System_FilterPlanInputDto plan)
        {
            if (plan.BillName.IsNullOrEmpty() || plan.Name.IsNullOrEmpty())
            {
                return WebResponseContent.Instance.Error("方案名称为空!");
            }

            if (plan.ID <= 0 || plan.ID.IsNullOrEmptyOrWhiteSpace())
            {
                var service = await AddPlan(plan);
                return service;
            }
            else
            {
                var service = await UpdatePlan(plan);
                return service;
            }
        }


        private async Task<WebResponseContent> AddPlan(System_FilterPlanInputDto plan)
        {
            var userInfo = Core.ManageUser.UserContext.Current.UserInfo;
            if (userInfo == null)
            {
                return WebResponseContent.Instance.OK("无自定义方案！", null);
            }
            string userId = userInfo.User_Id.ToString();
            Sys_FilterPlan filterPlan = new Sys_FilterPlan
            {
                BillName = plan.BillName,
                Name = plan.Name,
                Content = plan.Content,
                UserIds = userId,
                IsDefault = 0,
                IsSystem = 0,
            };
            filterPlan.SetCreateDefaultVal();
            DBServerProvider.DbContext.Add(filterPlan);
            DBServerProvider.DbContext.SaveChanges();
            return WebResponseContent.Instance.OK("新增自定义过滤方案成功！", null);
        }


        public async Task<WebResponseContent> GetPlan(string BillName)
        {
            var userInfo = Core.ManageUser.UserContext.Current.UserInfo;
            if (userInfo == null || BillName.IsNullOrEmptyOrWhiteSpace())
            {
                return WebResponseContent.Instance.OK("无自定义方案！", null);
            }
            string userId = userInfo.User_Id.ToString();
            List<Sys_FilterPlan> planList = await DBServerProvider.DbContext
                .Set<Sys_FilterPlan>()
                .Where(p => (p.UserIds.Contains(userId.ToString())) && p.BillName==BillName)
                .OrderByDescending(p => p.CreateDate)
                .ToListAsync();

            return WebResponseContent.Instance.OK("自定义过滤方案获取成功！", planList);
        }

        public async Task<WebResponseContent> DeletePlan(long id)
        {
            var userInfo = Core.ManageUser.UserContext.Current.UserInfo;
            if (userInfo == null)
            {
                return WebResponseContent.Instance.OK("自定义过滤方案获取成功！", null);
            }
            string userId = userInfo.User_Id.ToString();
            Sys_FilterPlan plan = await DBServerProvider.DbContext
                .Set<Sys_FilterPlan>()
                .FirstOrDefaultAsync(p => p.ID == id && p.UserIds.Contains(userId));
            if (plan == null)
            {
                return WebResponseContent.Instance.Error("方案不存在或无权限删除!");
            }
            DBServerProvider.DbContext.Remove(plan);
            await DBServerProvider.DbContext.SaveChangesAsync();
            return WebResponseContent.Instance.OK("方案删除成功!");
        }
        private async Task<WebResponseContent> UpdatePlan(System_FilterPlanInputDto plan)
        {
            if (plan.ID <= 0)
            {
                return WebResponseContent.Instance.Error("方案ID无效!");
            }
            var userInfo = Core.ManageUser.UserContext.Current.UserInfo;
            if (userInfo == null)
            {
                return WebResponseContent.Instance.OK("自定义过滤方案获取成功！", null);
            }
            string userId = userInfo.User_Id.ToString();
            Sys_FilterPlan existingPlan = await DBServerProvider.DbContext
                .Set<Sys_FilterPlan>()
                .FirstOrDefaultAsync(p => p.ID == plan.ID && p.UserIds.Contains(userId));
            if (existingPlan == null)
            {
                return WebResponseContent.Instance.Error("方案不存在或无权限修改!");
            }
            existingPlan.BillName = plan.BillName;
            existingPlan.Name = plan.Name;
            existingPlan.Content = plan.Content;
            DBServerProvider.DbContext.Update(existingPlan);
            await DBServerProvider.DbContext.SaveChangesAsync();
            return WebResponseContent.Instance.OK("方案更新成功!");
        }
    }
}
