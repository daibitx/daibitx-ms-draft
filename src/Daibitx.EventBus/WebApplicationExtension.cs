using Daibitx.Extension.Modularize.EventBus.Abstraction;
using Microsoft.Extensions.DependencyInjection;

namespace Daibitx.Extension.Modularize.EventBus
{
    public static class WebApplicationExtension
    {
        public static void AddCrossEventBus(this IServiceCollection services)
        {
            services.AddSingleton<IEventBusFactory>((serviceProvider) =>
            {
                return Event.CrossEventInstance;
            });
        }
    }
}
