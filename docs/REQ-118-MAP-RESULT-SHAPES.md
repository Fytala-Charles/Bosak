<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak map result-shape correction">
  <br><br><h1>Map Result Sequence Contracts</h1>
  <p>REQ-118 correction discovered by the REQ-126 reference examples</p>
</div>

## Problem and resulting behavior

Published Bosak 1.1.1 returns an array from `map:entries` and retains grouped
sequence values from `map:items`. Both shapes differ from the reviewed draft.
Published [**1.1.2**](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.1.2) corrects the supported contracts:

| Call | Published 1.1.1 | Corrected source 1.1.2 |
|---|---|---|
| `map:entries(map{'a':1,'b':2})` | One array item with two map members | Two single-entry map items |
| `map:entries(map{})` | Empty array item | Empty sequence |
| `map:items(map{'a':(1,2),'b':3})` | Two grouped values | Three ordinary items: `1,2,3` |
| `map:items(map{'a':(),'b':()})` | Retained empty value groups | Empty sequence |

Entries retain their original key and complete value sequence. Items concatenate
one level of value sequences, preserving entry order; nested arrays and maps
remain items. Neither operation recursively flattens nested containers. The
default XPath31 profile still rejects direct calls and hides dynamic lookup.
The change adds no public API and grants no experimental functions.

## Reviewed specification and provenance

Definitions were checked on **2026-10-11** against immutable QT4CG source
[`a014e1b408baf5614a11401d3570a04498ab76be`](https://github.com/qt4cg/qtspecs/blob/a014e1b408baf5614a11401d3570a04498ab76be/specifications/xpath-functions-40/src/function-catalog.xml).
Navigation references: [map:entries](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-map-entries)
and [map:items](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-map-items).
This is the correction's reviewed source, not a reconstructed historical adopted
snapshot. Initial function delivery was [PR #84](https://github.com/Fytala-Charles/Bosak/pull/84),
revision `f7f55b4e3a6e92da26ac607e34f46aede695cb19`.

Correction implementation revision: `c6fd1cb6`, [PR #160](https://github.com/Fytala-Charles/Bosak/pull/160).

## Implementation and migration

[FunctionLibrary](../src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs)
changes the entries signature metadata from Array to Sequence, constructs a
sequence of single-entry maps and concatenates each value sequence for items.
Both empty results use the standard empty-sequence representation.

Consumers opting into XPath40 should replace `map:entries($map)?*` with
`map:entries($map)`, and array-position lookup of that result with a sequence
predicate (`[2]`). Counts and iteration over `map:items` now observe individual
items, including every item of a multi-item value. Review callers relying on the
old grouping. The portable alternatives
`map:for-each($map, function($key,$value){map:entry($key,$value)})` and
`map:for-each($map, function($key,$value){$value})` remain valid.

The [product-index chapter entry](xpath40-catalog/functions/product-map-index.md)
now uses the corrected expression directly. Its result-shape probes deliberately
change to the draft sequence contract. Historical 1.1.1 observations remain
documented for migration; the original unit expectations are corrected rather
than retained as desired behavior.

## Regression evidence

[MapResultShapeTests](../tests/Bosak.XPath.Standard.Tests/MapResultShapeTests.cs)
adds **21 checks**: sixteen direct/dynamic shape, cardinality, ordering and
equivalence cases; two direct XPath31 refusals; two dynamic-lookup refusals; and
one check that entry values remain whole while map items concatenate them.
Inputs include empty maps/values, singleton and multi-item values, nested
arrays/maps, named references and `function-lookup` calls.

The existing `MapEntries_ReturnsSingleEntryMapsInOrder` and
`MapItems_ReturnsValuesInEntryOrder` checks in
[XPath40FunctionTests](../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs)
now assert ordinary sequence contracts.
[MapResults_RestoreCallerProfile](../tests/Bosak.Xslt.Tests/XPathCompatibilityRuntimeTests.cs)
adds two checks that nested earlier-version stylesheet calls hide both names
and restore the outer frozen profile for dynamic invocation.
The [catalog manifest](xpath40-catalog/examples/cases.json) continues to contain
144 checks; updated main and boundary probes preserve 168 total documentation
checks with the existing worked chapters.

## Verification and limits

Release solution build: **0 warnings / 0 errors** (final incremental build;
the initial rebuild reported 12 existing warnings). Full isolated solution:
**4,166 passed / 0 failed / 0 skipped**, across ten assemblies (Standard 1,303;
XSLT 1,299). Focused new standard checks **21/0/0**; catalog plus nested map
profile checks **146/0/0**. Frozen QT3 remains exactly **31,142/0/679**.
qt4 promoted sets remain **8,278/0/1,894**, zero gated/exploratory failures and
11 experimental-level passes. Logs: `.guard-tmp/map-shapes-*`.
All 41 catalog structures/index links, 166 primary anchors/links, 28 baseline
provenance records, 117 inventory names and 144 manifest files are verified.
All three family sample blocks match their exact files/results, and 54 catalog
pages plus this dossier and patch notes render successfully. Whitespace checks pass.

Targeted upstream `map-entries`: **16 passed / 0 failed / 1 skipped** (unsupported
dependency). Targeted `map-items`: **15/0/0**. These sets remain exploratory; the
skipped entries dependency prevents claiming every case passes. No conformance
set is promoted in this correction. A complete XSLT sweep is not claimed.
Braid adoption remains separate work.

**Publication verified:** tag v1.1.2, release revision `3aebf76a24b28af7d40aa42e6eaf714b3a596838`; all ten public package downloads and eight fresh-cache consumer checks passed. [Publishing workflow](https://github.com/Fytala-Charles/Bosak/actions/runs/38144723077).
