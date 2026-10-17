<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala value-of deletion contract">
<h1>REQ-130: value-of deletion contract</h1>
<p>Proposed bounded authoring operation — 17 October 2026</p>
</div>

## Status and review decision

**Pending: contract and acceptance plan proposed; owner Unassigned, target TBD.** The user requested continuation with REQ-130 after acceptance of REQ-129. This step designs deletion; it does not implement an API, advertise a capability, reserve a release, or change Braid. The [original consumer submission](REQ-130-source-preserving-value-of-deletion.md) is retained byte-for-byte. Baseline main is `731c94431a954a86cf771a6dbffee37eca92fa5a`; released libraries remain 1.17.0 at `15eacde48b47cc8e48af29ddc95949801a2e3e5d`.

**Product-owner direction — 17 October 2026:** Design explicit merged-text provenance before implementation. Ordinary indented/readable deletion must not be refused merely because two surviving whitespace nodes become one ordinary candidate text node. The original consumer submission requested total injective survivor mapping; this proposal revises that requirement explicitly for a narrowly defined text merge. Preserve the original submission unchanged and adapt/review the consumer against this revised contract. This decision selects the design direction, not final API acceptance, implementation authorization or Braid adoption.

Keep one-to-one correspondence for all other survivors; represent merged text through a separate immutable relation with exact retained-source fragments. Never remove surrounding whitespace, misclassify it as deleted, invent split candidate descriptors or infer identities from equal text. UnsupportedCorrespondence remains a refusal for evidence that cannot be proven, rather than for an otherwise supported whitespace merge.

## Proposed additive public surface

All names below are proposed and absent from 1.17.0. Keep insertion and expression replacement types and defaults intact.

| Surface | Contract |
|---|---|
| `AuthoringCapabilities.SupportsValueOfDeletion` | Independent flag enabled only after complete delivery gates for the documented bounded profile. |
| `MaxValueOfDeletionSourceBytes` | Original and complete candidate each at most 8,388,608 bytes, including BOM. |
| `AuthoringSnapshot.GetValueOfDeletionTarget(node, cancellationToken)` | Result has exactly one opaque `AuthoringValueOfDeletionTarget` or classified failure. Node must be the exact descriptor owned by this snapshot. No public target constructor. |
| `AuthoringValueOfDeletionProposal(target)` | Immutable engine-issued target only; no replacement string, caller range, whitespace cleanup, formatting mode or raw XML. Null target throws ArgumentNullException. |
| `AuthoringSnapshot.ProposeValueOfDeletion(proposal, cancellationToken)` | Exactly one complete `AuthoringValueOfDeletionCandidate` or classified failure; no partial candidate. Null proposal throws ArgumentNullException. |
| Candidate evidence | InputSnapshot, fresh Snapshot, ModuleUri, RemovedNode, RemovedNodes, SurvivingParent, OriginalRange, CandidateRange, SourceEdits, NodeCorrespondence (one-to-one survivors only), TextMergeGroups, defensive ExportSource(). |
| Separate `AuthoringDeletionFailureKind` / failure record | ForeignTarget, IneligibleNode, UnsupportedModule, UnsupportedProfile, UnsupportedCoverage, MixedContent, UnsupportedEncoding, SizeLimit, InvalidOriginal, InvalidCandidate, SourceAlignmentFailure, UnsupportedCorrespondence. Message, optional diagnostic code/range and module URI are data; consumers do not parse messages. |

Proposed `AuthoringTextMergeGroup` contains the actual CandidateNode and an immutable ordered Fragments collection. Each `AuthoringTextMergeFragment` contains its actual OriginalNode, full OriginalRange and exact CandidateRange for that node's surviving lexical bytes. These source ranges are owned by their respective snapshots; candidate fragment ranges are not new node descriptors. TextMergeGroups is empty when no merge occurs. A compatible deletion consumer must support and verify both evidence collections before Apply; a plain survivor dictionary is insufficient.

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

Let O be every original principal descriptor, including document, ancestors, text, comments and PIs; R the removed set; M the one-to-one survivor map; G the merge groups. R, domain(M), and all original fragment nodes in G form a disjoint complete partition of O. Each original survivor appears exactly once in M or one merge group. M is injective; its image and the candidate nodes of G form a disjoint complete partition of the actual candidate graph. Different merge groups cannot share original or candidate descriptors. Deletion introduces no unexplained candidate nodes. No survivor in a merge group appears in M or R.

For M prove kinds, expanded/preserved names, attributes, text values, translated exact source extents and parent/child ordering, accounting for the removed child and changed ancestor spans. For G prove the fragment rules below. Parent/child order is checked over the combined relation: replace the removed child with nothing and the two participating text siblings with the one proven candidate child. Use actual snapshot-owned descriptor references and exact source translation, not traversal ordinal or equal text. Repeated identical siblings remain distinct. No partial relation can be published as success. A merged candidate is a fresh text node with provenance from two original nodes, not continued identity of either one.

The selected instruction's direct containing template/literal-result parent survives in every eligible case; SurvivingParent is its mapped candidate descriptor and must be in the candidate snapshot. Empty parents remain selectable elements. There is no eligible document-root deletion or no-parent success in this slice. If parent correspondence cannot be proven, refuse rather than choose a sibling. Braid clears selection if its adapter loses/does not support this evidence; it must not infer selection from proximity.

### Bounded merged-text provenance

