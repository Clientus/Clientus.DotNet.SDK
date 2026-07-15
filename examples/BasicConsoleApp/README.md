# Basic console application

This example authenticates with Clientus and lists the customers visible to the authenticated user
through Supabase row-level security.

Set `CLIENTUS_BASE_URL`, `CLIENTUS_API_KEY`, `CLIENTUS_IDENTIFIER`, and `CLIENTUS_PASSWORD`, then run:

```powershell
dotnet run --project examples/BasicConsoleApp/BasicConsoleApp.csproj
```

Use a publishable/anonymous project key. Never use a Supabase service-role credential in an
application.
