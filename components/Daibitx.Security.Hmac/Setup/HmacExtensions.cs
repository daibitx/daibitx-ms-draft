namespace Daibitx.Nuxus.Hmac
{
    using global::Daibitx.Security.Hmac;
    using global::Daibitx.Security.Hmac.Abstractions;
    using global::Daibitx.Security.Hmac.Implementations;
    using global::Daibitx.Security.Hmac.Models;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    namespace Daibitx.Security.Hmac
    {
        public static class HmacExtensions
        {
            private static IServiceCollection AddHmacCore(this IServiceCollection services)
            {
                services.AddSingleton<IHmacValidator, HmacValidator>();
                services.AddSingleton<IHmacGenerator, HmacGenerator>();
                return services;
            }
            public static IServiceCollection AddHmac(this IServiceCollection services, IConfiguration config)
            {
                services.Configure<HmacOptions>(config.GetSection("Hmac"));
                return services.AddHmacCore();
            }

            public static IServiceCollection AddHmac(this IServiceCollection services, Action<HmacOptions> configure)
            {
                services.Configure<HmacOptions>(configure);
                return services.AddHmacCore();
            }


            public static IApplicationBuilder UseHmacValidation(this IApplicationBuilder app)
            {
                return app.UseMiddleware<HmacValidationMiddleware>();
            }
        }
    }

}
