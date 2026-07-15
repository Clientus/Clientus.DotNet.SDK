# SDK Sprint 01

> **Status: Completed — historical implementation record.** Current behavior is documented in the
> [root README](../../README.md) and [module reference](MODULES.md).

**Original date:** 14 July 2026

## Original intent

Create the official Clientus .NET solution, establish its HTTP/authentication foundation, and deliver
the first typed domain module.

## Work completed in the sprint

- Created `Clientus.Core`, `Clientus.ApiClient`, and `Clientus.Tools` in the solution.
- Added `ClientusConfiguration` with backend URL, API key, and timeout configuration.
- Added the shared HTTP transport with JSON, API-key, bearer-token, GET, and POST support.
- Added authentication login, current-user lookup, logout, and username/email handling.
- Added the initial typed `Customer` model and customer list, lookup, and search operations.
- Used `Clientus.Tools` during early integration verification.

## Final outcome after later refinement

The foundation, authentication, and Customers work remain part of the SDK. Later work expanded
Customers, added Quotes and Invoices, hardened retry/disposal/validation, and established a
deterministic test suite. The originally proposed Products follow-up was not implemented without a
verified contract; Catalog remains a future audit candidate.

Historical early live-backend checks are not the current test strategy. Current tests use local
deterministic handlers and require no credentials or external services.
