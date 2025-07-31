using System;
using System.Text.RegularExpressions;

namespace HDPro.Utilities
{
    /// <summary>
    /// 密码强度校验工具类
    /// </summary>
    public static class PasswordValidator
    {
        /// <summary>
        /// 验证密码强度：8~12位，必须包含大小写字母、数字、特殊字符
        /// </summary>
        /// <param name="password">密码</param>
        /// <returns>校验结果</returns>
        public static PasswordValidationResult ValidatePassword(string password)
        {
            var result = new PasswordValidationResult();
            
            if (string.IsNullOrWhiteSpace(password))
            {
                result.IsValid = false;
                result.ErrorMessage = "密码不能为空";
                return result;
            }

            // 长度检查：8~12位
            if (password.Length < 8 || password.Length > 12)
            {
                result.IsValid = false;
                result.ErrorMessage = "密码长度必须为8~12位";
                return result;
            }

            // 检查是否包含大写字母
            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                result.IsValid = false;
                result.ErrorMessage = "密码必须包含至少一个大写字母";
                return result;
            }

            // 检查是否包含小写字母
            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                result.IsValid = false;
                result.ErrorMessage = "密码必须包含至少一个小写字母";
                return result;
            }

            // 检查是否包含数字
            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                result.IsValid = false;
                result.ErrorMessage = "密码必须包含至少一个数字";
                return result;
            }

            // 检查是否包含特殊字符
            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?~`]"))
            {
                result.IsValid = false;
                result.ErrorMessage = "密码必须包含至少一个特殊字符";
                return result;
            }

            // 检查是否只包含允许的字符
            if (!Regex.IsMatch(password, @"^[A-Za-z0-9!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?~`]+$"))
            {
                result.IsValid = false;
                result.ErrorMessage = "密码包含不允许的字符";
                return result;
            }

            result.IsValid = true;
            result.ErrorMessage = "密码强度符合要求";
            return result;
        }
    }

    /// <summary>
    /// 密码校验结果
    /// </summary>
    public class PasswordValidationResult
    {
        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}