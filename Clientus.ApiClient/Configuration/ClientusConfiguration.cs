namespace Clientus.ApiClient.Configuration;

/// <summary>
/// Represents the configuration used to connect to the Clientus API.
/// </summary>
public class ClientusConfiguration
{
    /// <summary>
    /// URL del backend Clientus
    /// Es: https://xxxx.supabase.co
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Chiave pubblica (anon key)
    /// Mai usare la Service Role Key nel client.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Timeout delle richieste HTTP.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Numero massimo di tentativi per errori temporanei.
    /// Il valore include il primo tentativo.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Attesa iniziale tra un tentativo e il successivo.
    /// L'attesa aumenta progressivamente.
    /// </summary>
    public TimeSpan RetryBaseDelay { get; set; } =
        TimeSpan.FromMilliseconds(500);
}