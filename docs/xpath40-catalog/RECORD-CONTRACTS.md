<div align="center">
 <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala record cast coercion and update delivery">
 <h1>Record casts, coercion and updates</h1>
 <p>22 October 2026 - source 1.23.6, unpublished</p>
</div>

The [new record-update entry](type-system/record-updates.md), expanded [structural-record comparisons](type-system/structural-records.md) and [complete stock-adjustment task](examples/record-contracts/README.md) supply46 public executable checks. These separate cast filtering/value conversion from coercion's surplus-field rejection, present empty fields from absent keys, and reconstruction from mutation. Typed-let/nested promotion, field order, annotation preservation and undeclared lookup/map:get behavior have bounded probes.

## Contract and version boundary

Current immutable review pin, refreshed22 October 2026: [expressions XML](https://raw.githubusercontent.com/qt4cg/qtspecs/b4723ff52fd9d4fabf2b4cd33bd44f21939c512c/specifications/xquery-40/src/expressions.xml), sections `id-structural-record-types`, `id-item-coercion-rules`, `casting-to-record` and `id-with-expression`. The matching grammar XML is retained. This is not a recovered historical adoption pin. Original delivery PR90 / `ca6e9c27cf4c62953763f800bdef2610ff64789a` already contains named record-coercion/update tests.

The new examples exposed [an inline-signature record version-gate leak](INLINE-RECORD-TYPE-GATE.md): signatures could create/coerce records in 3.0/3.1 because their retained type text bypassed the ordinary record gate. Sixteen dedicated API cases fail on the old parser. Source 1.23.6 applies the same record token gate while retaining signature text, including nested map/array/function types. Twelve ordinary node-name controls plus sixteen 4.0 cases already pass, and all 44 pass after the fix. Existing4.0 record value behavior is unchanged.

This is the only engine correction in the slice. Other inline type gates, general cast semantics, nominal static/default fields and exhaustive host/schema/module/package contracts remain outside the claim. Earlier3.0/3.1 code relying on record signatures now receives XPST0003; consumers using record types must opt into4.0. Public and Braid dependency remain1.23.4; no release is included.

## Executable evidence

There are35 XPath value/error probes, three explicit 3.1 refusal cases and eight complete3.0/4.0 reports. The normal report yields `BOLT:10->12|NUT:4->3|items=2;total=15`; empty/reordered/invalid fixtures retain exact output/error contracts. The3.0 baseline selects/casts map fields and uses map:merge; it reproduces the task rather than record annotations.

Initial catalog execution:41 pass/5 fail, all at the expected3.1 signature refusal boundary. After the parser fix:46/0/0, with expectations unchanged. Dedicated API regression:16 fail/28 pass before,44/0/0 after. The initial test-build namespace-import error and its correction are retained separately; the executed red regression is not replaced by that build failure.

Final validation: **10,636 solution tests pass**, including 7,624 XSLT and 2,843 documentation cases (2,819 catalog plus24 chapters), with no failures/skips. All84 additional language-server checks pass. QT3 stays **31,142/0/679** and qt4 gated **8,278/0/1,894**, without fixture or gate-membership changes. Build succeeds with five warnings: four NU1900 audit-network warnings and the existing CS8604 conformance-runner warning; no successful vulnerability audit is claimed. [Machine-readable audit](RECORD-CONTRACTS.json) records exact case IDs, source/fixture hashes, original receipts and gate totals. New cases are appended without changing earlier manifest rows or text.

## Acceptance and next scope

Current catalog:73 entries,2,819 feature +24 chapter checks =2,843; four chapters and117 assigned function names. The [current matrix/inventory](COVERAGE-RECONCILIATION.md) reflects the supplied record entry and bounded contracts. The approved applicable-evidence AC-03 policy is used explicitly: named tests/examples, no feature-specific qt4 gated-set assignment and no automatic entry/criterion acceptance.

REQ-126 remains In Progress; D2/D3/D4 and AC-03 remain partial. Next bounded catalog scope: binary/hex literals and underscore separators. Complete nominal/default/static fields, broader type/host contexts and historical adoption pins remain separate work.

This branch is based on the fixed PR410 head `7f84f7156639f6c42b82073b03cbadd69ba10864`; PR410's green10630-test CI covers only reconciliation/focus syntax and does not validate this later parser change. Preserve its independent evidence and review head. The record slice remains local until independently reviewed; no main sync, tag, package publication, Braid update or consumer acceptance is inferred.
