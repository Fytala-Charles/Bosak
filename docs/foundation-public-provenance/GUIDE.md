<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak bounded atomic provenance guide">
  <br><br><h1>Atomic provenance observation</h1>
  <p>Cached type metadata with explicit lifetime and authority limits</p>
</div>

Available in development sources at 1.77.0. This does not identify a published package version. [Release gates](../release-planning/README.md) track artifacts, consumer acceptance and API freeze separately.

## Observing a supported value

```csharp
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;

static void Inspect(in XdmValue atom)
{
    if (!SchemaAtomics.TryGetProvenance(in atom, out var evidence))
        return;

    // Engine lifetime is checked again by each getter.
    ISchemaComponentHandle declared = evidence!.DeclaredType;
    ISchemaComponentHandle actual = evidence.ActualType;
    ISchemaAssessmentOrigin? original = evidence.OriginalAssessment;

    // These are borrowed cached references. Provider operations on them
    // still require the appropriate explicit owner/raw lifetime.
}
```

Use actual engine-admitted atomics from the selected typed-value or constructed-value route. For example, a successful `operation.ReadTypedValue(snapshot, issuedValue)` inside the matching session operation can supply an ordinary registered carrier. A successful `operation.ConstructSimpleValue(...)` supplies a receipt valid only in that raw operation. Plain provider payloads and a separately created `SchemaAtomicItem` do not themselves attach an engine observation marker.

True means the engine can describe the admitted cached declared/actual/origin tuple under its current engine lifetime. It does not mean provider ownership was freshly checked, a compatible-view proof is current, or the carrier may be evaluated in another operation. Runtime typed use continues to perform its original source/authority/currentness checks. Reading observations neither acquires a native lease nor consumes a new scalar/cache/source quota slot.

## Supported and refused routes

| Carrier | Result |
|---|---|
| Genuine ordinary registered scalar, exact representation and live engine session/snapshot/cache | True; cached declared/actual and optional original assessment. |
| Originless ordinary registered scalar | True under the same checks; original assessment stays null. |
| Genuine operation-local constructed receipt in its live raw operation | True; cached types and null original assessment. |
| Current-compatible operation-bound carrier | False/null in this context-free helper, including while a caller happens to be inside an operation. |
| Plain/computed/exact-numeric value or existing flattened plain atomization | False/null; no inference from type labels, QName or equal payloads. |
| Unknown/private legacy, unregistered, rebound, closed or canceled carrier | False/null. |
| Node, sequence, map, array, function or Undefined | False/null; no traversal or container reconstruction. |

Ordinary registered metadata can remain observable between raw operations while its source snapshot/cache and session remain engine-live. Snapshot opening cancellation also expires that observation. A prior raw operation's cancellation does not establish a new authority claim. Constructed metadata expires when its exact raw operation completes, closes or is canceled. No observation promotes a constructed receipt into another operation.

Each returned engine view checks lifetime on all three getters. Snapshot/session closure causes getters to refuse with `ObjectDisposedException`; captured cancellation can cause `OperationCanceledException`. A successful Try call does not guarantee that a later getter will still succeed during concurrent revocation. Keep use under the appropriate explicit ownership scope. Retaining the managed view does not keep native admissions alive, and the API does not recursively guard arbitrary methods on provider objects already obtained from it.

The helper and its provenance getters do not call provider/authority/type/origin getters, proof operations, enumerators or native enter/release callbacks. They return cached references only. If provider state changes without an engine snapshot closing, the observation remains a description of the previously admitted tuple, not a fresh provider-availability assertion; subsequent typed evaluation still revalidates its required live state.

## Containers and current-compatible evidence

A single atomic observation does not expose container governing type, list shape or cardinality. Empty lists can have valid container metadata while supplying no atomic carrier; false for Undefined does not classify that list as invalid. A singleton list remains a list. Per-cell declared and selected actual type references remain ordered and distinct, but observing them does not reconstruct the complete container. General public empty/list/union transport remains an open requirement.

Current-compatible carriers deliberately retain stable payload identities and reprove compatibility in each raw operation. The helper has no operation argument and cannot choose a fresh proof. It therefore returns false without retaining an old issuer, consulting ambient state or granting authority from cached target-type handles. A separately leased observation route/disposition remains required before final freeze. This refusal does not waive that requirement.

A returned observation can be used as a descriptive interface value, but cannot issue a new engine scalar or provider-owned container. Repackaging plain payload and observed handles into `SchemaAtomicItem` does not bypass issued-container ownership or current raw proof. There is no public marker attachment factory.

## Scope and qualification

[64 new controls](POLICY-CASE-MAPPING.json) cover the actual public ordinary and construction paths, callback poison, representation checks, expiry, compatible refusal and the selected internal container boundaries. A bounded integer fixture records zero caller allocations for 1000 warm observations plus three getters. It establishes no native/retained-memory/performance budget or general concurrency claim.

[Open items](OPEN-ITEMS.json) retain leased compatible observation, complete public containers, private legacy disposition and actual provider/owner acceptance. All47 proposed type names are present, but type presence is not completed behavior, API freeze or publication authorization. See the [implementation](IMPLEMENTATION.md) and [release plan](../release-planning/README.md).
