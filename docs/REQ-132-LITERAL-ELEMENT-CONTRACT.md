<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak REQ-132: literal result element insertion contract">
<h1>REQ-132: literal result element insertion contract</h1>
<p>Proposed design - 18 October 2026</p>
</div>

**Publication update:** [1.20.0 is published and verified](RELEASE-1.20.0-PUBLICATION.md) after [Braid implementation review](REQ-132-CONSUMER-IMPLEMENTATION-REVIEW.md) reported no blockers. Earlier unpublished/review-pending statements below preserve their source-delivery checkpoint; consumer adoption/native acceptance remains separate.


**Implementation update:** [Source 1.20.0](REQ-132-IMPLEMENTATION.md) passes source verification after the unchanged [no-blocking design review](REQ-132-CONSUMER-DESIGN-REVIEW.md). Proposal-only status below preserves the reviewed contract history; publication and consumer acceptance are separate.


## Status and intended outcome

**Pending; contract proposed for Braid review. Target version TBD; implementation owner unassigned.** The user authorized this design slice, including committing the existing consumer request. Engine implementation, public capability, version reservation and publication are separate decisions. Baseline: published 1.19.1 at `a9e954d75b3518340ce6da84c9ae06c50ab1f5a4`; documentation main `cd5507afa2e2bfc98f20669b2941acb5b938a121`. Existing expression replacement, value-of insertion/deletion and literal-text insertion remain the delivered capabilities.

Append one engine-created paired empty literal result element to an eligible principal template or literal-result constructor. Then discover a fresh text/value-of target on the actual new element to populate it in a separate proposal/history step. The [unchanged consumer request](REQ-132-literal-result-element-insertion.md) supplies the intended outcome and LE-01 through LE-12 groups. [Acceptance cases](REQ-132-LITERAL-ELEMENT-ACCEPTANCE.md) below are plans, not test results.

First profile: an exact unprefixed local name, no namespace, no user-supplied attributes, no content. Exclude namespaced names, raw XML, arbitrary positions, self-closing parent expansion, moving/wrapping existing nodes, attribute editing, replacement, deletion and namespace cleanup. The engine can refuse unsupported static contexts; it must never silently widen the operation.

## Proposed additive public surface

Names are proposals, absent from 1.19.1. Reuse existing immutable source/edit/range types, not existing operation-specific targets. Existing enum values, overloads and default behavior remain stable.

| Proposed member/type | Contract |
|---|---|
| `AuthoringCapabilities.SupportsLiteralElementInsertion` | Independent capability advertised only with verified delivery. |
| `SupportsReadableLiteralElementInsertion` | Independent optional layout capability; no inference from text/value-of capabilities. |
| `MaxLiteralElementInsertionSourceBytes` | 8,388,608 physical bytes for each original and emitted complete module, including BOM. |
| `LiteralElementInsertionSupportedEncodings` | UTF-8 with/without BOM; UTF-16 LE/BE with matching BOM/declaration, as existing structural operations. |
| `AuthoringSnapshot.GetLiteralElementInsertionTarget(parent, cancellationToken)` | Typed result: opaque snapshot-owned `AuthoringLiteralElementInsertionTarget` or classified failure. No public target constructor or numeric-ID reconstruction. |
| `AuthoringLiteralElementInsertionTarget` | Exact original Parent and ModuleUri; retained private source/static context bound by reference to its owning snapshot. |
| `AuthoringLiteralElementInsertionProposal(target, localName)` | Immutable exact local name; PreserveSource default. Explicit formatting-mode overload uses existing `AuthoringInsertionFormattingMode`. |
| `AuthoringSnapshot.ProposeLiteralElementInsertion(proposal, cancellationToken)` | Typed result with exactly one complete isolated candidate or failure. |
| `AuthoringLiteralElementInsertionCandidate` | InputSnapshot, fresh Snapshot, ModuleUri, FormattingMode, LocalName, ElementName, OriginalRange, InsertedRange, BodyRange, InsertedNode, InsertedParent, SourceEdits, NodeCorrespondence, NewNodes and defensive ExportSource(). |
| Existing insertion failure enum plus appended `InvalidElementName` | Name syntax/intent error, distinct from InvalidExpression/InvalidPayload. Append without renumbering. Namespace/static hazards use UnsupportedCoverage; no new generic source-edit authority. |

Null reference arguments throw ArgumentNullException; undefined formatting enum values throw ArgumentOutOfRangeException. Empty, malformed or unsupported name intent is a classified InvalidElementName refusal, not constructor trimming or implicit repair. Target and proposal result classes use the existing exclusive Target/Failure and Candidate/Failure patterns. Collections, byte exports and descriptors must not expose mutable backing storage. Consumers need a compatible adapter/package; trial calls to missing members are not capability discovery.

## Name and namespace semantics

