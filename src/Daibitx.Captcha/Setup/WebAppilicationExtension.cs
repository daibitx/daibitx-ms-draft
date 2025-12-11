using Daibitx.Captcha.Abstractions;
using Daibitx.Captcha.Skia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Daibitx.Captcha.Setup
{
    public static class WebAppilicationExtension
    {
        /// <summary>
        /// It provider a service named ICaptchaGenerator to create captcha
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        public static void AddCaptchaApi(this IServiceCollection services, Action<CaptchaOptions> options)
        {
            var captchaOptions = new CaptchaOptions();
            options.Invoke(captchaOptions);
            services.Configure<CaptchaOptions>(options);
            services.AddScoped<ICaptchaGenerator>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<CaptchaOptions>>();
                var generator = new SkiaCaptchaGenerator(options.Value);
                return generator;
            });
        }
    }

}
