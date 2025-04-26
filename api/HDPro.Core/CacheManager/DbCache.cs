using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HDPro.Core.Configuration;
using HDPro.Core.Const;
using HDPro.Core.DBManager;
using HDPro.Core.EFDbContext;
using HDPro.Core.Utilities;
using HDPro.Entity.DomainModels;

namespace HDPro.Core.CacheManager
{
    public static class DbCache
    {
        private static List<Sys_DbService> DbServices = null;
        private static object _lock_sbcnew = new object();


        public static void Init()
        {
            using (var dbContext = new SysDbContext())
            {
                //string sql = dbContext.Set<Sys_DbService>().ToQueryString();
                //DbServices = DBServerProvider.SqlMain.QueryList<Sys_DbService>(sql, new { });
                DbServices = dbContext.Set<Sys_DbService>().Select(s => new
                {
                    s.Pwd,
                    s.DbIpAddress,
                    s.DatabaseName,
                    s.DbServiceId,
                    s.UserId,
                    s.Remark,
                    s.PhoneNo,
                    s.DbServiceName,
                    s.GroupId
                })
                    //.Where(x => x.DatabaseName != null && x.DbIpAddress != null && x.UserId != null && x.Pwd != null)
                    .ToList()
                    .Select(s => new Sys_DbService()
                    {
                        Pwd = s.Pwd,
                        DbIpAddress = s.DbIpAddress,
                        DatabaseName = s.DatabaseName,
                        DbServiceName = s.DbServiceName,
                        DbServiceId = s.DbServiceId,
                        GroupId = s.GroupId,
                        UserId = s.UserId,
                        Remark = s.Remark,
                        PhoneNo = s.PhoneNo
                    }).ToList();
                InitConnection();
            }

        }
        public static List<Sys_DbService> GetList()
        {
            return DbServices;
        }

        public static WebResponseContent Reload(WebResponseContent webResponse)
        {
            if (webResponse.Status)
            {
                Init();
            }
            return webResponse;
        }

        public static void InitConnection()
        {
            foreach (var item in DbServices)
            {
                InitConnection(item);
            }
        }

        public static string InitConnection(Sys_DbService item, string databaseName = null)
        {
            string connectionString = GetConnectionString(item, databaseName);

            if (databaseName == null)
            {
                DBServerProvider.SetConnection(item.DbServiceId.ToString(), connectionString);
            }
            return connectionString;
        }

        public static string GetConnectionString(Sys_DbService item, string databaseName = null)
        {
            string connectionString = null;
            switch (DBType.Name)
            {
                //mysql如果端口不是3306，这里也需要修改
                case "MySql":
                    connectionString = @$" Data Source={item.DbIpAddress};Database={databaseName ?? item.DatabaseName};AllowLoadLocalInfile=true;User ID={item.UserId};Password={item.Pwd};allowPublicKeyRetrieval=true;pooling=true;CharSet=utf8;port=3306;sslmode=none;";
                    break;
                case "PgSql":
                    connectionString = $"Host={item.DbIpAddress};Port=5432;User id={item.UserId};password={item.Pwd};Database={databaseName ?? item.DatabaseName};";

                    break;
                case "MsSql":
                    connectionString = @$"Data Source={item.DbIpAddress};Initial Catalog={databaseName ?? item.DatabaseName};Persist Security Info=True;User ID={item.UserId};Password={item.Pwd};Connect Timeout=500;Max Pool Size = 512;Encrypt=True;TrustServerCertificate=True;";

                    break;
                case "Oracle":
                    connectionString = @$"user id={databaseName ?? item.DatabaseName};data source={item.DbIpAddress};password={item.Pwd};";
                    break;
            }
            return connectionString;
        }


        public static Sys_DbService GetDbInfo(Guid dbServiceId)
        {
            return DbServices.Where(x => x.DbServiceId == dbServiceId).FirstOrDefault();
        }

        public static IEnumerable<Sys_DbService> GetDbInfo(Func<Sys_DbService, bool> where)
        {
            return DbServices.Where(where);
        }


    }
}
