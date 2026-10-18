<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala REQ-133: literal text body replacement contract">
<h1>REQ-133: literal text body replacement contract</h1>
<p>Reviewed contract; implementation authorized - 18 October 2026</p>
</div>

**Publication update:** [1.21.0 is published and verified](RELEASE-1.21.0-PUBLICATION.md) after [Braid implementation review](REQ-133-CONSUMER-IMPLEMENTATION-REVIEW.md) reported no blockers. Earlier source-only, local-feed and review-pending statements below preserve their historical delivery checkpoint. Consumer adoption/native acceptance remains separate and pending.


## Status and intended outcome

**Design reviewed with no blocking findings; implementation authorized by the user on 18 October 2026.** [Braid review](REQ-133-CONSUMER-DESIGN-REVIEW.md), SHA-256 `8c333d6d349670c9e99683b0e8c4448bd3ab484399a63b2965556e3426ea9616`, covers immutable design `00b5e01e9749e6c11e3dfa8279dda76ff8a429df`; it reports seven existing-operation baseline probes and zero replacement tests. Source development targets 1.21.0. Public packages remain 1.20.0. This approval authorizes implementation and verification; publication and consumer/native acceptance remain later gates. See the [implementation record](REQ-133-IMPLEMENTATION.md).

Replace the body of one engine-approved existing paired ordinary literal `xsl:text`. A user changes a label or separator without appending another instruction. Keep opening/closing tags, attributes, prefix spelling and every byte outside that body intact. New logical text may be empty; paired tags remain. No formatting mode, self-closing expansion, generic text-node/subtree replacement, deletion, attribute/control edits or namespace changes are included.

The [original consumer request](REQ-133-literal-text-body-replacement.md) is preserved unchanged. Read the [planned acceptance matrix](REQ-133-TEXT-BODY-REPLACEMENT-ACCEPTANCE.md) and [Braid design handoff](REQ-133-CONSUMER-DESIGN-HANDOFF.md). Existing expression, value-of/text insertion/deletion and literal-element creation contracts remain independent.

## Proposed additive public API

Names below are implemented in source 1.21.0 and do not exist in released 1.20.0. Reuse immutable descriptor, source-range and source-edit types where their meanings agree; do not reuse an insertion/deletion target as replacement authority.

| Proposed surface | Contract |
|---|---|
| `AuthoringCapabilities.SupportsTextBodyReplacement` | Separate capability, advertised only after verified delivery. No inference from text insertion or expression editing. |
| `MaxTextBodyReplacementSourceBytes` | Each original and actual final complete module is bounded to 8,388,608 physical bytes, including BOM. |
| `TextBodyReplacementSupportedEncodings` | Immutable UTF-8, UTF-16LE and UTF-16BE list; UTF-8 permits BOM/no-BOM, UTF-16 requires matching BOM/declaration. |
| `AuthoringSnapshot.GetTextBodyReplacementTarget(instruction, cancellationToken)` | Exclusive typed Target/Failure result. Instruction is an actual descriptor from that snapshot; opaque `AuthoringTextBodyReplacementTarget` has no public constructor. |
| Target properties | Exact original Instruction, Parent, ModuleUri, OriginalBodyRange, OriginalTextValue and optional OriginalPayloadNode. Capture the private source/static context and owner by reference. |
| `AuthoringTextBodyReplacementProposal(target, textValue)` | Immutable exact logical Unicode string; empty legal. No source markup, formatting options or mutable settings. |
| `AuthoringSnapshot.ProposeTextBodyReplacement(proposal, cancellationToken)` | Three-way `AuthoringTextBodyReplacementResult`, as defined below. |
| `AuthoringTextBodyReplacementCandidate` | InputSnapshot, fresh Snapshot, ModuleUri, OriginalTextValue, TextValue, OriginalBodyRange, ReplacementBodyRange, EditedNode, EditedParent, OriginalPayloadNode, PayloadNode, SourceEdits, SurvivorNodeCorrespondence, ReplacedNodes, NewNodes and defensive ExportSource(). |
| `AuthoringTextBodyReplacementFailure` / `FailureKind` | Operation-specific classified failure; no renumbering or semantic changes to existing failure enums. NoChange is an outcome, not a failure. |

