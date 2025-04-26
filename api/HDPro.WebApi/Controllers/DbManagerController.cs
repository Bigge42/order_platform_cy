using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HDPro.Core.CacheManager;
using HDPro.Core.Configuration;
using HDPro.Core.DBManager;
using HDPro.Core.Extensions;
using HDPro.Core.Filters;
using HDPro.Core.ManageUser;
using HDPro.Core.Utilities;
using HDPro.Entity.DomainModels;

namespace HDPro.WebApi.Controllers
{

    [JWTAuthorize, ApiController]
    [Route("api/db")]
    public class DbManagerController : Controller
    {
        [Route("exectue"), HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ApiActionPermission(ActionRolePermission.SuperAdmin)]
        public async Task<ActionResult> Exectue([FromBody] TextInfo info)
        {
            if (!AppSetting.UseDynamicShareDB)
            {
                return Content($"只有动态分库才能执行脚本");
            }
            List<Task> tasks = new List<Task>();
            ConcurrentBag<string> result = new ConcurrentBag<string>();

            var list = DbCache.GetList().Select(s => new
            {
                s.DatabaseName,
                DbServiceName = s.DbServiceName,
                ConnectionString = DbCache.GetConnectionString(s)
            }).ToList();

            if (list.Count == 0)  
            {
                return Content($"没有配置数据库");
            }
            var db = DbCache.GetList().First().Serialize().DeserializeObject<Sys_DbService>();

            db.DatabaseName = "DB_Empty";
            //如果数据库都不在同一台服务器上，每个服务器都要有一个DB_Empty数据库,这里也应该也要根据ip去重循环，待完
            
            list.Add(new
            {
                db.DatabaseName,
                DbServiceName = "空数据库",
                ConnectionString = DbCache.GetConnectionString(db)
            });

            int errorCount = 0;
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                tasks.Add(Task.Run(() =>
                {
                    string text = $"{(DateTime.Now.ToString("yyyy-MM-dd HH:mm:sss"))},实例：{item.DbServiceName},数据库:{item.DatabaseName},";
                    try
                    {
                        DBServerProvider.GetSqlDapper(item.ConnectionString).SetTimeout(600).ExcuteNonQuery(info.Text, null);
                        text = text + "执行成功";
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        text = text + "执行失败," + ex.Message + ex.StackTrace;
                    }
                    result.Add(text);
                }));
                if (tasks.Count == 10)
                {
                    await Task.WhenAll(tasks);
                    tasks.Clear();
                }
            }
            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
            }
            result.Add($" --------{(DateTime.Now.ToString("yyyy-MM-dd HH:mm:sss"))}({UserContext.Current.UserTrueName})------------ ");
            result.Add($"执行sql({UserContext.Current.UserTrueName})：{info.Text}");
            result.Add("==========================================");
            result.Add("\r\n");
            FileHelper.WriteFile("DbLogger//DbExecute".MapPath(), DateTime.Now.ToString("yyyy-MM-dd") + ".txt", string.Join("\r\n", result), true);

            return Content($"执行成功:{list.Count - errorCount}个,失败：{errorCount}个");
        }
    }

    public class TextInfo
    {
        public string Text { get; set; }

    }
}