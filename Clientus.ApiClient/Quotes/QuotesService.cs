using Clientus.ApiClient.Http;

namespace Clientus.ApiClient.Quotes;

/// <summary>
/// Provides authenticated quote operations. Quote visibility and mutation authorization remain
/// controlled by Supabase row-level security, including the <c>manage_billing</c> policies.
/// </summary>
/// <remarks>
/// Quote creation and invoice conversion are intentionally not exposed because their verified
/// contracts are multi-step server workflows. Public-token operations, attachment mutations,
/// quote-item editing, and generic quote editing are also intentionally not exposed.
/// </remarks>
public class QuotesService
{
    private const string QuoteFields =
        "id,company_id,client_id,created_by,number,title,notes,status,currency,subtotal,tax_rate," +
        "tax_amount,total,valid_until,issued_at,sent_at,accepted_at,rejected_at,client_snapshot," +
        "public_token,client_response_note,discount_kind,discount_value,discount_scope," +
        "discount_amount,created_at,updated_at";

    private const string QuoteItemFields =
        "id,quote_id,catalog_item_id,description,quantity,unit,unit_price,line_total,position," +
        "line_kind,price_tax_mode_snapshot,vat_rate_snapshot,discount_snapshot,unit_price_input," +
        "unit_price_net,unit_price_gross,net_amount,vat_amount,gross_amount,created_at";

    private readonly ClientusHttpClient _http;

    /// <summary>Initializes the quote service.</summary>
    /// <param name="http">The authenticated HTTP transport.</param>
    public QuotesService(ClientusHttpClient http)
    {
        ArgumentNullException.ThrowIfNull(http);
        _http = http;
    }

    /// <summary>Gets one RLS-visible quote by exact identifier.</summary>
    /// <returns>The visible quote, or <see langword="null"/>.</returns>
    public async Task<Quote?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        ValidateId(id);

        var rows = await _http.GetAsync<List<Quote>>(
            $"/rest/v1/quotes?select={QuoteFields}&id=eq.{Escape(id)}&limit=1",
            cancellationToken);
        return rows?.FirstOrDefault();
    }

    /// <summary>
    /// Gets one RLS-visible quote and loads its RLS-visible items in ascending position order.
    /// </summary>
    /// <returns>The quote and items, or <see langword="null"/> when the quote is not visible.</returns>
    public async Task<QuoteWithItems?> GetWithItemsAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var quote = await GetAsync(id, cancellationToken);
        if (quote is null)
        {
            return null;
        }

        var items = await _http.GetAsync<List<QuoteItem>>(
            $"/rest/v1/quote_items?select={QuoteItemFields}&quote_id=eq.{Escape(id)}&order=position.asc",
            cancellationToken);
        return new QuoteWithItems(quote, items ?? []);
    }

    /// <summary>Lists every quote visible through RLS, newest creation first.</summary>
    public async Task<IReadOnlyList<Quote>> ListAsync(CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        var rows = await _http.GetAsync<List<Quote>>(
            $"/rest/v1/quotes?select={QuoteFields}&order=created_at.desc",
            cancellationToken);
        return rows ?? [];
    }

    /// <summary>Determines whether an exact quote identifier is visible through RLS.</summary>
    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        ValidateId(id);
        var rows = await _http.GetAsync<List<QuoteIdentity>>(
            $"/rest/v1/quotes?select=id&id=eq.{Escape(id)}&limit=1",
            cancellationToken);
        return rows?.Count > 0;
    }

    /// <summary>Gets the exact number of quotes visible through RLS.</summary>
    public Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        return _http.HeadCountAsync("/rest/v1/quotes?select=id", cancellationToken);
    }

    /// <summary>
    /// Applies a verified authenticated quote status transition. Authorization remains enforced
    /// by RLS and the quote <c>manage_billing</c> update policy.
    /// </summary>
    /// <remarks>
    /// Allowed transitions are draft to sent/accepted/rejected/expired; sent to
    /// accepted/rejected/expired/draft; and expired to sent. Accepted and rejected are terminal.
    /// A same-state request performs no PATCH.
    /// </remarks>
    public async Task<Quote> UpdateStatusAsync(
        string id,
        QuoteStatus targetStatus,
        CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        ValidateId(id);
        if (!Enum.IsDefined(targetStatus))
        {
            throw new ArgumentOutOfRangeException(nameof(targetStatus));
        }

        var current = await GetAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("The quote was not found or is not visible.");

        if (current.Status == targetStatus)
        {
            return current;
        }

        if (!IsAllowed(current.Status, targetStatus))
        {
            throw new InvalidOperationException(
                $"Quote status transition {current.Status} -> {targetStatus} is not allowed.");
        }

        var now = DateTimeOffset.UtcNow;
        object patch = targetStatus switch
        {
            QuoteStatus.Sent when current.SentAt is null => new { status = targetStatus, sent_at = now },
            QuoteStatus.Accepted => new { status = targetStatus, accepted_at = now },
            QuoteStatus.Rejected => new { status = targetStatus, rejected_at = now },
            _ => new { status = targetStatus }
        };

        var rows = await _http.PatchAsync<List<Quote>>(
            $"/rest/v1/quotes?select={QuoteFields}&id=eq.{Escape(id)}&company_id=eq.{Escape(current.CompanyId)}",
            patch,
            cancellationToken);
        return rows?.SingleOrDefault()
            ?? throw new InvalidOperationException("The API did not return the updated quote.");
    }

    /// <summary>
    /// Deletes an exact RLS-visible quote. Quote items cascade and invoice quote references become
    /// null. Quote-linked <c>company_documents</c> rows and storage objects do not cascade and are
    /// neither deleted nor claimed to be deleted by this operation.
    /// </summary>
    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        ValidateId(id);
        return _http.DeleteAsync($"/rest/v1/quotes?id=eq.{Escape(id)}", cancellationToken);
    }

    internal void ThrowIfDisposed() => _http.ThrowIfDisposed();

    private static bool IsAllowed(QuoteStatus current, QuoteStatus target) => current switch
    {
        QuoteStatus.Draft => target is QuoteStatus.Sent or QuoteStatus.Accepted or QuoteStatus.Rejected or QuoteStatus.Expired,
        QuoteStatus.Sent => target is QuoteStatus.Accepted or QuoteStatus.Rejected or QuoteStatus.Expired or QuoteStatus.Draft,
        QuoteStatus.Expired => target is QuoteStatus.Sent,
        QuoteStatus.Accepted or QuoteStatus.Rejected => false,
        _ => false
    };

    private static string Escape(string value) => Uri.EscapeDataString(value);

    private static void ValidateId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Quote Id is required.", nameof(id));
        }
    }

    private sealed class QuoteIdentity
    {
        public string Id { get; set; } = string.Empty;
    }
}
