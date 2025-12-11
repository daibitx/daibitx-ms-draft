namespace Daibitx.Grpc.Client.Exceptions
{
    public class ServiceUnavailableException : Exception
    {
        public string ServiceName { get; }

        public ServiceUnavailableException(string serviceName, Exception? innerException = null)
            : base($"Service {serviceName} is unavailable", innerException)
        {
            ServiceName = serviceName;
        }
    }
}
