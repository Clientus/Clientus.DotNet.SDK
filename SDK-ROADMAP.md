# Clientus .NET SDK engineering roadmap

## Public API transition status

Version 1.0.0-beta.2 establishes an internal transport boundary, future-neutral error and pagination
models, SDK identification, `Retry-After` support, explicit custom-handler ownership, and fail-closed
handling for unsupported username authentication and quote status mutation. Runtime domain methods
still use legacy Supabase/PostgREST routes until the canonical Clientus Public API v1 exists.

The next SDK phase must bind these foundations to verified Public API v1 endpoints, developer
credentials, scopes, environments, and paginated contracts. None of those server contracts are
claimed by this release.

This is the detailed engineering source of truth. The concise external view is the
[public roadmap](Clientus.ApiClient/docs/ROADMAP.md). “Blocked” means an operation needs verified
server orchestration or privilege and must not be approximated with direct database requests.

| Module | Backend contract | SDK implementation | Tests | Remaining work / blocked workflows |
|---|---|---|---|---|
| Core | Verified | Configuration, HTTP, retries, errors, serialization, cancellation, lifecycle | Complete | Continue compatibility and release audits |
| Authentication | Verified for email-based legacy operations | Email login, refresh, current user/session, logout; username fails closed | Covered | Developer credentials and broader auth/admin flows require verified Public API contracts |
| Customers | Verified for exposed operations | Reads, search, supported-field update, exists/count, delete | Complete | Creation and proposed workflows require verification |
| Quotes | Partial legacy contract | RLS reads/count/exists/items and delete; direct status mutation fails closed | Complete | Status workflow, creation, conversion, public token, attachments, and editing require Public API orchestration |
| Invoices | Verified | RLS reads/count/exists/items and delete | Complete | Creation/editing/status, numbering, conversions, QR/IBAN, deposits, installments, payments, public documents, email and PDFs are orchestrated |
| Catalog | Verified | RLS item/category reads, bounded search/type filter, exists/count, supported-field update, delete | Complete | Creation, category mutations, company rates, imports, and picker calculations are server workflows; no stock/active contract exists |
| Work Reports | Not yet audited | Not implemented | None | Verify reads; creation and conversion require workflow analysis |
| Agenda | Not yet audited | Not implemented | None | Verify appointments, recurrence, availability, and notifications |
| Payments | Invoice integration verified only | Not implemented | None | Recording, matching, Stripe, and bank imports are trigger/server workflows |
| Installments | Invoice integration verified only | Readable invoice fields only | Invoice model coverage | Generation and lifecycle are server workflows |
| Contracts | Not yet audited | Not implemented | None | Verify versions, signatures, and public sharing |
| Reports | Not yet audited | Not implemented | None | Verify registry, filters, limits, exports, and authorization |
| Marketplace | Not yet audited | Not implemented | None | Verify listing, installation, and authorization |
| Notifications | Not yet audited | Not implemented | None | Templates, reminders, queues, and sending require orchestration |
| AI | Not yet audited | Not implemented | None | Verify APIs, quotas, privacy, and tool authorization |
| Voice | Not yet audited | Not implemented | None | Verify session, media, provider, and retention contracts |
| Developer APIs | Not yet audited | Not implemented | None | Verify keys, scopes, webhooks, rate limits, and audit logs |

## Current release engineering

- Target: `net8.0`
- Version source: `Directory.Build.props` (`1.0.0-beta.2`)
- Nullable, deterministic builds, XML documentation, Source Link, and repository metadata: configured
- NuGet and `.snupkg` generation: verified locally
- Public package publication: not verified
- Deterministic test total: 149 passing

## Recommended next module

Work Reports are next because their Catalog dependencies and invoice-conversion boundary are now
known. Begin with a backend audit and expose safe reads before orchestrated workflows.
