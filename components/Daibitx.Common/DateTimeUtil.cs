using System;
using System.Globalization;

namespace Daibitx.Common
{
    /// <summary>
    /// DateTime utility class
    /// Provides common DateTime related operations
    /// </summary>
    public static class DateTimeUtil
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Get current timestamp (milliseconds)
        /// </summary>
        /// <returns>Current Unix millisecond timestamp</returns>
        public static long GetCurrentTimestamp()
        {
            return ToUnixTimeMilliseconds(DateTime.UtcNow);
        }

        /// <summary>
        /// Convert DateTime to Unix second timestamp
        /// </summary>
        /// <param name="dateTime">DateTime to convert</param>
        /// <returns>Unix second timestamp</returns>
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
        /// Convert DateTime to Unix millisecond timestamp
        /// </summary>
        /// <param name="dateTime">DateTime to convert</param>
        /// <returns>Unix millisecond timestamp</returns>
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
        /// Convert from Unix second timestamp to DateTime
        /// </summary>
        /// <param name="unixSeconds">Unix second timestamp</param>
        /// <returns>Converted DateTime (UTC)</returns>
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
        /// Convert from Unix millisecond timestamp to DateTime
        /// </summary>
        /// <param name="unixMilliseconds">Unix millisecond timestamp</param>
        /// <returns>Converted DateTime (UTC)</returns>
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
        /// Format DateTime
        /// </summary>
        /// <param name="dateTime">DateTime to format</param>
        /// <param name="format">Format string</param>
        /// <returns>Formatted string</returns>
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
        /// Parse DateTime string
        /// </summary>
        /// <param name="dateTimeString">DateTime string</param>
        /// <param name="format">Format string</param>
        /// <returns>Parsed DateTime</returns>
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
        /// Check if two dates are the same day
        /// </summary>
        /// <param name="dateTime1">First date</param>
        /// <param name="dateTime2">Second date</param>
        /// <returns>Whether they are the same day</returns>
        public static bool IsSameDay(DateTime dateTime1, DateTime dateTime2)
        {
            return dateTime1.Date == dateTime2.Date;
        }

        /// <summary>
        /// Get start of day (00:00:00)
        /// </summary>
        /// <param name="dateTime">DateTime</param>
        /// <returns>Start of day</returns>
        public static DateTime GetStartOfDay(DateTime dateTime)
        {
            return dateTime.Date;
        }

        /// <summary>
        /// Get end of day (23:59:59.999)
        /// </summary>
        /// <param name="dateTime">DateTime</param>
        /// <returns>End of day</returns>
        public static DateTime GetEndOfDay(DateTime dateTime)
        {
            return dateTime.Date.AddDays(1).AddTicks(-1);
        }
    }
}