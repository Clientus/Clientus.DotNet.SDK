# Basic console application

This example uses Clientus Public API v1 with an opaque developer credential.

Set these server-side environment variables:

- `CLIENTUS_BASE_URL`
- `CLIENTUS_DEVELOPER_CREDENTIAL`

Then run:

```powershell
dotnet run --project examples/BasicConsoleApp/BasicConsoleApp.csproj
```

The example uses the sandbox environment. A sandbox credential cannot become a live credential, and
live access still requires server-side Clientus approval.

Never commit a developer credential and never embed one in browser or mobile application source.