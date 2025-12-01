using System;
using System.Net;
using System.Text.RegularExpressions;

namespace Daibitx.Common
{
    /// <summary>
    /// 验证工具类
    /// 提供各种数据验证方法
    /// </summary>
    public static class ValidationUtil
    {
        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex PhoneRegex = new Regex(
            @"^1[3-9]\d{9}$",
            RegexOptions.Compiled);

        private static readonly Regex UrlRegex = new Regex(
            @"^(http|https|ftp)://[^\s/$.?#].[^\s]*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex IpAddressRegex = new Regex(
            @"^(\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})$",
            RegexOptions.Compiled);

        private static readonly Regex NumericRegex = new Regex(
            @"^-?\d+(\.\d+)?$",
            RegexOptions.Compiled);

        private static readonly Regex GuidRegex = new Regex(
            @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$",
            RegexOptions.Compiled);

        private static readonly Regex Base64Regex = new Regex(
            @"^[a-zA-Z0-9\+/]*={0,2}$",
            RegexOptions.Compiled);

        private static readonly Regex JsonRegex = new Regex(
            @"^\s*(\{.*\}|\[.*\])\s*$",
            RegexOptions.Compiled | RegexOptions.Singleline);

        /// <summary>
        /// 验证邮箱格式
        /// </summary>
        /// <param name="email">邮箱地址</param>
        /// <returns>是否为有效的邮箱格式</returns>
        public static bool IsEmail(string email)
        {
            if (StringUtil.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            return EmailRegex.IsMatch(email) && email.Length <= 254;
        }

        /// <summary>
        /// 验证手机号（中国大陆）
        /// </summary>
        /// <param name="phoneNumber">手机号</param>
        /// <returns>是否为有效的手机号</returns>
        public static bool IsPhoneNumber(string phoneNumber)
        {
            if (StringUtil.IsNullOrWhiteSpace(phoneNumber))
            {
                return false;
            }

            return PhoneRegex.IsMatch(phoneNumber);
        }

        /// <summary>
        /// 验证URL格式
        /// </summary>
        /// <param name="url">URL地址</param>
        /// <returns>是否为有效的URL格式</returns>
        public static bool IsUrl(string url)
        {
            if (StringUtil.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            return UrlRegex.IsMatch(url) && Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }

        /// <summary>
        /// 验证IP地址格式
        /// </summary>
        /// <param name="ipAddress">IP地址</param>
        /// <returns>是否为有效的IP地址</returns>
        public static bool IsIpAddress(string ipAddress)
        {
            if (StringUtil.IsNullOrWhiteSpace(ipAddress))
            {
                return false;
            }

            var match = IpAddressRegex.Match(ipAddress);
            if (!match.Success)
            {
                return false;
            }

            // 验证每个段是否在0-255范围内
            for (int i = 1; i <= 4; i++)
            {
                if (!int.TryParse(match.Groups[i].Value, out var segment) || segment < 0 || segment > 255)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 验证是否为数字
        /// </summary>
        /// <param name="value">要验证的字符串</param>
        /// <returns>是否为数字</returns>
        public static bool IsNumeric(string value)
        {
            if (StringUtil.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return NumericRegex.IsMatch(value);
        }

        /// <summary>
        /// 验证GUID格式
        /// </summary>
        /// <param name="guid">GUID字符串</param>
        /// <returns>是否为有效的GUID格式</returns>
        public static bool IsGuid(string guid)
        {
            if (StringUtil.IsNullOrWhiteSpace(guid))
            {
                return false;
            }

            return GuidRegex.IsMatch(guid) || Guid.TryParse(guid, out _);
        }

        /// <summary>
        /// 验证Base64格式
        /// </summary>
        /// <param name="base64">Base64字符串</param>
        /// <returns>是否为有效的Base64格式</returns>
        public static bool IsBase64(string base64)
        {
            if (StringUtil.IsNullOrWhiteSpace(base64))
            {
                return false;
            }

            // Base64字符串长度必须是4的倍数
            if (base64.Length % 4 != 0)
            {
                return false;
            }

            return Base64Regex.IsMatch(base64);
        }

        /// <summary>
        /// 验证JSON格式
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <returns>是否为有效的JSON格式</returns>
        public static bool IsJson(string json)
        {
            if (StringUtil.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            return JsonRegex.IsMatch(json);
        }

        /// <summary>
        /// 验证值是否在指定范围内
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="value">要验证的值</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>是否在范围内</returns>
        public static bool IsInRange<T>(T value, T min, T max) where T : IComparable<T>
        {
            if (value == null)
            {
                return false;
            }

            return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
        }

        /// <summary>
        /// 验证对象是否为null
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="value">要验证的对象</param>
        /// <returns>是否为null</returns>
        public static bool IsNull<T>(T value)
        {
            return value == null;
        }

        /// <summary>
        /// 验证对象是否有值（不为null）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="value">要验证的对象</param>
        /// <returns>是否有值</returns>
        public static bool HasValue<T>(T value)
        {
            return value != null;
        }
    }
}