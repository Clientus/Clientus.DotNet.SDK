# Clientus .NET SDK

**Build integrations, automations and AI-powered applications on the Clientus Developer Platform.**

`Clientus.ApiClient` is the official .NET 8 SDK for **Clientus Public API v1**.

## Install

```bash
dotnet add package Clientus.ApiClient --prerelease
```

Current beta line: **1.0.0-beta.6**

## Beta.6 Home sandbox preview

Beta.6 preserves the beta.5 read-only surfaces and adds household-scoped Home reads for context, members and Family Documents overview through `client.Home`.

Home uses the server-controlled `home:read` scope and is sandbox/grant-only in HOME.LONG.3B. Live Home access remains fail-closed and no Home API Pack price is implied by this package.

The Home surface is intentionally read-only. Server-side scopes, entitlements, tenant authority, quotas and rate limits remain authoritative.

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
