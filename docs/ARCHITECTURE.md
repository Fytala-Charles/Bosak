<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak architecture">
  <br><br>
  <h1>Bosak XPath Architecture</h1>
  <p>Technical design document — XPath 3.1 register-VM engine</p>
</div>

---

## Overview

A high-performance .NET implementation of **XPath 3.1** (with forward-compatibility for 4.0), serving as the expression-engine foundation for the XSLT 3.0 processor and the planned XQuery 3.1 processor.

## Guiding Principles

1. **XDM-First**: XPath operates on the W3C XQuery Data Model (XDM), not raw XML DOM. All inputs are adapted to XDM.
2. **Compile Once, Execute Many**: Expressions are parsed into an AST, optimized, lowered to an Intermediate Representation (IR), and then compiled to a register-based VM. Hot paths can be JIT-compiled to IL.
3. **Zero-Allocation Sequences**: Sequences are lazily evaluated using struct enumerators. We avoid `IEnumerable<T>` boxing on hot paths.
4. **Value Semantics**: XDM values are immutable. Small values (integers, doubles, booleans) are unboxed structs.
5. **Pluggable Backends**: XML can be sourced from `XDocument`, `XmlDocument`, streaming readers, databases, or custom implementations without copying into a proprietary DOM.
6. **Standards Compliance**: XPath 3.1 and XDM 3.1 are the baseline. The type system and function signature model must support higher-order functions and JSON literals for XPath 3.1.

---

## High-Level Architecture

<a id="architecture-layer-stack"></a>

```mermaid
flowchart TB
    subgraph API["📢 Public API"]
        EXP["XPath31Expression"]
        COMP["XPathCompiler"]
        ECTX["EvaluationContext"]
    end

    subgraph STD["📚 Standard Library"]
        FN["fn:*"]
        MATH["math:*"]
        MAP["map:*"]
        ARR["array:*"]
        XS["xs:* constructors"]
    end

    subgraph RT["⚡ Runtime / VM"]
        VM["Register VM"]
        FD["Function Dispatch"]
        SO["Sequence Operators"]
    end

    subgraph CP["🔧 Compiler / IR"]
        ASTO["AST Optimizer"]
        IRL["IR Lowerer"]
        JIT["IL JIT (optional)"]
    end

    subgraph PR["📝 Parser"]
        LEX["Lexer (Span-based)"]
        PAR["Recursive-Descent Parser"]
        AST["AST"]
    end

    subgraph XC["🧱 XDM Core"]
        XV["XdmValue"]
        XN["IXdmNode"]
        XSQ["XdmSequence"]
        XAV["XdmAtomicValue"]
    end

    subgraph NP["🌐 Node Providers"]
        XD["XDocument"]
        XMLD["XmlDocument"]
        X11["XML 1.1 loader"]
        STR["Streaming"]
        CUST["Custom"]
    end

    API --> STD
    STD --> RT
    RT --> CP
    CP --> PR
    PR --> XC
    XC --> NP

    classDef current fill:#F0FFF0,stroke:#518D8F,color:#2F4F4F,stroke-width:2px
    classDef external fill:#FFFFFF,stroke:#293F5F,color:#2F4F4F
    classDef planned fill:#FDF2CF,stroke:#556B2F,color:#2F4F4F,stroke-dasharray:5 5

    class EXP,COMP,ECTX,FN,MATH,MAP,ARR,XS,VM,FD,SO,ASTO,IRL,LEX,PAR,AST,XV,XN,XSQ,XAV,XD,XMLD,X11 current
    class JIT,STR planned
    class CUST external
```

<p class="fytala-figure-caption"><strong>Layer stack.</strong> Dependency direction across the Bosak XPath engine, from the public API down to pluggable node providers; streaming and IL JIT remain planned.</p>

---

## Layer Details

### 1. XDM Core (`Bosak.XPath.Core`)

The XQuery Data Model is the foundation. Every XPath expression evaluates to an XDM value.

#### `XdmValue` (Struct)
A discriminated-union struct representing any XDM value:
- **Atomic**: `xs:string`, `xs:integer`, `xs:decimal`, `xs:double`, `xs:boolean`, `xs:date`, etc.
- **Node**: A handle to an `IXdmNode`.
- **Sequence**: A lazy sequence of `XdmValue`.
- **Function**: Function items (higher-order functions in XPath 3.1).
- **External**: Opaque .NET objects passed into the query.

```csharp
public readonly struct XdmValue
{
    private readonly XdmValueKind _kind;
    private readonly object? _reference; // Nodes, sequences, functions, strings
    private readonly ulong _bits;       // Union overlay for numerics/bools
    // ~32 bytes total
}
```

**Performance notes**:
- Small integers, booleans, and doubles are stored inline in `_bits` (unboxed).
- Strings and larger objects use the `_reference` field.
- Sequences are **not** `List<XdmValue>`. They are lazy `IXdmSequence` objects.

**Effective boolean value**: `XdmValue.EffectiveBooleanValue()` follows XPath 3.1 rules — empty sequence is `false`, a singleton sequence delegates to its item, a multi-item sequence is `true` only if it contains a node, and a multi-item purely atomic sequence raises `FORG0006`.

#### `IXdmNode` (Interface)
An abstraction over any XML node. Implementations are provided for `XDocument`, `XmlDocument`, streaming, etc.

