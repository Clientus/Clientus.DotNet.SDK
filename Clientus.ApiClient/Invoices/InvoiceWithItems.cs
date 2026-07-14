namespace Clientus.ApiClient.Invoices;

/// <summary>Contains an invoice and an immutable snapshot of its position-ordered items.</summary>
public sealed class InvoiceWithItems
{
    /// <summary>Initializes a result and takes a read-only snapshot of the items.</summary>
    /// <param name="invoice">The visible invoice.</param>
    /// <param name="items">The position-ordered items.</param>
    public InvoiceWithItems(Invoice invoice, IReadOnlyList<InvoiceItem> items)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        ArgumentNullException.ThrowIfNull(items);
        Invoice = invoice;
        Items = items.ToArray();
    }

    /// <summary>Gets the invoice.</summary>
    public Invoice Invoice { get; }
    /// <summary>Gets the invoice items ordered by position ascending.</summary>
    public IReadOnlyList<InvoiceItem> Items { get; }
}
