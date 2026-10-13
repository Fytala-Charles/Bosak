<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak sequence selection and repetition contract review">
  <br><br><h1>Sequence Selection and Repetition Contracts</h1>
  <p>Batch exports, keyword/default boundaries and typed positions</p>
</div>

## Scope and outcome

Reviewed 2026-10-12 against Bosak `2efcd33a483cb9adb8eae35908a083a661388b1d`, version 1.1.2. Six functions have representative keyword/default/empty/invalid and batch-export evidence. Five extend the targeted review of absent-keyword metadata; slice already has keyword metadata. **13 of the original 37 absent-metadata families are now reviewed; 24 await targeted review.** No metadata or engine behavior was changed. D2 remains partial and REQ-126 In Progress.

65 new checks: 18 explicit keyword/mode cases, 41 focused default/typed-source/identity/keyword-validation cases and six normal/empty/reordered 3.1/4.0 reports. Catalog remains 67 entries, now 1,621 feature + 24 chapter checks = 1,645. No API/harness changes or gate promotion.

Primary contract: [QT4CG F&O catalog e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml). Implementation: [FunctionLibrary](../../src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs). [Raw signature comparison](audit-signature-contracts.json) links each family's exact cases. This is a current review pin, not recovery of historical adopted specification revisions.

## Declared signatures and keywords

| Function | Pinned parameters/defaults | Result | Keyword support |
|---|---|---|---|
| `fn:foot` | `input: item()*` | `item()?` | Absent metadata: XPST0017 |
| `fn:insert-separator` | `input: item()*, separator: item()*` | `item()*` | Absent metadata: XPST0017 |
| `fn:items-at` | `input: item()*, at: xs:integer*` | `item()*` | Absent metadata: XPST0017 |
| `fn:replicate` | `input: item()*, count: xs:nonNegativeInteger` | `item()*` | Absent metadata: XPST0017 |
| `fn:slice` | `input: item()*, start: xs:integer? := 0, end: xs:integer? := 0, step: xs:integer? := 0` | `item()*` | Available in 4.0/experimental |
| `fn:trunk` | `input: item()*` | `item()*` | Absent metadata: XPST0017 |

All six keyword expressions refuse 3.1 grammar with XPST0003. In 4.0/experimental, replicate/items-at/foot/trunk/insert-separator refuse keyword binding XPST0017 because metadata is absent. slice keywords succeed: input plus step -1 yields C,B,A, and reordered end/input/start yields B,C. Missing input and unknown limit refuse XPST0017. Adding experimental mode does not enable the other five families' keywords or change their Frozen 4.0 classification.

## Positional and default contracts

replicate repeats an entire sequence, not each item consecutively. Count zero or empty input produces empty; negative/fractional count refuses XPTY0004 in the tested cases. Replicating a source node retains identity rather than constructing new nodes. The batch example repeats presentation labels for two stations; it does not execute dispatches twice.

items-at follows requested-position order and preserves duplicate positions. Out-of-range, zero and negative positions are ignored; empty input/positions produce empty. This differs from a membership predicate, which preserves source order and selects each item once. foot selects the final item; trunk removes it, preserving preceding order. Empty/singleton boundaries and map items are explicit. insert-separator can insert several items and leaves empty/singleton input or empty separator boundaries unchanged as shown in the fixtures.

slice defaults use zero for omitted or empty start/end/step. All-default calls return the input; negative bounds count from the end, inferred direction can reverse, explicit negative step reverses and positive stride skips positions. Mismatched explicit direction returns empty. End is inclusive; it is not the subsequence length. The examples use bounded sequences and make no constant-memory claim: several implementations materialize input/results.

## Typed-position and coercion review

| Case | Current outcome | Contract boundary |
|---|---|---|
| [replicate integral decimal](examples/sequence-contracts/replicate-integral-decimal.xpath) | XPTY0004 | Declared count is nonNegativeInteger; runtime rejects this integral decimal as well as fractional/negative counts. |
| [items-at integral decimal](examples/sequence-contracts/items-integral-decimal.xpath) | B | Declared positions are integer*; integral decimals are tolerated, fractions refuse. |
| [slice decimal bound](examples/sequence-contracts/slice-decimal-bound.xpath) | XPTY0004 | slice rejects integral decimal bounds as well as fractions and strings. Do not infer uniform integer handling across families. |
| [items-at node position](examples/sequence-contracts/items-node-position.xpath), [slice node bound](examples/sequence-contracts/slice-node-bound.xpath) | XPTY0004 | Tested untyped XML position nodes are rejected. Review atomization/coercion against full calling rules; use typed integers in supported examples. |
| [replicate node identity](examples/sequence-contracts/replicate-node-identity.xpath) | true | Repetition preserves node identity, not a copy operation. |

These checks preserve observed outcomes. The audit does not silently change them or certify the full coercion/cast contract. Type-level gaps and missing keywords remain separate engine follow-ups.

## Complete batch example and remaining work

The [dispatch-batch README](examples/sequence-contracts/README.md) shows complete XML, both versions' full expressions and exact normal/empty/reordered outputs. It explains inclusive pagination, final-record checkpoints, duplicate label requests, station repetition and source-order dependence. The [existing family entry](functions/order-sequence-selection.md) retains delivery/gate evidence and introductory examples.

Next continue D2 with sequence equality, distinctness and subsequence predicates: all-equal, all-different, contains-subsequence, starts-with-subsequence and ends-with-subsequence. Review keywords, optional collation/defaults, callback forms and empty/typed cases using a realistic shipment-event workflow. Other missing metadata, complete coercion/options, exhaustive changed-3.1 inventory and D3/D4 acceptance work remain. Version remains 1.1.2; no closure or release follows from these samples.


The subsequent [predicate contract review](PREDICATE-CONTRACT-REVIEW.md) supplies the five equality/distinctness/subsequence families. Current targeted absent-keyword coverage is 18 of 37; 19 remain, with metadata unchanged. Its 102 added checks bring coverage to 67 entries, 1,723 feature + 24 chapter checks = 1,747. Earlier delivery counts above remain historical. Next: duplicate-values/highest/lowest/index-where comparison and selection contracts; D2 remains partial.
