# Clientus .NET SDK roadmap

## Completed

- VS.1 transition foundation
  - transport abstraction
  - error foundation
  - pagination foundation
  - retry diagnostics
  - unsafe username and quote workflow fail-closed behavior

- VS.2 Public API v1 realignment
  - developer credential Bearer authentication
  - sandbox/live configuration foundation
  - versioned read routes
  - current-context `/me`
  - customers/catalog/quotes/invoices read alignment
  - beta.5 Business Units, Warehouse, Loyalty, Customer Intelligence, Retail Returns, Retail Distribution, Retail Intelligence, Publishing, Website and Public Commerce reads
  - legacy Supabase/PostgREST transport retired from packaged SDK source
  - unsupported legacy mutations fail closed

- HOME.LONG.3B candidate
  - household-scoped `HomeService`
  - Home context, members and documents overview reads
  - `home:read` server-authority documentation
  - sandbox/grant-only Home contract; live remains fail-closed
  - beta.6 package candidate
  - PR build/test/pack and local-feed clean-install certification
  - no NuGet.org publication without explicit release approval

## Next

- explicit beta.6 NuGet release decision
- release tag/provenance only after approval
- public quickstart/versioning policy

## Future platform additions

Only after server contracts exist:

- Home live/commercial activation after explicit Home API Pack pricing approval
- write APIs
- outbound webhooks
- AI/Lumi APIs
- module/vertical development APIs
- marketplace publishing APIs
- usage/quota APIs
- monetization and payout APIs
- generated multi-language SDKs
