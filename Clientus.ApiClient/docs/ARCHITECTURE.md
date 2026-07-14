# Architettura

## Filosofia

Lo SDK è suddiviso per domini funzionali.

Ogni modulo contiene esclusivamente la logica relativa alla propria area.

Tutte le comunicazioni HTTP passano attraverso un unico livello comune.

---

## Architettura

```
Clientus.Tools

↓

ClientusClient

↓

Authentication
Customers
Products
Quotes
Invoices
WorkReports
Files

↓

ClientusHttpClient

↓

Supabase REST API
Supabase Auth API
RPC
Storage
```

---

## Progetti

### Clientus.Core

Contiene tutto il codice condiviso.

Cartelle:

- Common
- Configuration
- Exceptions
- Extensions
- Interfaces
- Models
- Validation

---

### Clientus.ApiClient

Gestisce tutta la comunicazione con il backend.

Ogni modulo è indipendente.

Esempio:

```
Customers

↓

CustomersService

↓

ClientusHttpClient

↓

REST API
```

---

### Clientus.Tools

Applicazione Console utilizzata per testare tutte le funzionalità dello SDK.

---

## Obiettivi architetturali

- codice pulito
- servizi indipendenti
- nessuna duplicazione
- alta riusabilità
- estendibilità
- semplicità di manutenzione

---

## Principi

- Single Responsibility
- Dependency Injection Ready
- Async First
- Strong Typing
- Nessuna chiamata HTTP diretta al di fuori di ClientusHttpClient