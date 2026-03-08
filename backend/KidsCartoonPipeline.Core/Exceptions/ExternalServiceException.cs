namespace KidsCartoonPipeline.Core.Exceptions;

public class ExternalServiceException : Exception
{
    public string ServiceName { get; }
    public int? StatusCode { get; }

    public ExternalServiceException(string serviceName, string message, int? statusCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        ServiceName = serviceName;
        StatusCode = statusCode;
    }
}
