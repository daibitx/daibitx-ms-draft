using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace Daibitx.EFCore.Extension.Interceptors
{
    public class LoggingInterceptor : DbCommandInterceptor
    {
        private readonly ILogger<LoggingInterceptor> _logger;

        public LoggingInterceptor(ILogger<LoggingInterceptor> logger)
        {
            _logger = logger;
        }

        public override InterceptionResult<DbCommand> CommandCreating(
            CommandCorrelatedEventData eventData,
            InterceptionResult<DbCommand> result)
        {
            _logger.LogDebug("EF SQL: {Command}", result.Result?.CommandText);
            return result;
        }
    }
}
