/*
 *所有关于Demo_Order类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*Demo_OrderService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using System.Diagnostics.CodeAnalysis;

namespace HDPro.DbTest.Services
{
    public partial class Demo_OrderService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDemo_OrderRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public Demo_OrderService(
            IDemo_OrderRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        public override PageGridData<Demo_Order> GetPageData(PageDataOptions options)
        {
            QueryRelativeExpression = (IQueryable<Demo_Order> queryable) =>
            {
                if (options.Value != null)
                {
                    switch (options.Value.GetInt())
                    {
                        //新订单
                        case 1:
                            queryable = queryable = queryable.Where(c => c.OrderType == 1);
                            break;
                        //采购订单
                        case 2:
                            queryable = queryable = queryable.Where(c => c.OrderType == 2);
                            break;
                        //退货订单
                        case 3:
                            queryable = queryable = queryable.Where(c => c.OrderType == 3);
                            break;
                        default:
                            break;
                    }
                }
                // queryable = queryable = queryable.Where(c => c.OrderType == options.Value.GetInt());

                //当前用户只能操作自己(与下级角色)创建的数据,如:查询、删除、修改等操作
                //IQueryable<int> userQuery = RoleContext.GetCurrentAllChildUser();
                //queryable = queryable.Where(x => x.CreateID == UserContext.Current.UserId || userQuery.Contains(x.CreateID ?? 0));
                return queryable;
            };


            //查询table界面显示求和
            SummaryExpress = (IQueryable<Demo_Order> queryable) =>
            {
                return queryable.GroupBy(x => 1).Select(x => new
                {
                    //注意大小写和数据库字段大小写一样
                    TotalPrice = x.Sum(o => o.TotalPrice),
                    TotalQty = x.Sum(o => o.TotalQty)
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
            return (queryeable as IQueryable<Demo_OrderList>).GroupBy(x => 1).Select(x => new
            {
                //Weight/Qty注意大小写和数据库字段大小写一样
                Price = x.Average(o => o.Price),
                Qty = x.Sum(o => o.Qty)
            }).ToList().FirstOrDefault();
        }

        public override WebResponseContent Update(SaveModel saveModel)
        {
            return base.Update(saveModel);
        }

        public override WebResponseContent Add(SaveModel saveDataModel)
        {
            saveDataModel.MainData["OrderNo"] = "111";

            WebResponseContent webResponse = new WebResponseContent();
            // 在保存数据库前的操作，所有数据都验证通过了，这一步执行完就执行数据库保存
            AddOnExecuting = (Demo_Order order, object list) =>
            {
                //生成订单号
               // order.OrderNo = order.Create<Demo_Order>(x => x.OrderNo, "D", x => x.CreateDate);
                return webResponse.OK();
            };

            return base.Add(saveDataModel);
        }

        ///// <summary>
        ///// 自动生成订单号
        ///// </summary>
        ///// <returns></returns>
        //public string GetOrderNo()
        //{
        //    //lock/redis自增
        //    DateTime dateNow = (DateTime)DateTime.Now.ToString("yyyy-MM-dd").GetDateTime();
        //    //查询当天最新的订单号
        //    string orderNo = repository.FindAsIQueryable(x => x.CreateDate > dateNow)
        //        .OrderByDescending(x => x.OrderNo)
        //        .Select(s => s.OrderNo)
        //        .FirstOrDefault();
        //    string rule = $"D{DateTime.Now.ToString("yyyyMMdd")}";
        //    if (string.IsNullOrEmpty(orderNo))
        //    {
        //        rule += "00001";
        //    }
        //    else
        //    {
        //        rule += (orderNo.Substring(orderNo.Length - 5).GetInt() + 1).ToString("00000");
        //    }
        //    return rule;
        //}

    }
}

