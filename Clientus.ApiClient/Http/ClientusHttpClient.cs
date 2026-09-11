using System.Net.Http.Json;
using System.Text;
using Clientus.ApiClient.Configuration;
using Clientus.ApiClient.Common;
using Clientus.ApiClient.Serialization;
using System.Net;
using System.Reflection;
using System.Text.Json;


namespace Clientus.ApiClient.Http;

/// <summary>
/// Provides low-level HTTP communication, JSON serialization, cancellation, and the SDK retry policy.
/// </summary>
/// <remarks>
/// GET, HEAD, and DELETE retry configured transient status codes. POST and PATCH are never retried.
/// This type owns its underlying HTTP resources and must be disposed.
/// </remarks>
public class ClientusHttpClient : IDisposable, IClientusApiTransport
{
    private readonly HttpClient _httpClient;
    private readonly int _maxRetryAttempts;
    private readonly TimeSpan _initialRetryDelay;
    private readonly TimeSpan _maximumRetryDelay;
    private int _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientusHttpClient"/> class.
    /// </summary>
    /// <param name="configuration">
    /// The Clientus API configuration.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configuration"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the base URL is missing, invalid, or not HTTP/HTTPS, or when the API key is missing.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when timeout or retry settings are outside their supported ranges.
    /// </exception>
    /// <remarks>The client owns and disposes its underlying <see cref="HttpClient"/>.</remarks>
    public ClientusHttpClient(ClientusConfiguration configuration)
        : this(configuration, configuration?.HttpMessageHandler)
    {
    }

    internal ClientusHttpClient(
        ClientusConfiguration configuration,
        HttpMessageHandler? handler)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        if (string.IsNullOrWhiteSpace(configuration.BaseUrl))
            throw new ArgumentException(
                "BaseUrl is not configured.",
                nameof(configuration.BaseUrl));

