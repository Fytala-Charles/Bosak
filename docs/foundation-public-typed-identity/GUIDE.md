# Using the bounded public profile

Provide an actual caller-selected ISchemaAssessor and caller-chosen SchemaQueryLimits. Core does not acquire or compile schemas, automatically assess XML, or infer authority from names/annotations. The borrowed provider must implement the represented contract and explicitly support the captured environment; the new profile is not a claim of native Schema/XmlDb/Braid acceptance or full XSD 1.1 backend coverage.

The following assumes `assessor`, `limits`, `input`, `lifetime`, optional `originalAssessment`, and an issued `typedValue` are already supplied by the provider/host composition:

```csharp
var compilation = SchemaQuerySession.CompileXPath("$value", limits, assessor: assessor);
if (compilation.Kind != SchemaResultKind.Supported)
    throw new NotSupportedException("The provider or query profile is unavailable.");

using var session = compilation.Value;
var admission = session.OpenSnapshot(input, lifetime, originalAssessment);
if (admission.Kind != SchemaResultKind.Supported)
    throw new NotSupportedException("This admission requires an unavailable capability.");

using var snapshot = admission.Value;
var result = session.Run([snapshot], operation =>
{
    var read = operation.ReadTypedValue(snapshot, typedValue);
    if (read.Kind != SchemaResultKind.Supported)
        throw new NotSupportedException("This profile requires exactly one atomic cell.");
    return operation.Evaluate(new EvaluationContext().WithVariable("value", read.Value));
});
```

`session.Advisor` is the exact bound borrowed advisor. The caller can use its supported provider operations under the appropriate owner lifetime; Core does not invoke Assess or ConstructSimpleValue automatically. The original assessment must be owned by this binding. Null origin is allowed for a provider-issued value associated with the actual admitted input; it does not grant source-free construction.

The provider's OwnsTypedValue receives the original public input, not Core's private projection. Source/snapshot binding and availability are checked through the actual admission. Foreign snapshot calls refuse before typed getters. On evaluation, Core finds the unique registered marker owner among the raw operation's admitted snapshots before provider metadata use, then validates current container/component/endpoint ownership and the exact payload representation. Cached identity alone is not authority.

ReadTypedValue consumes and validates the complete provider result under MaxTypedValueAtoms. Exactly one cell commits; governing/list shape and declared/selected actual/origin metadata remain internally registered. Empty and multi-atomic containers return Unsupported without publication or a reservation. Invalid metadata and capacity/cancellation failures throw rather than becoming an unmarked fallback. A singleton list is supported through its selected atomic member; count zero does not imply nil/no-value. The public diagnostic for unavailable typed capabilities is `BOSAK-TYPED-IDENTITY-UNSUPPORTED`.

Evaluate requires exactly one unprefixed `value` binding, no focus, and the established data-only context capture policy. Services, custom functions, populated execution state and structured inputs refuse. Namespace/base-URI data cannot enable resource access in the fixed variable-reference query; actual VM execution receives a fresh isolated context with closed resources. Arithmetic, calls, another variable name, sequences and even parenthesized source are outside this first grammar. Surrounding source whitespace is allowed.

Repeated raw operations on the same live snapshot reuse stable resident evidence without payload getter replay. Copied values retain data and private identity, not a lease. Operational reuse after raw completion needs a new live same-session admission interval. Close revokes access and clears registry entries while actual resource release follows raw completion/drain. Canceling a wait does not end raw work. Borrowed assessor/advisor objects are never implicitly disposed by Core.

MaxCachedValuesPerSnapshot counts scalar evidence; MaxTypedValueAtoms bounds one fully consumed result. Numeric digit/scale limits still apply to admitted payloads. These counts are not aggregate byte/time or representative workload budgets. Construction, compatibility, node facts, broader atomization, numeric projection, provenance observation and XQuery/XSLT/native routes remain separate qualification work.
