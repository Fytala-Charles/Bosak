<!-- Bosak XPath / XSLT — Customer A Integration Guide -->
<!-- Living document: updated with each significant Bosak change. -->

# Bosak XPath / XSLT — Customer A Integration Guide

> **Purpose:** Quick-reference for the Customer A project on how to consume the Bosak XPath 3.1 + XSLT stack.
> **Last updated:** 27 May 2026
> **Bosak baseline:** 831 unit tests passed / 0 failed (W3C QT3 conformance: ~18,272 passed / ~3,747 failed)

---

## 1. Project References

Add project references from `Customer A.Workbench.Desktop` (or whichever Customer A project needs XPath/XSLT) to the Bosak layer stack:

```xml
<ItemGroup>
  <ProjectReference Include="..\..\Bosak\src\Bosak.XPath.Api\Bosak.XPath.Api.csproj" />
  <ProjectReference Include="..\..\Bosak\src\Bosak.XPath.Xslt\Bosak.XPath.Xslt.csproj" />
</ItemGroup>
```

`Bosak.XPath.Api` and `Bosak.XPath.Xslt` pull in the lower layers automatically (Core, Parser, Compiler, Runtime, Standard, Providers).

**Target framework:** `net9.0` (both sides must align).

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
    BaseUri = "file:///C:/Customer A/Data/",
    DocumentLoader = uri => /* your IXdmNode loader */
};

ctx.WithNamespace("edi", "http://app.example.org/edi")
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

var xsl = @"<xsl:stylesheet version='2.0'
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
var xsl = @"<xsl:stylesheet version='2.0'
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

### 3.4 Parameters & Variables

- `xsl:param` on a named template receives values via `xsl:with-param`.
- Default values on `xsl:param` work via `select` attribute.
- `xsl:variable` binds into the current lexical scope and is available to XPath expressions (`$var`).
- Variable scoping is lexical: a `call-template` cannot leak variables into the caller.

---

## 4. XSLT Feature Matrix (Current State)

| Feature | Status | Notes |
|---------|--------|-------|
| `xsl:template match="…"` | ✅ Working | Pattern compiler supports element names, `*`, `@*`, predicates, union (`\|`) |
| `xsl:template name="…"` | ✅ Working | Named template dispatch via `call-template` |
| `xsl:call-template` | ✅ Working | With `xsl:with-param` support |
| `xsl:apply-templates` | ✅ Working | Default mode; `select` attribute supported |
| `xsl:value-of` | ✅ Working | |
| `xsl:for-each` | ✅ Working | Position / size context updated per item |
| `xsl:if` / `xsl:choose` | ✅ Working | `when` + `otherwise` |
| `xsl:element` / `xsl:attribute` | ✅ Working | |
| `xsl:text` | ✅ Working | |
| `xsl:copy-of` | ✅ Working | Deep copy of nodes |
| `xsl:variable` | ✅ Working | Bound to context; usable in XPath via `$var` |
| `xsl:param` | ✅ Working | On named templates and inline |
| Built-in template rules | ✅ Working | Shallow-copy elements, copy text/attributes |
| Literal result elements | ✅ Working | Namespace preservation |
| `xsl:import` / `xsl:include` | ✅ Working | URI resolution with `IXsltUriResolver`; correct precedence rules |
| Modes | ✅ Working | Named modes, `#current`, `#default`, `#all`, multi-mode templates |
| `xsl:sort` | ✅ Working | `select`, `data-type="number|text"`, `order="ascending|descending"` |
| `xsl:number` | ✅ Working | `single`, `any`, `multiple` levels; format tokens |
| `xsl:key` / `key()` | ✅ Working | Indexed lookup with `xsl:key` definitions |
| `xsl:output` | ✅ Working | `method="xml|text"`, `indent`, `omit-xml-declaration`, `encoding`, `standalone` |
| `xsl:function` | ✅ Working | User-defined XPath functions in XSLT; recursion supported |
| `xsl:sequence` | ✅ Working | Returns sequences from XSLT functions |
| `xsl:mode` | ✅ Working | `on-no-match` declarations |
| Tunnel parameters | ✅ Working | `tunnel="yes"` propagation through `apply-templates` |
| `fn:transform()` | ✅ Working | XPath-level XSLT invocation with `stylesheet-params`, `initial-template` |

---

## 5. XPath 3.1 Feature Highlights

### Well-covered areas
- Sequence construction, filtering, FLWOR expressions
- All standard `fn:*` functions (string, numeric, date/time, QName, URI)
- `map:*` and `array:*` functions
- Higher-order functions (`fn:for-each`, `fn:filter`, `fn:fold-left`, etc.)
- `fn:doc`, `fn:collection` with pluggable document loader
- Decimal formatting (`fn:format-number`)

### Known gaps
- `fn:load-xquery-module` — not implemented
- `fn:serialize` — partial (no XML serialization options)
- `fn:transform` options (`delivery-format`, etc.) — partial
- Schema-aware operations — not supported

---

## 6. Current Build State

Run the full suite from the Bosak repo root:

```bash
dotnet build Bosak.sln
dotnet test Bosak.sln
```

**Unit tests:** 838 passed, 0 failed, 0 skipped  
**W3C QT3 conformance (XPath):** ~18,272 passed / ~3,747 failed / ~9,802 skipped  
**W3C XSLT 3.0 conformance:** 1,566 passed / 3,901 failed / 9,133 skipped (~28.6% pass rate on supported features)  
**Target framework:** `net9.0`

> **Note:** The conformance runner locks DLLs. If you get build errors about locked files, run:
> ```bash
> taskkill /F /IM Bosak.XPath.Conformance.exe
> ```

---

## 7. Typical Customer A Patterns

### EDI → Canonical XML
```csharp
var xslt = File.ReadAllText("C:/Customer A/Maps/EDIFACT_to_Canonical.xsl");
var compiler = new XsltCompiler();
var executable = compiler.Compile(xslt);

var ediDoc = XDocument.Load("C:/Customer A/In/ORDERS_001.xml");
var canonicalXml = executable.TransformToString(new XDocumentNode(ediDoc));
```

### XPath-driven validation
```csharp
var expr = XPath31Expression.Compile(
    "every $item in /order/items/item satisfies $item/quantity castable as xs:integer");
bool valid = expr.Evaluate(context).EffectiveBooleanValue();
```

---

## 8. Getting Help / Reporting Issues

- Check `docs/ARCHITECTURE.md` in the Bosak repo for the layer overview and execution pipeline.
- XPath failures: capture the expression, input XML, and expected vs. actual result.
- XSLT failures: capture the stylesheet fragment, source XML, and expected output.
