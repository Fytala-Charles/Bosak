<!-- Bosak XPath / XSLT — General Integration Guide -->
<!-- Living document: updated with each significant Bosak change. -->

# Bosak XPath / XSLT / XQuery — Integration Guide

> **Purpose:** Quick-reference for any application consuming the Bosak XPath 3.1 + XSLT + XQuery stack.
> **Last updated:** 10 June 2026
> **Bosak baseline:** 875 unit tests passed / 0 failed / 0 skipped
> **XSLT baseline:** 3,468 passed / 1,937 failed / 9,195 skipped (64.2%)

---

## 1. Consuming Bosak

### 1.1 Via Project References (development)

Add project references to the Bosak layer stack from your consuming project:

```xml
<ItemGroup>
  <ProjectReference Include="..\Bosak\src\Bosak.XPath.Api\Bosak.XPath.Api.csproj" />
  <ProjectReference Include="..\Bosak\src\Bosak.Xslt\Bosak.Xslt.csproj" />
  <!-- <ProjectReference Include="..\Bosak\src\Bosak.XQuery\Bosak.XQuery.csproj" /> -->
</ItemGroup>
```

`Bosak.XPath.Api`, `Bosak.Xslt`, and `Bosak.XQuery` pull in the lower layers automatically (Core, Parser, Compiler, Runtime, Standard, Providers).

**Target framework:** `net10.0` (both sides must align).

### 1.2 Via NuGet Packages

All Bosak source projects are now packable. After packing (`dotnet pack`), the following packages are produced:

| Package | Description |
|---------|-------------|
| `Bosak.Xslt` | XSLT 3.0 processor and transform engine |
| `Bosak.XPath.Api` | Public API for compiling and evaluating XPath 3.1 |
| `Bosak.XPath.Core` | XDM types and core abstractions |
| `Bosak.XPath.Runtime` | Register-based VM execution engine |
| `Bosak.XPath.Standard` | Standard XPath 3.1 / XQuery function library |
| `Bosak.XPath.Providers` | `IXdmNode` adapters for `System.Xml.Linq` |
| `Bosak.XPath.Parser` | Recursive-descent XPath 3.1 parser |
| `Bosak.XPath.Compiler` | AST-to-IR compilation pipeline |

**Consuming from a private feed:**
```bash
# Pack all projects
dotnet pack Bosak.sln --output ./nupkgs

# Push to your private NuGet feed
dotnet nuget push ./nupkgs/Bosak.Xslt.1.0.0.nupkg --source https://your-feed/nuget/v3/index.json
```

Then reference in your consuming project:
```xml
<ItemGroup>
  <PackageReference Include="Bosak.Xslt" Version="1.0.0" />
  <PackageReference Include="Bosak.XPath.Providers" Version="1.0.0" />
</ItemGroup>
```

> **Note:** Transitive dependencies are automatically resolved. You only need to reference the top-level packages your code directly uses (`Bosak.Xslt` and/or `Bosak.XPath.Api`).

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
using Bosak.Xslt.Api;

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
| `xsl:variable` | ✅ Working | Lexical scoping; `as` attribute with basic atomic types (`xs:integer`, `xs:string`, `xs:boolean`, `xs:double`, `xs:decimal`); usable in XPath via `$var` |
| `xsl:param` | ✅ Working | On named templates, global params, default values; `as` attribute with basic atomic types |
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
| `xsl:attribute-set` / `use-attribute-sets` | ✅ Working | Accumulates across imports/includes; cycle detection; `xsl:next-match` inside attribute sets works |
| `xsl:use-when` | ⚠️ Partial | Top-level and nested elements; `true()`/`false()` evaluation works. Error cases (XTSE0090, XPST0003) not yet validated. |

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

**Unit tests:** 875 passed, 0 failed, 0 skipped  
**Target framework:** `net10.0`

### Behavioral Changes

| Change | Impact | When |
|--------|--------|------|
| Namespace node `parent::node()` now returns the element whose namespace axis includes the node (`_namespaceOwner`), not the element where the underlying `XAttribute` declaration resides. | Fixes `.. is $e` for inherited namespace nodes in XPath. Required for XSLT `namespace::*` axis correctness. | 2026-06-10 |

### Conformance Baselines

| Suite | Passed | Failed | Skipped | Pass Rate | Notes |
|-------|--------|--------|---------|-----------|-------|
| XSLT 3.0 (W3C) | 3,468 | 1,937 | 9,195 | 64.2% | +13 copy cluster tests fixed (namespace axis parent handling) |
| XPath 3.1 (QT3) | 18,785 | 3,085 | 9,951 | 59.04% | Stable |

> **Note:** The conformance runner locks DLLs. If you get build errors about locked files, run:
> ```bash
> taskkill /F /IM Bosak.XPath.Conformance.exe
> taskkill /F /IM Bosak.Xslt.Conformance.exe
> ```

---

## 8. VS Code Extension

Bosak ships with a VS Code extension (`vscode-bosak/`) that provides syntax highlighting, realtime diagnostics, and auto-completion via a Language Server Protocol (LSP) server.

### 8.1 Building & Running

```bash
# Build the language server (.NET 10)
dotnet build src/Bosak.LanguageServer/Bosak.LanguageServer.csproj

# Build the extension client (Node.js 18+)
cd vscode-bosak
npm install
npm run compile

# Launch Extension Development Host
code . --goto src/extension.ts
# Then press F5 inside VS Code
```

### 8.2 Packaging as VSIX

```bash
cd vscode-bosak
npx vsce package
# Produces: vscode-bosak-0.1.0.vsix
```

Install in VS Code: **Extensions** → **⋯** → **Install from VSIX…**

### 8.3 Extension Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `bosak.server.path` | `string \| null` | `null` | Absolute path to `Bosak.LanguageServer` binary. When null, the extension searches the workspace. |
| `bosak.trace.server` | `string` | `"off"` | LSP traffic tracing: `"off"`, `"messages"`, `"verbose"`. |

### 8.4 Supported File Types

| Language | Extensions | Features |
|----------|------------|----------|
| XPath | `.xpath` | Syntax highlight, diagnostics, completions (functions, axes, keywords) |
| XSLT | `.xsl`, `.xslt` | Syntax highlight, diagnostics, completions (XSLT instructions + XPath) |

---

## 9. Getting Help / Reporting Issues

- Check `docs/ARCHITECTURE.md` in the Bosak repo for the layer overview and execution pipeline.
- Check `docs/FEATURE_REQUESTS.md` for the feature request registry.
- XPath failures: capture the expression, input XML, and expected vs. actual result.
- XSLT failures: capture the stylesheet fragment, source XML, and expected output.
