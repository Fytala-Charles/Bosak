<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak raw initial-template delivery">
  <br><br><h1>Raw initial-template results</h1>
  <p>Untyped entry results and nested template delivery</p>
</div>

## Diagnosis and contract - 16 October 2026

Unpublished source candidate **1.14.3** corrects the existing `XsltExecutable.Transform(..., rawResult: true)` behavior for named initial templates without an explicit return type. The native array-010/011/012 scenarios declare `<output tree="no" result-var="result"/>`; the runner already honors that declaration by passing rawResult:true. They require an XDM array, not serialized XML. The engine previously captured initial-template output only when an `as` attribute triggered typed sequence collection. Without `as`, top-level arrays fell into ordinary serialization and raised SENR0001.

A pending raw-entry capture is now claimed once by the initial template invocation. An untyped entry uses the existing item()* sequence-constructor collection path. Existing explicit return-type conversion remains in force. Nested and recursive typed calls deliver into their caller's sequence rather than overwriting the initial result. No renderer, serializer, conformance adapter or public signature changes are made.

```csharp
var value = executable.Transform(null, initialTemplate: "start", rawResult: true);
// A start template returning xsl:array yields an XdmValue whose IsArray is true,
// whether or not the template declares an as type.
```

Arrays retain their member sequences; maps and functions remain function items; empty output stays empty. Atomics are not converted into a document node. Constructed element/text nodes retain their node kind, and repeated source-node references preserve identity and order. Entry capture also works with an explicit source/match pattern, the implicit xsl:initial-template entry and repeated use of an executable.

Explicit return-type mismatch errors and original body errors still propagate. Ordinary document delivery and TransformToString continue through existing result-tree/serialization rules, including rejection of arrays under the default XML output method. Other delivery formats, secondary-result handling and JNode serialization are outside this bounded slice.

## Primary rules and native evidence

Reviewed qtspecs remains `72f222929768b411d8e88d96a1a8740483290078`, xslt.xml blob `59b9bdc79dca23b0c18fdd3e94130bc59037db3e`. The [post-processing rules](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L1393) distinguish direct raw-sequence delivery, result-tree construction and serialization. The native declarations and existing public XML documentation independently establish the requested API contract.

PR326 passed exact-head CI run38893551100 before integration. Refreshed unchanged xslt40-test remains `91328fbf0fed4a5d2813eb3661808ee53cb961ba`. The original tree=no metadata, result-variable bindings, assert-type and deep-equality assertions are unchanged.

| Native selection | Before pass/fail/skip | After pass/fail/skip |
|---|---|---|
| array aggregate | 147/13/20 | 153/7/20 |
| array instruction set | 16/13/8 | 22/7/8 |
| sx-SquareArrayConstructor | 56/0/0 | 56/0/0 |
| arrays type set | 75/0/12 | 75/0/12 |
| record aggregate | 29/35/1 | 29/35/1 |
| type-alias | 22/9/1 | 23/8/1 |

Exactly array-010 through array-015 and type-alias-050 newly pass, with no lost prior pass. The array cases exercise grouping, sorting/merging, nested arrays and variable/current() use; type-alias-050 returns an array of grouped maps. All aggregate family runs still exit1 because failures remain. Skips remain skips; no catalog/expectation edit, error alias, upstream contribution or gate promotion occurs.

The initial test source had quoting errors; original bytes/log remain retained. The first runnable16-case diagnostic fixture produced3 pass/13 fail. Additional atomic-kind/count assertions made false document-wrapper successes visible, giving the strengthened baseline2 pass/14 fail. No expected output was weakened. The corrected engine passes all16 controls, including nested typed calls, recursion, identity, body/return-type errors and the ordinary XML-delivery control. Source, fixture and execution hashes plus unchanged native receipts are in [machine-readable evidence](XSLT40-RAW-INITIAL-RESULTS.json).

Source1.14.3 remains unpublished. Public libraries1.8.0, VSCode0.2.4/engine1.8.0, REQ statuses and independent Braid acceptance remain unchanged.

Next bounded review: native array-017/018 JNode sequence-construction diagnostics and placement against the current draft. Profile-selection and other aggregate/JNode issues remain separate.

## Verification

Release build passes with zero errors and two incremental warnings. Solution8,949/0/0 (XSLT6,015), language server84/0/0, focused raw-entry controls16/0/0. Frozen QT3 31,142/0/679 and qt4 gated8,278/0/1,894, both exit0. Native array aggregate153/7/20 and type-alias23/8/1 improve by six and one passes respectively; record29/35/1 unchanged. No lost prior passes. All family exits remain1 because failures remain. All local jobs complete. Canonical docs are rendered with markdown-it and Fytala CSS; kit-managed assets remain unchanged.
