using System.Net;

namespace Clientus.ApiClient.Common;

public class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public string? ResponseBody { get; }

    public ApiException(
        string message,
        HttpStatusCode statusCode,
        string? responseBody = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}