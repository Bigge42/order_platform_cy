using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;
using HDPro.Utilities.JsonConverter;

namespace HDPro.Utilities
{
    public static class JsonUtil
    {

        private static JsonSerializerSettings setting = GetSerializerSettings();//初始化静态变量


        //去掉f前缀
        private static JsonSerializerSettings formatKeySettting = GetNoFSerializerSettings();//初始化静态变量



        public static JsonSerializerSettings GetSerializerSettings()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            //默认转小写首字母，
            settings.ContractResolver = new CustomContractResolver();
            //日期格式化
            settings.Converters.Add(new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
            // 忽略循环引用
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return settings;

        }

        public static JsonSerializerSettings GetNoFSerializerSettings()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            //默认转小写首字母，
            settings.ContractResolver = new PropsContractResolver(null, true);

            //日期格式化
            settings.Converters.Add(new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
            // 忽略循环引用
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return settings;

        }

        /// <summary>
        /// 序列化值 
        /// </summary>
        /// <param name="value">被序列化的内容</param>
        /// <param name="isEnableF">是否去掉字段前缀F</param>
        /// <returns></returns>
        public static string SerializeObject(object? value, bool isEnableF = false)
        {

            if (value == null)
            {
                return "";
            }
            if (isEnableF)
            {

                return JsonConvert.SerializeObject(value, formatKeySettting);
            }

            return JsonConvert.SerializeObject(value, setting);

        }





        public static object? DeserializeObject(string value, JsonSerializerSettings settings)
        {
            return null;
        }
        //

        public static object? DeserializeObject(string value, Type type)
        {
            return null;

        }



