# Bosak — Roadmap

**Status:** Alpha · **Last updated:** 2026-09-03 · **Conformance baseline:** XSLT 3.0 strict sweep **7,713 / 17 / 6,870** (99.8%) · QT3 (XPath 3.1 + XQuery 3.1) **31,148 / 0 / 673** (100% of runnable) · unit tests **2,485 / 0 / 0**

---

## What Bosak is

A ground-up, pure-managed .NET implementation of **XPath 3.1** (with forward-compatibility for 4.0), **XSLT 3.0**, and **XQuery 3.1**, built on the W3C XQuery Data Model. No native dependencies, no Java bridge — the only third-party package in the tree is the MIT-licensed language-server library used by the VS Code extension.

## Where we are — Alpha

Alpha means: **feature-complete engine core, conformance-verified, API still evolving.**

| Area | State |
|------|-------|
| XPath 3.1 | Complete; QT3 sweep 100% of runnable tests |
| XQuery 3.1 | Phase 4 — full core FLWOR, direct/computed constructors, `switch`/`typeswitch`, library modules, `fn:load-xquery-module`, ordering features, schema-aware `fn:json-to-xml` |
| XSLT 3.0 | Packages (`xsl:package`/`xsl:use-package`/`xsl:override`/`xsl:original`), accumulators, keys, modes, `xsl:evaluate`, `fn:transform()`, strict error-code conformance at 99.8% |
| Tooling | VS Code language server (highlighting, diagnostics, completion, code lens, initial-template runner) |
| CI | GitHub Actions build+test on every push/PR; weekly W3C conformance sweep |

## Release stages

### Alpha → Beta

Everything below lands before the repo and packages are called Beta:

- [x] CI green on every push/PR
- [x] Apache-2.0 license + commercial layer defined (`COMMERCIAL.md`)
- [x] Repository hygiene (history scrubbed, customer references anonymized)
- [x] NuGet **preview** packages policy decided: `0.9.0-preview` line published from CI (see Versioning policy); local packages re-versioned from 1.0.0
- [ ] Remaining 17 strict-sweep failures triaged: fixed or individually documented as upstream test-suite artifacts / out-of-scope (see `docs/FEATURE_REQUESTS.md`, REQ-082 decision log)
- [ ] Public API review pass over `Bosak.XPath.Api` — thin surface, but naming and options objects freeze at Beta
- [ ] Issue templates, `CONTRIBUTING.md`, community scaffolding

### Beta → 1.0 (GA)

- [ ] Strict XSLT sweep at 100% of runnable tests that are not proven upstream artifacts
- [ ] API frozen; SemVer commitment begins
- [ ] Version promoted from `0.9.x-preview` to `1.0.0`; public release notes
- [ ] Integration guide (`docs/INTEGRATION.md`) and XML-doc coverage complete
- [ ] Support channel defined per `COMMERCIAL.md`

### Post-1.0 (not committed, in rough priority order)

- **XSLT streaming** — `xsl:supports-streaming` currently reports `no`; a streaming mode for very large documents is the largest single spec area not implemented
- **Full schema-awareness** — PSVI annotations exist for typed values and `fn:json-to-xml(validate:=true())`, but source-document validation (`is-schema-aware: no`) is not implemented; likely the core of a future commercial "Bosak Pro" add-on per `COMMERCIAL.md`
- **XPath 4.0** — the parser is forward-compatible; 4.0 features (e.g. `->` operator, bare `||`, etc.) land as the recommendation stabilizes
- **Performance work** — ArrayPool/span hot paths; no benchmarks published yet

## Known limitations (platform-bound, no fix planned)

These are consequences of .NET primitives, not bugs:

- **Date/time values with year < 1** — `DateTimeOffset` has no year 0 or negative years; affected tests are skipped in the QT3 sweep
- **Decimal precision above 28–29 significant digits** — .NET `decimal` is fixed-precision; literals and arithmetic beyond this are rounded (an arbitrary-precision decimal is a possible future project)
- **Remote-HTTP-dependent tests** — tests fetching external sites (e.g. `fn-unparsed-text-054a` → timeanddate.com) are blocked by Cloudflare from this environment; skipped, not failed

## Versioning policy

[Semantic Versioning](https://semver.org/). During Alpha/Beta the minor version moves freely and breaking API changes are called out in release notes. From 1.0.0 onward the public API of `Bosak.XPath.Api` is stable within a major version.

## How the backlog is tracked

Feature requests and engineering work items live in [`docs/FEATURE_REQUESTS.md`](docs/FEATURE_REQUESTS.md) — a living registry with per-item decision logs. Launch engineering is REQ-083; engine correctness history is REQ-078 through REQ-082.
