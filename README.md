# Clientus .NET SDK

Clientus .NET SDK is the prerelease .NET 8 client for the Clientus Public API v1.

## Current beta contract

The SDK now uses the versioned Clientus Public API instead of direct Supabase/PostgREST access.

Authentication uses an opaque **developer credential**:

```csharp
using var client = new ClientusClient(new ClientusConfiguration
{
    BaseUrl = "https://your-clientus-host",
    DeveloperCredential = Environment.GetEnvironmentVariable("CLIENTUS_DEVELOPER_CREDENTIAL")!,
    Environment = ClientusEnvironment.Sandbox
});
```

The credential is sent as:

`Authorization: Bearer <developer-credential>`

Never embed developer credentials in browser or mobile application source.

## Public API v1 scope

The current private-beta SDK exposes read-only access to:

- current developer/company context (`profile:read`)
- customers (`clients:read`)
- catalog (`catalog:read`)
- quotes (`quotes:read`)
- invoices (`invoices:read`)

The nine current versioned routes are:

- `/api/v1/me`
- `/api/v1/customers`
- `/api/v1/customers/{id}`
- `/api/v1/catalog`
- `/api/v1/catalog/{id}`
- `/api/v1/quotes`
- `/api/v1/quotes/{id}`
- `/api/v1/invoices`
- `/api/v1/invoices/{id}`

## Sandbox and live

Use `ClientusEnvironment.Sandbox` during development. Live access remains controlled server-side by Clientus and cannot be enabled by changing the SDK configuration value alone.

## Retired legacy behavior

Direct `/rest/v1` PostgREST access, `/auth/v1` end-user login, Supabase publishable keys, arbitrary user searches, direct mutations, destructive operations, direct quote status transitions, list counts, and other unversioned legacy methods are not part of the Public API v1 contract.

Where old public method signatures are retained for source compatibility, they are marked obsolete and fail closed without sending a network request.

## Installation

During private beta, install from the prerelease NuGet package supplied by Clientus. Public NuGet publication is handled by the next release phase.

See `examples/BasicConsoleApp` for a minimal sandbox example.

## Developer bearer authentication

The public SDK uses Clientus Developer credentials against Public API v1.

Credentials are app- and environment-specific. Human end-user authentication is not the public Developer API contract.

## First call

```csharp
var configuration = new ClientusConfiguration
{
    BaseUrl = new Uri("https://clientus.app"),
    ApiKey = "<developer_credential>",
};

using var client = new ClientusClient(configuration);
```

Use `client.Ai` for Clientus Intelligence / Lumi. Provider selection and credentials stay server-side.

Use `WebhookSignatureVerifier.Verify(...)` before processing webhook deliveries.

The SDK does not grant entitlement. Activate the required API Pack in the Developer Portal.