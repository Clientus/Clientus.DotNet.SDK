using Clientus.ApiClient.Http;

namespace Clientus.ApiClient.Invoices;

/// <summary>Provides authenticated, RLS-controlled invoice read and delete operations.</summary>
/// <remarks>
/// Creation, editing, status changes, conversions, payment recording, installment/deposit generation,
/// QR generation, public-token access, email, and document workflows are intentionally unavailable:
/// their verified contracts require server orchestration or server-side effects that direct PostgREST
/// requests cannot faithfully reproduce.
/// </remarks>
public class InvoicesService
{
    private const string InvoiceFields =
        "id,company_id,client_id,quote_id,created_by,number,title,notes,status,currency,subtotal," +
        "tax_rate,tax_amount,total,issued_at,due_at,sent_at,paid_at,client_snapshot,public_token," +
        "kind,parent_quote_id,deposit_percentage,deposit_of_amount,qr_reference,paid_amount," +
        "remaining_amount,discount_kind,discount_value,discount_scope,discount_amount," +
        "installment_plan_id,installment_number,installment_total,parent_invoice_id," +
        "installment_cadence,installment_state,is_installment_source,payment_method,payment_link_url," +
        "stripe_checkout_session_id,stripe_payment_intent_id,payment_amount,payment_currency," +
        "payment_terms_days,payment_account_id,payment_account_snapshot,pdf_storage_path," +
        "reminders_sent,work_report_id,subscription_id,subscription_cycle_id,created_at,updated_at";

    private const string ItemFields =
        "id,invoice_id,catalog_item_id,description,quantity,unit,unit_price,line_total,position," +
        "line_kind,price_tax_mode_snapshot,vat_rate_snapshot,discount_snapshot,unit_price_input," +
        "unit_price_net,unit_price_gross,net_amount,vat_amount,gross_amount,created_at";

    private readonly ClientusHttpClient _http;

    /// <summary>Initializes the invoice service with an authenticated transport.</summary>
    public InvoicesService(ClientusHttpClient http)
    {
        ArgumentNullException.ThrowIfNull(http);
        _http = http;
    }

    /// <summary>Gets one invoice visible to the authenticated caller through RLS.</summary>
    public async Task<Invoice?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        var rows = await _http.GetAsync<List<Invoice>>(
            $"/rest/v1/invoices?select={InvoiceFields}&{ExactId(id)}&limit=1", cancellationToken);
        return rows?.FirstOrDefault();
    }

    /// <summary>Gets one RLS-visible invoice and its RLS-visible items ordered by position.</summary>
    public async Task<InvoiceWithItems?> GetWithItemsAsync(string id, CancellationToken cancellationToken = default)
    {
        var invoice = await GetAsync(id, cancellationToken);
        if (invoice is null) return null;

        var items = await _http.GetAsync<List<InvoiceItem>>(
            $"/rest/v1/invoice_items?select={ItemFields}&{PostgRestQuery.ExactFilter("invoice_id", id, nameof(id))}&order=position.asc",
            cancellationToken);
        return new InvoiceWithItems(invoice, items ?? []);
    }

    /// <summary>Lists all invoices visible through RLS, newest creation first.</summary>
    public async Task<IReadOnlyList<Invoice>> ListAsync(CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        var rows = await _http.GetAsync<List<Invoice>>(
            $"/rest/v1/invoices?select={InvoiceFields}&order=created_at.desc", cancellationToken);
        return PostgRestQuery.OrEmpty(rows);
    }

    /// <summary>Determines whether an exact invoice identifier is visible through RLS.</summary>
    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        var rows = await _http.GetAsync<List<InvoiceIdentity>>(
            $"/rest/v1/invoices?select=id&{ExactId(id)}&limit=1", cancellationToken);
        return rows?.Count > 0;
    }

    /// <summary>Gets the exact RLS-visible invoice count using a retryable HEAD request.</summary>
    public Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        return _http.HeadCountAsync("/rest/v1/invoices?select=id", cancellationToken);
    }

    /// <summary>
    /// Deletes an exact RLS-visible invoice. Items and deposit links cascade. References configured
    /// with SET NULL are detached. Company-document rows and storage objects do not cascade and are
    /// not deleted by this operation. Transient DELETE failures may be retried.
    /// </summary>
    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        return _http.DeleteAsync($"/rest/v1/invoices?{ExactId(id)}", cancellationToken);
    }

    internal void ThrowIfDisposed() => _http.ThrowIfDisposed();

    private static string ExactId(string id) => PostgRestQuery.ExactFilter("id", id, nameof(id));

    private sealed class InvoiceIdentity { public string Id { get; set; } = string.Empty; }
}
