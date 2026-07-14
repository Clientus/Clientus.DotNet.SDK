# SDK Sprint 01

**Data:** 14 Luglio 2026

---

# Obiettivo

Avviare lo sviluppo dello SDK ufficiale .NET di Clientus creando le fondamenta dell'architettura e implementando il primo modulo completo.

---

# Attività svolte

## Creazione soluzione

È stata creata la soluzione:

```
Clientus.DotNet
```

composta da:

- Clientus.Core
- Clientus.ApiClient
- Clientus.Tools

---

## Clientus.Core

Implementata la libreria condivisa.

Struttura iniziale:

- Common
- Configuration
- Exceptions
- Extensions
- Interfaces
- Models
- Validation

Implementata la classe:

Result

---

## Clientus.ApiClient

Creata la libreria di comunicazione.

Implementata la struttura iniziale dei moduli.

---

## Configurazione

Implementata:

ClientusConfiguration

con supporto:

- BaseUrl
- ApiKey
- Timeout

Configurazione caricata tramite User Secrets.

---

## Livello HTTP

Implementato:

ClientusHttpClient

Funzionalità:

- GET
- POST
- gestione Bearer Token
- gestione ApiKey
- serializzazione JSON

---

## Authentication

Implementati:

LoginAsync

GetCurrentUserAsync

LogoutAsync

Supporto login tramite:

- email
- username

---

## Customers

Implementato il primo modulo operativo.

Funzioni disponibili:

- GetAllAsync
- GetByIdAsync
- SearchAsync

---

## Customer

Creato modello tipizzato.

Supporto proprietà principali:

- Id
- CompanyId
- FirstName
- LastName
- DisplayName
- Email
- Phone
- Address
- PostalCode
- City
- Country
- ClientType
- CreatedAt

Proprietà calcolata:

FullName

---

## Clientus.Tools

Applicazione Console per test dello SDK.

Verificati con successo:

- Login
- Bearer Token
- Recupero utente
- Recupero clienti
- Ricerca clienti
- Recupero cliente tramite ID
- Logout

---

# Stato

Sprint completato con successo.

Lo SDK comunica correttamente con il backend reale di Clientus.

---

# Sprint successivo

Modulo Products.

Obiettivo:

realizzare il secondo modulo completo dello SDK con supporto CRUD e ricerca.