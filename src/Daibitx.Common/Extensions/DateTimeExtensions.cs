using System;

namespace Daibitx.Common.Extensions
{
    /// <summary>
    /// DateTime extension methods
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Convert DateTime to Unix second timestamp
        /// </summary>
        /// <param name="dateTime">DateTime</param>
        /// <returns>Unix second timestamp</returns>
        public static long ToUnixTimeSeconds(this DateTime dateTime)
        {
            return DateTimeUtil.ToUnixTimeSeconds(dateTime);
        }

        /// <summary>
        /// Convert DateTime to Unix millisecond timestamp
        /// </summary>
        /// <param name="dateTime">DateTime</param>
        /// <returns>Unix millisecond timestamp</returns>
        public static long ToUnixTimeMilliseconds(this DateTime dateTime)
        {
            return DateTimeUtil.ToUnixTimeMilliseconds(dateTime);
        }

        /// <summary>
        /// Format DateTime
        /// </summary>
        /// <param name="dateTime">DateTime</param>
        /// <param name="format">Format string</param>
        /// <returns>Formatted string</returns>
        public static string Format(this DateTime dateTime, string format)
        {
            return DateTimeUtil.Format(dateTime, format);
        }

        /// <summary>
        /// Check if two dates are the same day
        /// </summary>
        /// <param name="dateTime1">First date</param>
        /// <param name="dateTime2">Second date</param>
        /// <returns>Whether they are the same day</returns>
        public static bool IsSameDay(this DateTime dateTime1, DateTime dateTime2)
        {
            return DateTimeUtil.IsSameDay(dateTime1, dateTime2);
        }

        /// <summary>
        /// Get start of day (00:00:00)
        /// </summary>
        /// <param name="dateTime">DateTime</param>
        /// <returns>Start of day</returns>
        public static DateTime GetStartOfDay(this DateTime dateTime)
        {
            return DateTimeUtil.GetStartOfDay(dateTime);
        }

        /// <summary>
        /// Get end of day (23:59:59.999)
        /// </summary>
        /// <param name="dateTime">DateTime</param>
        /// <returns>End of day</returns>
        public static DateTime GetEndOfDay(this DateTime dateTime)
        {
            return DateTimeUtil.GetEndOfDay(dateTime);
        }
    }
}