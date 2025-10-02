namespace PosHubApi.Errors
{
    public class ClientNotFoundException : Exception
    {
        public ClientNotFoundException(string message = "Client not found.") : base(message) { }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message = "Resource not found.") : base(message) { }
    }

    public class RateLimitExceededException : Exception
    {
        public RateLimitExceededException(string message = "Rate limit exceeded.") : base(message) { }
    }

    public class BadGatewayException : Exception
    {
        public BadGatewayException(string message = "Bad gateway.") : base(message) { }
    }

    public class ServiceUnavailableException : Exception
    {
        public ServiceUnavailableException(string message = "Service unavailable.") : base(message) { }
    }

    public class GatewayTimeoutException : Exception
    {
        public GatewayTimeoutException(string message = "Gateway timeout.") : base(message) { }
    }


}