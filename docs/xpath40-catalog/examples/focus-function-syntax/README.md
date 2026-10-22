# Order-line pricing with explicit focus boundaries

This synthetic feed is a simplified one-currency order, not an industry-standard order or accounting implementation. Quantity and unit must cast to xs:decimal; no tax, currency conversion or discounts are modeled. SKU values in this fixture contain no report delimiters. Results use .NET-backed XPath decimal arithmetic and two display decimals; arbitrary-precision accounting is not claimed.

The [3.0 baseline](report-3.xsl) uses parameterized inline functions; [4.0](report-4.xsl) uses a focus amount callback, explicit-signature `fn(...)` formatting helper, mapping arrow labels, and a whole-sequence pipeline summary. Both require only the supplied XML, with no variables or resources. Bosak uses its default Legacy host; stylesheet versions select the effective expression syntax. These are ordinary XSLT source examples, not a new authoring API.

| Construct | Task contract |
|---|---|
| xsl:stylesheet / namespace declarations | Bind the XSLT, Schema and source-order vocabularies; select version 3.0 or 4.0. |
| xsl:output | Emit text, not an XML serialization. |
| xsl:template match="/" | Consume one orders document and emit the complete report. |
| xsl:variable | Bind lines, callback helpers and computed amounts. |
| xsl:value-of | Emit the scalar report assembled by string-join. |
| `fn { ... }` | One sequence-valued argument supplies the context; amount and label tasks supply one line. |
| `fn($sku, $value) { ... }` | Explicit parameters; keyword abbreviation does not turn it into a focus callback. |
| `=!>` | Invoke each line callback independently. |
| `->` | Evaluate the summary once with all amounts as context; empty input still produces a summary. |
| xs:decimal / format-number | Validate numeric lexical input; format only the displayed amount. |

## Complete input/output fixtures

### normal

```xml
<orders xmlns="urn:fytala:focus"><line sku="BOLT" qty="2" unit="12.50"/><line sku="NUT" qty="4" unit="2.50"/><line sku="FREE" qty="0" unit="99.00"/></orders>
```

Expected in both versions: `BOLT=25.00|NUT=10.00|FREE=0.00|lines=3;total=35.00`.

### empty

```xml
<orders xmlns="urn:fytala:focus"/>
```

Expected in both versions: `lines=0;total=0.00`.

### reordered

```xml
<orders xmlns="urn:fytala:focus"><line sku="FREE" qty="0" unit="99.00"/><line sku="NUT" qty="4" unit="2.50"/><line sku="BOLT" qty="2" unit="12.50"/></orders>
```

Expected in both versions: `FREE=0.00|NUT=10.00|BOLT=25.00|lines=3;total=35.00`.

### invalid

```xml
<orders xmlns="urn:fytala:focus"><line sku="BAD" qty="oops" unit="12.50"/></orders>
```

Expected in both versions: `FORG0001 (invalid quantity lexical value; no successful report)`.


Source line order is retained; zero-quantity FREE is retained as a line with amount 0.00, so normal count is three. Reordering is not normalized away. Invalid lexical quantities are conversion errors, not silently omitted lines. Repair the input and rerun; the example performs no transactional writes.

## Runnable checks

Run `dotnet test tests/Bosak.Xslt.Tests -c Release --filter "FullyQualifiedName~CatalogExampleTests&DisplayName~focus-syntax"` from the repository. Cases are in [the shared manifest](../cases.json). The XSLT controls run both complete report versions. XPath probes declare their entire inputs inline and require no external context.

The probes cover whole/empty focus, mapping versus whole-sequence focus, caller versus callback positions, closure capture, nested callbacks, caller-focus restoration, fn abbreviation and explicit 3.1 syntax refusals. A probe uses string-join to produce a scalar comparable report; CatalogExampleTests currently compares XdmValue.ToString(), which otherwise displays a sequence wrapper as `(sequence)`. The initial caller-restoration probe and failed receipt are retained in the slice evidence; its expected restored value was not weakened.

These 30 cases supplement existing named engine tests. No exact qt4 gated-set assignment or promotion is claimed. [Catalog contract](../../operators-and-syntax/focus-function-syntax.md) and [slice evidence](../../FOCUS-FUNCTION-SYNTAX.json) retain scope and provenance.
