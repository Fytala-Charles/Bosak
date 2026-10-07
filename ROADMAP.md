# Bosak — Roadmap

**Status:** Beta — **1.0 close-out in progress (2026-10-07): release notes drafted (`docs/RELEASE-NOTES-1.0.md`), API-freeze/INTEGRATION/XML-doc items closed below; remaining before the paired tag: Bosak.Schema `v0.3.0` sign-off + §13.6 live-purchase/community/dead-letter smokes + first licensee + support-channel decision + the pin bump to `1.0.0`** · **Last updated:** 2026-10-07 · **Conformance baseline:** XSLT 3.0 schema-aware sweep **11,054 / 1 / 3,546** (conformance tail fully closed; the 1 is a documented platform limit) · XSLT 3.0 basic sweep **10,242 / 0 / 4,359 — 100.0% pass rate** (2026-10-06, harness 3.73 on main `929eea7`; the 26 former failures are triaged documented known limitations — upstream catalog dependency gap, `docs/BASIC_SWEEP_TRIAGE.md`, w3c/xslt30-test#90; earlier 3.72 record 10,250/0/4,351 on `b1a7479`) · QT3 (XPath 3.1 + XQuery 3.1) **31,142 / 0 / 679** (100% of runnable) · unit tests **2,729+ green** across 9 assemblies + LanguageServer 72/72

---

## What Bosak is

A ground-up, pure-managed .NET implementation of **XPath 3.1** (with forward-compatibility for 4.0), **XSLT 3.0**, and **XQuery 3.1**, built on the W3C XQuery Data Model. No native dependencies, no Java bridge — the only third-party package in the tree is the MIT-licensed language-server library used by the VS Code extension.

## Where we are — Beta

Beta means: **feature-complete, conformance-verified (100% of runnable tests on both W3C suites), public API reviewed and frozen for the 1.0 line.**

| Area | State |
|------|-------|
| XPath 3.1 | Complete; QT3 sweep 100% of runnable tests |
| XQuery 3.1 | Phase 4 — full core FLWOR, direct/computed constructors, `switch`/`typeswitch`, library modules, `fn:load-xquery-module`, ordering features, schema-aware `fn:json-to-xml`; QT3 sweep 100% of runnable tests |
| XSLT 3.0 | Packages (`xsl:package`/`xsl:use-package`/`xsl:override`/`xsl:original`), accumulators, keys, modes, `xsl:evaluate`, `fn:transform()`; strict error-code sweep 100% of runnable tests |
| Tooling | VS Code language server (highlighting, diagnostics, completion, code lens, initial-template runner) |
| CI | GitHub Actions build+test on every push/PR; weekly W3C conformance sweep |

## Release stages

### Alpha → Beta

Everything below lands before the repo and packages are called Beta:

- [x] CI green on every push/PR
- [x] Apache-2.0 license + commercial layer defined (`COMMERCIAL.md`)
- [x] Repository hygiene (history scrubbed, customer references anonymized)
- [x] NuGet **preview** packages policy decided: `0.9.0-preview` line published from CI (see Versioning policy); local packages re-versioned from 1.0.0
- [x] Strict-sweep failures triaged: **QT3 31,142/0/679 and XSLT 7,722/3/6,875 — 100% of runnable on both suites**; the 3 XSLT residuals are individually documented as out-of-scope (see `docs/FEATURE_REQUESTS.md`, REQ-082 decision log)
- [x] Public API review pass over `Bosak.XPath.Api` — thin surface confirmed; naming and options objects frozen as of the Beta line (`0.10.0-beta`); full XML-doc coverage pass over every published package (see `docs/INTEGRATION.md` recent changes)
- [x] Issue templates, `CONTRIBUTING.md`, community scaffolding — issue forms + PR template in `.github/`, Discussions enabled

### Beta → 1.0 (GA)

- [x] `v0.12.1-beta` published to nuget.org 2026-10-02 (all 9 packages via Trusted Publishing OIDC; carries REQ-101…REQ-115 — schema-aware sweep 11,027/28/3,546, QT3 31,142/0/679). Package version is pinned in `src/Directory.Build.props` — bump the pin before tagging.
- [x] `v0.12.2-beta` published to nuget.org 2026-10-03 (all 9 packages via Trusted Publishing OIDC; carries REQ-116 + REQ-117 + the REQ-117 tail PR #51 — schema-aware sweep 11,054/1/3,546 with the conformance tail fully closed, basic 10,250/26/4,325, QT3 31,142/0/679). Pin bumped before tagging per the pinned-version rule above.
- [x] `v0.12.3-beta` published to nuget.org 2026-10-03 (10 packages — first publish of `Bosak.XPath.Providers.Database`; carries REQ-118/119/120)
- [x] Basic-sweep residuals (26) triaged 2026-10-05 — all 26 documented as known limitations (upstream catalog dependency gap; harness 3.72 basic-only skips) in `docs/BASIC_SWEEP_TRIAGE.md`; **confirmation full sweep on `b1a7479`: 10,250/0/4,351 — 100.0% pass rate**; reported upstream as w3c/xslt30-test#90
- [x] Full basic-sweep record refreshed on harness 3.73 (maintainer rule: any env with a `<schema>` element = implicit schema-awareness dependency) — **2026-10-06, main `929eea7`: 10,242/0/4,359, 100.0% pass rate, single chunk**; per-test delta vs the 3.72 record is exactly 8 pass→skip (merge-049/050/052/053/054, type-0303, xpath-default-namespace-0501/0502), zero pass→fail; `.guard-tmp/work/sweep373-basic-final.log`, baseline `.sweep-baselines/basic-after-373.txt`
- [ ] Core 1.0 tagged as a pair with the commercial Bosak.Schema 1.0 (see the Bosak.Schema roadmap)
- [ ] **Bosak.Schema go-live ceremony — EXECUTED 2026-10-07 (owner-side, Bosak.Schema repo REQ-005 §13 runbook):** Steps 0–4 done (Stripe KYC + live products, production tenant `rg-fytala-licensing-prod`, ADR-003 signing-key ceremony, live secrets, Postmark/DKIM + `licensing@fytala.com`, bounce/complaint webhooks verified) and the **trial flow verified live** (real Turnstile → 30-day key, production-signature-verified). **Remaining: §13.6 live-purchase + community + dead-letter production smokes, `v0.3.0` tag (owner sign-off), first licensee — then the paired tags.** Zero core-repo work in this item; core 1.0 tags as the paired tag at Phase 4.
- [x] Strict XSLT sweep at 100% of runnable tests that are not proven upstream artifacts
- [x] API frozen; SemVer commitment begins — public API frozen since the Beta line (`0.10.0-beta`, see `docs/API_FREEZE.md`); the commitment formalizes at the `1.0.0` tag
- [ ] Version promoted from `0.9.x-preview` to `1.0.0`; public release notes — **release notes drafted at `docs/RELEASE-NOTES-1.0.md` (2026-10-07)**; the promotion itself is the pin bump in `src/Directory.Build.props` at tag time (pinned-version rule)
- [x] Integration guide (`docs/INTEGRATION.md`) and XML-doc coverage complete — XML-doc pass shipped at the Beta line (every published package); INTEGRATION.md current through 2026-10-07 (§0 covers every release and behavioral change since Beta)
- [ ] Support channel defined per `COMMERCIAL.md`

### Post-1.0 (not committed, in rough priority order)

- **XSLT streaming** — Phases A+B landed 2026-09-16: burst-mode streaming input via `XmlStreamingProvider` and `XsltExecutable.TransformStreaming` (record-at-a-time processing in bounded memory, verified at 500k records), plus push-style streaming accumulators (values computed per record as the stream arrives). Phase C landed 2026-09-17: `xsl:supports-streaming` reports `yes`, `xsl:source-document streamable="yes"` streams mid-transform documents, the `strm/` sets run through the harness, and a §19 static streamability analyzer raises XTSE3430 (106/110 unique expected-error corpus cases; `decl/accumulator` 102/0/5). Phase D landed 2026-09-17: analyzer-accepted streamable constructs execute — fused single-pass eager helpers, §11.7.3 content semantics, `fn:snapshot` grounding, opt-in record retention (tee/replay) for crawling union/except/intersect/fork shapes, streaming DTD support — full sweep **10,156/119/4,325** (post-release fork fix `9d81042`: si-fork-119/810/811/815). Remaining: the documented residual backlog (REQ-087)
- **Full schema-awareness** — PSVI annotations exist for typed values and `fn:json-to-xml(validate:=true())`, but source-document validation (`is-schema-aware: no`) is not implemented; likely the core of a future commercial "Bosak Pro" add-on per `COMMERCIAL.md`
- **XPath 4.0** — the parser is forward-compatible; 4.0 features (e.g. `->` operator, bare `||`, etc.) land as the recommendation stabilizes
- **Performance work** — first wave landed 2026-09-09 (REQ-085): BenchmarkDotNet harness in `benchmarks/Bosak.Benchmarks` with a published baseline; document-evaluation benchmarks improved 33–48% on time and 45–51% on allocations (wrapper cache, lazy axes, copy-free materialization). Further: XSLT result-tree construction, per-transform thread reuse, predicate-path allocations

## Known limitations (platform-bound, no fix planned)

These are consequences of .NET primitives, not bugs:

- **Date/time values with year < 1** — `DateTimeOffset` has no year 0 or negative years; affected tests are skipped in the QT3 sweep
- **Decimal precision above 28–29 significant digits** — .NET `decimal` is fixed-precision; literals and arithmetic beyond this are rounded (an arbitrary-precision decimal is a possible future project)
- **Remote-HTTP-dependent tests** — tests fetching external sites (e.g. `fn-unparsed-text-054a` → timeanddate.com) are blocked by Cloudflare from this environment; skipped, not failed

## Versioning policy

[Semantic Versioning](https://semver.org/). During Alpha/Beta the minor version moves freely and breaking API changes are called out in release notes. From 1.0.0 onward the public API of `Bosak.XPath.Api` is stable within a major version.

## How the backlog is tracked

Feature requests and engineering work items live in [`docs/FEATURE_REQUESTS.md`](docs/FEATURE_REQUESTS.md) — a living registry with per-item decision logs. Launch engineering is REQ-083; engine correctness history is REQ-078 through REQ-082.
