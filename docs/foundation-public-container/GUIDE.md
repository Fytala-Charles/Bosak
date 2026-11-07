# Ordinary complete-container reads in development sources

Inside an ordinary public typed-identity session compiled for `$value`, use the admitted source snapshot and provider-issued value:

```csharp
session.Run([snapshot], operation =>
{
    var result = operation.ReadTypedContainer(snapshot, providerValue);
    if (result.Kind != SchemaResultKind.Supported) return 0;
    var complete = result.Value;
    var governing = complete.GoverningType;
    var isList = complete.IsList;
    foreach (var item in complete.AtomicItems)
    {
        var plainValue = item.Value;
        var selectedType = item.TypeEvidence.ActualType;
        // Consume the borrowed metadata inside this raw callback.
    }
    return 0;
});
```

This selects complete containers of plain string, boolean, integer and decimal cells within captured atom/digit/scale bounds. Valid unsupported primitive families return Unsupported for the whole result. Empty containers are Supported; their list flag and governing type remain meaningful. Empty does not mean nil or no typed value. TryGetSingleAtomic is true for exactly one cell, including a singleton list; inspect IsList separately.

The method first authenticates exact snapshot/input/origin/container issuance and validates the entire ordered container before delivery. Every later metadata, cursor and evidence access checks originating raw liveness and current ownership of the container and captured types. Provider contents must remain immutable or ownership-version guarded while issued. Cached contents are not re-read through provider metadata getters. A fresh call materializes a fresh complete read; it does not persist empty groups or consume scalar cache slots. Per-call atom bounds do not impose an aggregate retained-output memory bound.

Each enumeration has its own cursor. Current requires a positioned live cursor. Reset is unsupported; Dispose is idempotent callback-free cursor cleanup, including after raw expiry. All other borrowed access expires with the exact operation; a later operation cannot revive a view. The adapter owns no independent lease. A copied SchemaAtomicItem.Value is plain and can survive expiry; its TypeEvidence remains guarded. SchemaAtomics on that plain value returns false. No type-evidence tuple or delivered adapter mints provider issuance or an engine scalar marker on a round trip.

Compatible, numeric, node-data/nilled, construction and other profiles currently return Unsupported for this complete boundary. Existing ReadTypedValue scalar behavior and typed-identity descriptors remain unchanged. The additive signature is Core-selected; final owner/manifest acceptance and package availability remain pending. This guide describes development 1.79.0, not a selected release version.