Null reference arguments throw ArgumentNullException. Target/proposal inputs, collections and exported bytes cannot mutate retained state. Consumers require a compatible adapter/package before accessing new members; missing-member trial calls are not capability discovery.

## Three exclusive outcomes and NoChange ordering

`AuthoringTextBodyReplacementOutcome` has Changed, NoChange and Refused. The result exposes Outcome and exactly one corresponding Candidate, NoChange or Failure value. Do not add a generic IsSuccess property that could be mistaken for a non-null candidate.

- **Changed:** Candidate is present; NoChange and Failure are absent. It contains the complete changed source and evidence, never a partial proposal.
- **NoChange:** an immutable `AuthoringTextBodyReplacementNoChange` holds the exact InputSnapshot, Target, ModuleUri and TextValue. Candidate and Failure are absent. No new snapshot, source edits, candidate export, payload identity or history step is manufactured.
- **Refused:** Failure is present; Candidate and NoChange are absent. Original source and authority are unchanged.

Validate ownership, original byte bound, supported encoding/profile, exact body eligibility and original static/resource coverage before returning NoChange. Validate replacement Unicode and observe cancellation. Compare TextValue with the target's actual decoded OriginalTextValue using StringComparison.Ordinal. No trim, case folding or Unicode normalization. NoChange cannot bypass foreign/unsupported/self-closing/invalid-original/oversized-source/cancellation refusals.

Return NoChange before canonical encoding, hypothetical escaped-output sizing, allocation or candidate inspection. Equal text may have a different possible lexical spelling; a hypothetical normalized form must not cause a size refusal or rewrite. For example, replacing the logical A in `<xsl:text>&#x41;</xsl:text>` with A preserves `&#x41;` exactly. An empty paired body replaced with empty text also returns NoChange.

Original XML parsing may normalize physical CR/CRLF to logical LF. Equality uses that inspected logical value, not the raw spelling. Explicit new CR, LF and CRLF strings remain different intents when their logical code units differ. New input `&#x41;` is literal ampersand/hash text, not an instruction to decode an entity.

NoChange does not refresh the consumer's current revision or revive stale review/validation authority. Braid still checks its current owner, source revision, capability and permission/session gates. Cancellation observed immediately before either Changed or NoChange returns Refused/Cancelled; no rollback after a committed consumer Apply is implied.

## Target eligibility and captured context

Identify `{http://www.w3.org/1999/XSL/Transform}text` by expanded name, including alternate prefixes and default-XSLT namespace syntax. A no-namespace output element named text is not eligible. The first profile covers the principal explicit 3.0/4.0 stylesheet/transform, a top-level template, and an ordinary literal-result-only constructor chain beneath that template. The instruction must have explicit opening and closing tags. Self-closing `<xsl:text/>` refuses even for empty replacement intent.

The body is either empty with zero payload descriptors, or exactly one ordinary XText descriptor whose value and lexical range cover the complete body. Refuse CDATA, comments, PIs, elements, mixed/split or ambiguous payload evidence, including apparently empty variants. Do not infer that a Kind=Text descriptor proves absence of CDATA: the engine must check its retained source/context. OriginalPayloadNode is null only for a truly empty paired source body; no zero-length synthetic child is created.

Prove effective non-expansion: absent expand-text or a captured effective false value (no/false/0 where supported). A local false override under an expanding ancestor is supported when the remaining module is inside captured validation coverage. Effective expansion, disable-output-escaping=yes and unsupported control forms refuse; never rewrite attributes to make them eligible. Invalid XSLT attributes remain invalid original source, not inferred defaults.

This operation has no layout edits. Support ordinary literal text under xml:space=preserve/default, including inherited preservation and significant surrounding parent text, when captured static validation otherwise succeeds. Preserve all xml:space metadata and every surrounding byte. Do not inherit append-layout restrictions merely because an internal helper is shared. Intentional xsl:text whitespace is payload, never an indentation gap.

Retain the bounded principal-module/resource/static coverage: dependencies, packages/schema imports, unsupported type/component annotations, static/use-when/shadow preprocessing and extension contexts refuse before evaluation. Namespace bindings and existing declarations are unchanged; there is no prefix or namespace generation. Supported existing declaration contexts require actual original/candidate validation; unsupported coverage is a classified refusal, not broader conformance or permission for Braid to parse/rewrite source. Additional conservative refusals must be documented and reviewed, without narrowing existing operations.

