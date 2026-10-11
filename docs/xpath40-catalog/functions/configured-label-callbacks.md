<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: configured shipping-label callbacks">
  <br><br><h1>Configured Label Callbacks</h1>
  <p>Frozen 4.0 — bind policy arguments and pass through unchanged values</p>
</div>

## 1. Name and Classification

**Configured label callbacks — Frozen 4.0.** [fn:partial-apply](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-partial-apply) and [fn:identity](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-identity).
Initial engine delivery: [PR #88](https://github.com/Fytala-Charles/Bosak/pull/88),
[revision 0a928bb8](https://github.com/Fytala-Charles/Bosak/commit/0a928bb81b81d0631cc681f86bba83259c0ed790), REQ-118 4.0-S5.
Definitions reviewed **2026-10-11** against immutable
[QT4CG function source a014e1b4](https://github.com/qt4cg/qtspecs/blob/a014e1b408baf5614a11401d3570a04498ab76be/specifications/xpath-functions-40/src/function-catalog.xml).
This review pin does not recover the unavailable historical adopted specification snapshot.

The original identity registration follow-up is
[eedcb321](https://github.com/Fytala-Charles/Bosak/commit/eedcb321848ff7e9381c34fd72cb42db4994992f).

## 2. Intent

Configure a shipping-label formatter once by binding its prefix and case policy,
then reuse the resulting one-argument callback for every customer name.
Use an identity callback when already formatted labels should pass through unchanged.

## 3. Also Known As

Argument binding and a pass-through callback describe the two operations.
They introduce no function aliases. Partial application binds selected arguments;
it does not execute the formatter at binding time.

## 4. History / Supersedes

XPath 3.1 supports placeholder partial application: `$format('SHIP: ', ?, true())`.
The complete comparison below produces the same callback and report. The 4.0
function accepts a runtime map of argument positions without requiring a
statically known arity, and can bind every argument to produce a zero-argument
function. The earlier identity equivalent is `function($value) { $value }`.

## 5. Motivation

A dispatch batch can share a label prefix and capitalization rule while each
record supplies a different name. Passing all configuration arguments on every
call obscures that fixed policy. Binding policy values exposes a simple callback
that fits `for-each`. An identity callback lets the same processing stage accept
already formatted labels without changing their content.

## 6. Language Alignment

`partial-apply($function, $arguments)` takes a callable and a map with positive
integer keys identifying **one-based** argument positions. Unbound parameters
retain their original relative order. Binding all parameters gives arity zero;
an empty map returns the original callable. Keys beyond the function's arity
are ignored, rather than reporting a misspelled configuration position.
String, zero and negative keys raise XPTY0004 in the demonstrated cases.
Maps and arrays are callable too, and can have their single lookup argument bound.

Supplied values undergo function conversion when parameter metadata permits.
Some Bosak built-ins lack that metadata; their conversion errors may surface
when the resulting function is invoked. The bound-type-error fixture exercises
that call, rather than claiming all validation happens eagerly.

`identity($input)` returns the same `item()*` value, including an empty sequence,
a whole sequence or a container. For nodes it preserves identity, rather than
copying. `identity#1` supplies a function item where a callback is required.

## 7. Applicability

Use for reusable callbacks configured by runtime policy, or for binding all
arguments into a deferred zero-argument call. A fixed, statically known function
may be clearer with 3.1 placeholders. Validate configuration positions separately
if silently ignored high keys would hide errors. The label case assumes nonempty
customer names validated upstream. It normalizes whitespace and applies a simple
uppercase policy; it is not a postal-address validator or a complete label layout.

## 8. Structure

The base formatter has parameters `(prefix, name, uppercase)`. Binding positions
one and three leaves `(name)`, which `for-each` calls for each supplied name.
The formatter returns a text label. The optional pass-through stage uses
`identity#1` for labels already prepared by an earlier stage.

## 9. Participants

Customer attributes provide names. A configuration map supplies fixed argument
values. The base formatter implements policy. The bound callback is reused by
`for-each`. Identity supplies unchanged values in the alternative stage.

## 10. Collaborations

Construct the bound callback outside the per-customer loop. Leave the varying
name argument unbound, and check `function-arity` to make that contract explicit.
If only the middle name argument is bound, the remaining callback parameters
are `(prefix, uppercase)`, in that order. Bind every argument for a deferred call
and invoke it with `()` to obtain the label.

## 11. Consequences

Policy becomes explicit and reusable. Binding creates a callable, not its final
result; invoking it performs formatting. Identity preserves values without
normalization or validation. For node values it preserves the original node,
and for maps/arrays it does not flatten their contents. No shipping action or
file output occurs in these expressions.

## 12. Implementation

Compile new calls with `CompileOptions.Compatibility40`; default XPath 3.1
rejects them. The placeholder and inline-identity comparisons use
`Compatibility31`. The existing [catalog harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs)
checks exact report/error markers and default-profile refusal for successful
4.0 fixtures. The source and customer inputs are complete, inline expressions.

## 13. Sample Code

Full synthetic dispatch input:

```xml
<dispatch>
  <customer name="  Ada  "/>
  <customer name="Grace"/>
</dispatch>
```

[Complete XPath 4.0 formatter](../examples/configured-label-callbacks/main.xpath):

```xpath
let $names := parse-xml('<dispatch><customer name="  Ada  "/><customer name="Grace"/></dispatch>')/dispatch/customer/@name ! string(.)
let $format := function($prefix, $name, $uppercase) { concat($prefix, if ($uppercase) then upper-case(normalize-space($name)) else normalize-space($name)) }
let $label := partial-apply($format, map {1: 'SHIP: ', 3: true()})
return concat('arity=', function-arity($label), '; labels=', string-join(for-each($names, $label), ' | '))
```

Exact output:

```text
arity=1; labels=SHIP: ADA | SHIP: GRACE
```

[Complete XPath 3.1 placeholder comparison](../examples/configured-label-callbacks/baseline.xpath):

```xpath
let $names := parse-xml('<dispatch><customer name="  Ada  "/><customer name="Grace"/></dispatch>')/dispatch/customer/@name ! string(.)
return let $format := function($prefix, $name, $uppercase) { concat($prefix, if ($uppercase) then upper-case(normalize-space($name)) else normalize-space($name)) }
return let $label := $format('SHIP: ', ?, true())
return concat('arity=', function-arity($label), '; labels=', string-join(for-each($names, $label), ' | '))
```

Exact output:

```text
arity=1; labels=SHIP: ADA | SHIP: GRACE
```

### Already formatted labels

[Complete pass-through expression](../examples/configured-label-callbacks/identity-labels.xpath):

```xpath
let $labels := ('SHIP: ADA', 'SHIP: GRACE')
return string-join(for-each($labels, identity#1), ' | ')
```

Exact output:

```text
SHIP: ADA | SHIP: GRACE
```

The [3.1 identity callback comparison](../examples/configured-label-callbacks/identity-baseline.xpath)
uses `function($label) { $label }` and produces the identical output.

### Complete fixture inventory

Each file supplies its complete input and exact report or error marker.

| Runnable case | Exact result or diagnostic marker |
|---|---|
| [main](../examples/configured-label-callbacks/main.xpath) | `arity=1; labels=SHIP: ADA | SHIP: GRACE` |
| [baseline](../examples/configured-label-callbacks/baseline.xpath) | `arity=1; labels=SHIP: ADA | SHIP: GRACE` |
| [preserve-case](../examples/configured-label-callbacks/preserve-case.xpath) | `arity=1; labels=SHIP: Ada | SHIP: Grace` |
| [empty-input](../examples/configured-label-callbacks/empty-input.xpath) | `arity=1; labels=` |
| [all-bound](../examples/configured-label-callbacks/all-bound.xpath) | `arity=0; label=SHIP: ADA` |
| [empty-bindings](../examples/configured-label-callbacks/empty-bindings.xpath) | `arity=3; label=SHIP: ADA` |
| [remaining-order](../examples/configured-label-callbacks/remaining-order.xpath) | `arity=2; label=PICK: Ada` |
| [ignored-high-key](../examples/configured-label-callbacks/ignored-high-key.xpath) | `arity=1; labels=SHIP: ADA | SHIP: GRACE` |
| [bound-sequence](../examples/configured-label-callbacks/bound-sequence.xpath) | `FRAME+MOTOR` |
| [callable-map](../examples/configured-label-callbacks/callable-map.xpath) | `ready` |
| [callable-array](../examples/configured-label-callbacks/callable-array.xpath) | `express` |
| [string-key](../examples/configured-label-callbacks/string-key.xpath) | `XPTY0004` |
| [zero-key](../examples/configured-label-callbacks/zero-key.xpath) | `XPTY0004` |
| [negative-key](../examples/configured-label-callbacks/negative-key.xpath) | `XPTY0004` |
| [noncallable](../examples/configured-label-callbacks/noncallable.xpath) | `XPTY0004` |
| [bound-type-error](../examples/configured-label-callbacks/bound-type-error.xpath) | `XPTY0004` |
| [identity-labels](../examples/configured-label-callbacks/identity-labels.xpath) | `SHIP: ADA | SHIP: GRACE` |
| [identity-baseline](../examples/configured-label-callbacks/identity-baseline.xpath) | `SHIP: ADA | SHIP: GRACE` |
| [identity-empty](../examples/configured-label-callbacks/identity-empty.xpath) | `0` |
| [identity-node](../examples/configured-label-callbacks/identity-node.xpath) | `true` |
| [identity-sequence](../examples/configured-label-callbacks/identity-sequence.xpath) | `FRAME,MOTOR` |
| [identity-container](../examples/configured-label-callbacks/identity-container.xpath) | `true` |

The case-preserving fixture binds `false()` and retains Ada/Grace capitalization.
The empty dispatch fixture reports arity one and no labels. The all-bound fixture
produces arity zero and one label when invoked. The remaining-order fixture binds
Ada and calls the remaining `(prefix, uppercase)` parameters. The high-key fixture
shows that position 99 is ignored. Sequence-valued bindings remain a single
argument value, as the FRAME+MOTOR string-join example verifies.
Identity cases cover empty, sequence, node and nested-container values.

## 14. Performance

Bind configuration once and reuse the resulting function. Bosak captures bound
argument values in a wrapper; this is not a memoization cache for formatter
results. Label work still occurs on each invocation. Identity returns its supplied
value unchanged. No benchmark or allocation-free pipeline claim follows from
these examples.

## 15. Known Uses

This batch-label workflow is a reproducible synthetic example. Existing evidence includes
[`PartialApply_InlineFunctionBase`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`PartialApply_EmptyMap_ReturnsFunctionUnchanged`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`PartialApply_BindingAllArguments_YieldsZeroArityFunction`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`PartialApply_KeysBeyondArity_AreIgnored`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`PartialApply_ArrayAsFunction`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`PartialApply_BoundValueCoercion_FailsAtCallTimeWhenNoTypeMetadata`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`PartialApply_NonIntegerKey_RaisesXpty0004`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
and [`Identity_ReturnsArgumentUnchanged`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs).
The [4.0 runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs)
assigns exact `fn-identity` to GatedSets; no exact `fn-partial-apply` set is assigned.
These 22 executable fixtures and named tests provide separate evidence.
No gate membership changes are included.

## 16. Related Features

[Higher-order selection](higher-order-selection.md) describes consumers of callbacks.
[Product dependency closure](product-dependency-closure.md) applies a captured
resolver callback to graph nodes. [Bounded retry state](bounded-retries.md)
uses two-argument state callbacks. [The completeness audit](../COMPLETENESS-AUDIT.md)
tracks remaining names, separate from exhaustive signature coverage.
