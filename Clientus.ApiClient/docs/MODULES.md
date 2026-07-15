# Supported modules

This reference lists only operations implemented by the current public SDK. Every domain operation
uses the shared authenticated transport and remains subject to Supabase row-level security.

## Catalog

Supported:

- `GetAsync(string id, CancellationToken cancellationToken = default)`
- `ListAsync(CancellationToken cancellationToken = default)`
- `GetByTypeAsync(CatalogItemType type, CancellationToken cancellationToken = default)`
- `SearchAsync(string text, int limit = 20, CatalogItemType? type = null, CancellationToken cancellationToken = default)`
- `ListCategoriesAsync(CancellationToken cancellationToken = default)`
- `ExistsAsync`, `CountAsync`, `UpdateAsync`, and `DeleteAsync`

Lists order by kind and name. Type-filtered and search results order by name. Search performs the
verified case-insensitive contains query across name, description, and legacy category text, accepts
an optional item kind, and permits limits from 1 through 50. Categories order by `sort_order` and name.

`UpdateAsync` sends kind, name, description, legacy category, category ID, unit, price, photo URL,
price tax mode, and VAT rate. It never writes ID, company ID, creator, or timestamps. Owner-or-creator
RLS policies authorize updates and deletes.

Deleting an item sets verified quote-item, invoice-item, appointment, and work-report travel
references to null. Historical document-line pricing and VAT snapshots remain.

Not exposed: creation, category mutation/reordering/deletion, active behavior, stock/inventory,
travel-zone/rate workflows, imports, or server picker calculations. Creation is server-only because
it resolves or creates the caller's company and injects `company_id` and `created_by`.

## Customers

Supported:

- `GetAllAsync(int? limit = null, CancellationToken cancellationToken = default)`
- `GetByIdAsync(string id, CancellationToken cancellationToken = default)`
- `SearchAsync(string text, int limit = 20, CancellationToken cancellationToken = default)`
- `UpdateAsync(Customer customer, CancellationToken cancellationToken = default)`
- `DeleteAsync(string id, CancellationToken cancellationToken = default)`
- `ExistsAsync(string id, CancellationToken cancellationToken = default)`
- `CountAsync(CancellationToken cancellationToken = default)`

`UpdateAsync` selects by `Customer.Id` and sends first/last/display name, email, phone, address,
postal code, city, country, and client type. It does not write `Id`, `CompanyId`, or `CreatedAt`.
Customer creation is not exposed because there is no verified SDK creation contract.

## Quotes

Supported:

- `GetAsync`, `GetWithItemsAsync`, `ListAsync`, `ExistsAsync`, `CountAsync`
- `UpdateStatusAsync`
- `DeleteAsync`

`GetWithItemsAsync` performs two safe requests. It returns `null` when the quote is not RLS-visible;
otherwise it returns a quote plus a read-only item snapshot ordered by position ascending.

The verified status transitions are:

- `draft` → `sent`, `accepted`, `rejected`, or `expired`
- `sent` → `accepted`, `rejected`, `expired`, or `draft`
- `expired` → `sent`
- `accepted` and `rejected` are terminal
- same-state requests return the current quote without PATCH

Transitioning to sent sets `sent_at` only when it is currently null. Acceptance and rejection set
their corresponding timestamps. Moving to draft does not clear timestamps, and expiry invents no
timestamp. Authorization remains controlled by RLS and billing-management policies.

Deleting a quote cascades quote items and nulls configured invoice references. It does not delete
logical `company_documents` attachment rows or storage objects.

Not exposed: creation, generic content editing, item editing, conversion to invoice, public-token
GET/POST, or attachment upload/deletion. Those contracts require server workflows or have unsafe
cascade limitations.

## Invoices

Supported:

- `GetAsync`, `GetWithItemsAsync`, `ListAsync`, `ExistsAsync`, `CountAsync`
- `DeleteAsync`

`GetWithItemsAsync` uses separate invoice and invoice-item requests and orders items by position
ascending. Lists order by creation time descending. Counts and visibility include only rows allowed by
RLS.

Invoice status mutation is unavailable. The verified backend combines payment triggers, timestamps,
and automation side effects, and it does not expose a closed transition contract that a direct SDK
PATCH can faithfully reproduce.

Deletion cascades invoice items and deposit links and detaches references configured with `SET NULL`.
It does not delete logical `company_documents` rows or storage objects.

Not exposed: creation, generic editing, status mutation, numbering, quote/work-report conversion,
payment recording/processing, QR Bill or IBAN generation, deposits, installment generation,
public-token endpoints, email, attachments, or PDF/document workflows.

## Security and tenancy

- A successful SDK login supplies the bearer token used by domain requests.
- The configured API key is the public/anonymous key, never a service-role credential.
- Supabase RLS controls tenant visibility and mutation authorization.
- The SDK does not bypass RLS or permit arbitrary `company_id` selection in supported workflows.
- Service-role/public-token routes and direct database access are outside the SDK contract.
