<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala literal-text insertion contract">
<h1>REQ-131: literal-text insertion contract</h1>
<p>Proposed structural authoring operation — 17 October 2026</p>
</div>

## Status and scope

**Pending: proposed contract only. Target version TBD; implementation owner unassigned.** The product owner requested the next design step after Braid accepted REQ-129. This registers an internal authoring-roadmap proposal for Braid; it does not claim a separately submitted consumer request or authorize engine implementation. A future implementation would need a minor version increment from the then-current source version; no version is reserved here.

Append one engine-created `xsl:text` carrying literal text at the end of an eligible principal-module sequence constructor. This complements value-of insertion; sequence insertion, result-element creation, arbitrary XML fragments, text value templates, disable-output-escaping and general document formatting remain separate operations. Existing text execution in the compiler is not an advertised authoring creation capability.

Design baseline: public 1.17.0 at `15eacde48b47cc8e48af29ddc95949801a2e3e5d`, with documentation main `24841b7ca33b6b9ad31313ef19cda12e7dea4e82`. Existing insertion APIs, `ValueOfInsertionPipeline`, `ValueOfInsertionLayout`, capabilities and source descriptors were reviewed. No source, public API, dependency pin, version, gate membership or release changes in this design step.

**Identifier correction:** An independent Braid handoff registered REQ-130 for value-of deletion while this text design was being drafted. Its dossier is preserved unchanged. This proposal uses REQ-131; it does not displace that consumer request or authorize text implementation ahead of deletion. The earlier drafting commit and superseded CI receipt remain in history.

## Proposed additive API

Names below are proposed and absent from released 1.17.0. Preserve existing value-of constructors and result types. Do not turn their targets into a generic XML-insertion handle.

| Proposed surface | Contract |
|---|---|
| `AuthoringCapabilities.SupportsTextInsertion` | Independent capability, advertised only after the declared text operation passes its delivery gates. |
| `SupportsReadableTextInsertion` | Independent readable-layout capability; readable value-of support does not imply this capability. |
| `MaxTextInsertionSourceBytes` | Original and complete emitted module each limited to 8,388,608 bytes, including BOM; independently documented from the value-of limit. |
| `AuthoringSnapshot.GetTextInsertionTarget(parent, cancellationToken)` | Typed result containing an opaque snapshot-owned `AuthoringTextInsertionTarget` or existing classified insertion failure. No public target constructor. |
| `AuthoringTextInsertionProposal(target, textValue)` | Immutable literal string; default PreserveSource. Null arguments throw ArgumentNullException; empty text is legal. |
| Three-argument proposal with `AuthoringInsertionFormattingMode` | Explicit PreserveSource or Readable; reuse the existing mode enum, reject undefined values before emission. No ambient mutable formatting settings. |
| `AuthoringSnapshot.ProposeTextInsertion(proposal, cancellationToken)` | `AuthoringTextInsertionResult`: exactly one complete candidate or failure, never partial success. |
| `AuthoringTextInsertionCandidate` | InputSnapshot, fresh Snapshot, ModuleUri, FormattingMode, OriginalRange anchor, InsertedRange, SourceEdits, TextValue, TextPayloadRange, optional PayloadNode, InsertedNode, NodeCorrespondence and defensive ExportSource(). |
| Existing failure enum plus appended `InvalidPayload` | Invalid XML characters/unpaired surrogates are payload errors, not InvalidExpression. Keep existing enum numeric values stable. Other scope/layout/validation/resource/alignment failures retain existing meanings. |

Target results follow the existing mutually exclusive Target/Failure pattern. Candidates and edit/mapping collections are immutable; inputs cannot be modified through exported arrays or descriptors. Consumers use a compatible package/adapter before accessing newly introduced members: missing members on an older engine are not a trial-call capability mechanism.

## Literal payload and emission

TextValue is decoded Unicode text, not XPath, an AVT, a text value template, markup or a CDATA fragment. Do not trim, normalize Unicode, infer entities, double braces or evaluate expression-looking content. A string such as `{doc('https://invalid.example/')}` must remain literal and cause no resource request.

Validate the complete payload as characters accepted by the existing supported XML source profile. Refuse unpaired UTF-16 surrogates, NUL and other illegal XML characters as InvalidPayload; never use encoding replacement characters to hide an invalid input. This slice adds no XML-version support.

