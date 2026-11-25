# Guarded calendar comparisons

`SchemaQuerySession.CompileCalendarComparison` is an opt-in assessor-bound profile for two admitted scalar calendars. It supports exactly `$left eq $right`, `ne`, `lt`, `le`, `gt` or `ge`, with XML whitespace and a source limit of128UTF-16characters. Supply a fixed implicit timezone in minutes from -840 through840; it becomes part of the immutable compilation descriptor.

```csharp
using var session = SchemaQuerySession.CompileCalendarComparison(
    "$left lt $right", limits, assessor, implicitTimezoneMinutes: 60).Value;
using var snapshot = session.OpenSnapshot(input, lifetime, originalAssessment).Value;
bool earlier = session.Run([snapshot], operation =>
{
    var left = operation.ReadTypedValue(snapshot, leftIssuedValue).Value;
    var right = operation.ReadTypedValue(snapshot, rightIssuedValue).Value;
    return operation.Evaluate(new EvaluationContext()
        .WithVariable("left", left).WithVariable("right", right)).BooleanValue;
});
```

The application supplies its existing immutable/version-guarded assessor, owned input/lifetime, issued singleton values and capacities. Actual type handles must be owned by the selected authority, which must resolve and answer same/derived relations for the relevant XSD calendar types (including dateTimeStamp for dateTime operands). DateTime and dateTimeStamp share a comparison family; time is separate. Primitive calendar component legality and stamp timezone requirements are checked. The issuer still attests schema validity and user facets; this API does not independently validate an arbitrary schema.

Stored exact fractions and extended Gregorian years remain exact. Legacy projection retains stored milliseconds and discards the incidental embedded date of time values; it cannot recover discarded original DTO ticks. Receiving numeric capacities apply to normalized stored seconds for both representations. Time comparison uses one reference date and retains UTC day displacement. The fixed implicit timezone applies only to absent offsets.

Runtime context must contain exactly the two unprefixed variables and default namespace state, with no focus, clock, timezone override, host hooks or resource services. Plain values, foreign sessions/nonmembers, invalid families, revoked sources/types and unsupported authority answers refuse. Both operands may come from different snapshots admitted to the same raw operation. Cancellation and callback boundaries are rechecked. Async work retains raw resources until it completes, while disposal immediately revokes access. Results are plain booleans with no schema authority.

Default compile options and captured namespace/default namespace/BaseUri metadata are supported. Alternate language modes, backwards compatibility, strict static typing, XML1.1line handling, JIT, static variable/type alias maps, callbacks, mutable BCL schema sets and custom/nonordinal collections refuse before assessor binding. The profile does not expose root traversal or complete container adapters.

Twenty cohesive contracts: [adoption](ADOPTION.json), [option bounds](OPTIONS-BOUND.json), [contract](CONTRACT.md), [API review](API-REVIEW.md), [qualification](QUALIFICATION.json), [review](REVIEW.json), [status](STATUS.json). This is bounded XPath query support, not twenty new XSD features. Source1.86.0 is unpublished; native Schema calendar admission, default operators/maps, C45 and publication remain separate.
