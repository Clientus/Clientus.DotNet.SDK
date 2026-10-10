using System.Text.Json;

namespace Clientus.ApiClient.SdkNext;

/// <summary>
/// Represents one SDK.NEXT beta.5 public-read object whose fields are intentionally
/// open in the Public API v1 contract.
/// </summary>
public sealed class SdkNextRecord : Dictionary<string, JsonElement>
{
}