```csharp
public interface IXdmNode
{
    XdmNodeKind NodeKind { get; }
    string LocalName { get; }
    string NamespaceUri { get; }
    string Prefix { get; }
    string StringValue { get; }
    string BaseUri { get; }
    string DocumentUri { get; }  // separate from BaseUri; empty for temporary trees

    // DTD properties (for fn:id / fn:idref / fn:element-with-id and serialization)
    bool HasDocumentType => false;
    string DocumentTypeName => string.Empty;
    string PublicId => string.Empty;
    string SystemId => string.Empty;
    string InternalSubset => string.Empty;

    // PSVI / is-id accessor (used by fn:id and fn:element-with-id)
    bool IsId => false;

    IXdmNode? Parent { get; }
    XdmSequence Children(XdmNodeKind kind = XdmNodeKind.All);
    XdmSequence Attributes(string? localName = null, string? namespaceUri = null);

    // Axis navigation returns lazy sequences
    XdmSequence Axis(XdmAxis axis);
}
```

#### `XdmSequence` (Struct + Enumerator Pattern)
A lazy, struct-backed sequence with zero-allocation enumeration.

```csharp
public readonly struct XdmSequence
{
    private readonly IXdmSequence? _source;
    
    public Enumerator GetEnumerator() => new(_source);
    
    public struct Enumerator : IEnumerator<XdmValue>
    {
        // Struct enumerator: no allocation
    }
}
```

**Why not `IEnumerable<XdmValue>`?**
- Boxing on foreach is unacceptable for high-performance path evaluation.
- Struct enumerators give us C# `foreach` syntax with zero allocations.
- LINQ-style operators (`Where`, `Select`) are reimplemented as struct-returning methods.

---

### 2. Parser (`Bosak.XPath.Parser`)

A hand-written, recursive-descent parser using `ReadOnlySpan<char>` for zero-allocation tokenization.

**Why hand-written?**
- XPath grammar is small but context-sensitive (e.g., `*` can be multiply or `any-name`).
- `ReadOnlySpan<char>` gives us O(1) substring slicing without `Substring` allocations.
- Full control over error messages and recovery.

**Pipeline**:
1. **Lexer**: Tokenizes the XPath string. Tokens contain `(TokenKind, ReadOnlySpan<char>)`.
2. **Parser**: Recursive descent into an AST.
3. **AST**: A simple immutable tree of `XPathAstNode` records.

---

### 3. Compiler (`Bosak.XPath.Compiler`)

Transforms the AST into an executable form.

#### Phase 1: AST Optimizer
- Constant folding (`1 + 2` → `3`), preserving XPath 1.0 negative-zero semantics for unary minus on integer zero
- Predicate analysis (`[1]` → `First()`, `[last()]` → `Last()`)
- Axis merging (`child::foo` + `child::bar` where possible)
- Dead code elimination

#### Phase 2: IR Lowerer
Lowers the AST to a **register-based intermediate representation** (`IrInstruction`). This is similar to LLVM IR but domain-specific for XPath.

```mermaid
flowchart LR
    AST_IN["Optimized AST"]
    IR["IR Instructions<br/>+ Literal Pool"]
    AST_IN -->|LowerNode| IR

    classDef current fill:#F0FFF0,stroke:#518D8F,color:#2F4F4F,stroke-width:2px

    class AST_IN,IR current
```

Example IR for `//book[price > 10]`:
```
LOAD_CONTEXT     r0
DESCENDANT_AXIS  r1, r0, "book"
FILTER           r2, r1, label_1
RETURN           r2

label_1:
CHILD_AXIS       r3, r0, "price"
ATOMIC_VALUE     r4, r3
GREATER_THAN     r5, r4, 10
RETURN           r5
```

#### Phase 3: Bytecode Emitter
Emits the IR into a compact format. `IrInstruction` is an 11-byte struct (`IrOpCode` + 3×`ushort` registers + `int` operand) with `Pack=1`.

#### Phase 4: IL JIT (Optional / Future)
For expressions executed frequently (>N times), the IR can be compiled to a `DynamicMethod` using `System.Linq.Expressions` or `System.Reflection.Emit` for near-native performance.

---

### 4. Runtime / VM (`Bosak.XPath.Runtime`)

A lightweight, register-based virtual machine.

```mermaid
flowchart TD
    IR["IR Module<br/>(Instructions + Literal Pool)"]
    REG["Register File<br/>XdmValue[module.MaxRegisterCount]"]
    IP["Instruction Pointer"]
    CTX["EvaluationContext<br/>Focus + Variables + Functions"]
    OUT["XdmValue Result"]

    IR --> VM["VmEngine.Execute"]
    REG --> VM
    CTX --> VM
    VM --> IP
    VM --> OUT

    classDef current fill:#F0FFF0,stroke:#518D8F,color:#2F4F4F,stroke-width:2px

    class IR,REG,IP,CTX,OUT,VM current
```

**Design**:
- Dynamically-sized `XdmValue` register array sized to the module's max register count (up to 65,536 registers).
- Instruction pointer walks the bytecode.
- Axis instructions delegate to the `IXdmNode` provider.
- Function calls dispatch through a vtable in `EvaluationContext`.
- `FunctionSignature` may supply a `DynamicImplementation` used only when the function is invoked through a function item (named reference or partial application) rather than a static call. The XSLT layer uses this to raise the spec-mandated dynamic errors (`XTDE1061`/`XTDE1071`/`XTDE3480`/`XTDE3510`) for dynamic calls on `current-group`, `current-grouping-key`, `current-merge-group`, and `current-merge-key`, whose context components are not retained in function-item closures.
- `EvaluationContext.ImplicitTimezoneOffsetMinutes` supplies the dynamic context's implicit timezone (default UTC) for date/time comparisons and `adjust-*-to-timezone#1`.
- `EvaluationContext.Collections` maps collection names (empty string for the default collection) to the URIs or file paths of their documents, used by `fn:collection` and `fn:uri-collection`.

