# Changelog

## 1.0.0-beta.6

- Added read-only household-scoped Home support through `client.Home`.
- Added `GET /api/v1/home/context`, `GET /api/v1/home/members` and `GET /api/v1/home/documents/overview`.
- Kept the Home surface mutation-free and based on the canonical `home:read` scope.
- Documented Home as sandbox preview / grant-only; live Home API access remains fail-closed server-side.
- No Home API Pack price is introduced by the SDK.
- Added package and clean-install validation to pull-request CI.
- Target Public API source: Clientus/clientus-ai commit `b40f330340d00df2cc6a13d55e05bdce4221d6ce`.
- NuGet.org publication is intentionally not performed by HOME.LONG.3B.

## 1.0.0-beta.5

- Added read-only Business Units support.
- Added read-only Warehouse stock support.
- Added read-only Loyalty support.
- Added read-only Customer Intelligence support.
- Added read-only Retail Returns support.
- Added read-only Retail Distribution support.
- Added read-only Advanced Retail Intelligence support.
- Added read-only Publishing support.
- Added read-only Website support.
- Added read-only Public Commerce support.
- Added `SdkNextRecord` for Public API objects intentionally defined as open schemas.
- Preserved developer-credential Bearer authentication and server-derived tenant authority.
- Kept all newly introduced beta.5 surfaces mutation-free.
- Target Public API source: Clientus/clientus-ai commit `8a7101944cbcacddbb1b37307f6b85ca37dfed85`.
- NuGet publication is not performed by the realignment phase.

## 1.0.0-beta.4

- Improved NuGet discovery and public Developer Platform positioning.
- Added package-specific NuGet README content for `Clientus.ApiClient` and `Clientus.Core`.
- Added Developer Portal project URL and stronger package tags.
- Updated public installation, developer credential, Lumi AI, webhook, API Pack and Marketplace documentation.
- Removed stale private-beta and future-publication copy from the public SDK README.
- Corrected package copyright encoding.

## 1.0.0-beta.3

- Realigned the SDK to Clientus Public API v1.
- Added developer credential Bearer authentication.
- Added sandbox/live environment configuration foundation.
- Routed supported read operations through nine versioned `/api/v1` endpoints.
- Retired direct Supabase Auth and PostgREST as SDK public contracts.
- Preserved unsupported legacy APIs only as obsolete fail-closed compatibility methods.
- Updated the console example and Public API v1 tests.
- No NuGet package was published by this phase.

## 1.0.0-beta.2

- Added the VS.1 Public API transition foundation.
