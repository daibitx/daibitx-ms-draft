using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Daibitx.Localizator
{
    /// <summary>
    /// ServiceExtension for resx localization foe aspnetcore.webapi 
    /// </summary>
    public static class ApplicationServiceExtension
    {
        public static void AddResxLocalization(this IServiceCollection services, Action<ResxLocalizationOptions>? configure = null)
        {
            var options = new ResxLocalizationOptions();
            configure?.Invoke(options);
            services.AddSingleton(options);
            services.AddSingleton<ResxResourceProvider>();
            services.AddSingleton<IStringLocalizer, ResxStringLocalizer>();
        }

    }
}
