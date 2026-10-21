<div align="center">
  <img src="../../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Intake orders: type and pattern contracts">
  <h1>Intake orders: type and pattern contracts</h1>
  <p>21 October 2026</p>
</div>

A synthetic namespaced intake feed contains unique-ID line orders and bundles. Line quantities are assumed integer-parseable; this example is not schema validation. Normal input includes a positive line, a zero-quantity line, a nonempty bundle and an empty bundle. Keep order IDs explicit so no rejected item disappears silently.

The [portable3.0 report](report-3.xsl) uses maps/arrays, a named template and explicit predicates. The [4.0 report](report-4.xsl) uses an integer alias, closed map-entry patterns and array-member patterns. Both produce identical output for normal and reordered input. String SKU member order is preserved; sorting only normalizes order IDs.

| Fixture | Expected exact result |
|---|---|
| [normal](normal.xml) | `orders=4\|O-100:ship\|O-200:bundle\|O-300:review\|O-400:review` |
| [reordered](reordered.xml) | Same report, ordered by unique ID. |
| [empty](empty.xml) | `orders=0` |

```powershell
dotnet test tests/Bosak.Xslt.Tests/Bosak.Xslt.Tests.csproj -c Release --filter "FullyQualifiedName~CatalogExampleTests&DisplayName~intake-types"
```

All40 cases are complete standalone files: six reports, six alias controls, four nominal identity controls, four type/record pattern controls, eight map controls, eight array controls, two malformed-declaration controls and two3.0 pattern refusals. The runner uses default Legacy host; stylesheet mode labels do not select a host profile. Negative files intentionally fail with their manifest diagnostic.

Read the [grammar review](../../TYPE-PATTERN-GRAMMAR.md) before substituting types or patterns. Binding may convert a map while instance-of/pattern matching does not. Missing map key is different from a present empty value. An empty array satisfies a universal member constraint, so the report separately requires array:size greater than zero for a bundle. A '+' inside an array pattern does not impose a minimum array size. These distinctions are business-significant, not interchangeable syntax shortcuts.
