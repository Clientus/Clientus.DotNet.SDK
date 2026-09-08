# Basic console application

This example uses current legacy connectivity to authenticate with Clientus and list customers
visible to the authenticated user through Supabase row-level security. It is not a Public API v1 or
developer-credential example; those contracts are not available yet.

Set `CLIENTUS_BASE_URL`, `CLIENTUS_API_KEY`, `CLIENTUS_IDENTIFIER`, and `CLIENTUS_PASSWORD`, then run:

```powershell
dotnet run --project examples/BasicConsoleApp/BasicConsoleApp.csproj
```

Use a publishable/anonymous project key. Never use a Supabase service-role credential in an
application.
