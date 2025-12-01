using System;
using System.ComponentModel;
using System.Globalization;

namespace Daibitx.Common
{
    /// <summary>
    /// 类型转换工具类
    /// 提供安全且灵活的类型转换方法
    /// </summary>
    public static class ConvertUtil
    {
        /// <summary>
        /// 安全转换为指定类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="value">要转换的值</param>
        /// <returns>转换后的值，如果转换失败则返回类型的默认值</returns>
        public static T To<T>(object value)
        {
            return To(value, default(T));
        }

        /// <summary>
        /// 安全转换为指定类型，并提供默认值
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="value">要转换的值</param>
        /// <param name="defaultValue">转换失败时的默认值</param>
        /// <returns>转换后的值，如果转换失败则返回默认值</returns>
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
        /// 转换为Int32类型
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns>转换后的Int32值，失败返回0</returns>
        public static int ToInt32(object value)
        {
            return To<int>(value);
        }

        /// <summary>
        /// 转换为Int64类型
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns>转换后的Int64值，失败返回0</returns>
        public static long ToInt64(object value)
        {
            return To<long>(value);
        }

        /// <summary>
        /// 转换为Decimal类型
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns>转换后的Decimal值，失败返回0</returns>
        public static decimal ToDecimal(object value)
        {
            return To<decimal>(value);
        }

        /// <summary>
        /// 转换为Double类型
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns>转换后的Double值，失败返回0</returns>
        public static double ToDouble(object value)
        {
            return To<double>(value);
        }

        /// <summary>
        /// 转换为Boolean类型
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns>转换后的Boolean值，失败返回false</returns>
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
        /// 转换为DateTime类型
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns>转换后的DateTime值，失败返回DateTime.MinValue</returns>
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
                // 尝试作为时间戳解析
                if (timestamp > 10000000000L) // 毫秒时间戳
                {
                    return DateTimeUtil.FromUnixTimeMilliseconds(timestamp);
                }
                else // 秒时间戳
                {
                    return DateTimeUtil.FromUnixTimeSeconds(timestamp);
                }
            }

            return To<DateTime>(value);
        }

        /// <summary>
        /// 转换为String类型
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns>转换后的String值，失败返回空字符串</returns>
        public static string ToString(object value)
        {
            return To<string>(value, string.Empty);
        }

        /// <summary>
        /// 转换为指定类型
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <param name="targetType">目标类型</param>
        /// <returns>转换后的值，如果转换失败则返回null</returns>
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
        /// 尝试转换为指定类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="value">要转换的值</param>
        /// <param name="result">转换后的结果</param>
        /// <returns>转换是否成功</returns>
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