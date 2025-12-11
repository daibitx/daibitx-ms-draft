namespace Daibitx.Grpc.Server.Exceptions
{
    public class BusinessException : Exception
    {
        public int ErrorCode { get; }
        public object? Details { get; }

        public BusinessException(string message, int errorCode = 400, object? details = null)
            : base(message)
        {
            ErrorCode = errorCode;
            Details = details;
        }
    }

}
