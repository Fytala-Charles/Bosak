# Agent Instructions — Bosak XPath

## File Headers

**Every `.cs` source file** (excluding generated files in `obj/`) must begin with the following header template:

```csharp
// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : <dd MMMM yyyy>
// PURPOSE              : <One-line description of the file's responsibility>
// SPECIAL NOTES        : <Layer-specific note or "Part of the Bosak XPath 3.1 implementation.">
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | <dd-MM-yyyy>   | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
```

### Rules

1. **PURPOSE** — Derive from the XML `<summary>` of the primary type when possible. Keep it under 100 characters.
2. **SPECIAL NOTES** — Use layer-specific guidance:
   - `src/Bosak.XPath.Core/Xdm/*` → "Foundation types for the XQuery Data Model; used by all higher layers."
   - `src/Bosak.XPath.Runtime/Vm/*` → "Part of the register-based virtual machine execution engine."
   - `src/Bosak.XPath.Parser/*` → "Part of the hand-written recursive-descent parser pipeline."
   - `src/Bosak.XPath.Compiler/*` → "Part of the AST-to-IR compilation pipeline."
   - `src/Bosak.XPath.Standard/*` → "Part of the standard XPath / XQuery function library."
   - `src/Bosak.XPath.Api/*` → "Public surface API for compiling and evaluating XPath 3.1 expressions."
   - `tests/*` → "Unit tests verifying correctness of the underlying implementation."
3. **Change History** — When modifying an existing file, append a new row to the change history table with the current date, a bumped version number, and a brief note.
4. **Generated files** — Skip `obj/`, `bin/`, and any file containing "Generated" or "Auto-generated" in the first 200 characters.

## Build & Test

```bash
dotnet build Bosak.sln
dotnet test Bosak.sln
```

Target framework: `net10.0`. All tests must pass before considering a task complete.

> **Note — Application Control workaround on the development machine**
> Windows Application Control blocks the rebuilt `Bosak.Xslt.Tests` assembly in its
> normal `bin\Release\net10.0` directory. If `dotnet test` fails with
> `0x800711C7` for that project, use the provided script instead:
> ```powershell
> powershell -ExecutionPolicy Bypass -File .\run-xslt-tests.ps1 -Configuration Release
> ```
> The script copies the build output to `%TEMP%` and runs `dotnet vstest` from there,
> which bypasses the policy. Other test projects run normally with `dotnet test`.

## Architecture Overview

This is a layered XPath 3.1 implementation:

| Layer | Project | Responsibility |
|-------|---------|----------------|
| XDM Core | `Bosak.XPath.Core` | `XdmValue`, `IXdmNode`, `XdmSequence`, axis/node kinds |
| Parser | `Bosak.XPath.Parser` | `XPathLexer`, `XPathParser`, AST nodes |
| Compiler | `Bosak.XPath.Compiler` | `XPathOptimizer`, `IrLowerer`, opcodes |
| Runtime | `Bosak.XPath.Runtime` | `VmEngine`, `EvaluationContext`, function dispatch |
| Standard | `Bosak.XPath.Standard` | `FunctionLibrary` — `fn:*`, `math:*`, `map:*`, `array:*`, JSON functions |
| API | `Bosak.XPath.Api` | `XPath31Expression` — public compile/evaluate surface |
| XSLT | `Bosak.Xslt` | `XsltCompiler`, `TransformEngine`, `fn:transform()` |
| XQuery | `Bosak.XQuery` | XQuery 3.1 processor — `XQueryCompiler`, `XQueryParser` (prolog), `XQueryStaticContext`, FLWOR, constructors, modules |
| Providers | `Bosak.XPath.Providers` | `IXdmNode` adapters (`XDocumentNode`) |

The execution pipeline is:
```
Source XPath → Lexer → Parser → AST → Optimizer → IR Lowerer → VmEngine.Execute → XdmValue
```

## Collation Support

The implementation supports three collation URIs:
- **Codepoint**: `http://www.w3.org/2005/xpath-functions/collation/codepoint` (default)
- **HTML ASCII Case-Insensitive**: `http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive`
- **UCA**: `http://www.w3.org/2013/collation/UCA?lang=XX;strength=Y;alternate=blanked`

`EvaluationContext.DefaultCollation` is used by 2-argument forms of `fn:compare`, `fn:contains`, `fn:starts-with`, `fn:ends-with`, `fn:substring-before`, and `fn:substring-after`.

UCA `alternate=blanked` maps to `CompareOptions.IgnoreSymbols`. Due to .NET `IsPrefix`/`IsSuffix` bugs with this flag, `StringStartsWith` and `StringEndsWith` use custom `IndexOf` / `LastIndexOf` + match-length verification.

