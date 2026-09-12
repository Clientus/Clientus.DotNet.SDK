namespace Clientus.ApiClient.Ai;

public sealed record AiRequest(
    string Prompt,
    string? Capability = null);

public sealed record AiError(
    string Code,
    string? Message = null);

public sealed record AiResponse<T>(
    T? Data,
    AiError? Error,
    string? RequestId);