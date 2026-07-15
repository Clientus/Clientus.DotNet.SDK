# Clientus .NET SDK

The official .NET SDK for authenticated access to the Clientus platform. The current beta provides
the HTTP, configuration, authentication, tenancy, serialization, retry, and lifecycle foundation,
plus supported operations for Customers, Quotes, and Invoices.

## Current status

Version `1.0.0-beta.1` is defined in `Directory.Build.props`. The SDK targets .NET 8 (`net8.0`).
Local NuGet package generation is supported. Public NuGet distribution has not been verified and is
planned for a later release.

## Installation

Until a public package is announced, build a local package:

```powershell
dotnet pack Clientus.ApiClient/Clientus.ApiClient.csproj
dotnet add package Clientus.ApiClient --source Clientus.ApiClient/bin/Release
```

Do not treat the package ID as proof that the package is already available from nuget.org.

## Basic configuration

```csharp
using Clientus.ApiClient;
using Clientus.ApiClient.Configuration;

using var client = new ClientusClient(new ClientusConfiguration
{
    BaseUrl = "https://your-clientus-backend.example",
    ApiKey = "your-supabase-anon-key",
    Timeout = TimeSpan.FromSeconds(30),
    MaxRetryAttempts = 3,
    InitialRetryDelay = TimeSpan.FromMilliseconds(500)
});
```

`BaseUrl` must be an absolute HTTP or HTTPS URL. `ApiKey` is the public/anonymous project key. Never
embed a Supabase service-role key in a client application.

## Authentication overview

Authentication supports username or email login, session refresh, current-user lookup, and logout.
Successful login installs the bearer access token on the shared transport.

```csharp
using Clientus.ApiClient.Authentication.Models;

var login = await client.Auth.LoginAsync(new LoginRequest
{
    Identifier = "developer@example.com",
    Password = "secret"
});

if (!login.Success)
    throw new InvalidOperationException(login.Error);

var currentUser = await client.Auth.GetCurrentUserAsync();
```

## Quick start

```csharp
using var source = new CancellationTokenSource(TimeSpan.FromSeconds(15));

var customers = await client.Customers.GetAllAsync(cancellationToken: source.Token);
var quotes = await client.Quotes.ListAsync(source.Token);
var invoices = await client.Invoices.ListAsync(source.Token);
```

Every service operation accepts an optional `CancellationToken` as its final argument.

## Customers examples

```csharp
var customers = await client.Customers.GetAllAsync(limit: 50, cancellationToken: cancellationToken);
var customer = await client.Customers.GetByIdAsync(customerId, cancellationToken);
var matches = await client.Customers.SearchAsync("Ada", limit: 20, cancellationToken: cancellationToken);
var exists = await client.Customers.ExistsAsync(customerId, cancellationToken);
var count = await client.Customers.CountAsync(cancellationToken);

if (customer is not null)
{
    customer.Phone = "+41 00 000 00 00";
    customer = await client.Customers.UpdateAsync(customer, cancellationToken);
}

await client.Customers.DeleteAsync(customerId, cancellationToken);
```

Customer creation is not exposed. `UpdateAsync` sends only the supported contact/profile fields;
`Id`, `CompanyId`, and `CreatedAt` are selection/protected fields and are not included in its PATCH.

## Quotes examples

```csharp
using Clientus.ApiClient.Quotes;

var quote = await client.Quotes.GetAsync(quoteId, cancellationToken);
var quoteWithItems = await client.Quotes.GetWithItemsAsync(quoteId, cancellationToken);
var quotes = await client.Quotes.ListAsync(cancellationToken);
var exists = await client.Quotes.ExistsAsync(quoteId, cancellationToken);
var count = await client.Quotes.CountAsync(cancellationToken);

if (quote?.Status == QuoteStatus.Draft)
    quote = await client.Quotes.UpdateStatusAsync(quoteId, QuoteStatus.Sent, cancellationToken);

await client.Quotes.DeleteAsync(quoteId, cancellationToken);
```

