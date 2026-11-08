# Compatible complete-container reads in development sources

Inside the callback of a public compatible typed-identity session, use the exact admitted compatible snapshot and original provider-issued source container:

```csharp
compatibleSession.Run([compatibleSnapshot], operation =>
{
    var result = operation.ReadTypedContainer(compatibleSnapshot, providerValue);
    if (result.Kind != SchemaResultKind.Supported) return 0;
    var complete = result.Value;
    var governing = complete.GoverningType;
    var isList = complete.IsList;
    foreach (var item in complete.AtomicItems)
    {
        var plain = item.Value;
        var selectedSourceType = item.TypeEvidence.ActualType;
        // Consume the borrowed type handles in this raw callback.
    }
    return 0;
});
```

The existing [ordinary complete-container route](../foundation-public-container/GUIDE.md) remains available. Compatible support selects complete containers with plain string, boolean, integer or decimal cells within captured atom/digit/scale bounds. Empty containers are Supported; list and governing metadata remain meaningful for zero/one/multiple members. Per-cell declared/selected handles and original assessment describe the source. They are not rewritten into target authority handles. Unsupported families refuse the whole result; malformed members and invalid ownership remain errors.

Complete materialization and each container getter, enumerable/cursor acquisition, MoveNext, Current, Reset and evidence getter run in the originating raw operation's serialized paired source/target provider window. Each checks the established positive proof, exact endpoints/environment, live source input/origin/container and type ownership. There is no fresh Establish per getter and no ambient/latest proof. Overlap and reentry refuse before a second provider window; a failed overlapping MoveNext does not advance the cursor. Reset remains unsupported after live checks. Cursor Dispose is idempotent callback-free cleanup, including after expiry.

The adapter and evidence have no independent lease and expire with that exact raw operation or relevant lifetime/cancellation. A later raw operation must make a new complete read; it cannot revive the old view. In-flight calls retain existing admissions until completion. Copied plain item.Value remains usable after expiry; TypeEvidence remains guarded. SchemaAtomics on those plain values returns false. Delivered adapters and copied tuples cannot mint provider issuance, compatibility authority or scalar markers.

The route creates no scalar registry entries or persistent empty-group cache. Repeated calls perform fresh complete admission; source contents must remain immutable or ownership-version guarded. Existing scalar identity/evaluation and profile descriptors are unchanged. Per-call atom limits do not bound aggregate caller-retained output memory. Wider primitive/persistent/construction/nil/no-value coverage, legacy disposition, native/performance and final owner/freeze acceptance remain pending. This guide describes development 1.80.0; no package release version is selected.