**Performance features**:
- **Sequence pipelining**: Axis results are not buffered unless required by sorting or positional predicates.
- **Predicate short-circuiting**: Predicates are evaluated lazily; `//a[b and c]` stops at first false.
- **Document-order preservation**: Maintained by the node provider, not by sorting after the fact.

---

### 5. Standard Library (`Bosak.XPath.Standard`)

Implementation of:
- **fn namespace**: All XPath 3.1 functions (`fn:string`, `fn:concat`, `fn:current-dateTime`, etc.)
- **math namespace**: Trigonometric and logarithmic functions.
- **map namespace**: `map:merge`, `map:get`, etc.
- **array namespace**: `array:size`, `array:get`, etc.
- **xs constructors**: Type constructors cast/validate values.

Each function is implemented as a static method conforming to a delegate signature:
```csharp
public delegate XdmValue XPathFunction(EvaluationContext context, ReadOnlySpan<XdmValue> arguments);
```

Functions are registered in a `FunctionLibrary` that supports runtime extensibility. Functions that accept numeric arguments in XPath 1.0 backwards-compatible mode (e.g. `fn:subsequence`) coerce strings, untyped atomics, and nodes to `xs:double` instead of raising `XPTY0004`.

#### Regular-expression support

Regular expressions are centralized in `RegexHelper` (`src/Bosak.XPath.Standard/Functions/RegexHelper.cs`). It is used by `fn:matches`, `fn:replace`, `fn:tokenize`, `fn:analyze-string`, and `xsl:analyze-string` to:

- Parse `i`, `m`, `s`, `x`, and `q` flags.
- Validate XSD regex syntax and reject invalid patterns (`FORX0002`).
- Translate XSD-specific constructs (e.g. ambiguous backreferences, category escapes) to .NET `Regex` syntax.
- Translate XPath/XSD replacement strings for `fn:replace`.
- Detect patterns that match the empty string (`FORX0003`).
- Build capturing-group parent maps so `fn:analyze-string` emits the correctly nested `<group>` tree required by the spec.

XSD character classes are handled by a dedicated engine rather than .NET's Unicode-category support, because .NET tracks the runtime's Unicode version while XPath/XSD pins specific code-point sets:

- `XsdCharClasses` (`src/Bosak.XPath.Standard/Functions/XsdCharClasses.cs`) is a range-set engine that parses XSD class expressions — `\p{X}` / `\P{X}` (all 38 general categories, including the grouped `LC`), `\p{IsBlock}` script blocks, `\d \D \w \W \s \S \i \I \c \C`, ranges, negation, unions, and class subtraction (`[A-[B]]`, one level of nesting) — and emits .NET-compatible patterns. Astral ranges are emitted as surrogate-pair alternations so matches/replacements never split a supplementary character. `\w` follows the XSD definition `[^\p{P}\p{Z}\p{C}]` (so e.g. emoji of category `So` are word characters). `\i`/`\c` use the explicit XML 1.0 (5th edition) `NameStartChar`/`NameChar` range tables required by XSD 1.1 (these deliberately include code points such as U+212E that are not Unicode letters). XSD 1.1 hyphen rules are honored: `-` is a subtraction operator only when immediately followed by `[`; elsewhere it is a range operator or a literal hyphen (`[a-d-b-c]` = `{a-d, '-', b-c}`, negated or not), and a range endpoint may not be an unescaped hyphen (`[a--b]` → `FORX0002`).
- `UnicodeData90` (`src/Bosak.XPath.Standard/Functions/UnicodeData90.cs`) pins **Unicode 9.0.0** general-category and block ranges as flat sorted `int[]` tables, generated from `DerivedGeneralCategory-9.0.0.txt` / `Blocks-9.0.0.txt`. This satisfies the W3C XSLT 3.0 `unicode-90` conformance set, which requires Unicode 9.0 semantics exactly (e.g. `\p{Nd}` = 580 code points, Adlam block = U+1E900..U+1E95F).
- Both the XSD→.NET pattern translation and the constructed `Regex` objects are cached (keyed by the short original pattern) in `RegexHelper`, so hot loops that evaluate the same literal pattern millions of times pay only a small dictionary lookup. Compiled regexes are used throughout: `RegexOptions.NonBacktracking` was evaluated but rejected because it both refused large alternations and silently mis-matched U+000A on some patterns.
- `fn:codepoints-to-string` validates its input against the XML 1.1 `Char` production (`#x1-#xD7FF|#xE000-#xFFFD|#x10000-#x10FFFF`; Bosak is XML 1.1-capable); noncharacters such as U+FDD0..U+FDEF and astral `xFFFE`/`xFFFF` planes are legal, while surrogates, U+FFFE/U+FFFF, and NUL raise `FOCH0001`.

---

### 6. Public API (`Bosak.XPath.Api`)

The user-facing surface, modeled after `System.Xml.XPath` but modernized.

```csharp
// Compile once
var expr = XPath31Expression.Compile("//book[price gt 10]/title");

// Execute many times against different documents
var result = expr.Evaluate(document);
foreach (var title in result.AsNodes())
{
    Console.WriteLine(title.StringValue);
}

// With context (variables, namespaces, functions)
var ctx = new EvaluationContext()
    .WithNamespace("p", "http://example.com")
    .WithVariable("minPrice", new XdmValue(10.0m));

var result2 = expr.Evaluate(document, ctx);

// With default element namespace (XSLT xpath-default-namespace)
var options = new CompileOptions
{
    Namespaces = new Dictionary<string, string> { ["p"] = "http://example.com" },
    DefaultElementNamespace = "http://example.com"
};
var expr3 = XPath31Expression.Compile("//book", options);

// The XML default namespace of the element containing the expression is tracked
// separately so that XSLT's fn:element-available() can expand unprefixed QNames
// per the XSLT specification, independent of xpath-default-namespace.
options = new CompileOptions
{
    DefaultElementNamespace = null,
    DefiningElementDefaultNamespace = "http://www.w3.org/1999/XSL/Transform"
};
```

