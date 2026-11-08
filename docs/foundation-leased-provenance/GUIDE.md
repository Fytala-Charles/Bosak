# Leased compatible provenance in development sources

Inside the matching compatible `SchemaQuerySession.Run` or `RunAsync` callback, call `operation.ReadAtomicProvenance(value)`. Inspect the supported result and consume its `ISchemaAtomicProvenance` properties before the raw operation ends. Other live operation profiles return Unsupported. Selected coverage is registered, non-list, singleton string, boolean, integer or decimal carriers; invalid or rebound carriers are rejected.

`DeclaredType`, `ActualType` and `OriginalAssessment` describe the original source metadata. They do not invent target type handles or grant independent authority. Each getter enters the existing serialized source/target provider window, validates the established operation proof and live ownership, then checks again before returning. Overlapping or reentrant windows refuse. Provider failures retain their exception identity.

The view owns no independent lease and needs no disposal. Raw completion, cancellation or relevant lifetime closure invalidates it. Late reads refuse before provider callbacks. A stable scalar can be used by another raw operation, but that operation must obtain its own view; an old view never acquires the new proof. Do not retain provider handles past their documented lifetime.

The context-free `SchemaAtomics.TryGetProvenance` continues to return false for compatible carriers. General empty/list/union containers remain a separate PV-T01 requirement. The additive operation signature is a Core selection beyond the original draft; final owner/manifest acceptance is pending. Development 1.78.0 is not a selected publication version.
