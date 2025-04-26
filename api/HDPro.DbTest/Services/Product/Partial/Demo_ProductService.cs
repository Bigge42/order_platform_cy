/*
 *所有关于Demo_Product类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*Demo_ProductService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using HDPro.DbTest.IRepositories;

namespace HDPro.DbTest.Services
{
    public partial class Demo_ProductService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDemo_ProductRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public Demo_ProductService(
            IDemo_ProductRepository dbRepository,
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
        /// 主表设置合计
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public override PageGridData<Demo_Product> GetPageData(PageDataOptions options)
        {
            //查询table界面显示求和
            SummaryExpress = (IQueryable<Demo_Product> queryable) =>
            {
                return queryable.GroupBy(x => 1).Select(x => new
                {
                    //注意大小写和数据库字段大小写一样
                    Price = x.Sum(o => o.Price)
                })
                .ToList().FirstOrDefault();
            };

            return base.GetPageData(options);
        }


        /// <summary>
        /// 设置弹出框明细表的合计信息
        /// </summary>
        /// <typeparam name="detail"></typeparam>
        /// <param name="queryeable"></param>
        /// <returns></returns> 
        protected override object GetDetailSummary<Detail>(IQueryable<Detail> queryeable)
        {
            //判断是哪个明细表
            if (typeof(Detail) ==typeof(Demo_ProductColor))
            {
                //转换为明细表
                return (queryeable as IQueryable<Demo_ProductColor>).GroupBy(x => 1).Select(x => new
                {
                    //Qty注意大小写和数据库字段大小写一样
                //    Qty = x.Average(o => o.Qty)
                }).ToList().FirstOrDefault();
            }
            return null;
        }

    }
}
