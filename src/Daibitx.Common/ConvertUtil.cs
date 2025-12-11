using System;
using System.ComponentModel;
using System.Globalization;

namespace Daibitx.Common
{
    /// <summary>
    /// Type conversion utility class
    /// Provides safe and flexible type conversion methods
    /// </summary>
    public static class ConvertUtil
    {
        /// <summary>
        /// Safely convert to specified type
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="value">Value to convert</param>
        /// <returns>Converted value, returns default value of type if conversion fails</returns>
        public static T To<T>(object value)
        {
            return To(value, default(T));
        }

        /// <summary>
        /// Safely convert to specified type with default value
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value when conversion fails</param>
        /// <returns>Converted value, returns default value if conversion fails</returns>
        public static T To<T>(object value, T defaultValue)
        {
            if (value == null || value == DBNull.Value)
            {
                return defaultValue;
            }

            try
            {
                var targetType = typeof(T);
                var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

                if (underlyingType.IsEnum)
                {
                    return (T)Enum.Parse(underlyingType, value.ToString(), true);
                }

                var converter = TypeDescriptor.GetConverter(underlyingType);
                if (converter != null && converter.CanConvertFrom(value.GetType()))
                {
                    return (T)converter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
                }

                return (T)Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Convert to Int32 type
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Converted Int32 value, returns 0 on failure</returns>
        public static int ToInt32(object value)
        {
            return To<int>(value);
        }

        /// <summary>
        /// Convert to Int64 type
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Converted Int64 value, returns 0 on failure</returns>
        public static long ToInt64(object value)
        {
            return To<long>(value);
        }

        /// <summary>
        /// Convert to Decimal type
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Converted Decimal value, returns 0 on failure</returns>
        public static decimal ToDecimal(object value)
        {
            return To<decimal>(value);
        }

        /// <summary>
        /// Convert to Double type
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Converted Double value, returns 0 on failure</returns>
        public static double ToDouble(object value)
        {
            return To<double>(value);
        }

        /// <summary>
        /// Convert to Boolean type
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Converted Boolean value, returns false on failure</returns>
        public static bool ToBoolean(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return false;
            }

            var stringValue = value.ToString().ToLowerInvariant();
            if (stringValue == "true" || stringValue == "1" || stringValue == "yes" || stringValue == "on")
            {
                return true;
            }

            if (stringValue == "false" || stringValue == "0" || stringValue == "no" || stringValue == "off")
            {
                return false;
            }

            return To<bool>(value);
        }

        /// <summary>
        /// Convert to DateTime type
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Converted DateTime value, returns DateTime.MinValue on failure</returns>
        public static DateTime ToDateTime(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return DateTime.MinValue;
            }

            if (value is DateTime dateTime)
            {
                return dateTime;
            }

            var stringValue = value.ToString();
            if (long.TryParse(stringValue, out var timestamp))
            {
                // Try to parse as timestamp
                if (timestamp > 10000000000L) // Millisecond timestamp
                {
                    return DateTimeUtil.FromUnixTimeMilliseconds(timestamp);
                }
                else // Second timestamp
                {
                    return DateTimeUtil.FromUnixTimeSeconds(timestamp);
                }
            }

            return To<DateTime>(value);
        }

        /// <summary>
        /// Convert to String type
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Converted String value, returns empty string on failure</returns>
        public static string ToString(object value)
        {
            return To<string>(value, string.Empty);
        }

        /// <summary>
        /// Convert to specified type
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="targetType">Target type</param>
        /// <returns>Converted value, returns null if conversion fails</returns>
        public static object ChangeType(object value, Type targetType)
        {
            if (value == null || value == DBNull.Value)
            {
                return null;
            }

            if (targetType == null)
            {
                return null;
            }

            try
            {
                var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

                if (underlyingType.IsEnum)
                {
                    return Enum.Parse(underlyingType, value.ToString(), true);
                }

                var converter = TypeDescriptor.GetConverter(underlyingType);
                if (converter != null && converter.CanConvertFrom(value.GetType()))
                {
                    return converter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
                }

                return Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Try to convert to specified type
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="value">Value to convert</param>
        /// <param name="result">Conversion result</param>
        /// <returns>Whether conversion was successful</returns>
        public static bool TryConvert<T>(object value, out T result)
        {
            result = default(T);

            if (value == null || value == DBNull.Value)
            {
                return false;
            }

            try
            {
                var targetType = typeof(T);
                var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

                if (underlyingType.IsEnum)
                {
                    result = (T)Enum.Parse(underlyingType, value.ToString(), true);
                    return true;
                }

                var converter = TypeDescriptor.GetConverter(underlyingType);
                if (converter != null && converter.CanConvertFrom(value.GetType()))
                {
                    result = (T)converter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
                    return true;
                }

                result = (T)Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}