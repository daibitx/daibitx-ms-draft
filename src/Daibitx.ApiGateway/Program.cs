using Daibitx.Nuxus.Hmac.Daibitx.Security.Hmac;
using Daibitx.Security.Hmac.Abstractions;
using System.Text;
using System.Threading.RateLimiting;
using Yarp.ReverseProxy.Transforms;

namespace Daibitx.ApiGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHmac(option =>
            {
                option.SecretKey = builder.Configuration.GetValue<string>("hmacsecert") ?? "6ICB5p2/5piv5Liq5aSn5YK76YC8";
            });

            builder.Host.UseAgileConfig();

            builder.Services.AddRateLimiter(options =>
            {
                var config = builder.Configuration.GetSection("RateLimit");

                int permitLimit = config.GetValue<int>("PermitLimit", 1000);
                int window = config.GetValue<int>("Window", 10);
                int segmentCount = config.GetValue<int>("SegmentsPerWindow", 10);
                int queueLimit = config.GetValue<int>("QueueLimit", 0);

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: ip,
                        factory: _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = permitLimit,
                            Window = TimeSpan.FromSeconds(window),
                            SegmentsPerWindow = segmentCount,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = queueLimit
                        }
                    );
                });

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });

            builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
               .AddTransforms(transformBuilderContext =>
               {
                   transformBuilderContext.AddRequestTransform(async transformContext =>
                   {
                       var req = transformContext.HttpContext.Request;
                       var config = transformContext.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                       var hmacValidator = transformContext.HttpContext.RequestServices.GetRequiredService<IHmacValidator>();
                       var hmacGenerater = transformContext.HttpContext.RequestServices.GetRequiredService<IHmacGenerator>();
                       string body = "";
                       string method = req.Method;
                       string path = req.Path;
                       if (req.ContentLength > 0)
                       {
                           req.EnableBuffering();
                           using var reader = new StreamReader(req.Body, Encoding.UTF8, leaveOpen: true);
                           body = await reader.ReadToEndAsync();
                           req.Body.Position = 0;
                       }
                       var hmacSet = hmacGenerater.GenerateHeaderSet(method, path, body);
                       foreach (var kv in hmacSet.ToDictionary())
                       {
                           transformContext.ProxyRequest.Headers.Add(kv.Key, kv.Value);
                       }
                   });
               });

            var app = builder.Build();

            app.UseRateLimiter();

            app.MapReverseProxy();

            app.Run();
        }
    }
}
