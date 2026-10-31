# F140 — Exception-safe streaming completion

30 October 2026. **Implemented and locally qualified in unpublished source candidate 1.34.2.** This resolves the completion-callback cleanup gap reproduced by [the ownership review](../foundation-result-ownership/DECISION.md). It does not settle general early-abandonment/escaped-result ownership or freeze any API.

## Behavior

The streaming pump now attempts disposal of its owned XmlReader after completion handlers, even when a handler throws. Caller-supplied readers remain caller-owned; CloseInput still controls closure of an owned reader's input. Completion is still marked Done before callbacks, raised once, and uses ordinary multicast short-circuiting.

- Callback-only failure: the original callback exception and stack propagate.
- Disposal-only failure: the original disposal exception propagates.
- Both fail: AggregateException retains the original callback exception first and disposal exception second. Neither failure is silently discarded.

The [pre-implementation adoption](ADOPTION.md) and [immutable source locks](ADOPTION.json) record exact C# sections, supporting runtime implementation, Bosak ownership contracts and the intentional dual-error policy. No W3C error identity, public member, required interface slot or carrier layout changes. Source version advances from 1.34.1 to 1.34.2; nothing is published, tagged or adopted by another project.

## Validation

[RESULTS.json](RESULTS.json) and compressed original TRX receipts retain exact executions and hashes:

| Check | Result |
|---|---|
| New regression cases against original code | 6 failed / 5 passed; original failure evidence retained |
| Focused cases after fix / final rebuild | 11 passed / 0 failed each; overlaps full suite |
| Complete Release solution | 24,225 passed / 0 failed across ten projects, including all 8,196 XSLT cases |
| Release build | Initial full build: zero errors, five existing nullable warnings; final incremental build: zero warnings/errors |
| Frozen QT3 | 31,397 passed / 0 failed / 424 skipped; exact retained names/reasons unchanged |
| Gated qt4 | 8,941 passed / 0 failed / 1,918 skipped; 230 gated sets, 11 experimental passes; exact retained names/reasons unchanged |
| Compiled public API comparison | All 5,766 declaration/attribute/field-shape entries identical across 11 assemblies / 270 types; patch assembly identities recorded separately |

The tests exercise normal completion, CloseInput true/false, supplied readers, callback cancellation, disposal-only and simultaneous faults, empty roots, retained replay, repeated Drain and multicast handlers. This is local Windows/.NET 10 qualification, not an independent consumer/package ABI certificate. Application Control did not require the XSLT workaround on this run.

[Candidate source](evidence/candidate-source.json) records exactly three changed production files: StreamingSource implementation/history, IStreamingDocument remarks/history and source package version. The new regression file is separate. Earlier source-hash verifiers intentionally identify those changes; do not overwrite old API/lifetime receipts. The old LO-07 observation applies to the pre-fix revision and is superseded only for this completion path. Other historical early-exit findings remain open.

## Reproduce

```powershell
python docs/streaming-completion-cleanup/verify.py
dotnet build Bosak.sln -c Release
dotnet test Bosak.sln -c Release --no-build
dotnet test tests/Bosak.XPath.Providers.Tests/Bosak.XPath.Providers.Tests.csproj -c Release --no-build --filter FullyQualifiedName~StreamingCompletionTests
dotnet run --project tests/Bosak.XPath.Conformance -c Release --no-build
$env:BOSAK_QT4_GATE_ONLY = '1'
dotnet run --project tests/Bosak.XPath.Conformance40 -c Release --no-build -- tests/qt4tests
```

Use the repository's documented XSLT workaround only if Application Control blocks a later run. New gate skip exports should use separate paths; preserve these original receipts. [API comparison](evidence/api-comparison.json) is metadata evidence, not executed unchanged-binary consumer qualification.

## Remaining work

Record post-completion versus early-abandonment ownership separately. No general cancellation/disposal protocol, recursive arbitrary-value disposal or optional result scope is introduced. Schema/XmlDb work stays with their delegated owners; pins, storage and private licensing are untouched. Core REQ-140 remains In Progress, C45/general schema contracts and actual dual-consumer qualification remain pending, and API freeze is not accepted.

Next core task: inventory public signature dependencies outside Bosak assemblies, particularly legacy System.Xml.Schema contracts, and decide the proposed stable/legacy boundary before selecting any new neutral seam. This advances FG-01/02/07 while owners deliver their requirements.
