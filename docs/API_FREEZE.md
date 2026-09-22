# API Freeze Proposal — pre-1.0 audit

> Status: **Draft for decision** — 2026-09-21. Source: reflection-based inventory of all 9 published
> assemblies at v0.10.2-beta (`mult/apidump`, scratch, not committed): **238 public types, ~3,900
> public member entries**. Per-package raw inventories: `mult/apidump/out/*.txt` (regenerate on demand).
>
> The freeze rule once 1.0 ships: everything public stays source- and binary-compatible. Everything in
> the "Internalize" tables below that is still public at the 1.0 tag locks implementation detail into
> the permanent contract. Doing this now, pre-1.0, is the cheap moment.

## Verdict at a glance

| Package | Public types | Keep public (freeze) | Internalize | Reshape/rename first | Human decision |
|---|---|---|---|---|---|
| Bosak.XPath.Core | 29 | ~15 | 8 | 4 renames | 2 (IXdmNode facets, FunctionItem) |
| Bosak.XPath.Api | 9 | 8–9 | 0 | ~4 renames/reshapes | 1 (XSD scope) |
| Bosak.XPath.Providers | 20 | 7 | 10 | 4 renames/reshapes | 3 |
| Bosak.Xslt | 37 | ~11 | 26 | 2 reshapes | 2 (OutputProperties, FunctionLibrary split) |
| Bosak.XQuery | 15 | 3–4 | 11 | 1 reshape | 1 (XQueryStaticContext) |
| Bosak.XPath.Runtime | 16 | 12 (after pruning) | 2 + ~25 members | 4 reshapes | 2 |
| Bosak.XPath.Compiler | 21 | 0 | **all 21** | — | assembly-level only |
| Bosak.XPath.Standard | 5 | 3 | 2 + 4 loose members | 1 reshape | 1 |
| Bosak.XPath.Parser | 85 | **1** (ParseException) | **~83** | naming cleanups | 1 (AST as tooling surface?) |

The layering leaks are concentrated where expected: Parser and Compiler are internal-by-design layers
that are currently fully public. Internalizing them (with `InternalsVisibleTo` for the in-repo
consumers: Runtime, Standard, Xslt, XQuery, LanguageServer) removes roughly **60% of the frozen
surface in two moves**.

## The eight decisions (needed before the freeze work starts)

1. **Parser/Compiler: internalize wholesale?** Recommended yes. The only consumer-relevant Parser
   type is `ParseException`; LanguageServer and the other layers keep access via `InternalsVisibleTo`.
   Alternative: deliberately curate the AST as a public tooling surface (language servers, editors) —
   much bigger design job, recommend deferring until a real consumer asks.
2. **XSD validation scope** (`Bosak.XPath.Api.Xsd` — `XsdValidator`, `IXsdValidator`, result types):
   keep in the Api package, or move to Providers / a future `Bosak.Xsd` package? XSD is not XPath 3.1;
   freezing it inside the Api facade freezes the shape in the wrong home.
3. **`IXdmNode` facets**: the core extension interface has 33 members including DTD/schema facets
   (`InternalSubset`, `PublicId`, `SystemId`, `SchemaTypeAnnotation`, …). Split core vs. optional
   facets, or accept the implementation cost for custom providers? Decide now — it is the
   single most-consumed interface in the stack.
4. **`EvaluationContext` shape**: keep as the blessed extension point (documented hooks:
   `DocumentLoader`, `SchemaResolver`, constructor hooks, `FunctionLookupInterceptor`) but strip
   ~25 engine-state members (`IsXsltMode`, `InStreamingMapContext`, `RegexGroups`, `XQueryModuleLoader`,
   Snapshot/Restore plumbing, tuple-keyed dictionaries) to internal. Alternative: a thinner public
   `EvaluationOptions` facade. Recommended: keep EvaluationContext, prune it.
