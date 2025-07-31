/*
 *所有关于OCP_SOProgress类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*OCP_SOProgressService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using HDPro.CY.Order.Services;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;
using System.Linq;
using HDPro.Core.Utilities;
using HDPro.Core.Extensions;
using Microsoft.AspNetCore.Http;
using HDPro.CY.Order.IRepositories;
using HDPro.Core.ManageUser;
using Microsoft.Extensions.DependencyInjection;

namespace HDPro.CY.Order.Services
{
    public partial class OCP_SOProgressService
    {
        private readonly IOCP_SOProgressRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public OCP_SOProgressService(
            IOCP_SOProgressRepository dbRepository,
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
        /// 可在此处添加OCP_SOProgress特有的初始化代码
        /// </summary>
        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
            // 在此处添加OCP_SOProgress特有的初始化逻辑
        }

        /// <summary>
        /// 重写CY.Order项目通用数据验证方法
        /// 可在此处添加OCP_SOProgress特有的数据验证逻辑
        /// </summary>
        /// <param name="entity">要验证的实体</param>
        /// <returns>验证结果</returns>
        protected override WebResponseContent ValidateCYOrderEntity(OCP_SOProgress entity)
        {
            var response = base.ValidateCYOrderEntity(entity);

            // 在此处添加OCP_SOProgress特有的数据验证逻辑

            return response;
        }
         public override PageGridData<OCP_SOProgress> GetPageData(PageDataOptions options)
            {

                QueryRelativeExpression = (IQueryable<OCP_SOProgress> queryable) =>
                {
                    // 权限过滤：当前用户与销售负责人一致
                    var currentUser = UserContext.Current;

                    // 超级管理员或特定角色（角色Id=41）可以查看所有数据
                    if (!currentUser.IsSuperAdmin && !currentUser.RoleIds.Contains(41))
                    {
                        var currentUserName = currentUser.UserName;
                        var currentUserTrueName = currentUser.UserTrueName;

                        // 只显示当前用户作为销售负责人的订单
                        // 支持用户名和真实姓名两种匹配方式
                        queryable = queryable.Where(x =>
                            x.SalesPerson == currentUserName ||
                            x.SalesPerson == currentUserTrueName ); // 如果销售负责人为空，也显示（兼容性考虑）
                    }

                    //明细表一起查询返回
                    //queryable=queryable.Include(x=>x.明细表);//sugar后台改为queryable=queryable.Includes(x=>x.明细表);

                //查看生成的sql语句：Console.Write(queryable.ToQueryString())
                    return queryable;
                };



                var res= base.GetPageData(options);
                //这里可以自定义返回内容，前端searchAfter(rows,res)的第二个参数获取返回的数据
                //res.extra = new { 自定义值=123};
                return res;
            }

  }
} 