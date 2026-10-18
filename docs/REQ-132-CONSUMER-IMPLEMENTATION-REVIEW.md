# Bosak REQ-132 Consumer Implementation Review

Date: 2026-10-18. Disposition: **No blocking implementation finding within the delivered bounded literal-element insertion profile.** Public release verification, Braid adoption and native acceptance remain separate gates.

## Exact Reviewed Delivery

Reviewed PR374 merge **91f1af0d488bb2ed72b219118eeb1ab3570726bd**, final head **2a614603319a007248e43b445ac60510cebf4f28**, source head0677f060. GitHub reports MERGED; both exact-head CI checks succeeded, including run39119447552. Final-head and merge source/tests have no differences. Independent clean checkout: `.dependencies/bosak-req132-review`; no engine edits.

Read the implementation handoff, implementation details, reviewed contract/acceptance matrix and completed local delivery receipt. The candidate source is **1.20.0, unpublished**; GitHub v1.20.0 release lookup returned no release during this review. No public NuGet publication or release approval is inferred. Braid remains pinned to published1.19.0/27691fdb.

## Independently Observed Checks

| Check | Observed result |
|-------|-----------------|
| Exact-merge source-focused literal-element tests | **91 passed, zero failed/skipped**, including public cases and six instrumented source-only controls. |
| Separate public-API package consumer | **399 passed, zero failed/skipped**: 374 imported public checks plus 25 new Braid probes, distinct non-friend assembly, no project references. |
| Candidate provenance | Ten original local archive hashes and nuspec revisions match upstream exact-merge proof. Nine consumed archive/cache/SHA-512/cache-DLL/output-DLL sets match; nine assembly identities are exactly1.20.0+91f1af0d488bb2ed72b219118eeb1ab3570726bd. |
| Dependency source and audit | Fresh isolated package cache; Bosak package mapping and nine restore metadata sources identify the exact merged local feed. Dependency audit reports no vulnerable packages. |
| Source integrity | Review checkout remains clean. Final-head/merge src/tests content matches. |

Counts overlap and must not be added as unique coverage. The database-provider archive was hash/revision checked but is not consumed by the nine-assembly focused harness. These are **unsigned local candidate packages**, not public/signed package verification. Upstream's full9,824 solution,84 language-server and frozen conformance results were read as upstream evidence, not independently rerun here. Braid's existing1,079-test result is historical baseline evidence, not a new Braid implementation run.

## Code and Consumer Findings

No required implementation correction was found. Reviewed the additive target/proposal/candidate surface, pipeline and integration changes against [the accepted consumer design review](BOSAK_REQ132_CONSUMER_DESIGN_REVIEW.md):

- Opaque typed targets are reference-bound to exact snapshots. Proposed names are not trimmed or normalized; NCName validation and namespace semantics remain engine-owned. Independent runtime-generated invalid surrogate, whitespace, colon, markup and empty-name probes return InvalidElementName without changing source.
- Local namespace resets and no-prefix default-XSLT contexts produce actual no-namespace output elements. A 16-case Braid matrix covers four physical encodings, both modes and ordinary/default-XSLT contexts. Each case creates an empty element, then populates it sequentially with literal Unicode/markup-looking/braced text and a value-of using fresh actual identities. Expanded output names and combined payload are checked independently.
- Complete byte edits, total old mapping, parent/order/attribute proof and actual NewNodes partition support consumer fidelity checks. Namespace declarations remain actual attribute metadata; empty-body selection has no fabricated node/Expression slot. Imported public cases verify edit replay, partitions, size/shrink-to-fit, refusal, immutability and legacy compatibility.
- Paired empty markup and exact body caret survive encoding variants. Candidate exports are defensive; original source stays byte-identical. Braid probes write a new ordinary file, independently reload its bytes, reinspect and execute them, comparing output. They use a fixed review module URI; this is not destination-relocation acceptance or the product's restricted export/worker/UI workflow.
- Four additional Braid checks verify ambiguous Readable layout refuses UnsupportedFormatting, preserves source and permits a separate explicit PreserveSource retry. The engine does not silently downgrade modes.
- Existing operation implementations remain unchanged; the new operation shares bounded layout/scope/correspondence primitives, adds a separate pipeline/capability, and appends the failure enum value without renumbering. Legacy public checks pass.

## Retained Corrections and Verification Limits

The first independent probe build failed CS1591 for four undocumented public harness members; XML comments were added without suppressions. The first runtime run passed387 and failed8 assertions expecting Readable success for an indentation-ambiguous fixture. Inspection showed classified conservative refusal consistent with the contract. Original TRX is retained; compact happy-path fixtures replaced that assumption and four explicit refusal/retry checks were added. Final399/0/0 passes; no engine fix or hidden fallback was used.

The nested upstream source-test project initially inherited outer Braid documentation/style enforcement and failed on legacy test sources. The isolated source rerun used command-line GenerateDocumentationFile=false, RunAnalyzers=false and TreatWarningsAsErrors=false. These override the incidental outer policy only for that source verification; normal Braid settings and the package-consumer enforcement remain unchanged. No zero-warning full-source-build claim is made from this rerun.

Receipts, original failed run, corrected run,91 source TRX, independent probes, audit and exact package provenance are retained under `.acceptance/req132-implementation-review-91f1af0d`. New probes cover sixteen lifecycle cases, five invalid-name cases and four readable refusal/retry cases. Full upstream solution/LS/frozen gates and153 broad-package checks were not independently rerun.

## Next Gate

The reviewed implementation is suitable to proceed through Bosak's release workflow, subject to maintainer approval. Verify the actual public release/tag/packages separately before changing Braid's pin. Then implement typed consumer adapter/evidence guards, separate review/Apply/history/new-element/body selection, outline/graph UI and real worker/restricted-export/reopen golden coverage, followed by human native acceptance.

No production pin/UI change, engine edit, commit, push, upstream notification or functional/native acceptance was performed by this review.

## References

- [Consumer scope and acceptance groups](LITERAL_ELEMENT_INSERTION.md)
- [Proposed ADR-018](ADR-018-bounded-literal-element-insertion.md)
- Upstream handoff: D:/Development/Bosak/docs/REQ-132-CONSUMER-IMPLEMENTATION-HANDOFF.md
- Upstream receipt: D:/Development/Bosak/.guard-tmp/req132-implementation-delivery.json
