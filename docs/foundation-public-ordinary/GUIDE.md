# Ordinary-source session integration

The opt-in `Bosak.XPath.Api.Schema` facade exposes the initial `bosak-ordinary-string-boolean-v1` profile. Existing XPath APIs retain their existing profiles. The new public contracts are additive engineering source at 1.45.0; package publication and API freeze remain pending.

## Compile and evaluate

```csharp
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Runtime.Vm;

var limits = new SchemaQueryLimits(
    maxOpenSnapshots: 4, maxCachedValuesPerSnapshot: 128,
    maxTypedValueAtoms: 64, maxNumericDigits: 1024, maxDecimalScale: 256);
var compiled = SchemaQuerySession.CompileXPath("'hello'", limits);
if (compiled.Kind != SchemaResultKind.Supported)
    throw new NotSupportedException(compiled.DiagnosticCode);
using var session = compiled.Value;
var result = session.Run([], operation => operation.Evaluate(new EvaluationContext()));
Console.WriteLine(result.StringValue);
```

The numbers above are example caller selections, not workload defaults. The default limits struct is invalid. Only the snapshot count is exercised by this initial profile: schema caches, typed enumeration and the numeric evaluation channel are unavailable. Passing larger limits does not enable unavailable operations.

Supported syntax includes string/boolean literals and bindings, admitted node paths, predicates using effective boolean value, `and`/`or`, `if` branches, and `fn:string`, `boolean`, `true`, `false`, `name`, `local-name`, `namespace-uri`, `exists`, `empty`, `not`. Top-level results must be copied strings or booleans. Numeric literals refuse before numeric parsing; arithmetic, comparisons, numeric constructors, other functions, maps, arrays, dynamic calls and node-result queries return Unsupported at compilation. A variable query can compile, but supplying a node result refuses at evaluation. Navigation is not a hard time or graph-size budget.

Supported static options are copied before profile validation and compilation. Custom/nonordinal collections, callback services, mutable BCL schema sets, non-3.1/backward-compatible/XML-1.1 modes and type-alias/record/variable-type maps require another profile. Function namespace aliases must resolve to the standard function namespace; matching local names alone do not admit functions. Environment fields and fingerprints describe the captured compilation; comparing their text never proves authority.

## Supply an ordinary XML snapshot

Implement `ISchemaAssessmentInput` and `IQueryOperationLifetime` in trusted host composition. Their names allow later assessment composition, but an ordinary input requires no assessor or Schema-issued projection. The input owner supplies live node membership, resource availability and unchanged snapshot/version checks. `OwnsNode` must verify its own issuance or stable identity registry before reading a purported node. Node getters, QNames, namespace labels, digests and schema annotations cannot establish ownership. Projected descendants must preserve the same source ownership and owner resource policy.

`Enter` admits already-owned resources and returns an idempotent `IQueryOperationLease`. Core retains that admission until snapshot close and all raw work finish. Lease disposal releases only the admission; Core never disposes the provider's snapshot, stream, assessor or database. Keep source/snapshot binding objects stable by reference for the admission's lifetime. Availability calls receive compilation/admission or raw-work cancellation as appropriate; implementations must honor both revocation and cancellation.

```csharp
// input and lifetime come from trusted host/provider composition.
var admitted = session.OpenSnapshot(input, lifetime);
if (admitted.Kind != SchemaResultKind.Supported)
    throw new NotSupportedException(admitted.DiagnosticCode);
using var snapshot = admitted.Value;
var text = session.Run([snapshot], operation =>
{
    var root = operation.GetRoot(snapshot);
    return operation.Evaluate(new EvaluationContext()
        .WithFocus(Bosak.XPath.Core.Xdm.XdmValue.FromNode(root), 1, 1)).StringValue;
});
```

Use a session compiled with `string(.)` for the second example. Membership is an explicit immutable set of unique snapshots from that exact session. Focus and node variables must have exactly one owner in the complete set. The engine checks those incoming nodes before their getters. Scalar variables may be direct strings/booleans; numeric/structured values and extra context services refuse before dispatch. Dynamic input dictionaries are copied using their observed finite cardinalities; concurrent caller mutation is unsupported. Custom functions, loaders, gates, resource policies and ambient context services cannot be silently carried into the closed evaluation context.

## Lifetime and refusals

`GetRoot` returns a borrowed node for the callback interval; it does not detach or confer ownership. Callback results are not recursively copied. Do not retain an operation for later use. A raw asynchronous callback must include all deferred node consumption. Canceling a wait on its task or on `DrainAsync` does not end the callback or release its owner admission.

Open-session drain observes the current busy epoch, including pending admission/rollback and source-free work, and allows later admissions. Do not await a drain from the callback it is waiting for. `Dispose` revokes admissions immediately; subsequent closed-session drain also waits for deferred cleanup and preserves cleanup errors. Callers must observe the raw task and closed drain even after abandoning a canceled wait. All acquired admissions have rollback obligations on failure; primary and cleanup errors are retained together.

Assessor binding, original assessment, compatibility, `ReadTypedValue` and `ConstructSimpleValue` explicitly return Unsupported in this profile without invoking purported provider authority/getters. Typed reads still require a live operation and exact member snapshot before refusal. The transitive provider interfaces/data records support subsequent owner implementation; data records do not validate authority or automatically establish complete Valid assessment. Consuming providers must validate budgets, modes, component membership, provenance, diagnostic ranges and complete-valid root invariants. `SchemaAtomics`, exact decimals, XQuery and XSLT facade declarations remain unpromoted.

Invalid arguments, duplicate/foreign/ambiguous membership, binding drift, revoked admissions and cancellation throw argument, availability or cancellation errors. Unsupported query/profile choices use explicit results; unsupported dynamic inputs/results raise `NotSupportedException` because `Evaluate` returns `XdmValue`. Consult `Kind` before reading `SchemaResult.Value`; Unsupported has no value.
