using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using HDPro.Core.Configuration;
using HDPro.Core.DBManager;
using HDPro.Core.Enums;
using HDPro.Core.Extensions;
using HDPro.Core.ManageUser;
using HDPro.Core.UserManager;
using HDPro.Entity.DomainModels;
using HDPro.Entity.SystemModels;

namespace HDPro.Core.Tenancy
{
    public static class TenancyExpression
    {
        /// <summary>
        /// 获取数据权限sql
        /// 调用方式：DBServerProvider.DbContext.Set<表>().CreateTenancyFilterSql();
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static string CreateTenancyFilterSql<T>(this IQueryable<T> query) where T : class
        {
            return query.CreateTenancyFilter<T>().ToQueryString();
        }
        /// <summary>
        /// 注意：数据库表中必须包括appsettings.json配置文件UserIdField的字段才会进行数据隔离。如果表没有这些字段，请在上面单独写过滤逻辑
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        ///  <param name="checkAdmin">是否对管理员帐号也做验证</param>
        /// <returns></returns>
        public static IQueryable<T> CreateTenancyFilter<T>(this IQueryable<T> query) where T : class
        {
            //2024.07.04增加租户不分库统一过滤处理
            // query = query.FilterTenancy();

            //if (AppSetting.TenancyField != null
            //    && typeof(T).GetProperty(AppSetting.TenancyField) != null
            //    && !string.IsNullOrEmpty(UserContext.Current.UserInfo.TenancyValue))
            //{
            //    query = query.Where(AppSetting.TenancyField.CreateExpression<T>(UserContext.Current.UserInfo.TenancyValue, LinqExpressionType.Equal));
            //}

            //是否用户表
            bool isUserTable = typeof(T) == typeof(Sys_User);

            //默认通过创建人id过滤数据
            string filterCreateId = null;
            //用户表通过user_id过滤数据
            if (isUserTable)
            {
                filterCreateId = "User_Id";
            }
            else
            {
                //获取表的创建人id字段，在配置appsettings文件中UserIdField值
                var properties = typeof(T).GetProperties();
                //使用创建人id过滤数据
                filterCreateId = properties.Where(x => x.Name == AppSetting.CreateMember.UserIdField).FirstOrDefault()?.Name;
                if (filterCreateId == null)
                {
                    filterCreateId = properties.Where(x => x.Name == "CreateId").FirstOrDefault()?.Name;
                }
                //没有配置创建人id的表不执行数据权限过滤
                if (filterCreateId == null)
                {
                    return query;
                }
            }


            string tableName = typeof(T).Name;

            //使用用户数据权限(用户管理界面配置的指定看到某些用户创建的数据库)
            if (AppSetting.UserAuth)
            {
                int[] userIds = UserContext.Current.GetCurrentUserAuthUserIds(tableName);
                //设置查看指定用户的数据
                if (userIds != null && userIds.Length > 0)
                {
                    return query.Where(filterCreateId.CreateExpression<T>(userIds, LinqExpressionType.In));
                }
            }
            var roleIds = UserContext.Current.RoleIds;

            //2024.08.11增加菜单数据权限(优先级高级角色数据权限)
            List<int> authDataTypes = null;
            string authMenuData = UserContext.Current.GetPermissions(tableName.ToLower())?.AuthMenuData;
            if (!string.IsNullOrEmpty(authMenuData))
            {
                authDataTypes = new List<int>() { authMenuData.GetInt() };
            }
            else
            {
                authDataTypes = RoleContext.GetRoles(x => roleIds.Contains(x.Id))
                   .Where(x => x.AuthData > 0)
                   .Select(s => s.AuthData).ToList();
            }

            if (!isUserTable)
            {
                if (authDataTypes.Count == 0)
                {
                    return query;
                }
            }
            //!!不要给超级管理员设置部门，否则可能会被组织权限共享显示出来
            if (!isUserTable && (authDataTypes.Contains((int)AuthData.本组织及下数据) || authDataTypes.Contains((int)AuthData.本组织数据)))
            {
                var deptIds = UserContext.Current.DeptIds;
                var userDeptQuery = DBServerProvider.DbContext.Set<Sys_UserDepartment>().Where(x => x.Enable == 1);
                if (authDataTypes.Contains((int)AuthData.本组织及下数据))
                {
                    var childDeptIds = DepartmentContext.GetAllChildrenIds(deptIds);
                    userDeptQuery = userDeptQuery.Where(x => childDeptIds.Contains(x.DepartmentId));
                }
                else
                {
                    userDeptQuery = userDeptQuery.Where(x => deptIds.Contains(x.DepartmentId));
                }

                //分库
                if (CheckDb<T>())
                {
                    var userIds = userDeptQuery.Select(s => s.UserId).Distinct();
                    query = query.QueryTenancyDynamicShareDBFilter<T>(filterCreateId, userIds);
                }
                else
                {
                    query = query.QueryTenancyFilter<T, Sys_UserDepartment>(filterCreateId, userDeptQuery, "UserId");
                }
                return query;
            }
            //如果角色没有配置数据权限，当前页面是isUserTable=true用户表时，默认显示当前角色下的数据
            if (isUserTable || authDataTypes.Contains((int)AuthData.本角色以及下数据) || authDataTypes.Contains((int)AuthData.本角色数据))
            {
                var userRoleQuery = DBServerProvider.DbContext.Set<Sys_UserRole>().Where(x => x.Enable == 1 && x.RoleId > 1);
                if (isUserTable || authDataTypes.Contains((int)AuthData.本角色以及下数据))
                {
                    //获取所有子角色
                    var childRoleIds = RoleContext.GetAllChildrenIds(roleIds);
                    userRoleQuery = userRoleQuery.Where(x => childRoleIds.Contains(x.RoleId));
                }
                else
                {
                    userRoleQuery = userRoleQuery.Where(x => roleIds.Contains(x.RoleId));
                }
                //分库
                if (CheckDb<T>())
                {
                    var userIds = userRoleQuery.Select(s => s.UserId).Distinct();
                    query = query.QueryTenancyDynamicShareDBFilter<T>(filterCreateId, userIds);
                }
                else
                {
                    query = query.QueryTenancyFilter<T, Sys_UserRole>(filterCreateId, userRoleQuery, "UserId");
                }
                return query;
            }
            if (authDataTypes.Contains((int)AuthData.仅自己数据))
            {
                return query.Where(filterCreateId.CreateExpression<T>(UserContext.Current.UserId, LinqExpressionType.Equal));
            }
            return query;
        }

