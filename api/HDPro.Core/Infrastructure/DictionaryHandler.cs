using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDPro.Core.Configuration;
using HDPro.Core.Const;
using HDPro.Core.DBManager;
using HDPro.Core.Enums;
using HDPro.Core.ManageUser;
using HDPro.Core.Tenancy;
using HDPro.Core.UserManager;
using HDPro.Entity.DomainModels;

namespace HDPro.Core.Infrastructure
{
    public static class DictionaryHandler
    {
        /*2020.05.01增加根据用户信息加载字典数据源sql*/

        /// <summary>
        /// 获取自定义数据源sql
        /// </summary>
        /// <param name="dicNo"></param>
        /// <param name="originalSql"></param>
        /// <returns></returns>
        public static string GetCustomDBSql(string dicNo, string originalSql)
        {
            switch (dicNo)
            {
                //case "字典编号":
                //    //获取数据权限查询
                //    string sql = DBServerProvider.DbContext.Set<表>().Where(x => true)
                //          .CreateTenancyFilter<表>().ToQueryString();
                //    originalSql = $"select 字段 as [key],字段 as value from ({sql}) as t ";
                //    break;
                case "部门级联":
                    originalSql = GetDeptSql(originalSql);
                    break;
                case "岗位":
                    originalSql = GetPostSql(originalSql);
                    break;
                //2020.05.24增加绑定table表时，获取所有的角色列表
                //注意，如果是2020.05.24之前获取的数据库脚本
                //请在菜单【下拉框绑定设置】添加一个字典编号【t_roles】,除了字典编号，其他内容随便填写
                case "roles":
                case "t_roles":
                case "tree_roles":
                    originalSql = GetRolesSql(originalSql);
                    break;
                case "集团":

                    if (IsPgSql)
                    {
                        originalSql = "SELECT \"GroupId\" AS key,\"GroupName\" AS value FROM  PUBLIC.\"Sys_Group\"";
                    }
                    break;
                default:
                    break;
            }
            return originalSql;
        }

        private static bool IsPgSql
        {
            get { return DBType.Name == DbCurrentType.PgSql.ToString(); }
        }
        private static bool IsOracle
        {
            get { return DBType.Name == DbCurrentType.Oracle.ToString(); }
        }

        /// <summary>
        /// 获取解决的数据源，只能看到自己与下级所有角色
        /// </summary>
        /// <param name="context"></param>
        /// <param name="originalSql"></param>
        /// <returns></returns>
        public static string GetRolesSql(string originalSql)
        {
            if (IsPgSql)
            {
                return GetRolesPgSql(originalSql);
            }
            if (IsOracle)
            {
                return GetRolesOralce(originalSql);
            }
            originalSql = "SELECT Role_Id AS id,parentId,Role_Id AS  'key',RoleName AS value FROM Sys_Role where 1=1 ";
            if (UserContext.Current.IsSuperAdmin)
            {
                return originalSql;
            }

            var roleIds = UserContext.Current.GetAllChildrenRoleIds();
            string sql = $@" {originalSql}  and  Role_Id in ({string.Join(',', roleIds)})";
            return sql;
        }
        public static string GetRolesOralce(string originalSql)
        {
            originalSql = "SELECT Role_Id AS \"id\",ParentId as  \"parentId\",Role_Id AS  \"key\",RoleName AS \"value\" FROM SYS_ROLE where 1=1 ";
            if (UserContext.Current.IsSuperAdmin)
            {
                return originalSql;
            }

            var roleIds = UserContext.Current.GetAllChildrenRoleIds();
            string sql = $" {originalSql}  and  Role_Id in ({string.Join(',', roleIds)})";
            return sql;
        }
        public static string GetRolesPgSql(string originalSql)
        {
            originalSql = "SELECT \"Role_Id\" AS id,\"ParentId\" as  \"parentId\",\"Role_Id\" AS  key,\"RoleName\" AS value FROM \"public\".\"Sys_Role\" where 1=1 ";
            if (UserContext.Current.IsSuperAdmin)
            {
                return originalSql;
            }

            var roleIds = UserContext.Current.GetAllChildrenRoleIds();
            string sql = $" {originalSql}  and  \"Role_Id\" in ({string.Join(',', roleIds)})";
            return sql;
        }

        /// <summary>
        /// 岗位数据源
        /// </summary>
        /// <param name="originalSql"></param>
        /// <returns></returns>
        public static string GetPostSql(string originalSql)
        {
            if ((DBType.Name == "MySql" || DBType.Name == "MsSql") && AppSetting.UseDynamicShareDB)
            {
                originalSql = $"SELECT PostId AS id,PostId AS 'key',ParentId AS parentId,PostName AS 'value' FROM Sys_Post where DbServiceId='{UserContext.CurrentServiceId}'";
            }
            //其他数据库自己完善下
            return originalSql;
        }

        /// <summary>
        /// 部门数据源
        /// </summary>
        /// <param name="originalSql"></param>
        /// <returns></returns>
        public static string GetDeptSql(string originalSql)
        {
            if (IsPgSql)
            {
                originalSql = "SELECT \"DepartmentId\" AS id,\"DepartmentId\" AS key,\"ParentId\" AS \"parentId\",\"DepartmentName\" AS value FROM PUBLIC.\"Sys_Department\" ";
            }
            else if (IsOracle)
            {
                originalSql = "SELECT DEPARTMENTID AS \"id\",DEPARTMENTID AS \"key\",PARENTID as \"parentId\",DEPARTMENTNAME as \"value\" FROM SYS_DEPARTMENT ";
            }
            else
            {
                originalSql = "SELECT DepartmentId AS id,DepartmentId AS 'key',ParentId AS parentId,DepartmentName AS value FROM Sys_Department";
            }
            if (UserContext.Current.IsSuperAdmin)
            {
                return originalSql;
            }
            var deptIds = UserContext.Current.DeptIds;
            deptIds = DepartmentContext.GetAllChildrenIds(deptIds);

            switch (DBType.Name)
            {
                //mysql如果端口不是3306，这里也需要修改
                case "MySql":
                    originalSql = $@"{originalSql}
                           WHERE DepartmentId in ('{string.Join("','", deptIds)}')";
                    break;
                case "PgSql":
                    originalSql = $"{originalSql}  WHERE \"DepartmentId\" in ('{string.Join("','", deptIds)}')";
                    break;
                case "MsSql":
                    originalSql = $@"{originalSql}
                           WHERE DepartmentId in ('{string.Join("','", deptIds)}')";
                    break;
                case "Oracle":
                    originalSql = $@"{originalSql}
                           WHERE DEPARTMENTID in ('{string.Join("','", deptIds)}')";
                    break;
            }
            return originalSql;
        }

    }
}
