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
public class ClientusHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ClientusConfiguration _configuration;

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

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(configuration.BaseUrl),
            Timeout = configuration.Timeout
        };

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
    public async Task<T?> GetAsync<T>(
    string endpoint,
    CancellationToken cancellationToken = default)
    {
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
    public async Task<T?> PostAsync<T>(
    string endpoint,
    object body,
    CancellationToken cancellationToken = default)
    {
        var json = JsonHelper.Serialize(body); ;

        using var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        using var response =
    await SendWithRetryAsync(
        () => _httpClient.PostAsync(
            endpoint,
            content,
            cancellationToken),
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
            _configuration.RetryBaseDelay.TotalMilliseconds *
            attemptNumber);
    }

    /// <summary>
    /// Sets or clears the bearer access token used for authenticated requests.
    /// </summary>
    /// <param name="accessToken">
    /// The access token, or <c>null</c> to clear the current authorization header.
    /// </param>
    public void SetAccessToken(string? accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;

        if (string.IsNullOrWhiteSpace(accessToken))
            return;

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                accessToken);
    }
}