# Consumer flow for the bounded constructor profile

Supply an already acquired assessor and a host-captured ISchemaAssessmentEpisode; the engine never acquires a schema backend or chooses an ambient assessment clock. Check the compile outcome before reading Value. Missing advisor lifetime capability returns Unsupported without activating a constructor.

```csharp
var compiled = SchemaQuerySession.CompileXPath(
    "$constructed", limits, assessor: assessor);
if (compiled.Kind != SchemaResultKind.Supported)
    return compiled.Kind;

using var session = compiled.Value;
return session.Run([], operation =>
{
    var constructed = operation.ConstructSimpleValue(
        XdmValue.FromString("42"), targetType, capturedEpisode);
    if (constructed.Kind != SchemaResultKind.Supported)
        return constructed.Kind; // Invalid and Unsupported are distinct.

    var context = EvaluationContext.CreateIsolated()
        .WithVariable("constructed", constructed.Value);
    var sameValue = operation.Evaluate(context);
    ConsumePayload(sameValue); // Keep authorized use inside this callback.
    return SchemaResultKind.Supported;
});
```

Only plain lexical String is input. Target/governing/declared ownership and authoritative component equality are required; actual atomic member evidence is preserved, never substituted with the target. Supported outputs are one non-list plain String/Boolean/Integer/Decimal with fresh null original assessment. The backend owns lexical/facet validity and exact numeric construction. Precision/coverage inability must be Unsupported rather than rounded success.

Retain the callback task through all use with RunAsync. Cancellation of DrainAsync cancels only that wait. Raw cancellation/close stops resumed guarded delivery while real provider calls and cleanup finish. The engine disposes only its borrowed admission, not the provider's resources. Returned values may retain copied data, not active authority after the raw callback ends. A second raw operation cannot import the receipt even in the same session.

Argument/malformed-evidence failures include foreign handles, wrong target proof, changing episode constants, non-null original assessment, malformed provider outcomes and forged receipts. Revocation/cancellation and actual callback/cleanup exceptions propagate. Numeric/count capacity throws NotSupportedException; unsupported shapes/families return Unsupported. Provider diagnostics are deliberately not transported in this first profile. Invalid has fixed BOSAK-CONSTRUCTION-INVALID and no automatic XPath error-code mapping.

No snapshots or source cache slots are used. Output limits do not bound provider lexical parsing, total strings/bytes, number of construction calls, retained memory or time. Do not infer persistent ownership, native/facet conformance, complete XSD list/union coverage, general casts or stable API freeze from this finite route.
