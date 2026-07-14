# Clientus .NET SDK

Official .NET SDK for the Clientus platform.

## Packages

- Clientus.Core
- Clientus.ApiClient

## Installation

```powershell
dotnet add package Clientus.ApiClient
```

## Example

```csharp
using Clientus.ApiClient;
using Clientus.ApiClient.Configuration;

using var client = new ClientusClient(new ClientusConfiguration
{
    BaseUrl = "https://your-server",
    ApiKey = "your-api-key"
});
```

## Available modules

- Authentication
- Customers
- Quotes (RLS-controlled reads, verified status transitions, and deletion)
- Invoices (RLS-controlled reads and deletion; orchestrated workflows intentionally excluded)
- Users

More modules will be added in future releases.
