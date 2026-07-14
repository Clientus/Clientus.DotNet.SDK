using Clientus.ApiClient.Http;

namespace Clientus.ApiClient.Customers;

public class CustomersService
{
    private readonly ClientusHttpClient _http;

    public CustomersService(ClientusHttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync(
     int? limit = null,
     CancellationToken cancellationToken = default)
    {
        if (limit is <= 0)
            throw new ArgumentOutOfRangeException(nameof(limit));

        var endpoint =
    "/rest/v1/clients" +
    "?select=id,company_id,first_name,last_name,display_name,email,phone,address,postal_code,city,country,client_type,created_at" +
    "&order=created_at.desc";

        if (limit.HasValue)
        {
            endpoint += $"&limit={limit.Value}";
        }

        var customers =
            await _http.GetAsync<List<Customer>>(
                endpoint,
                cancellationToken);

        return customers ?? new List<Customer>();
    }

    public async Task<Customer?> GetByIdAsync(
    string id,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException(
                "L'ID cliente è obbligatorio.",
                nameof(id));

        var endpoint =
            "/rest/v1/clients" +
            "?select=*" +
            $"&id=eq.{Uri.EscapeDataString(id)}" +
            "&limit=1";

        var result =
            await _http.GetAsync<List<Customer>>(
                endpoint,
                cancellationToken);

        return result?.FirstOrDefault();
    }

    public async Task<IReadOnlyList<Customer>> SearchAsync(
    string text,
    int limit = 20,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<Customer>();

        if (limit <= 0)
            throw new ArgumentOutOfRangeException(nameof(limit));

        var search =
            Uri.EscapeDataString($"*{text.Trim()}*");

        var endpoint =
            "/rest/v1/clients" +
            "?select=id,company_id,first_name,last_name,display_name,email,phone,address,postal_code,city,country,client_type,created_at" +
            $"&or=(first_name.ilike.{search},last_name.ilike.{search},display_name.ilike.{search},email.ilike.{search},phone.ilike.{search})" +
            "&order=display_name.asc.nullslast,first_name.asc" +
            $"&limit={limit}";

        var customers =
            await _http.GetAsync<List<Customer>>(
                endpoint,
                cancellationToken);

        return customers ?? new List<Customer>();
    }
}