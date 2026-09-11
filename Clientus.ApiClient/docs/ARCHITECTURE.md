# Clientus SDK architecture

## Public API v1 boundary

The .NET SDK is aligned to the Clientus Public API v1 boundary.

The SDK does not treat database tables, Supabase Auth, or PostgREST routes as its public contract.

### Authentication

`ClientusConfiguration.DeveloperCredential` is an opaque developer credential issued by Clientus. The transport sends it through the HTTP Bearer authorization scheme.

Human email/password authentication is not part of the developer SDK contract.

### Environments

`ClientusEnvironment.Sandbox` and `ClientusEnvironment.Live` express the intended environment. Server-side credential and installation authority remains definitive; an SDK enum cannot elevate sandbox access into live access.

### Capabilities

The private-beta SDK currently exposes read-only operations for profile/current context, customers, catalog, quotes and invoices.

Write APIs, AI/Lumi APIs, webhooks, billing/developer payouts, vertical creation and marketplace publishing are separate future contracts and are not invented by this SDK.

### Fail-closed legacy compatibility

Some pre-Public-API method signatures are retained temporarily to reduce source breakage. Unsupported legacy operations throw `NotSupportedException` before making a network request.

### Transport

`IClientusApiTransport` remains the internal service boundary. `ClientusHttpClient` validates that SDK endpoints are versioned `/api/v1` routes and carries request/retry/error diagnostics.