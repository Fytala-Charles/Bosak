<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak implementation status">
  <br><br><h1>Bosak implementation status</h1>
  <p>Technical qualification, source candidates and open integration boundaries</p>
</div>

Last updated: **29 October 2026**. This page holds current engineering detail; the [README](../README.md) introduces the product and the [roadmap](../ROADMAP.md) describes direction. Dated checkpoints moved out of the README are preserved in [development history](DEVELOPMENT_HISTORY.md#readme-snapshot-2026-10-29). Original evidence dossiers remain unchanged.

## Releases and source state

| Scope | Recorded state | Evidence |
|---|---|---|
| Published Bosak packages | Verified 1.26.3, source `08fbdc31ac79baac04cf00f4773ec6d0b2dd247f` | [Publication receipt](RELEASE-1.26.3-PUBLICATION.md) |
| Current core source | Unpublished candidate 1.33.1 | `src/Directory.Build.props`; [C42 repair](conformance/k04-schema-element-fix/README.md) |
| Public schema engine | Apache-2.0, activation-free, 0.4 development line; no new NuGet release from the transition | [Schema publication](SCHEMA-APACHE-PUBLICATION-2026-10-29.md) |

## Recorded qualification

| Scope | Recorded result | Interpretation |
|---|---|---|
| Complete core solution | 24,203 passing tests | Inherited source-identical engine qualification; not rerun for this documentation cleanup. |
| Frozen QT3 gate | 31,397 passed / 0 failed / 424 skipped | Preserve exact skip names/reasons and adopted baseline. |
| qt4 gated run | 8,941 passed / 0 failed / 1,918 skipped; 230 gated sets, 11 experimental passes | Bounded adopted coverage, not complete current-draft certification. |
| Revised isolated schema prototype | 123 controls; twelve detected bad variants; 32 compiled proposed neutral types | [C44-R2 dossier](conformance/k04-schema-compatibility-prototype/README.md); no production API/ABI acceptance. |
| Public Schema solution | 114 passing tests and public CI | Separate schema-only suite; not a complete XSD 1.1 or core-provider certification. |

The [cross-system review](compatibility/xdm40-2026-10-29/README.md) and [revised prototype qualification](conformance/k04-schema-compatibility-prototype/QUALIFICATION.md) distinguish real observations, fake-provider controls, BCL witnesses and inherited results. Counts from different scopes must not be added together as one suite.

## Schema and XDM integration

Schema is now open source; the earlier proprietary-provider boundary no longer determines the public API shape. Reassess the smallest useful contract before C45. Keep host/XDM profile, XSD version, component authority and source/snapshot identity distinct, retain original PSVI across compatible universes and preserve caller ownership/cancellation.

Full XSD 1.1 structural validation, complete real component/provider coverage, engine atomization/copy integration and actual consumer source/ABI acceptance remain pending. The proposed API is unaccepted. REQ-139 remains pending; REQ-126 retains its bounded completion and all 48 historical observations.

The first [real open Schema integration](schema-integration/README.md) now provides an XSD 1.0 validation/typed-transformation example and a separate optional CI job. Fresh qualification passes 24,203 core tests, 122 source-integration cases (114 existing plus eight new), 114 unchanged Schema binary-replacement cases and both exact conformance gates, including unchanged skip identities/reasons. Braid passes 1,872 unchanged cases plus four separately rerun cases with its expected engine identity updated only in an isolated copy. These are bounded observations, not full XSD 1.1/session interoperability or consumer adoption; see the results receipt for the complete consumer disposition. The recorded qualification table above remains its earlier checkpoint.

A local unsigned preview combines the existing nine core candidate packages with Schema `0.4.0-alpha.1` using isolated dependency-pin overrides. Its package-only typed transformation and all ten package/output identities verify. This local preparation does not publish a release, change the public source pins or adopt an API.

XmlDb's corrected candidate route passes all 27 adapter source tests. All 593 unchanged integration binary cases have passing evidence across the initial run and explicit layout/timeout reruns; this is not one uninterrupted pass. The independent storage baseline was cancelled after 1,258 observed passes and is not certified as complete. Exact routing, package/output identities and original failures are retained in the qualification dossier.

Use `D:/Development/Bosak.Schema.Public` for active public Schema source. `D:/Development/Bosak.Licensing` contains private infrastructure; the original locked `D:/Development/Bosak.Schema` directory also points to that private repository. This local migration detail belongs here and in the handover, not in the user quick start.

## Authoring, tooling and newer profiles

XPath, XSLT and XQuery share the core data model/runtime. Source-preserving inspection and bounded edit operations underpin Braid's visual/source workflow; full live bidirectional editing is still a product goal, not a blanket delivered capability. See the [authoring roadmap](STRUCTURAL-AUTHORING-ROADMAP.md), [authoring implementation](REQ-135-IMPLEMENTATION.md), [4.0 coverage](REQ-118-xpath-xslt-40.md) and [integration guide](INTEGRATION.md).

The current-draft JNode/record/map/comparison gaps are recorded in the [immutable-draft review](compatibility/xdm40-2026-10-29/SPEC-REVIEW.md). A later repair requires its own adoption scope, regressions and unchanged-profile checks. No baseline promotion follows from a diagnostic pass alone.

## Known limits and development commands

- Extended-year date/dateTime construction is supported in documented cases; operations using `DateTimeOffset` remain bounded to years 1–9999 and can raise FODT0001 outside that range.
- Decimal arithmetic uses .NET's fixed 28–29 significant-digit representation.
- Some remote HTTP conformance resources are unavailable from this environment; the recorded Cloudflare challenge is separate from an engine assertion failure.

Normal source verification is `dotnet build Bosak.sln` and `dotnet test Bosak.sln`. [AGENTS.md](../AGENTS.md) supplies manual conformance commands and the Windows XSLT test workaround. The [XSLT harness guide](XSLT40-HARNESS-HARDENING.md) documents watchdogs and retained receipts.

New engineering checkpoints belong in this page, request-specific dossiers and [agent handover](AGENT_HANDOVER.md); dated historical summaries belong in [development history](DEVELOPMENT_HISTORY.md). Keep receipt provenance, source/publication identity and owner decisions separate.
