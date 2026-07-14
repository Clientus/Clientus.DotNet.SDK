namespace Clientus.ApiClient.Quotes;

/// <summary>Contains a quote and its position-ordered line items.</summary>
public sealed class QuoteWithItems
{
    /// <summary>Initializes a quote-with-items result and takes a read-only snapshot of the items.</summary>
    /// <param name="quote">The visible quote.</param>
    /// <param name="items">The position-ordered items to snapshot.</param>
    /// <exception cref="ArgumentNullException">Thrown when either argument is null.</exception>
    public QuoteWithItems(Quote quote, IReadOnlyList<QuoteItem> items)
    {
        ArgumentNullException.ThrowIfNull(quote);
        ArgumentNullException.ThrowIfNull(items);
        Quote = quote;
        Items = items.ToArray();
    }

    /// <summary>Gets the quote.</summary>
    public Quote Quote { get; }
    /// <summary>Gets the quote items ordered by position ascending.</summary>
    public IReadOnlyList<QuoteItem> Items { get; }
}
