<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak complete JNode sum atomization">
  <br><br><h1>Complete input atomization for fn:sum</h1>
  <p>Every wrapped value contributes; emptiness is tested after atomization</p>
</div>

## Bug and correction - 16 October 2026

Unpublished source candidate **1.14.1** fixes both arities of `fn:sum`. In the unchanged native array-020/arrays-020 scenarios, array:members supplies a JNode wrapping `(5,6)`. The scalar atomizer recursively reached that sequence and retained only its first item, producing 5. Both native assertions correctly require 11.

The two sum entry points now use the existing sequence-aware `Data` atomizer before materialization and empty-input handling. The established sum arithmetic/type-promotion implementation is retained. This includes all values from wrapped sequences, recursively handles JNodes and arrays, preserves list-typed node values, omits empty atomized values, and rejects non-atomizable maps/functions. The one-argument form returns integer zero for atomized-empty input; the two-argument form returns its supplied zero value. A nonempty input summing to zero does not select the fallback.

```xpath
sum(jtree((5,6)))             (: 11 :)
sum(jtree(()))                (: 0 :)
sum(jtree(()), 99)            (: 99 :)
sum(jtree((-1,1)), 99)        (: 0 :)
sum(array:members([1,(5,6),10])) (: 22 :)
```

Later invalid items can no longer disappear behind the first item: a wrapped `(5,'bad')` raises FORG0006, while a wrapped map raises FOTY0013. Existing duration and untyped-numeric summation behavior applies to every atomized value.

No public API, array member function, JNode model or shared scalar atomizer is changed. This slice does not claim to correct the corresponding behavior of avg/min/max or every other scalar-atomization caller. Those need separate review and controls.

## Draft and upstream review

Refreshed qtspecs remains `72f222929768b411d8e88d96a1a8740483290078`. The pinned [XPath atomization rules](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xquery-40/src/expressions.xml#L3929) require atomization of the JNode's entire jvalue. The locally retained expressions.xml blob is `b13e9f35f86c3b5bb318d07d81c085534e8ca2df`. The [sum signature/rules](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xpath-functions-40/src/function-catalog.xml#L18580), catalog blob `930edc7ade2bdf7afecdf8019b93e5b450d9f10e`, specify an atomic sequence argument and zero handling after conversion. The live rendered XPath page could not be fetched; review used retained pinned primary XML.

Unchanged xslt40-test remains `91328fbf0fed4a5d2813eb3661808ee53cb961ba`. Both upstream stylesheets are identical in the relevant scenario, and their original assertions require in values 1,11,10. Before correction Bosak returned 1,5,10. The upstream files and expected results are unchanged.

| Native selection | Before pass/fail/skip | After pass/fail/skip |
|---|---|---|
| array substring aggregate | 144/16/20 | 146/14/20 |
| array instruction set | 15/14/8 | 16/13/8 |
| sx-SquareArrayConstructor | 56/0/0 | 56/0/0 |
| arrays type set | 73/2/12 | 74/1/12 |
| record aggregate | 29/35/1 | 29/35/1 |
| type-alias | 22/9/1 | 22/9/1 |

Only array-020 and arrays-020 newly pass; no prior pass is lost. All three family selections retain exit1. Other serialization/profile/result issues remain failures. No native expectation edit, new skip/error alias, upstream contribution, gate promotion or family-wide conformance claim occurs.

The exact 20-case Standard baseline produced 2 pass/18 fail, and both XSLT scenario controls failed before correction. All assertions are retained. After the two-line behavioral correction, Standard focused cases total89 (new20 plus existing member/JNode cases), and both new XSLT scenario cases pass. Original fixture bytes, before/after logs, source hashes and native receipts are recorded in [machine-readable evidence](XSLT40-JNODE-SUM.json).

PR324 exact-head CI and integration metadata are recorded in the receipt. Source1.14.1 remains unpublished; public libraries1.8.0 and VSCode0.2.4/engine1.8.0 are unchanged, as are REQ statuses and independent Braid acceptance.

Next bounded review: the remaining arrays-021 static diagnostic against the current xsl:for-each rules before changing error precedence. Other aggregate/JNode atomization callers and serialization/profile failures remain separate.

## Verification

Release build passes with zero errors and two incremental warnings. Solution 8,916/0/0 (Standard1,351; XSLT5,982), language server84/0/0, focused Standard89/0/0 plus XSLT2/0/0. Frozen QT3 31,142/0/679 and qt4 gated8,278/0/1,894, both exit0. Native array aggregate improves to146/14/20, with only array-020/arrays-020 newly passing and no lost passes; record29/35/1 and type-alias22/9/1 remain unchanged. All family exits remain1 because failures remain. All local jobs have completed. Canonical docs are rendered with markdown-it and Fytala CSS; kit-managed assets remain unchanged.
