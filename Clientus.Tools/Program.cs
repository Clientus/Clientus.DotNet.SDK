using Clientus.ApiClient;
using Clientus.ApiClient.Authentication.Models;
using Clientus.ApiClient.Configuration;
using Microsoft.Extensions.Configuration;

Console.WriteLine("=== Clientus SDK Test ===");
Console.WriteLine();

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var client = new ClientusClient(
    new ClientusConfiguration
    {
        BaseUrl = configuration["Clientus:BaseUrl"]!,
        ApiKey = configuration["Clientus:ApiKey"]!
    });

var result = await client.Auth.LoginAsync(
    new LoginRequest
    {
        Identifier = configuration["Clientus:TestEmail"]!,
        Password = configuration["Clientus:TestPassword"]!
    });

Console.WriteLine();

if (result.Success)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("LOGIN RIUSCITO!");
    Console.ResetColor();

    Console.WriteLine("Access Token ricevuto correttamente.");
    Console.WriteLine($"Scadenza: {result.Session!.ExpiresIn} secondi");

    var user = await client.Auth.GetCurrentUserAsync();

    Console.WriteLine();

    if (user is not null)
    {
        Console.WriteLine($"Utente ID: {user.Id}");
        Console.WriteLine($"Email: {user.Email}");

        var profile = await client.Users.GetCurrentAsync(user.Id);

        Console.WriteLine();
        Console.WriteLine("=== PROFILO CLIENTUS ===");

        if (profile is null)
        {
            Console.WriteLine("Profilo Clientus non trovato.");
        }
        else
        {
            Console.WriteLine($"Nome: {profile.FullName ?? "non disponibile"}");
            Console.WriteLine($"Username: {profile.Username ?? "non disponibile"}");
            Console.WriteLine($"Tipo account: {profile.AccountType ?? "non disponibile"}");
            Console.WriteLine($"Stato account: {profile.AccountStatus ?? "non disponibile"}");
            Console.WriteLine($"Stato approvazione: {profile.ApprovalStatus ?? "non disponibile"}");
            Console.WriteLine($"Demo: {(profile.DemoMode ? "Sì" : "No")}");
            Console.WriteLine($"Beta tester: {(profile.IsBetaTester ? "Sì" : "No")}");
        }
    }
    else
    {
        Console.WriteLine("Impossibile recuperare l'utente autenticato.");
    }

    Console.WriteLine();
    Console.WriteLine("=== CLIENTI ===");

    var customers = await client.Customers.GetAllAsync(10);

    Console.WriteLine();
    Console.WriteLine("=== TEST GetById ===");

    var first = customers.FirstOrDefault();

    if (first != null)
    {
        var loaded =
            await client.Customers.GetByIdAsync(first.Id);

        Console.WriteLine($"Cliente trovato: {loaded?.FullName}");
    }


    if (customers.Count == 0)
    {
        Console.WriteLine("Nessun cliente trovato.");
    }
    else
    {
        foreach (var customer in customers)
        {
            Console.WriteLine(
                $"- {customer.FullName} | " +
                $"{customer.Email ?? "email assente"} | " +
                $"{customer.City ?? "città assente"}");
        }
    }

    Console.WriteLine();
    Console.WriteLine("=== TEST RICERCA CLIENTI ===");

    Console.Write("Testo da cercare: ");
    var searchText = Console.ReadLine();

    if (!string.IsNullOrWhiteSpace(searchText))
    {
        var matches =
            await client.Customers.SearchAsync(searchText);

        Console.WriteLine($"Risultati trovati: {matches.Count}");

        foreach (var customer in matches)
        {
            Console.WriteLine(
                $"- {customer.FullName} | " +
                $"{customer.Email ?? "email assente"} | " +
                $"{customer.Phone ?? "telefono assente"}");
        }
    }

    Console.WriteLine();
    Console.WriteLine("=== TEST RICERCA UTENTI ===");

    Console.Write("Testo da cercare: ");
    var userSearchText = Console.ReadLine();

    if (!string.IsNullOrWhiteSpace(userSearchText))
    {
        var users = await client.Users.SearchAsync(userSearchText);

        Console.WriteLine($"Utenti trovati: {users.Count}");

        foreach (var profile in users)
        {
            Console.WriteLine(
                $"- {profile.FullName ?? "nome assente"} | " +
                $"{profile.Username ?? "username assente"} | " +
                $"{profile.AccountType ?? "tipo assente"}");
        }
    }

    await client.Auth.LogoutAsync();

    Console.WriteLine();

    Console.WriteLine(
        client.Auth.IsAuthenticated
            ? "Logout non riuscito."
            : "Logout riuscito.");
}

Console.WriteLine();
Console.WriteLine("Premi un tasto...");
Console.ReadKey();
