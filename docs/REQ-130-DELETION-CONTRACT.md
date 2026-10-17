<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala value-of deletion contract">
<h1>REQ-130: value-of deletion contract</h1>
<p>Proposed bounded authoring operation — 17 October 2026</p>
</div>

## Status and review decision

**Pending: contract and acceptance plan proposed; owner Unassigned, target TBD.** The user requested continuation with REQ-130 after acceptance of REQ-129. This step designs deletion; it does not implement an API, advertise a capability, reserve a release, or change Braid. The [original consumer submission](REQ-130-source-preserving-value-of-deletion.md) is retained byte-for-byte. Baseline main is `731c94431a954a86cf771a6dbffee37eca92fa5a`; released libraries remain 1.17.0 at `15eacde48b47cc8e48af29ddc95949801a2e3e5d`.

**Review required before implementation:** instruction-only removal, ordinary fresh inspection and total injective surviving-node correspondence cannot all succeed when deletion coalesces two adjacent source text descriptors. Recommend preserving these guarantees and refusing such targets as `UnsupportedCorrespondence` in the first slice. This excludes many normally indented first/middle/last/sole deletions, including readable-inserted instructions with whitespace on both sides. This is a material limitation requiring consumer agreement, not fulfillment of all requested happy paths. Broader coverage would require a separately agreed correspondence model (for example explicit many-to-one text provenance) and consumer adapter changes. Do not quietly remove whitespace, classify surviving whitespace as removed, split the ordinary inspection artificially, or weaken injectivity.

## Proposed additive public surface

All names below are proposed and absent from 1.17.0. Keep insertion and expression replacement types and defaults intact.

| Surface | Contract |
|---|---|
| `AuthoringCapabilities.SupportsValueOfDeletion` | Independent flag enabled only after complete delivery gates for the documented bounded profile. |
| `MaxValueOfDeletionSourceBytes` | Original and complete candidate each at most 8,388,608 bytes, including BOM. |
| `AuthoringSnapshot.GetValueOfDeletionTarget(node, cancellationToken)` | Result has exactly one opaque `AuthoringValueOfDeletionTarget` or classified failure. Node must be the exact descriptor owned by this snapshot. No public target constructor. |
| `AuthoringValueOfDeletionProposal(target)` | Immutable engine-issued target only; no replacement string, caller range, whitespace cleanup, formatting mode or raw XML. Null target throws ArgumentNullException. |
| `AuthoringSnapshot.ProposeValueOfDeletion(proposal, cancellationToken)` | Exactly one complete `AuthoringValueOfDeletionCandidate` or classified failure; no partial candidate. Null proposal throws ArgumentNullException. |
| Candidate evidence | InputSnapshot, fresh Snapshot, ModuleUri, RemovedNode, RemovedNodes, SurvivingParent, OriginalRange, CandidateRange, SourceEdits, NodeCorrespondence, defensive ExportSource(). |
| Separate `AuthoringDeletionFailureKind` / failure record | ForeignTarget, IneligibleNode, UnsupportedModule, UnsupportedProfile, UnsupportedCoverage, MixedContent, UnsupportedEncoding, SizeLimit, InvalidOriginal, InvalidCandidate, SourceAlignmentFailure, UnsupportedCorrespondence. Message, optional diagnostic code/range and module URI are data; consumers do not parse messages. |

Result carriers follow the existing mutually exclusive result pattern. Use a separate deletion enum rather than inserting values into existing failure enums. Cancellation throws OperationCanceledException with no candidate; programmer-null arguments throw, while well-formed but ineligible source produces classified refusal. Missing deletion members in old packages require a compatible adapter/version, not speculative member calls. Snapshot ownership is reference/instance authority, never equality of IDs, labels, bytes or URIs. An old immutable snapshot remains usable on its own; only Braid knows whether it is still the current session revision.

## Eligibility and validation order

One instruction element in the principal module of a single-module explicit XSLT 3.0 or 4.0 stylesheet/transform, beneath a top-level template through a literal-result-only parent chain. Use the existing restricted static-validation and expression-coverage boundaries deliberately; do not equate compilability with complete authoring coverage. Keep captured expression profiles and namespace/static context unchanged. The candidate must pass the same independent whole-module restricted validation as the original.

