using Clientus.ApiClient;
using Clientus.ApiClient.Configuration;

var developerCredential =
    Environment.GetEnvironmentVariable("CLIENTUS_DEVELOPER_API_KEY")
    ?? throw new InvalidOperationException(
        "Set CLIENTUS_DEVELOPER_API_KEY before running this sample.");

var baseUrl =
    Environment.GetEnvironmentVariable("CLIENTUS_API_BASE_URL")
    ?? "https://clientus.app";

var configuration = new ClientusConfiguration
{
    BaseUrl = baseUrl,
    DeveloperCredential = developerCredential,
};

using var client = new ClientusClient(configuration);

Console.WriteLine("Clientus Developer SDK configured.");
Console.WriteLine("Use client.Customers, client.Catalog, client.Quotes, client.Invoices or client.Ai.");