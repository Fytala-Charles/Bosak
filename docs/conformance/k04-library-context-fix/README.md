# K04-C28 - Library context type scope and identity

28 October 2026. Implemented against `93c564da`; candidate **1.30.1** is unpublished. Public version remains **1.26.3**.

Library context constraints now capture the declaring module's namespace bindings and default element/type namespace. Atomic aliases receive canonical expanded identities while node-name positions, processing-instruction literals, URI case and local-name case retain their meaning. Evaluation restores the caller's namespace context afterward. Every imported module constraint is retained; equal lexical type names no longer collapse constraints from different namespaces.

The dynamic module loader validates its target constraint after preparing the target's schema context. Target mismatches retain **FOQM0005**, static target errors retain **FOQM0003**, and mismatches from other imported module constraints retain **XPTY0004**. Direct imports continue to use the host's prepared schema environment; this slice does not introduce automatic schema loading for every direct import. Bosak.Schema retains XSD 1.1 ownership.

## Verification

| Check | Outcome |
|---|---|
| Same 112 regression tests against preserved 1.30.0 engine DLLs | 60 passed / 52 failed |
| Current focused regression tests | **112 passed** |
| Complete solution | **23,934 passed**, ten projects |
| C27 original resource/module cases, isolated routing | **13 passed**; only contextDecl-056 and contextDecl-057 change from failure |
| Independent C27 controls | **12 passed**; four prior failures repaired |
| Complete isolated QT3 context-declaration set | **44 / 0 / 0** |
| Normal context sets | QT3 **4 / 0 / 40**; qt4 **0 / 0 / 53** |
| Complete normal QT3 gate | **31,165 / 0 / 656** |
| Complete normal qt4 gate | **8,939 / 0 / 1,920**, 230 gated sets, 11 experimental passes |

All 80 C26 execution results, 97 filter assessments and five library-error controls retain their previous results. Retained gate skip names and reasons are unchanged. The complete copied prototype harness changes only its diagnostic context-routing refusal and history row; production routing, dependency guards, fixtures, assertions, skip lists and gated sets remain untouched.

The initial 110-case fixture omitted prepared schemas for three direct-import positive cases. Its source and failed logs are retained. The corrected fixture follows the existing normal QT3 schema preparation policy; dynamic-loader tests remain resolver-only and exercise loader-owned schema preparation. The final identical 112-case source was rerun against preserved old DLLs and the current engine. See [fixture disposition](evidence/schema-fixture-disposition.json), [review deltas](evidence/review-deltas.json), and [results](RESULTS.json). Historical failed experiments are preserved separately from final qualified receipts.

The pre-implementation [semantic pin](evidence/IMPLEMENTATION-PIN.json), prior C27 identities, source/binary identities, original resources and raw execution receipts support this bounded correction. REQ-126 remains Complete under its accepted bounded catalog; its 48 historical unmet observations remain recorded.

## Next slice

C29 should review routing admission separately: inventory every affected QT3 case across the catalog, preserve unrelated prolog and pragma guards, verify lexical boundaries, then run a complete isolated gate rehearsal and reconcile the exact skip delta. Only that evidence can support a concrete owner baseline proposal. The green context set alone does not authorize broader routing admission; qt4 XQuery-only cases retain their applicability exclusions. No release is included.
