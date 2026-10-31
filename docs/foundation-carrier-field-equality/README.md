# Fieldwise carrier equality ? 31 October 2026

The optimized isolated carrier removes the measured annotated-equality penalty while retaining the exact previous frozen caller and its results. **Annotated integer Equals now allocates 64 bytes/call**, down from 408 in the previous normalization candidate; the ordinary unannotated control remains 344. Each of four cohorts passes 828 controls in two fresh processes, and all four checked legacy replays pass 20/0. Production Core remains unchanged at 1.35.0.

## Change and oracle

Only the diagnostic annotated equality helper changes. It compares all six existing fields: kind, integer, double using double.Equals, unwrapped reference using object.Equals, and both legacy type-name strings using ordinal equality. Neither operand carrying metadata still takes the original ValueType.Equals path. Hash normalization is unchanged. A source-field inventory guard stops generation if the six fields/types/order change; it does not silently omit a future field.

This implements CLR field equality, not XPath numeric equality, schema component identity or assessment authority. The parent source/product archives remain sealed and are reused by hash. [frozen-caller.zip](evidence/frozen-caller.zip) retains the exact parent Probe DLL/deps/runtimeconfig plus licenses; no caller is recompiled. Its bytes match the [parent qualification](../foundation-carrier-equality/README.md).

[tools/run.py](tools/run.py) builds four isolated layouts: existing product, unchanged-source rebuild, previous selective normalization, and new fieldwise selective comparison. Only Core is replaced in rebuilt layouts; the other six DLLs remain exact qualified product bytes. The existing 27 actual API/VM cases replay in every process with matching plain values/errors. Public diagnostic delegates are used for marker setup/readback, without private-field reflection or engine bypass. Markers remain opaque diagnostic objects, not actual trusted schema evidence.

The unchanged caller supplies 23 payload/type fixtures and the full 529-pair oracle, plus direct payload access, symmetric equality, boxed/generic equality, hash/set/dictionary and marker controls. Both candidates preserve all 828 controls per process in reversed cohort orders. Shapes outside the parent fixture coverage are not inferred to be supported. The inherited Equals/GetHashCode declaration change remains the same known C45 reflection-review item; this optimization adds no further public method or interface.

## Allocation result

The measured call is value.Equals(plainInteger), at 1,000/100,000 iterations, three warmups and seven raw samples per group, repeated in two reversed process orders with TieredCompilation/ReadyToRun disabled. [RESULTS.json](RESULTS.json) summarizes the 100,000-iteration medians; raw samples and actual loaded assembly hashes are retained under [evidence](evidence/INPUTS.json).

| Operation | Product/source | Prior selective | Fieldwise selective |
| --- | --- | --- | --- |
| Unannotated CLR Equals, bytes/call | 344 | 344 | 344 |
| Annotated CLR Equals, bytes/call | 344 (no marker) | 408 | **64** |
| Annotated CLR hash, bytes/call | 64 | 64 | 64 |
| Integer attachment beyond array storage, bytes/item | 0 | 0 | 0 |
| String/decimal attachment beyond array storage, bytes/item | 0 | 32 | 32 |
| XdmValue x64 size, bytes | 48 | 48 | 48 |

The improvement is 344 bytes/call versus the prior annotated candidate; comparison is not allocation-free. The remaining 64 bytes are consistent with boxing the old object-parameter call, but no IL-level allocation attribution is claimed. Equality allocation measurements here are integer-specific, not a promise for every atomic family. Timings are retained without statistical-significance, throughput, p95 or macro-workload claims. Small attachment-counter variations are preserved rather than normalized away.

## Disposition and next step

Keep this implementation as the preferred isolated selective carrier. It is not promoted into production: reference-backed holders/unwrapping costs, CLR declaration metadata, actual trusted kernel/provider transport, arbitrary scalar cases, lifetimes and consumer/PF budgets remain open. Next connect this candidate to the production evidence kernel and the real native assessment path in a bounded integration cohort, then qualify actual consumer behavior before selecting a public/stable contract or publishing packages. C45 and Core/Schema/XmlDb/Braid freeze sign-offs remain pending.

No production source/version, public schema API, external owner checkout, original store, test corpus or sealed older dossier changes. This step runs isolated cloned Core/API/VM checks; it claims no new full-product unit/conformance gate run. The earlier production qualification remains scoped to its original bytes.

[Pre-execution basis](SPEC-BASIS.md) records immutable references and the private CLR optimization. Reproduce in a fresh copy with `python -B docs/foundation-carrier-field-equality/tools/run.py`; existing legacy report files are protected from overwrite. Verify the retained packet with `python -B docs/foundation-carrier-field-equality/tools/verify.py`. Child execution uses the checked runner/scoped error mode, caught .NET entry points and watchdogs; no intentional unhandled crash is used.
