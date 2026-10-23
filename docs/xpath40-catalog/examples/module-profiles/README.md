# Order quantity libraries across module boundaries

This synthetic orders vocabulary has namespaced line elements, delimiter-free IDs and optional integer qty attributes. Missing quantity defaults to1; explicit0 remains0. The reports do not validate business constraints, contact external services, persist data or stream input.

All principals declare version 3.0. A baseline library declares 3.0 and spells the default as if/then/else. Its mixed-version partner declares 4.0 and uses `(xs:integer($line/@qty) ?? 1)`. The public host is selected independently through XsltCompiler.ProcessorProfile. Both variants produce the same report under Legacy and Xslt40Draft after the isolated source 1.23.8 default-mode correction; public 1.23.7 remains unchanged.

## Complete principal/library pairs

### include

- baseline: [principal](include/report-baseline/app/main.xsl), [library](include/report-baseline/library/module.xsl).
- module40: [principal](include/report-module40/app/main.xsl), [library](include/report-module40/library/module.xsl).

The principal loads `../library/module.xsl` relative to its own app/main.xsl file URI, using the normal file resolver. Pass that principal URI to Compile; a source string without its base URI does not establish this relative-path contract.

### import

- baseline: [principal](import/report-baseline/app/main.xsl), [library](import/report-baseline/library/module.xsl).
- module40: [principal](import/report-module40/app/main.xsl), [library](import/report-module40/library/module.xsl).

The principal loads `../library/module.xsl` relative to its own app/main.xsl file URI, using the normal file resolver. Pass that principal URI to Compile; a source string without its base URI does not establish this relative-path contract.

### package

- baseline: [principal](package/report-baseline/app/main.xsl), [library](package/report-baseline/library/module.xsl).
- module40: [principal](package/report-module40/app/main.xsl), [library](package/report-module40/library/module.xsl).

Before Compile, register the name `urn:fytala:module-profile:package:report-baseline` or `urn:fytala:module-profile:package:report-module40`, version 1.0, using XsltFunctionLibrary.RegisterPackage and the corresponding absolute library file URI. The manifest supplies these exact names/versions/paths; the runner registers before compilation and clears in finally on success or failure. Tests share the existing XSLT package-registry collection. RegisterPackage is an existing public API, not a new per-compiler registry.

| Instruction/context | Responsibility |
|---|---|
| Principal stylesheet | Bind XSLT/Schema, order and function namespaces; retain principal 3.0. |
| include/import/use-package | Load one library; package names are XSLT package identities, not NuGet package versions. |
| Library stylesheet/package | Declare its own 3.0/4.0 version; packages expose the quantity function as public. |
| Function and line parameter | Parse/default quantity for the supplied line; no caller focus is assumed. |
| Principal lines variable and for-each | Select namespaced rows and emit ID/quantity in source order. |
| Final value-of / sum | Emit row count/total; empty input has a zero total. |
| Output | Produce exact text. |

## Fixtures and exact outcomes

### normal

[normal.xml](normal.xml):

```xml
<orders xmlns="urn:fytala:dispatch"><line id="A" qty="2"/><line id="B"/><line id="C" qty="0"/></orders>
```

All six principal/library pairs under both hosts emit `A=2|B=1|C=0|lines=3;total=3`.

### empty

[empty.xml](empty.xml):

```xml
<orders xmlns="urn:fytala:dispatch"></orders>
```

All six principal/library pairs under both hosts emit `lines=0;total=0`.

### reordered

[reordered.xml](reordered.xml):

```xml
<orders xmlns="urn:fytala:dispatch"><line id="C" qty="0"/><line id="B"/><line id="A" qty="2"/></orders>
```

All six principal/library pairs under both hosts emit `C=0|B=1|A=2|lines=3;total=3`.

### invalid

[invalid.xml](invalid.xml):

```xml
<orders xmlns="urn:fytala:dispatch"><line id="A" qty="oops"/></orders>
```

All six principal/library pairs under both hosts raise `FORG0001` for invalid integer text.

## Execution and limits

CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome executes the real files with the selected processorProfile and principal file URI. Optional packages metadata contains name/version/file fields; omitted metadata preserves prior case setup. Input rows need no external parameters. The manifest mode field does not override XSLT host/effective-version selection for these nonstreaming cases.

The48 report runs are three relationships times two variants times four fixtures times two hosts. Another66 controls cover dependency/root/local versions, bounded future-attribute validation, static use-when and frozen scan refusal. [The audit](../../MODULE-PROFILE-BOUNDARIES.json) and [manifest](../cases.json) list every ID, file, host, registration and expected output/error. Run the root AGENTS.md build/test commands, using its temporary-copy XSLT workaround if required.

This is one direct dependency edge, with local file resolution and exact package-version 1.0. It does not certify import precedence, overrides/visibility, package version ranges, multi-hop graphs, schema/static parameters, authoring, resource policies, streaming or all default-mode contexts. [The entry](../../xslt-instructions/module-profiles.md) states the bounded contract and separate runtime-fix dependency.
