# Changelog

All notable SDK changes are documented here. Public NuGet publication is not implied by a version
entry; the repository currently verifies local package generation.

## Unreleased

### Documentation

- Consolidated the root README as the public entry point.
- Added the current module reference and security/tenancy guidance.
- Split the concise public roadmap from the detailed engineering roadmap.
- Marked sprint documents as completed historical implementation records.
- Reconciled examples, package instructions, retry behavior, and unavailable operations with the
  actual public API.

## 1.0.0-beta.1

### Added

- Initial SDK solution, configuration, authenticated HTTP transport, authentication/session support,
  error handling, JSON serialization, cancellation, and disposal lifecycle.
- Customers reads, search, update, existence/count, and deletion.
- Quotes RLS-controlled reads, item loading, existence/count, verified status transitions, and
  deletion.
- Invoices RLS-controlled reads, item loading, existence/count, and deletion.
- XML API documentation, package README/icon, Source Link, portable symbols, deterministic build,
  nullable annotations, and local NuGet packaging.
- Detailed SDK engineering roadmap.

### Changed

- Refined validation and lifecycle consistency across services.
- Centralized exact PostgREST filter/identifier handling and lowercase enum serialization where
  duplication was proven.
- Corrected retries so only GET, HEAD, and DELETE retry supported transient responses; POST and PATCH
  do not retry, cancellation does not retry, and each retry uses a fresh request.
- Preserved final API status/body information through `ApiException`.
- Grew the deterministic suite to 149 passing tests covering foundation, Customers, Quotes, and
  Invoices.

### Security

- Documented authenticated RLS-controlled visibility, anonymous-key configuration, and the exclusion
  of service-role/public-token workflows from the normal SDK client.
