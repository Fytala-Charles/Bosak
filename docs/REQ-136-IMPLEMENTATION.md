<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala literal result element lifecycle implementation">
  <h1>REQ-136 literal result element lifecycle</h1>
  <p>23 October 2026 — source implementation; public release and consumer acceptance pending</p>
</div>

The owner approved the consumer-reviewed design on 23 October 2026 (`Approve`). Both Rename and whole-subtree Delete are implemented together on `feature/req136-element-lifecycle`, based on reviewed commit `3f099b5eca2e40d18358581924a373f2a8c9c6d3`. Source version is **1.24.0, unpublished**; published 1.23.8 has neither new API. Existing Create and other authoring operations retain their behavior. REQ-136 is In Progress until implementation review and delivery; Braid adoption and native acceptance remain separate.

The [maintainer proposal](REQ-136-MAINTAINER-SCOPE-REVIEW.md) is retained unchanged (SHA-256 `8b27d99dbf0f7281b9a8eec3a5e52d8ca1b634b67396388a800883d34628f0ac`). Braid found no blocking consumer-design findings within that bounded profile. The [exact consumer review](req136-evidence/BOSAK_REQ136_CONSUMER_DESIGN_REVIEW.txt) is retained without rewriting its historical status or original links.

## Public contract

`AuthoringCapabilities.SupportsLiteralElementRename` and `SupportsLiteralElementDeletion` discover the separate source capabilities. On an owned snapshot:

```csharp
var discovery = snapshot.GetLiteralElementRenameTarget(actualElement);
if (discovery.Target is { } target)
{
    var result = snapshot.ProposeLiteralElementRename(
        new AuthoringLiteralElementRenameProposal(target, "renamed"));
    // Inspect result.Outcome: Changed, NoChange or Refused.
    // Only result.Candidate carries changed source and fresh descriptors.
}

var removal = snapshot.GetLiteralElementDeletionTarget(actualElement);
if (removal.Target is { } deleteTarget)
{
    var result = snapshot.ProposeLiteralElementDeletion(
        new AuthoringLiteralElementDeletionProposal(deleteTarget));
    // result.Candidate or result.Failure; no unwrap or formatting option.
}
```

Targets have internal constructors and retain actual snapshot/descriptor identity. Equal-byte snapshots and equal numeric IDs confer no authority. Proposals are pure and re-prove original eligibility. The exact name is an XML NCName, without trimming, case folding or normalization. Equal ordinal names yield eligibility-checked NoChange with no candidate, edits, fresh ports or Apply authority. Cancellation yields a typed refusal.

Changed candidates expose input and fresh snapshots, operation, original element, actual `CandidateSelection`, `SourceEdits`, complete `NodeCorrespondence`, per-owner `AttributeCorrespondence`, `RemovedNodes`, `RemovedAttributes` and `MergedTextGroups`. Rename selects the renamed element; Delete selects its surviving direct parent. All evidence collections wrap private storage; exports return independent arrays. Every edit uses original coordinates in the same original module; its replacement range uses candidate coordinates including earlier deltas. Replay in descending original-offset order or copy untouched original intervals. Paired rename produces two edits in one candidate; self-closing rename and Delete produce one. Delete's replacement range is a zero-byte anchor.

## Exact supported vocabulary and validation order

The initial host is the existing Legacy authoring compiler with an explicit effective 3.0/4.0 `stylesheet`/`transform`, one dependency-free principal module and strict UTF-8 (optional BOM) or BOM-marked matching UTF-16LE/BE. Target ancestry is a top-level template through literal-result elements only. The target must have empty expanded namespace; its subtree may contain proven literal result descendants, constant attributes, ordinary text, comments/PIs and the covered instructions below. Alternate XSLT prefixes, prefix rebinding and default-namespace resets use expanded names. Namespace declarations remain byte-exact.

The streaming whole-module preflight uses a closed instruction/attribute vocabulary before compilation:

| XSLT instruction | Unqualified attributes admitted to subsequent validation |
|---|---|
| `stylesheet`, `transform` | `version`, `id`, `exclude-result-prefixes` |
| `output` | `method`, `version`, `encoding`, `omit-xml-declaration`, `standalone`, `indent`, `media-type` |
| `template` | `match`, `name`, `priority`, `version`, `exclude-result-prefixes` |
| `param` | `name`, `select`, `required`, `tunnel` |
| `variable` | `name`, `select` |
| `function` | `name`, `override` |
| `value-of` | `select`, `separator` |
| `sequence`, `for-each`, `comment` | `select` |
| `if`, `when` | `test` |
| `processing-instruction` | `name`, `select` |
| `text`, `choose`, `otherwise` | None |

