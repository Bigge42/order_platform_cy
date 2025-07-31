using Jint;
using Newtonsoft.Json.Linq;
using HDPro.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HDPro.Utilities
{
    public static  class CommonUtils
    {
        public static bool IsPropertyExist(dynamic data, string propertyname)
        {
            if (data is ExpandoObject)
                return ((IDictionary<string, object>)data).ContainsKey(propertyname);
            return data.GetType().GetProperty(propertyname) != null;
        }

        /// <summary>
        /// 用于解析变量 a 在范围内：(0,2800]，解析结果 a<=2800 && a>0
        /// </summary>
        /// <param name="varName">变量 a</param>
        /// <param name="valueScope">值范围(0,2800]</param>
        /// <returns> a<=2800 && a>0</returns>
        public static string SplitValueScope(this string varName, string valueScope)
        {
            if (string.IsNullOrWhiteSpace(valueScope))
            {
                return "";
            }
            List<string> statements = new List<string>();
            var valueArr = valueScope.Split(',');
            if (valueArr.Length == 2)
            {
                if (valueArr[0].Length >=0)
                {
                    var a = '(';
                    if(valueArr[0].Length>0)
                    {
                        a = valueArr[0][0];
                    }
                    var aNumber = valueArr[0].Substring(1);
                    if(aNumber.IsNullOrEmptyOrWhiteSpace())
                    {
                        aNumber = "0";
                    }
                    if (a == '[')
                    {
                        statements.Add($"{varName}>={aNumber}");
                    }
                    else
                    {
                        statements.Add($"{varName}>{aNumber}");
                    }
                }
                if (valueArr[1].Length > 0)
                {
                    var b = ')';
                    if (valueArr[1].Length > 0)
                    {
                        b = valueArr[1][valueArr[1].Length - 1];
                    }
                    var bNumber = valueArr[1].Substring(0, valueArr[1].Length - 1);
                    if(bNumber.IsNullOrEmptyOrWhiteSpace())
                    {
                        bNumber = int.MaxValue.ToString();
                    }
                    if (b == ']')
                    {
                        statements.Add($"{varName}<={bNumber}");
                    }
                    else
                    {
                        statements.Add($"{varName}<{bNumber}");
                    }
                }
            }
            return String.Join("&&", statements);
        }

        /// <summary>
        /// 用于解析变量 a 在范围内：(0,2800]，解析结果 a<=2800 && a>0
        /// </summary>
        /// <param name="varName">变量 a</param>
        /// <param name="valueScope">值范围(0,2800]</param>
        /// <returns> bool </returns>
        public static bool SplitValueScope(decimal number, string valueScope)
        {
            bool flag =false;
            if (valueScope.IsNullOrEmptyOrWhiteSpace())
            {
                return flag;
            }
            List<string> statements = new List<string>();
            var valueArr = valueScope.Split(',');
            int flagInt = 0;
            if (valueArr.Length == 2)
            {
                if (valueArr[0].Length >= 0)
                {
                    var a = '(';
                    if (valueArr[0].Length > 0)
                    {
                        a = valueArr[0][0];
                    }
                    var aNumber = valueArr[0].Substring(1);
                    if (aNumber.IsNullOrEmptyOrWhiteSpace())
                    {
                        aNumber = "0";
                    }
                    if (a == '[')
                    {
                        if(number >= Convert.ToDecimal(aNumber))
                        {
                            flagInt += 1;
                        }
                        
                    }
                    else
                    {
                        if (number > Convert.ToDecimal(aNumber))
                        {
                            flagInt += 1;
                        }
                    }
                }
                if (valueArr[1].Length > 0)
                {
                    var b = ')';
                    if (valueArr[1].Length > 0)
                    {
                        b = valueArr[1][valueArr[1].Length - 1];
                    }
                    var bNumber = valueArr[1].Substring(0, valueArr[1].Length - 1);
                    if (bNumber.IsNullOrEmptyOrWhiteSpace())
                    {
                        bNumber = int.MaxValue.ToString();
                    }
                    if (b == ']')
                    {
                        if (number <= Convert.ToDecimal(bNumber))
                        {
                            flagInt += 1;
                        }
                    }
                    else
                    {
                        if (number < Convert.ToDecimal(bNumber))
                        {
                            flagInt += 1;
                        }
                    }
                }
            }
            if (flagInt==2)
            {
                flag = true;
            }
            return flag;
        }

        public static string GetSize(params decimal[] list)
        {
            var tmpList = list;
            if (list.Any(p => p != 0))
            {
                tmpList = list.Where(p => p > 0).ToArray();
            }
            return string.Join("*", tmpList.Select(p=>p.TrimEndZero()));
        }
        /// <summary>
        /// 向下取整
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string GetFLoorSize(params decimal[] list)
        {
            var tmpList = list;
            if (list.Any(p => p != 0))
            {
                tmpList = list.Where(p => p > 0).ToArray();
            }
            return string.Join("*", tmpList.Select(p => Math.Floor(p).TrimEndZero()));
        }
        /// <summary>
        /// 向下取整
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string GetFLoorSizeWithZero(params decimal[] list)
        {
            var tmpList = list;
        
            return string.Join("*", tmpList.Select(p => Math.Floor(p).TrimEndZero()));
        }
        public static string GetRemark(string 详单备注, string 说明)
        {
            bool flag = 详单备注.IsNullOrEmptyOrWhiteSpace();
            if (flag)
            {
                详单备注 = "";
            }
            bool flag2 = !说明.IsNullOrEmptyOrWhiteSpace();
            if (flag2)
            {
                bool flag3 = 详单备注.IsNullOrEmptyOrWhiteSpace();
                if (flag3)
                {
                    详单备注 = 说明;
                }
                else
                {
                    详单备注 = 详单备注 + "," + 说明;
                }
            }
            return 详单备注;
        }
        public static void AddOrUpdate(this JObject dicVars, string key, JToken value)
        {
            if (dicVars.ContainsKey(key))
            {
                dicVars[key] = value;
            }
            else
            {
                dicVars.Add(key, value);
            }
        }

        /// <summary>
        /// 将对象转为字典
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ignorePropertyNames">忽略的属性名</param>
        /// <returns></returns>
        public static Dictionary<string, object> ToDictionary(this object obj,List<string> ignorePropertyNames)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            if (obj != null)
            {
                PropertyInfo[] properties = obj.GetType().GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    if (ignorePropertyNames == null || !ignorePropertyNames.Contains(property.Name))
                    {
                        dict[property.Name] = property.GetValue(obj);
                    }
                }
            }

            return dict;
        }
        public static bool IsInRange(this decimal c, string s)
        {
            var attr = s.Split(',');
            decimal startValue = decimal.Parse(attr[0]);
            decimal endValue = decimal.Parse(attr[1]);
            return c >= startValue && c <= endValue;
        }

        public static List<IDictionary<string,object>> ToDictionaryList(dynamic dynObj)
        {
            var list = new List<IDictionary<string, object>>();
            foreach (var item in dynObj)
            {
                if (item is IDictionary<string, object>)
                {
                    list.Add(item);
                }
            }
            return list;
        }
    }
}
