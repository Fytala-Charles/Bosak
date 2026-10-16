<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak array member JNode functions">
  <br><br><h1>Array member functions</h1>
  <p>JNode decomposition and reconstruction in XPath 4.0</p>
</div>

## Contract - 16 October 2026

Unpublished source candidate **1.14.0** adds `array:members($array as array(*)) as jnode()*` and `array:of-members($input as jnode()*) as array(*)`. Both require XPath 4.0 or later, including their named-function references and dynamic lookup visibility. Named arguments are `array` and `input` respectively. Existing built-in profile gates and type metadata enforce the signatures.

`array:members` returns one JNode per array member, in member order. Each wrapper retains the complete member value, including empty and multi-item sequences, nested arrays, maps or JNodes. The wrappers have one shared root parent that wraps the original array, and their keys are one-based member indexes. An empty array produces an empty sequence. A one-member array containing an empty sequence produces one JNode, not an empty result.

`array:of-members` constructs one member per input JNode from its wrapped value. It ignores keys and parents, retains order and duplicates, and does not flatten or atomize. Empty input produces an empty array. Every supplied item must be a JNode; non-JNodes raise XPTY0004. Existing member objects and nested array identities are preserved rather than deep-copied.

```xpath
array:of-members(array:members([(), (1, 2), [3]]))
(: returns [(), (1, 2), [3]] :)

array:of-members(reverse(array:members([1, 2, 3])))
(: returns [3, 2, 1] :)
```

These functions reuse the existing XdmJNode and XdmArray representations. No public C# API or JNode model change is introduced. Older map-record-based proposals for these functions are not implemented. JNode serialization, XSLT instruction/profile handling, additional array functions and JNode patterns remain separate.

## Primary draft review

Reviewed qtspecs remains `72f222929768b411d8e88d96a1a8740483290078`. The exact F&O function catalog blob is `930edc7ade2bdf7afecdf8019b93e5b450d9f10e`, saved with its original bytes. The live rendered F&O page exceeded the browser fetch size, so review used the pinned primary catalog: [array:members](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xpath-functions-40/src/function-catalog.xml#L30557) and [array:of-members](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xpath-functions-40/src/function-catalog.xml#L30706). The July 2026 change notes specify JNode input/output; parent/key properties and value reconstruction are explicit in those rules.

## Evidence and remaining failures

PR323 passed exact-head CI run 38889429791 before integration. Refreshed xslt40-test remains `91328fbf0fed4a5d2813eb3661808ee53cb961ba`; the detached suite is unchanged. The previous immutable array-family receipt supplies the before counts. Current runs preserve native stylesheet versions and exact expected errors.

| Selection | Before pass/fail/skip | After pass/fail/skip |
|---|---|---|
| array substring aggregate | 143/17/20 | 144/16/20 |
| array instruction set | 15/14/8 | 15/14/8 |
| sx-SquareArrayConstructor | 56/0/0 | 56/0/0 |
| arrays type set | 72/3/12 | 73/2/12 |
| record aggregate | 29/35/1 | 29/35/1 |
| type-alias | 22/9/1 | 22/9/1 |

Only unchanged `arrays-006` newly passes; no prior pass is lost. All three selections retain exit 1. Array-013/015 now reach serialization errors rather than missing functions; array-020 and arrays-020 reach result mismatches; array-021/022 expose 3.1 profile selection rather than unknown-function errors. These remain failures, not passes or skips. No catalog/expectation change, error alias, upstream contribution or gate promotion occurs.

The original fixture had a compile error from treating the struct sequence enumerator as LINQ IEnumerable; that log and exact initial bytes remain preserved. The corrected 26-case baseline produced 4 pass / 22 fail. Implementation plus the existing JNode suite passed 67 cases; two dynamic lookup controls bring the final new fixture to 28 cases and the combined selection to 69. Cases cover empty/multi-item members, nested identity, shared parents and keys, reconstruction order/duplicates, type errors, named arguments and profile gates.

Original logs, native receipts, source/fixture hashes and primary source hash are in [machine-readable evidence](XSLT40-ARRAY-MEMBER-FUNCTIONS.json). Public libraries remain1.8.0; VSCode0.2.4 bundles engine1.8.0. No publication, installed-extension change, REQ status change or independent Braid acceptance occurs.

Next bounded review: inspect native arrays-020 and array-020 result mismatches against the pinned current draft and original expected results before changing engine behavior. Serialization/profile discrepancies remain separately recorded.

## Verification

Release build passes with zero errors and 25 warnings. Solution 8,894/0/0 (Standard 1,331; XSLT 5,980), language server 84/0/0, focused member/JNode cases 69/0/0 (28 new cases). Frozen QT3 31,142/0/679 and qt4 gated 8,278/0/1,894, both exit 0. Native array aggregate improves to 144/16/20, with only arrays-006 newly passing; record29/35/1 and type-alias22/9/1 remain unchanged. All family exits remain1 because failures remain. All local jobs have completed. Canonical docs are rendered with markdown-it and Fytala CSS; kit-managed assets remain unchanged.