5. **Custom-function extension surface**: `FunctionItem`/`NamedFunctionItem`/`CurriedFunctionItem`
   currently expose VM capture state with public setters and `Object`-typed members. Decide whether
   custom function libraries are a supported extension point (then redesign: validating ctor, typed
   capture state) or internal.
6. **`XsltExecutable` Transform\* overload soup**: 13 overloads; `TransformCaptured`/`TransformFunctionCaptured`
   leak fn:transform() plumbing (raw `out` params, delivery-format strings). Consolidate into one
   options-based call + result object before signatures freeze.
7. **`XsltFunctionLibrary` split**: global mutable static registry mixing a legitimate consumer hook
   (`RegisterPackage`) with engine internals (`Populate`, `ClearXQueryModuleSources`). Keep a slim
   public registry; internalize the rest.
8. **`XQueryStaticContext` fate**: internalize, or keep as a curated builder-only extension surface
   (drop public setters, fix tri-state `Nullable<bool>`s as enums)?

## Internalize lists (mechanical, no design needed)

- **Parser**: entire `Bosak.XPath.Parser.Ast` hierarchy (~60 nodes + helper records + 5 enums),
  `Ast.XPathParser`, `Lexer.XPathLexer`, `Lexer.Token`, `Lexer.TokenKind` — keep `ParseException`
  public. Optional cheap renames first: `ParseException` → `XPathParseException`.
- **Compiler**: everything — `Ir.IrOpCode` (~150 members), `IrModule`, `IrInstruction`, `IrLowerer`,
  `Optimizer.XPathOptimizer`, `StaticNameTestValidator`, all 12 `*Info` payload records + 2 kind enums.
- **Xslt**: all 21 `Bosak.Xslt.Stylesheet.*` types (contingent on decision 6/OutputProperties fate),
  `Patterns.PatternCompiler` + `PatternPredicate`, `Runtime.TransformEngine`, `Runtime.KeyIndex`,
  `Runtime.ResultTreeSerializer`.
- **XQuery**: `Compiler.XQueryParser` + `XQueryParseResult`, declaration DTOs exposing AST/IR bodies
  (`UserFunctionDeclaration`, `UserVariableDeclaration`, `CompiledUserFunction`, `CompiledUserVariable`),
  `XQueryModuleLoader.Load1/Load2`, `XQueryModuleSource`.
- **Runtime**: `VmEngine` except `Cast`/`TryCast`/`ValueMatchesType`/`ApplyFunctionConversion`/
  `InvokeFunctionItem` (carve into a public `XdmConversions` or keep pruned on VmEngine);
  `XPathError` (kills a Parser-AST type and a 7-tuple leak in one move).
- **Core**: `XPathDateTimeHelper`, `XPathDateTimeExtensions`, `DecimalRangeSequence`,
  `IntegerRangeSequence`, `EnumerableXdmSequence`, `MaterializedSequence` (pending Uses check —
  XQuery/LanguageServer may need `InternalsVisibleTo` from Core).
- **Standard**: `Functions.RegexHelper` (12 statics), `Functions.FormatIntegerEngine`,
  loose helpers `FunctionLibrary.CompareStrings` / `IsAbsoluteUri`.
- **Providers**: the 10 XObject-annotation/bookkeeping types (`DtdElementOnlyAnnotation`,
  `NamespaceInheritanceBarrier`, `NamespaceInheritanceExplicitYes`, `NonPropagatingNamespaceBinding`,
  `ParentlessNamespaceNode`, `ExcludedNamespaceUris`, `NamespaceInheritanceContext`,
  `OriginalPrefixAnnotation`, `PrefixedNamespaceUndeclarations`, `UnparsedEntityAnnotation`).

## Reshape/rename before freeze (mechanical)

- Core: `XdmValue.EffectiveBooleanValue()` → property or `Get…`; `NamedFunctionItem.ArityValue` → `Arity`;
  camelCase record ctor params (`XsQName`, `XdmAttributeValue`, `XdmElementSpec`); constructor DTOs
  (`XdmElementSpec`, `XdmAttributeValue`, `XdmContentItem`) → init-only.
