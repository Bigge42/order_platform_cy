using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HDPro.Core.BaseProvider;
using HDPro.Core.Configuration;
using HDPro.Core.EFDbContext;
using HDPro.Core.Enums;
using static Dapper.SqlMapper;

namespace HDPro.Core.Extensions
{
    public static class DbContextExtension
    {

        public static int Update<TSource>(this BaseDbContext dbContext, TSource entity, string[] properties, bool saveChanges = false) where TSource : class
        {
            return dbContext.UpdateRange<TSource>(new List<TSource>() { entity }, properties, saveChanges);
        }
        public static int Update<TSource>(this BaseDbContext dbContext, TSource entity, bool saveChanges = false) where TSource : class
        {
            return dbContext.UpdateRange<TSource>(new List<TSource>() { entity }, new string[0], saveChanges);
        }
        public static int UpdateRange<TSource>(this BaseDbContext dbContext, IEnumerable<TSource> entities, Expression<Func<TSource, object>> properties, bool saveChanges = false) where TSource : class
        {
            return dbContext.UpdateRange<TSource>(entities, properties?.GetExpressionProperty(), saveChanges);
        }
        public static int UpdateRange<TSource>(this BaseDbContext dbContext, IEnumerable<TSource> entities, bool saveChanges = false) where TSource : class
        {
            return dbContext.UpdateRange<TSource>(entities, new string[0], saveChanges);
        }


        public static int UpdateRange<TSource>(this BaseDbContext dbContext, IEnumerable<TSource> entities, string[] properties, bool saveChanges = false) where TSource : class
        {
            if (properties != null && properties.Length > 0)
            {
                PropertyInfo[] entityProperty = typeof(TSource).GetProperties()
                        .Where(x => x.GetCustomAttribute<NotMappedAttribute>() == null).ToArray();
                string keyName = entityProperty.GetKeyName();
                if (properties.Contains(keyName))
                {
                    properties = properties.Where(x => x != keyName).ToArray();
                }
                properties = properties.Where(x => entityProperty.Select(s => s.Name).Contains(x)).ToArray();
            }
            foreach (TSource item in entities)
            {
                if (properties == null || properties.Length == 0)
                {
                    dbContext.Entry<TSource>(item).State = EntityState.Modified;
                    continue;
                }
                var entry = dbContext.Entry(item);
                properties.ToList().ForEach(x =>
                {
                    entry.Property(x).IsModified = true;
                });
            }
            if (!saveChanges)
            {
                return 0;
            }
            else {
                dbContext.SaveChanges();
            }
            return entities.Count();
        }


        /// <summary>
        /// 通过主键批量删除
        /// </summary>
        /// <param name="keys">主键key</param>
        /// <param name="delList">是否连明细一起删除</param>
        /// <returns></returns>
        public static int DeleteWithKeys<T>(this BaseDbContext dbContext, object[] keys, bool saveChange = false) where T : class
        {
            var keyPro = typeof(T).GetKeyProperty();
            foreach (var key in keys.Distinct())
            {
                T entity = Activator.CreateInstance<T>();
                keyPro.SetValue(entity, key.ChangeType(keyPro.PropertyType));
                dbContext.Entry<T>(entity).State = EntityState.Deleted;
            }
            if (saveChange)
            {
                dbContext.SaveChanges();
            }
            return keys.Length;
        }

        public static int Delete<T>(this BaseDbContext dbContext, [NotNull] Expression<Func<T, bool>> wheres, bool saveChange = false) where T : class
        {
            var keyProperty = typeof(T).GetKeyProperty();
            string keyName = typeof(T).GetKeyProperty().Name;
            var expression = keyName.GetExpression<T, object>();
            var ids = dbContext.Set<T>().Where(wheres).Select(expression).ToList();
            List<T> list = new List<T>();
            foreach (var id in ids)
            {
                T entity = Activator.CreateInstance<T>();
                keyProperty.SetValue(entity, id);
                list.Add(entity);
            }
            dbContext.RemoveRange(list);
            if (saveChange)
            {
                return dbContext.SaveChanges();
            }
            return 0;
        }
        /// <summary>
        /// 过滤逻辑删除 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<T> FilterLogicDel<T>(this IQueryable<T> query) where T:class
        {
            if (!string.IsNullOrEmpty(AppSetting.LogicDelField))
            {
                if (typeof(T).GetProperty(AppSetting.LogicDelField) != null)
                {
                    var expression = AppSetting.LogicDelField.CreateExpression<T>((int)DelStatus.正常, LinqExpressionType.Equal);
                    return query.Where(expression);
                }
            }
            return query;
        }
    }
}
