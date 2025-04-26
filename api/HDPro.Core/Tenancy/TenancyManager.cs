using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using HDPro.Core.Const;
using HDPro.Core.DBManager;
using HDPro.Core.Enums;
using HDPro.Core.ManageUser;
using HDPro.Core.UserManager;
using HDPro.Entity.DomainModels;

namespace HDPro.Core.Tenancy
{
    public static class TenancyManager<T> where T : class
    {
        /// <summary>
        /// 数据隔离操作(2023.08.16优化增加了queryable参数可以写EF查询，及增加了返回参数)
        /// 注意(必看)：数据库表字段必须包括appsettings.json配置文件中的CreateMember->UserIdField创建人id字段才会进行数据隔离。
        /// 如果表没有这些字段，请在下面 switch (tableName)单独写过滤逻辑
        /// </summary>
        /// <param name="tableName">数据库表名</param>
        /// <returns></returns>
        public static (string sql, IQueryable<T> query) GetSearchQueryable(string multiTenancyString, string tableName, IQueryable<T> queryable)
        {

            //超级管理员不限制(这里可以根据tableName表名自己判断要不要限制超级管理员)
            if (UserContext.Current.IsSuperAdmin)
            {
                return (multiTenancyString, queryable);
            }

            switch (tableName)
            {
                //例如：指定用户表指定查询条件

                //自定义用户表过滤仅显示当前租户下的所有用户
                //case nameof(Sys_User):
                //    //  获取当前租户下用户的所有角色
                //    var roleIds = RoleContext.GetRoles(x => x.DbServiceId == UserContext.CurrentServiceId)
                //        //   超级管理员不限制角色
                //        .Where(x => UserContext.Current.IsSuperAdmin ? true : UserContext.Current.RoleIds.Contains(x.Id))
                //        .Select(s => s.Id).ToArray();
                //    // 获取用户的所有子角色
                //    roleIds = RoleContext.GetAllChildren(roleIds).Where(x=>x.DbServiceId== UserContext.CurrentServiceId).Select(s=>s.Id).ToArray();
                //    var roleQuery = DBServerProvider.DbContext.Set<Sys_UserRole>().Where(x => x.Enable == 1 && roleIds.Contains(x.RoleId));//&& x.RoleId > 1 
                //    queryable = (IQueryable<T>)(queryable as IQueryable<Sys_User>).Where(c => roleQuery.Any(uid => c.User_Id == uid.UserId));
                //    break;
                case nameof(Sys_Role):
                    break;
                case nameof(Sys_Group):
                    break;
                case nameof(Sys_Department):
                    //   case nameof(Demo_Order):
                    ////例：订单管理只看自己角色及子角色对应用户创建的数据
                    ////注：下面的Sys_UserRole表存的是每个角色对应有哪些用户,Sys_UserRole不包括超级管理员RoleId=1的用户

                    /*************************方式一：用EF查询***********************************/
                    ////1、 获取当前登录帐的角色及子角色
                    //var roleIds = RoleContext.GetAllChildrenIds(UserContext.Current.RoleIds);
                    ////2、查询这些角色对应的用户
                    //var userIdsQuery = DBServerProvider.DbContext.Set<Sys_UserRole>().Where(x => roleIds.Contains(x.RoleId) && x.Enable == 1).Select(s => s.UserId);

                    ////上面1、2的操作可以简化， RoleContext.GetCurrentAllChildUser()已经封装了获取当前登录帐号角色下的所有用户id方法
                    ////userIdsQuery = RoleContext.GetCurrentAllChildUser();

                    ////3、进行子查询
                    //queryable = (IQueryable<T>)(queryable as IQueryable<Demo_Order>).Where(c => userIdsQuery.Any(uid => uid == c.CreateID));

                    ////4、queryable.ToQueryString()可以查看实际生成的sql
                    ////Console.WriteLine(queryable.ToQueryString());
                    ///

                    /*************************方式二：写原生sql查询***********************************/
                    //multiTenancyString = $" select * from {tableName}  where CreateID in (select UserId from  Sys_UserRole where RoleId in ({string.Join(",", roleIds)}))";


                    /********注：上面的两种方式，如果熟悉表达式语法，尽量采用第一种方式；也可以采用第二种方式写原生sql,数据过滤的规则由自己定义********/

                    //*************************方式三：写原生sql查询，某些表只能查看自己的数据***********************************/
                    // multiTenancyString += $" select * from {tableName} where CreateID='{UserContext.Current.UserId}'";


                    break;
                default:
                    //1、其他表默认执行数据隔离,隔离方式与角色管理页面的[数据权限]：
                    //2、注：角色设置数据权限后就会进行数据隔离，如果不需要隔离的数据，见上面switch (tableName)说明
                    //3、隔离方式：本组织(部门)及下数据、本组织(部门)数据、本角色以及下数据、本角色数据、仅自己数据


                    //统一执行数据隔离
                    // 注意(必看)：数据库表字段必须包括appsettings.json配置文件中的CreateMember->UserIdField创建人id字段才会进行数据隔离。
                    // 如果表没有这些字段，请在上面 switch (tableName)单独写过滤逻辑
                    queryable = queryable.CreateTenancyFilter<T>();
                    break;
            }
            return (multiTenancyString, queryable);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName">数据库表名</param>
        /// <param name="ids">当前操作的所有id</param>
        /// <param name="tableKey">主键字段</param>
        /// <returns></returns>
        public static string GetMultiTenancySql(string tableName, string ids, string tableKey)
        {
            //使用方法同上
            string multiTenancyString;
            switch (tableName)
            {
                default:
                    multiTenancyString = $"select count(*) FROM {tableName} " +
                       $" where CreateID='{UserContext.Current.UserId}'" +
                       $" and  {tableKey} in ({ids}) ";
                    break;
            }
            return multiTenancyString;
        }
    }



}
