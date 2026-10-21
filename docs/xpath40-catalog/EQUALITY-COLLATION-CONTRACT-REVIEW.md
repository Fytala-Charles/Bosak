<div align="center">
<img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak Equality and Collation Contract Review">
<h1>Equality and Collation Contract Review</h1>
<p>21 October 2026</p>
</div>

**Index-of correction - 21 October 2026:** Source1.23.2 [defaults an empty collation only in4.0](INDEX-OF-EMPTY-COLLATION.md), retaining XPath3.1 behavior. The manifest reproducer now verifies result1 plus the3.1 XPTY0004 baseline. Original source5de2cfc5 findings/hashes below remain historical; deep-equal options and unrelated keyword findings remain open. No new release.


Reviewed 21 October2026 against source `5de2cfc5213529cdef419b9b7dec3cf9b6c03f7f` (unpublished1.23.1; public 1.23.0), with the unchanged catalog runner. [Pinned draft declarations](https://github.com/qt4cg/qtspecs/blob/3230573afe246f1b502ed2e22d187ce9928155b1/specifications/xpath-functions-40/src/function-catalog.xml) at `3230573afe246f1b502ed2e22d187ce9928155b1` have SHA-256 `feafdd1dff45c323e371ab5a9d0d3386a470bf719bab02a3826910e9b5985010`. [Dated F&O3.1 declarations](https://www.w3.org/TR/2017/REC-xpath-functions-31-20170321/function-catalog.xml) have SHA-256 `0a387a2a160d608788e293479412fc4b43152a07193184b315e6323b799426da`. This is a current contract comparison; no historical adopted-specification pin is recovered.

58 new checks comprise 52 focused function/mode boundaries and six full customer reports. Current catalog: **68 entries,2,636 feature checks +24 chapter checks =2,660**; four worked chapters and117 assigned4.0-only names are unchanged. REQ-126 remains In Progress. No source/API/test-harness/version/gate changes or release.

## Declaration and metadata comparison

`distinct-values` takes atomic values and an optional collation; `index-of` takes atomic input, one required target and optional collation; `contains-token` takes strings, one required token and optional collation. The current draft makes those collation defaults apply on an explicit empty sequence. `compare` changes3.1 string operands to optional atomic operands in 4.0 and calls them `value1`/`value2`. `deep-equal` changes its third argument from3.1 string collation to optional string-or-map options. Exact arguments/types/defaults and current registered arities are recorded in the [audit](audit-equality-collation-contracts.json), rather than inferred from overload availability.

| Family | Current implemented keyword names | qt4 membership |
|---|---|---|
| fn:distinct-values | No keyword metadata | Not gated |
| fn:index-of | input, target, collation | Not gated |
| fn:deep-equal | No keyword metadata | Not gated |
| fn:compare | comparand1, comparand2, collation | Gated |
| fn:contains-token | value, token, collation | Gated |

Gate membership is read from current `GatedSets`, not inferred from the presence of a test-set file. Each family has a matching QT3/qt4 set, but only the two stated qt4 sets are gated. No fresh manual conformance run is claimed by this documentation-only review; no promotion occurs. [Known Uses](functions/customer-reference-matching.md#15-known-uses) links exact sets and runners.

[Separate engine follow-up record](EQUALITY-COLLATION-ENGINE-FINDINGS.md) keeps the two minimal reproductions outside the supported consumer contract.

## Engine findings kept separate

1. `index-of(('A','a'),'A',())`: pinned4.0 default-on-empty expects position1 in a default-codepoint context; current positional#3 raises XPTY0004. `IndexOf_3` calls `RequireStringRequired`.
2. `deep-equal('A','a',map{'collation':'http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive'})`: pinned4.0 expects true; current third argument raises FOTY0013 while atomizing the map. It does not implement the full draft options form.
3. `compare(value1 := 'A',value2 := 'A')` refuses XPST0017, while implemented `comparand1`/`comparand2` succeeds. This spelling drift was already documented in [measurement comparison](functions/measurement-comparison.md); this review refreshes the evidence without treating it as a new discovery.
4. No keyword metadata exists for the tested distinct-values/deep-equal forms. Their4.0 refusals remain coverage findings, not approved consumer contracts.

These are bounded findings, not an exhaustive defect inventory. Use supported valid arguments in the complete report. The [audit](audit-equality-collation-contracts.json) retains separate normative outcomes, observed errors, first-run hashes and final case IDs; no expectation is changed silently.

## Original run and fixture corrections

The first run is53/5/0. Two failures establish the empty-collation boundary and actual options-map error above. Three failures are missing `frozenExpected` metadata for successful experimental keyword cases: the runner otherwise expects rejection in frozen 4.0. Adding that metadata corrects the test precondition. Initial logs/TRX/manifest remain under `.guard-tmp/req126-equality-*`; the audit preserves their hashes and original failure names. Final focused58/0/0 verifies supported examples and explicitly labelled current observations. Counts overlap full-suite coverage.

## Consumer policy and remaining queue

The [complete customer example](examples/equality-contracts/README.md) states synthetic inputs, output, empty/reordered behavior, exact token matching, ASCII-only folding and explicit Unicode normalization. Its3.1/4.0 variants agree without using unsupported draft options or empty collation arguments. No distinct representative order, fuzzy identity or broad normalization is promised.

This supplies the next five-family D2 review and one dedicated family entry. D2's full signature/default/options coverage, D3 grammar/type/pattern mapping, D4 historical provenance and AC-03 acceptance reconciliation remain partial. A suitable next bounded documentation slice maps the already delivered switch/array/select grammar and version gates to entries/tests; it is not authorized engine work or a promise that the current draft is fully implemented. REQ-126 is not closed and hash-only PR132 is untouched.


**Validation:** Release build succeeds with the pre-existing conformance Program.cs:1868 CS8604 warning. All **10,304 solution tests pass**, including 7,363 XSLT tests and2,636 manifest rows. The 58 new focused checks overlap these totals. [Verification receipt](EQUALITY-COLLATION-VERIFICATION.json) preserves all ten TRX hashes. Manual engine gates were not rerun: engine/harness source, gate membership and package version are unchanged.
