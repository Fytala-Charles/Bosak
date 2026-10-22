# Stock adjustment: cast, then construct a new typed record

This synthetic inventory vocabulary is simplified: quantities/deltas are integers, SKUs contain no report delimiters, source order matters, and negative deltas are permitted. It performs no persistence, concurrent stock update, unit conversion or business nonnegative-stock validation. `but with` constructs a value; it is not a database update.

The complete [3.0 baseline](report-3.xsl) explicitly selects map fields, casts quantity and uses map:merge with use-last. [The 4.0 report](report-4.xsl) casts to a structural record and uses `but with` to construct the amended record. The baseline reproduces this business report, not general record annotation/type safety.

| Step | Contract |
|---|---|
| xsl:stylesheet / namespaces | Bind the XSLT, Schema, map and source-stock vocabularies; select effective3.0/4.0. |
| xsl:output | Emit text. |
| Template / | Consume one complete stock document and produce all rows plus count/total. |
| xsl:variable rows as map(*)* | Retain maps/records as values instead of wrapping sequence-constructor content in a temporary document. |
| xsl:for-each / raw map | Process source items in order and declare SKU, string quantity and import-only trace. |
| Cast adapter | Select sku/qty/note, cast quantity, discard trace and create the missing note as a present empty entry. |
| but with | Copy the annotated record with replacement qty, recoercing fields under that same annotation. |
| xsl:sequence | Retain original quantity alongside the adjusted value; input/before value is not mutated. |
| xsl:value-of / string-join | Emit exact before/after rows and the final count/quantity total. |

## Complete fixtures and expected results

### normal

```xml
<stock xmlns="urn:fytala:stock"><item sku="BOLT" qty="10" delta="2"/><item sku="NUT" qty="4" delta="-1"/></stock>
```

Both versions expect `BOLT:10->12|NUT:4->3|items=2;total=15`.

### empty

```xml
<stock xmlns="urn:fytala:stock"/>
```

Both versions expect `items=0;total=0`.

### reordered

```xml
<stock xmlns="urn:fytala:stock"><item sku="NUT" qty="4" delta="-1"/><item sku="BOLT" qty="10" delta="2"/></stock>
```

Both versions expect `NUT:4->3|BOLT:10->12|items=2;total=15`.

### invalid

```xml
<stock xmlns="urn:fytala:stock"><item sku="BAD" qty="10" delta="oops"/></stock>
```

Both versions expect `FORG0001 for invalid delta; no successful report`.

Reordering changes row order, not total. An invalid lexical number is not silently dropped; repair it and rerun. Field filtering is deliberate at the adapter boundary: the cast discards the raw trace field. Coercion would reject that extra field rather than silently adapt it. The probes independently check optional-field presence, missing required fields, nested conversion, field order and original-record preservation.

## Runnable checks

Run `dotnet test tests/Bosak.Xslt.Tests -c Release --filter "FullyQualifiedName~CatalogExampleTests&DisplayName~record-contract"`. All 46 cases are in [the manifest](../cases.json): eight complete stylesheet reports,35 inline XPath value/error probes and three explicit 3.1 refusals. Successful4.0 XPath probes also assert3.1 refusal where declared. XPath probes supply their entire input inline and need no external context.

The stylesheets use Bosak's default Legacy host with effective stylesheet versions3.0/4.0. This is not an exhaustive Draft-host, named-declaration, schema, module or package matrix. The baseline uses ordinary maps; it does not claim a 3.1 annotated record type. Source 1.23.6 fixes a signature version-gate leak exposed by these probes; [finding and regression](../../INLINE-RECORD-TYPE-GATE.md) retain the original failures. No feature-specific qt4 gated-set assignment is claimed; named engine tests and executable cases are the applicable evidence.

[Record update entry](../../type-system/record-updates.md), [cast/coercion reference](../../type-system/structural-records.md) and [slice audit](../../RECORD-CONTRACTS.json) state supported scope and remaining gaps.
