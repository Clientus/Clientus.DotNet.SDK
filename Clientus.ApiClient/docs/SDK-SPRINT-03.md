# SDK Sprint 03

> **Status: Completed — historical implementation record.** This sprint records the Quotes and
> Invoices outcome. Current behavior is defined in [Modules](MODULES.md).

## Original intent

Add billing-domain services using verified backend contracts while preserving the hardened SDK
foundation.

## Final outcome

### Quotes

- Added RLS-controlled get, list, ordered items, existence, exact count, verified status transitions,
  and deletion.
- Added typed quote, item, discount, line-kind, and status models.
- Kept creation, generic editing, conversion, public-token, and attachment workflows unavailable.

### Invoices

- Added RLS-controlled get, list, ordered items, existence, exact count, and deletion.
- Added typed invoice, item, kind, status, discount, line-kind, and installment cadence models.
- Omitted status mutation because payment triggers and automation cannot be reproduced by direct PATCH.
- Kept creation, numbering, conversions, QR/IBAN, deposits, installments, payments, public documents,
  delivery, attachments, and PDFs unavailable.

### Foundation and quality

- Reused shared PostgREST query behavior and centralized proven enum conversion duplication.
- Preserved idempotent retry and lifecycle behavior.
- Reached 149 deterministic passing tests.
- Verified local NuGet and symbol packages without claiming public publication.

Catalog is the recommended next backend contract audit, beginning with safe reads.
