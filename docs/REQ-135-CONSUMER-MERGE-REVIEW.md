# Bosak REQ-135 Consumer Implementation Review

## Exact-Merge Delivery and PR393 Review - 2026-10-21

Disposition: **No blocking findings** in [PR393](https://github.com/Fytala-Charles/Bosak/pull/393) at documentation head **932ce591910b4a58cabe16d719810ff58329c354**, or in the exact-merge REQ-135 delivery within the reviewed bounded profile. PR392 is now merged at **9218a2b6f2339f923fd6d018ba988c8325b2c44d**. PR393 remains open and changes documentation only. Its three CI checks pass (run39543856078); merged-main CI39543201162 succeeds at the exact implementation merge. Earlier open-PR/pending-merge statements below are historical.

Independently repeated the complete public-only consumer with a new isolated cache and Bosak.* mapped exclusively to the merge-specific local feed: **700 passed, zero failed/skipped** (117 public lifecycle +551 prior authoring/identity +32 Braid probes). This repeats head coverage; counts are not additive. No harness correction, engine change or removed/skipped case was needed for this rerun. The 19 source-only controls remain excluded.

All ten archive nuspec revisions and all nine consumed informational versions identify the full implementation merge. Independently compared archive/cache/output DLL bytes, cached archives, SHA-512, project.assets/contentHash and actual mapped restore-source receipts. Source and tests are unchanged between reviewed f9e618dc and the merge. Immutable handoff/readiness/verification snapshots, logs, TRX, assembly metadata and independent verified-provenance.json are retained separately in `.acceptance/req135-implementation-review-9218a2b6`; original head evidence remains intact.

The merge handoff correctly separates upstream117 checks, independent Braid evidence, frozen prior conformance results and pending consumer gates. Packages are unsigned local 1.23.0 candidates. Publication remains a separate decision, followed by signed public-package verification and Braid adoption. Session/review/Apply/history/UI/worker/restricted-export/reopen/native lifecycle acceptance remains pending. Braid stays on published1.22.0. No upstream message, merge, commit, push, release or native acceptance was performed by this review.

## Historical PR-Head Review - 2026-10-20

Date: 2026-10-20. Disposition: **No blocking implementation findings at the requested immutable PR head within the declared bounded literal-attribute lifecycle profile.** Reviewed [PR #392](https://github.com/Fytala-Charles/Bosak/pull/392) at **f9e618dc1bcec38ff552d6edfa80e36bfb42a335**. PR remains OPEN with no merge SHA. Source 1.23.0 is unpublished; Braid remains on published 1.22.0.

This is a separate implementation review. The [design review](BOSAK_REQ135_CONSUMER_DESIGN_REVIEW.md) and its scope/approval remain unchanged. No implementation merge authorization, publication approval, Braid adoption or native acceptance is implied by this result.

## Immutable Scope and CI

Read the exact-head implementation handoff, implementation record, verification JSON, API/capability/snapshot additions, complete LiteralAttributeLifecyclePipeline and both source test files through immutable Git blobs. Reviewed against the [consumer design](LITERAL_ATTRIBUTES.md), [ADR-021](ADR-021-reviewed-literal-attribute-lifecycle.md) and the reviewed upstream contract/LA01-LA23 groups. Moving upstream working files were not substituted for the requested commit.

Engine src is byte-identical between initial implementation head 7f4c66c0642f9eb9d0ffdfcbecefcdd9301c8335 and final requested head f9e618dc. The source test change between these heads adds XML return documentation/change history for a test, not runtime assertions. Three exact-head CI checks pass at run39454354443: Build & test (net10.0), XSLT wrapper controls (Windows), and VS Code highlighting.

The supplied delivery receipt identifies this exact head and final-head local package feed. Its package-provenance file SHA-256 matches the receipt. Source/API snapshots, handoff, verification record, original/corrected harness logs, test TRX and independent provenance are retained under `.acceptance/req135-implementation-review-f9e618dc`.

## Independently Executed Public Consumer Checks

Fresh isolated PackageReference-only local-package suite: **700 passed, zero failed/skipped**. Final TRX counters verified. Breakdown:

| Cases | Count | Scope |
|-------|-------|-------|
| Imported public REQ-135 cases | 117 | Complete lifecycle, literal semantics, source footprints, bounds, public authority/refusal/cancellation and compatibility/composition |
| Prior public authoring/identity checks | 551 | Existing expression/insertion/readable/text/element/deletion behavior and prior Braid probes |
| New independent Braid attribute probes | 32 | 16 lifecycle/source-form/encoding combinations,12 alternate-spelling NoChange cases, plus scope refusal, typed cancellation, foreign ownership and concurrent creation |

The 117 public cases overlap upstream source evidence; earlier upstream/source/package runs are not additive unique coverage. The 19 new internal source-only controls were excluded from this public consumer and were not independently executed. No engine ProjectReference, friend assembly identity, internal constructors or reflection into engine internals is used. The inherited metadata test reads public assembly identity only.

New probes independently verify one-edit byte replay; complete total graph mapping and complete per-owner attribute relations including namespace/control metadata; actual addressed attribute absence/pair and first-created position; ordinary attribute name/value/slot/order/byte retention; mapped owners; defensive source exports; fresh lifecycle targets; exact literal braces, quotes, tab/CR/LF, supplementary and decomposed Unicode in actual runtime attribute values; equal-Update exclusive NoChange with quote/entity/brace spelling retention; and source preservation on foreign/cancelled/unsupported operations. Source forms include paired/self-closing owners, ordinary mixed children, xml:space-preserved content and nested owners with unrelated namespace metadata across all four physical encodings.

Lifecycle tests cover Create -> equal Update NoChange -> Changed Update -> Delete -> Create empty. Each writes a resulting ordinary stylesheet to an isolated temporary file, independently reopens/inspects/compiles it and checks actual output attribute values. This is direct real-engine/file-consumption evidence, **not** Braid worker execution, restricted export, session/history/UI or native acceptance.

Initial harness builds stopped before test execution on the stricter Braid analyzer rules: an unused import in the extracted public-only boundary tests and a collection-size assertion in a new probe. Only the local harness import/assertion was corrected; an initial correction script also required explicit UTF-8 decoding for the Unicode probe source. Original build logs are retained. The successful run has no skipped/removed test or changed engine expectation; no engine/product fix was required.

## Independent Candidate Package Provenance

All **ten local candidate archive nuspec repository revisions** identify f9e618dc1bcec38ff552d6edfa80e36bfb42a335. All **nine consumed engine assembly informational revisions** identify 1.23.0 plus that exact head. Independently verified:

- Local archive, restored cache and consumer output DLL bytes agree.
- Cached archive bytes equal the provided candidate archives.
- SHA-512 archive/cache metadata and project.assets content hashes agree.
- Actual restore receipts use the final-head candidate feed, with Bosak.* mapped exclusively there and a new isolated cache; other test dependencies use nuget.org.

Evidence: archive-provenance.json, assembly-metadata.json, verified-provenance.json, restore.log and results-corrected2/consumer.trx in the retained review directory. These are **unsigned local source candidates**, not signed public NuGet packages. No public-feed/signature verification or adopted dependency is claimed.

## Implementation Assessment

| Area | Result |
|------|--------|
| Coordinated delivery and authority | All three operations are present in one PR, with one independent lifecycle capability and distinct opaque targets/proposals/results. Shared candidate Operation is explicit; Create/Delete have Candidate or Failure, Update has Changed/NoChange/Refused. |
| Constant logical values | Literal-only recognition operates on XML-expanded text rather than SlotKind/raw brace heuristics. Numeric-reference-introduced expressions refuse. Emission doubles literal AVT braces, escapes XML/retained delimiter and references tab/CR/LF. Actual candidate logical value is rechecked. |
| Source footprints | Create inserts space plus double-quoted token immediately after lexical owner name; Update replaces only retained ValueRange; Delete removes only the full attribute token and retains surrounding trivia. Exact outside bytes, owner spelling and descendants are preserved. |
| Empty versus absent / NoChange | Explicit optional attribute/logical/range metadata preserves absence versus empty. Equal logical Update runs after original authority/coverage/validity/bounds and before hypothetical canonical sizing, returns no candidate or edit, and retains original spelling/snapshot. |
| Complete graph/attribute proof | Recursive actual-reference graph mapping preserves child counts/order/names/text/ranges with duplicate-ID controls. Complete per-element ordinary attribute maps plus addressed pair/absence account for every attribute, including namespace/control metadata. Addressed Update position/name/value/slot consistency is separately checked; no fake attribute child IDs or deletion merge groups. |
| Profile and validation | Conservative no-namespace target, constant-result-attribute module coverage, static/resource/dependency/type/control/CDATA exclusions and independent original/candidate restricted validation match the reviewed profile. Ordinary mixed/preserved children can remain unchanged. |
| Bounds and encoding | Original/final physical bounds, count-before-allocation including escaping, strict UTF-8 BOM/no-BOM and UTF-16 LE/BE declaration rules, independent coordinates and real zero spans are retained. |
| Cancellation/immutability | New typed cancellation, final pre-success checks, defensive exports/read-only evidence and isolated parallel proposals fit the contract. Legacy APIs retain behavior. Cancellation is cooperative, not a preemptive parser deadline. |

No required correction identified within these boundaries. This does not approve dynamic AVT editing, qualified attributes/owners, rename, namespace/control edits, generic upsert, broader source editing or unrestricted conformance.

## Evidence Limits and Next Gates

Upstream reports 136 focused source cases (117 public/19 internal),10,214 solution,84 language-server,16 runner controls, frozen QT3 31,142/0/679 and qt4 gated 8,278/0/1,894. These are upstream receipts/CI, not independently rerun source/conformance checks. The earlier full-build nullable warning in conformance Program.cs:1868 is reported as pre-existing; the final incremental build reports zero warnings/errors. This review does not certify a new clean full build or eliminate that warning. Previous Braid 1,316 Release/551 public-package results remain separate historical integration evidence.

Next gates are separate: implementation merge authorization; immutable merge SHA plus matching CI/source/package provenance and a repeated exact-merge public consumer review; separately authorized signed public release and fresh public-feed verification; then Braid capability/port mapping, frozen operation-specific validation/review, current session/source/revision/license/overlap gates, atomic Apply/history/navigation, actual worker/export/reopen goldens and native lifecycle acceptance under LA17-LA22.

No Braid runtime/source/pin or engine implementation was changed. No upstream files/messages, PR merge, release authorization, commit or push were performed. Local implementation-review and companion tracking documentation remain uncommitted.