        if (!Uri.TryCreate(configuration.BaseUrl, UriKind.Absolute, out var baseUri) ||
            (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException(
                "BaseUrl must be an absolute HTTP or HTTPS URL.",
                nameof(configuration.BaseUrl));
        }

        if (string.IsNullOrWhiteSpace(configuration.DeveloperCredential))
            throw new ArgumentException(
                "DeveloperCredential is not configured.",
                nameof(configuration.DeveloperCredential));

        if (!Enum.IsDefined(configuration.Environment))
            throw new ArgumentOutOfRangeException(
                nameof(configuration.Environment),
                "Environment must be Sandbox or Live.");

        if (configuration.Timeout != Timeout.InfiniteTimeSpan && configuration.Timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(configuration.Timeout), "Timeout must be positive or infinite.");

        if (configuration.MaxRetryAttempts < 1)
            throw new ArgumentOutOfRangeException(nameof(configuration.MaxRetryAttempts), "At least one request attempt is required.");

        if (configuration.InitialRetryDelay < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(configuration.InitialRetryDelay), "Retry delay cannot be negative.");

        if (configuration.MaximumRetryDelay < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(configuration.MaximumRetryDelay), "Maximum retry delay cannot be negative.");

        _maxRetryAttempts = configuration.MaxRetryAttempts;
        _initialRetryDelay = configuration.InitialRetryDelay;
        _maximumRetryDelay = configuration.MaximumRetryDelay;

        _httpClient = handler is null
            ? new HttpClient()
            : new HttpClient(handler, configuration.DisposeHttpMessageHandler);

        _httpClient.BaseAddress = baseUri;
        _httpClient.Timeout = configuration.Timeout;

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                configuration.DeveloperCredential.Trim());
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(GetUserAgent());
    }




    private async Task<HttpResponseMessage> SendWithRetryAsync(
    Func<Task<HttpResponseMessage>> sendRequest,
    CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            var response = await sendRequest();

            if (response.IsSuccessStatusCode)
                return response;

            if (attempt >= _maxRetryAttempts ||
                !IsTransientStatusCode(response.StatusCode))
            {
                return response;
            }

            var retryDelay = GetRetryDelay(response, attempt);
            response.Dispose();
            await Task.Delay(retryDelay, cancellationToken);
        }
    }

    /// <summary>
    /// Sends an HTTP GET request and deserializes the response.
    /// </summary>
    /// <typeparam name="T">
    /// The response type.
    /// </typeparam>
    /// <param name="endpoint">
    /// The relative API endpoint.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the request.
    /// </param>
    /// <returns>
    /// The deserialized response, or <c>null</c> when the response contains no value.
    /// </returns>
    /// <exception cref="ApiException">
    /// Thrown when the API returns an unsuccessful HTTP status code.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when this instance has been disposed.
    /// </exception>
    public async Task<T?> GetAsync<T>(
    string endpoint,
    CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ValidateEndpoint(endpoint);

        using var response =
    await SendWithRetryAsync(
        () => _httpClient.GetAsync(endpoint, cancellationToken),
        cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateApiExceptionAsync(response, cancellationToken);
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return PublicApiJson.Deserialize<T>(responseBody);
    }

    /// <summary>
    /// Sends an HTTP POST request and deserializes the response.
    /// </summary>
    /// <typeparam name="T">
    /// The response type.
    /// </typeparam>
    /// <param name="endpoint">
    /// The relative API endpoint.
    /// </param>
    /// <param name="body">
    /// The object to serialize as the request body.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the request.
    /// </param>
    /// <returns>
    /// The deserialized response, or <c>null</c> when the response contains no value.
    /// </returns>
    /// <exception cref="ApiException">
    /// Thrown when the API returns an unsuccessful HTTP status code.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when this instance has been disposed.
    /// </exception>
    public async Task<T?> PostAsync<T>(
    string endpoint,
    object body,
    CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ValidateEndpoint(endpoint);
        ArgumentNullException.ThrowIfNull(body);

        var json = JsonHelper.Serialize(body);

        using var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.PostAsync(
            endpoint,
            content,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateApiExceptionAsync(response, cancellationToken);
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return PublicApiJson.Deserialize<T>(responseBody);
    }

    /// <summary>
    /// Sends an HTTP PATCH request and deserializes the response without retrying the request.
    /// </summary>
    /// <typeparam name="T">The response type.</typeparam>
    /// <param name="endpoint">The relative API endpoint.</param>
    /// <param name="body">The object to serialize as the request body.</param>
    /// <param name="cancellationToken">A token used to cancel the request.</param>
    /// <returns>The deserialized response, or <see langword="null"/> when the response contains no value.</returns>
    /// <exception cref="ApiException">
    /// Thrown when the API returns an unsuccessful HTTP status code.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when this instance has been disposed.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when cancellation is requested through <paramref name="cancellationToken"/>.
    /// </exception>
    public async Task<T?> PatchAsync<T>(
        string endpoint,
        object body,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ValidateEndpoint(endpoint);
        ArgumentNullException.ThrowIfNull(body);

        var json = JsonHelper.Serialize(body);

        using var request = new HttpRequestMessage(HttpMethod.Patch, endpoint)
        {
            Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json")
        };
        request.Headers.Add("Prefer", "return=representation");

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateApiExceptionAsync(response, cancellationToken);
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return PublicApiJson.Deserialize<T>(responseBody);
    }

    /// <summary>
    /// Sends an HTTP DELETE request.
    /// </summary>
    /// <param name="endpoint">The relative API endpoint.</param>
    /// <param name="cancellationToken">A token used to cancel the request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ApiException">
    /// Thrown when the API returns an unsuccessful HTTP status code.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when this instance has been disposed.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when cancellation is requested through <paramref name="cancellationToken"/>.
    /// </exception>
    public async Task DeleteAsync(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ValidateEndpoint(endpoint);

        using var response = await SendWithRetryAsync(
            () => _httpClient.DeleteAsync(endpoint, cancellationToken),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateApiExceptionAsync(response, cancellationToken);
        }
    }

    /// <summary>
    /// Sends an HTTP HEAD request and returns the exact PostgREST resource count.
    /// </summary>
    /// <param name="endpoint">The relative API endpoint.</param>
    /// <param name="cancellationToken">A token used to cancel the request.</param>
    /// <returns>The exact resource count reported by PostgREST.</returns>
    /// <exception cref="ApiException">
    /// Thrown when the API returns an unsuccessful HTTP status code.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a successful response does not contain a total count.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when this instance has been disposed.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when cancellation is requested through <paramref name="cancellationToken"/>.
    /// </exception>
    public async Task<long> HeadCountAsync(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ValidateEndpoint(endpoint);

        using var response = await SendWithRetryAsync(
            async () =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Head, endpoint);
                request.Headers.Add("Prefer", "count=exact");
                request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(0, 0);

                return await _httpClient.SendAsync(request, cancellationToken);
            },
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateApiExceptionAsync(response, cancellationToken);
        }

        return response.Content.Headers.ContentRange?.Length
            ?? throw new InvalidOperationException(
                "The PostgREST response did not include an exact resource count.");
    }

    private static bool IsTransientStatusCode(HttpStatusCode statusCode)
    {
        return statusCode is
            HttpStatusCode.RequestTimeout or
            HttpStatusCode.TooManyRequests or
            HttpStatusCode.BadGateway or
            HttpStatusCode.ServiceUnavailable or
            HttpStatusCode.GatewayTimeout;
    }

    private TimeSpan GetRetryDelay(HttpResponseMessage response, int attemptNumber)
    {
        var retryAfter = GetRetryAfter(response);
        if (retryAfter is not null)
        {
            return retryAfter.Value > _maximumRetryDelay
                ? _maximumRetryDelay
                : retryAfter.Value;
        }

        var milliseconds = Math.Min(
            int.MaxValue,
            _initialRetryDelay.TotalMilliseconds * attemptNumber);
        var delay = TimeSpan.FromMilliseconds(milliseconds);
        return delay > _maximumRetryDelay ? _maximumRetryDelay : delay;
    }

    private static TimeSpan? GetRetryAfter(HttpResponseMessage response)
    {
        var retryAfter = response.Headers.RetryAfter;
        if (retryAfter?.Delta is { } delta)
        {
            return delta < TimeSpan.Zero ? TimeSpan.Zero : delta;
        }

        if (retryAfter?.Date is { } date)
        {
            var delay = date - DateTimeOffset.UtcNow;
            return delay < TimeSpan.Zero ? TimeSpan.Zero : delay;
        }

        return null;
    }

    private static async Task<ApiException> CreateApiExceptionAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        string? code = null;
        string? serverMessage = null;
        IReadOnlyDictionary<string, IReadOnlyList<string>>? validationErrors = null;

        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                using var document = JsonDocument.Parse(body);
                var root = document.RootElement;
                if (root.ValueKind == JsonValueKind.Object)
                {
                    code = ReadString(root, "code");
                    serverMessage = ReadString(root, "message");
                    validationErrors = ReadValidationErrors(root);
                }
            }
            catch (JsonException)
            {
                // Preserve non-JSON legacy response bodies without guessing their structure.
            }
        }

        var requestId = GetHeader(response, "x-request-id")
            ?? GetHeader(response, "x-correlation-id");
        var message = string.IsNullOrWhiteSpace(serverMessage)
            ? $"The API request failed with status {(int)response.StatusCode} {response.ReasonPhrase}."
            : serverMessage;

        return new ApiException(
            message,
            response.StatusCode,
            body,
            innerException: null,
            errorCode: code,
            requestId: requestId,
            isRetryable: IsTransientStatusCode(response.StatusCode),
            retryAfter: GetRetryAfter(response),
            validationErrors: validationErrors);
    }

    private static string? ReadString(JsonElement root, string propertyName) =>
        root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static IReadOnlyDictionary<string, IReadOnlyList<string>>? ReadValidationErrors(JsonElement root)
    {
        if (!root.TryGetProperty("errors", out var errors) || errors.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var result = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        foreach (var property in errors.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Array)
            {
                result[property.Name] = property.Value.EnumerateArray()
                    .Where(value => value.ValueKind == JsonValueKind.String)
                    .Select(value => value.GetString()!)
                    .ToArray();
            }
            else if (property.Value.ValueKind == JsonValueKind.String)
            {
                result[property.Name] = new[] { property.Value.GetString()! };
            }
        }

        return result.Count == 0 ? null : result;
    }

    private static string? GetHeader(HttpResponseMessage response, string name) =>
        response.Headers.TryGetValues(name, out var values) ? values.FirstOrDefault() : null;

    private static string GetUserAgent()
    {
        var version = typeof(ClientusHttpClient).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion.Split('+')[0] ?? "unknown";
        return $"Clientus-DotNet-SDK/{version}";
    }

    /// <summary>
    /// Sets or clears the bearer access token used for authenticated requests.
    /// </summary>
    /// <param name="accessToken">
    /// The access token, or <c>null</c> to clear the current authorization header.
    /// </param>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when this instance has been disposed.
    /// </exception>
    [Obsolete("Human access-token mutation is not supported by the Public API v1 SDK.")]
    public void SetAccessToken(string? accessToken)
    {
        ThrowIfDisposed();
        throw new NotSupportedException(
            "Clientus Public API v1 authenticates with the configured developer credential.");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
        {
            _httpClient.Dispose();
        }
    }

    internal void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(
            Volatile.Read(ref _disposed) != 0,
            this);
    }

    void IClientusApiTransport.ThrowIfDisposed() => ThrowIfDisposed();
    private static void ValidateEndpoint(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new ArgumentException("An API endpoint is required.", nameof(endpoint));
        }

        if (!endpoint.StartsWith("/api/v1/", StringComparison.Ordinal) &&
            !string.Equals(endpoint, "/api/v1", StringComparison.Ordinal))
        {
            throw new NotSupportedException(
                "Clientus SDK Public API mode only permits versioned /api/v1 endpoints.");
        }
    }
}
