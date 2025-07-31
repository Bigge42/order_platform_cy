using System;

namespace HDPro.Utilities.Attributes
{
    /// <summary>
    /// 用于控制decimal类型的精度
    /// 可以应用于属性或方法参数
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class DecimalPrecisionAttribute : Attribute
    {
        /// <summary>
        /// 精度（小数位数）
        /// </summary>
        public int Precision { get; private set; }

        /// <summary>
        /// 初始化一个新的decimal精度控制特性实例
        /// </summary>
        /// <param name="precision">小数位数，例如2表示保留2位小数</param>
        public DecimalPrecisionAttribute(int precision)
        {
            if (precision < 0 || precision > 28)
            {
                throw new ArgumentOutOfRangeException(nameof(precision), "精度必须在0到28之间");
            }
            
            Precision = precision;
        }
    }
} 