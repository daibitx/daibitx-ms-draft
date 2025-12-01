using System;
using System.Text;

namespace Daibitx.Common
{
    /// <summary>
    /// 异常处理工具类
    /// 提供异常相关的常用操作方法
    /// </summary>
    public static class ExceptionUtil
    {
        /// <summary>
        /// 获取异常的完整信息（包括所有内部异常）
        /// </summary>
        /// <param name="exception">异常对象</param>
        /// <returns>完整的异常信息字符串</returns>
        public static string GetFullMessage(Exception exception)
        {
            if (exception == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            var currentException = exception;
            var level = 0;

            while (currentException != null)
            {
                if (level > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine($"--- Inner Exception Level {level} ---");
                }

                sb.AppendLine($"Exception Type: {currentException.GetType().FullName}");
                sb.AppendLine($"Message: {currentException.Message}");
                sb.AppendLine($"Source: {currentException.Source}");
                
                if (!string.IsNullOrWhiteSpace(currentException.StackTrace))
                {
                    sb.AppendLine("Stack Trace:");
                    sb.AppendLine(currentException.StackTrace);
                }

                currentException = currentException.InnerException;
                level++;
            }

            return sb.ToString();
        }

        /// <summary>
        /// 获取异常的堆栈跟踪信息
        /// </summary>
        /// <param name="exception">异常对象</param>
        /// <returns>堆栈跟踪字符串</returns>
        public static string GetStackTrace(Exception exception)
        {
            return exception?.StackTrace ?? string.Empty;
        }

        /// <summary>
        /// 判断是否为关键异常（如OutOfMemoryException、StackOverflowException等）
        /// </summary>
        /// <param name="exception">异常对象</param>
        /// <returns>是否为关键异常</returns>
        public static bool IsCritical(Exception exception)
        {
            if (exception == null)
            {
                return false;
            }

            var exceptionType = exception.GetType();
            
            // 关键异常类型
            return exceptionType == typeof(OutOfMemoryException) ||
                   exceptionType == typeof(StackOverflowException) ||
                   exceptionType == typeof(ThreadAbortException) ||
                   exceptionType == typeof(AccessViolationException) ||
                   exceptionType == typeof(AppDomainUnloadedException) ||
                   exceptionType == typeof(BadImageFormatException) ||
                   exceptionType == typeof(CannotUnloadAppDomainException) ||
                   exceptionType == typeof(ExecutionEngineException) ||
                   exceptionType == typeof(InvalidProgramException);
        }

        /// <summary>
        /// 尝试执行操作，返回是否成功
        /// </summary>
        /// <param name="action">要执行的操作</param>
        /// <returns>执行是否成功</returns>
        public static bool TryExecute(Action action)
        {
            if (action == null)
            {
                return false;
            }

            try
            {
                action();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 尝试执行操作并返回值，返回是否成功
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="func">要执行的函数</param>
        /// <param name="result">执行结果</param>
        /// <returns>执行是否成功</returns>
        public static bool TryExecute<T>(Func<T> func, out T result)
        {
            result = default(T);

            if (func == null)
            {
                return false;
            }

            try
            {
                result = func();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 包装异常，添加自定义错误信息
        /// </summary>
        /// <typeparam name="T">异常类型</typeparam>
        /// <param name="action">要执行的操作</param>
        /// <param name="errorMessage">自定义错误信息</param>
        public static void WrapException<T>(Action action, string errorMessage) where T : Exception, new()
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            try
            {
                action();
            }
            catch (Exception ex)
            {
                var wrappedException = Activator.CreateInstance(typeof(T), errorMessage, ex) as T;
                throw wrappedException ?? new Exception(errorMessage, ex);
            }
        }

        /// <summary>
        /// 包装异常，添加自定义错误信息（泛型版本）
        /// </summary>
        /// <param name="action">要执行的操作</param>
        /// <param name="errorMessage">自定义错误信息</param>
        /// <param name="exceptionType">异常类型</param>
        public static void WrapException(Action action, string errorMessage, Type exceptionType)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (exceptionType == null || !typeof(Exception).IsAssignableFrom(exceptionType))
            {
                throw new ArgumentException("Invalid exception type", nameof(exceptionType));
            }

            try
            {
                action();
            }
            catch (Exception ex)
            {
                var wrappedException = Activator.CreateInstance(exceptionType, errorMessage, ex) as Exception;
                throw wrappedException ?? new Exception(errorMessage, ex);
            }
        }
    }
}