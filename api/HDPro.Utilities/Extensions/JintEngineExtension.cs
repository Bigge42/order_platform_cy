using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Jint;
using NPOI.OpenXmlFormats.Dml;

namespace HDPro.Utilities.Extensions
{

    public static class JintEngineExtension
    {
        /// <summary>
        /// 注入自定义js函数
        /// </summary>
        /// <param name="engine"></param>
        /// <returns></returns>
        public static Engine SetJsFuntion(this Engine engine)
        {
            #region ∈数值区间判断：Jcompare(1,'[1,2]')
            string funEara = @"function Jcompare(num, range) { 
                          if (range == """") {
                            return true;
                          }
                          if (range.indexOf('[') > -1 || range.indexOf('(') > -1) {
                            var bigOrBigThen = range.indexOf('[') > -1 ? true : false;
                            var lessOrLessEQ = range.indexOf(']') > -1 ? true : false;
                            var smallValue = parseFloat(
                              range.split("","").shift().split(""["").pop().split(""("").pop()
                            );
                            var bigValue = parseFloat(
                              range.split("","").pop().split(""]"").shift().split("")"").shift()
                            );  

                            if (isNaN(smallValue)) {
                              smallValue = 0;
                            }   

                            if (isNaN(bigValue)) {
                              return bigOrBigThen ? (smallValue <= num) : (smallValue < num)
                            } else {
                              return (bigOrBigThen ? (smallValue <= num) : (smallValue < num)) &&
                                (lessOrLessEQ ? (bigValue >= num) : (bigValue > num));
                            }
                          } else {
                            return parseFloat(range) == parseFloat(num);
                          }
                          return false;
                        }";
            var fromValue = engine.Execute(funEara).GetValue("Jcompare");
            //engine.Execute("Jcompare(2,'[1, 4]')")
            #endregion

            return engine;
        }

        public static decimal EvaluateToDecimal(this Engine engine, string expression, List<string> errors = null)
        {
            if(expression.IsNullOrEmptyOrWhiteSpace())
            {
                return 0;
            }
            else
            {
                decimal result = 0;
                var processedExpression = expression.ExpressionPreprocess();
                try
                {
                 
                    var evaluateResult = engine.Evaluate(processedExpression).ToString();
                    if (!decimal.TryParse(evaluateResult, out result))
                    {
                        errors?.Add($"公式：{expression}，公式执行结果：{evaluateResult}，转为数值失败");
                        HDLogHelper.Log("EvaluateToDecimal_Error", $"expression：{expression},evaluateResult：{evaluateResult}");
                        return 0;
                    }
                }
                catch(Exception ex)
                {
                    errors?.Add($"公式：{expression}，执行错误信息：{ex.Message}");
                    HDLogHelper.Log("EvaluateToDecimal_Error", $"公式：{expression},格式化公式：{processedExpression}");
                    HDLogHelper.Log("EvaluateToDecimal_Error", ex.Message);
                    throw ;
                }
                return result.ControlPrecision();
            }
        }
        public static decimal EvaluateToDecimalNoReplaceSpecialChar(this Engine engine, string expression, List<string> errors = null)
        {
            if (expression.IsNullOrEmptyOrWhiteSpace())
            {
                return 0;
            }
            else
            {
                decimal result = 0;
                try
                {
                    var evaluateResult = engine.Evaluate(expression).ToString();
                    if (!decimal.TryParse(evaluateResult, out result))
                    {
                        errors?.Add($"公式：{expression}，公式执行结果：{evaluateResult}，转为数值失败");
                        HDLogHelper.Log("EvaluateToDecimal_Error", $"expression：{expression},evaluateResult：{evaluateResult}");
                        return 0;
                    }
                }
                catch (Exception ex)
                {
                    errors?.Add($"公式：{expression}，执行错误信息：{ex.Message}");
                    HDLogHelper.Log("EvaluateToDecimal_Error", $"公式：{expression},格式化公式：{expression}");
                    HDLogHelper.Log("EvaluateToDecimal_Error", ex.Message);
                    throw;
                }
                return result.ControlPrecision();
            }
        }
        public static string EvaluateToString(this Engine engine, string expression, List<string> errors = null)
        {
            string result = "";
            var processedExpression = expression.ExpressionPreprocess();
            try
            {
                result= processedExpression.IsNullOrEmptyOrWhiteSpace() ? "" : engine.Evaluate(processedExpression).ToString();
            }
            catch(Exception ex)
            {
                errors?.Add($"公式：{expression}，执行错误信息：{ex.Message}");
                HDLogHelper.Log("EvaluateToString_Error", $"公式：{expression},格式化公式：{processedExpression}");
                HDLogHelper.Log("EvaluateToString_Error", ex.Message);
                throw;
            }
            return result;
        }
        public static bool EvaluateToBool(this Engine engine, string expression, List<string> errors = null)
        {
            bool result = false;
            var processedExpression = expression.ExpressionPreprocess();
            try
            {
                result = processedExpression.IsNullOrEmptyOrWhiteSpace() || Convert.ToBoolean(engine.Evaluate(processedExpression).ToObject());
            }
            catch (Exception ex)
            {
                errors?.Add($"公式：{expression}，执行错误信息：{ex.Message}");
                HDLogHelper.Log("EvaluateToBool_Error", $"公式：{expression},格式化公式：{processedExpression}");
                HDLogHelper.Log("EvaluateToBool_Error", ex.Message);
                throw;
            }
            return result;
        }

        private static string ExpressionPreprocess(this string expression)
        {
            expression = expression.IsNullOrEmptyOrWhiteSpace() ? "" : expression;
            //expression = expression
            //               .Replace("=", "==")
            //               .Replace("====", "==")
            //               .Replace("<==", "<=")
            //               .Replace(">==", ">=")
            //               .Replace("<>", "!=")
            //               .Replace("!==", "!=")
            //               .Replace("_floor", "Math.floor", StringComparison.OrdinalIgnoreCase)
            //               .Replace("Ceiling", "Math.ceil", StringComparison.OrdinalIgnoreCase)
            //               .Replace("高", "长")
            //               ;
            //expression = SpecialExpression(expression);
            return ExpressionPreprocessor.Preprocess(expression);
        }

        private static string SpecialExpression(string expression)
        {
            List<string> result = new List<string>();
            string[] arr;
            if (expression.Contains("&&"))
            {
                arr = expression.Split("&&");
            }
            else
            {
                arr = expression.Split("||");
            }

            foreach (var item in arr)
            {
                if (item.Contains("NOT IN", StringComparison.OrdinalIgnoreCase))
                {
                    var itemTemp = item.Replace("in", "IN", StringComparison.OrdinalIgnoreCase)
                        .Replace("not", "NOT", StringComparison.OrdinalIgnoreCase);

                    var itemArr = itemTemp.Split("NOT IN");
                    var a = itemArr[0].Trim();
                    var b = itemArr[1].Replace("(", "")
                        .Replace(")", "")
                        .Replace("（", "")
                        .Replace("）", "");
                    var bArr = b.Split(",");
                    List<string> bResult = new List<string>();
                    foreach (var bItem in bArr)
                    {
                        bResult.Add($"{a}!={bItem}");
                    }
                    var newItem = $"({string.Join("&&", bResult)})";
                    result.Add(newItem);
                }
                else if (!item.Contains("includes") && item.Contains("IN", StringComparison.OrdinalIgnoreCase))
                {
                    var itemTemp = item.Replace("in", "IN", StringComparison.OrdinalIgnoreCase);
                    var itemArr = itemTemp.Split("IN");
                    var a = itemArr[0].Trim();
                    var b = itemArr[1].Replace("(", "")
                        .Replace(")", "")
                        .Replace("（", "")
                        .Replace("）", "");
                    var bArr = b.Split(",");
                    List<string> bResult = new List<string>();
                    foreach (var bItem in bArr)
                    {
                        bResult.Add($"{a}=={bItem}");
                    }
                    var newItem = $"({string.Join("||", bResult)})";
                    result.Add(newItem);
                }
                else if (item.IndexOf("<=") == -1 && item.IndexOf("<") > -1 && item.LastIndexOf("<") > -1 && item.IndexOf("<") != item.LastIndexOf("<"))
                {
                    var itemArr = item.Split("<");
                    var newItem = $"{itemArr[0]}<{itemArr[1]}&&{itemArr[1]}<{itemArr[2]}";
                    result.Add(newItem);
                }
                else if (item.IndexOf("<=") > -1 && item.LastIndexOf("<=") > -1 && item.IndexOf("<=") != item.LastIndexOf("<="))
                {
                    var itemArr = item.Split("<=");
                    var newItem = $"{itemArr[0]}<={itemArr[1]}&&{itemArr[1]}<={itemArr[2]}";
                    result.Add(newItem);
                }
                else if (item.IndexOf("<") > -1 && item.IndexOf("<=") > -1)
                {
                    var i = item.IndexOf("<=");
                    var j = item.IndexOf("<");
                    if (i > j)
                    {
                        var itemArr = item.Split("<=");
                        var itemArr0 = itemArr[0].Split("<");
                        var a = itemArr0[0];
                        var b = itemArr0[1];
                        var c = itemArr[1];
                        var newItem = $"{a}<{b}&&{b}<={c}";
                        result.Add(newItem);
                    }
                    else
                    {
                        var itemArr = item.Split("<=");
                        if (itemArr[1].IndexOf("<") > -1)
                        {
                            var itemArr1 = itemArr[1].Split("<");
                            var a = itemArr[0];
                            var b = itemArr1[0];
                            var c = itemArr1[1];
                            var newItem = $"{a}<={b}&&{b}<{c}";
                            result.Add(newItem);
                        }
                        else
                        {
                            result.Add(item);
                        }
                    }
                }
                else
                {
                    result.Add(item);
                }
            }
            if (expression.Contains("&&"))
            {
                return string.Join("&&", result);
            }
            else
            {
                return string.Join("||", result);
            }
        }

        public static List<string> GetVarsFromExpressionOld(this string expression)
        {
            List<string> result = new List<string>();
            if (!expression.IsNullOrEmptyOrWhiteSpace())
            {
                var tmpExpression = expression.
                    Replace("Math.abs", "").
                    Replace("Math.ceil", "").
                    Replace("Math.floor", "").
                    Replace("Math.round", "");
                Regex rg = new Regex("[A-Za-z_\u4e00-\u9fa5]+[A-Za-z0-9_\u4e00-\u9fa5]*");
                var newExpression = Regex.Replace(tmpExpression, "[\"'](.*?)[\"']", "");
                System.Text.RegularExpressions.MatchCollection matchs = rg.Matches(newExpression);
                if (matchs.Count != 0)
                {
                    foreach (Match m in matchs)
                    {
                        if (!result.Contains(m.Value))
                        {
                            result.Add(m.Value);
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 返回变量名-报价服务表达式
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static List<string> GetVarsFromQuotedPriceExp(this string expression)
        {
            List<string> result = new List<string>();
            if (!expression.IsNullOrEmptyOrWhiteSpace())
            {
                var tmpExpression = expression
                    .Replace("_floor", "")
                    .Replace("Ceiling", "")
                    .Replace("IN", "")
                    .Replace("NOT", "")
                    .Replace("Math.ceil", "")
                    .Replace("Math.floor", "")
                    .Replace("Math.round", "")
                    .Replace("高", "长")
                    .Replace("includes","")
                    .Replace("Jcompare", "")
                    .Replace("null","")
                    ;
                Regex rg = new Regex("[A-Za-z_\u4e00-\u9fa5]+[A-Za-z0-9_\u4e00-\u9fa5]*");
                var newExpression = Regex.Replace(tmpExpression, "[\"'](.*?)[\"']", "");
                System.Text.RegularExpressions.MatchCollection matchs = rg.Matches(newExpression);
                if (matchs.Count != 0)
                {
                    foreach (Match m in matchs)
                    {
                        if (!result.Contains(m.Value))
                        {
                            result.Add(m.Value);
                        }
                    }
                }
            }
            return result;
        }


    }
}
