<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala literal-text authoring implementation">
<h1>REQ-131: literal-text insertion implementation</h1>
<p>Source-only slice — 17 October 2026</p>
</div>

## Delivery state

**1.19.0 published and verified - 18 October 2026:** Immutable `v1.19.0` at27691fdb supplies bounded REQ-131 literal-text insertion. [Public delivery proof](RELEASE-1.19.0-PUBLICATION.md) records one successful Release run (9,726 tests/ten uploads), ten verified signed archives and fresh nuget.org-only153 broad/282 focused checks with exact provenance and clear audits. REQ-131 Braid adoption/functional/native acceptance remains pending; accepted1.18.0 pin and extension are unchanged. Earlier preparation-only statements below are historical.

**1.19.0 release prepared ? 17 October 2026:** [Braid?s unchanged implementation review](REQ-131-CONSUMER-IMPLEMENTATION-REVIEW.md) reports no blocking finding at exact head189405f2/merge27691fdb, with overlapping291/282/298 checks (16 new probes within298). [Release readiness](RELEASE-1.19.0-READINESS.md) and [prepared notes](RELEASE-NOTES-1.19.0.md) target immutable27691fdb. Source1.19.0 is unpublished; publication approval and separate Braid integration/native acceptance remain open. Accepted REQ-130 stays on public1.18.0; no Braid pin or extension change is included.

**REQ-131 source delivery complete ? 17 October 2026:** [PR #364](https://github.com/Fytala-Charles/Bosak/pull/364) merged as `27691fdb3708bb4ffd7c984abbf3196b7cde5667`; final-head and integrated CI each pass 9,810 checks. Local focused 291 and frozen QT3 31,142/0/679 / qt4 gated 8,278/0/1,894 pass. Independent exact-merge local packages pass 282 public-API checks with exact ten-archive/nine-assembly provenance and no vulnerable dependencies. Source **1.19.0 remains unpublished**. [Independent consumer review handoff](REQ-131-CONSUMER-REVIEW-HANDOFF.md) is next; no Braid/native acceptance is inferred.

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

Full solution/language-server/frozen gates pass. Independent exact-source-head, final-head and integrated package-only proof and both CI runs pass. Four new internal cancellation controls, four deletion proof controls and two legacy traversal controls remain source-only; independent consumers must use public APIs and a distinct assembly identity. T23–T25 Braid review/Apply/selection/history/worker/export/reopen/native acceptance are independently required after engine delivery; REQ-129/REQ-130 acceptance cannot be borrowed. Source release/publication is separate.

[Reviewed contract](REQ-131-TEXT-INSERTION-CONTRACT.md), [25-group acceptance matrix](REQ-131-TEXT-INSERTION-ACCEPTANCE.md).

## Observed local verification

Full Release build succeeds with zero errors and inherited unrelated warnings. **9,726 solution tests**, **84 language-server tests** and **291 focused cases** pass, zero failed/skipped. Frozen QT3 remains **31,142/0/679**, qt4 gated **8,278/0/1,894** with zero gated failures. Independent package-only verification passes **282 cases** (61 text, 45 deletion, 175 legacy insertion/readable, one metadata check), with zero failures/skips. Ten local archives and nine restored assemblies match revision `bd73238d3cc2187c09a14bb18d09ea899ab1cd72`, including archive/cache/output bytes, SHA-512 assets and restore-source receipts; dependency audit reports no vulnerabilities. Ten internal controls remain source-only. Final-head CI39035190436 and integrated CI39035670638 both pass 9,810 checks. Separate exact-final-head and exact-merge package consumers each pass the same 282-case suite; their ten archives/nine assemblies match their respective revisions and restore/cache/output bytes, with zero vulnerable dependencies. No public 1.19.0 packages exist or are claimed.

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
