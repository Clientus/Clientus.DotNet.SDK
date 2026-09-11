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
  - nine versioned read routes
  - current-context `/me`
  - customers/catalog/quotes/invoices read alignment
  - legacy Supabase/PostgREST transport retired from packaged SDK source
  - unsupported legacy mutations fail closed

## Next

- VS.3 package/release automation
- authenticated NuGet publication
- release tags and provenance
- clean-machine install certification
- public quickstart/versioning policy

## Future platform additions

Only after server contracts exist:

- write APIs
- outbound webhooks
- AI/Lumi APIs
- module/vertical development APIs
- marketplace publishing APIs
- usage/quota APIs
- monetization and payout APIs
- generated multi-language SDKs