### 7. Node Providers (`Bosak.XPath.Providers`)

The provider layer adapts external XML object models to the XDM `IXdmNode` interface.

- **`XDocumentNode`** – adapter for `System.Xml.Linq.XDocument`. This is the default provider used by the API, the XSLT compiler, and the test harness.
- **`Xml11Loader` / `Xml11NameCodec`** – because .NET's `System.Xml` stack only accepts XML 1.0 names, the XML 1.1 provider rewrites XML 1.1 declarations, encodes XML 1.1-only name characters as private-use sentinel sequences, and stores them in `XName`. On output, names are decoded again and C0/C1 controls are serialized as numeric references. Prefixed namespace undeclarations (`xmlns:prefix=""`) are preserved via a placeholder URI and a `PrefixedNamespaceUndeclarations` annotation.
- **Planned providers** – `XmlDocument` adapter, streaming `XmlReader` adapter, and database-backed nodes.

All XML parsing in the XSLT pipeline (stylesheets, source documents, `doc()`, `parse-xml()`, `parse-xml-fragment()`, `fn:transform()`, and the conformance harness) now routes through `Xml11Loader` so XML 1.1 constructs are accepted everywhere.

---

## Performance Strategy

| Technique | Application |
|-----------|-------------|
| `ReadOnlySpan<char>` | Lexer, string comparisons, name tests |
| Struct enumerators | `XdmSequence`, axis iteration |
| `ArrayPool<T>` | Temporary buffers during sorting, sequence materialization |
| Lazy evaluation | Sequences, predicates, path steps |
| Register VM | Expression execution (better cache locality than tree walking) |
| General-comparison integer sets | `=`/`!=` between a single `xs:integer` and a large all-integer sequence uses a cached `HashSet<long>` (e.g. `$validrange[not(. = $c)]` in the unicode-90 tests: 1.1M × 2k pairwise comparisons collapse to O(n)) |
| Cached regex translation/compilation | XSD→.NET pattern translation and `Regex` objects cached by original pattern; compiled regexes reused across millions of `fn:matches`/`fn:replace` calls |
| Persistent map storage | `XdmMap` is backed by `ImmutableDictionary<XdmValue, XdmValue>` plus an order list/index so `map:remove`, `map:put`, and `map:merge` share structure while preserving insertion-order iteration |
| IL JIT (future) | Hot expression compilation to `DynamicMethod` |
| QName interning | `StringPool` for element/attribute names |
| Avoid `System.Xml.XmlNode` | `IXdmNode` adapter pattern prevents DOM locking |

---

## Extensibility Roadmap

| Priority | Phase | Deliverable | Status | Notes |
|----------|-------|-------------|--------|-------|
| 1 | 4 | **XQuery 3.1** | 🚧 In Progress — Phase 4 | Full core FLWOR (`order by`/`count`/`group by`/`window`), direct and computed constructors with constructor-local namespaces, switch/typeswitch, output declarations and serialization, user-defined functions and variables (`declare function`/`declare variable` with function-item coercion and strict declared-type enforcement), library modules (`module namespace`/`import module` with %public/%private visibility), try/catch with named error codes and `err:*` variables, string constructors, ordering features (`ordered`/`unordered`, ordering declarations), spec-correct name tests and constructor in-scope namespaces, namespace undeclaration and declaration static errors (XQST0033/XQST0070, two-phase prolog ordering), inline-function annotations and function-test annotation assertions, character/entity reference validation, map constructors in step position with key disambiguation, `allowing empty` for clauses, computed namespace constructors in content, higher-order function conformance (conversions, absent focus, base-URI capture), duration subtype arithmetic, stable order-by and full switch semantics, `fn:load-xquery-module` (URI resolution with location hints, transitive import closure, external-variable/context-item binding, public-declarations result map), `declare decimal-format` and `declare boundary-space` (with module-local formats), and the full HTML/XHTML serialization matrix (version-dependent void lists, boolean attributes, raw-text elements, CDATA XML islands, XHTML prefix normalization); QT3 harness routes supported XQuery tests (29,745/0, 93.48%). Remaining: residual singles. |
| 2 | — | **XSLT 3.0 packages** | 🔮 Planned | `xsl:package`, `xsl:use-package`. Completes the XSLT 3.0 surface. |
| 3 | — | **Schema awareness / XSD validation** | 🔮 Planned | Cross-cutting feature for XPath + XSLT; clears remaining schema-dependent skips. |
| 4 | 5 | **Streaming** | 🔮 Planned | `IXdmNode` backed by `XmlReader` with look-ahead constraints. |
| 5 | — | **Custom decimal + date-time types** | 🔮 Planned | Replaces .NET `decimal`/`DateTimeOffset` to clear platform-limitation skips. Large effort, low test impact. |
| 6 | 6 | **Database backends** | 🔮 Planned | `IXdmNode` implementations over XML databases. |
| 7 | — | **XPath / XSLT 2.0 legacy certification** | 🔮 Lowest | 3.1/3.0 are supersets; no separate mode planned unless a specific customer requires it. |
| 8 | — | **XPath 4.0 / XSLT 4.0** | 🔮 Lowest | W3C specs are still drafts; wait for Recommendation status. |

---

