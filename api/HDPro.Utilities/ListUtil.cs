using HDPro.Utilities.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HDPro.Utilities
{
    public static class ListUtil
    {



        public static DataTable ConvertToDataTable<T>(ICollection<T> elementList)
        {
            DataTable dtDataSource = new DataTable();

            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo item in properties)
            {
                dtDataSource.Columns.Add(item.Name, item.PropertyType);
            }

            foreach (T elementItem in elementList)
            {
                DataRow drItem = dtDataSource.NewRow();
                foreach (PropertyInfo item in properties)
                {
                    drItem[item.Name] = item.GetValue(elementItem, null);
                }
                dtDataSource.Rows.Add(drItem);
            }
            return dtDataSource;
        }

        public static ICollection<T> ConvertFromDataTable<T>(DataTable dtDataSource)
            where T : new()
        {
            ICollection<T> elementList = new List<T>();
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (DataRow drItem in dtDataSource.Rows)
            {
                T item = new T();
                foreach (PropertyInfo property in properties)
                {
                    property.SetValue(item, drItem[property.Name], null);
                }
                elementList.Add(item);
            }
            return elementList;
        }

      

        public static bool IsEmpty(this string s1)
        {
            return string.IsNullOrEmpty(s1);
        }

        /// <summary>
        /// 判断集合元素个数是否大于N，优化性能
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="elements"></param>
        /// <param name="count"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static bool IsGreaterThan<T>(this IEnumerable<T> elements, int count, Predicate<T> condition = null)
        {
            if (elements == null)
            {
                return count < 0;
            }
            int flag = 0;
            foreach (var el in elements)
            {
                if (condition == null || condition.Invoke(el))
                {
                    flag++;
                }
                if (flag > count) break;
            }
            return flag > count;
        }

        /// <summary>
        /// 判断集合元素个数是否小于N，优化性能
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="elements"></param>
        /// <param name="count"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static bool IsLessThan<T>(this IEnumerable<T> elements, int count, Predicate<T> condition = null)
        {
            if (elements == null)
            {
                return count > 0;
            }
            int flag = 0;
            foreach (var el in elements)
            {
                if (condition == null || condition.Invoke(el))
                {
                    flag++;
                }
                if (flag >= count) return false;
            }
            return true;
        }

        /// <summary>
        /// 将一个字符的列表集合转化为带分隔符的字符串
        /// </summary>
        /// <param name="list"></param>
        /// <param name="seporator"></param>
        /// <param name="addquot"></param>
        /// <returns></returns>
        public static string JoinEx(this IEnumerable<string> list, string seporator, bool addquot)
        {
            string str = "";
            foreach (string item in list)
            {
                if (!item.IsNullOrEmptyOrWhiteSpace())
                {
                    if (addquot)
                    {
                        str = string.Format("{0}'{1}'{2}", str, item, seporator);
                    }
                    else
                    {
                        str = string.Format("{0}{1}{2}", str, item, seporator);
                    }
                }
            }
            if (str.Length > 1)
            {
                str = str.Substring(0, str.Length - seporator.Length);
            }
            else if (addquot)
            {
                str = "' '";
            }
            return str;
        }


        /// <summary>
        /// 按指定数量对List分组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="groupNum"></param>
        /// <returns></returns>
        public static List<List<T>> GetListGroup<T>(this IEnumerable<T> list, int groupNum)
        {
            List<List<T>> listGroup = new List<List<T>>();
            for (int i = 0; i < list.Count(); i += groupNum)
            {
                listGroup.Add(list.Skip(i).Take(groupNum).ToList());
            }
            return listGroup;
        }

    }
}
