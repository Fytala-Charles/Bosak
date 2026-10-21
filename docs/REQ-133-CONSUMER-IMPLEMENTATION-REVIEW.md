# Bosak REQ-133 Consumer Implementation Review

Date: 2026-10-18. Disposition: **No blocking implementation finding within the delivered bounded literal-text body replacement profile.** Publication, public-package verification, Braid adoption and native acceptance remain separate gates.

## Exact Reviewed Delivery

Reviewed PR378 merge **8929a1301d1846bb24b49bcc505877f05fd3395a**, final head **6c5aa30f2ed59eee55649815f1645143c9ee7cc6**, implementation headd09737f1. Both exact-head CI checks succeeded, including run39138064320. Final-head/merge src/tests have no differences. The independent checkout `.dependencies/bosak-req133-review` remains clean; no engine edits.

Read the handoff, implementation record, agreed contract/matrix and completed local delivery/provenance receipts. Source1.21.0 remains unpublished: GitHub v1.21.0 lookup returned no release during review. This is not a public NuGet verification. Braid's production pin/runtime remain on1.20.0/91f1af0d.

## Independently Executed Checks

| Check | Observed result |
|-------|-----------------|
| Exact-merge replacement source-focused tests | **76 passed, zero failed/skipped**, including63 public cases and13 source-only authority/cancellation controls. |
| Imported public-only package consumer | **437 passed, zero failed/skipped** in a distinct non-friend assembly using PackageReferences only. |
| Imported suite plus new Braid probes | **460 passed, zero failed/skipped**, comprising the437 imported checks plus23 independent public probes. |
| Package provenance | Ten local archives match original exact-merge SHA-256/nuspec revision and delivery DLL hashes; nine consumed archive/cache/SHA-512/cache-DLL/output-DLL/assembly identities match exact1.21.0+8929a1301d1846bb24b49bcc505877f05fd3395a. |
| Restore and audit | Fresh isolated cache; explicit Bosak source mapping and consumed package metadata identify the merged local feed. No project references; dependency audit reports no vulnerable packages. |

Counts overlap and must not be added as unique coverage. The database-provider archive/revision/DLL hash was checked but is not consumed by the nine-assembly focused suite. Candidate feeds are **unsigned local packages**. Upstream's full9,900 solution,84 language-server,153 broad-package and frozen conformance results were read as upstream evidence, not independently rerun. Braid's previous1,157-test checkpoint is historical evidence, not a replacement integration rerun.

## Code and Consumer Findings

No required implementation correction was identified against [the consumer design review](BOSAK_REQ133_CONSUMER_DESIGN_REVIEW.md).

- The public surface is operation-specific. Opaque targets and descriptor ownership are checked by reference; Changed, NoChange and Refused expose exclusive evidence. NoChange creates no candidate, edit or new snapshot. Same logical intent cannot bypass original size/profile/encoding/eligibility/validation/Unicode/cancellation checks, and equality precedes hypothetical canonical-output sizing.
- The replacement pipeline does not borrow append-layout restrictions. Captured non-expansion, ordinary paired body, supported preserved/mixed surroundings and static/resource scope are checked before proposal success. Self-closing, CDATA, comment and PI bodies refuse discovery even where the logical intent could appear equal or empty.
- Changed emission preserves exact prefix/suffix source bytes and uses one complete body edit. Escaping retains requested controls/Unicode/markup-looking/entity-looking strings as logical data. Original/final physical bounds subtract the actual old body; imported cases exercise shrinkage and NoChange budgets.
- Survivor correspondence excludes the explicitly replaced payload even for nonempty-to-nonempty changes. Complete disjoint old/new partitions, actual zero-or-one payload references, parent/child order, names, logical outside content and full attribute metadata/ranges are proved. Empty output has no synthetic child and exposes the zero-byte body caret.
- Legacy authoring implementations and failure enum values are unchanged. The source diff adds separate public types/pipeline/capability/snapshot methods, version metadata and operation tests; imported legacy package cases pass.

## New Braid Probe Coverage

Twelve lifecycle cases cover four physical encodings and all changed empty/nonempty payload transitions. They use significant surrounding text and preserved XML whitespace, verify the exact single edit and unchanged neighbours, both complete disjoint partitions, actual edited instruction/parent/payload identity, exact literal output, defensive exports and unchanged originals. Each changed snapshot is then proposed again with equal logical text, verifying exclusive NoChange and exact original-owner/target references.

Four equality cases cover numeric references and physical CRLF spelling in every encoding, retain exact source bytes and refuse foreign equal-byte owners and pre-cancelled equal intent. Four exclusion cases check self-closing, CDATA, comment and PI bodies. Three invalid-scalar cases construct unpaired high/low surrogates and NUL at runtime to avoid runner serialization changing the intended input.

Lifecycle probes write a new ordinary file, independently reload its bytes, inspect and run them, comparing output. They use a fixed review module URI; this is not destination relocation, Braid's restricted export/worker coordinator, session/history/license or native acceptance.

## Corrections and Verification Limits

The first new-probe compile attempted the non-public FindNodeById helper and failed. The harness was corrected to traverse public descriptor children and use actual mapped IDs; its original source is retained. No runtime test ran in that failed compile, and no engine visibility/source change was made. All23 new runtime probes passed on their first executed run.

The nested upstream source-test run used command-line GenerateDocumentationFile=false, RunAnalyzers=false and TreatWarningsAsErrors=false to avoid incidental outer Braid documentation/style rules on legacy upstream tests. Normal Braid settings and the independent package-consumer enforcement remain unchanged. This is not an independent zero-warning full-source-build certification.

Receipts, public harness/probes, retained compile correction, source TRX/log, package identities, audit and independent provenance are under `.acceptance/req133-implementation-review-8929a130`. No full solution/LS/frozen-gate or153 broad-package rerun is claimed. No public signed archive or package publication was verified by consuming the candidate feed.

## Next Gate

The reviewed bounded implementation is suitable for Bosak's release workflow, subject to maintainer approval. Verify the actual published tag/packages independently before Braid changes its pin. Then implement replacement-specific consumer DTOs/validator and Changed/NoChange handling, frozen review/Apply/history/payload selection, outline/graph UI and real worker/restricted-export/reopen goldens, followed by human native review.

Existing insertion validators require positive replacement lengths or total old maps and cannot be reused wholesale. NoChange must not enter source-owner Apply/history. These remain required consumer implementation work, not blockers in the delivered engine contract.

No production pin/UI changes, upstream edits/notification, engine fixes, commit or push were performed by this review. No functional/native acceptance is granted.

## References

- [Consumer scope and acceptance groups](LITERAL_TEXT_EDITING.md)
- [Proposed ADR-019](ADR-019-reviewed-literal-text-replacement.md)
- Upstream handoff: D:/Development/Bosak/docs/REQ-133-CONSUMER-IMPLEMENTATION-HANDOFF.md
- Upstream receipt: D:/Development/Bosak/.guard-tmp/req133-implementation-delivery.json
