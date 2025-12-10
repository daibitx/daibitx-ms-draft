using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Daibitx.Common
{
    /// <summary>
    /// String utility class
    /// Provides common string related operations
    /// </summary>
    public static class StringUtil
    {
        private static readonly Regex CamelCaseRegex = new Regex(@"([a-z])([A-Z])", RegexOptions.Compiled);
        private static readonly Regex PascalCaseRegex = new Regex(@"(?:^|[^a-zA-Z])([a-z])", RegexOptions.Compiled);
        private static readonly Regex SnakeCaseRegex = new Regex(@"([a-z])([A-Z])|([A-Z])([A-Z][a-z])", RegexOptions.Compiled);
        private static readonly Regex KebabCaseRegex = new Regex(@"([a-z])([A-Z])|([A-Z])([A-Z][a-z])", RegexOptions.Compiled);

        /// <summary>
        /// Check if string is null or empty
        /// </summary>
        /// <param name="value">String to check</param>
        /// <returns>Whether it's null or empty</returns>
        public static bool IsNullOrEmpty(string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Check if string is null, empty or whitespace
        /// </summary>
        /// <param name="value">String to check</param>
        /// <returns>Whether it's null, empty or whitespace</returns>
        public static bool IsNullOrWhiteSpace(string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Safely trim whitespace from both ends of string
        /// </summary>
        /// <param name="value">String to process</param>
        /// <returns>Processed string</returns>
        public static string TrimSafe(string value)
        {
            return value?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Convert string to camelCase
        /// </summary>
        /// <param name="value">String to convert</param>
        /// <returns>camelCase string</returns>
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
        /// Convert string to PascalCase
        /// </summary>
        /// <param name="value">String to convert</param>
        /// <returns>PascalCase string</returns>
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
        /// Convert string to snake_case
        /// </summary>
        /// <param name="value">String to convert</param>
        /// <returns>snake_case string</returns>
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
        /// Convert string to kebab-case
        /// </summary>
        /// <param name="value">String to convert</param>
        /// <returns>kebab-case string</returns>
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
        /// Truncate string to specified length
        /// </summary>
        /// <param name="value">String to truncate</param>
        /// <param name="maxLength">Maximum length</param>
        /// <returns>Truncated string</returns>
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
        /// Mask string
        /// </summary>
        /// <param name="value">String to mask</param>
        /// <param name="start">Number of characters to keep at start</param>
        /// <param name="end">Number of characters to keep at end</param>
        /// <param name="maskChar">Mask character</param>
        /// <returns>Masked string</returns>
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
        /// Generate random string of specified length
        /// </summary>
        /// <param name="length">String length</param>
        /// <param name="charset">Character set (default is letters and numbers)</param>
        /// <returns>Random string</returns>
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
        /// Compute string hash (using SHA256)
        /// </summary>
        /// <param name="value">String to compute hash for</param>
        /// <returns>Hash hexadecimal string</returns>
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