Validate the complete supplied string as an NCName supported by the existing XML source/parser profile: no colon, Clark/EQName syntax, markup, leading/trailing whitespace, leading digits, NUL or unpaired surrogate. Do not trim, normalize Unicode, change case, escape invalid names into different names, or parse names in Braid. Legal Unicode names retain exact spelling. A spelling such as `value-of`, `text` or `template` is literal output because classification uses expanded identity, not the local-name label. XML/namespace reserved-prefix rules do not justify a blanket ban on otherwise legal unprefixed local names; test `xml`, `xmlns` and xml-prefixed local spellings against the supported parser and classify any actual unsupported context explicitly.

The inserted source and runtime element expanded name must both be `{}` plus LocalName. With no effective default namespace, emit `<item></item>`. With a nonempty inherited default namespace, emit `<item xmlns=""></item>`. The generated local reset is namespace metadata, not a user-requested ordinary attribute. It affects only the new subtree; do not rewrite ancestor bindings, prefixes, xpath-default-namespace or unrelated declarations. A default XSLT namespace also requires the reset; ordinary lexical `xsl`/alternate-prefix choices do not change this rule.

Paired tags are mandatory in source even when the serializer later outputs a self-closing result. Emit exactly the accepted name in both tags, with no internal whitespace or payload. The body caret is immediately after the opening tag and immediately before the closing tag. Readable may format surrounding gaps only; it must not put indentation in the empty body.

Namespace copying may produce result namespace nodes under existing XSLT semantics; no-namespace element identity does not promise a result with no namespace declarations. No user attributes or attribute sets are added. Refuse any stylesheet containing `xsl:namespace-alias` in the first profile, including #default mappings, before candidate generation: source-name inspection alone cannot prove the intended result name under aliasing. Preserve the existing refusal of extension-element-prefixes and unsupported static preprocessing. Do not declare prefixes or add XSLT control attributes merely to evade those refusals.

