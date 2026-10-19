# REQ-134: literal text instruction deletion implementation

19 October 2026. **Source implementation locally verified; consumer implementation review pending.** Version 1.22.0; implementation base `bdefbfbfcc2f3654708e8cfd200416377bd623e3`. User explicitly authorized implementation after the [no-blocking Braid design review](REQ-134-CONSUMER-DESIGN-REVIEW.md). Review SHA-256 `4833aa45c539cd58fafad4ca35a3b3d3a57daf3a0b2f1603136df5045e0b5d1e`; imported bytes preserved unchanged.

## Delivered behavior

`AuthoringCapabilities.SupportsTextDeletion`, `MaxTextDeletionSourceBytes` and immutable `TextDeletionSupportedEncodings` describe a separate additive API. `snapshot.GetTextDeletionTarget(instruction)` returns an opaque owned target or operation-specific failure. `snapshot.ProposeTextDeletion(new(target))` returns exclusively a fresh validated candidate or failure. Null arguments throw; cancellation returns typed Cancelled. There is no NoChange branch. Legacy APIs, enum values, eligibility and cancellation behavior remain unchanged.

Paired ordinary empty/nonempty and self-closing text are supported beneath a top-level template through an LRE-only chain, including aliases/default XSLT namespaces and explicit 3.0/4.0 profiles. The target exposes actual Instruction/Parent, full OriginalRange, IsSelfClosing, nullable OriginalBodyRange, logical OriginalTextValue and actual optional OriginalPayloadNode. Self-closing has no invented body span. Comments/PIs/elements/CDATA/split bodies and unsupported effective controls refuse. Direct parent significant text/CDATA and any surviving preserving ancestor refuse, including nearer default resets; supported local xml:space on the removed instruction does not alone refuse. Existing replacement's broader mixed/preserved profile is unchanged.

One exact full-instruction source edit removes its optional payload without formatting, cleanup or reserialization. Every outside byte, BOM/declaration, encoding, URI, comment/PI, namespace and parent's paired spelling remains unchanged. Original and final are bounded to 8,388,608 physical bytes; an oversized original refuses even if deletion would fit. Four encoding variants and each source's own byte/UTF-16 coordinate map establish the zero-byte candidate seam, including a seam inside newly joined CRLF.

RemovedNodes is exactly the instruction and its actual ordinary payload when present, in preorder. Complete disjoint original and candidate partitions use actual descriptor references, an injective ordinary map and zero/one two-fragment adjacent ordinary XML-whitespace merge. Payload never becomes a survivor or merge fragment. Full lexical fragment ranges preserve exact bytes and cover the actual destination once. Fresh ordinary inspection owns joined logical text; original parsed values are not concatenated. SurvivingParent is the actual mapped direct parent. Duplicate identities, incomplete relations, inconsistent metadata/ranges and unsupported merges refuse. Collections are read-only and exported source defensive.

Validation checks source-derived profile/static/resource exclusions before restricted original compilation; independently inspects/validates exact emitted bytes before source/graph proof and final cancellation. Proposals do not transform or acquire ambient resources. Chunked copying and traversal observe cooperative cancellation; parsing has no preemptive deadline. Immutable owners remain usable on themselves and support concurrent independent candidates; application revision/history authority belongs to Braid.

Shared restricted validation has an optional internal refusal filter used only by this new operation: XML/parser and coded static diagnostics retain classified refusals; unexpected exceptions propagate. Pattern coverage uses the same filter. All legacy callers retain their existing default handling.

## Verification receipts

[Machine-readable verification receipt](REQ-134-VERIFICATION.json) preserves counts, original TRX hashes and initial package provenance at immutable implementation head `16c71179ecc1b79bfa2c36fcd884437128e71440`. [PR #383](https://github.com/Fytala-Charles/Bosak/pull/383) carries the source and documentation; final-head CI and final-head/merge package receipts must be confirmed before the [consumer implementation handoff](REQ-134-CONSUMER-IMPLEMENTATION-HANDOFF.md). Later documentation-only heads preserve the reviewed engine/test trees. No public package, signed release, consumer integration or native acceptance is claimed.

| Verification | Result |
|---|---|
| Release solution build | Exit 0; zero warnings/errors |
| Full solution, final strict validator | 10,010/0/0; all ten TRX files verified, exit 0 |
| Language-server tests | 84/0/0, exit 0 |
| New deletion source cases | 110/0/0: 94 public + 16 internal controls |
| Initial isolated package-public consumer | 531/0/0: 94 new + 437 existing checks; exact 16c71179 packages; clear audit |
| Initial broad compatibility consumer | 153 checks pass; all ten assembly identities |
| QT3 | 31,142/0/679; exit 0 |
| qt4 gated | 8,278/0/1,894; exit 0 |

The 531 package checks overlap source coverage; they are not additional unique features. Sixteen new internal controls and thirteen prior source controls are excluded from the public package harness. Ten unsigned local archives, nine focused restored archive/cache/output DLLs, informational revisions, hashes, restore-source receipts and dependency audit are independently verified. These are local candidates, not signed public packages. Build/test/gate logs and distinct initial/final-head/merged package caches live under `.guard-tmp/req134-*`.

Canonical documentation was rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/req134-implementation-render`; no native IDE/PDF visual walkthrough is claimed. README, branding assets, extension lifecycle and published tags remain unchanged.

Initial receipts are retained under `.guard-tmp/req134-*`: the first positive test run exposed a test fixture relying on an unknown unqualified attribute rejected by neither the existing compiler nor validation; it was corrected to an actually invalid missing-variable original. A replacement-result harness assertion incorrectly used IsSuccess and was corrected to Candidate. Source-control fixture setup initially omitted constants, then was corrected. The corrupted duplicate-ID control found a real new-pipeline gap, fixed by explicit per-snapshot identity uniqueness checks; correspondence still uses references. A targeted rebuild overlapping full-suite execution encountered a locked test DLL; the original failure is retained and the subsequent sequential targeted/full runs pass. The initial broad consumer retained an old numeric assembly-version assertion (1,21,0,0); only the harness expectation was corrected to 1,22,0,0 before its successful rerun. No engine change followed that harness correction. Final audit tightened only new-operation exception classification through an optional internal filter; final full/source/gate checks were rerun afterward.

## Acceptance and next gate

The [28-group acceptance plan](REQ-134-TEXT-DELETION-ACCEPTANCE.md) distinguishes engine evidence from consumer-only TD23-TD27. Internal corruption/cancellation controls are source-only, excluded from public PackageReference tests. Braid must independently review the immutable merged implementation/package through the [implementation handoff](REQ-134-CONSUMER-IMPLEMENTATION-HANDOFF.md), then separately adopt verified public packages and implement its adapter, complete review, atomic Apply/history/parent focus, real worker/export/reopen and native observations. Publication requires separate approval after implementation review. No Braid files, pins or runtime were modified.

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
