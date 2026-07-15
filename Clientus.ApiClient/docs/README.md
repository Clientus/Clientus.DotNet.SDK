# Clientus .NET SDK documentation

This directory contains the public and historical documentation for the SDK.

## Current sources of truth

- [Root README](../../README.md) — installation, configuration, examples, behavior, security, and release commands.
- [Modules](MODULES.md) — exact supported Customers, Quotes, and Invoices operations and limitations.
- [Architecture](ARCHITECTURE.md) — current implementation structure, transport policy, lifecycle, and packaging.
- [Public roadmap](ROADMAP.md) — concise external view of completed and planned modules.
- [Engineering roadmap](../../SDK-ROADMAP.md) — detailed contract, implementation, test, and blocker status.
- [Changelog](CHANGELOG.md) — release notes for the beta and unreleased documentation work.

## Historical implementation records

- [SDK Sprint 01](SDK-SPRINT-01.md) — initial foundation, authentication, and Customers work.
- [SDK Sprint 02](SDK-SPRINT-02.md) — foundation hardening and Customers completion.
- [SDK Sprint 03](SDK-SPRINT-03.md) — Quotes and Invoices outcome.

Sprint documents preserve implementation history; they are not the current API reference. When a
historical note conflicts with current code, the root README, module reference, generated XML
documentation, and current tests take precedence.

## Document roles

| Document | Role |
|---|---|
| Root README / Modules | Current user documentation |
| Architecture | Current implementation notes |
| Changelog | Release notes |
| Public roadmap | High-level planned direction |
| Engineering roadmap | Detailed internal planning status |
| Sprint documents | Historical records |
