using Grpc.Core;
using Grpc.Net.Client;
using System.Net.Security;

namespace Daibitx.Grpc.Client
{
    public static class CallInvokerFactory
    {
        public static CallInvoker Create(string baseAddress, GrpcClientOptions? options = null)
        {
            options ??= new GrpcClientOptions();

            var handler = new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = options.EnableMultipleHttp2Connections ?? true,
                MaxConnectionsPerServer = options.MaxConnectionsPerServer ?? 100,
                PooledConnectionIdleTimeout = options.PooledConnectionIdleTimeout ?? TimeSpan.FromMinutes(5),
                KeepAlivePingDelay = options.KeepAlivePingDelay ?? TimeSpan.FromSeconds(60),
                KeepAlivePingTimeout = options.KeepAlivePingTimeout ?? TimeSpan.FromSeconds(30)
            };

            // SSL 配置
            if (options.IgnoreSslErrors == true)
            {
                handler.SslOptions = new SslClientAuthenticationOptions
                {
                    RemoteCertificateValidationCallback = (_, _, _, _) => true
                };
            }

            var channel = GrpcChannel.ForAddress(baseAddress, new GrpcChannelOptions
            {
                HttpHandler = handler,
                DisposeHttpClient = true,
                MaxReceiveMessageSize = options.MaxReceiveMessageSize,
                MaxSendMessageSize = options.MaxSendMessageSize
            });

            return channel.CreateCallInvoker();
        }
    }
}