## Known Limitations

- **DateTime year < 1**: `DateTimeOffset` minimum year is 1. Tests using year `-2` cannot pass without switching to a custom date representation.
- **Decimal precision**: .NET `decimal` is fixed-precision (28-29 significant digits). XPath decimal literals and arithmetic that exceed this range are rounded; tests that expect exact results beyond this range cannot pass without an arbitrary-precision decimal implementation.
- **Remote HTTP tests**: `fn-unparsed-text-054a` fetches `https://timeanddate.com`, which answers .NET `HttpClient` with a Cloudflare JS challenge (`Cf-Mitigated: challenge`, HTTP 403) regardless of request headers; the test cannot pass from this environment (its any-of accepts only assert-true).

## Documentation Style (Fytala Docs Kit)

Public-facing Markdown follows the Fytala branding contract in `docs/DOCUMENTATION_STYLE_GUIDE.md` (kit version recorded in `.fytala-docs.json`; canonical assets under `assets/`, renderer settings in `.vscode/settings.json` and `.crossnote/`). Verify rendered output with `docs/DOCUMENTATION_RENDERER_TEST.md`.

**Rules:**
- Canonical docs (`README.md`, `docs/ARCHITECTURE.md`, `docs/FEATURE_REQUESTS.md`, `docs/INTEGRATION.md`) use the branded banner header with meaningful `alt` text; internal handover notes may use a compact heading.
- Mermaid diagrams embed the Fytala `classDef` palette (`current` / `platform` / `external` / `planned`) and use anchors + `fytala-figure-caption` captions when referenceable.
- Do not hand-edit kit-managed assets (`assets/css`, `assets/logos`, `assets/images/brand-swatches`, `.crossnote`); sync them from the Prime docs-kit and keep SHA-256 hashes matching its manifest.

## XML Documentation

All projects generate documentation files (`<GenerateDocumentationFile>true</GenerateDocumentationFile>`). Public APIs must have triple-slash XML comments.

**Rules:**
- Every `public` and `protected` type and member must have a `///` comment.
- Use `<summary>` for the primary description.
- Use `<param>` for every parameter.
- Use `<returns>` for methods with non-void return types.
- Use `<exception>` when a method throws documented exceptions.

---

## Testing Requirements

### Test Organization

| Test Project | Purpose |
|--------------|---------|
| `Bosak.XPath.Core.Tests` | XDM foundation types, pure logic |
| `Bosak.XPath.*.Tests` | One test project per source project |

### Test Patterns

- All tests must pass before a task is considered complete.
- New functionality needs at least one happy-path and one edge-case test.

---

## Versioning

- Use [Semantic Versioning](https://semver.org/) (`Major.Minor.Patch`).
- Bump `Minor` for new features; bump `Patch` for bug fixes.
- Record version bumps in file header change-history tables.

---

## Documentation Sync Checklist (Mandatory)

After **every** successful implementation step, update the following canonical documents before concluding the step or session:

| File | Purpose | When to Update |
|------|---------|----------------|
| `README.md` | Human-facing project overview | Structural or CLI changes |
| `docs/ARCHITECTURE.md` | Layer details, performance strategy, extensibility | New layers, projects, or public APIs |
| `docs/FEATURE_REQUESTS.md` | Living registry of all REQ items | Any REQ status change |
| `docs/INTEGRATION.md` | Consumer integration guide | New public APIs, behavioral changes |
| `docs/AGENT_HANDOVER.md` | Session state and canonical agent context | Major architectural shifts |

**Rule:** If a file was modified during the step, its documentation counterpart must be updated in the same step. No exceptions.

### Pre-Handover Checklist

```markdown
- [ ] `README.md` — Build/test commands and quick-start are current.
- [ ] `docs/ARCHITECTURE.md` — Status reflects current reality.
- [ ] `docs/FEATURE_REQUESTS.md` — Registry table is accurate; "Last updated" date is today.
- [ ] `docs/INTEGRATION.md` — Feature status matrix is accurate.
- [ ] `docs/AGENT_HANDOVER.md` — Date and commit hash match latest; "What Was Built" is complete.
- [ ] `AGENTS.md` — Coding conventions still match reality.
```

---

## Coding Style

- Use `file`-scoped namespaces when possible.
- Prefer `readonly struct` for small value types.
- Use `ArrayPool<T>` or struct enumerators for hot paths; avoid `IEnumerable<T>` boxing on critical loops.
- Keep the public API surface in `Bosak.XPath.Api` thin; delegate to the runtime layer.
