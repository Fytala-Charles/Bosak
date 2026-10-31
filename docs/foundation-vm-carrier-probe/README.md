# Actual VM carrier probe and selective allocation ? 31 October 2026

**A selective reference-slot prototype removes the extra holder allocation for the sampled inline integer, while retaining the 48-byte outer carrier.** It does not solve reference-backed allocation, CLR default equality or the provenance laws for casts/computed values. No carrier or production API is selected.

This extends the [first carrier experiments](../foundation-carrier-experiments/README.md) with actual package API/compiler/VM/standard-library execution. [RESULTS.json](RESULTS.json) records six processes and 27 distinct expression cases, repeated across two process orders. All case payloads/kinds and error codes match the package baseline. These repeated runs are not 162 distinct tests, full E-02 completion or a conformance/performance certification.

## Cohorts and isolation

- **baseline:** all seven exact 1.34.2 package-derived DLLs from source `c88eea43`.
- **holder:** the earlier accessor-aware Core holder prototype, rebuilt from the same Core source; the other six package DLLs remain byte-identical.
- **selective:** the same Core patch, except boolean/integer/double/float tags can occupy the otherwise-unused reference field directly. Reference-backed atomics still use a payload/marker holder. Allocation measurements here cover integer and string only; do not generalize the integer result to all atomic types.

The two modified Core builds retain assembly version 1.34.2.0 so the actual donor layers can bind; they are explicitly **unqualified hybrid cohorts**, not original package bytes or ABI-approved replacements. The builds retain original internal-access declarations and add only experimental helpers/accessor unwrapping in the isolated Core source. No Runtime/Standard/compiler/API/provider DLL or production file is patched. Actual loaded identities, paths, Default/noncollectible contexts and hashes of all seven assemblies are recorded per process.

Source/project templates, patches, source archive reference and feed identities are in [INPUTS.json](evidence/INPUTS.json). `tools/run.py` recreates ignored `.guard-tmp/vm-carrier-probe` layouts and replaces this dossier's unsealed run outputs; run it in a copy to preserve this sealed receipt. Direct references and cleared package sources avoid upgrading product pins. All five final build invocations pass with zero warnings/errors. All six final stderr records are empty. The runner catches failures and exits normally with a failure status, with a 120-second subprocess watchdog; it does not deliberately trigger an unhandled .NET exception popup.

## VM observations

The provenance markers A/B are synthetic shared objects. The typed-node adapter supplies values through existing public IXdmNode APIs; it is not a real Schema assessment, component authority, union validator or XmlDb snapshot. No new provenance semantics or normative source revision is adopted.

| Boundary | Observed holder/selective behavior |
|---|---|
| Variable, let, filter, simple map | Original marker A survives. |
| Array, map value, nested map/array | Original marker A survives actual expression evaluation and lookup. |
| Inline XPath function | Identity function return retains A through actual VM dispatch. |
| `data($v)` | Existing atomic marker survives. |
| `data($n)` with legacy type annotation | Synthetic typed node returns Integer 42 with A. |
| Synthetic two-atomic typed node | Actual atomization returns ordered Integer 42/A and Integer 7/B. This is not an XSD list/union assessment or proof that governing list boundaries survive. |
| Untyped-node witness | With no SchemaTypeAnnotation, existing atomization returns String 42 without a marker, despite the synthetic TypedValue getter. This is the legacy route selection, not a new defect. |
| `distinct-values`, `sort` | Sampled selected original values retain A/B; sort reorders to 7/B then 42/A. |
| `xs:integer($v)`, `xs:decimal($v)` | Values remain 42, but the marker is absent. Even the sampled integer-to-integer constructor does not preserve it. |
| Arithmetic, comparisons, string operations | Computed outputs carry no marker in these runs. |
| Map key lookup | Annotated numeric key can be looked up using plain numeric 42. Sampled XPath equality and hash controls also match. |
| JSON serialization | Same JSON text, with no live-origin marker on the constructed string. No authority-transfer or general PSVI serialization claim follows. |
| No typed value, invalid lexical cast, wrong cardinality | FOTY0012, FORG0001 and FORG0005 respectively in all cohorts. |
| CLR default equality | Annotated integer differs from plain integer in both modified Core variants; the dedicated XPath comparer/hash checks pass. This remains a public-consumer compatibility decision, not a waived concern. |

Marker disappearance at a cast/construction boundary is **not automatically a bug**. Original node PSVI, a value's current declared/actual type and computation lineage are distinct. Before any production change, specify which operations preserve original information, create new typed information, deliberately strip it or refuse. Pin immutable normative sections/deviations for new semantic behavior and test it. Mechanically copying an old schema authority onto every computed result would be unsound.

The existing Data/VM atomization source checks SchemaTypeAnnotation before using a node's TypedValue. The synthetic type name here is a routing witness only: it does not authenticate a component or make the proposal's authority contract optional. Exact source hashes are retained. The new optional schema channel still needs explicit routing/authority design.

## Allocation result and limits

Inputs and provenance markers are prebuilt, shared and outside the timed region. Each measured action creates a fresh XdmValue array and attaches the same available marker per item, with three warmups and seven samples at sizes 1,000 and 100,000 per process.

| 1,000 entries | Baseline | Holder | Selective |
|---|---:|---:|---:|
| Integer | 48,024 bytes | 80,024 bytes | 48,024 bytes |
| String | 48,024 bytes | 80,024 bytes | 80,024 bytes |

The selective integer path avoids the prior approximately 32-byte per-item holder in this scenario. It does not eliminate the cost of creating real per-value provenance when that metadata is not shareable or already available. The string path still allocates a holder per item; source retention, metadata graph size and resource leases are not measured by this array allocation counter.

Raw elapsed samples and GC counters are retained; TieredCompilation and ReadyToRun are disabled as in the prior probe. This is a controlled construction micro-experiment, not a VM-throughput, default-path, end-to-end preview, tail-latency, peak-memory or owner-budget acceptance. A faster integer representation cannot waive the remaining reference-backed/default-path/equality/identity/lifetime requirements.

## Corrections and next decision

An initial build exposed the synthetic adapter's omitted required DocumentUri member; the build failure is retained and the adapter now delegates it. The first node fixture omitted SchemaTypeAnnotation, so the existing engine correctly treated it as untyped. Its source/raw results remain in `initial-node-fixture-without-type-annotation.zip`; final runs add an explicit annotated route and retain a separate untyped witness. No product behavior or test expectation was changed to hide a failure.

Keep selective inline storage as a candidate optimization, not an accepted API or a promise of zero allocation. The next decision requires explicit provenance transition laws for the observed cast/construction boundaries, real provider typed/list/union/source facts, CLR equality compatibility and reference-backed cost design. E-03 operation granularity and E-04 session/worker lifetime remain open; neither package baseline success nor this hybrid trace completes them.

Run `python docs/foundation-vm-carrier-probe/tools/verify.py` for data-only replay verification. Production engine tests/gates were not rerun because this step changes only isolated diagnostic clones and documentation. Full changed-engine/actual-consumer qualification remains required before implementation acceptance, API freeze or publication.