## XSLT Architecture & Roadmap

XSLT is implemented as a **thin compiler/runtime layer** on top of the existing XPath stack. The XPath engine (parser, compiler, VM, XDM) handles all expression evaluation; XSLT adds stylesheet parsing, pattern compilation, template dispatch, and result-tree construction.

<a id="architecture-xslt-layer"></a>

```mermaid
flowchart TB
    subgraph XSLT_API["📢 XSLT Public API"]
        XC["XsltCompiler"]
        XE["XsltExecutable"]
        XT["fn:transform()"]
    end

    subgraph XSLT_RT["⚡ XSLT Runtime"]
        TE["TransformEngine"]
        TT["TemplateTable"]
        BRT["ResultTreeBuilder"]
    end

    subgraph XSLT_CP["🔧 XSLT Compiler"]
        SL["StylesheetLoader<br/>(xsl:import / xsl:include)"]
        PC["PatternCompiler"]
        IC["InstructionCompiler"]
    end

    subgraph XPATH["Existing Bosak XPath Stack"]
        P["Parser"]
        C["Compiler / IR"]
        V["VM Engine"]
        X["XDM Core"]
    end

    XSLT_API --> XSLT_RT
    XSLT_RT --> XSLT_CP
    XSLT_CP --> P
    XSLT_CP --> C
    XSLT_RT --> V
    XSLT_RT --> X

    classDef current fill:#F0FFF0,stroke:#518D8F,color:#2F4F4F,stroke-width:2px
    classDef platform fill:#E8F4EE,stroke:#5178A8,color:#2F4F4F

    class XC,XE,XT,TE,TT,BRT,SL,PC,IC current
    class P,C,V,X platform
```

<p class="fytala-figure-caption"><strong>XSLT layer.</strong> The XSLT compiler and runtime sit as a thin layer on the shared Bosak XPath stack.</p>

### XSLT Project Structure

```
src/
  Bosak.Xslt/
    Stylesheet/          Stylesheet DOM, template rule table, import/include resolution
    Patterns/            Pattern AST, pattern compiler, match predicate generation
    Instructions/        XSLT instruction compiler (apply-templates, for-each, value-of, etc.)
    Runtime/             Transform engine, result-tree builder, built-in template rules
    Api/                 XsltCompiler, XsltExecutable, public surface
```

### XSLT Implementation Phases

#### Phase 1 — EDI Conversion Core (XSLT 2.0 subset)
**Goal:** Unblock Customer A EDI transforms (Canonical XML ↔ BOD).

| Feature | Status | Notes |
|---------|--------|-------|
| `xsl:stylesheet` / `xsl:transform` | 🎯 Phase 1 | Root element parsing |
| `xsl:template` with `match` | 🎯 Phase 1 | Pattern compilation for element names, wildcards, predicates, union |
| `xsl:template` with `name` | 🎯 Phase 1 | Named template invocation |
| `xsl:apply-templates` | 🎯 Phase 1 | Default mode, `select` attribute |
| `xsl:call-template` | 🎯 Phase 1 | Named template calls with `xsl:with-param` |
| `xsl:value-of` | 🎯 Phase 1 | Text node construction |
| `xsl:copy` | 🎯 Phase 1 | Shallow copy; `@select` attribute support |
| `xsl:copy-of` | 🎯 Phase 1 | Deep copy to result tree |
| `xsl:for-each` | 🎯 Phase 1 | Iteration over sequence |
| `xsl:if` / `xsl:choose` | 🎯 Phase 1 | Conditional logic |
| `xsl:variable` / `xsl:param` | ✅ Implemented | Scoped variables and parameters; static variables/parameters supported with external override; compile-time validation of required/disallowed attributes and `required`/`select` combinations |
| `xsl:element` / `xsl:attribute` | 🎯 Phase 1 | Dynamic element/attribute construction |
| `xsl:text` | 🎯 Phase 1 | Literal text output |
| Built-in template rules | 🎯 Phase 1 | Default handling for text, attributes, elements |
| `xsl:import` / `xsl:include` | 🎯 Phase 1 | **Required for Customer A:** modular stylesheets, shared function libraries |
| `xsl:output` | ✅ Implemented | Serialization method, encoding, indentation, doctype, CDATA sections, URI escaping, content-type meta, byte-order-mark, html-version, suppress-indentation, `use-character-maps` |
| `xsl:sort` | ✅ Implemented | Sorting within `xsl:apply-templates` / `xsl:for-each` |
| `xsl:number` | ✅ Implemented | Number formatting |
| `xsl:key` / `key()` | ✅ Implemented | Indexed lookup |
| Modes (named) | ✅ Implemented | `mode="foo"`, `xsl:apply-templates mode` |
| `xsl:function` | ✅ Implemented | User-defined XPath functions in XSLT; `@name` and `@_name` AVTs are resolved to expanded QNames at parse time using the stylesheet static context (including externally supplied static parameters), with duplicate-name and invalid-name validation |
| Shadow attributes (`_{attr}` static AVTs) | ✅ Implemented | Underscore-prefixed XSLT attributes (e.g. `_version`, `_href`, `_use-when`, `_xpath-default-namespace`, `_static`, `_select`) are evaluated as AVTs at compile time in the current static context and replace the corresponding non-underscore attributes. Shadow attributes on literal result elements are ignored. |
| `fn:transform()` | ✅ Implemented | XPath function invoking XSLT from expressions; full option surface (`stylesheet-location`/`node`/`text`, `package-name`/`package-version` via a package registry, `initial-match-selection`, `initial-template`/`initial-mode`/`default-mode`, `delivery-format` document/raw/serialized, `stylesheet-params`/`template-params`/`tunnel-params`/`static-params`, `global-context-item`, `xslt-version`, `base-output-uri`, `serialization-params`, secondary result-document capture into the result map); callable in static expressions; W3C `fn-transform` set 117/124 passed (7 skipped) |
| Tunnel parameters | ✅ Implemented | `tunnel="yes"` propagation |
| `xsl:mode` | ✅ Implemented | `on-no-match`, `on-multiple-match`, `warning-on-no-match`, `warning-on-multiple-match`, `visibility`, `typed`, `streamable`, `default-mode`, duplicate-declaration checks, and `#unnamed` normalization |
| `xsl:analyze-string` | ✅ Implemented | Regex matching/non-matching children; `regex-group()`; XSLT 3.0 zero-length match semantics |
| `xsl:try` / `xsl:catch` | ✅ Implemented | Error variables (`err:code`, `err:description`, `err:value`, ...); global-variable errors propagate uncaught |
| `xsl:result-document` | ✅ Implemented | Secondary result documents, URI conflict detection, nested principal output, `rollback-output` handling |
| `xsl:iterate` | ✅ Implemented | Stateful iteration with `xsl:param`, `xsl:next-iteration`, `xsl:break`, and `xsl:on-completion` in result-tree and function-body contexts; `iterate` conformance cluster 44/44 |
| XSLT 3.0 packages | 🔮 Phase 3 | `xsl:package`, `xsl:use-package` |
| Streaming | 🔮 Phase 3 | `streamable="yes"` (skeletal support only) |

