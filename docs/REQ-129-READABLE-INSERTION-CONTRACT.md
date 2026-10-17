<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala readable insertion contract">
<h1>REQ-129: readable value-of insertion contract</h1>
<p>Proposed design for review — 17 October 2026</p>
</div>

**Implementation checkpoint - 17 October 2026:** The user authorized this bounded engine slice after design preparation. Source 1.17.0 now implements the proposed public surface and edit/provenance contract; final validation/integration is tracked in [implementation evidence](REQ-129-IMPLEMENTATION.md). Released 1.16.5 does not contain this capability. Original proposed-state wording below is preserved as design history.



## Decision and delivery boundary

Add an explicit opt-in readable formatting mode to the existing bounded REQ-128 value-of insertion operation. The existing two-argument proposal constructor and omitted formatting option retain byte-identical legacy behavior. This document is a proposed contract, not an implemented capability or consumer acceptance. REQ-129 remains **Pending**, target **TBD**, owner **Unassigned** until implementation is authorized and assigned. A released feature would require a minor version bump from the then-current library version; no version is reserved here.

Baseline: released engine 1.16.5, with the insertion API originally shipped in 1.8.0. This design was checked against main `6069a7a6d74d693fe13d1013f8c9946f291fd821`, particularly `AuthoringValueOfInsertion.cs`, `AuthoringCapabilities.cs` and `ValueOfInsertionPipeline.cs`. Existing insertion coverage, eligibility, static validation and denied-resource behavior remain prerequisites. No new instruction creation, dependency editing, self-closing-parent expansion, general document formatter or Braid-side XML splicing is introduced.

## Proposed additive public surface

These are proposed names; they do not exist in the released API.

| Addition | Contract |
|---|---|
| `AuthoringInsertionFormattingMode` | `PreserveSource = 0`, `Readable = 1`; unknown values are rejected as invalid arguments before emission. |
| Three-argument `AuthoringValueOfInsertionProposal(target, selectExpressionText, formattingMode)` | Preserve the existing two-argument constructor exactly; delegate its behavior to PreserveSource. Immutable mode captured per proposal, with no mutable global preferences. |
| `AuthoringCapabilities.SupportsReadableValueOfInsertion` | Independent capability discovery; true only when this contract is implemented and verified. Existing insertion support alone does not imply formatting support. |
| Candidate `FormattingMode` | Report the mode actually applied; never silently downgrade Readable to PreserveSource. |
| Candidate `SourceEdits` | Immutable ordered collection of engine-issued edits, each with an original-source `OriginalRange` and candidate-source `ReplacementRange`. Ranges carry the existing byte/line/column coordinate convention. Replacement bytes are obtained from the exact exported candidate, not consumer reconstruction. |
| `AuthoringInsertionFailureKind.UnsupportedFormatting` | Formatting-specific ambiguity or inability to prove safe layout. Existing MixedContent, UnsupportedCoverage, UnsupportedEncoding, SizeLimit, Cancelled and alignment failures retain their meanings. |

New edit descriptors have no public mutable constructors/backing collections. PreserveSource reports its existing one zero-length insertion as one edit. In Readable, edits may replace existing whitespace gaps and insert new whitespace/instruction bytes. The legacy candidate `OriginalRange` remains the zero-length instruction insertion anchor in the input; `InsertedRange` remains exactly the new instruction markup, excluding surrounding formatting. Neither legacy property describes the entire formatted footprint. Readable consumers must use SourceEdits plus the exported candidate. The select descriptor remains the actual new Expression slot in the fresh candidate snapshot.

Older binaries continue using the original constructor and receive the original layout. Consumers compiled against the new API must use their normal package/version compatibility checks before calling it; a missing member on an old engine is not a supported capability probe.

## Permitted footprint and algorithm

