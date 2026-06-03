<!-- Bosak XPath / XSLT — General Integration Guide -->
<!-- Living document: updated with each significant Bosak change. -->

# Bosak XPath / XSLT — Integration Guide

> **Purpose:** Quick-reference for any application consuming the Bosak XPath 3.1 + XSLT stack.
> **Last updated:** 03 June 2026
> **Bosak baseline:** 867 unit tests passed / 0 failed / 0 skipped

---

## 1. Project References

Add project references to the Bosak layer stack from your consuming project:

```xml
<ItemGroup>
  <ProjectReference Include="..\Bosak\src\Bosak.XPath.Api\Bosak.XPath.Api.csproj" />
  <ProjectReference Include="..\Bosak\src\Bosak.XPath.Xslt\Bosak.XPath.Xslt.csproj" />
</ItemGroup>
```

`Bosak.XPath.Api` and `Bosak.XPath.Xslt` pull in the lower layers automatically (Core, Parser, Compiler, Runtime, Standard, Providers).

**Target framework:** `net10.0` (both sides must align).

---

## 2. XPath 3.1 Expressions

### 2.1 Compile & Evaluate

```csharp
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;

// One-shot evaluation
var expr = XPath31Expression.Compile("/invoice/items/item[@price > 100]");
var result = expr.Evaluate(document);

// Re-use compiled expression
var expr2 = XPath31Expression.Compile("$minPrice + $taxRate * $amount");
var result2 = expr2.Evaluate(
    new EvaluationContext()
        .WithVariable("minPrice",  XdmValue.FromDecimal(100.00m))
        .WithVariable("taxRate",   XdmValue.FromDecimal(0.21m))
        .WithVariable("amount",    XdmValue.FromDecimal(500.00m)));
```

### 2.2 Evaluation Context

```csharp
using Bosak.XPath.Runtime.Vm;

var ctx = new EvaluationContext
{
    BaseUri = "file:///C:/Data/",
    DocumentLoader = uri => /* your IXdmNode loader */
};

ctx.WithNamespace("edi", "http://example.org/edi")
   .WithVariable("docId", XdmValue.FromString("DOC-1234"));

var result = expr.Evaluate(ctx);
```

### 2.3 Reading Results

```csharp
if (result.IsNode && result.NodeValue is { } node)
{
    Console.WriteLine(node.StringValue);
}
else if (result.IsSequence && result.SequenceValue is { } seq)
{
    foreach (var item in XdmSequence.FromSource(seq))
        Console.WriteLine(item.ToString());
}
else
{
    Console.WriteLine(result.ToString());
}
```

### 2.4 Context Item (Focus)

```csharp
// Evaluate with a context item so `.` and `position()` work
ctx.WithFocus(XdmValue.FromNode(document), position: 1, size: 1);
var result = expr.Evaluate(ctx);
```

---

## 3. XSLT Transforms

### 3.1 Compile a Stylesheet

```csharp
using Bosak.XPath.Xslt.Api;

var xsl = @"<xsl:stylesheet version='3.0'
    xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
    <xsl:template match='/'>
        <output><xsl:value-of select='root/@id'/></output>
    </xsl:template>
</xsl:stylesheet>";

var compiler = new XsltCompiler();
var executable = compiler.Compile(xsl);
```

### 3.2 Transform a Document

```csharp
using Bosak.XPath.Providers.Xml;
using System.Xml.Linq;

var source = new XDocument(new XElement("root", new XAttribute("id", "42")));
var resultXml = executable.TransformToString(new XDocumentNode(source));
// => "<output>42</output>"
```

### 3.3 Named Templates & `call-template`

```csharp
var xsl = @"<xsl:stylesheet version='3.0'
    xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
    <xsl:template match='/'>
        <result>
            <xsl:call-template name='format-address'>
                <xsl:with-param name='city' select='root/city'/>
            </xsl:call-template>
        </result>
    </xsl:template>

    <xsl:template name='format-address'>
        <xsl:param name='city'/>
        <address><xsl:value-of select='$city'/></address>
    </xsl:template>
</xsl:stylesheet>";

var executable = new XsltCompiler().Compile(xsl);
var result = executable.TransformToString(new XDocumentNode(source));
```

### 3.4 Tunnel Parameters

```csharp
var xsl = @"<xsl:stylesheet version='3.0'
    xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
    <xsl:template match='/'>
        <xsl:apply-templates>
            <xsl:with-param name='traceId' select='"REQ-123"' tunnel='yes'/>
        </xsl:apply-templates>
    </xsl:template>

    <xsl:template match='item'>
        <!-- $traceId is available here via tunnel -->
        <item trace='{$traceId}'><xsl:value-of select='.'/></item>
    </xsl:template>
</xsl:stylesheet>";
```

### 3.5 `fn:transform()` — XSLT from XPath

```csharp
var callerXsl = @"<xsl:stylesheet version='3.0'
    xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
    xmlns:map='http://www.w3.org/2005/xpath-functions/map'>
    <xsl:template match='/'>
        <result>
            <xsl:copy-of select='transform(map{
                ""stylesheet-location"": ""file:///C:/styles/main.xsl"",
                ""source-node"": .,
                ""stylesheet-params"": map{""greeting"": ""world""}
            })?output'/>
        </result>
    </xsl:template>
</xsl:stylesheet>";
```

---

## 4. XSLT Feature Matrix (Current State)

