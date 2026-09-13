# Clientus .NET SDK

**Build integrations, automations and AI-powered applications on the Clientus Developer Platform.**

`Clientus.ApiClient` is the official .NET 8 SDK for **Clientus Public API v1**. It gives developers a typed client for authenticated Clientus integrations while keeping provider credentials, tenant security and platform authority on the Clientus side.

## What can you build?

Use Clientus to connect applications and services to business data and platform capabilities such as:

- customers and business context
- catalog data
- quotes and invoices
- Clientus Intelligence / **Lumi AI**
- signed webhooks
- sandbox and live environments
- scoped developer credentials
- API Packs and usage-controlled premium services
- apps prepared for the Clientus Marketplace

## Install

```bash
dotnet add package Clientus.ApiClient --prerelease
```

This package is currently distributed as a public beta.

## Get Developer access

Developer status is assigned by Clientus. There is currently **no monthly Developer membership fee**.

Open the Developer Portal:

**https://clientus.app/developer**

From the portal you can create apps, work with sandbox/live environments, manage credentials and scopes, configure webhooks, review usage, activate API Packs and prepare Marketplace submissions.

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

Developer credentials are app- and environment-specific. Keep them server-side and never embed them in browser or mobile application source.

## Public API v1

The SDK uses the versioned Clientus Public API rather than direct database, Supabase Auth or PostgREST access.

Current versioned business surfaces include:

- developer/company context
- customers
- catalog
- quotes
- invoices

Scopes and entitlements are enforced server-side.

## Lumi AI

The SDK exposes Clientus Intelligence through `client.Ai`.

Lumi requests are subject to the Developer Platform's authorization, consent, entitlement and credit authority. AI provider credentials remain inside Clientus and are never exposed to third-party applications.

## Webhooks

Use `WebhookSignatureVerifier.Verify(...)` before accepting a Clientus webhook payload.

Webhook endpoints use versioned event delivery, signing credentials and replay-window verification.

## API Packs

Clientus does not currently charge a monthly fee for Developer status.

Access is based on API products:

- a one-time **Core API Pack** for the main low-cost Clientus APIs
- optional API packs for additional capabilities
- premium APIs/services with included usage allowances
- explicit pay-as-you-go overage after included premium usage

Entitlements and billing are authoritative on the Clientus server.

## Sandbox and live

Develop against `ClientusEnvironment.Sandbox`.

Live access is controlled by Clientus scopes, entitlements and approvals. Changing the SDK environment value alone cannot grant production access.

## Marketplace

Clientus includes a Developer Marketplace workflow with versioned submissions, Lumi pre-review, Clientus review, publishing controls, purchase entitlements, earnings and payout foundations.

Marketplace eligibility and publishing approval are managed in the Developer Portal.

## Security

The SDK is designed around:

- scoped developer credentials
- server-side authorization
- tenant isolation
- fail-closed entitlement checks
- rate limits and quotas
- signed webhooks
- no public service-role access
- no direct database contract for third-party applications

## Links

- **Developer Portal:** https://clientus.app/developer
- **Clientus:** https://clientus.app
- **Source:** https://github.com/Clientus/Clientus.DotNet.SDK

---

Clientus is building a unified platform for Business, Life and future Clientus products, with Lumi as the AI interaction layer and a Developer Platform for external integrations and applications.