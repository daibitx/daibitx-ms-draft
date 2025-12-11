namespace Daibitx.Grpc.Client
{
    public class GrpcClientOptions
    {
        public bool? EnableMultipleHttp2Connections { get; set; }
        public int? MaxConnectionsPerServer { get; set; }
        public TimeSpan? PooledConnectionIdleTimeout { get; set; }
        public TimeSpan? KeepAlivePingDelay { get; set; }
        public TimeSpan? KeepAlivePingTimeout { get; set; }
        public int? MaxReceiveMessageSize { get; set; }
        public int? MaxSendMessageSize { get; set; }
        public bool? IgnoreSslErrors { get; set; }
    }
}
