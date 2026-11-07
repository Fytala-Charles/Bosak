# Ordinary XQuery with explicit raw ownership

This API is available in development sources and the qualified candidate, not the older published 1.26.3 packages. Use Bosak.XQuery and its matching dependency closure once exact reviewed artifacts are available. The signature includes an assessor parameter for the represented contract; the current ordinary profile returns Unsupported for non-null assessors or schema imports.

```csharp
using Bosak.XQuery.Api.Schema;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;

// Illustrative explicit limits, not library workload defaults.
var limits = new SchemaQueryLimits(4, 8, 16, 128, 128);
var compiled = SchemaXQueryCompiler.Compile(
    "declare variable $prefix external; for $i in 1 to 3 return concat($prefix, string($i))",
    limits);
if (compiled.Kind != SchemaResultKind.Supported)
    throw new NotSupportedException(compiled.DiagnosticCode);
using var session = compiled.Value;
var values = session.Run([], operation =>
{
    var context = new EvaluationContext().WithVariable("prefix", XdmValue.FromString("item-"));
    var result = operation.Evaluate(context);
    var copied = new List<string>();
    if (result.IsSequence)
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            copied.Add(item.ToString());
    else if (!result.IsUndefined)
        copied.Add(result.ToString());
    return copied.ToArray();
});
// values: item-1, item-2, item-3
```

For XML, first OpenSnapshot with the actual ISchemaAssessmentInput and IQueryOperationLifetime. Pass that snapshot in Run membership, and use its root/data in the evaluation context. Source-free work uses explicit empty membership. Foreign or multiply-owned input nodes refuse before engine getters. Do not supply a fabricated original assessment for ordinary XML.

Consume sequences and borrowed node graphs before the raw callback/task finishes. Returning an enumerator does not retain permission to fetch later. Canceling a caller WaitAsync only cancels that wait; use the raw token and DrainAsync according to the existing session lifetime contract. Already-cached scalar values can remain usable; this API does not recursively detach graphs or guard every existing node getter.

Inputs support direct scalar/node data and admitted node focus. Empty/atomic/node outputs and flat sequences are supported; map/array/function or nested-result profiles refuse. A late streamed refusal/error can follow a previously observed prefix. Runtime services/loaders and populated execution state refuse; static query/module settings are frozen. Existing ordinary numeric behavior remains, and the five limits do not impose a general query-result size, byte/time or exact-numeric budget.

[Qualification and remaining scope](README.md), [implementation policy](IMPLEMENTATION.md), [preview package scope](PACKAGE-SCOPE.json).
