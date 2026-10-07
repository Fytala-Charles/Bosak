# Bosak 1.0.0 — Release Notes (DRAFT)

> **Draft status:** prepared 2026-10-07 during the 1.0 close-out. Publish as the GitHub
> Release body for tag `v1.0.0` (and mirror to the release-notes field on nuget.org).
> Fill in the tag date and the support-channel section before publishing. The package
> version is **pinned in `src/Directory.Build.props`** — bump the pin to `1.0.0` before
> tagging (the v0.12.1 lesson: the tag does not set the version).
>
> **Release strategy (owner decision 2026-10-07):** these notes publish in two steps —
> first a **0.13.0 stable soak release** (same content, `-beta` postfix stripped, GA
> framing and the two TODOs removed) to gather issues/discussions before the 1.0
> commitment; then the 1.0.0 pair after the soak. The 0.13.0 notes must state that 0.x
> breaking changes remain possible per SemVer and the within-major API commitment
> formally begins at 1.0.0.

---

## What Bosak is

A ground-up, pure-managed .NET implementation of **XPath 3.1**, **XSLT 3.0**, and
**XQuery 3.1** on the W3C XQuery Data Model — no native dependencies, no Java bridge.
Version 1.0 is the general-availability release: the public API frozen since the Beta
line (0.10.0-beta) now carries a SemVer commitment, and the commercial schema-awareness
add-on **Bosak.Schema 1.0** tags alongside as a paired release.

## Conformance at 1.0

| Suite | Result | Notes |
|-------|--------|-------|
| QT3 (XPath 3.1 + XQuery 3.1, strict error codes) | **31,142 passed / 0 failed / 679 skipped** | 100% of runnable tests |
| W3C XSLT 3.0, basic processor sweep | **10,242 passed / 0 failed / 4,359 skipped** | **100.0% pass rate** (harness 3.73, 2026-10-06) |
| W3C XSLT 3.0, schema-aware sweep | **11,054 passed / 1 failed / 3,546 skipped** | the single failure is a documented platform limit (`type-functions-0401`, DateTime year < 1) |
| Unit tests | **2,758 green** across 9 assemblies + LanguageServer 72/72 | |

The 26 formerly-failing basic-sweep tests are triaged documented known limitations —
an upstream catalog annotation gap (no `schema_aware` dependency), reported as
[w3c/xslt30-test#90](https://github.com/w3c/xslt30-test/issues/90) and skipped with
reason in the harness (`docs/BASIC_SWEEP_TRIAGE.md`).

## Highlights

- **Complete XPath 3.1** — forward-compatible parser, three collations (codepoint,
  HTML ASCII case-insensitive, UCA), full `fn:*`/`math:*`/`map:*`/`array:*`/JSON library.
- **Complete XSLT 3.0** — packages (`xsl:package`/`use-package`/`override`), accumulators,
  keys, modes, `xsl:evaluate`, `fn:transform()`, strict static and dynamic error codes.
- **XQuery 3.1** — full core FLWOR, direct/computed constructors, `switch`/`typeswitch`,
  library modules, `fn:load-xquery-module`, ordering features, schema-aware
  `fn:json-to-xml`.
- **Streaming** (XSLT §19) — burst-mode streaming input, push-style streaming
  accumulators, streamable `xsl:source-document`, and a static streamability analyzer
  (XTSE3430); verified at 500k records in bounded memory.
- **Database providers** — `Bosak.XPath.Providers.Database`: REST adapters +
  `fn:collection` seam for BaseX, eXist, and MarkLogic.
- **Tooling** — the **Bosak XPath / XSLT** VS Code extension is on the Marketplace
  (`fytala.vscode-bosak`, 0.1.5): highlighting, semantic tokens, diagnostics, completion,
  hover, go-to-definition, document outline, code actions, code lens, XPath/XSLT/XQuery
  evaluation commands.
- **Licensing** — Bosak.Schema 1.0 (commercial add-on, sold separately): XSD complex-type
  schema-awareness through the same engine, BSK1 offline license keys with trial,
  Community, and paid tiers, 14-day renewal grace, and an automated issuance pipeline
  (Stripe Managed Payments, Postmark delivery) now live in production. See
  `COMMERCIAL.md`.

## Packages (10)

`Bosak.XPath.Api` · `Bosak.XPath.Core` · `Bosak.XPath.Parser` · `Bosak.XPath.Compiler` ·
`Bosak.XPath.Runtime` · `Bosak.XPath.Standard` · `Bosak.XPath.Providers` ·
`Bosak.XPath.Providers.Database` · `Bosak.Xslt` · `Bosak.XQuery` — all at `1.0.0`,
Apache-2.0, published to nuget.org via Trusted Publishing.

## Upgrading from the beta line

No breaking changes since `0.12.3-beta`. Consumers on any `0.10.x-beta`…`0.12.x-beta`
can bump the package reference to `1.0.0` and rebuild. (The one historical breaking
change — the `Bosak.XPath.Xslt` → `Bosak.Xslt` namespace rename — shipped before the
Beta line and is documented in `docs/FEATURE_REQUESTS.md`.)

## Known limitations (platform-bound)

- Date/time values with year < 1 (`DateTimeOffset` has no year 0/negative years).
- Decimal precision beyond 28–29 significant digits (.NET `decimal` is fixed-precision).
- Remote-HTTP-dependent conformance tests are blocked by Cloudflare from the test
  environment (skipped, not failed).

## Support

*TODO (owner, before tag): support channel per `COMMERCIAL.md` — community (Discussions /
issues) vs paid-priority terms for Bosak.Schema licensees.*

## After 1.0

XPath/XSLT 4.0 adoption per REQ-118 (stabilized features only, post-1.0), EXSLT
compatibility library (REQ-121, in progress), streaming residual backlog (REQ-087),
performance wave 2.
