<div align="center">
  <img src="../../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Dispatch batches: grammar and version boundaries">
  <h1>Dispatch batches: grammar and version boundaries</h1>
  <p>21 October 2026</p>
</div>

A warehouse receives namespaced batches with a priority and SKU lines. It needs a stable routing report that retains empty batches for review. Inputs are synthetic; IDs are assumed unique, SKU order is significant, and no logistics standard or schema validation is claimed.

Compare [portable XSLT3.0](report-3.xsl) with the [implemented4.0 forms](report-4.xsl). Both sort batch IDs, distinguish flattened SKU items from grouped SKU sequences, route urgent/rush to expedite, normal to standard and unknown priorities to review. Empty SKU groups appear as `-` rather than disappearing. The4.0 report combines xsl:array for-each/select, switch and text-body select; the3.0 report uses XPath arrays, choose and value-of. No external variables or resources are needed.

| Input | Expected exact text from either version |
|---|---|
| [normal.xml](normal.xml) | `batches=3;flat=3;groups=3\|B-100:expedite:A+B\|B-200:review:-\|B-300:standard:C` |
| [reordered.xml](reordered.xml) | Same report; batch IDs determine the order. |
| [empty.xml](empty.xml) | `batches=0;flat=0;groups=0` |

Run the exact published files through the existing runner:

```powershell
dotnet test tests/Bosak.Xslt.Tests/Bosak.Xslt.Tests.csproj -c Release --filter "FullyQualifiedName~CatalogExampleTests&DisplayName~dispatch-grammar"
```

There are37 checks: six complete reports, six recognition probes, four expression-version gates, ten static/result-error controls and eleven member/type/focus/identity controls. Mode labels xslt31/xslt40 describe stylesheet version; the runner uses Bosak's default Legacy host. Bosak recognizing a draft instruction under version3.0 does not make that instruction portable XSLT3.0. The explicit draft host has separate existing tests.

`select-focus` returns adjacent atomic strings and verifies their separator. `select-focus-text` and the report use value-of to create text explicitly. CDATA braces are XPath expression syntax, not an AVT. `array-empty-member` distinguishes one empty member from the zero-member `array-empty` case. `switch-node-identity` verifies the selected source node without copying it.

The [grammar review](../../SWITCH-ARRAY-SELECT-GRAMMAR.md) maps every contract to source, primary specification sections and existing tests. Negative files are intentionally malformed or ill-typed and should fail with their manifest error codes; do not use them as application templates. The runner checks error text through public capture and does not prove static detection timing. No new gate, processor profile or upstream acceptance is claimed.
