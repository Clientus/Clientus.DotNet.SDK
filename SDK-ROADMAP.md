# Clientus .NET SDK roadmap

This roadmap records only verified SDK capability. “Blocked” means the backend operation is an
orchestrated or privileged workflow and must not be approximated with direct database requests.

| Module | Backend contract | SDK implementation | Tests | Remaining work / blocked workflows |
|---|---|---|---|---|
| Core | Verified | Configuration, HTTP transport, retries, errors, serialization, lifecycle | Complete | Continue compatibility and packaging audits |
| Authentication | Partially verified | Login, logout, current session/user | Covered | Token refresh and broader auth flows require contract verification |
| Customers | Verified for exposed operations | Reads, search, create/update/delete as currently supported | Complete | Add only newly verified customer workflows |
| Quotes | Verified | RLS reads/count/exists/items, verified status transitions, delete | Complete | Creation, conversion, public-token, attachments, item/content editing are blocked server workflows |
| Invoices | Verified | RLS reads/count/exists/items and delete | Complete | Creation/editing/status workflows, numbering, quote/work-report conversion, QR/IBAN generation, deposits, installments, payments, public documents, email and PDFs are server-orchestrated |
| Catalog | Not yet audited | Not implemented | None | Verify catalog, pricing/VAT, barcode, stock and mutation contracts |
| Work Reports | Not yet audited | Not implemented | None | Verify reads first; creation and invoice conversion are expected server workflows |
| Agenda | Not yet audited | Not implemented | None | Verify appointments, recurrence, availability and notification effects |
| Payments | Invoice integration verified only | Not implemented | None | Payment recording, matching, Stripe and bank imports are server/trigger workflows |
| Installments | Invoice integration verified only | Readable through invoice fields | Invoice model coverage | Generation and lifecycle operations are server workflows |
| Contracts | Not yet audited | Not implemented | None | Verify contracts, versions, signatures and public sharing |
| Reports | Not yet audited | Not implemented | None | Verify report registry, filters, limits and export contracts |
| Marketplace | Not yet audited | Not implemented | None | Verify listing, installation and authorization contracts |
| Notifications | Not yet audited | Not implemented | None | Sending, templates, reminders and queues require server orchestration |
| AI | Not yet audited | Not implemented | None | Verify authenticated APIs, quotas, privacy and tool authorization |
| Voice | Not yet audited | Not implemented | None | Verify session, media and provider contracts |
| Developer APIs | Not yet audited | Not implemented | None | Verify API keys, scopes, webhooks, rate limits and audit logs |

## Recommended next module

Catalog is the next conservative candidate because quote and invoice line snapshots reference catalog
items. Start with a backend contract audit and expose read operations before considering mutations.