1. Perform ordinary REQ-128 target, scope, encoding, contextual expression and original-source validation. A formatting request cannot make an otherwise ineligible parent eligible.
2. Identify only the target parent's **immediate content gaps**: the text between its opening tag and first direct child markup, between adjacent direct child markup, and between its last direct child and closing tag. Child elements, comments and processing instructions are markup units. A gap is eligible only when empty or composed exclusively of XML whitespace (space, tab, CR, LF). Markup token boundaries come from retained engine source evidence, not a second parser in Braid.
3. Preserve every direct child's entire markup bytes, including nested indentation, attributes, XPath strings, namespace declarations, comments and PI bodies. Preserve the parent's opening/closing tag bytes. The new instruction's prefix selection, namespace declaration and expression escaping use the existing insertion rules. Only new instruction markup and these explicitly enumerated gaps may differ.
4. Produce one direct child markup unit per line, including the inserted instruction appended last. Existing children keep their order. Rewrite eligible gaps only as necessary; do not add a duplicate blank line when repeating insertion. Existing nonempty whitespace gaps remain nonempty, so an existing whitespace descriptor is not silently deleted. A source-alignment proof must account for all existing descriptors, including any whitespace represented in the inspection graph.
5. For an already indented parent, infer a single child indentation from consistent direct-child line prefixes and use the closing-tag indentation as the parent baseline. Require the child indentation to extend the parent baseline by a consistent nonempty all-space or all-tab unit. Inconsistent indentation, mixed space/tab units or inline/multiline combinations without a unique layout refuse UnsupportedFormatting. Empty parents may use a consistent surrounding ancestor indentation unit, with the same proof requirements.
6. For fully compact content with no existing multiline child layout, take the leading spaces/tabs of the line containing the parent opening tag as the baseline and use **two spaces** as the child indentation unit. If the opening tag follows other markup on that line, this is still the line's leading indentation; do not invent nesting depth or reformat ancestors. This bounded operation may leave ancestor markup inline. Compact gap spaces may be replaced under the declared footprint.
7. Preserve a uniform existing newline convention: LF, CRLF or lone CR. Determine it from the retained source, treating CRLF as one newline; mixed conventions refuse UnsupportedFormatting. If the entire source has no newline, use LF. Insert exactly that convention for all generated layout; do not normalize line endings elsewhere. Do not expose a custom newline/indent setting in this first slice.
8. Emit a sorted, nonoverlapping edit plan in original byte coordinates. Coalesce changes sharing a gap/offset into one unambiguous edit; apply against original coordinates, never progressively shifted offsets. Every byte outside these spans must match the original exactly. Validate the resulting size before allocation and preserve strict UTF-8/no-BOM or BOM, UTF-16 LE/BE BOM/declaration, Unicode and module URI.

For a parent beginning at column one, compact `<result><a/><b/></result>` becomes the following after appending the shown instruction (within an otherwise eligible stylesheet):

```xml
<result>
  <a/>
  <b/>
  <xsl:value-of select="/input/name"/>
</result>
```

The example assumes the xsl prefix is already valid in the actual stylesheet context. No comment/PI payload is reformatted. A later append uses a newly issued target from the accepted candidate's fresh inspection and keeps the same indentation, rather than compounding blank lines or reusing the old handle.

## Semantic guards and refusals

Formatting is allowed only where the affected source whitespace is proven to be ignorable stylesheet whitespace under the supported XSLT profile. Effective `xml:space="preserve"`, significant text, CDATA and unsupported text-value-template contexts retain the existing conservative refusal. Do not infer safety merely because characters are whitespace, or trim xsl:text content. Namespace/profile/shadow/static-preprocessing/component restrictions from REQ-128 remain in force. Do not broaden eligibility to make formatting examples pass.

Ambiguous newline or indent evidence, a non-whitespace affected gap, an unprovable whitespace-stripping context or inability to preserve total node correspondence refuses with no candidate. PreserveSource is an explicit separate retry decision for the caller; the engine never silently changes the requested mode. PreserveSource must continue accepting every case it accepted before, even when Readable refuses it.

Compilation alone does not prove semantic equivalence. Acceptance compares execution of Readable and PreserveSource candidates for the **same insertion and select expression** on identical fixtures, using exact output bytes where declared deterministic. The original stylesheet is not expected to have the same output as a stylesheet with an inserted value-of. No global proof of arbitrary XPath/XSLT equivalence is claimed.

**Verified coverage refinement - 17 October 2026:** Readable refuses beneath any ancestor `xml:space="preserve"`, including a nearer `xml:space="default"` reset. Same-insertion execution comparisons exposed observable result whitespace in this engine context; effective XML settings alone do not prove safe engine behavior. PreserveSource remains supported under its existing rules. The first failing equivalence receipt is preserved; no execution-engine whitespace correction is included in this slice.

## Snapshot, range and provenance rules

The original snapshot and all exported copies remain immutable. Export the exact formatted bytes, inspect those bytes into a fresh ordinary candidate snapshot, and validate the actual inserted select slot under its captured static context. Retain the module URI and existing strict coordinate semantics across UTF-8 multi-byte scalars and UTF-16 surrogate pairs.

