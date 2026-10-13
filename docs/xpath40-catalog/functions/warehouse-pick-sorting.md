<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: Warehouse Pick Sorting">
  <br><br><h1>Warehouse Pick Sorting</h1>
  <p>Retained sorting functions — portable workflow and 4.0 contract limits</p>
</div>

## 1. Name and Classification

[fn:sort](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-sort)
and [array:sort](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-array-sort)
are existing 3.1 functions retained in the 4.0 draft. They are outside the
117-name inventory of 4.0-only functions. Implementation reviewed at Bosak
[source baseline 0597047a](https://github.com/Fytala-Charles/Bosak/blob/0597047aed99851d10445993e2e636f3aeb437f2/src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs),
with the named tests below. Historical adopted draft pins remain unavailable.
Current review: 2026-10-12 against immutable
[QT4CG F&O source e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml).

## 2. Intent

Produce a warehouse pick order by aisle and numeric bin, preserving input order
for tied locations. Sort a sequence of XML lines and an array whose members
contain both the original line and its quantity without separating that payload.

## 3. Also Known As

Stable key sorting and lexicographic composite keys describe this workflow.
There is no new function name or alias.

## 4. History / Supersedes

The stable [3.1 sequence sort](https://www.w3.org/TR/xpath-functions-31/#func-sort)
and [3.1 array sort](https://www.w3.org/TR/xpath-functions-31/#func-array-sort)
already support a collation and a key callback. The reviewed 4.0 draft defines
these compatibility functions through `sort-by`/`array:sort-by`, including the
new numeric comparison rules. Retaining the function names does not make every
numeric contract unchanged. The portable report below runs in both profiles.

## 5. Motivation

Bin `10` should follow bin `2` within an aisle. Sorting raw bin text would put
`10` before `2`. Equal-location lines must retain their original order, and
quantities must travel with their corresponding lines. Explicit typed keys and
member boundaries make those rules visible.

## 6. Language Alignment

Both functions have one-, two- and three-argument forms. Omitted/empty collation
uses the context default. An omitted key uses atomized input values. Explicit
empty keys currently raise `XPTY0004` in the nonempty sequence/array probes,
although the reviewed 4.0 draft defines default-on-empty. Supply `data#1`
explicitly as a verified workaround; an empty key argument differs from a key
callback that returns an empty sequence. A key can be a sequence: values are compared lexicographically,
with the first differing position determining order. Equal complete keys preserve
input order; an empty key precedes a nonempty key.

The sequence callback receives one input item. The array callback receives a
whole member, which can contain multiple items or be empty. Sorting rearranges
members without flattening their payload. `array:append` deliberately creates
one two-item member per line; an `array { ... }` constructor over a flat sequence
would create different member boundaries.

**Observed numeric gaps:** Bosak's retained functions use the shared
`XdmValueComparer`, rather than the reviewed exact numeric ordering. Input
`(3.1e0, 3.1)` remains double then decimal because promotion makes its keys tie;
the reviewed 4.0 ordering puts decimal first. Both sequence and array probes
record that limit. The two-item NaN probes also depend on input direction:
`(NaN, 1)` produces number then NaN, while `(1, NaN)` produces NaN then number.
The pinned numeric ordering places NaN below numbers. These are observed gaps,
not a promised ordering policy for larger inputs. Corrected engine behavior must
update the observed expectations while retaining historical migration notes.

QName keys are refused in the demonstrated case. The reviewed sorting rules also
exclude nonordered QName keys; broader `fn:compare` support does not authorize
assuming every atomic family is a valid sort key. Bosak accepts the unknown-collation URI in both empty-input and nonempty-string
probes; the latter returns codepoint-like order rather than `FOCH0002`. These
results do not establish that the URI is supported. Use a known explicit
collation and keep unknown-URI validation as an implementation follow-up.

## 7. Applicability

This synthetic warehouse vocabulary assumes present IDs/aisles, valid integer
bins and quantities, and a declared codepoint aisle policy. It omits route
optimization, stock allocation, units, scheduling and locale-sensitive aisle
names. Invalid bin text raises a constructor error. Production data should be
validated before sorting, and NaN/mixed numeric keys need an explicit policy.

## 8. Structure

Parse XML, construct an aisle/bin key, sort original lines, build an array of
line/quantity members, and sort those members using the key from their first item.
Render both orders for reconciliation.

## 9. Participants

Lines carry locations and quantities. A key callback returns a string followed
by an integer. The collation controls aisle strings. Array members bind each line
to its converted quantity. Output projection preserves the sorted order.

## 10. Collaborations

Use `string(@aisle)` and `xs:integer(@bin)` to state both key types. Compare
corresponding key positions, not aisle strings against bin integers.
Simple-map projection (`! string(@id)`) preserves the sequence's chosen order;
a path over nodes can restore document order. The member callback accesses the
line with `$member[1]`, and indexed output keeps its quantity alongside it.
Lines C and D share a key and therefore remain in input order.

## 11. Consequences

Both profiles produce C, D, B, A with quantities 3, 4, 1, 2. The workflow uses
homogeneous types at each key position and avoids the observed mixed numeric
and NaN gaps. Stable ties do not define a secondary business priority. An empty
batch yields empty sequence/member reports, while an empty array member remains
a member in the separate boundary probe.

## 12. Implementation

The report runs with either `CompileOptions.Compatibility40` or
`CompileOptions.Compatibility31`. Keyword calls use 4.0 syntax and match the
reviewed names `input`/`array`, `collation` and `key`. The explicit `sort-by`
comparison verifies the illustrated sequence-key equivalence, not every draft
contract or support for the draft's entire array-sort-by surface.

The [catalog harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs)
checks `baselineExpected` for shared default-mode outcomes and `baselineError`
for keyword refusal. Error cases verify diagnostic markers. Observed-limit
fixtures preserve current evidence rather than certifying conformance. No engine
or gate membership changes are made by this delivery.

**Existing-function default review (2026-10-12):** The [ordering/default review](../ORDER-DEFAULT-CONTRACT-REVIEW.md) and [detailed supplier README](../examples/order-contracts/README.md) add 132 checks across sort/array:sort/min/max/map:merge, explicit key/options and collation/NaN/cardinality boundaries, and six full 3.1/4.0 reports. Supplier price-plus-ID keys preserve business tie-breaking under source reordering. Use simple mapping to project sorted node IDs; a path restores document order. This is targeted evidence, not an engine fix or full D2 completion.

## 13. Sample Code

Complete synthetic input:

```xml
<picks><line id="A" aisle="B" bin="2" qty="2"/><line id="B" aisle="A" bin="10" qty="1"/><line id="C" aisle="A" bin="2" qty="3"/><line id="D" aisle="A" bin="2" qty="4"/></picks>
```

[XPath 4.0 runnable file](../examples/warehouse-pick-sorting/main.xpath):

```xpath
let $rows := parse-xml('<picks><line id="A" aisle="B" bin="2" qty="2"/><line id="B" aisle="A" bin="10" qty="1"/><line id="C" aisle="A" bin="2" qty="3"/><line id="D" aisle="A" bin="2" qty="4"/></picks>')/picks/line
return let $cp := 'http://www.w3.org/2005/xpath-functions/collation/codepoint'
return let $key := function($line) { (string($line/@aisle), xs:integer($line/@bin)) }
return let $ordered := sort($rows, $cp, $key)
return let $members := fold-left($rows, [], function($a, $line) { array:append($a, ($line, xs:integer($line/@qty))) })
return let $sorted := array:sort($members, $cp, function($member) { $key($member[1]) })
return concat('sequence=', string-join($ordered ! string(@id), ','), '; members=', string-join(for $i in 1 to array:size($sorted) return concat($sorted($i)[1]/@id, ':', $sorted($i)[2]), ','))
```

Exact output:

```text
sequence=C,D,B,A; members=C:3,D:4,B:1,A:2
```

### XPath 3.1 comparison

The [complete 3.1 runnable file](../examples/warehouse-pick-sorting/baseline.xpath)
contains the same expression and input; select `Compatibility31`. Its exact
output is also:

```text
sequence=C,D,B,A; members=C:3,D:4,B:1,A:2
```

No newer syntax is needed for this workflow. The distinction appears in the
separate numeric-key probes, not in the intentionally portable report.

### Complete fixture inventory

| Runnable case | Mode | Exact result/error marker | Default 3.1 outcome also checked |
|---|---|---|---|
| [main](../examples/warehouse-pick-sorting/main.xpath) | 40 | `sequence=C,D,B,A; members=C:3,D:4,B:1,A:2` | `sequence=C,D,B,A; members=C:3,D:4,B:1,A:2` |
| [baseline](../examples/warehouse-pick-sorting/baseline.xpath) | 31 | `sequence=C,D,B,A; members=C:3,D:4,B:1,A:2` | separate 3.1 fixture |
| [empty](../examples/warehouse-pick-sorting/empty.xpath) | 40 | `sequence=; members=` | `sequence=; members=` |
| [sort-by-equivalence](../examples/warehouse-pick-sorting/sort-by-equivalence.xpath) | 40 | `sequence=C,D,B,A; members=C:3,D:4,B:1,A:2` | compile refusal |
| [default-numeric](../examples/warehouse-pick-sorting/default-numeric.xpath) | 40 | `1,2,3` | `1,2,3` |
| [empty-default-key](../examples/warehouse-pick-sorting/empty-default-key.xpath) | 40 | `XPTY0004` | not asserted for this error case |
| [nested-array-members](../examples/warehouse-pick-sorting/nested-array-members.xpath) | 40 | `1:e,1:f,2:g,2:i` | `1:e,1:f,2:g,2:i` |
| [array-empty-default-key](../examples/warehouse-pick-sorting/array-empty-default-key.xpath) | 40 | `XPTY0004` | not asserted for this error case |
| [multi-item-members](../examples/warehouse-pick-sorting/multi-item-members.xpath) | 40 | `0/\|2/x` | `0/\|2/x` |
| [stable-ties](../examples/warehouse-pick-sorting/stable-ties.xpath) | 40 | `first,second` | `first,second` |
| [empty-key-sequence](../examples/warehouse-pick-sorting/empty-key-sequence.xpath) | 40 | `A,B` | `A,B` |
| [numeric-key-observed](../examples/warehouse-pick-sorting/numeric-key-observed.xpath) | 40 | `double,decimal` | `double,decimal` |
| [numeric-array-key-observed](../examples/warehouse-pick-sorting/numeric-array-key-observed.xpath) | 40 | `double,decimal` | `double,decimal` |
| [nan-forward-observed](../examples/warehouse-pick-sorting/nan-forward-observed.xpath) | 40 | `1,NaN` | `1,NaN` |
| [nan-reversed-observed](../examples/warehouse-pick-sorting/nan-reversed-observed.xpath) | 40 | `NaN,1` | `NaN,1` |
| [codepoint-labels](../examples/warehouse-pick-sorting/codepoint-labels.xpath) | 40 | `B,a` | `B,a` |
| [ascii-labels](../examples/warehouse-pick-sorting/ascii-labels.xpath) | 40 | `a,B` | `a,B` |
| [keyword-sequence](../examples/warehouse-pick-sorting/keyword-sequence.xpath) | 40 | `1,2,3` | error `XPST0003` |
| [keyword-array](../examples/warehouse-pick-sorting/keyword-array.xpath) | 40 | `1,2,3` | error `XPST0003` |
| [empty-array](../examples/warehouse-pick-sorting/empty-array.xpath) | 40 | `0` | `0` |
| [singleton-array](../examples/warehouse-pick-sorting/singleton-array.xpath) | 40 | `1/2` | `1/2` |
| [empty-invalid-collation-observed](../examples/warehouse-pick-sorting/empty-invalid-collation-observed.xpath) | 40 | empty string | empty string |
| [incomparable-keys](../examples/warehouse-pick-sorting/incomparable-keys.xpath) | 40 | `XPTY0004` | not asserted for this error case |
| [non-callable-key](../examples/warehouse-pick-sorting/non-callable-key.xpath) | 40 | `XPTY0004` | not asserted for this error case |
| [unknown-collation](../examples/warehouse-pick-sorting/unknown-collation.xpath) | 40 | `a,b` | `a,b` |
| [invalid-bin](../examples/warehouse-pick-sorting/invalid-bin.xpath) | 40 | `FORG0001` | not asserted for this error case |
| [qname-key](../examples/warehouse-pick-sorting/qname-key.xpath) | 40 | `XPTY0004` | not asserted for this error case |
| [explicit-data-key](../examples/warehouse-pick-sorting/explicit-data-key.xpath) | 40 | `1,2,3\|1,2,3` | `1,2,3\|1,2,3` |

The nested-array and two-item-member probes preserve different member shapes.
Default-key probes distinguish omission from the failing explicit-empty key
argument; `data#1` provides a verified workaround. Empty-key sequences and equal
keys preserve ties. String collation cases specify their policy. The NaN and
mixed numeric probes are marked as observed limitations. Invalid bin and
incomparable-key errors are not converted into successful reports.

## 14. Performance

Bosak computes keys before comparison and retains original indices for stable
ties. Sorting typically takes O(n log n) comparisons plus key construction;
multi-item keys add comparison work. Member payloads remain associated with
their items. No benchmark or streaming-memory guarantee is implied.

## 15. Known Uses

Synthetic warehouse pick preparation. Named evidence:
[`Sort_Mixed_Integer_Decimal`](../../../tests/Bosak.XPath.Standard.Tests/FunctionLibraryTests.cs),
[`Sort_Strings`](../../../tests/Bosak.XPath.Standard.Tests/FunctionLibraryTests.cs),
[`Sort_Mixed_Types_Throws`](../../../tests/Bosak.XPath.Standard.Tests/FunctionLibraryTests.cs)
and [`SortBy_MultipleKeyDefinitions_MajorToMinor`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs).
The [4.0 runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs)
contains exact `fn-sort` and `array-sort` gated sets. Their presence does not
close the observed gaps; these 28 documentation cases are separate evidence.
No set is promoted, and manual engine gates are not rerun for this documentation
delivery.

## 16. Related Features

[Supplier offer ranking](supplier-offer-ranking.md) uses separate sort-by key
definitions. [Comparator-based sorting](sort-with.md) covers explicit comparators.
[Typed measurement comparison](measurement-comparison.md) explains exact numeric
ordering, and [batch extrema](batch-extrema.md) records other promotion limits.
[The supplementary audit](../SUPPLEMENTARY-COVERAGE-AUDIT.md) tracks remaining
options and grammar/disposition coverage.