> **Implementation note:** `TransformEngine.ExecuteXsltInstruction` treats `xsl:fallback`, `xsl:sort`, `xsl:on-empty`, `xsl:on-non-empty`, and `xsl:assert` as no-ops when reached directly. `xsl:sort` is consumed by its parent sorting instruction; `xsl:on-empty`/`xsl:on-non-empty` are handled by dedicated sequence-constructor processing; `xsl:fallback` is processed only as a child of an unrecognized/extension instruction; `xsl:assert` is accepted but not yet evaluated (pending an enable-assertions switch).
>
> **Serialization:** `ResultTreeSerializer` honours `xsl:output`/`xsl:result-document` properties including `method` (`xml`, `html`, `xhtml`, `text`, `json`), `encoding`, `indent`, `omit-xml-declaration`, `standalone`, `version`, `doctype-system`, `doctype-public`, `cdata-section-elements`, `escape-uri-attributes`, `include-content-type`, `media-type`, `byte-order-mark`, `html-version`, `suppress-indentation`, `use-character-maps`, `normalization-form`, `json-node-output-method`, `allow-duplicate-names`, `escape-solidus`, `item-separator`, and `parameter-document`. Named `xsl:output` declarations are resolved across imports/includes by import precedence (higher-precedence definitions override lower ones; equal-precedence definitions are merged). `xsl:character-map` definitions are parsed and merged; multiple maps are applied in declaration order with the first definition winning for duplicate characters. Characters that cannot be represented in the selected `encoding` are emitted as decimal numeric character references, and CDATA sections are split around such characters. Tab, line-feed, and carriage-return characters inside XML attribute values are also written as decimal numeric character references, because .NET's `XmlWriter` emits them literally and XML attribute-value normalization would otherwise turn them into spaces on parsing. For XHTML (`method="xhtml"`), `doctype-public` without `doctype-system` is ignored. XHTML 1.0 (`html-version="1.0"`) uses the HTML 4 empty-element list for self-closing tags; XHTML5 (`html-version="5.0"`) strips the XHTML namespace prefix, serializes HTML5 void elements as empty tags, and preserves root-element case in the DOCTYPE. DOCTYPE literal values are quoted with the delimiter that does not occur in the value, and a default `<!DOCTYPE html>` is emitted only for `html` roots in the HTML/XHTML namespace (or no namespace for HTML). Multiple `xsl:output` declarations are merged (later values override earlier ones; `cdata-section-elements`, `suppress-indentation`, and `use-character-maps` are unioned), and a principal `xsl:result-document`'s properties are captured so `TransformToString` serializes with the correct encoding, character maps, and Unicode normalization. For `method="json"`, top-level maps/arrays produced by the transformation are collected as raw XDM items and serialized through `XdmJsonSerializer`; nested nodes are first serialized using the effective `json-node-output-method` and then JSON-escaped. The same raw-item collection is used for `xsl:result-document` with `method="json"`, `method="adaptive"`, or `build-tree="no"`. For XML, HTML, XHTML, and text methods, maps, arrays, functions, attribute nodes, or namespace nodes at the top level raise serialization error `SENR0001`. The W3C conformance harness also supports the `serialization-matches` assertion by matching the serialized result against a regular expression, including nested `<all-of>` / `<any-of>` combinations. The `text` method serializes only the string-value of text nodes — comment and processing-instruction nodes contribute nothing. When the harness reparses HTML output for tree assertions it first self-closes HTML void elements, and `assert-xml` comparisons strip the serialization-injected Content-Type `meta` element, which is a serialization artifact rather than part of the result tree. Unnamed `xsl:output` declarations are merged across the import/include tree by import precedence via `Stylesheet.EffectiveOutputProperties` (mirroring the named-declaration merge). The HTML serializer minimizes recognized boolean attributes (e.g. `checked` instead of `checked="checked"`) using an allowlist so other attributes always keep their explicit value; `escape-uri-attributes` defaults to true for both the `html` and `xhtml` methods. Method inference honours the XSLT 3.0 backwards-compatibility rule: a stylesheet with `version="1.0"` building an *implicit* result tree (no `xsl:result-document`) whose root is `html` in the XHTML namespace defaults to the `xml` method (tracked via `OutputProperties.StylesheetVersion` / `ExplicitResultDocument`). `xsl:merge` raises `XTDE2210` when corresponding `xsl:merge-key` attributes (`lang`, `order`, `collation`, `case-order`, `data-type`) are present on one source and absent on another, and `xsl:include`/`xsl:import` hrefs resolve against each element's base URI so modules pulled in through DTD external entities resolve nested references correctly.
>
> **JSON / raw collection:** `XdmJsonSerializer` treats array and map members as sequences, so a single node serializes as a JSON string, multiple items as a nested array, and an empty member as `null`. `TransformEngine.IsRawCollectionTopLevel` is scoped to the actual principal or secondary result-document container, preventing raw-item collection from swallowing literal elements inside typed `xsl:variable` bodies.
>
> **Sequence constructors:** `EvaluateSequenceConstructorToItems` uses a placeholder accumulator so sequence-producing instructions (e.g. `xsl:sequence`, `xsl:document`) keep their document order relative to text nodes and literal elements. Zero-length text nodes produced by `xsl:text` and empty text-value templates are preserved as items during sequence collection; they are discarded only when the result is copied into a result tree, where they still break adjacent atomic-value spacing. `CopyToResult` carries over the preceding atomic state when processing a sequence so the first atomic value is correctly separated from a preceding atomic sibling.
>
> **Backwards compatibility:** XSLT 1.0 backwards-compatible mode (`xsl:version < 2.0`) is honoured at compile time and runtime. `CompileOptions.BackwardsCompatible` causes the optimizer to promote integer arithmetic to `xs:double`; the runtime applies first-item rules to function arguments, `xsl:number/@value`, and `to` operands; general comparisons use XPath 1.0 coercion (node-set ↔ boolean, numeric relational comparisons); and `xsl:value-of` without an explicit `separator` outputs only the first item. Key values are stored as strings under BC so that `key()` lookups behave like XPath 1.0.
>
> **Copied-attribute namespace fixup:** When `xsl:copy` or `xsl:copy-of` adds an attribute in a non-default namespace to an element, `TransformEngine` materialises an explicit `xmlns:*` declaration for the namespace. This guarantees that `name()` returns a distinct prefixed name for attributes that share a local name but have different namespace URIs (e.g. `p1:aaa` and `p2:aaa`), matching XSLT semantics.

