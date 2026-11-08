<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak public XSLT capture guide">
  <br><br><h1>Captured ordinary XSLT</h1>
  <p>Explicit source ownership, bounded serialized output and local run diagnostics</p>
</div>

Available in development sources at 1.76.0. This guide does not identify a published package version. The [release gates](../release-planning/README.md) track artifacts and acceptance separately.

## Source-free capture

```csharp
using System.Collections.Immutable;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.Xslt.Api.Schema;
using Bosak.Xslt.Capture;

var stylesheet = """
    <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
      <xsl:output method="text" encoding="UTF-8" byte-order-mark="no"/>
      <xsl:template name="main">Hello</xsl:template>
    </xsl:stylesheet>
    """;
var compiled = SchemaXsltCompiler.Compile(stylesheet,
    new SchemaQueryLimits(4, 8, 16, 128, 128));
if (compiled.Kind != SchemaResultKind.Supported)
    throw new NotSupportedException("Compiler profile is outside this captured policy.");
var executable = compiled.Value;
using var session = executable.Session;
var result = executable.RunCaptureTransform(
    ImmutableArray<SchemaQuerySnapshot>.Empty,
    new SchemaXsltInvocation(initialTemplate: "main"),
    new XsltCaptureOptions
    {
        MaxSecondaryOutputCount = 4,
        MaxBytesPerOutput = 4096,
        MaxTotalBytes = 8192,
        MaxMessages = 4,
        TimeLimit = TimeSpan.FromSeconds(10),
        MaxEngineOperations = 100000
    });
if (result.IsSuccess)
{
    var bytes = result.Result!.Capture.Principal!.Bytes;
    // These managed output bytes remain available after session disposal.
}
```

The five session limits retain their documented snapshot/cache/typed-atom/numeric units. They do not become default ordinary-query item/byte/time budgets. Capture independently requires all six controls shown above. Count and byte caps may be zero; time and operation limits must be positive. Copying freezes options for the invocation, without a concurrent caller-mutation guarantee.

## Invocation and input ownership

`SchemaXsltInvocation` is readonly and independently identifies `InitialMatchInput` and `GlobalContextInput`. Each supplied role must be an admitted document root from this executable's session and included in the operation membership. A shared snapshot can supply both roles; membership still lists it once. Match input never supplies absent global context. Source-free templates use explicit empty membership. Initial template/mode names use the existing XSLT host convention; named modes, `#default` and `#unnamed` are supported. `BaseOutputUri` identifies the principal and resolves secondary identities; it does not authorize output IO.

Hosts open input snapshots through `Session.OpenSnapshot` with their real assessment-input and operation-lifetime owners. Dynamic node focus/variables must have exactly one admitted owner across the complete set. Admission precedes role/root getters. Direct scalar values and admitted nodes are selected; maps, arrays, functions, custom dynamic services and caller resource policies refuse. Context data is copied into an isolated child using actual rechecked binding/namespace cardinalities. Effective frozen statics win over same-name dynamic bindings.

The compiler captures principal stylesheet text/base URI, effective static parameters and selected settings. It refuses non-null assessors, schema-aware/nonlegacy compilers, external include/import/package/schema routes, conditional/shadow preprocessing and mutable resolver/listener services. No assessor callback or external fallback is invoked. Unsupported static profiles return `SchemaResultKind.Unsupported`; malformed source and compilation errors retain exceptions. Compiler cancellation is checked before/after compilation, with no mid-parse hard deadline promise.

## Borrowed and owned results

`Transform(operation, invocation, context)` returns a borrowed ordinary graph. Consume or explicitly copy it inside the matching `Session.Run`/`RunAsync` callback. `RawResult` follows the existing ordinary host convention. Secondary results stay captured internally and are not delivered to files/network by this profile. Returning a graph does not recursively detach or guard every node or cached value.

`CaptureTransform(operation, invocation, options, context)` retains the borrowed raw admission through actual output serialization and returns a managed capture envelope. Raw revocation can prevent this direct call from delivering the envelope. Capture requires `RawResult=false`. `options.Context`, `InitialTemplate`, `InitialMode` and `BaseOutputUri` must all be null: the method's context and invocation are the sole input/selection sources. Duplicate/missing settings throw `ArgumentException`; unsupported runtime context/role shapes throw `NotSupportedException`.

`RunCaptureTransform` owns the complete raw operation and returns only after actual in-flight work and all cleanup attempts finish. Its result separates:

| Property | Meaning |
|---|---|
| `Result` | Detached capture and run-local receipts when produced, including diagnostics retained after final refusal. |
| `AdmissionError` | Original raw-entry failure; no capture producer ran. |
| `WorkError` | Original callback/final delivery failure when the borrowed path could not deliver normally. |
| `CleanupErrors` | Every original cleanup error in reverse acquisition order; aggregate boundaries and repeated identities remain intact. |
| `IsSuccess` | Successful classified capture with no execution, admission, work or cleanup error. |

`Result.Capture` contains the existing serialized outputs, effective properties, bounded messages and classified failures. Completed secondary outputs survive subsequent transform/serialization failure. `Partial` and `Cancelled` are unsuccessful. `Result.ExecutionErrors` retains identity-deduplicated original execution/serialization exceptions, including failures otherwise swallowed or wrapped by classification. It does not flatten provider aggregates or recursively sanitize exception graphs. A capture result can coexist with work/cleanup errors; presence alone is not success or proof of native release.

`Result.ResourceReceipts` contains actual acquisition-attempt receipts from this run's fresh closed policy, excluding compilation and earlier invocations. No external acquisition route is selected. Descriptors, URI strings and receipts do not grant provider authority. Receipt/exception graphs are local diagnostics and may contain sensitive paths/messages; this API is not an IPC/redaction format.

## Bounds and cancellation

Principal and secondary bytes use the actual existing XML/HTML/XHTML/text serialization paths and effective output properties, including encoding/BOM bytes. Count/byte limits apply during capture production; messages use their separate count channel. Byte-bounded unsupported serialization profiles remain classified refusals. Message count does not bound individual message length. Serialized-byte limits do not bound tree construction, normalization, XPath working memory, source graphs or compilation.

The raw cancellation token and optional capture token are linked. A supplied internal inherited execution gate and local time/operation controls compose without weakening either. Controls are cooperative at engine/serialization checkpoints; a long XPath expression or provider callback between checkpoints has no hard interruption guarantee. Legacy capture gate precedence remains unchanged on legacy entry points. An abandoned/canceled caller wait does not release raw leases or terminate actual serialization early; session close revokes access while actual work retains ownership until completion. Process/worker termination and representative owner resource budgets remain separate integrations.

[Qualification and exclusions](IMPLEMENTATION.md) retain 67 public controls and [the C01-C10 mapping](FOLLOWUP-MAPPING.json). [PV-01](PLAN.json) is next; broader provenance/container/numeric/schema/native and release requirements remain unwaived.
