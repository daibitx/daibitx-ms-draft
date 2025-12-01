using System;
using System.Globalization;

namespace Daibitx.Common
{
    /// <summary>
    /// 日期时间工具类
    /// 提供日期时间相关的常用操作方法
    /// </summary>
    public static class DateTimeUtil
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// 获取当前时间戳（毫秒）
        /// </summary>
        /// <returns>当前时间的Unix毫秒时间戳</returns>
        public static long GetCurrentTimestamp()
        {
            return ToUnixTimeMilliseconds(DateTime.UtcNow);
        }

        /// <summary>
        /// 将DateTime转换为Unix秒时间戳
        /// </summary>
        /// <param name="dateTime">要转换的日期时间</param>
        /// <returns>Unix秒时间戳</returns>
        public static long ToUnixTimeSeconds(DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
            {
                return 0;
            }

            var utcDateTime = dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime();
            return (long)(utcDateTime - UnixEpoch).TotalSeconds;
        }

        /// <summary>
        /// 将DateTime转换为Unix毫秒时间戳
        /// </summary>
        /// <param name="dateTime">要转换的日期时间</param>
        /// <returns>Unix毫秒时间戳</returns>
        public static long ToUnixTimeMilliseconds(DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
            {
                return 0;
            }

            var utcDateTime = dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime();
            return (long)(utcDateTime - UnixEpoch).TotalMilliseconds;
        }

        /// <summary>
        /// 从Unix秒时间戳转换为DateTime
        /// </summary>
        /// <param name="unixSeconds">Unix秒时间戳</param>
        /// <returns>转换后的DateTime（UTC时间）</returns>
        public static DateTime FromUnixTimeSeconds(long unixSeconds)
        {
            if (unixSeconds < 0)
            {
                return DateTime.MinValue;
            }

            try
            {
                return UnixEpoch.AddSeconds(unixSeconds);
            }
            catch
            {
                return DateTime.MaxValue;
            }
        }

        /// <summary>
        /// 从Unix毫秒时间戳转换为DateTime
        /// </summary>
        /// <param name="unixMilliseconds">Unix毫秒时间戳</param>
        /// <returns>转换后的DateTime（UTC时间）</returns>
        public static DateTime FromUnixTimeMilliseconds(long unixMilliseconds)
        {
            if (unixMilliseconds < 0)
            {
                return DateTime.MinValue;
            }

            try
            {
                return UnixEpoch.AddMilliseconds(unixMilliseconds);
            }
            catch
            {
                return DateTime.MaxValue;
            }
        }

        /// <summary>
        /// 格式化日期时间
        /// </summary>
        /// <param name="dateTime">要格式化的日期时间</param>
        /// <param name="format">格式字符串</param>
        /// <returns>格式化后的字符串</returns>
        public static string Format(DateTime dateTime, string format)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }

            try
            {
                return dateTime.ToString(format);
            }
            catch
            {
                return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 解析日期时间字符串
        /// </summary>
        /// <param name="dateTimeString">日期时间字符串</param>
        /// <param name="format">格式字符串</param>
        /// <returns>解析后的DateTime</returns>
        public static DateTime Parse(string dateTimeString, string format)
        {
            if (string.IsNullOrWhiteSpace(dateTimeString))
            {
                return DateTime.MinValue;
            }

            if (string.IsNullOrWhiteSpace(format))
            {
                if (DateTime.TryParse(dateTimeString, out var result))
                {
                    return result;
                }
                return DateTime.MinValue;
            }

            try
            {
                return DateTime.ParseExact(dateTimeString, format, CultureInfo.InvariantCulture, DateTimeStyles.None);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// 判断两个日期是否为同一天
        /// </summary>
        /// <param name="dateTime1">第一个日期</param>
        /// <param name="dateTime2">第二个日期</param>
        /// <returns>是否为同一天</returns>
        public static bool IsSameDay(DateTime dateTime1, DateTime dateTime2)
        {
            return dateTime1.Date == dateTime2.Date;
        }

        /// <summary>
        /// 获取当天的开始时间（00:00:00）
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>当天的开始时间</returns>
        public static DateTime GetStartOfDay(DateTime dateTime)
        {
            return dateTime.Date;
        }

        /// <summary>
        /// 获取当天的结束时间（23:59:59.999）
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>当天的结束时间</returns>
        public static DateTime GetEndOfDay(DateTime dateTime)
        {
            return dateTime.Date.AddDays(1).AddTicks(-1);
        }
    }
}