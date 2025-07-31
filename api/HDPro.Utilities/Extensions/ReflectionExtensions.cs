using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace HDPro.Utilities.Extensions
{
    public static class ReflectionExtensions
    {
        public static string GetDisplayName(this Enum value)
        {
            return GetDisplayName(value.GetType().GetRuntimeField(value.ToString()));
        }

        public static string GetDisplayGroup(this Enum value)
        {
            return GetDisplayGroup(value.GetType().GetRuntimeField(value.ToString()));
        }

        public static string GetDisplayName(this MemberInfo member)
        {
            if (member == null)
            {
                return null;
            }
            var display = member.GetCustomAttribute<DisplayAttribute>();
            if (display != null)
            {
                return display.GetName();
            }

            var attribute = member.GetCustomAttribute<DisplayNameAttribute>();
            return attribute?.DisplayName ?? member.Name;
        }
        public static string GetDisplayGroup(this MemberInfo member)
        {
            return member.GetCustomAttribute<DisplayAttribute>(false)?.GetGroupName() ?? string.Empty;
        }
    }
}