Initially accept an expanded XSLT-name `value-of` with a valid supported select slot and the existing validated supported attributes. Self-closing spelling and explicitly closed spelling with an empty body are eligible. Whitespace, text, CDATA, comment, PI or element children inside the selected value-of are outside this first slice; refuse rather than deleting an unreviewed subtree. Namespace declarations/attributes on the instruction are part of its removed bytes. They are not separate node descriptors; the removed element owns their evidence. Bindings on surviving ancestors remain unchanged, even if unused afterwards. A same-local-name literal result element is ineligible. Prefix aliases/default XSLT bindings use expanded names, not lexical labels.

Apply the existing conservative principal-module coverage exclusions: dependencies/include/import/use-package, schema/type/component references, static preprocessing/use-when/shadow attributes, unsupported fixed/type namespaces or extension constructs, unsupported effective versions, unsupported expression slots and existing text-value-template coverage. Refuse significant mixed text/CDATA in the containing constructor and any ancestor xml:space=preserve, including a nearer default reset, for this initial bounded slice. This is an explicit restriction, not a claim that exact-byte deletion intrinsically needs formatting. Ordinary comments/PIs and literal whitespace elsewhere remain source-backed evidence. Do not expand the instruction allow-list to implement deletion; accepted original instructions already covered by existing validation retain their behavior.

Order: validate arguments/cancellation and descriptor ownership; enforce original byte limit/encoding/single-module/profile and static-execution exclusions before any compilation; align exact target extent and prove the proposed survivor shape; validate original; emit privately; inspect candidate without implicit compilation or dependency fetch; validate candidate under the same restricted boundaries; prove all evidence; publish success. Recheck cancellation during scans, mapping, validation and before returning. Discovery performs enough of these checks to refuse known impossible targets; proposal repeats authoritative checks, never trusts cached caller evidence. Unexpected implementation defects must not be hidden as user syntax errors.

## Exact byte removal and coordinates

Let original bytes be B and selected full instruction range [a,b). Emit C = B[0:a] + B[b:length(B)], with 0 <= a < b <= length(B). Require the range to align to the retained XML scanner extent and encoding boundaries. Never find an instruction by a string match for a closing tag: quoted attributes, comments, sibling repetitions and namespace spellings require structural alignment. Remove the entire selected start/end or self-closing tag, and nothing before or after it.

Exactly one immutable source edit has the complete original range and empty replacement bytes/text. CandidateRange is a zero-length range at offset a in the candidate coordinate map; OriginalRange belongs to the original map. Report byte, decoded UTF-16 and line/column coordinates from each owning map, including normalized-line-ending rules; do not subtract byte lengths from character positions. NodeCorrespondence uses candidate ranges after the removed span shifts. Replay must reconstruct the exported candidate bytes exactly and all unchanged intervals must match. A retained ancestor/document range spans the smaller candidate extent although its node kind/name and unaffected content remain proven.

Preserve URI, BOM, encoding declaration, prefix spellings, surrounding indentation/blank lines, comments, PIs, namespace declarations, Unicode and physical newline bytes. A now-empty `<out></out>` stays explicitly closed. No formatter or serializer, no namespace cleanup, no parent self-closing rewrite and no synthesized padding. UTF-8 with/without BOM and BOM-marked UTF-16 LE/BE use existing strict matching-declaration rules; refuse unsupported encodings/invalid byte sequences rather than transcode. Exported arrays and source edit collections cannot mutate either snapshot.

## Removed and surviving identity evidence

RemovedNodes contains exactly the selected instruction descriptor in this empty-body first slice; RemovedNode is that same original descriptor. Removed IDs are scoped to InputSnapshot, not globally reserved numbers. A fresh candidate may independently allocate an equal numeric ID; that does not resurrect a removed identity. Never select by numeric ID across snapshots.

