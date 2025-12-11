using System;
using System.Net;
using System.Text.RegularExpressions;

namespace Daibitx.Common
{
    /// <summary>
    /// Validation utility class
    /// Provides various data validation methods
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
        /// Validate email format
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns>Whether it's a valid email format</returns>
        public static bool IsEmail(string email)
        {
            if (StringUtil.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            return EmailRegex.IsMatch(email) && email.Length <= 254;
        }

        /// <summary>
        /// Validate phone number (Mainland China)
        /// </summary>
        /// <param name="phoneNumber">Phone number</param>
        /// <returns>Whether it's a valid phone number</returns>
        public static bool IsPhoneNumber(string phoneNumber)
        {
            if (StringUtil.IsNullOrWhiteSpace(phoneNumber))
            {
                return false;
            }

            return PhoneRegex.IsMatch(phoneNumber);
        }

        /// <summary>
        /// Validate URL format
        /// </summary>
        /// <param name="url">URL address</param>
        /// <returns>Whether it's a valid URL format</returns>
        public static bool IsUrl(string url)
        {
            if (StringUtil.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            return UrlRegex.IsMatch(url) && Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }

        /// <summary>
        /// Validate IP address format
        /// </summary>
        /// <param name="ipAddress">IP address</param>
        /// <returns>Whether it's a valid IP address</returns>
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

            // Validate each segment is within 0-255 range
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
        /// Validate if it's a number
        /// </summary>
        /// <param name="value">String to validate</param>
        /// <returns>Whether it's a number</returns>
        public static bool IsNumeric(string value)
        {
            if (StringUtil.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return NumericRegex.IsMatch(value);
        }

        /// <summary>
        /// Validate GUID format
        /// </summary>
        /// <param name="guid">GUID string</param>
        /// <returns>Whether it's a valid GUID format</returns>
        public static bool IsGuid(string guid)
        {
            if (StringUtil.IsNullOrWhiteSpace(guid))
            {
                return false;
            }

            return GuidRegex.IsMatch(guid) || Guid.TryParse(guid, out _);
        }

        /// <summary>
        /// Validate Base64 format
        /// </summary>
        /// <param name="base64">Base64 string</param>
        /// <returns>Whether it's a valid Base64 format</returns>
        public static bool IsBase64(string base64)
        {
            if (StringUtil.IsNullOrWhiteSpace(base64))
            {
                return false;
            }

            // Base64 string length must be a multiple of 4
            if (base64.Length % 4 != 0)
            {
                return false;
            }

            return Base64Regex.IsMatch(base64);
        }

        /// <summary>
        /// Validate JSON format
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <returns>Whether it's a valid JSON format</returns>
        public static bool IsJson(string json)
        {
            if (StringUtil.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            return JsonRegex.IsMatch(json);
        }

        /// <summary>
        /// Validate if value is within specified range
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="value">Value to validate</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <returns>Whether it's within range</returns>
        public static bool IsInRange<T>(T value, T min, T max) where T : IComparable<T>
        {
            if (value == null)
            {
                return false;
            }

            return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
        }

        /// <summary>
        /// Validate if object is null
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Object to validate</param>
        /// <returns>Whether it's null</returns>
        public static bool IsNull<T>(T value)
        {
            return value == null;
        }

        /// <summary>
        /// Validate if object has value (not null)
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Object to validate</param>
        /// <returns>Whether it has value</returns>
        public static bool HasValue<T>(T value)
        {
            return value != null;
        }
    }
}