#### Pattern Compilation Strategy

XSLT **patterns** (`match` attributes) are *not* XPath expressions. They use a restricted syntax:
- `foo` → matches element `foo` (child axis implied)
- `*` → matches any element
- `@*` → matches any attribute
- `foo[bar]` → matches `foo` with a `bar` child
- `a | b` → union of two patterns

The PatternCompiler transforms patterns into **XPath predicates** evaluated against the current node. For example:
- `match="foo"` → compile to `self::foo` predicate
- `match="foo[bar]"` → compile to `self::foo[child::bar]` predicate
- Union patterns compile to a disjunction of predicates
- `match="doc('uri')"` / `match="document('uri')"` → the literal URI is resolved against the stylesheet base URI and compared with the candidate document node's `DocumentUri`

This lets us reuse the existing XPath compiler and VM for pattern matching.

#### Instruction Compilation Strategy

XSLT instructions compile to a **higher-level IR** that the Transform Engine interprets:
- `xsl:apply-templates` → `ApplyTemplates(select, mode)` instruction
- `xsl:for-each` → `ForEach(select, body)` instruction
- `xsl:value-of` → `ValueOf(select)` instruction
- `xsl:element` → `Element(name, namespace, body)` instruction

The Transform Engine walks this instruction tree, evaluating XPath expressions via the VM and building the result tree via `ResultTreeBuilder`.

---

## XQuery Architecture & Roadmap

XQuery 3.1 is implemented as a **thin compiler layer** on top of the existing XPath stack. The XPath engine (parser, optimizer, IR lowerer, VM, and XDM) handles all expression evaluation; XQuery adds a top-level parser for the prolog and query body, a static context, and (in later phases) constructors and serialization.

<a id="architecture-xquery-layer"></a>

```mermaid
flowchart TB
    subgraph XQ_API["📢 XQuery Public API"]
        QC["XQueryCompiler"]
        QX["XQueryExecutable"]
        QCTX["XQueryContext"]
    end

    subgraph XQ_CP["🔧 XQuery Compiler"]
        QP["XQueryParser<br/>(prolog + top-level syntax)"]
        SC["XQueryStaticContext"]
    end

    subgraph XPATH["Existing Bosak XPath Stack"]
        XP["XPathParser"]
        XO["Optimizer"]
        XL["IrLowerer"]
        XV["VmEngine"]
        XX["XDM Core"]
    end

    XQ_API --> XQ_CP
    XQ_CP --> XP
    XP --> XO
    XO --> XL
    XL --> XV
    XV --> XX

    classDef current fill:#F0FFF0,stroke:#518D8F,color:#2F4F4F,stroke-width:2px
    classDef platform fill:#E8F4EE,stroke:#5178A8,color:#2F4F4F

    class QC,QX,QCTX,QP,SC current
    class XP,XO,XL,XV,XX platform
```

<p class="fytala-figure-caption"><strong>XQuery layer.</strong> The XQuery prolog parser and static context compile down to the shared Bosak XPath stack.</p>

### XQuery Project Structure

```
src/
  Bosak.XQuery/
    Parser/              XQueryParser and XQueryStaticContext
    Compiler/            Static context and prolog analysis helpers
    Api/                 XQueryCompiler, XQueryExecutable, XQueryContext
```

