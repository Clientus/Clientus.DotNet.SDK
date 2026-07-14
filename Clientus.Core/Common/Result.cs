namespace Clientus.Core.Common;

public class Result
{
    public bool Success { get; }

    public string? Error { get; }

    protected Result(bool success, string? error)
    {
        Success = success;
        Error = error;
    }

    public static Result Ok()
    {
        return new Result(true, null);
    }

    public static Result Fail(string error)
    {
        return new Result(false, error);
    }
}