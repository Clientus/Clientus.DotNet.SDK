namespace Clientus.ApiClient.Pagination;

/// <summary>
/// Represents one page returned by the future Clientus Public API.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
/// <param name="Items">The items in this page.</param>
/// <param name="ContinuationToken">An opaque token for the next page, or <see langword="null"/>.</param>
/// <param name="TotalCount">The total number of matching items when supplied by the server.</param>
public sealed record Page<T>(
    IReadOnlyList<T> Items,
    string? ContinuationToken = null,
    long? TotalCount = null)
{
    /// <summary>Gets a value indicating whether another page is available.</summary>
    public bool HasMore => !string.IsNullOrWhiteSpace(ContinuationToken);
}
