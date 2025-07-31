using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HDPro.Utilities
{

    [Description("数字工具类")]
    public class NumbericUtil
    {

        /// <summary>
        /// 去数字尾0
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string TrimZero(string str)
        {
            if (str.IndexOf(".") > 0)
            {
                str = Regex.Replace(str.Trim(), "0+?$", " ");
                str = Regex.Replace(str.Trim(), "[.]$", " ");
            }
            return str;
        }


        public static T TrimZero<T>(object value)
        {
            string str = Convert.ToString(value);
            if (string.IsNullOrWhiteSpace(str))
            {
                return (T)Convert.ChangeType(0, typeof(T));
            }
            if (str.IndexOf(".") > 0)
            {
                str = Regex.Replace(str.Trim(), "0+?$", " ");
                str = Regex.Replace(str.Trim(), "[.]$", " ");
            }

            return   (T)Convert.ChangeType(str, typeof(T)) ;
        }

    }
}
