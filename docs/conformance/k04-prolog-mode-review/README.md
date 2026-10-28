# K04-C30 - Construction and copy-namespaces boundary review

28 October 2026. Review complete against `fa199412`. **Admission is not qualified.** The normal guards and approved baselines remain unchanged. Candidate **1.30.1** remains unpublished; public **1.26.3**.

## Complete catalog boundary

The real harness parser and lexical normalizers inspect all **31,821 QT3 cases / 428 sets**. Independently loaded normal and copied-prototype harnesses agree with their respective routing predicates for every case. The copied prototype disables only the remaining construction/copy-namespaces prolog refusal (`(?!)`); pragma refusal, comments/literal normalization, dependencies, environments, schema policy, comparisons and all other source stay intact.

Exactly **136 predicates** change across **13 sets**: **75 construction-only**, **49 copy-namespaces-only**, and **12 containing both**. Four retain independent exact-XQ10 dependency exclusions under current harness policy; their direct-executor results do not override those exclusions. All 132 independently admitted cases are individually reconciled with the complete gate.

| Fresh bounded outcome | Passed / failed / skipped |
|---|---|
| Full normal QT3 gate | **31,208 / 0 / 613** |
| Full isolated QT3 gate | **31,214 / 126 / 481** |
| Full normal qt4 gate | **8,939 / 0 / 1,920** |
| All 136 originals, direct normal executor | **5 / 0 / 131** |
| Same originals, direct isolated executor | **6 / 130 / 0** |
| Independent public XQuery controls | **6 / 14 / 0** |

The isolated gate exits **2**, the expected failed-assertion status; this exit and all 126 failure messages are preserved. Exactly 132 former skips execute, yielding six passes and 126 failures. All 481 retained skip names/reasons are unchanged, with no added skips. The four independent dependency exclusions account for the difference between 130 direct failures and 126 gated failures. Normal QT3 and qt4 skip maps match the approved C29 adoption exactly; qt4 retains 230 gated sets and 11 experimental passes.

The five direct normal passes bypass runner dependency admission and accept XPST0003; the complete normal runner still excludes all 136 cases. Five isolated passes likewise accept the original XPST0003 assertion. The sixth, `version_declaration-005`, accepts an earlier **XQST0038** unsupported-collation error, independently reproduced through the module parser before construction parsing. These six passes do not demonstrate either missing declaration's semantics. Every direct isolated failure reports XPST0003 at unsupported prolog syntax.

## Syntax and semantic witnesses

The public API controls bypass routing and preserve their own expected results: all four copy-mode combinations and both valid construction modes fail at parsing. Duplicate declarations also yield XPST0003 instead of the required **XQST0055** (copy-namespaces) and **XQST0067** (construction). Nested-comment spellings fail in the same way. Four deliberately malformed declarations correctly produce XPST0003. Two independent declaration-free witnesses pass the existing default namespace preservation/inheritance behavior; this is bounded evidence, not proof of every namespace-copy rule. Explicit no-preserve/no-inherit witnesses remain blocked at parsing.

The XQuery prolog parser has no construction or copy-namespaces branch, and its static context carries neither declared mode. Existing XSLT namespace controls are a separate layer and do not supply the missing XQuery contract. The immutable [XQuery 3.1 review pin](evidence/REVIEW-PIN.json) and [reviewed sections 4.6/4.9](evidence/reviewed-specification-excerpts.json) distinguish construction type-annotation behavior from namespace copying. Bosak.Schema retains XSD 1.1 ownership; this review changes no schema backend or capability claim.

All **434 final diagnostic processes** finish without timeout: 408 original/resource probes, 20 public controls, two complete catalog inventories, three full gates and one early-error parser witness. Three build logs are separate. Twenty-six lexical controls pass under both real harness assemblies. The 503 fixture/resource identities, normal source identities, byte-identical prototype engine DLLs and original assertions are preserved; the entire C29 review/adoption folder is unchanged. C29's 23,934 solution passes are inherited, not a fresh solution run in this read-only review.

See [structured reconciliation](evidence/review-analysis.json) and [results](RESULTS.json). No engine, fixture, assertion, KnownSkips, GatedSets, version, release, upstream or Braid change is included. REQ-126 remains Complete under bounded owner acceptance; original dispositions and 48 historical unmet observations remain recorded.

## Recommended next slice

[C31 should implement copy-namespaces semantics first](NEXT-SLICE.md), using the **49 copy-only originals** as its bounded cohort. Preserve the construction guard and all independent dependency exclusions. Parsing, static duplicate/order errors, per-module mode propagation, constructor copying, namespace preservation/inheritance and caller-node isolation must be tested together; accepting syntax alone is insufficient. Keep both normal guards until the semantic correction is qualified, then review any exact admission separately. No baseline approval is requested from this failed rehearsal.

## Reproduction

Run `prepare-review.py` and `prepare-inventory.py` in the evidence folder with Python; build their generated Release projects under `.guard-tmp/k04-prolog-mode-harness`, `k04-prolog-mode-driver` and `k04-prolog-mode-inventory`. Then run `run-inventory.py`, `run-originals.py`, `run-controls.py` and `run-gates.py`. Scripts locate the repository root themselves. The full isolated gate must reproduce exit 2; orchestration preserves it as a finding. `analyze-review.py` reconciles all case identities and outcomes. Use `validate-evidence.py` to check the preserved evidence against the pinned source and fixture revisions. Re-running receipts requires regenerating their manifest identities.
