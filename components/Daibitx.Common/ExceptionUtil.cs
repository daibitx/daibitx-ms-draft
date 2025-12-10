using System;
using System.Text;

namespace Daibitx.Common
{
    /// <summary>
    /// Exception handling utility class
    /// Provides common exception related operations
    /// </summary>
    public static class ExceptionUtil
    {
        /// <summary>
        /// Get complete exception information (including all inner exceptions)
        /// </summary>
        /// <param name="exception">Exception object</param>
        /// <returns>Complete exception information string</returns>
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
        /// Get exception stack trace information
        /// </summary>
        /// <param name="exception">Exception object</param>
        /// <returns>Stack trace string</returns>
        public static string GetStackTrace(Exception exception)
        {
            return exception?.StackTrace ?? string.Empty;
        }

        /// <summary>
        /// Check if it's a critical exception (e.g., OutOfMemoryException, StackOverflowException, etc.)
        /// </summary>
        /// <param name="exception">Exception object</param>
        /// <returns>Whether it's a critical exception</returns>
        public static bool IsCritical(Exception exception)
        {
            if (exception == null)
            {
                return false;
            }

            var exceptionType = exception.GetType();
            
            // Critical exception types
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
        /// Try to execute action, return whether successful
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <returns>Whether execution was successful</returns>
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
        /// Try to execute function and return value, return whether successful
        /// </summary>
        /// <typeparam name="T">Return value type</typeparam>
        /// <param name="func">Function to execute</param>
        /// <param name="result">Execution result</param>
        /// <returns>Whether execution was successful</returns>
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
        /// Wrap exception with custom error message
        /// </summary>
        /// <typeparam name="T">Exception type</typeparam>
        /// <param name="action">Action to execute</param>
        /// <param name="errorMessage">Custom error message</param>
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
        /// Wrap exception with custom error message (generic version)
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <param name="errorMessage">Custom error message</param>
        /// <param name="exceptionType">Exception type</param>
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