Targets belong by reference to one immutable snapshot, not to coincidentally equal IDs, URIs or reopened bytes. A target on an older snapshot can still propose to its own owner; Braid alone can decide whether that owner is current in the application. No engine wall-clock staleness guarantee is invented.

## Logical text, emission and exact edit footprint

Validate complete replacement intent against the supported XML character profile. Reject NUL, illegal XML characters and unpaired UTF-16 surrogates as InvalidPayload; no replacement-character fallback or new XML-version support. Do not evaluate XPath/AVT/text templates, infer entities or normalize Unicode. Expression-looking and markup-looking strings remain literal.

Changed emission uses the established literal-text encoding policy: ampersand/less-than/greater-than become `&amp;`, `&lt;`, `&gt;`; CR, LF and TAB become `&#xD;`, `&#xA;`, `&#x9;`. Other legal scalars retain their exact characters in the retained encoding. Encode both halves of a logical CRLF pair independently. Braces and quotes have no expression or attribute-escaping role here. Escaping greater-than prevents a raw `]]>` sequence. Opening/closing markup is never regenerated.

A Changed candidate has exactly one SourceEdits entry. Its OriginalRange equals Target.OriginalBodyRange; ReplacementRange equals candidate ReplacementBodyRange. Both are complete body spans in their own sources. The replacement may be zero bytes; OriginalBodyRange may be zero bytes for an originally empty paired body. These are valid single-body replacements, not general insertion/deletion authority.

Replaying this one edit reproduces ExportSource exactly. Prefix/suffix bytes match the input byte-for-byte, including instruction tags, attributes, namespace declarations, comments/PIs, outside text/whitespace, declaration, BOM and physical newline spelling. Mixed physical source newlines outside the body do not create a formatting refusal. Candidate body newlines use numeric references; NoChange retains original lexical spelling instead.

Use existing SourceRange byte/line/column conventions, including BOM bias and Unicode columns. Body range lengths are encoded source bytes, not TextValue.Length. Empty ReplacementBodyRange is the actual caret immediately after the existing opening tag and before its closing tag. EditedNode.Range provides the whole resulting instruction for review; there is no fabricated Expression slot.

## Descriptor partitions and identity

Use actual ordinary inspection of the exact emitted bytes at the same URI. EditedNode is the actual candidate counterpart of target Instruction; EditedParent is the actual counterpart of target Parent. Preserve instruction expanded name/prefix and every attribute's raw/expanded metadata. The instruction remains paired. Changed child state is exactly one real ordinary payload descriptor with Text equal to TextValue and Range equal to ReplacementBodyRange, or no child for empty text.

Define O as all original descriptors, C as all candidate descriptors, R as ReplacedNodes and N as NewNodes. R is exactly the original payload descriptor when present, otherwise empty. N is exactly the new payload descriptor when present, otherwise empty. Both lists are immutable, contain actual owned descriptors and have cardinality at most one.

SurvivorNodeCorrespondence is a total injective old-ID to candidate-ID map on O minus R, scoped to InputSnapshot and Snapshot. Its image is exactly C minus N. Require both disjoint partitions O = survivor keys plus R and C = survivor image plus N. OriginalPayloadNode and PayloadNode agree with R and N respectively. Integer IDs alone cannot prove cross-snapshot identity or membership.

The old payload is explicitly replaced even in nonempty-to-nonempty changes; do not map it as an unchanged survivor merely because its ID/kind/range happens to resemble the new payload. Empty-to-nonempty has R empty/N one; nonempty-to-empty has R one/N empty; nonempty-to-nonempty has R one/N one. Empty-to-empty is NoChange and has no candidate partition.

Preserve kinds, expanded names, raw/expanded attribute values, surviving logical content and parent/order relationships outside the payload replacement. Translate source ranges through the single body delta; owner/ancestor extents may shrink or grow without rewriting their markup. All other parent child lists are unchanged after filtering out the declared payload pair. Namespace/attribute descriptors remain metadata under current rules, not new node IDs.

Any extra/missing/split/merged descriptor outside R/N, duplicate map entry, foreign descriptor, wrong parent/name/value/range or unproved metadata change refuses SourceAlignmentFailure. Do not import REQ-130 merged-text provenance, synthesize a graph or guess the new payload from largest IDs/labels. Source identities do not assert separate result-tree text identities: transformations can merge/drop result text under existing semantics.

