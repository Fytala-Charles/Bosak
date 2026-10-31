# V5 typed-value boundary experiment ? 31 October 2026

The isolated V5 draft supplies the producer boundary missing from V4. It compiles against the exact retained Core 1.34.2 DLL and passes **75 controls: all 54 prior protocol controls plus 21 new producer/consumer controls**. This is a shape and test-double composition result, not production Schema/VM integration or API acceptance.

## Candidate changes

[CORE-CANDIDATE-V5.cs.txt](CORE-CANDIDATE-V5.cs.txt) derives from the [V4 candidate](../foundation-granularity-experiment/CORE-CANDIDATE-V4.cs.txt):

- ISchemaTypedValue now exposes ordered SchemaAtomicItem cells containing plain XdmValue and separate provenance. An optional default-interface scalar method avoids enumeration for ordinary scalars. IsList preserves empty/singleton list semantics.
- ISchemaAssessmentAdvisor.OwnsTypedValue checks issuance of the immutable container before its getters are accessed. Core can eventually attach its own carrier privately; no public arbitrary attachment factory is introduced.
- IQueryStaticContext keeps only the immutable namespace snapshot. Ten compiler/host getters move out of the proposed public surface; actual effective-context capture remains private implementation work.
- IQuerySemanticEnvironment adds descriptive format-version and context-fingerprint strings. They confer no authority and do not replace exact environment admission.

V5 has 34 public draft types versus V4's 33. The extra readonly cell type supplies a missing operation; public type count alone does not measure overhead. The struct constructor rejects null evidence; the consumer must additionally reject default cells and non-atomic payloads. These are draft contracts, not new product APIs.

## What ran

The [reproduction runner](tools/run.py) copies the draft and [probe](tools/Probe.cs.txt) into ignored private scratch, extracts only the exact Core DLL from the retained candidate feed, clears NuGet sources and builds net10.0 with XML documentation and warnings as errors. Build and child execution are bounded and errors are caught. [INPUTS.json](evidence/INPUTS.json) records exact input hashes and project text; [build output](evidence/build.txt), [raw results and reflected surface](evidence/run.json), and [stderr](evidence/run.stderr.txt) are retained.

The prior 54 protocol checks run unchanged in intent, using fixtures adapted for the two descriptor getters and typed-value ownership. The fixture static context also drops its ten obsolete getters. The probe adds:

| Controls | Observed result |
| --- | --- |
| B01?04 | Plain string plus separate current type evidence transported, scalar path avoids enumeration, computed evidence has no original assessment. |
| B05?07 | Empty/singleton lists retain their boundary and two items retain ordered distinct actual-member evidence. |
| B08?11 | Unissued container rejected before getters; copying legitimate cells does not grant issuance and copied container is not enumerated. |
| B12?15 | Default cell, non-atomic node, null evidence and foreign type handles rejected. |
| B16?18 | Ordered test descriptor is independent of dictionary insertion order; private host revision changes it; namespace snapshot survives caller mutation. |
| B19?21 | Equal descriptive environment fields do not grant admission; closing the endpoint prevents subsequent reads. |

The test consumer checks the whole container once, then validates cell payloads, component membership and any original assessment identity. The scalar fixture and immutable issued-array fixtures are trusted test producers. The issuance set is reference-based. This is not a production issuer, concurrency model or lazy snapshot lease.

## Limits and next implementation

The descriptor uses a diagnostic-only versioned JSON encoding of sorted namespace pairs and an immutable host revision. It deliberately omits the full effective compiler/host state; it must not be used as a production semantic cache key. The environment replay fixture uses a fixed diagnostic fingerprint, separately exercising instance admission. Neither proves actual compiler capture, delegate snapshotting or worker transport.

The consumer returns cells for inspection; it does not attach or propagate a VM carrier. No real native Schema adapter, QName constructor, union validator, typed XmlDb route or Braid worker route ran. No allocation, timing, retained-memory, ABI or full-provider performance acceptance is claimed. Original test-double component identities are preserved; they do not qualify actual anonymous/local schema components.

Next implement the smallest real adapter for the native decimal assessment profile identified in the [source audit](../foundation-callsite-disposition/README.md), preserving original type/declaration and copied-snapshot identity. Core-side carrier selection and real API/VM transport qualification must accompany that adapter before stable API acceptance. External owner work remains delegated; this experiment edits no external checkout.

[Specification basis](SPEC-BASIS.md) records immutable reference revisions and the diagnostic-only scope. Product Core remains 1.34.2/source c88eea43. C45, full Schema/XmlDb/Braid qualification, performance budgets and four-owner freeze remain pending. No new production semantics, packages or conformance gate results are claimed.

Reproduce with `python docs/foundation-boundary-experiment/tools/run.py`; verify retained evidence with `python docs/foundation-boundary-experiment/tools/verify.py`.
