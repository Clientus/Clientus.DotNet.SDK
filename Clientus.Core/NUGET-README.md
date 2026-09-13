# Clientus Core

`Clientus.Core` contains the shared contracts, models, results and exceptions used by the official **Clientus .NET SDK**.

Most developers should install **`Clientus.ApiClient`**, which references this package automatically.

```bash
dotnet add package Clientus.ApiClient --prerelease
```

## Clientus Developer Platform

Clientus provides a public Developer Platform for building authenticated integrations and applications using:

- Clientus Public API v1
- scoped developer credentials
- sandbox and live environments
- Lumi AI
- signed webhooks
- API Packs, quotas and usage controls
- Marketplace publishing foundations

Start here:

**https://clientus.app/developer**

## When should I reference Clientus.Core directly?

Reference `Clientus.Core` directly only when your project specifically needs shared SDK contracts without the full API client.

## Links

- **Developer Portal:** https://clientus.app/developer
- **Clientus:** https://clientus.app
- **Official .NET SDK:** https://www.nuget.org/packages/Clientus.ApiClient
- **Source:** https://github.com/Clientus/Clientus.DotNet.SDK