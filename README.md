# Clientus .NET SDK

The official .NET 8 SDK for developers building integrations and applications on the **Clientus Developer Platform**.

The SDK uses **Clientus Public API v1**, scoped developer credentials and server-side authorization. It does not expose Clientus database tables, Supabase Auth, service-role credentials or unversioned PostgREST endpoints as the third-party contract.

## Public beta

Current package line:

```text
Clientus.ApiClient 1.0.0-beta.4
Clientus.Core      1.0.0-beta.4
```

Install from NuGet:

```bash
dotnet add package Clientus.ApiClient --prerelease
```

Developer Portal:

**https://clientus.app/developer**

Developer status is assigned by Clientus and currently has no monthly membership fee. API access is activated through API Packs and premium service entitlements.

## Authentication

The public SDK uses an opaque **developer credential**:

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

The credential is sent as:

```text
Authorization: Bearer <developer-credential>
```

Never embed developer credentials in browser or mobile application source.

## Public API v1

The current versioned business surfaces include:

- current developer/company context (`profile:read`)
- customers (`clients:read`)
- catalog (`catalog:read`)
- quotes (`quotes:read`)
- invoices (`invoices:read`)

Scopes, entitlements, quotas and rate limits are enforced server-side.

## Lumi AI

Use `client.Ai` for Clientus Intelligence / Lumi.

AI requests remain subject to Clientus authorization, consent, entitlement and credit authority. Provider selection and provider credentials stay inside Clientus.

## Webhooks

Use:

```csharp
WebhookSignatureVerifier.Verify(...)
```

before processing webhook deliveries.

Webhook delivery uses signed versioned events and replay-window verification.

## Sandbox and live

Use `ClientusEnvironment.Sandbox` during development.

Live access remains controlled server-side by Clientus. Selecting the live environment in the SDK does not itself grant production access.

## API Packs

Clientus Developer status currently has no monthly fee.

The commercial model is based on:

- a one-time Core API Pack
- optional additional API packs
- premium APIs/services with included usage
- explicit pay-as-you-go overage after included premium usage

The Clientus server is authoritative for entitlements, metering and billing.

## Marketplace

The Developer Platform includes versioned submissions, Lumi pre-review, Clientus review, Trusted Publishing eligibility, Marketplace purchase entitlements, earnings and payout foundations.

## Retired legacy behavior

Direct `/rest/v1` PostgREST access, `/auth/v1` end-user login, Supabase publishable keys, arbitrary user searches, direct destructive mutations and other unversioned legacy behavior are not part of Public API v1.

Legacy public method signatures retained for source compatibility are obsolete and fail closed rather than silently bypassing the public contract.

## Documentation

- Developer Portal: https://clientus.app/developer
- Clientus: https://clientus.app
- Repository: https://github.com/Clientus/Clientus.DotNet.SDK
- Minimal example: `examples/BasicConsoleApp`
- SDK documentation: `Clientus.ApiClient/docs`

## Security principles

The SDK and Developer Platform are designed around:

- scoped developer credentials
- server-side authorization
- tenant isolation
- fail-closed entitlement checks
- rate limits and quotas
- signed webhooks
- no service-role access for third-party applications
- no direct database contract