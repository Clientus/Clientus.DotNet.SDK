namespace Clientus.ApiClient.Pagination;

/// <summary>
/// Describes a future Public API page request without prescribing a backend query format.
/// </summary>
/// <param name="PageSize">The maximum number of items requested.</param>
/// <param name="ContinuationToken">The opaque continuation token returned by a previous page.</param>
public sealed record PageRequest(int PageSize = 50, string? ContinuationToken = null)
{
    /// <summary>Gets the largest supported page size.</summary>
    public const int MaximumPageSize = 100;

    /// <summary>Validates this request.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the page size is outside 1 through 100.</exception>
    public void Validate()
    {
        if (PageSize is < 1 or > MaximumPageSize)
        {
            throw new ArgumentOutOfRangeException(nameof(PageSize), $"Page size must be between 1 and {MaximumPageSize}.");
        }
    }
}
