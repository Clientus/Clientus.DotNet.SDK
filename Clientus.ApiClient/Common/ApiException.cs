using System.Net;

namespace Clientus.ApiClient.Common;

/// <summary>
/// Represents an error returned by the Clientus API.
/// </summary>
public class ApiException : Exception
{
    /// <summary>
    /// Gets the HTTP status code returned by the server.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets the raw response body.
    /// </summary>
    public string? ResponseBody { get; }

    /// <summary>
    /// Initializes a new API exception.
    /// </summary>
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