/*
 *所有关于OCP_PrdMODetail类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*OCP_PrdMODetailService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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

namespace HDPro.CY.Order.Services
{
    public partial class OCP_PrdMODetailService
    {
        private readonly IOCP_PrdMODetailRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public OCP_PrdMODetailService(
            IOCP_PrdMODetailRepository dbRepository,
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
        /// 可在此处添加OCP_PrdMODetail特有的初始化代码
        /// </summary>
        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
            // 在此处添加OCP_PrdMODetail特有的初始化逻辑
        }

        /// <summary>
        /// 重写CY.Order项目通用数据验证方法
        /// 可在此处添加OCP_PrdMODetail特有的数据验证逻辑
        /// </summary>
        /// <param name="entity">要验证的实体</param>
        /// <returns>验证结果</returns>
        protected override WebResponseContent ValidateCYOrderEntity(OCP_PrdMODetail entity)
        {
            var response = base.ValidateCYOrderEntity(entity);
            
            // 在此处添加OCP_PrdMODetail特有的数据验证逻辑
            
            return response;
        }

        //查询(主表合计)
        public override PageGridData<OCP_PrdMODetail> GetPageData(PageDataOptions options)
        {
            //EF:查询table界面显示合计（需要与前端开发文档上的【table显示合计】一起使用）
            SummaryExpress = (IQueryable<OCP_PrdMODetail> queryable) =>
            {
                return queryable.GroupBy(x => 1).Select(x => new
                {
                    //注意大小写和数据库字段大小写一样
                    PlanQty = x.Sum(o => o.PlanQty ?? 0),
                    InboundQty = x.Sum(o => o.InboundQty ?? 0),
                    UnInboundQty = x.Sum(o => o.UnInboundQty ?? 0),
                    OverdueQty = x.Sum(o => o.OverdueQty ?? 0)
                })
                .FirstOrDefault();
            };
            return base.GetPageData(options);
        }
  }
} 