### XQuery Implementation Phases

#### Phase 1 — Foundation (current)
**Goal:** Compile and execute prolog-less XQuery expressions by delegating the query body to the XPath pipeline.

| Feature | Status | Notes |
|---------|--------|-------|
| `XQueryCompiler` / `XQueryExecutable` / `XQueryContext` | ✅ Implemented | Public API wired to XPath parser, optimizer, IR lowerer, and VM |
| `XQueryParser` | ✅ Implemented | Parses version declaration and basic prolog declarations; delegates `Expr` to `XPathParser` |
| `XQueryStaticContext` | ✅ Implemented | Holds namespaces, default element/function namespace, default collation, base URI, declared variables, and declared functions |
| Prolog-less queries (`for`/`let`/`where`/`return`) | ✅ Implemented | Reuses XPath 3.1 FLWOR support |

#### Phase 2 — Full core FLWOR
**Goal:** Add `order by`, `group by`, `count`, and `window` clauses.

| Feature | Status | Notes |
|---------|--------|-------|
| `order by` | ✅ Implemented | Tuple-based lowering with `OrderBy`/`TupleBind` opcodes; supports ascending/descending, empty least/greatest, and collation |
| `count` | ✅ Implemented | Compiler-managed integer counters during tuple construction/post-`order by` iteration; no new VM opcode |
| `group by` | ✅ Implemented | `GroupBy` opcode merges the tuple stream by grouping-key equality; `:=` specs lowered as synthetic lets; post-group `order by` via a re-keying pass |
| `window` | ✅ Implemented | `Window` opcode; tumbling/sliding windows with start/end condition blocks and current/positional/previous/next vars, `only end` |

#### Phase 3 — Constructors and XQuery-specific expressions
**Goal:** Support direct and computed constructors, `typeswitch`, `switch`, and `validate`.

| Feature | Status | Notes |
|---------|--------|-------|
| Direct element/attribute constructors | ✅ Implemented | Lexer constructor mode + `ConstructElement` opcode; computed attributes/content, constructor-local namespaces, copy semantics |
| Computed constructors | ✅ Implemented | `ConstructComputed` opcode + shared content accumulator; static EQName or computed `{expr}` names; all seven forms (`element`/`attribute`/`document`/`text`/`comment`/`processing-instruction`/`namespace`) |
| `switch` / `typeswitch` | ✅ Implemented | Desugared in the lowerer to `let` + `if`/`eq`/`instance-of` chains; sequence-type unions in case clauses |
| `validate` | 🔮 Phase 3 | Lower to conditional IR |

#### Phase 4 — Modules, serialization, and advanced features
**Goal:** Library modules, `import module`, and serialization.

| Feature | Status | Notes |
|---------|--------|-------|
| Serialization | ✅ Implemented | All six output methods (`xml`, `xhtml`, `html`, `text`, `json`, `adaptive`) with full Serialization 3.1 parameter fidelity; ser/* sets and fn/serialize all green |
| Output declarations | ✅ Implemented | `declare option output:*` with static parameter merging and `output:parameter-document` |
| Library modules | ✅ Implemented | `module namespace`, `import module` with location hints, transitive import graph, %public/%private visibility, per-module static contexts |
| Advanced prolog options | 🔮 Phase 4 | Boundary-space, construction mode, ordering mode, copy-namespaces, decimal formats, context-item declaration |

### Parser Layering Strategy

`XQueryParser` owns the XQuery top-level grammar (version declaration, prolog, and query body) but delegates all `Expr` parsing to the proven `XPathParser`. This keeps the XPath lexer/parser free of XML-like tokenization rules and avoids regressions in XPath conformance. The XQuery AST is a separate `XQueryAstNode` hierarchy that wraps XPath AST nodes where appropriate.

---

## Project Structure

```
src/
  Bosak.XPath.Core/         XDM types, sequences, atomic values
  Bosak.XPath.Parser/       Lexer, parser, AST
  Bosak.XPath.Compiler/     Optimizer, IR, bytecode emitter
  Bosak.XPath.Runtime/      Register VM, execution context, function dispatch
  Bosak.XPath.Api/          Public API, expression compilation, navigator
  Bosak.XPath.Standard/     Standard function library (fn, math, map, array, xs, JSON)
  Bosak.XPath.Providers/    XDocument adapter (XDocumentNode), XML 1.1 loader/codec; XmlDocument and streaming adapters planned
  Bosak.Xslt/         XSLT 2.0/3.0 processor (stylesheet compiler, transform engine)
  Bosak.XQuery/       XQuery 3.1 processor (query compiler, static context, prolog parser, FLWOR engine)
  Bosak.LanguageServer/   LSP server for XPath / XSLT diagnostics & completions (OmniSharp 0.19.9)

tests/
  Bosak.XPath.Core.Tests/
  Bosak.XPath.Parser.Tests/
  Bosak.XPath.Runtime.Tests/
  Bosak.XPath.Api.Tests/
  Bosak.XPath.Standard.Tests/
  Bosak.Xslt.Tests/   XSLT transform and pattern-matching tests
  Bosak.XQuery.Tests/ XQuery placeholder tests
```

The published NuGet package will likely be a single `Bosak.XPath` assembly (produced via ILMerge or source consolidation) to simplify deployment, while development maintains the layered project separation.

---

<div align="center" style="background:#2F4F4F; color:#F0FFF0; padding:1rem; border-radius:12px; margin-top:2rem;">
  <p style="margin:0; font-family:Poppins,Segoe UI,sans-serif;">
    <strong>© Fytala</strong> — Bosak XPath Engine Architecture
  </p>
</div>
