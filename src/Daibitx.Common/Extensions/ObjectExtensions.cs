using System;

namespace Daibitx.Common.Extensions
{
    /// <summary>
    /// 对象扩展方法
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// 安全转换为指定类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="value">要转换的值</param>
        /// <returns>转换后的值，如果转换失败则返回类型的默认值</returns>
        public static T To<T>(this object value)
        {
            return ConvertUtil.To<T>(value);
        }

        /// <summary>
        /// 安全转换为指定类型，并提供默认值
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="value">要转换的值</param>
        /// <param name="defaultValue">转换失败时的默认值</param>
        /// <returns>转换后的值，如果转换失败则返回默认值</returns>
        public static T To<T>(this object value, T defaultValue)
        {
            return ConvertUtil.To(value, defaultValue);
        }

        /// <summary>
        /// 转换为Int32类型
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns>转换后的Int32值，失败返回0</returns>
        public static int ToInt32(this object value)
        {
            return ConvertUtil.ToInt32(value);
        }

        /// <summary>
        /// 转换为Int64类型
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns>转换后的Int64值，失败返回0</returns>
        public static long ToInt64(this object value)
        {
            return ConvertUtil.ToInt64(value);
        }

        /// <summary>
        /// 转换为Decimal类型
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns>转换后的Decimal值，失败返回0</returns>
        public static decimal ToDecimal(this object value)
        {
            return ConvertUtil.ToDecimal(value);
        }

        /// <summary>
        /// 转换为Double类型
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns>转换后的Double值，失败返回0</returns>
        public static double ToDouble(this object value)
        {
            return ConvertUtil.ToDouble(value);
        }

        /// <summary>
        /// 转换为Boolean类型
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns>转换后的Boolean值，失败返回false</returns>
        public static bool ToBoolean(this object value)
        {
            return ConvertUtil.ToBoolean(value);
        }

        /// <summary>
        /// 转换为DateTime类型
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns>转换后的DateTime值，失败返回DateTime.MinValue</returns>
        public static DateTime ToDateTime(this object value)
        {
            return ConvertUtil.ToDateTime(value);
        }

        /// <summary>
        /// 转换为String类型
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns>转换后的String值，失败返回空字符串</returns>
        public static string ToString(this object value)
        {
            return ConvertUtil.ToString(value);
        }

        /// <summary>
        /// 尝试转换为指定类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="value">要转换的值</param>
        /// <param name="result">转换后的结果</param>
        /// <returns>转换是否成功</returns>
        public static bool TryConvert<T>(this object value, out T result)
        {
            return ConvertUtil.TryConvert(value, out result);
        }

        /// <summary>
        /// 验证对象是否为null
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="value">要验证的对象</param>
        /// <returns>是否为null</returns>
        public static bool IsNull<T>(this T value)
        {
            return ValidationUtil.IsNull(value);
        }

        /// <summary>
        /// 验证对象是否有值（不为null）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="value">要验证的对象</param>
        /// <returns>是否有值</returns>
        public static bool HasValue<T>(this T value)
        {
            return ValidationUtil.HasValue(value);
        }
    }
}