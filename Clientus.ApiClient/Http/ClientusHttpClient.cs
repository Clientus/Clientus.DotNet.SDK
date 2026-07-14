using System.Net.Http.Json;
using System.Text;
using Clientus.ApiClient.Configuration;
using Clientus.ApiClient.Common;
using Clientus.ApiClient.Serialization;
using System.Net;


namespace Clientus.ApiClient.Http;

public class ClientusHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ClientusConfiguration _configuration;

    public ClientusHttpClient(ClientusConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration.BaseUrl))
            throw new ArgumentException("BaseUrl non configurato.");

        if (string.IsNullOrWhiteSpace(configuration.ApiKey))
            throw new ArgumentException("ApiKey non configurata.");

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