These semantics follow [XSLT 3.0 section 11.1](https://www.w3.org/TR/xslt-30/#literal-result-element) and [Namespaces in XML NCName/defaulting rules](https://www.w3.org/TR/REC-xml-names/#defaulting). They define this bounded proposal, not wider XSLT conformance.

## Target and captured-context boundary

Principal-only single module, explicit XSLT 3.0/4.0 stylesheet/transform, top-level template or literal-result-only ancestor chain below it, and explicit parent end tag. Retain existing supported captured-context validation and deny-by-default resources. Dependencies/packages/schema imports, unsupported type/component annotations, static/use-when/shadow processing and denied resource authority refuse. Inspect the full principal document before compilation where preprocessing could execute caller code. No transformation, extension invocation, network fetch or file access occurs in proposal generation.

Refuse direct significant parent text, direct CDATA and effective xml:space=preserve. Readable additionally retains the existing any-ancestor preserve refusal, even under a nearer default reset. Significant text inside an existing literal xsl:text is protected content, not direct-parent layout whitespace. For the new operation, adopt REQ-131's non-expanding ordinary xsl:text profile without disable-output-escaping or body markup; legacy value-of operations keep their broader measured behavior unchanged. Existing literal text payload bytes are never reformatted.

A target is valid only for its immutable owner; an equivalent descriptor/target from a separately inspected identical source is ForeignSnapshot. The engine does not know Braid's latest revision: an older snapshot can still propose against itself. Braid classifies stale current-session authority and rejects Apply; do not invent an engine wall-clock staleness guarantee.

Candidate inspection must support the new literal element as a constructor under the same bounded profile. On the fresh snapshot, both GetTextInsertionTarget and GetValueOfInsertionTarget for InsertedNode must succeed for the accepted composition fixtures, including the local namespace-reset case. Discover each target afresh after every Apply/undo/redo. Do not change existing capabilities or silently reject previously accepted value-of/text operations merely to reuse internal helpers.

## Exact source edits and ranges

PreserveSource emits a single append insertion at the proven parent closing-tag anchor, including any required local namespace reset inside the new element. Every pre-existing byte is retained. Readable reuses the established bounded direct-child gap policy: uniform newline/indent inference, compact two-space fallback, conservative ambiguity refusal, and complete disclosure of whitespace edits. Mixed/ambiguous layouts refuse UnsupportedFormatting without implicit downgrade. Existing comments, PIs, CDATA elsewhere, payloads, declarations and descendants stay byte-identical outside disclosed ranges.

SourceEdits uses ordered, nonoverlapping existing AuthoringInsertionSourceEdit records with original-coordinate OriginalRange and candidate-coordinate ReplacementRange; coalesce coincident positions. Replaying all edits must reproduce ExportSource exactly. OriginalRange on the candidate is only the zero-length original append anchor; it is not the complete Readable footprint. InsertedRange covers the paired element markup and generated declaration, excluding surrounding indentation. BodyRange is a zero-byte candidate range between its tags; it is not an expression slot or text descriptor.

Use existing SourceRange byte/line/column and Unicode conventions, including BOM bias. UTF-16 lengths are physical bytes; never derive byte offsets from string.Length. Count both opening and closing occurrences of LocalName, any namespace-reset spelling and all layout bytes using overflow-safe arithmetic before large allocations. Limit original and final module independently; account for replaced Readable gap bytes so a valid shrink-to-fit candidate is not rejected based on a hypothetical pure append. No source encoding/declaration upgrade, replacement fallback or newline normalization is permitted.

## Complete candidate and descriptor evidence

The candidate is a fresh ordinary inspection of the exact emitted bytes at the same principal URI. InsertedNode is its actual literal-element descriptor with ElementName namespace URI empty and local name exactly LocalName, no children and only the generated namespace declaration when required. ElementName is an AuthoringQName, not a numeric-ID guess. InsertedParent is the actual candidate counterpart of the original target Parent, and contains InsertedNode at the proven append position. ExportSource returns a defensive copy of its source bytes.

NodeCorrespondence is a total injective old-ID to candidate-ID dictionary scoped to InputSnapshot and Snapshot, as current insertion APIs. Every old descriptor, including ancestors, comments/PIs, literal text and layout text, has exactly one actual candidate counterpart. Preserve kinds, expanded names, attribute values and parent/order relationships; permit changed lexical extents/coordinates and explicitly disclosed layout-whitespace values only. An ancestor's enlarged range is not a source rewrite of that ancestor. Attribute descriptors have no independent node IDs: compare their names, values, order and translated source ranges as metadata of the mapped element.

NewNodes is an immutable list of all actual candidate descriptors outside the image of NodeCorrespondence: exactly InsertedNode plus any newly introduced layout text. Formally, candidate descriptors are the disjoint union of mapped old descriptors and NewNodes; IDs cannot duplicate or refer outside either snapshot. A local `xmlns=""` is an attribute descriptor on InsertedNode, not an invented namespace child. BodyRange has no node. If ordinary inspection merges/splits/drops an old text descriptor so this total injective mapping cannot be established, refuse SourceAlignmentFailure rather than repair or synthesize a graph. REQ-130 merged-text deletion provenance does not implicitly authorize insertion merges.

Prove the complete inserted subtree, exact parent/name/ranges/body and all new layout descriptors. Do not accept matching labels, largest IDs, assumed child counts or a partial unchanged-node map as identity evidence. Missing or ambiguous mapping returns no candidate.

## Failure, cancellation and consumer lifecycle

Use existing classified failures for unavailable capability, foreign/unknown target, unsupported module/profile/coverage/encoding, size, parent/self-close/mixed content, invalid original/candidate, denied resource, alignment, cancellation and formatting. Add InvalidElementName only for invalid name intent; a valid name under namespace-alias hazards is UnsupportedCoverage. Do not mislabel names as XPath or fabricate original-source diagnostic ranges for a proposed name. Multiple-invalid-input precedence need not be universal; all valid single-cause fixtures need deterministic classification.

Validate original and emitted candidate using captured static context and denied resources. Cooperatively observe cancellation during name/count/layout/source traversal, before/after inspection/validation and immediately before success. No partial candidate or original mutation; no preemptive parser deadline promised. Late cancellation after the consumer has committed atomic Apply does not undo the accepted edit. Proposal computations remain isolated under concurrency.

Braid independently checks capabilities, current source/revision/license/permissions and the complete bounded frozen evidence. Review displays exact local/expanded name, whole inserted element including reset, all edits and candidate provenance. Any name/mode/source change invalidates prior review. Separate explicit Apply is atomic, selects actual InsertedNode and BodyRange, and creates one exact-byte history step. A later text/value-of insertion has a fresh target and its own step. Undo/redo rebuilds fresh inspection; old validation/preview/export receipts remain historical. Real worker validation/preview/export/reopen and native evidence are separate consumer acceptance gates, not implied by engine tests or the design PR.

## Delivery gates and review decisions

Review the [acceptance matrix](REQ-132-LITERAL-ELEMENT-ACCEPTANCE.md) and [consumer review handoff](REQ-132-CONSUMER-DESIGN-HANDOFF.md) before implementation. Proposed decisions for Braid: local reset supported, alias contexts refused, paired source/no internal whitespace, exact NewNodes partition and mandatory composition with both existing operations. No unresolved decision is silently delegated to consumer source manipulation.

A later authorized implementation needs a minor version increment, full solution/separate language-server tests, unchanged existing-operation regressions, exact-head CI, exact-merge independent package consumers, QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**. Public release verification and Braid/native acceptance follow separately. This design adds no engine source, package version or conformance gate changes.