Choose a valid in-scope nonempty XSLT prefix using the existing deterministic namespace rules, otherwise declare a collision-free prefix on the new instruction only. Keep all old namespace bindings and markup bytes intact. Emit an explicit paired instruction, even for empty text:

```xml
<xsl:text expand-text="no">encoded literal payload</xsl:text>
```

The actual prefix/declaration depends on the captured target context. Always set the instruction's unqualified `expand-text="no"` so inherited expansion cannot interpret braces. No disable-output-escaping option is exposed and its default is retained. Prove the local override in supported captured profiles; if the compiler cannot honor it, refuse the affected context rather than silently changing the payload or evaluating it. Text value templates are controlled by the nearest applicable expansion setting in [XSLT 3.0, Text Value Templates](https://www.w3.org/TR/xslt-30/).

Deterministic encoding for newly emitted payload text:

| Logical character | Source spelling |
|---|---|
| Ampersand, less-than, greater-than | `&amp;`, `&lt;`, `&gt;` respectively; escaping greater-than also prevents a raw `]]>` sequence. |
| CR, LF, TAB | `&#xD;`, `&#xA;`, `&#x9;` respectively. |
| Braces, quotes and other legal characters | Literal characters in the retained source encoding; no XPath or brace escaping. |

Encode both members of a logical CRLF pair independently, preserving that pair in TextValue. Numeric newline references prevent payload line breaks from introducing a competing physical source newline convention. XML otherwise normalizes literal source line endings, so source layout and payload characters must be handled separately. [XML 1.0 §2.11](https://www.w3.org/TR/REC-xml/#sec-line-ends).

For example, the logical payload represented as `"Label: & <{customer}>\r\n"` in C# string notation produces this instruction body in a context with an existing xsl prefix:

```xml
<xsl:text expand-text="no">Label: &amp; &lt;{customer}&gt;&#xD;&#xA;</xsl:text>
```

The output characters are the requested text. XML/HTML serialization may escape them according to the selected output method; this operation grants no raw-markup bypass. Empty text still creates a reviewable source instruction, though result-tree construction can discard empty text or merge adjacent result text nodes. Source identities do not assert distinct result-tree identities.

## Targets, coverage and coexistence

Retain REQ-128's single-principal-module, explicit-close, template/literal-result-chain and captured profile restrictions, including unsupported static preprocessing, dependencies, schema/component coverage and denied resources. Current bounded explicit 3.0/4.0 authoring profiles must be tested separately; this proposal adds no wider language conformance. Reject foreign descriptors and targets by snapshot ownership, not coincidentally equal IDs or bytes. A target remains valid for its own immutable input snapshot; only Braid can determine whether that snapshot is still the application's current revision.

Keep significant direct parent text, direct CDATA and effective xml:space=preserve outside the initial target profile, as for value-of insertion. A literal xsl:text child is a markup unit whose protected payload is not parent formatting whitespace. Preserve its entire original source bytes during later insertions.

**Required coverage change, not automatic reuse:** released 1.17.0's insertion allow-list excludes `xsl:text`. The implementation must deliberately recognize validated non-expanding literal xsl:text, including newly generated instructions, so repeated text insertion and mixed text/value-of authoring work. Do not merely add its name to an allow-list. Prove literal expansion policy, supported attributes/content, static validity and unchanged provenance; existing expanding/disable-output-escaping or otherwise unsupported forms retain coverage refusal.

Document and test this bounded newly supported literal-text input form for both insertion operations. Every previously successful value-of proposal must retain its original behavior and bytes in PreserveSource; unrelated refusal boundaries remain. Text and value-of target objects remain distinct even when the private parent eligibility/layout code is shared. A fresh text or value-of target must be discovered from each newly accepted snapshot.

## Formatting and exact edit footprint

PreserveSource inserts only the complete new instruction at the existing engine-proven append anchor. Readable uses the established REQ-129 direct-child whitespace plan: uniform existing newlines/indentation, two-space compact fallback and conservative ambiguity refusal. Surrounding generated indentation is outside the xsl:text payload range. Never indent, wrap, trim or normalize payload content, existing literal text bodies, comments, PI payloads, attributes or descendants.

Readable retains the verified guard against **any ancestor xml:space=preserve, including a nearer default reset**. Do not silently fall back to PreserveSource. If prospective formatted text cannot be proved safe, return UnsupportedFormatting. Compare Readable and PreserveSource execution for the same literal insertion; neither is required to match the original stylesheet's output before the insertion.

The final byte count includes original bytes minus replaced gap bytes plus new instruction, encoded literal payload and formatting bytes. Count all escaped/reference expansion with overflow-safe arithmetic before allocating the escaped payload or output buffer. A small decoded payload can exceed the byte limit after escaping; UTF-16 counts physical bytes. Whitespace shrinkage may make a formatted candidate fit even when a pure append would overflow. Do not change the existing value-of accounting.

## Candidate ranges and identity

InsertedRange covers the complete new instruction markup only. TextPayloadRange is its exact encoded content span in the candidate source; for empty text, it is a zero-length range after the opening tag. It is not a range in the input and its byte/column length need not equal TextValue.Length. Do not fabricate an Expression/select slot for literal text.

For a nonempty payload, PayloadNode is the actual new source text descriptor with decoded Text exactly equal to TextValue and the proven lexical range. For empty text, PayloadNode is null; no fake zero-length source text descriptor is manufactured. InsertedNode and any payload descriptor are explicitly new identities in the fresh candidate, never guessed from largest IDs, labels or child position.

Return total injective correspondence for every old source descriptor, including ancestors, existing text and any changed formatting-whitespace descriptors. Explicitly account for the entire newly inserted subtree (instruction plus optional text child) and newly introduced formatting whitespace. The value-of-only assumption of one extra source node is insufficient. Missing, duplicate, merged or ambiguous old identity evidence refuses SourceAlignmentFailure. Empty/adjacent result text behavior never weakens source-graph completeness.

SourceEdits reuses the immutable existing edit descriptor with original-coordinate OriginalRange and candidate-coordinate ReplacementRange. Edits are ordered/nonoverlapping, coalesced at shared positions, and replay to the exact exported candidate. Untouched spans are byte-identical. OriginalRange on the candidate remains the zero-length instruction anchor and is not the full Readable footprint. All existing line/column conventions, byte offsets, BOM bias and Unicode coordinate handling apply.

## Validation, resources, cancellation and consumer lifecycle

Inspect the exact emitted bytes into a fresh ordinary snapshot at the same module URI, preserving strict UTF-8 BOM/no-BOM and BOM-marked UTF-16 LE/BE/declaration rules. Validate original and candidate under the captured supported context with deny-by-default resource authority. Proposal generation neither transforms input nor performs module/network/file access or extension execution. The payload never goes through an XPath parser. A failed static validation, alignment check or cancellation returns no candidate and leaves original source and all existing authority unchanged.

Observe cooperative cancellation during scalar validation/encoding/counting, source/layout traversal, emission, before/after candidate inspection/validation and immediately before success. No preemptive deadline inside a synchronous parser is promised. Keep per-proposal state isolated for concurrent requests.

Braid separately discovers text/readable-text capability, edits a literal-text field, reviews the exact exported candidate and all source edits, and explicitly Applies under current permission/source/cancellation checks. It selects the engine-issued new instruction, not a fabricated select slot. One accepted insertion is one history step; undo/redo restores exact bytes under fresh source revisions without reviving old inspection/validation/preview/export authority. New preview/export/reopen work is required after Apply. These consumer changes require independent implementation and acceptance; REQ-129 acceptance does not cover this new operation.

## Acceptance and implementation gates

The [planned acceptance matrix](REQ-131-TEXT-INSERTION-ACCEPTANCE.md) specifies happy-path, edge, compatibility, payload, provenance, budget and consumer cases. They are plans, not executed tests. Implementation requires full solution and language-server tests, exact-head and exact-merge package-only consumers, unchanged frozen QT3 **31,142/0/679**, and qt4 gated **8,278/0/1,894** unless a separately approved gate change occurs. Preserve original failure receipts and use separate corrections; no skips, aliases or gate promotions merely to make this slice green.

Review this contract before starting one bounded engine implementation PR. Ship no capability flag, version bump, package or Braid command as part of this design document.
