using Jint;
using ServiceStack.Script;
using System.Security.Cryptography;
using System.Text;
using static System.Formats.Asn1.AsnWriter;

namespace HDPro.Utilities.Extensions
{
    public static class StringExtension
    {
        public static string To32MD5(this string s)
        {
            //创建MD5对象
            MD5 md5 = MD5.Create();
            //开始加密
            //需要将字符串转化成字节数组
            byte[] buffer = Encoding.UTF8.GetBytes(s);
            byte[] MD5Buffer = md5.ComputeHash(buffer);
            //将字节数组转化成字符串
            //字节数组---字符串
            //将字节数组中每个元素按照指定的编码格式解析成字符串
            //直接将数组ToString（）；
            //将字节数组中的每个元素Tostring（）；
            //return Encoding.Default.GetString(MD5Buffer);//指定编码解析乱码
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < MD5Buffer.Length; i++)
            {
                sb.Append( MD5Buffer[i].ToString("x2"));//加个参数x表示16进制，不加就是10进制，再加上2就正常了
            }
            return sb.ToString();
        }
        public static string TrimNotNull(this string s)
        {
            return s == null ? "" : s.Trim();
        }
        public static bool IsNullOrEmptyOrWhiteSpace(this string data)
        {
            return string.IsNullOrEmpty(data) || string.IsNullOrWhiteSpace(data);
        }

        /// <summary>
        /// scope以逗号分割的字符串包含str
        /// </summary>
        /// <param name="str"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        public static bool SplitContains(this string scope, string str)
        {
            return scope.SplitContains(str,',');
        }
        /// <summary>
        /// scope以separator分割的数组包含str
        /// </summary>
        /// <param name="str"></param>
        /// <param name="scope"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static bool SplitContains(this string scope, string str, char separator)
        {
            return !scope.IsNullOrEmptyOrWhiteSpace() && scope.Split(separator).Contains(str,StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// scope以逗号分割的数组包含str
        /// </summary>
        /// <param name="str"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        public static bool WithIn(this string str, string scope)
        {
            return str.WithIn(scope, ',');
        }
        /// <summary>
        /// scope以separator分割的数组包含str
        /// </summary>
        /// <param name="str"></param>
        /// <param name="scope"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static bool WithIn(this string str, string scope,char separator)
        {
            return scope != null && scope.Split(separator).Contains(str);
        }
        /// <summary>
        ///  计算字符串公式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public static decimal FormulaCalculation(string expression, List<string> errLst)
        {
            var engine = new Engine();
            return engine.EvaluateToDecimal(expression, errLst);
        }



        /// <summary>
        /// 判定是包含某个字符串
        /// </summary>
        /// <param name="s"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsInStr(this string s, string str)
        {
            if (str.IsNullOrEmptyOrWhiteSpace())
            {
                return true;
            }
            string[] first = s.Split(',');
            string[] second = str.Split(',');
            return first.Intersect(second).ToArray().Length != 0;
        }

        public static string Trim(this string S, string R)
        {
            bool flag = S.IsNullOrEmptyOrWhiteSpace();
            string result;
            if (flag)
            {
                result = S;
            }
            else
            {
                while (S.StartsWith(R))
                {
                    S = S.Substring(R.Length);
                }
                while (S.EndsWith(R))
                {
                    S = S.Substring(0, S.Length - R.Length);
                }
                result = S;
            }
            return result;
        }
        public static string TrimEndZero(this string str)
        {
            if (decimal.TryParse(str, out var result))
            {
                return result.ToString("0.######");
            }
            return str;
        }
        public static string TrimEndZero(this decimal d)
        {
            return d.ToString("0.######");
        }

    }
}
