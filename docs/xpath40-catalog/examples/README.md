<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak executable catalog examples">
  <br><br><h1>Executable Catalog Examples</h1>
  <p>Real tasks, earlier-version baselines, edge cases, and explicit compatibility</p>
</div>

## Run the examples

From the repository root:

```powershell
dotnet test tests/Bosak.Xslt.Tests/Bosak.Xslt.Tests.csproj -c Release --filter FullyQualifiedName~CatalogExampleTests
```

The [case manifest](cases.json) contains 144 named checks. Each points to a complete
published expression or stylesheet and declares its expected text or error. The
test project copies those exact files; [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs)
uses public compiler/evaluation APIs and verifies the declared outcomes.

| Mode | How it runs |
|---|---|
| `31` | `XPath31Expression.Compile` with `XPathCompatibility.XPath31`. |
| `40` | Explicit `XPathCompatibility.XPath40`; successful cases also verify refusal in the default mode. |
| `experimental` | Explicit `XPathCompatibility.XPath40Experimental`; successful cases verify refusal at both lower modes. |
| `xslt31` | Complete standard XSLT 3.0 baseline, paired with its declared XML input. |
| `xslt40` | Complete version="4.0" stylesheet demonstrating a Bosak-supported draft instruction; effective-version expressions use frozen XPath40 in 1.1.1; local earlier-version overrides retain XPath31. |

XPath files declare their synthetic input inline and require no external variables,
namespaces, or document focus. XSLT files declare their namespaces and use the
manifest's `input` file. Text-method capture returns the exact expected text
without file output. Error cases intentionally fail and verify the recorded error
code in the exception message.

## Read the comparisons honestly

Before/after cases agree on the illustrated result. They do not claim universal
semantic equivalence. For example, the CSV baseline begins with a host-parsed
row array: it demonstrates using the row, not implementing a CSV parser in 3.1.
The record baseline checks relevant map fields; it does not create a record
annotation. The coercion baseline uses an explicit constructor; its behavior on
fractional values differs from integer relabeling.

The stock-balance scan returns a **sequence of single-member arrays**, including
the initial state. Mapping arrows process individual items. The rejected `=>>`
case cannot compile; the deferred method-call entry instead demonstrates the
stable `array:size` alternative. Neither example introduces support for absent
syntax.

The generic fixture vocabulary is a dispatch task with priority and product
codes. Larger document cases live in the [worked chapters](../worked-examples/README.md).
Both types of evidence complement engine unit/conformance tests; these examples
do not change GatedSets or imply standards-level compatibility for every draft
surface. See the [catalog index](../README.md) for feature entries.

The new sequence/HOF and decision examples complement the [order-routing chapter](../worked-examples/order-routing/README.md), which uses an XML document context and a host-supplied plan rather than the context-free manifest runner.

## Indexing, batches and export selections

Read [product indexing](../functions/product-map-index.md),
[array batches](../functions/import-array-batches.md) and
[sequence selection](../functions/order-sequence-selection.md) for 40 additional
checks. Their complete expressions declare all synthetic input inline.
Comparisons at mode 31 explain exactly which behavior agrees and which differs.
The map result-shape probes preserve observed 1.1.1 behavior: `map:entries`
returns an array and `map:items` retains grouped multi-item values. They are
implementation-gap evidence, not assertions of draft conformance.
