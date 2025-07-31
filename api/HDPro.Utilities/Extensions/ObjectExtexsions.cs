using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;
using ServiceStack.Text;
using Npoi.Mapper.Attributes;
using System.Reflection;

namespace HDPro.Utilities.Extensions
{
    public static class ObjectExtexsions
    {

        /// <summary>
        /// 判断指定的对象是否是空引用，或空字符串。
        /// </summary>
        /// <param name="value">需要测试的对象</param>
        /// <returns>如果 value 参数为空引用或空字符串 ("")，则为 true；否则为 false。</returns>
        public static bool IsNullOrEmpty(this object value)
        {
            if (value == null || value == DBNull.Value || value.ToString().Length == 0)
            {
                return true;
            }

            if (value is string)
            {
                return value.ToString().Length == 0;
            }

            if (value is IEnumerable)
            {
                foreach (var x in (value as IEnumerable))
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断指定的对象是否是空引用，或空字符串， 或空白字符。
        /// </summary>
        /// <param name="value">需要测试的对象</param>
        /// <returns>如果 value 参数为空引用或空字符串 ("")或空白字符，则为 true；否则为 false。</returns>
        public static bool IsNullOrEmptyOrWhiteSpace(this object value)
        {
            if (value == null || value == DBNull.Value || value.ToString().Length == 0 || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 为对象创建一个拷贝
        /// </summary>
        /// <param name="obj">需要创建拷贝的对象</param>
        /// <returns>新建立的拷贝对象</returns>
        public static T CreateCopy<T>(this T obj) where T : class
        {
            T clone = default(T);

            //核心点2：这里的XmlSerializer序列化器要使用实例obj的类型，不能直接使用类型T，否则会报异常(当obj实例为T类型的子类时，可重现该异常)
            //异常详细信息："ClassName":"System.InvalidOperationException","Message":"There was an error generating the XML document.","Data":null,"InnerException":{"ClassName":"System.InvalidOperationException","Message":"The type MeShopPay.View.API.Models.ConfigModel.CallBackSettingPayPal was not expected. Use the XmlInclude or SoapInclude attribute to specify types that are not known statically."
            System.Xml.Serialization.XmlSerializer tXmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream(1024))
            {
                tXmlSerializer.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                // 反序列化至另一个对象(即创建了一个原对象的深表副本)
                clone = (T)tXmlSerializer.Deserialize(ms);
            }

            return clone;
        }

        ///// <summary>
        ///// 将对象序列化为字节流
        ///// </summary>
        ///// <param name="obj">需要序列化的对象</param>
        ///// <returns>序列化后的字节流</returns>
        //public static byte[] GetObjectSream(object obj)
        //{
        //    if (obj == null)
        //    {
        //        return null;
        //    }
        //    byte[] ret = null;
        //    using (MemoryStream stmSeriBuff = new MemoryStream())
        //    {
        //        IFormatter formatter = new BinaryFormatter();

        //        //序列化.
        //        formatter.Serialize(stmSeriBuff, obj);
        //        ret = stmSeriBuff.ToArray();
        //        stmSeriBuff.Close();
        //        stmSeriBuff.Dispose();
        //    }
        //    return ret;
        //}

        ///// <summary>
        ///// 将字节流反序列化为对象
        ///// </summary>
        ///// <param name="bytes">需要反序列化的字节流</param>
        ///// <returns>反序列化后的对象</returns>
        //public static object GetObject(byte[] bytes)
        //{
        //    if (bytes == null)
        //    {
        //        return null;
        //    }
        //    object ret = null;
        //    using (MemoryStream stmSeriBuff = new MemoryStream(bytes))
        //    {
        //        IFormatter formatter = new BinaryFormatter();
        //        ret = formatter.Deserialize(stmSeriBuff);
        //        stmSeriBuff.Close();
        //        stmSeriBuff.Dispose();
        //    }
        //    return ret;
        //}

        /// <summary>
        /// 判断返回主键值是否是一个无效的值。
        /// </summary>
        /// <param name="pkValue">主键值</param>
        /// <returns>主键值是一个无效的值，则为 true；否则为 false。</returns>
        public static bool IsEmptyPrimaryKey(this object pkValue)
        {
            if (pkValue == null)
            {
                return true;
            }

            string str = pkValue as string;
            if (str != null)
            {
                //无效的输入，返回空
                return (string.IsNullOrWhiteSpace(str));
            }

            //加快判断速度，其实用下面的IsValueType判断也是可以的。
            if (pkValue is int)
            {
                return ((int)pkValue == 0);
            }

            if (pkValue is long)
            {
                return ((long)pkValue == 0);
            }

            if (pkValue.GetType().IsValueType)
            {
                object stractValue = Activator.CreateInstance(pkValue.GetType());
                return object.Equals(stractValue, pkValue);
            }
            return false;
        }
        /// <summary>
        /// 返回字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Object2String(object obj)
        {
            return obj == null ? "" : obj.ToString();
        }

        /// <summary>
        /// 得到对象对应的数值
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <param name="iDefaultValue">默认值</param>
        /// <returns>得到的int值</returns>
        public static int Object2Int(object obj, int iDefaultValue = 0)
        {
            if (obj is DBNull)
            {
                return iDefaultValue;
            }
            if (obj == null)
            {
                return iDefaultValue;
            }
            try
            {
                return Convert.ToInt32(obj);
            }
            catch
            {
                return iDefaultValue;
            }
        }
        /// <summary>
        /// 得到对象对应的数值（Decimal）
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defVal"></param>
        /// <returns></returns>
        public static decimal Object2Decimal(object obj, decimal defVal = 0.00M)
        {
            if (obj == null || string.IsNullOrWhiteSpace(obj.ToString())) return defVal;
            decimal.TryParse(obj.ToString(), out defVal);
            return defVal;
        }

        /// <summary>
        /// 将一个值对象转换成指定枚举值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objVal"></param>
        /// <param name="defVal"></param>
        /// <returns></returns>
        public static T ToEnum<T>(this object objVal, T defVal = default(T))
        {
            if (objVal.IsNullOrEmpty()) return defVal;
            if (typeof(T).IsEnum)
            {
                if (Enum.IsDefined(typeof(T), objVal))
                {
                    return (T)Enum.Parse(typeof(T), objVal.ToString(), true);
                }
                var objEnum = objVal;
                int enumVal;
                if (int.TryParse(objVal.ToString(), out enumVal))
                {
                    if (Enum.IsDefined(typeof(T), enumVal))
                    {
                        return (T)Convert.ChangeType(enumVal, typeof(int));
                    }
                }
            }
            return defVal;
        }

        /// <summary>
        /// 返回时间范围（dt ≥ t1  and dt 小于 t2）
        /// </summary>
        /// <param name="cnName"></param>
        /// <returns></returns>
        public static string[] GetDateTimesByName(string cnName)
        {
            /*
             本月： yyyy-MM-01 ~ 下个月1日零点
             上月： yyyy-上月-01 ~ 本月1日零点（yyyy-MM-01 00:00:00）
             今天： yyyy-MM-dd ~ 明天零点（yyyy-MM-dd 00:00:00）
             昨天： yyyy-MM-dd ~ 今天零点（yyyy-MM-dd 00:00:00）
             本周： Sunday为一周开始（DayOfWeek：周日 0，周六 6）
             上周： 上周日 ~ 本周日零点
             本季度：一季度（1、2、3月），...，四季度（10、11、12月）
             上季度： 根据月份所属季度、往前三个月
             本年：  yyyy-01-01 ~ 明年1月1日零点
             */
            DateTime dtNow = DateTime.Now;
            string[] Times = new string[] { dtNow.ToString("yyyy-MM-dd 00:00:00"), dtNow.ToString("yyyy-MM-dd HH:mm:ss") };
            switch (cnName)
            {
                case "本月":
                    Times[0] = dtNow.ToString("yyyy-MM-01 00:00:00");
                    Times[1] = dtNow.AddMonths(1).ToString("yyyy-MM-01 00:00:00");// 下个月1日
                    break;
                case "上月":
                    Times[0] = dtNow.AddMonths(-1).ToString("yyyy-MM-01 00:00:00");
                    Times[1] = dtNow.AddDays(1 - dtNow.Day).ToString("yyyy-MM-dd 00:00:00");//本月1日
                    break;
                case "昨天":
                    Times[0] = dtNow.AddDays(-1).ToString("yyyy-MM-dd 00:00:00");
                    Times[1] = dtNow.ToString("yyyy-MM-dd 00:00:00");//今天0点
                    break;
                case "前天":
                    Times[0] = dtNow.AddDays(-2).ToString("yyyy-MM-dd 00:00:00");
                    Times[1] = dtNow.AddDays(-1).ToString("yyyy-MM-dd 00:00:00");//昨天0点
                    break;
                case "本周":
                    Times[0] = dtNow.AddDays((int)dtNow.DayOfWeek * -1).AddDays(1).ToString("yyyy-MM-dd 00:00:00");
                    Times[1] = dtNow.AddDays((int)dtNow.DayOfWeek * -1).AddDays(7).AddDays(1).ToString("yyyy-MM-dd 00:00:00");
                    break;
                case "上周":
                    Times[0] = dtNow.AddDays(-7).AddDays((int)dtNow.DayOfWeek * -1).AddDays(1).ToString("yyyy-MM-dd 00:00:00");
                    Times[1] = dtNow.AddDays((int)dtNow.DayOfWeek * -1).AddDays(1).ToString("yyyy-MM-dd 00:00:00");
                    break;
                case "本季度":
                    Times[0] = dtNow.AddMonths(0 - ((dtNow.Month - 1) % 3)).AddDays(1 - dtNow.Day).ToString("yyyy-MM-dd 00:00:00");
                    Times[1] = dtNow.AddMonths(3 - ((dtNow.Month - 1) % 3)).AddDays(1 - dtNow.Day).ToString("yyyy-MM-dd 00:00:00");
                    break;
                case "上季度":
                    Times[0] = dtNow.AddMonths(-3 - ((dtNow.Month - 1) % 3)).AddDays(1 - dtNow.Day).ToString("yyyy-MM-dd 00:00:00");
                    Times[1] = dtNow.AddMonths(0 - ((dtNow.Month - 1) % 3)).AddDays(1 - dtNow.Day).ToString("yyyy-MM-dd 00:00:00");
                    break;
                case "本年":
                    Times[0] = dtNow.ToString("yyyy-01-01 00:00:00");
                    Times[1] = dtNow.AddYears(1).ToString("yyyy-01-01 00:00:00");
                    break;
                case "去年":
                    Times[0] = dtNow.AddYears(-1).ToString("yyyy-01-01 00:00:00");
                    Times[1] = dtNow.ToString("yyyy-01-01 00:00:00");
                    break;
                case "今天":
                    Times[1] = dtNow.AddDays(1).ToString("yyyy-MM-dd 00:00:00");
                    break;
                default:
                    break;
            }
            return Times;
        }

        /// <summary>
        /// 将对象序列为json对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJson(this object obj)
        {
            if (obj.IsNullOrEmpty()) return null;
            return StringExtensions.ToJson(obj);
        }

        /// <summary>
        /// 格式化序列化的内容
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="formatted"></param>
        /// <returns></returns>
        public static string ToJson(this object obj, bool formatted)
        {
            if (obj.IsNullOrEmpty()) return null;
            var json = StringExtensions.ToJson(obj);

            return formatted ? json.IndentJson() : json;
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="useNewtonSoft"></param>
        /// <returns></returns>
        public static T FromJson<T>(this string json, bool useNewtonSoft = false)
        {
            if (json.IsNullOrEmptyOrWhiteSpace()) return default(T);

            if (useNewtonSoft) return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            return StringExtensions.FromJson<T>(json);
        }


        /// <summary>
        /// 用Jsv方式反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T FromJsv<T>(this string obj)
        {
            return StringExtensions.FromJsv<T>(obj);
        }

        /// <summary>
        /// 以任一对象的哈希值为种子，返回一个指定长度的随机字符串，常用于密钥生成。
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetRandstring(this object obj, int length = 8)
        {
            if (obj == null) obj = new object();
            string chars = @"0123456789=+%abcdefghijklmnopqrstuvwxyz#^!ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var random = new Random(obj.GetHashCode());
            string strRndKey = "";
            for (int i = 0; i < length; i++)
            {
                strRndKey += chars[random.Next(chars.Length)];
            }
            return strRndKey;
        }




        /// <summary>
        /// 获取随机码
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetRandomCode(this object obj, int length = 4)
        {
            if (obj == null) obj = new object();
            string chars = @"0123456789";
            var random = new Random(obj.GetHashCode());
            string strRndKey = "";
            for (int i = 0; i < length; i++)
            {
                strRndKey += chars[random.Next(chars.Length)];
            }
            return strRndKey;
        }

        /// <summary>
        /// 将指定对象转换为指定类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T As<T>(this object obj)
            where T : class
        {
            return obj as T;
        }

        /// <summary>
        /// 获取NpoiMapper映射对象的列名
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetNpoiMapperColumnName(this object obj, string name)
        {
            string result = name;

            var property = obj.GetType().GetProperties().FirstOrDefault(x => x.Name.Equals(name));
            if (property != null)
            {
                var columnAttribute = (ColumnAttribute)property.GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(ColumnAttribute));
                result = columnAttribute?.Name;
            }

            return result;
        }

        /// <summary>
        /// 通过反射获取所有字段列表 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>

       public static  List<FieldInfo> GetFields(this object obj)
        {
            List<FieldInfo> list = new List<FieldInfo>();
            FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            //FieldInfo[] array = fields;
            //foreach (FieldInfo fieldInfo in array)
            //{
            //    if (!fieldInfo.Name.Equals("TableName") && !fieldInfo.Name.Equals("PrimaryKey") && !fieldInfo.Name.Equals("NullKey") && !fieldInfo.Name.Equals("IdentityKey"))
            //    {
            //        list.Add(fieldInfo);
            //    }
            //}
            return fields.ToList();
        }


        public static FieldInfo GetField(this object obj,string FieldName)
        {
            List<FieldInfo> fields = obj.GetFields();
            foreach (FieldInfo item in fields)
            {
                if (item.Name.Equals(FieldName))
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取对象字段值 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>

        public static object GetValue(this object obj,string fieldName)
        {
            FieldInfo field = obj.GetField(fieldName);
            if (field != null)
            {
                return field.GetValue(obj);
            }
            PropertyInfo property = obj.GetProperty(fieldName);
            if (property != null && property.CanRead)
            {
                return property.GetValue(obj, null);
            }
            return null;
        }

        public static T GetValue<T>(this object obj, string fieldName)
        {
            FieldInfo field = obj.GetField(fieldName);
            if (field != null)
            {
                return (T)field.GetValue(obj);
            }
            PropertyInfo property = obj.GetProperty(fieldName);
            if (property != null && property.CanRead)
            {
                return (T)property.GetValue(obj, null);
            }
            return default(T);
        }



        /// <summary>
        /// 获取对象指定属性信息PropertyInfo
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="PropertyName"></param>
        /// <returns></returns>
        public static PropertyInfo GetProperty(this object obj,string PropertyName)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();
            PropertyInfo[] array = properties;
            foreach (PropertyInfo propertyInfo in array)
            {
                if (propertyInfo.Name.Equals(PropertyName))
                {
                    return propertyInfo;
                }
            }
            return null;
        }



    }
}