| Feature | Status | Notes |
|---------|--------|-------|
| `xsl:template match="…"` | ✅ Working | Pattern compiler: element names, `*`, `@*`, predicates, union (`\|`) |
| `xsl:template name="…"` | ✅ Working | Named template dispatch |
| `xsl:call-template` | ✅ Working | With `xsl:with-param` support |
| `xsl:apply-templates` | ✅ Working | Default mode; `select` attribute supported |
| `xsl:value-of` | ✅ Working | |
| `xsl:for-each` | ✅ Working | Position / size context updated per item |
| `xsl:if` / `xsl:choose` | ✅ Working | `when` + `otherwise` |
| `xsl:element` / `xsl:attribute` | ✅ Working | |
| `xsl:text` | ✅ Working | |
| `xsl:copy-of` | ✅ Working | Deep copy of nodes; Document nodes supported |
| `xsl:comment` | ✅ Working | `select` attribute or text content |
| `fn:copy-of` | ✅ Working | XSLT 3.0 context function |
| `xsl:decimal-format` | ✅ Working | Parsed and registered for `fn:format-number` |
| `xsl:variable` | ✅ Working | Lexical scoping; usable in XPath via `$var` |
| `xsl:param` | ✅ Working | On named templates, global params, default values |
| Built-in template rules | ✅ Working | Shallow-copy elements, copy text/attributes |
| Literal result elements | ✅ Working | Namespace preservation, AVT evaluation |
| `xsl:import` / `xsl:include` | ✅ Working | URI resolution with correct precedence rules |
| Modes | ✅ Working | Named modes, `#current`, `#default`, `#all`, multi-mode templates |
| `xsl:sort` | ✅ Working | Single and multi-key; `data-type`, `order`, `stable` |
| `xsl:number` | ✅ Working | `single`, `any`, `multiple` levels; format tokens |
| `xsl:key` / `key()` | ✅ Working | Indexed lookup with `xsl:key` definitions |
| `xsl:output` | ✅ Working | `method`, `indent`, `omit-xml-declaration`, `encoding` |
| `xsl:function` | ✅ Working | User-defined XPath functions in XSLT |
| `xsl:sequence` | ✅ Working | Returns sequences from functions |
| `xsl:mode` | ✅ Working | `on-no-match` declarations |
| Tunnel parameters | ✅ Working | `tunnel="yes"` propagation through `apply-templates` |
| `fn:transform()` | ✅ Working | XPath-level XSLT invocation |

---

## 5. XPath 3.1 Feature Highlights

### Well-covered areas
- Sequence construction, filtering, FLWOR expressions
- Standard `fn:*` functions (string, numeric, date/time, QName, URI)
- `map:*` and `array:*` functions
- Higher-order functions (`fn:for-each`, `fn:filter`, `fn:fold-left`, etc.)
- `fn:doc`, `fn:collection` with pluggable document loader
- Decimal formatting (`fn:format-number`)
- JSON functions: `fn:parse-json`, `fn:json-to-xml`, `fn:xml-to-json`, `fn:json-doc`
- Date/time ordering (`lt`, `gt`, `le`, `ge`)

### Known gaps
- `fn:load-xquery-module` — not implemented
- `fn:serialize` — partial (no XML serialization options)
- `fn:transform` options (`delivery-format`, etc.) — partial
- Schema-aware operations — not supported

---

## 6. XSD Validation

Bosak provides an `IXsdValidator` abstraction for XML Schema validation:

```csharp
using Bosak.XPath.Api.Xsd;

var validator = new XsdValidator();
var result = validator.TryValidate(xmlString, xsdStream);

if (result.IsValid)
{
    Console.WriteLine("Document is valid");
}
else
{
    foreach (var error in result.OnlyErrors)
    {
        Console.WriteLine($"Error at line {error.LineNumber}: {error.Message}");
    }
}
```

Features:
- Single-schema and multi-schema validation (handles `xs:import`/`xs:include`)
- Structured error results with line/column numbers
- Non-throwing `TryValidate` and throwing `Validate` variants
- Configurable via `XsdValidatorOptions` (max error count, treat warnings as errors)

---

## 7. Current Build State

Run the full suite from the Bosak repo root:

```bash
dotnet build Bosak.sln
dotnet test Bosak.sln
```

**Unit tests:** 867 passed, 0 failed, 0 skipped  
**Target framework:** `net10.0`

### Conformance Baselines

| Suite | Passed | Failed | Skipped | Pass Rate | Notes |
|-------|--------|--------|---------|-----------|-------|
| XSLT 3.0 (W3C) | 3,257 | 2,204 | 9,139 | 59.6% | Stable; +2 from number-0111/0807 fixes |
| XPath 3.1 (QT3) | 18,651 | 3,279 | 9,891 | 58.6% | Completed all 428 sets; register overflow fixed (byte→ushort); normalize-space harness fix |

> **Note:** The conformance runner locks DLLs. If you get build errors about locked files, run:
> ```bash
> taskkill /F /IM Bosak.XPath.Conformance.exe
> taskkill /F /IM Bosak.XPath.Xslt.Conformance.exe
> ```

---

## 8. Getting Help / Reporting Issues

- Check `docs/ARCHITECTURE.md` in the Bosak repo for the layer overview and execution pipeline.
- Check `docs/FEATURE_REQUESTS.md` for the feature request registry.
- XPath failures: capture the expression, input XML, and expected vs. actual result.
- XSLT failures: capture the stylesheet fragment, source XML, and expected output.