Namespace declarations and `xml:space="default"` are admitted. Instruction `expand-text="no"/"false"/"0"` and `disable-output-escaping="no"` on text/value-of are admitted, preserving existing insertion output. Other instruction attributes refuse. Literal result controls admit only XSLT `version` and `exclude-result-prefixes`; unqualified ordinary attributes must pass the shared complete constant-AVT recognizer, including doubled literal braces. Other qualified result attributes refuse. Admission to this table does **not** waive context, value, signature or complete stylesheet validation.

The shared restricted expression recognizer accepts literals, variables, context/QNames, known-signature named calls/references, parentheses, binary/unary/if/sequence/range expressions, paths/steps/predicates/lookups, map/array constructors, for/let and quantified expressions. It checks namespace prefixes and rejects unknown signatures, keyword calls, typed/schema node tests and other AST shapes. Existing patterns use that bounded recognizer and pattern namespace validation. All original slots participate; deletion cannot repair an invalid original.

Dependencies, schema/package acquisition, static/shadow/use-when evaluation, namespace-alias, components/modes/types/extensions, expanded text, preserved space, CDATA, enabled disable-output-escaping and opaque instructions refuse before resource-capable compilation. Original and candidate validation use the controlled deny-resource compiler; proposals execute no transformation. A captured consumer module resolver is never consulted by these operations. Candidate inspection also receives a denying resolver and skips compilation until preflight and scope checks pass. Unexpected programming/infrastructure exceptions are not converted into unsupported-input failures.

## Bounds and provenance

Independent public limits are 8,388,608 bytes for each source; 1,024 UTF-16 units and 1,024 retained-encoding bytes for a requested name; 65,536 nodes and 65,536 attributes for each graph; maximum depth 256; and 16,777,216 bytes of complete review payload. Streaming XML depth/token/attribute preflight runs before candidate inspection; graph traversal is iterative before recursive proof/validation. Checked arithmetic covers edits, extents and physical growth. Complete payload is conservatively budgeted before candidate allocation; sources below the individual byte ceiling may still refuse the evidence ceiling. Nothing is truncated.

Rename edits only actual opening/closing name tokens. Delete removes the exact full element extent, including all owned attributes, instructions, text, comments/PIs and descendants. Parent delimiters and outside bytes remain unchanged; an emptied paired parent stays paired. The proof checks kinds, expanded/lexical names, logical text, parent/child order, all attribute metadata/order and translated ranges. Original nodes partition into removed subtree, ordinary survivors and merge contributors; candidate nodes partition into survivor images and actual merged nodes. All original attributes belong either to removed owners or exhaustive per-owner survivor maps.

At most one merge of the immediate whitespace-only text siblings around the removed element is supported. It exposes actual original descriptors and the actual inspected candidate descriptor, with complete ordered unchanged raw fragments exhausting its range. XML inspection owns CR/LF/entity normalization. Non-whitespace and other unproved merges refuse; this does not reduce populated deletion to empty-only deletion.

## Verification and consumer handoff

Initial focused run: 62 passed / 2 failed. It found an empty-name exception and rejection of existing text insertion's explicit `expand-text="no"`; both were corrected without weakening expectations. The expanded focused run passed 71/0/0; two additional unchanged-output cases bring the final focused matrix to **73/0/0**. Final full solution: **11,009/0/0** across ten projects; separate language-server: **84/0/0**. Fresh mandatory gates remain **QT3 31,142/0/679** and **qt4 gated 8,278/0/1,894**, with zero gated failures. [Machine-readable verification](REQ-136-IMPLEMENTATION-REVIEW.json) retains exact receipts and SHA-256 values. Original logs/TRX remain under `.guard-tmp/req136-*`.

The tests cover imported paired/self-closing forms under both versions and four encodings, exact multi-edit replay, populated/nested deletion and complete partitions, Unicode/ordinal intent, NoChange, foreign authority, profile refusals, namespace rebinding/resets, physical/depth/count/evidence limits, whitespace entities/CRLF, concurrent proposals, defensive exports and the full Create/populate/Rename/Delete/Create sequence with an ordinary execution golden.

LE01–LE13 engine contracts and LE16 engine lifecycle have source tests; LE14–LE15 session/license/revision/atomic Apply/history and LE17–LE18 public package provenance, worker/export/reopen/native acceptance require subsequent delivery and Braid integration. Consumer implementation review is required before merge/publication. Neither a source test nor the earlier 1.23.8 smoke establishes acceptance of unpublished 1.24.0.

Verification environment: the first restricted full attempt could not start existing database HttpListener fixtures; a diagnostic and all original logs were retained. Loopback-capable execution passes those tests. A subsequent final rerun initially omitted the temporary frozen-fixture junctions after gate cleanup; its failures are retained separately, and the corrected final run passes with fixtures restored and cleaned afterward. Restricted restore NU1900 warnings were cleared by a successful fresh network-enabled restore. The earlier build retains the existing conformance-runner CS8604 warning; no new source warning was introduced. These corrections changed verification setup, not engine expectations.
