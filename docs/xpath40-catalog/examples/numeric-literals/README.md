# Telemetry readiness report

This synthetic feed uses nonnegative decimal integers for flags/count and IDs without report delimiters. A reading is ready when bit4 is set and its count is at least1000. Values are illustrative; there is no device I/O, schema validation, persistence or streaming claim.

The complete [3.0 baseline](report-3.xsl) spells constants in decimal. The [4.0 report](report-4.xsl) spells the mask as `0b0100 + 0x0` and threshold as `1_000`. Both deliberately use the same arithmetic algorithm. Literal spelling improves source readability; runtime input remains decimal text.

| Instruction | Responsibility |
|---|---|
| xsl:stylesheet | Set effective3.0/4.0, bind XSLT/Schema and urn:fytala:telemetry namespaces. |
| xsl:output | Produce exact text output. |
| template / | Read one complete input document with no external parameters/resources. |
| variable rows | Select namespaced reading nodes in source order. |
| variable ready | Parse flags/count, use idiv/mod to test bit4, and compare the count threshold. |
| for-each / value-of | Emit each ID and readiness label in source order. |
| final value-of | Emit ready count and total count, including a zero total for empty input. |

## Inputs and exact expectations

### normal

[normal.xml](normal.xml):

```xml
<telemetry xmlns="urn:fytala:telemetry"><reading id="A" flags="5" count="1500"/><reading id="B" flags="2" count="900"/></telemetry>
```

Both versions emit `A:ready|B:hold|ready=1;total=2400`.

### empty

[empty.xml](empty.xml):

```xml
<telemetry xmlns="urn:fytala:telemetry"></telemetry>
```

Both versions emit `ready=0;total=0`.

### reordered

[reordered.xml](reordered.xml):

```xml
<telemetry xmlns="urn:fytala:telemetry"><reading id="B" flags="2" count="900"/><reading id="A" flags="5" count="1500"/></telemetry>
```

Both versions emit `B:hold|A:ready|ready=1;total=2400`.

### invalid

[invalid.xml](invalid.xml):

```xml
<telemetry xmlns="urn:fytala:telemetry"><reading id="A" flags="5" count="oops"/></telemetry>
```

Both versions raise `FORG0001` for the invalid decimal count.

## Execution and boundaries

The manifest records eight complete report cases,38 focused probes, three explicit3.1 refusals and one experimental/frozen agreement check. CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome executes the actual files through public XPath/XSLT APIs. Run the full solution using the root AGENTS.md commands and the documented XSLT temporary-copy workaround when required. The [feature entry](../../operators-and-syntax/numeric-literals.md) explains lexical rules, typed values, malformed digit groups, old-profile refusals and string-cast differences. All XPath probes need no context item or external variables. Their exact results/errors are in [cases.json](../cases.json) and [the slice audit](../../NUMERIC-LITERALS.json).
