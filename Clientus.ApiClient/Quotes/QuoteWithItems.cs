namespace Clientus.ApiClient.Quotes;

/// <summary>Contains a quote and its position-ordered line items.</summary>
public sealed class QuoteWithItems
{
    /// <summary>Initializes a quote-with-items result.</summary>
    public QuoteWithItems(Quote quote, IReadOnlyList<QuoteItem> items)
    {
        ArgumentNullException.ThrowIfNull(quote);
        ArgumentNullException.ThrowIfNull(items);
        Quote = quote;
        Items = items;
    }

    /// <summary>Gets the quote.</summary>
    public Quote Quote { get; }
    /// <summary>Gets the quote items ordered by position ascending.</summary>
    public IReadOnlyList<QuoteItem> Items { get; }
}
