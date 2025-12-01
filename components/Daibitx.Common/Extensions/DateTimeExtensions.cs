using System;

namespace Daibitx.Common.Extensions
{
    /// <summary>
    /// 日期时间扩展方法
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// 将DateTime转换为Unix秒时间戳
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>Unix秒时间戳</returns>
        public static long ToUnixTimeSeconds(this DateTime dateTime)
        {
            return DateTimeUtil.ToUnixTimeSeconds(dateTime);
        }

        /// <summary>
        /// 将DateTime转换为Unix毫秒时间戳
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>Unix毫秒时间戳</returns>
        public static long ToUnixTimeMilliseconds(this DateTime dateTime)
        {
            return DateTimeUtil.ToUnixTimeMilliseconds(dateTime);
        }

        /// <summary>
        /// 格式化日期时间
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <param name="format">格式字符串</param>
        /// <returns>格式化后的字符串</returns>
        public static string Format(this DateTime dateTime, string format)
        {
            return DateTimeUtil.Format(dateTime, format);
        }

        /// <summary>
        /// 判断两个日期是否为同一天
        /// </summary>
        /// <param name="dateTime1">第一个日期</param>
        /// <param name="dateTime2">第二个日期</param>
        /// <returns>是否为同一天</returns>
        public static bool IsSameDay(this DateTime dateTime1, DateTime dateTime2)
        {
            return DateTimeUtil.IsSameDay(dateTime1, dateTime2);
        }

        /// <summary>
        /// 获取当天的开始时间（00:00:00）
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>当天的开始时间</returns>
        public static DateTime GetStartOfDay(this DateTime dateTime)
        {
            return DateTimeUtil.GetStartOfDay(dateTime);
        }

        /// <summary>
        /// 获取当天的结束时间（23:59:59.999）
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>当天的结束时间</returns>
        public static DateTime GetEndOfDay(this DateTime dateTime)
        {
            return DateTimeUtil.GetEndOfDay(dateTime);
        }
    }
}