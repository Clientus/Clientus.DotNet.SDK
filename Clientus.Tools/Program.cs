using Clientus.ApiClient;
using Clientus.ApiClient.Configuration;

var credential =
    Environment.GetEnvironmentVariable("CLIENTUS_DEVELOPER_API_KEY")
    ?? throw new InvalidOperationException(
        "Set CLIENTUS_DEVELOPER_API_KEY before running Clientus.Tools.");

var baseUrl =
    Environment.GetEnvironmentVariable("CLIENTUS_API_BASE_URL")
    ?? "https://clientus.app";

var configuration = new ClientusConfiguration
{
    BaseUrl = baseUrl,
    DeveloperCredential = credential,
};

using var client = new ClientusClient(configuration);

Console.WriteLine("Clientus.Tools");
Console.WriteLine("Public API v1 developer credential configured.");
Console.WriteLine("Available SDK surfaces: Customers, Catalog, Quotes, Invoices, Users, Ai.");
Console.WriteLine("Human email/password login is intentionally not supported.");