Quote items are loaded with a second request and returned in ascending position order. See
[Modules](Clientus.ApiClient/docs/MODULES.md#quotes) for the exact status state machine and deletion
limitations.

## Invoices examples

```csharp
var invoice = await client.Invoices.GetAsync(invoiceId, cancellationToken);
var invoiceWithItems = await client.Invoices.GetWithItemsAsync(invoiceId, cancellationToken);
var invoices = await client.Invoices.ListAsync(cancellationToken);
var exists = await client.Invoices.ExistsAsync(invoiceId, cancellationToken);
var count = await client.Invoices.CountAsync(cancellationToken);

await client.Invoices.DeleteAsync(invoiceId, cancellationToken);
```

Invoice status mutation is intentionally unavailable because the verified backend workflow includes
payment-trigger and automation effects that a direct PATCH cannot reproduce.

## Retry behavior

The transport may retry GET, HEAD, and DELETE after HTTP 408, 429, 502, 503, or 504. The configured
`MaxRetryAttempts` includes the initial request, and each retry creates a fresh request. POST and
PATCH are never retried. Cancellation and non-transient failures are never retried.

DELETE retry safety describes transport behavior; authorization and final visibility remain backend
concerns.

## Cancellation

Pass a token to any asynchronous operation. Cancellation produces `OperationCanceledException` and
is not converted into a retry or an `ApiException`.

## Error handling

```csharp
using Clientus.ApiClient.Common;

try
{
    var quote = await client.Quotes.GetAsync(quoteId, cancellationToken);
}
catch (ApiException exception)
{
    Console.Error.WriteLine($"HTTP {(int)exception.StatusCode}: {exception.ResponseBody}");
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("The request was cancelled.");
}
```

Validation uses standard .NET exceptions. Successful count responses without a valid exact
`Content-Range` produce `InvalidOperationException`.

## Security, RLS, and tenant isolation

- SDK service requests are authenticated after a successful login.
- Supabase row-level security (RLS) determines which tenant rows are visible or mutable.
- The SDK does not bypass RLS and does not provide service-role behavior.
- Supported workflows do not let callers arbitrarily choose `company_id`.
- Public-token and service-role routes are intentionally not exposed.
- Direct database access is outside the SDK contract.

## Disposal and lifecycle

`ClientusClient` owns one shared HTTP transport. `Auth`, `Customers`, `Quotes`, `Invoices`, and
`Users` return stable service instances. Dispose the parent client with `using`; the SDK implements
`IDisposable`, not `IAsyncDisposable`. Access after disposal throws `ObjectDisposedException`.

## Supported operations matrix

| Module | Supported operations |
|---|---|
| Authentication | `LoginAsync`, `RefreshAsync`, `GetCurrentUserAsync`, `LogoutAsync` |
| Customers | `GetAllAsync`, `GetByIdAsync`, `SearchAsync`, `UpdateAsync`, `DeleteAsync`, `ExistsAsync`, `CountAsync` |
| Quotes | `GetAsync`, `GetWithItemsAsync`, `ListAsync`, `ExistsAsync`, `CountAsync`, `UpdateStatusAsync`, `DeleteAsync` |
| Invoices | `GetAsync`, `GetWithItemsAsync`, `ListAsync`, `ExistsAsync`, `CountAsync`, `DeleteAsync` |
| Users | Existing user lookup service; not expanded as part of the domain-service roadmap |

## Intentionally unavailable operations

The SDK does not expose customer creation; quote creation or generic editing; invoice creation,
editing, or status mutation; quote/work-report conversion; numbering; payments; QR Bill/IBAN
generation; deposits; installment generation; public-token flows; attachment mutation; PDF/document
generation; or email sending. These require contracts or server orchestration not represented by the
current authenticated SDK surface.

## Package contents

`Clientus.ApiClient` packages contain the `net8.0` assembly, XML API documentation, this README, and
the package icon. Packing also creates a `.snupkg` symbol package with portable PDBs. Source Link,
repository metadata, deterministic builds, nullable annotations, and embedded untracked sources are
configured centrally.

## Testing and build

```powershell
dotnet test Clientus.DotNet.sln
dotnet build Clientus.DotNet.sln
dotnet pack Clientus.ApiClient/Clientus.ApiClient.csproj
```

Package output is under `Clientus.ApiClient/bin/Release/`. The test suite uses deterministic HTTP
handlers and does not require live credentials or external services.

## Release checklist

- Run tests, build, pack, and `git diff --check`.
- Confirm zero build warnings/errors and inspect both `.nupkg` and `.snupkg`.
- Validate XML documentation, README, icon, repository metadata, and Source Link.
- Review public API compatibility and the changelog.
- Verify backend contracts for every newly exposed operation.
- Publish only through the project’s approved release process.

## Documentation and roadmap

- [Documentation index](Clientus.ApiClient/docs/README.md)
- [Module reference](Clientus.ApiClient/docs/MODULES.md)
- [Public roadmap](Clientus.ApiClient/docs/ROADMAP.md)
- [Detailed engineering roadmap](SDK-ROADMAP.md)

## Contributing and license

No public contribution process or standalone license file is currently defined in this repository.
Coordinate changes with the Clientus maintainers and do not assume permission beyond the repository’s
configured copyright and ownership terms. Repository metadata points to
[Clientus.DotNet.SDK](https://github.com/Clientus/Clientus.DotNet.SDK).
