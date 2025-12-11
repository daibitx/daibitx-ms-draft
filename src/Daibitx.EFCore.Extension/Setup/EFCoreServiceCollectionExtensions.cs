using Daibitx.EFCore.Extension.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Daibitx.EFCore.Extension.Setup
{
    public static class EFCoreServiceCollectionExtensions
    {
        public static IServiceCollection AddDaibitxEFCore(this IServiceCollection services)
        {
            services.AddSingleton<AuditingInterceptor>();
            services.AddSingleton<SoftDeleteInterceptor>();
            services.AddSingleton<LoggingInterceptor>();

            return services;
        }

        public static DbContextOptionsBuilder AddDaibitxInterceptors(
            this DbContextOptionsBuilder options,
            IServiceProvider sp)
        {
            return options
                .AddInterceptors(sp.GetRequiredService<AuditingInterceptor>())
                .AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>())
                .AddInterceptors(sp.GetRequiredService<LoggingInterceptor>());
        }
    }
}
