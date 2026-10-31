# Frozen-caller carrier equality qualification ? 31 October 2026

A 48-byte selective carrier can preserve the tested legacy CLR equality/hash behavior and atomic VM transport, including calls compiled before the overrides. **It is not ready for production:** this normalization implementation allocates **64 extra bytes per annotated CLR Equals** in the measured integer loop, reference-backed attachment still adds a 32-byte holder, and two inherited-method overrides change declared reflection metadata.

## Exact comparison

The [inputs](evidence/INPUTS.json) pin Core source 8080b18b and the exact seven source-qualified 1.35.0 product DLLs. Source and product archives retain licenses/NOTICE. Five cohorts run twice in reversed order:

| Cohort | Core representation | Purpose |
| --- | --- | --- |
| product | Existing qualified product DLL | Actual 1.35.0 control. |
| source | Unchanged source rebuilt with recorded minimal project metadata | Separates source rebuilding from representation changes. |
| holder | Prior payload-aware holder with default CLR equality | Known-bad equality control; attachment is bounded to atomic kinds. |
| equal-holder | Holder plus Equals/GetHashCode normalization | Correctness candidate. |
| equal-selective | Inline scalar markers; holder for reference-backed payloads; equality normalization | Lower attachment-allocation candidate. |

All rebuilt variants replace **Core only**; the other six DLLs remain exact product bytes. These are isolated hybrid cohorts. Core/Runtime/API production sources and version 1.35.0 remain unchanged.

One [probe caller](tools/Probe.cs.txt) is compiled once against the existing product, before adding overrides, then its DLL/deps/runtimeconfig bytes are retained unchanged in every cohort. It uses public diagnostic helper delegates only for marker setup/readback; it does not reflect private fields or engine methods. All entry-point exceptions are caught, and the runner uses scoped Windows error-mode handling plus build/process watchdogs. Separately, the original 1.34.1 retained caller is run through the checked runner in every cohort: all five pass 20/0 with unchanged case observations and caller bytes.

## Equality, payload and VM controls

Each successful candidate/control process passes **828 controls**: thirteen controls per fixture across 23 values, plus a complete 529-pair matrix against the plain-value CLR oracle. The known-bad holder passes 253 expected negative/positive controls, recording its mismatches without deliberately throwing an unhandled exception. Eight non-bad processes and two known-bad processes are retained.

Fixtures cover Boolean/integer/decimal/double/float/string, duration, QName and ordinary date/time values, including NaN, infinities, signed zero, decimal scale, equal strings with separate objects, and legacy/user type-name distinctions. Binary/URI examples use the current public string-annotation representation; this is not positive coverage of reserved Binary/Uri-kind factories, every subtype or extended-year operation.

Direct payload access, ToString, retagging, equality in both directions, object.Equals, EqualityComparer, HashSet and Dictionary are checked. Different origin markers do not affect ordinary CLR value equality, but remain separately observable. This never grants schema component or assessment authority. Plain-pair results match product in every cohort. The two candidates preserve tagged/untagged symmetry, legacy type distinctions and hash consistency for these fixtures; the original holder fails the expected equality/collection controls.

All 27 existing actual API/VM expression/error cases replay in ten fresh processes with matching plain values/errors. Identity/container/function/typed-atomization paths retain markers in instrumented cohorts; constructors and calculations retain the previously recorded bounded behavior. This uses synthetic typed nodes/opaque markers, not actual trusted production evidence, a Schema provider or a complete provenance law.

## Measured allocation boundary

[RESULTS.json](RESULTS.json) summarizes raw seven-sample distributions after three warmups, at 1,000 and 100,000 items/calls, repeated in two reversed process orders. Tiered compilation and ReadyToRun are disabled for this diagnostic. Numbers below are the 100,000-item allocation medians, excluding preparation of the shared input value/marker.

| Operation | Product / source | Equality holder | Equality selective |
| --- | --- | --- | --- |
| Attach integer, additional bytes/item beyond the output array | 0 | 32 | **0** |
| Attach string/decimal, additional bytes/item beyond the output array | 0 | 32 | 32 |
| CLR Equals, plain integer, bytes/call | 344 | 344 | 344 |
| CLR Equals, annotated integer, bytes/call | 344 (no marker) | **408** | **408** |
| CLR hash, integer, bytes/call | 64 | 64 | 64 |
| XdmValue x64 size | 48 | 48 | 48 |

Array storage itself is 48 bytes/item plus a 24-byte array header in this runtime. Some eight-megabyte holder samples contain another 8?24 bytes per run beyond that formula; raw variations are retained, their cause is not attributed, and the reported medians remain unchanged. The additional 64 bytes per annotated Equals are consistent with an extra boxed XdmValue in the normalization helper; this is an inference, not an IL-level allocation attribution. Default ValueType equality already allocates in this measured call shape. The cloned reference-backed accessors also add an unwrapping branch, so ordinary workload throughput remains unqualified. Timings are retained but no statistical significance, production throughput ratio, p95, retained-memory result or consumer budget acceptance is asserted. Hash values/checksums are not compared numerically across processes because runtime/string hashing is process-dependent.

## Decision and next change

Keep selective 48-byte storage as the preferred direction, but **do not promote this implementation**. Replace the annotated equality normalization with a lower-allocation implementation, preserving the unannotated path and checking every result against the same frozen-caller oracle. Any fieldwise implementation needs explicit coverage of all existing storage/type fields; CLR equality must not be replaced with XPath numeric equality.

Two public overrides of already inherited slots, Equals(object) and GetHashCode(), are needed by these candidates. Old compiled calls work in this probe, but DeclaringType/reflection metadata changes from ValueType to XdmValue. Record that in the C45 member manifest before production acceptance. The public diagnostic attachment/read helpers are not proposed product APIs. No IEquatable interface or instance field is added here.

Actual production-kernel/provider transport, arbitrary scalar subtypes, live source lifetimes, trusted type evidence, complete consumers and PF budgets remain pending. These results do not accept a public API, carrier, release or four-owner freeze. No production language semantics, owner checkout, original store or sealed earlier dossier is changed.

## Runner and corrections

Use the live `tools/run-retained-foundation-caller.py` for retained legacy runs. Its optional `--candidate-evidence` names an explicit seven-assembly qualified hash manifest; the original caller/configuration bytes and fresh workspace output remain mandatory. Candidate names are restricted to the exact seven expected assemblies. Five guard tests pass, including refusal of a malformed manifest without launching .NET. The earlier sealed runner packet stays unchanged.

The first comparison is preserved in initial-comparison.zip. The expanded payload-access probe initially called DateTimeValue on Date/Time kinds; the caught XPTY0004 receipt and inputs are preserved in initial-accessor-correction.zip. The probe now uses DateValue and TimeValue. No engine source was altered to change those payload expectations and no negative .NET crash was induced.

Run `python -B docs/foundation-carrier-equality/tools/test_runner.py` for runner guards. Reproduce the comparison in a fresh dossier/copy with `python -B docs/foundation-carrier-equality/tools/run.py`; existing legacy report files are deliberately not overwritten. Verify sealed retained data with `python -B docs/foundation-carrier-equality/tools/verify.py`. [SPEC-BASIS.md](SPEC-BASIS.md) records the pre-execution specification boundary.