Let O be every original principal descriptor, including document, ancestors, text, comments and PIs; R the removed set; M the survivor map. R and domain(M) are disjoint and their union is O. M is total on survivors and injective into actual candidate descriptors. Its image is every candidate descriptor: deletion introduces no extra nodes. Prove kinds, expanded/preserved names, attributes, text values, translated exact source extents and parent/child ordering, accounting for the removed child and changed ancestor spans. Use actual original/candidate descriptor references and validated source translation, not traversal ordinal alone. Repeated identical siblings must remain distinct. No partial correspondence can be published as success.

The selected instruction's direct containing template/literal-result parent survives in every eligible case; SurvivingParent is its mapped candidate descriptor and must be in the candidate snapshot. Empty parents remain selectable elements. There is no eligible document-root deletion or no-parent success in this slice. If parent correspondence cannot be proven, refuse rather than choose a sibling. Braid clears selection if its adapter loses/does not support this evidence; it must not infer selection from proximity.

### Text-coalescence boundary

The current AuthoringInspector loads with PreserveWhitespace and builds one descriptor per XText (AuthoringInspector.cs, LoadOptions and BuildText). Consider:

```xml
<out>
  <xsl:value-of select="1"/>
</out>
```

The example contains physical newline characters. Before removal, the constructor has text / value-of / text descriptors. Afterwards ordinary inspection has one text descriptor whose value is both surviving whitespace runs concatenated. Both original text ranges survive byte-for-byte, so neither can enter RemovedNodes; mapping both to one candidate breaks injectivity. Disjoint source fragments are not distinct candidate node identities. The failure is UnsupportedCorrespondence, not invalid XSLT and not permission to trim a blank line.

Compact `<out><xsl:value-of select="1"/></out>` has no neighboring text and can succeed. One-sided whitespace, a surviving element, comment or PI separating the whitespace runs can also succeed where ordinary inspection and full evidence prove one-to-one survival. Do not promise these cases solely from lexical adjacency; verify the complete graph. A previously inserted compact PreserveSource instruction can succeed; a readable insertion between two whitespace runs must refuse under this proposed first-slice policy. Do not claim support for every inserted candidate.

## Execution, resources and consumer lifecycle

Deleting output-producing value-of intentionally changes results. Validate original and candidate statically, then use separately specified changed-output goldens for controlled execution tests. No output-equivalence requirement and no execution as part of the proposal. Compilation/preprocessing must not evaluate user code or resolve external resources; refuse unsupported static evaluation before invoking it. Retain current resolver denial, DTD/entity restrictions, limits and no filesystem/network/process side effects. Byte bound checks precede whole-source allocation; decoded buffers/maps remain bounded and cancellation cooperative. A smaller candidate does not excuse an oversized original.

Braid discovers compatible capability and an engine-issued target; captures exact current revision, node and permissions; asks for a proposal; freezes complete bytes/edits/removals/survivors/parent evidence in a bounded review; independently checks replay and authority; and applies only an explicit session-issued reviewed deletion. Stale, cancelled, denied, overlapping or incomplete evidence cannot Apply. Oversized review refuses, never truncates. One Apply adds one history step; undo/redo restores exact bytes under new revisions and invalidates old inspection/validation/preview/export authority. Export goes to a new file with provenance/hash; independent ordinary-file reopen/run proves the changed-output golden. No Braid command/pin change is included in this design.

## Delivery and review gates

The [acceptance matrix](REQ-130-DELETION-ACCEPTANCE.md) is planned evidence only. Before an implementation PR, confirm with the consumer whether coalescence refusal is useful enough for the first delivery; otherwise agree a broader identity contract first. Review remaining proposed API/eligibility/refusal names and exact scope. Any implementation needs a minor version bump from then-current source, XML API documentation, preserved file headers, canonical docs sync, full solution/language-server tests, unchanged frozen QT3 31,142/0/679 and qt4 gated 8,278/0/1,894 baselines, and exact-head independent package consumer proof. Do not run conformance gates merely for this documentation-only step. Engine integration, release/publication and Braid adoption/native acceptance are separate decisions. REQ-131 remains a pending independent design.