        private static bool CheckDb<T>()
        {
            //是否使用分库
            return AppSetting.UseDynamicShareDB || typeof(T).BaseType.Name != typeof(SysEntity).Name;
        }
        private static IQueryable<T1> QueryTenancyDynamicShareDBFilter<T1>(this IQueryable<T1> query, string createIdField, IQueryable<int> userIds)
        {
            return query.Where(createIdField.CreateExpression<T1>(userIds.Take(5000).ToArray(), LinqExpressionType.In));
        }
        private static IQueryable<T1> QueryTenancyFilter<T1, T2>(this IQueryable<T1> query, string createIdField, IQueryable<T2> subQuery, string userIdField)
        {
            Type t1 = typeof(T1);
            Type t2 = typeof(T2);
            // 创建参数表达式
            ParameterExpression x = Expression.Parameter(t1, "x");
            ParameterExpression c = Expression.Parameter(t2, "c");

            // 构建内部Any条件表达式：c => c.User_Id == x.CreateID

            MemberExpression t1Member = t1.GetProperty(createIdField).PropertyType == typeof(int)
                ? Expression.Property(x, createIdField)
                : Expression.Property(Expression.Property(x, createIdField), "value");

            MemberExpression t2Member = t2.GetProperty(userIdField).PropertyType == typeof(int)
                ? Expression.Property(c, userIdField)
                : Expression.Property(Expression.Property(c, userIdField), "value");

            Expression innerCondition = Expression.Equal(
                t1Member, t2Member
            // //Expression.Property(x, "User_Id")
            // Expression.Property(Expression.Property(x, createIdField), "value"),
            //// Expression.Property(x, "CreateID"),
            //Expression.Property(Expression.Property(c, userIdField), "value")
            );

            // 构建外部Where条件表达式：x => role.Any(c => c.Role_Id == x.User_Id)
            Expression outerCondition = Expression.Call(
                typeof(Queryable),
                "Any",
                new[] { t2 },
                subQuery.Expression,
                Expression.Lambda<Func<T2, bool>>(innerCondition, c)
            );

            // 构建最终的查询表达式：user.Where(x => role.Any(c => c.Role_Id == x.User_Id))
            Expression<Func<T1, bool>> lambda = Expression.Lambda<Func<T1, bool>>(outerCondition, x);

            query = query.Where(lambda);

            return query;

        }
    }
}
