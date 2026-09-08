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

    /// <summary>Gets the stable server error code, when supplied by the server.</summary>
    public string? ErrorCode { get; }

    /// <summary>Gets the server correlation or request identifier, when supplied.</summary>
    public string? RequestId { get; }

    /// <summary>Gets a value indicating whether retrying the operation may be safe.</summary>
    public bool IsRetryable { get; }

    /// <summary>Gets the server-requested retry delay, when supplied.</summary>
    public TimeSpan? RetryAfter { get; }

    /// <summary>Gets structured validation details when supplied by the server.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>>? ValidationErrors { get; }

    /// <summary>
    /// Initializes a new API exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="statusCode">The HTTP status returned by the API.</param>
    /// <param name="responseBody">The raw response body, when available.</param>
    /// <param name="innerException">The exception that caused this failure, when available.</param>
    public ApiException(
        string message,
        HttpStatusCode statusCode,
        string? responseBody = null,
        Exception? innerException = null)
        : this(
            message,
            statusCode,
            responseBody,
            innerException,
            errorCode: null,
            requestId: null,
            isRetryable: false,
            retryAfter: null,
            validationErrors: null)
    {
    }

    internal ApiException(
        string message,
        HttpStatusCode statusCode,
        string? responseBody,
        Exception? innerException,
        string? errorCode = null,
        string? requestId = null,
        bool isRetryable = false,
        TimeSpan? retryAfter = null,
        IReadOnlyDictionary<string, IReadOnlyList<string>>? validationErrors = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
        ErrorCode = errorCode;
        RequestId = requestId;
        IsRetryable = isRetryable;
        RetryAfter = retryAfter;
        ValidationErrors = validationErrors;
    }
}
