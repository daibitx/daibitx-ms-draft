using Daibitx.Grpc.Server.Abstraction;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;

namespace Daibitx.Grpc.Server
{
    public static class ServiceExtensions
    {
        public static void MapGrpcServiceAuto(this IEndpointRouteBuilder app)
        {
            var grpcServices = AppDomain.CurrentDomain
                        .GetAssemblies()
                       .SelectMany(a =>
                       {
                           try { return a.GetTypes(); }
                           catch { return Array.Empty<Type>(); }
                       })
                        .Where(p => p.IsClass
                                    && !p.IsAbstract
                                    && typeof(IGrpcServiceMarker).IsAssignableFrom(p))
                        .ToList();
            foreach (var service in grpcServices)
            {
                var method = typeof(GrpcEndpointRouteBuilderExtensions)
                     .GetMethods()
                     .First(p => p.Name == "MapGrpcService" && p.IsGenericMethod && p.GetParameters().Length == 1);
                var genericMethod = method.MakeGenericMethod(service);
                genericMethod.Invoke(null, new object[] { app });
            }
        }
    }
}