## Physical bounds, validation and cancellation

Each original and actual final source is at most 8,388,608 bytes including BOM. For Changed, finalBytes = originalBytes - originalBodyBytes + encodedReplacementBodyBytes. Use overflow-safe arithmetic and count escaped/reference expansion before allocation. A shrinking replacement may fit when a hypothetical append would exceed the limit. Original sources over the bound still refuse, even if a replacement could shrink them. NoChange checks original size and avoids hypothetical canonical-output sizing.

Keep strict UTF-8 with/without BOM and BOM-marked UTF-16 LE/BE with matching declaration, as for existing bounded structural authoring. No encoding conversion, declaration rewrite, lossy fallback or unrelated newline normalization. Inspection may support other encodings that this replacement operation refuses.

Validate original and Changed candidate under the captured supported static context with deny-by-default resource authority. No transformation, extension execution or network/file acquisition occurs during proposal. The new payload never goes through an XPath parser. Any attempted denied acquisition is ResourceDenied; there is no fallback to ambient authority.

Observe cooperative cancellation during target/source traversal, scalar validation/counting, emission, correspondence proof, before/after inspection/validation and immediately before either successful outcome. No preemptive parser deadline is promised. Proposals against a shared immutable snapshot remain isolated under concurrency. Refused, cancelled and NoChange outcomes never publish a partial candidate or mutate the input.

Proposed FailureKind members cover CapabilityUnavailable, ForeignSnapshot, UnknownTarget, UnsupportedModule, UnsupportedProfile, UnsupportedCoverage, UnsupportedEncoding, SizeLimit, IneligibleInstruction, SelfClosingInstruction, InvalidOriginal, InvalidPayload, InvalidCandidate, ResourceDenied, SourceAlignmentFailure and Cancelled. Wrong node identity is IneligibleInstruction; self-closing syntax is SelfClosingInstruction; excluded body/control forms are UnsupportedCoverage. Existing failure enum values/behavior are untouched. A single-cause case must have stable classification; multiple-invalid-input precedence is not a universal ordering guarantee.

## Consumer workflow and delivery gates

Braid offers separate check/edit-text actions only after compatible public delivery. Show old logical text separately from old lexical bytes. Freeze the full changed candidate, exact single edit, whole instruction, complete partitions and source/engine/revision provenance; refuse an oversized complete footprint without truncation. Changes of intent/source invalidate review. NoChange produces feedback only, preserving bytes, revision, history and current selection/authority as appropriate.

Changed Apply is separate, explicit and atomic under current capability/license/source/inspection/revision/cancellation/overlap checks. Select actual EditedNode plus PayloadNode.Range or the empty ReplacementBodyRange caret. Reopen graph selection against the fresh snapshot. Undo/redo restores exact bytes under fresh revisions and inspection; old validation/preview/export receipts stay historical. Cancellation after committed Apply reports the committed result. No Braid XML parser, source splice, friend access or reflection bypass grants edit authority.

Independent Braid design review and separate user implementation authorization are complete. The additive feature increments source version from 1.20.0 to 1.21.0. Gates include full solution and separate language-server tests, unchanged legacy authoring behavior, exact-head CI, exact-merge independent public-API package verification, frozen QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**. Public signed-package verification and Braid UI/worker/export/reopen/native acceptance remain separate later gates.

## Evidence used for this design

Current code audit covers the operation-specific insertion/deletion targets, descriptor metadata, literal emission, layout and correspondence helpers. These are constraints and reusable primitives, not replacement authority. Seven read-only public 1.20.0 baseline probes confirm existing inspection/runtime behavior for empty/reference/numeric-newline/physical-newline/preserved/mixed-parent/local-expansion-override text. Their exact assembly revision is checked; `.guard-tmp/req133-baseline-probes.json` records results. They execute no replacement operation and establish no new capability or acceptance.

The semantic references are [XSLT 3.0 xsl:text](https://www.w3.org/TR/xslt-30/#element-text), [text value templates](https://www.w3.org/TR/xslt-30/#text-value-templates) and [XML end-of-line handling](https://www.w3.org/TR/REC-xml/#sec-line-ends). The XSLT source was read from the retained primary W3C document after the web reader's size limit. Context/mapping/API rules above are Bosak's proposed engineering contract, not wider standards-conformance claims.
