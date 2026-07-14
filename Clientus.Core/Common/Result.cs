namespace Clientus.Core.Common;

/// <summary>
/// Represents the result of an operation.
/// </summary>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// Gets the error message when the operation failed.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="success">
    /// A value indicating whether the operation succeeded.
    /// </param>
    /// <param name="error">
    /// The error message, or <c>null</c> for successful operations.
    /// </param>
    protected Result(bool success, string? error)
    {
        Success = success;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful result.</returns>
    public static Result Ok()
    {
        return new Result(true, null);
    }

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="error">
    /// The error message describing the failure.
    /// </param>
    /// <returns>A failed result.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="error"/> is <see langword="null"/>, empty, or consists only of white-space characters.
    /// </exception>
    public static Result Fail(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            throw new ArgumentException("The error message cannot be null, empty, or whitespace.", nameof(error));
        }

        return new Result(false, error);
    }
}