- Api: `IXsdValidator.Validate`/`TryValidate` pair (both return a result — violates Try-pattern);
  `CompileOptions.DefiningElementDefaultNamespace` (ambiguous next to `DefaultElementNamespace`);
  `XsdValidationResult.OnlyErrors/OnlyWarnings` → `ErrorsOnly`/`WarningsOnly`; verify
  `CompileOptions.Default`/`XsdValidatorOptions.Default` are fresh instances, not shared mutable state.
- Providers: `IStreamingDocument.StreamCompleted` (Action property → event); `EnableReplay()` →
  `TryEnableReplay`; `XDocumentProvider.LoadXml(filePath)` → `LoadFile` (LoadXml conventionally
  parses a string); resolve `RecordPostProcessor` duplication (keep `StreamingLoadOptions` only).
- Xslt: `XsltExecutable.LastResultDocumentProperties` setter → get-only/internal (part of decision 6).
- XQuery: tri-state `Nullable<bool>` (`BoundarySpaceStrip`, `WithDefaultEmptyOrderLeast`) → enums.
- Runtime: `FunctionSignature` validating ctor (currently parameterless + 12 unconditional setters);
  seal record clones / init-only on `CoercedFunctionItem`, `DelegateFunctionItem`, `InlineFunctionItem`.
- Standard: `XdmJsonOptions.CharacterMap` (`Dictionary` get+set → read-only interface).

## Freeze as-is (the curated 1.0 public surface, ~70 types)

- **Core**: `IXdmNode` (pending decision 3), `IXdmSequence`, `ISinglePassSequence`, `XdmValue`,
  `XdmSequence`, `XdmArray`, `XdmMap`, `XdmAxis`, `XdmNodeKind`, `XdmValueKind`,
  `OccurrenceIndicator`, `XPathDateTime`, `XsQName`, `XdmValueEqualityComparer`.
- **Api**: `XPath31Expression`, `CompileOptions`, `XPathCompatibility` (+ XSD types pending decision 2).
- **Standard**: `FunctionLibrary` (`TryGetFunction`/`Populate`), `XdmJsonOptions`, `XdmJsonSerializer.Serialize`.
- **Providers**: `XDocumentNode`, `XDocumentProvider`, `XmlStreamingProvider`, `IStreamingDocument`,
  `IStreamingNode`, `StreamingLoadOptions`, `StreamingException` (+ `Xml11NameCodec` if decoding is
  part of the contract).
- **Runtime**: `EvaluationContext` (pruned, decision 4), `XPathFunction` + `XPathFunction0/1/2`,
  `DelegateFunctionItem` (reshaped), `DecimalFormat`, `ErrorDetails`, `XPathErrorException`,
  `XdmValueComparer`, conversion helpers (pruned from `VmEngine`).
- **Xslt**: `XsltCompiler`, `XsltExecutable` (reshaped, decision 6), `IXsltUriResolver`,
  `IXsltMessageListener`, `FileSystemUriResolver`, `StreamingTransformOptions`, `PackageVersion`,
  `PackageVersionResolutionStrategy`, `XsltRuntimeException`, `XsltGlobalVariableException`,
  slimmed package registry (decision 7), `OutputProperties` (pending decision).
- **XQuery**: `XQueryCompiler`, `XQueryExecutable`, `XQueryContext` (+ `XQueryStaticContext`
  pending decision 8).
- **Parser**: `ParseException`.

## Proposed process

1. User rules on the eight decisions above (this document is the agenda).
2. Land internalizations + reshapes as one `refactor(api)!:` batch (pre-1.0 breaking is fine —
   note it in the next beta release notes).
3. Regenerate the inventory; the freeze list becomes `docs/API_FREEZE.md`'s "Freeze as-is" section,
   enforced from then on by review + (optionally) `Microsoft.CodeAnalysis.PublicApiAnalyzers`
   baselines per project.
4. Version promotion to 1.0 only after the regenerated inventory matches the freeze list.
