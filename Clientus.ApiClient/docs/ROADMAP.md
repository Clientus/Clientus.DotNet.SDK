# Public roadmap

This is the concise public roadmap. Detailed contract and blocker status lives in the
[engineering roadmap](../../SDK-ROADMAP.md). Planned items are not available SDK capabilities.

## Available in the current beta

- Core configuration, HTTP, authentication/session, serialization, retry, error, cancellation, and
  lifecycle foundation.
- Catalog: RLS item/category reads, type filtering, bounded search, supported-field update,
  existence/count, and deletion.
- Customers: reads, search, supported-field update, existence/count, and deletion.
- Quotes: reads, ordered items, existence/count, verified status transitions, and deletion.
- Invoices: reads, ordered items, existence/count, and deletion.
- Deterministic automated tests and local NuGet/symbol package generation.

## Recommended next module

Work Reports are the next contract-audit candidate because their Catalog dependencies and
invoice-conversion boundary are now identified. Safe reads should be verified first.

## Later contract audits

- Work Reports and Agenda
- Payments and Installments
- Contracts and Reports
- Marketplace and Notifications
- AI and Voice
- Developer APIs, including keys, scopes, webhooks, limits, and audit behavior

## Server-orchestrated blockers

Current SDK services intentionally do not approximate catalog creation/category workflows,
quote/invoice creation, numbering, conversion,
payments, QR Bill/IBAN logic, deposits, installment generation, public documents, attachment
mutation, PDF generation, or delivery workflows. These require verified server APIs and side effects.

## Release direction

The repository is packaging-ready for local beta artifacts. Public distribution, contribution
process, and later target-framework support require explicit release decisions and are not current
capabilities.
