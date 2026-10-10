# Clientus .NET SDK

**Build integrations, automations and AI-powered applications on the Clientus Developer Platform.**

`Clientus.ApiClient` is the official .NET 8 SDK for **Clientus Public API v1**.

## Install

```bash
dotnet add package Clientus.ApiClient --prerelease
```

Current beta line: **1.0.0-beta.5**

## Beta.5 read surfaces

Beta.5 adds read-only access to:

- Business Units
- Warehouse stock
- Loyalty
- Customer Intelligence
- Retail Returns
- Retail Distribution status
- Advanced Retail Intelligence
- Publishing
- Website
- Public Commerce

The new beta.5 surfaces are intentionally read-only. Server-side scopes, entitlements, tenant authority, quotas and rate limits remain authoritative.

Where Public API v1 intentionally defines an open object schema, the SDK exposes `SdkNextRecord` instead of inventing a closed DTO.

## Quick start

```csharp
using Clientus.ApiClient;
using Clientus.ApiClient.Configuration;

using var client = new ClientusClient(new ClientusConfiguration
{
    BaseUrl = "https://clientus.app",
    DeveloperCredential =
        Environment.GetEnvironmentVariable("CLIENTUS_DEVELOPER_CREDENTIAL")!,
    Environment = ClientusEnvironment.Sandbox
});
```

Developer credentials are app- and environment-specific. Keep them server-side and never embed them in browser or mobile source.

## Security

The SDK uses versioned Public API v1 endpoints with scoped developer credentials. It does not make direct database, service-role or unversioned PostgREST access part of the third-party contract.

## Links

- Developer Portal: https://clientus.app/developer
- Clientus: https://clientus.app
- Source: https://github.com/Clientus/Clientus.DotNet.SDK
