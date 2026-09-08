# SDK architecture

This document describes the architecture that exists in `1.0.0-beta.2`. Future modules are listed in
the roadmaps, not presented here as implemented components.

## Public API transition boundary

Public services depend internally on `IClientusApiTransport`, rather than on the concrete HTTP
implementation. `ClientusHttpClient` is the legacy transport and still applies Supabase/PostgREST
details required by this beta. The internal boundary allows a later Public API v1 transport without
inventing routes or credentials before their server contracts exist.

`ClientusConfiguration.BaseUrl` is future-neutral. `ApiKey` remains the legacy Supabase publishable
key and must never contain a service-role key. Optional message-handler ownership is explicit through
`DisposeHttpMessageHandler`. Developer credentials and environment selection remain deferred.

`ApiException` carries server-supplied HTTP status, code, message, request ID, retryability,
`Retry-After`, and validation details where available. The SDK does not synthesize missing server
metadata. A versioned User-Agent identifies SDK traffic without sending customer telemetry.

`PageRequest` and `Page<T>` define opaque continuation metadata for Public API v1. Current legacy
list methods remain compatibility APIs and do not pretend to implement server pagination.

## Dependency diagram

```text
Application
    |
    v
ClientusClient
    |-- AuthService
    |-- CustomersService
    |-- QuotesService
    |-- InvoicesService
    `-- UserService
             |
             v
     shared ClientusHttpClient
        |-- ClientusConfiguration
        |-- JsonHelper
        `-- authenticated HTTP requests
             |
             v
      Supabase Auth and PostgREST
             |
             v
       Row-level security (RLS)
```

## Entry point and service lifecycle

`ClientusClient` is the main entry point. Its constructor validates configuration by creating one
`ClientusHttpClient`, then creates stable service instances that share that transport. The parent owns
the transport and disposes it exactly once. Service access and operations after disposal throw
`ObjectDisposedException`.

The SDK implements synchronous `IDisposable`; consumers use `using`, not `await using`.

## Configuration validation

`ClientusConfiguration` supplies an absolute HTTP/HTTPS `BaseUrl`, a non-empty anonymous/public API
key, timeout, maximum attempts, and initial retry delay. Invalid URLs and missing keys produce
`ArgumentException`; invalid timeout/retry values produce `ArgumentOutOfRangeException`.

Service-role credentials are not part of the client-application contract.

## HTTP and retry policy

All current services use the shared transport. GET, HEAD, and DELETE can retry HTTP 408, 429, 502,
503, and 504 up to the configured total attempt count. POST and PATCH are not retried. Cancellation
and non-transient failures are not retried. Retried operations create a fresh request, and both
intermediate and final responses are disposed.

An unsuccessful final response becomes `ApiException`, preserving the HTTP status and raw response
body. HEAD count operations require an exact `Content-Range` total.

## Authentication, PostgREST, and tenancy

Authentication requests use Supabase Auth. A successful login sets the bearer token on the shared
transport. Domain services use authenticated PostgREST requests. Supabase RLS remains the authority
for tenant visibility and mutation authorization; the SDK neither resolves tenant membership itself
nor bypasses RLS.

`PostgRestQuery` is a small internal helper for identifier validation, escaped exact filters, and
empty read-only list normalization. It is not public API.

Legacy email/password authentication remains available. Username authentication is disabled because
its pre-authentication lookup RPC is no longer available to anonymous callers. Developer/application
authentication remains deferred to a verified Public API contract.

Direct quote status mutation is obsolete and fails closed because a table PATCH cannot reproduce the
current server permission, delivery, transition, and automation workflow. Other PATCH and DELETE APIs
remain legacy compatibility operations; canonical authorization must be enforced by the future API.

## Serialization

`System.Text.Json` is configured centrally through `JsonHelper`. Database fields use explicit
`JsonPropertyName` attributes where snake_case mapping is required. Numeric database values remain
`decimal`; timestamps use `DateTimeOffset`; existing date-only database values use the established
string representation; JSON snapshots use `JsonElement`.

Verified enums use the internal lowercase enum converter. Unknown named values fail deserialization
with `JsonException`, matching the established strict named-value policy.

Mutation payloads are anonymous, operation-specific objects. This prevents readable or protected
model fields from being serialized accidentally into PATCH requests.

## Results and collections

List methods expose `IReadOnlyList<T>`. Null database arrays normalize to empty read-only results.
`QuoteWithItems` and `InvoiceWithItems` copy item lists into snapshots so later changes to the source
collection do not alter the returned result.

## Testability

The public transport constructor owns a normal `HttpClient`. An internal constructor accepts an
`HttpMessageHandler` for deterministic tests. `InternalsVisibleTo("Clientus.ApiClient.Tests")` grants
the test assembly access without exposing handler injection as production API. Tests use no live
network or credentials.

## Projects and packaging

- `Clientus.Core` contains shared foundational types referenced by the API client.
- `Clientus.ApiClient` contains configuration, authentication, transport, models, and services.
- `Clientus.ApiClient.Tests` contains deterministic unit/contract tests.
- `Clientus.Tools` is a solution utility project; it is not part of the packaged API surface.

The `Clientus.ApiClient` NuGet package targets `net8.0` and includes its DLL, XML documentation,
README, and icon. Packing also creates a symbol package containing the portable PDB. Repository URL,
Source Link, deterministic build, nullable annotations, and symbol generation are configured in the
project and `Directory.Build.props`.