        public static T DeserializeObject<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value);

        }





        /// <summary>
        /// 合并2个json对象
        /// </summary>
        /// <param name="srcObj"></param>
        /// <param name="dstObj"></param>
        /// <param name="isMatchArrayItem"></param>
        /// <param name="isDeepClone"></param>
        /// <returns></returns>
        public static JObject Merge(this JObject srcObj, JObject dstObj, Func<JToken, JToken, string, bool> isMatchArrayItem, bool isDeepClone = true)
        {
            JObject rootObj;
            if (isDeepClone)
                rootObj = srcObj.DeepClone() as JObject;
            else rootObj = srcObj;

            foreach (var item in dstObj)
            {
                var propVal = rootObj[item.Key];
                if (propVal == null)
                {
                    rootObj[item.Key] = item.Value.DeepClone();
                    continue;
                }
                if (item.Value is JObject && propVal is JObject)
                {
                    rootObj[item.Key] = (propVal as JObject).Merge(item.Value as JObject, isMatchArrayItem);
                }
                else if (item.Value is JArray && propVal is JArray)
                {
                    rootObj[item.Key] = ((JArray)(propVal)).Merge(item.Value as JArray, isMatchArrayItem, false, item.Key);
                }
                else
                {
                    rootObj[item.Key] = item.Value;
                }
            }

            return rootObj;
        }

        /// <summary>
        /// 两个json数组合并
        /// </summary>
        /// <param name="srcArrayObj"></param>
        /// <param name="dstArrayObj"></param>
        /// <param name="isMatchArrayItem"></param>
        /// <param name="isDeepClone"></param>
        /// <param name="parentPropKey"></param>
        /// <returns></returns>
        public static JArray Merge(this JArray srcArrayObj, JArray dstArrayObj, Func<JToken, JToken, string, bool> isMatchArrayItem, bool isDeepClone = true, string parentPropKey = "")
        {
            JArray rootArrayObj = new JArray();
            int iSrcPos = 0;
            List<int> lstDstArrayIndex = new List<int>();
            List<JToken> lstMatchedItem = new List<JToken>();
            foreach (var item2 in srcArrayObj)
            {
                JToken mergeItem = item2;
                if (item2 is JObject)
                {
                    foreach (var item3 in dstArrayObj)
                    {
                        if (isMatchArrayItem?.Invoke(item2, item3, parentPropKey) == true)
                        {
                            lstMatchedItem.Add(item3);
                            mergeItem = (item2 as JObject).Merge(item3 as JObject, isMatchArrayItem, false);
                        }
                    }
                }
                else if (item2 is JArray)
                {
                    if (iSrcPos < dstArrayObj.Count)
                    {
                        var dstItem = dstArrayObj[iSrcPos];
                        //相同位置处都是数组，则进行合并，否则只追加
                        if (dstItem is JArray)
                        {
                            mergeItem = (item2 as JArray).Merge(dstItem as JArray, isMatchArrayItem, false, parentPropKey);
                            lstDstArrayIndex.Add(iSrcPos);
                        }
                    }
                }

                iSrcPos++;
                rootArrayObj.Add(mergeItem);

            }

            //数组对数组时只追加
            for (int index = 0; index < dstArrayObj.Count; index++)
            {
                var item3 = dstArrayObj[index];
                if (item3 is JArray && !lstDstArrayIndex.Contains(index)
                    || !lstMatchedItem.Contains(item3))
                {
                    rootArrayObj.Add(item3);
                }
            }

            return rootArrayObj;
        }

        /// <summary>
        /// 读取json对象中某个属性值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonObj"></param>
        /// <param name="propName"></param>
        /// <param name="defVal"></param>
        /// <returns></returns>
        public static T GetJsonValue<T>(this JObject jsonObj, string propName, T defVal = default(T))
        {
            if (jsonObj == null) return defVal;
            JToken propVal;
            if (jsonObj.TryGetValue(propName, StringComparison.OrdinalIgnoreCase, out propVal))
            {
                if (propVal.IsNullOrEmpty())
                {
                    return defVal;
                }
                return propVal.Value<T>() ;
            }
            return defVal;
        }


        /// <summary>
        /// 读取json对象中某个属性值，并指示读取的属性是否存在
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonObj"></param>
        /// <param name="propName"></param>
        /// <param name="hasProperty"></param>
        /// <param name="defVal"></param>
        /// <returns></returns>
        public static T GetJsonValue<T>(this JObject jsonObj, string propName, out bool hasProperty, T defVal = default(T))
        {
            hasProperty = false;
            if (jsonObj == null) return defVal;
            var jPropItem = jsonObj.Properties().FirstOrDefault(p => p.Name.EqualsIgnoreCase(propName));
            hasProperty = jPropItem != null;
            if (jPropItem != null
                && jPropItem.Value != null)
            {
                return jPropItem.Value.Value<T>();
            }
            return defVal;
        }

        /// <summary>
        /// 读取json对象中某个属性值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonData"></param>
        /// <param name="propName"></param>
        /// <param name="defVal"></param>
        /// <returns></returns>
        public static T GetJsonValue<T>(this JToken jsonData, string propName, T defVal = default(T))
        {
            if (jsonData is JObject) return GetJsonValue<T>(jsonData as JObject, propName, defVal);

            return jsonData.Value<T>(propName);
        }


        /// <summary>
        /// 判定JToken是为空
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>

        public static bool IsNullOrEmpty(this JToken token)
        {
            return (token == null) ||
                   (token.Type == JTokenType.Array && !token.HasValues) ||
                   (token.Type == JTokenType.Object && !token.HasValues) ||
                   (token.Type == JTokenType.String && token.ToString() == String.Empty) ||
                   (token.Type == JTokenType.Null)||( token.Type == JTokenType.Undefined)
                    || (token.Type == JTokenType.Property && ((JProperty)token).Value.ToString() == string.Empty);
        }

    }



    public class PropsContractResolver : CamelCasePropertyNamesContractResolver
    {
        Dictionary<string, string> dict_props = null;

        bool isEnableF = false;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="props">传入的属性数组</param>
        public PropsContractResolver(Dictionary<string, string> dictPropertyName, bool _isEnableF)
        {
            //指定字段要序列化成什么名称
            this.dict_props = dictPropertyName;
            isEnableF = _isEnableF;
        }
        protected override string ResolvePropertyName(string propertyName)
        {
            string newPropertyName = string.Empty;
            //属性转换
            if (dict_props != null && dict_props.TryGetValue(propertyName, out newPropertyName))
            {
                return newPropertyName;
            }
            else
            {
                //if (isEnableF && propertyName.StartsWith("f", StringComparison.InvariantCultureIgnoreCase))
                //{
                //    return propertyName.Substring(1);//截取f字符
                //}
                return base.ResolvePropertyName(propertyName);
            }
        }
    }

}
