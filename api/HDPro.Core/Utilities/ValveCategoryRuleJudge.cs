using System;
using System.Collections.Generic;
using System.Linq;

namespace HDPro.Core.Utilities
{
    public static class ValveCategoryRuleJudge
    {
        private const string SoftSealBallValve = "软密封球阀";
        private const string HardSealBallValve = "硬密封球阀";
        private const string StraightValve = "直通阀";
        private const string ButterflyValve = "蝶阀";

        private static readonly HashSet<string> MissingList = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ZJFR-1001",
            "ZJFR-1100",
            "ZJFR-1101",
            "ZJFRB",
            "ZJFRF",
            "ZJGWG-0.1"
        };

        private static readonly Dictionary<string, (string Category, string RuleCode)> SpecialCases =
            new Dictionary<string, (string Category, string RuleCode)>(StringComparer.OrdinalIgnoreCase)
            {
                { "H41H-16C", (SoftSealBallValve, "SPECIAL_H41H-16C") },
                { "H41H-16P", (SoftSealBallValve, "SPECIAL_H41H-16P") },
                { "H420-T", (SoftSealBallValve, "SPECIAL_H420-T") },
                { "H44Y-10P", (SoftSealBallValve, "SPECIAL_H44Y-10P") },
                { "R1/4闸阀（小球阀）", (SoftSealBallValve, "SPECIAL_R1/4闸阀（小球阀）") },
                { "RC1/4小球阀", (SoftSealBallValve, "SPECIAL_RC1/4小球阀") },
                { "REM", (SoftSealBallValve, "SPECIAL_REM") },
                { "JVBJG", (ButterflyValve, "SPECIAL_JVBJG") }
            };

        private static readonly HashSet<int> SoftSealNums = new HashSet<int>
        {
            1000, 1001, 1003, 1010, 1011, 2000, 2010, 4000, 4010
        };

        private static readonly HashSet<int> HardSealNums = new HashSet<int>
        {
            1100, 1101, 1110, 1111, 1113, 2100, 2110, 3110, 3111, 4100, 3
        };

        public static (string Category, string RuleCode)? TryJudge(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
            {
                return null;
            }

            var trimmed = productName.Trim();
            if (MissingList.Contains(trimmed))
            {
                return null;
            }

            if (SpecialCases.TryGetValue(trimmed, out var specialCase))
            {
                return specialCase;
            }

            var upper = trimmed.ToUpperInvariant();
            if (upper.StartsWith("ZJH", StringComparison.OrdinalIgnoreCase))
            {
                return TryJudgeZjh(upper);
            }

            if (upper.StartsWith("ZZ", StringComparison.OrdinalIgnoreCase))
            {
                return (StraightValve, "PREFIX_ZZ");
            }

            if (upper.StartsWith("V", StringComparison.OrdinalIgnoreCase))
            {
                return (ButterflyValve, "PREFIX_V");
            }

            if (upper.StartsWith("K", StringComparison.OrdinalIgnoreCase))
            {
                return (StraightValve, "PREFIX_K");
            }

            if (upper.StartsWith("P", StringComparison.OrdinalIgnoreCase)
                || upper.StartsWith("Q", StringComparison.OrdinalIgnoreCase)
                || upper.StartsWith("T", StringComparison.OrdinalIgnoreCase))
            {
                return (SoftSealBallValve, "PREFIX_PQT");
            }

            if (upper.StartsWith("J", StringComparison.OrdinalIgnoreCase))
            {
                return (SoftSealBallValve, "PREFIX_J");
            }

            if (upper.StartsWith("H", StringComparison.OrdinalIgnoreCase))
            {
                return (StraightValve, "PREFIX_H");
            }

            if (upper.StartsWith("R", StringComparison.OrdinalIgnoreCase))
            {
                return (HardSealBallValve, "PREFIX_R");
            }

            if (upper.StartsWith("Z", StringComparison.OrdinalIgnoreCase)
                && !upper.StartsWith("ZZ", StringComparison.OrdinalIgnoreCase)
                && !upper.StartsWith("ZJH", StringComparison.OrdinalIgnoreCase))
            {
                return (SoftSealBallValve, "PREFIX_Z");
            }

            return null;
        }

        private static (string Category, string RuleCode)? TryJudgeZjh(string upper)
        {
            if (upper.StartsWith("ZJHQR", StringComparison.OrdinalIgnoreCase))
            {
                return (SoftSealBallValve, "ZJHQR");
            }

            if (upper.StartsWith("ZJHRF", StringComparison.OrdinalIgnoreCase))
            {
                return (SoftSealBallValve, "ZJHRF");
            }

            if (upper.StartsWith("ZJHVF", StringComparison.OrdinalIgnoreCase))
            {
                return (SoftSealBallValve, "ZJHVF");
            }

            if (upper.StartsWith("ZJHV", StringComparison.OrdinalIgnoreCase))
            {
                return (HardSealBallValve, "ZJHV");
            }

            if (upper.StartsWith("ZJHR-", StringComparison.OrdinalIgnoreCase))
            {
                var suffix = upper.Substring("ZJHR-".Length);
                var digits = new string(suffix.TakeWhile(char.IsDigit).ToArray());
                if (int.TryParse(digits, out var number))
                {
                    if (SoftSealNums.Contains(number))
                    {
                        return (SoftSealBallValve, "ZJHR_NUM_SOFT");
                    }

                    if (HardSealNums.Contains(number))
                    {
                        return (HardSealBallValve, "ZJHR_NUM_HARD");
                    }
                }

                return (HardSealBallValve, "ZJHR_NUM_DEFAULT");
            }

            if (upper.StartsWith("ZJHR", StringComparison.OrdinalIgnoreCase))
            {
                return (HardSealBallValve, "ZJHR_DEFAULT");
            }

            return null;
        }
    }
}
