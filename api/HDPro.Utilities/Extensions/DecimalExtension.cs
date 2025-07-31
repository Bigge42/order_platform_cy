using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDPro.Utilities.Extensions
{
    public static class DecimalExtension
    {
        /// <summary>
        /// 控制decimal精度，确保不超过数据库decimal(18,6)的范围
        /// </summary>
        /// <param name="value">原始decimal值</param>
        /// <param name="precision">小数位数，默认为6</param>
        /// <returns>处理后的decimal值</returns>
        public static decimal ControlPrecision(this decimal value, int precision = 6)
        {
            return Math.Round(value, precision, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 将decimal转换为字符串，并控制精度
        /// </summary>
        /// <param name="value">原始decimal值</param>
        /// <param name="precision">小数位数，默认为6</param>
        /// <returns>处理后的字符串</returns>
        public static string ToStringWithPrecision(this decimal value, int precision = 6)
        {
            return ControlPrecision(value, precision).ToString();
        }
        /// <summary>
        /// 根据字段的DisplayFormat属性解析精度
        /// </summary>
        /// <param name="formatString">DisplayFormat属性的DataFormatString值，如"18,6"或"18,2"</param>
        /// <returns>小数位数</returns>
        public static int ParsePrecision(string formatString)
        {
            if (string.IsNullOrEmpty(formatString))
                return 6; // 默认精度

            string[] parts = formatString.Split(',');
            if (parts.Length == 2 && int.TryParse(parts[1], out int precision))
                return precision;

            return 6; // 解析失败时的默认精度
        }

        /// <summary>
        /// 安全执行加法并控制精度
        /// </summary>
        /// <param name="a">第一个操作数</param>
        /// <param name="b">第二个操作数</param>
        /// <param name="precision">小数位数，默认为6</param>
        /// <returns>处理后的结果</returns>
        public static decimal SafeAdd(this decimal a, decimal b, int precision = 6)
        {
            return ControlPrecision(a + b, precision);
        }

        /// <summary>
        /// 安全执行减法并控制精度
        /// </summary>
        /// <param name="a">第一个操作数</param>
        /// <param name="b">第二个操作数</param>
        /// <param name="precision">小数位数，默认为6</param>
        /// <returns>处理后的结果</returns>
        public static decimal SafeSubtract(this decimal a, decimal b, int precision = 6)
        {
            return ControlPrecision(a - b, precision);
        }

        /// <summary>
        /// 安全执行乘法并控制精度
        /// </summary>
        /// <param name="a">第一个操作数</param>
        /// <param name="b">第二个操作数</param>
        /// <param name="precision">小数位数，默认为6</param>
        /// <returns>处理后的结果</returns>
        public static decimal SafeMultiply(this decimal a, decimal b, int precision = 6)
        {
            return ControlPrecision(a * b, precision);
        }

        /// <summary>
        /// 安全执行除法并控制精度
        /// </summary>
        /// <param name="a">被除数</param>
        /// <param name="b">除数</param>
        /// <param name="precision">小数位数，默认为6</param>
        /// <returns>处理后的结果，如果除数为0则返回0</returns>
        public static decimal SafeDivide(this decimal a, decimal b, int precision = 6)
        {
            if (b == 0)
            {
                return 0;
            }
            return ControlPrecision(a / b, precision);
        }

    }
} 