The current AuthoringInspector loads with PreserveWhitespace and builds one descriptor per XText (AuthoringInspector.cs, LoadOptions and BuildText). For example:

```xml
<out>
  <xsl:value-of select="1"/>
</out>
```

The constructor has text / value-of / text descriptors. After instruction-only removal, ordinary inspection has one whitespace text descriptor. Both original text ranges survive byte-for-byte; neither is removed. This is a supported success case when all evidence below is proven. The retained blank indentation line is intentional. The original LINQ-to-XML shape probe establishes this shape issue only; it is not a public deletion implementation test.

In this single empty-body instruction deletion slice there can be at most one merge group, with exactly two nonempty ordinary whitespace text siblings immediately before and after the removed instruction, under its surviving parent. Their concatenated retained lexical bytes form the entire range of exactly one ordinary candidate text child at that position. CDATA, significant mixed text, unrelated/split/reordered nodes, larger merge groups and merges in other parents remain outside this scope. One-sided whitespace and constructors separated by comments/PIs/elements use M when ordinary inspection proves no merge. Compact and readable-inserted sources receive the same byte-removal rule; readable insertion does not imply whitespace cleanup.

For each merge group prove all of the following before success:

1. Both OriginalNodes belong to InputSnapshot, are distinct, have full nonempty nonoverlapping ranges, and are the exact adjacent text siblings of RemovedNode. Their only intervening lexical bytes are OriginalRange of the removed instruction. The parent maps through M to the candidate node's exact parent.
2. Fragment OriginalRanges equal the complete retained descriptor ranges. Translate each around [a,b) into its candidate interval. CandidateRanges are ordered, nonoverlapping, contiguous and cover the entire CandidateNode.Range exactly once, with no gaps or padding. Every fragment byte equals the corresponding original byte. All ranges align to encoding boundaries and each owning coordinate map.
3. The candidate is the actual fresh ordinary inspection descriptor, with the exact expected kind, child position and source range. Inspection of the complete retained lexical run determines its parsed Text value. Do not create extra descriptors or use fragment ranges as candidate node identities.
4. XML line-end normalization must be checked on the joined lexical run. In particular an original left run ending in CR and right run beginning in LF becomes a physical CRLF after removal, which parses as one LF. Therefore candidate.Text need not equal concatenation of the two original parsed Text values. Byte preservation is authoritative; prove the actual candidate parsed value under existing XML normalization, without normalizing/replacing source bytes or claiming unchanged decoded whitespace semantics. Whole-module candidate validation and changed-output goldens still apply.
5. The complete original/candidate graph satisfies the partition and ordering rules above. Missing/duplicate/foreign/out-of-order/overlapping/partial or ambiguous evidence yields UnsupportedCorrespondence and no candidate. Bound evidence size and cancellation; never truncate it.

Only this explicitly classified text merge relaxes injectivity. All elements (including surviving parent), document, comments, PIs and unaffected text retain one-to-one mapping. Consumers must not flatten TextMergeGroups into a dictionary by taking the first/last original node, or generalize it to arbitrary many-to-one structural edits. If a consumer tracks selection on a participating text node, the merge group proves its resulting candidate node but not an unambiguous caret position; selection policy may clear it. After this deletion, the default element selection is the proven SurvivingParent, never a nearby sibling or one arbitrarily chosen text fragment.

## Execution, resources and consumer lifecycle

Deleting output-producing value-of intentionally changes results. Validate original and candidate statically, then use separately specified changed-output goldens for controlled execution tests. No output-equivalence requirement and no execution as part of the proposal. Compilation/preprocessing must not evaluate user code or resolve external resources; refuse unsupported static evaluation before invoking it. Retain current resolver denial, DTD/entity restrictions, limits and no filesystem/network/process side effects. Byte bound checks precede whole-source allocation; decoded buffers/maps remain bounded and cancellation cooperative. A smaller candidate does not excuse an oversized original.

Braid discovers compatible capability and an engine-issued target; captures exact current revision, node and permissions; asks for a proposal; freezes complete bytes/edits/removals/one-to-one survivors/text-merge fragments/parent evidence in a bounded review; independently checks replay, complete disjoint graph partitions, fragment coverage and authority; and applies only an explicit session-issued reviewed deletion. Stale, cancelled, denied, overlapping or incomplete evidence cannot Apply. Oversized review refuses, never truncates. One Apply adds one history step; undo/redo restores exact bytes under new revisions and invalidates old inspection/validation/preview/export authority. Export goes to a new file with provenance/hash; independent ordinary-file reopen/run proves the changed-output golden. No Braid command/pin change is included in this design.

## Delivery and review gates

The [acceptance matrix](REQ-130-DELETION-ACCEPTANCE.md) is planned evidence only. The product owner selected explicit merged-text provenance; before an implementation PR, review the concrete evidence shape and normalization boundary with the consumer. Do not claim the original injective-only request was fulfilled unchanged. Review remaining proposed API/eligibility/refusal names and exact scope. Any implementation needs a minor version bump from then-current source, XML API documentation, preserved file headers, canonical docs sync, full solution/language-server tests, unchanged frozen QT3 31,142/0/679 and qt4 gated 8,278/0/1,894 baselines, and exact-head independent package consumer proof. Do not run conformance gates merely for this documentation-only step. Engine integration, release/publication and Braid adoption/native acceptance are separate decisions. REQ-131 remains a pending independent design.
