<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala literal-text authoring implementation">
<h1>REQ-131: literal-text insertion implementation</h1>
<p>Source-only slice — 17 October 2026</p>
</div>

## Delivery state

Unpublished source **1.19.0**, branch feat/req131-literal-text-insertion, base/current published-source documentation main `38221050986355db78b31f2679216a81a621379d`. The user authorized contract review and bounded implementation while Braid integrates REQ-130. No publication, Braid source/pin/UI change or extension update is included; public packages remain 1.18.0 and independent consumer/native acceptance remains open.

## Reviewed compatibility decision

The earlier contract said xsl:text was absent from the value-of allow-list. Actual 1.18.0 source and published-API probes disprove that assumption. Plain literal text, disable-output-escaping=yes text, and whitespace-only expand-text=yes text already permit value-of insertion; significant expanded text refuses UnsupportedCoverage. Preserve these behaviors rather than tighten the shared validation/deletion boundary. New text insertion has an independent narrower check over existing xsl:text: ordinary text-only body, no effective expansion, and absent/no disable-output-escaping. Comments/PIs/CDATA/elements inside an existing text instruction are outside this new-operation profile; old value-of behavior is unchanged. The original four-probe receipt remains `.guard-tmp/req131-baseline-probes.log`.

Shared source-derived scope/static/resource validation remains bounded to one explicit supported principal stylesheet, eligible template/literal-result-only chain and explicit closing parent. Effective preserving/mixed/CDATA parents refuse; Readable also retains the any-ancestor-preserve/default-reset guard. This adds no wider language conformance or arbitrary XML fragment API.

## Public API and literal semantics

Independent capability flags SupportsTextInsertion/SupportsReadableTextInsertion and MaxTextInsertionSourceBytes (8,388,608 original/candidate bytes including BOM). Discover `snapshot.GetTextInsertionTarget(parent, token)`, then `snapshot.ProposeTextInsertion(new AuthoringTextInsertionProposal(target, textValue[, formattingMode]), token)`. Targets are opaque and typed separately from value-of; source ownership is by exact snapshot reference. Existing immutable snapshots remain valid on their own; only a consumer knows its current revision. Null programmer arguments/undefined modes throw; empty text is valid. Cancellation is a classified insertion Cancelled result, unlike deletion's throwing cancellation contract. InvalidPayload is appended after existing failure enum values without renumbering them.

Payload is unnormalized literal Unicode, never XPath/AVT/TVT/markup. Validate scalars with cooperative cancellation, reject XML-invalid BMP characters and unpaired surrogates, count encoded/escaped bytes before allocating the escaped payload or output buffer. Escape ampersand/less-than/greater-than; represent TAB/CR/LF as numeric references, keeping a logical CRLF pair intact and out of physical source layout. Non-BMP/combining/U+FEFF characters remain literal. Choose an existing deterministic nonempty XSLT prefix or declare a collision-free prefix on the new instruction only. Emit paired `<prefix:text expand-text="no">payload</prefix:text>` even when empty; no DOE option.

PreserveSource uses the same append anchor and touches no original bytes. Explicit Readable reuses the existing direct-child gap plan and counts its complete footprint under the independently supplied text limit; no fallback or payload formatting. The original value-of layout limit/default is unchanged. Original/candidate validation and fresh ordinary inspection use denied resolution and never execute the payload. No source serialization or external access is performed by a proposal.

## Candidate evidence and operation-specific mapping

InputSnapshot, fresh valid Snapshot, ModuleUri, FormattingMode, TextValue, OriginalRange zero-length anchor, InsertedRange full instruction, TextPayloadRange exact encoded body, InsertedNode, optional actual PayloadNode, immutable complete ordered SourceEdits and defensive ExportSource(). Empty text uses zero-length payload range/null PayloadNode without an invented source node/select slot. Nonempty payload has exactly one ordinary text descriptor whose Text equals logical payload and Range equals encoded source content. Full instruction subtree is explicitly new; additional layout whitespace is explained by SourceEdits.

As with value-of insertion, **text NodeCorrespondence is a total injective dictionary of old IDs to candidate IDs**, scoped to the two owning snapshots. This differs from deletion's descriptor-reference map/merge groups: adapters must not interchange the operation-specific schemas. Shared provenance excludes the full newly generated subtree for text; legacy value-of still excludes only its single new instruction. Every old descriptor (including old literal payload/changed whitespace/ancestors) maps once, and unmatched candidate nodes outside the new subtree can only be source-edit-covered formatting whitespace. Missing/duplicate/merged/ambiguous old evidence refuses. Source edit replay and untouched bytes are independently checked in fixtures.

## Verification checkpoint

Release build passes with existing unrelated warnings. Initial 44 text cases pass; expanded suite now **291 focused cases pass**: **65 text +177 value-of/readable +49 deletion**. Text evidence includes eleven independent payload/source goldens under both captured profiles, four encodings/both modes, inherited expansion overrides, exact Unicode/control output, repeated text/value-of/deletion workflows, measured legacy text compatibility, default/alias/collision bindings, exact source replay/complete graph ownership/read-only evidence, inclusive UTF-8/UTF-16 byte and escaping amplification limits, original limit, readable shrinkage, protected old text/comments/PIs/descendants and newline styles, profile/resource/mixed/preserved/refusal cases, immutable concurrent input reuse and four deterministic target/proposal traversal cancellation controls. No failing text API run is claimed or hidden; the initial contract assumption was corrected from baseline evidence before implementation.

Full solution/language-server/frozen gates and exact-head/integrated independent package-only proof are in progress and must pass before source integration. Four new internal cancellation controls, four deletion proof controls and two legacy traversal controls remain source-only; independent consumers must use public APIs and a distinct assembly identity. T23–T25 Braid review/Apply/selection/history/worker/export/reopen/native acceptance are independently required after engine delivery; REQ-129/REQ-130 acceptance cannot be borrowed. Source release/publication is separate.

[Reviewed contract](REQ-131-TEXT-INSERTION-CONTRACT.md), [25-group acceptance matrix](REQ-131-TEXT-INSERTION-ACCEPTANCE.md).

## Observed local verification

Full Release build succeeds with zero errors and inherited unrelated warnings. **9,726 solution tests**, **84 language-server tests** and **291 focused cases** pass, zero failed/skipped. Frozen QT3 remains **31,142/0/679**, qt4 gated **8,278/0/1,894** with zero gated failures. Exact-head CI and independent public-API-only local-package proof remain integration gates; no public 1.19.0 packages exist or are claimed.

```csharp
var targetResult = snapshot.GetTextInsertionTarget(parent, cancellationToken);
if (targetResult.IsSuccess)
{
    var proposal = new AuthoringTextInsertionProposal(
        targetResult.Target!, "Total: ", AuthoringInsertionFormattingMode.Readable);
    var result = snapshot.ProposeTextInsertion(proposal, cancellationToken);
    if (result.IsSuccess)
    {
        var candidate = result.Candidate!;
        var reviewBytes = candidate.ExportSource();
        // The consumer freezes/reviews all evidence and explicitly Applies under current authority.
    }
}
```

The example requests Readable explicitly; omitting the mode retains PreserveSource. Handle classified refusals, including cancellation, without silent fallback. An empty logical string still selects a real new instruction even though PayloadNode is null.
