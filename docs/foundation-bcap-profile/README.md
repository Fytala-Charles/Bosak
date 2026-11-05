# BCAP-01 invocation and full isolated qualification

2 November 2026. The unchanged isolated outcome source now passes **83 controls twice**, including 13 added invocation/profile controls. A fresh complete solution build and all **24,390 unit tests across ten projects pass** on that candidate. QT3 stays exactly **31,397/0/424**; qt4 gated stays **8,941/0/1,918**, with 11 experimental passes. Both skip maps are byte-identical to the frozen production baseline. Production remains 1.42.0 and unchanged; qualification does not promote this candidate or accept the proposed public SDK.

## Profile evidence

The added controls exercise implicit/default/named/expanded initial modes; distinct match/global source roles within those modes; named-template focus with initial-mode selection; required global context refusing implicit fallback from match; and absent global context clearing named-template focus. They also verify explicit failure for an admitted element match, caller resource-policy replacement, missing execution limits, and include/import-schema/use-when compilation profiles. Unsupported profiles remain refusals rather than silent legacy fallback. [Two probe runs](evidence/probe-0.json), [current status](evidence/STATUS.json).

The 70 earlier capture, receipt, original-exception, cleanup-order, gate/cancellation and serializer-lifetime controls rerun on the same rebuilt engine binaries. The source contains no semantic change from [the outcome candidate](../foundation-bcap-outcomes/README.md); the engine-source hashes are retained. Tests still use synthetic provider authority and actual Core snapshot/lease paths, not native Schema/XmlDb ownership.

## Full qualification

| Evidence | Result |
|---|---|
| Complete Bosak.sln Release build | Pass, zero errors; five warnings in unchanged test/conformance sources, retained in the build log. |
| All ten unit projects | 24,390 passed, zero failed; includes 8,234 XSLT and 47 Database tests. |
| QT3 frozen 3.1 gate | 31,397 passed, zero failed, 424 skipped. |
| qt4 gated sets | 8,941 passed, zero failed, 1,918 skipped; 11 experimental passes. |
| Frozen skips and corpora | Both skip maps byte-identical; corpus revision/status unchanged. |
| Runtime binding observation | Every unit/gate runner's actually loaded Bosak assembly hashes match its frozen candidate layout. |
| Shipping API inventory | All 11 assemblies retain 270 public types, 5,770 declaration rows and their original friend grants. |

The [full qualification receipt](evidence/FULL-QUALIFICATION.json), [all unit counters](evidence/ALL-UNITS.json), compressed original TRX files, raw gate/build logs, process binding receipts and [public API comparison](evidence/API-COMPARISON.json) preserve the evidence. LanguageServer builds separately because it is outside Bosak.sln; it is included in the 11-assembly metadata comparison. The solution build's five warnings are from copied, hash-pinned test/runner source; those sources were not edited to suppress warnings.

Tests run from isolated execution copies with the original corpus/docs fixtures. XSLT uses the existing temporary-layout Application Control workaround; Database's existing local HTTP fixture runs with loopback access. Negative probes catch exceptions under the scoped child error-mode wrapper. No deliberately unhandled crash or machine-wide setting is used. No new full-suite failure or engine fix was needed in this cohort.

The actual engine public inventory is distinct from the **47 proposed review types**: the latter remain the unchanged throwing declarations in the outcome dossier. No public foundation SDK body is implemented by this qualification, and no prior owner response is rebound to its revised manifest.

## Remaining work

P03 remains InProgress for cold/workload allocation and cancellation-latency evidence; warmed source-free observations are not accepted native budgets. Actual Schema/XmlDb providers, worker handoff, arbitrary authority and concurrent/reentrant gate use remain outside this bounded cohort. Exact Braid/impacted-owner disposition, R2 closure, public implementation, C45/freeze and publication remain pending. Full qualification now exists for this exact private source profile; future source/semantic changes need their own receipts.

Read [specification/profile basis](SPEC-BASIS.md), [source/test inputs](evidence/SOURCE-INPUTS.json), [execution layouts](evidence/QUALIFICATION-INPUTS.json), [source/binary archive hashes](evidence/INPUTS.json) and [loaded probe hashes](evidence/BINARY-PINS.json). Qualified archives are retained under `.guard-tmp/bcap-profile` and sealing refuses overwrite. Production sources, owner registries, product pins and prior cohort receipts remain untouched.

## Reproduction

`tools/prepare.py` verifies the retained outcome candidate and creates a fresh source layout with pinned test sources and junctions to the original corpus/docs fixtures. The runners read those fixtures; the junctions are not an operating-system write restriction. Preparation refuses an existing destination. From the repository root run tools with the full `docs/foundation-bcap-profile/` prefix: `tools/qualify.py build`, `tools/qualify.py observer`, `tools/qualify.py units`, `tools/qualify.py database`, `tools/qualify.py finish`, `tools/qualify.py gates`, `tools/run.py`, `tools/audit.py`, `tools/qualification-evidence.py`, `tools/seal-inputs.py`, then `tools/verify.py`. Database requires the existing loopback fixture permission. Full build precedes probe binding; every compared DLL is hash-pinned, so a later rebuild cannot silently replace the qualified cohort.
