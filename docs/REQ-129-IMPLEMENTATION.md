<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala REQ-129 readable insertion implementation">
<h1>REQ-129 readable insertion implementation</h1>
<p>Opt-in bounded source formatting — 17 October 2026</p>
</div>

## Behavior and compatibility

Unpublished source 1.17.0 implements the [reviewed contract](REQ-129-READABLE-INSERTION-CONTRACT.md). Source edits affect only the selected parent's direct-child whitespace gaps and one appended value-of instruction. Existing tags, child markup, comments, PI payloads, XPath expressions and bytes outside declared spans stay intact. The complete final encoded length is counted before replacement/output allocation, including BOM. A compact parent gets two-space child indentation; a consistently indented parent retains its unit. Uniform LF/CRLF/lone CR are retained; sources with no newlines use LF.

Mixed newline/indent conventions, raw non-whitespace gaps and unprovable layout refuse UnsupportedFormatting without implicit fallback. Existing MixedContent, xml:space, static coverage, encoding, explicit-close, module/resource and cancellation restrictions remain. Readable also refuses beneath any ancestor xml:space=preserve even when a nearer xml:space=default resets the effective XML setting: execution testing found observable preservation in that engine context. PreserveSource remains available; no broader whitespace-execution fix is included. No supported XSLT profile, instruction set, gate membership or external resource access is broadened.

## Public integration

```csharp
if (AuthoringCapabilities.SupportsReadableValueOfInsertion)
{
    var proposal = new AuthoringValueOfInsertionProposal(
        target, "/input/name", AuthoringInsertionFormattingMode.Readable);
    var result = snapshot.ProposeValueOfInsertion(proposal, cancellationToken);
    // Review result.Candidate.ExportSource() and SourceEdits before consumer Apply.
}
```

The existing two-argument constructor is retained and selects PreserveSource. The new enum and three-argument overload capture immutable per-proposal intent. Candidate FormattingMode reports the actual mode. Candidate SourceEdits is an immutable ordered list of AuthoringInsertionSourceEdit, each with OriginalRange in input coordinates and ReplacementRange in exported candidate coordinates. Splicing those exact candidate bytes into the original ranges reproduces the export exactly. Exported source remains defensive-copy data.

OriginalRange on the candidate remains the legacy zero-length instruction anchor, not the complete formatted footprint; InsertedRange covers only the new instruction, and SelectSlot is the actual contextual expression descriptor. Readable consumers must use SourceEdits to review all affected gaps. The complete old descriptor graph maps injectively to the candidate, including represented whitespace; replaced trailing whitespace maps to the leading part of its replacement, and added whitespace gets fresh identities. Missing or ambiguous correspondence refuses SourceAlignmentFailure. No consumer may reformat the candidate and retain its acceptance evidence.

## Implementation

AuthoringValueOfInsertion.cs adds the immutable public mode/edit evidence and compatible constructor overload. AuthoringCapabilities advertises readable formatting independently. ValueOfInsertionLayout plans exact character spans, checks encoded final size, emits retained byte spans and translates provenance through all edits. ValueOfInsertionPipeline retains validation/escaping/resource rules, inspects the emitted bytes and proves complete surviving identities, allowing only explicitly accounted new whitespace descriptors. No VM/parser opcode or engine execution semantics change.

## Verification and integration gates

The final focused run passes **177 cases** (35 new formatting cases and 142 unchanged insertion controls). The initial complete focused run passed 173 cases (31 new formatting cases and 142 unchanged insertion controls). Four supplementary profile/xml:space/scope cases bring the final total to 177. Their first run passes 176 and exposes output whitespace beneath an ancestor xml:space=preserve with a default reset. The implementation conservatively refuses that formatting context; the original failure log is retained, and corrected verification passes all 177 cases. Golden candidate/output checks cover same-expression readable versus PreserveSource execution. Encoding cases cover UTF-8 BOM/no-BOM, UTF-16 LE/BE and non-BMP/combining text. Exact edit replay, unchanged spans, repeated insertion, namespace variants, defensive copies, ambiguity/refusal and final-byte-limit edges are covered.

Frozen QT3: **31,142/0/679**. qt4 gated: **8,278/0/1,894**, zero gated failures. Initial local build sandbox access failure and supplemental rebuild file-lock failures are retained separately; these are build-environment/overlap receipts, not passing test claims. The earlier full solution run passes 9,608 cases (before the four supplementary fixtures and final guard); 84 language-server tests also pass. Rebuild file-lock attempts and the first 176/1 equivalence run are retained separately. Exact final-head CI must pass all 9,612 solution cases and 84 language-server cases before integration; its result is attached to the implementation PR. Exact-head and merged-revision package-only verification must pass 140 current public legacy cases plus all 35 new formatting cases (175 total); the two internal traversal controls remain in source unit tests and are excluded from the public-only consumer without spoofing a friend assembly.

No public release or consumer adoption occurs in this slice. Public libraries remain 1.16.5; VS Code 0.2.5 still bundles 1.16.5. Braid must independently add capability discovery, exact formatted review/Apply, history and native acceptance. Original contract/request receipts and root concurrent edits are preserved.
