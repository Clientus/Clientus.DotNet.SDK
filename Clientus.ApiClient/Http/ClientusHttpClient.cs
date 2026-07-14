using System.Net.Http.Json;
using System.Text;
using Clientus.ApiClient.Configuration;
using Clientus.ApiClient.Common;
using Clientus.ApiClient.Serialization;
using System.Net;


namespace Clientus.ApiClient.Http;

/// <summary>
/// Provides HTTP communication with the Clientus API.
/// </summary>
public class ClientusHttpClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ClientusConfiguration _configuration;
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
    /// Thrown when the base URL or API key is missing.
    /// </exception>
    public ClientusHttpClient(ClientusConfiguration configuration)
        : this(configuration, null)
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

        if (string.IsNullOrWhiteSpace(configuration.ApiKey))
            throw new ArgumentException(
    "ApiKey is not configured.",
    nameof(configuration.ApiKey));

        _configuration = configuration;

        _httpClient = handler is null
            ? new HttpClient()
            : new HttpClient(handler);

        _httpClient.BaseAddress = new Uri(configuration.BaseUrl);
        _httpClient.Timeout = configuration.Timeout;

        _httpClient.DefaultRequestHeaders.Add("apikey", configuration.ApiKey);
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

            if (attempt >= _configuration.MaxRetryAttempts ||
                !IsTransientStatusCode(response.StatusCode))
            {
                return response;
            }

            response.Dispose();

            await Task.Delay(
                GetRetryDelay(attempt),
                cancellationToken);
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

        using var response =
    await SendWithRetryAsync(
        () => _httpClient.GetAsync(endpoint, cancellationToken),
        cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody =
                await response.Content.ReadAsStringAsync(cancellationToken);

            throw new ApiException(
                $"La richiesta API è fallita con stato {(int)response.StatusCode} {response.ReasonPhrase}.",
                response.StatusCode,
                responseBody);
        }

        return await response.Content.ReadFromJsonAsync<T>(
     JsonHelper.SerializerOptions,
     cancellationToken);
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

        var json = JsonHelper.Serialize(body); ;

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
            var responseBody =
                await response.Content.ReadAsStringAsync(cancellationToken);

            throw new ApiException(
                $"La richiesta API è fallita con stato {(int)response.StatusCode} {response.ReasonPhrase}.",
                response.StatusCode,
                responseBody);
        }

        return await response.Content.ReadFromJsonAsync<T>(
    JsonHelper.SerializerOptions,
    cancellationToken);
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
            var responseBody =
                await response.Content.ReadAsStringAsync(cancellationToken);

            throw new ApiException(
                $"The API request failed with status {(int)response.StatusCode} {response.ReasonPhrase}.",
                response.StatusCode,
                responseBody);
        }

        return await response.Content.ReadFromJsonAsync<T>(
            JsonHelper.SerializerOptions,
            cancellationToken);
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

        using var response = await SendWithRetryAsync(
            () => _httpClient.DeleteAsync(endpoint, cancellationToken),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody =
                await response.Content.ReadAsStringAsync(cancellationToken);

            throw new ApiException(
                $"The API request failed with status {(int)response.StatusCode} {response.ReasonPhrase}.",
                response.StatusCode,
                responseBody);
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
            var responseBody =
                await response.Content.ReadAsStringAsync(cancellationToken);

            throw new ApiException(
                $"The API request failed with status {(int)response.StatusCode} {response.ReasonPhrase}.",
                response.StatusCode,
                responseBody);
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

    private TimeSpan GetRetryDelay(int attemptNumber)
    {
        return TimeSpan.FromMilliseconds(
            _configuration.InitialRetryDelay.TotalMilliseconds *
            attemptNumber);
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
    public void SetAccessToken(string? accessToken)
    {
        ThrowIfDisposed();

        _httpClient.DefaultRequestHeaders.Authorization = null;

        if (string.IsNullOrWhiteSpace(accessToken))
            return;

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                accessToken);
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
}
