using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Daibitx.Common
{
    /// <summary>
    /// 字符串工具类
    /// 提供字符串相关的常用操作方法
    /// </summary>
    public static class StringUtil
    {
        private static readonly Regex CamelCaseRegex = new Regex(@"([a-z])([A-Z])", RegexOptions.Compiled);
        private static readonly Regex PascalCaseRegex = new Regex(@"(?:^|[^a-zA-Z])([a-z])", RegexOptions.Compiled);
        private static readonly Regex SnakeCaseRegex = new Regex(@"([a-z])([A-Z])|([A-Z])([A-Z][a-z])", RegexOptions.Compiled);
        private static readonly Regex KebabCaseRegex = new Regex(@"([a-z])([A-Z])|([A-Z])([A-Z][a-z])", RegexOptions.Compiled);

        /// <summary>
        /// 判断字符串是否为null或空
        /// </summary>
        /// <param name="value">要判断的字符串</param>
        /// <returns>是否为null或空</returns>
        public static bool IsNullOrEmpty(string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// 判断字符串是否为null、空或空白字符
        /// </summary>
        /// <param name="value">要判断的字符串</param>
        /// <returns>是否为null、空或空白字符</returns>
        public static bool IsNullOrWhiteSpace(string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// 安全去除字符串两端的空白字符
        /// </summary>
        /// <param name="value">要处理的字符串</param>
        /// <returns>处理后的字符串</returns>
        public static string TrimSafe(string value)
        {
            return value?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// 将字符串转换为驼峰命名（camelCase）
        /// </summary>
        /// <param name="value">要转换的字符串</param>
        /// <returns>驼峰命名字符串</returns>
        public static string ToCamelCase(string value)
        {
            if (IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var pascalCase = ToPascalCase(value);
            if (pascalCase.Length > 0)
            {
                return char.ToLowerInvariant(pascalCase[0]) + pascalCase.Substring(1);
            }
            return pascalCase;
        }

        /// <summary>
        /// 将字符串转换为帕斯卡命名（PascalCase）
        /// </summary>
        /// <param name="value">要转换的字符串</param>
        /// <returns>帕斯卡命名字符串</returns>
        public static string ToPascalCase(string value)
        {
            if (IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var result = new StringBuilder();
            var words = value.Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var word in words)
            {
                if (word.Length > 0)
                {
                    result.Append(char.ToUpperInvariant(word[0]));
                    if (word.Length > 1)
                    {
                        result.Append(word.Substring(1).ToLowerInvariant());
                    }
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 将字符串转换为蛇形命名（snake_case）
        /// </summary>
        /// <param name="value">要转换的字符串</param>
        /// <returns>蛇形命名字符串</returns>
        public static string ToSnakeCase(string value)
        {
            if (IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var result = SnakeCaseRegex.Replace(value, "$1$3_$2$4").ToLowerInvariant();
            return result.Replace('-', '_').Replace(' ', '_');
        }

        /// <summary>
        /// 将字符串转换为烤肉串命名（kebab-case）
        /// </summary>
        /// <param name="value">要转换的字符串</param>
        /// <returns>烤肉串命名字符串</returns>
        public static string ToKebabCase(string value)
        {
            if (IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var result = KebabCaseRegex.Replace(value, "$1$3-$2$4").ToLowerInvariant();
            return result.Replace('_', '-').Replace(' ', '-');
        }

        /// <summary>
        /// 截断字符串到指定长度
        /// </summary>
        /// <param name="value">要截断的字符串</param>
        /// <param name="maxLength">最大长度</param>
        /// <returns>截断后的字符串</returns>
        public static string Truncate(string value, int maxLength)
        {
            if (IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            if (maxLength <= 0)
            {
                return string.Empty;
            }

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        /// <summary>
        /// 对字符串进行掩码处理
        /// </summary>
        /// <param name="value">要处理的字符串</param>
        /// <param name="start">开始保留的字符数</param>
        /// <param name="end">结束保留的字符数</param>
        /// <param name="maskChar">掩码字符</param>
        /// <returns>掩码处理后的字符串</returns>
        public static string Mask(string value, int start, int end, char maskChar = '*')
        {
            if (IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            if (start + end >= value.Length)
            {
                return value;
            }

            var maskedLength = value.Length - start - end;
            var maskedPart = new string(maskChar, maskedLength);
            
            return value.Substring(0, start) + maskedPart + value.Substring(value.Length - end);
        }

        /// <summary>
        /// 生成指定长度的随机字符串
        /// </summary>
        /// <param name="length">字符串长度</param>
        /// <param name="charset">字符集（默认为字母和数字）</param>
        /// <returns>随机字符串</returns>
        public static string GenerateRandom(int length, string charset = null)
        {
            if (length <= 0)
            {
                return string.Empty;
            }

            const string defaultCharset = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var chars = charset ?? defaultCharset;
            
            if (string.IsNullOrEmpty(chars))
            {
                return string.Empty;
            }

            var result = new StringBuilder(length);
            var random = new Random();

            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            return result.ToString();
        }

        /// <summary>
        /// 计算字符串的哈希值（使用SHA256）
        /// </summary>
        /// <param name="value">要计算哈希的字符串</param>
        /// <returns>哈希值的十六进制字符串</returns>
        public static string ComputeHash(string value)
        {
            if (IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(value);
                var hash = sha256.ComputeHash(bytes);
                
                var result = new StringBuilder();
                foreach (var b in hash)
                {
                    result.Append(b.ToString("x2"));
                }
                
                return result.ToString();
            }
        }
    }
}