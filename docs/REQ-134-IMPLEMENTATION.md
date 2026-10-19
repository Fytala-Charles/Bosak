# REQ-134: literal text instruction deletion implementation

19 October 2026. **Source implementation in progress; verification pending.** Version 1.22.0; implementation base `bdefbfbfcc2f3654708e8cfd200416377bd623e3`. User explicitly authorized implementation after the [no-blocking Braid design review](REQ-134-CONSUMER-DESIGN-REVIEW.md). Review SHA-256 `4833aa45c539cd58fafad4ca35a3b3d3a57daf3a0b2f1603136df5045e0b5d1e`; imported bytes preserved unchanged.

## Delivered behavior

`AuthoringCapabilities.SupportsTextDeletion`, `MaxTextDeletionSourceBytes` and immutable `TextDeletionSupportedEncodings` describe a separate additive API. `snapshot.GetTextDeletionTarget(instruction)` returns an opaque owned target or operation-specific failure. `snapshot.ProposeTextDeletion(new(target))` returns exclusively a fresh validated candidate or failure. Null arguments throw; cancellation returns typed Cancelled. There is no NoChange branch. Legacy APIs, enum values, eligibility and cancellation behavior remain unchanged.

Paired ordinary empty/nonempty and self-closing text are supported beneath a top-level template through an LRE-only chain, including aliases/default XSLT namespaces and explicit 3.0/4.0 profiles. The target exposes actual Instruction/Parent, full OriginalRange, IsSelfClosing, nullable OriginalBodyRange, logical OriginalTextValue and actual optional OriginalPayloadNode. Self-closing has no invented body span. Comments/PIs/elements/CDATA/split bodies and unsupported effective controls refuse. Direct parent significant text/CDATA and any surviving preserving ancestor refuse, including nearer default resets; supported local xml:space on the removed instruction does not alone refuse. Existing replacement's broader mixed/preserved profile is unchanged.

One exact full-instruction source edit removes its optional payload without formatting, cleanup or reserialization. Every outside byte, BOM/declaration, encoding, URI, comment/PI, namespace and parent's paired spelling remains unchanged. Original and final are bounded to 8,388,608 physical bytes; an oversized original refuses even if deletion would fit. Four encoding variants and each source's own byte/UTF-16 coordinate map establish the zero-byte candidate seam, including a seam inside newly joined CRLF.

RemovedNodes is exactly the instruction and its actual ordinary payload when present, in preorder. Complete disjoint original and candidate partitions use actual descriptor references, an injective ordinary map and zero/one two-fragment adjacent ordinary XML-whitespace merge. Payload never becomes a survivor or merge fragment. Full lexical fragment ranges preserve exact bytes and cover the actual destination once. Fresh ordinary inspection owns joined logical text; original parsed values are not concatenated. SurvivingParent is the actual mapped direct parent. Duplicate identities, incomplete relations, inconsistent metadata/ranges and unsupported merges refuse. Collections are read-only and exported source defensive.

Validation checks source-derived profile/static/resource exclusions before restricted original compilation; independently inspects/validates exact emitted bytes before source/graph proof and final cancellation. Proposals do not transform or acquire ambient resources. Chunked copying and traversal observe cooperative cancellation; parsing has no preemptive deadline. Immutable owners remain usable on themselves and support concurrent independent candidates; application revision/history authority belongs to Braid.

Shared restricted validation has an optional internal refusal filter used only by this new operation: XML/parser and coded static diagnostics retain classified refusals; unexpected exceptions propagate. Pattern coverage uses the same filter. All legacy callers retain their existing default handling.

## Verification receipts

Verification in progress. Exact counts, command exits, TRX counters, frozen gates, immutable CI and isolated local-package identities will be recorded before handoff. No public package, signed release, consumer integration or native acceptance is claimed.

Initial receipts are retained under `.guard-tmp/req134-*`: the first positive test run exposed a test fixture relying on an unknown unqualified attribute rejected by neither the existing compiler nor validation; it was corrected to an actually invalid missing-variable original. A replacement-result harness assertion incorrectly used IsSuccess and was corrected to Candidate. Source-control fixture setup initially omitted constants, then was corrected. The corrupted duplicate-ID control found a real new-pipeline gap, fixed by explicit per-snapshot identity uniqueness checks; correspondence still uses references. A targeted rebuild overlapping full-suite execution encountered a locked test DLL; it is retained and will be rerun after the suite completes.

## Acceptance and next gate

The [28-group acceptance plan](REQ-134-TEXT-DELETION-ACCEPTANCE.md) distinguishes engine evidence from consumer-only TD23-TD27. Internal corruption/cancellation controls are source-only, excluded from public PackageReference tests. Braid must independently review the immutable merged implementation/package through the forthcoming implementation handoff, then separately adopt verified public packages and implement its adapter, complete review, atomic Apply/history/parent focus, real worker/export/reopen and native observations. Publication requires separate approval after implementation review. No Braid files, pins or runtime were modified.

## Consumer API example

Source 1.22.0 only; public 1.21.0 does not provide these members. Start with an actual instruction descriptor selected from the current snapshot:

```csharp
var discovered = snapshot.GetTextDeletionTarget(instruction, cancellationToken);
if (discovered.Target is { } target)
{
    var result = snapshot.ProposeTextDeletion(new(target), cancellationToken);
    if (result.Candidate is { } candidate)
    {
        // Freeze and verify complete source/removal/map/merge evidence before review.
        byte[] proposedBytes = candidate.ExportSource();
        var proposedSelection = candidate.SurvivingParent;
        // The application separately authorizes atomic Apply and one history step.
    }
}
```

Read Failure.Kind instead of parsing messages; failure ranges, when present, belong to input coordinates. Do not infer this capability from value-of deletion or replacement, count only one removed descriptor, treat fragment ranges as nodes, or concatenate pre-merge parsed values. A null body span denotes self-closing syntax; it does not prevent whole-instruction removal. All successful candidates change source, including empty/self-closing instructions with unchanged sample output.
