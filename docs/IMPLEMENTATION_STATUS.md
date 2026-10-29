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

The unpublished [Schema 0.5 native assessment candidate](schema-assessment/README.md) now checks one complete finite XSD 1.1 structure/decimal/assertion profile before exposing an owned document and original component state. Its 191-case suite (114 original plus 77 new) passes with published 1.26.3 and current core source; 114 unchanged legacy test binaries also pass with only Schema replaced. The API adds seven exported types without removing existing public members. These scopes overlap. Full typed anonymous-root projection remains explicitly unsupported; no general session API, product adoption or full XSD 1.1 claim follows.

Development is isolated at `D:/Development/Bosak/.worktrees/Bosak.Schema.Assessment`, branch `feature/bounded-document-assessment`, with uncommitted changes on public base `044d2cb`. The existing public checkout and its 1.26.3 dependency pins are unchanged. Core source/gates remain source-identical; their complete earlier qualification is inherited for this step. The candidate guide and receipts own the exact supported profile and refusal boundaries.

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

## Whole-document assessment candidate — 29 October 2026

The [bounded document bridge](schema-document-bridge/README.md) supersedes the earlier decimal-projection-only boundary for this unpublished candidate. Core 1.34.0 adds an optional XML-provider assessment carrier; native Schema 0.5 exports original anonymous-root/decimal states, preserving/stripping copies and explicit mutation/reassessment refusal. The general session proposal and broader REQ-139/REQ-009 acceptance remain pending. Source changes remain uncommitted; public release and consumer pins are unchanged.

Fresh bridge qualification: Release build has zero warnings/errors; complete core aggregate **24,207 passed**, native Schema **204 passed** (13 whole-document cases), QT3 **31,397/0/424**, qt4 gated **8,941/0/1,918**, 230 sets and 11 experimental passes. Exact skip mappings match the sealed integration checkpoint. The initial sandbox database failures are retained separately; the local-HTTP-authorized rerun passes all 47 cases. Whole-document example and strict Schema documentation checks pass. See [bridge evidence](schema-document-bridge/README.md).

## Bounded bridge API review — 29 October 2026

The [API review](schema-document-bridge/API-REVIEW.md) found and corrected failed-attachment exception safety: a replacement is checked completely before the previous binding is removed. Two regressions cover failed replacement and failed initial attachment. The trusted-host API carries immutable original state supplied by the host; it does not validate schemas or certify arbitrary host claims. Existing finite semantics and candidate version 1.34.0 remain unchanged. Shape-verification allocation cost is documented for later measurement. General API freeze and broader schema/session acceptance remain pending.

Current qualification and local checkpoint disposition are recorded in the [review checkpoint](schema-document-bridge/api-review/README.md). Earlier bridge/assessment receipts remain unchanged. Optional-child support is a proposed future slice, not implemented by this review.

Post-review verification: **24,209 core tests pass in one complete solution run**, **204 Schema tests pass**, and all six focused provider attachment cases pass. Release build: zero warnings/errors. QT3 **31,397/0/424**, qt4 **8,941/0/1,918** (230 gated sets, 11 experimental passes), with exact skip mappings unchanged. The full-document example retains original root state and passes typed-value and preserve/strip checks. Prior sealed assessment, bridge and integration receipts remain unchanged.

## Optional decimal children: scope prepared — 29 October 2026

The [pre-implementation adoption/scope checkpoint](schema-optional-children/README.md) pins the next bounded native Schema slice: optional local decimals, maximum occurrence one, ordered matching, present-instance state and assertion effective Boolean conversion. Optional support is not implemented; no current API, version, REQ completion, consumer pin or gate change. Fresh Schema baseline: 204 passes; 13 unique isolated diagnostic observations (the initial twelve are preserved separately). Core 24,209-pass and exact gate results are inherited from source-identical `a163ff11`, not rerun. Next: implement the separately recorded plan in the Schema feature worktree. General sessions and full XSD 1.1 remain pending.

Scope refinement: a thirteenth isolated observation exposes ignorable-whitespace removal being refused for an empty element-only assessed root. Supporting all-optional-absent documents therefore needs a bounded provider binding correction. The future implementation proposes core patch 1.34.1 plus fresh complete core tests and exact conformance gates; current versions and production source stay unchanged. Both the initial twelve and extended thirteen-observation receipts are preserved in the optional-child dossier; they overlap.

## Optional decimal scope delivery — 29 October 2026

Current delivery: [synchronized scope checkpoint](schema-optional-children/delivery/README.md). The user requested commit and push of the documentation in both repositories. Optional children remain planned, with 13 diagnostic observations and 204 fresh Schema baseline passes; current Schema 0.5/core 1.34.0 and all product source/pins are unchanged. The future implementation targets Schema 0.6 and the bounded core 1.34.1 whitespace fix, requiring fresh full core/gate qualification. Earlier uncommitted notices describe capture time, not this delivery. No REQ completion, merge or release is inferred.

## Optional decimal implementation candidate — 29 October 2026

The [bounded implementation](schema-optional-implementation/README.md) supersedes the earlier optional-scope next-action notices. Schema 0.6 adds local minimum-zero decimal particles, ordered matching, effective Boolean assertion conversion and present-only original state. Required-only v1 remains; optional schemas use v2. Core 1.34.1 corrects whitespace binding for empty element-only roots, retaining real-content/decimal edit refusal. Current Schema tests: 260 passes, 56 new cases; core adds five provider cases. Current candidate dependency references change only in the feature worktree; product pins, gates/fixtures and broader REQ acceptance remain unchanged. Full qualification is recorded separately in the new dossier; earlier sealed evidence remains unchanged. No commit/push/tag/publication in this step.

Fresh final qualification: **24,214 complete core tests pass** (including 8,196 XSLT and 212 provider cases), **260 complete Schema tests pass** (56 new optional cases), normal QT3 **31,397/0/424**, qt4 **8,941/0/1,918**, 230 gated sets and 11 experimental passes. Exact skip names/reasons match the sealed integration checkpoint. Core build has zero errors and five warnings in unchanged Standard test/XSLT conformance sources; no new warning source is introduced. The optional-gap whole-document example and strict Schema documentation check pass. Original 45-case before evidence is 9 passed/36 failed; all 45 now pass alongside eleven added boundary cases. Five new core cases cover the root binding correction.

## Optional decimal review checkpoint — 29 October 2026

Current delivery: [reviewed optional implementation](schema-optional-review/README.md). Review found no further production defect; all changed source hashes match the qualified 24,214-core/260-Schema candidate and exact unchanged gates. Schema 0.6/core 1.34.1 remain unpublished. User-authorized checkpoint commits/pushes use the existing core checkpoint and Schema feature branches. Earlier uncommitted implementation notices are capture-time history. Next: separately scope bounded repeated decimal children; no repetition, full schema/session acceptance, consumer adoption or release is inferred.

## Bounded repeated decimal scope — 29 October 2026

The reviewed optional implementation is checkpointed at core `5e277c6a` / Schema `773fd69`. The [next bounded repetition scope](schema-repeated-children/README.md) is prepared before implementation: local minimum zero/one, maximum up to sixteen and total maximum budget sixteen, ordered distinct instances sharing original declarations, narrow cardinality-error handling and ambiguous singular-export refusal. Current Schema 0.6/core 1.34.1 and tested source are unchanged. Sixteen scratch observations support scoping; 260 Schema/24,214 core and exact gate results are inherited from the reviewed source-identical candidate. Schema 0.7/v3 is a proposed future implementation only. No general API/session acceptance, merge or release.
