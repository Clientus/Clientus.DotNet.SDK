using System.Reflection;
using Clientus.ApiClient.Ai;
using Xunit;

namespace Clientus.ApiClient.Tests;

public sealed class AiSurfaceTests
{
    [Fact]
    public void Ai_service_is_public()
    {
        Assert.True(typeof(AiService).IsPublic);
    }

    [Fact]
    public void Ai_request_exposes_prompt_and_capability()
    {
        var request = new AiRequest("hello", "chat");

        Assert.Equal("hello", request.Prompt);
        Assert.Equal("chat", request.Capability);
    }

    [Fact]
    public void ClientusClient_exposes_Ai_service()
    {
        var property = typeof(ClientusClient).GetProperty(
            "Ai",
            BindingFlags.Instance | BindingFlags.Public);

        Assert.NotNull(property);
        Assert.Equal(typeof(AiService), property!.PropertyType);
    }

    [Fact]
    public void Ai_service_does_not_expose_provider_selection()
    {
        var publicMembers = typeof(AiService)
            .GetMembers(BindingFlags.Instance | BindingFlags.Public)
            .Select(member => member.Name)
            .ToArray();

        Assert.DoesNotContain(
            publicMembers,
            name => name.Contains("Provider", StringComparison.OrdinalIgnoreCase));

        Assert.DoesNotContain(
            publicMembers,
            name => name.Contains("Model", StringComparison.OrdinalIgnoreCase));
    }
}