Return total, injective old-node-to-surviving-candidate correspondence for the whole original descriptor graph. Existing whitespace descriptors whose content changed map only through the proven corresponding gap; other existing descriptors map by the explicit edit translation and structural identity. Added formatting-only descriptors are new identities and cannot be mistaken for surviving originals or the new instruction. Duplicate, missing, merged or ambiguous original identity evidence refuses SourceAlignmentFailure. This is an implementation gate, not permission to weaken completeness to element-only mapping.

Require exactly one identifiable inserted instruction and its actual select range/expanded value. Input and candidate ranges are never interchangeable. SourceEdits must replay to the exported candidate exactly, and unchanged spans must match the original byte for byte. Preserve the current defensive-copy behavior of exported bytes and descriptor collections. Candidate success means produced for review; it does not grant consumer Apply/export authority or revive prior validation.

## Resources, cancellation and consumer lifecycle

Count original bytes, all replaced/inserted layout bytes and final bytes against the existing 8,388,608-byte insertion limit, including BOM. Check lengths with overflow-safe arithmetic before allocating output; honor existing module/validation resource limits. Both modes use the existing deny-by-default resource policy and perform no transformation, external acquisition or file writes while proposing. No mutable resolver or prior compilation result supplies authority.

Observe cooperative cancellation during whitespace scanning and edit planning, before emission and validation/reinspection, after validation/reinspection, and immediately before returning success. Cancellation returns no candidate, leaves the source intact and does not mutate any cache or consumer state. Do not promise preemptive cancellation inside a synchronous parser call.

Braid discovers the separate capability, chooses Readable explicitly and reviews the exact engine candidate with its full changed spans. One accepted insertion is one history step, even if several gaps changed. Undo restores exact prior bytes; redo restores exact reviewed formatted bytes under a new source revision. Neither restores old inspection/validation/preview/export authority. Braid must recheck source identity, cancellation and permission at Apply and must never format or splice the returned candidate itself. These consumer changes and native acceptance are a separate delivery after the engine implementation is reviewed.

## Acceptance matrix and release gates

All rows below are planned tests, not passing test claims.

| Group | Required evidence |
|---|---|
| Compatibility | Existing two-argument constructor and explicit PreserveSource yield exact historical bytes, ranges, failures and provenance; no capability inferred from SupportsValueOfInsertion alone. |
| Happy paths | Empty explicit-close parent, compact existing siblings and consistently indented parent; verify readable golden bytes, SourceEdits replay, select and instruction ranges. |
| Layout | Two-space compact fallback, inherited consistent spaces/tabs, LF/CRLF/lone CR, source without newline, comments/PIs, repeat insertion with fresh target and no extra blank lines. |
| Refusal edges | Mixed newline/indent styles, mixed content, CDATA, effective xml:space, unsupported profiles/coverage, self-closing parent, dependencies, foreign/stale targets and undefined mode. Verify original bytes stay unchanged and no silent fallback. |
| Fidelity | UTF-8 BOM/no-BOM, UTF-16 LE/BE BOM/declarations, non-BMP/combining characters, escaped XPath strings, prefix collision and inherited/rebound namespaces; all unchanged spans byte-identical. |
| Provenance | Full old descriptor map including ancestors and represented whitespace, added whitespace identities, exact new instruction/select, no ambiguous/missing correspondence and defensive-copy checks. |
| Execution | Same insertion/select in both modes executes to identical declared exact output bytes on representative literal-result trees and variable/function contexts already inside REQ-128 coverage. Significant-content contexts refuse. |
| Bounds | Original at/over limit, formatting-induced over-limit candidate, multi-byte length accounting, arithmetic overflow guards, cancellation at work boundaries and zero resolver/file/network side effects. |
| Consumer | Capability discovery, exact reviewed candidate Apply, refusal visibility, one history step, exact undo/redo, stale authority/permission/cancellation checks, selection and range navigation; native appearance/focus review recorded separately. |

Implementation requires happy-path and edge-case tests, the complete existing insertion compatibility suite, full solution tests, language-server compatibility checks and unchanged engine gates: QT3 exactly **31,142/0/679**, qt4 gated **8,278/0/1,894** unless a separately approved gate change occurs. Package-only consumers must verify new APIs against the exact merged revision and replay legacy insertion cases. Release notes and canonical docs must clearly state opt-in behavior and bounded coverage. Neither upstream tests nor gate memberships change merely to accommodate formatting.

## Next step

Review this contract before implementing it. The recommended implementation is one bounded engine slice adding the opt-in mode, explicit edit evidence and planned tests, followed by independent Braid integration and native review. No implementation, release, consumer pin or final acceptance is authorized by this design document.
