# Clientus .NET SDK engineering roadmap

This is the detailed engineering source of truth. The concise external view is the
[public roadmap](Clientus.ApiClient/docs/ROADMAP.md). “Blocked” means an operation needs verified
server orchestration or privilege and must not be approximated with direct database requests.

| Module | Backend contract | SDK implementation | Tests | Remaining work / blocked workflows |
|---|---|---|---|---|
| Core | Verified | Configuration, HTTP, retries, errors, serialization, cancellation, lifecycle | Complete | Continue compatibility and release audits |
| Authentication | Verified for exposed operations | Login, refresh, current user/session, logout | Covered | Broader auth/admin flows require verification |
| Customers | Verified for exposed operations | Reads, search, supported-field update, exists/count, delete | Complete | Creation and proposed workflows require verification |
| Quotes | Verified | RLS reads/count/exists/items, verified status transitions, delete | Complete | Creation, conversion, public token, attachments, item/content editing are blocked |
| Invoices | Verified | RLS reads/count/exists/items and delete | Complete | Creation/editing/status, numbering, conversions, QR/IBAN, deposits, installments, payments, public documents, email and PDFs are orchestrated |
| Catalog | Not yet audited | Not implemented | None | Verify pricing/VAT, barcode, stock, search, and mutations |
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
- Version source: `Directory.Build.props` (`1.0.0-beta.1`)
- Nullable, deterministic builds, XML documentation, Source Link, and repository metadata: configured
- NuGet and `.snupkg` generation: verified locally
- Public package publication: not verified
- Deterministic test total: 149 passing

## Recommended next module

Catalog is next because quote and invoice line snapshots reference catalog items. Audit the backend
contract first and expose safe reads before considering mutations.
