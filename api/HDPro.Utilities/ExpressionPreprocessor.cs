using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HDPro.Utilities
{
    /// <summary>
    /// 匹配变量名规则：[\u4e00-\u9fa5] 是中文Unicode范围，\w 包括英文、数字、下划线
    /// </summary>
    public class ExpressionPreprocessor
    {
        public static string Preprocess(string expression)
        {
            string processed = expression;
            processed = processed
                           .Replace("_floor", "Math.floor", StringComparison.OrdinalIgnoreCase)
                           .Replace("Ceiling", "Math.ceil", StringComparison.OrdinalIgnoreCase)
                           .Replace("高", "长")
                           .Replace("<>", "!=");

            processed = FixSpelling(processed); // 修正拼写错误
            processed = ReplaceFullWidthOperators(processed); // 处理全角符号
            processed = ProcessInClauses(processed, "not in"); // 处理NOT IN
            processed = ProcessInClauses(processed, "in");  // 处理IN
           
            processed = ProcessChainedComparisons(processed); // 处理链式比较
            processed = ReplaceEqualityOperators(processed);// 将所有的 = 和 == 统一替换为 ===，保留原有的 === 不变
            processed = processed.Replace("!===", "!=");//修正不等于的场景
            return processed;
        }

        private static string FixSpelling(string input) => Regex.Replace(input, @"\.incudes\b", ".includes");

        private static string ReplaceFullWidthOperators(string input) => input.Replace("‖", "||").Replace('＝', '=');// 全角‖转半角||
        private static string ReplaceEqualityOperators(string input)
        {
            // 正则说明：匹配1到2个等号（=或==），且后面没有第三个等号
            // - @={1,3} 匹配1到3个等号
            // - (?!=) 确保后面没有第三个等号
            return Regex.Replace(input, @"(?<!<|>)={1,3}(?!=)", "===");
        }

        private static string ProcessChainedComparisons(string input)
        {
            //匹配形如 A op1 B op2 C 的链式比较，A / B / C 可以是中英文混合变量或数值
            var pattern = @"([\u4e00-\u9fa5\w\d\.]+)\s*(<|>|<=|>=|==|!=)\s*([\u4e00-\u9fa5\w\d\.]+)\s*(<|>|<=|>=|==|!=)\s*([\u4e00-\u9fa5\w\d\.]+)";
            return Regex.Replace(input, pattern, m => $"({m.Groups[1]} {m.Groups[2]} {m.Groups[3]}) && ({m.Groups[3]} {m.Groups[4]} {m.Groups[5]})");
        }

        private static string ProcessInClauses(string input, string keyword)
        {
            var pattern = keyword == "in"
                ? @"([\u4e00-\u9fa5\w]+)\s+in\s*\(([^)]+)\)"
                : @"([\u4e00-\u9fa5\w]+)\s+not\s+in\s*\(([^)]+)\)";

            var replacement = keyword == "in"
                ? "[$2].includes($1)"
                : "!([$2].includes($1))";

            return Regex.Replace(input, pattern, replacement, RegexOptions.IgnoreCase);
        }
    }
}
