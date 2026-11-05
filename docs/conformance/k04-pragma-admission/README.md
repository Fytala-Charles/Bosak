# K04-C38 — Exact pragma routing admission qualification

28 October 2026. Qualification base **254d07d7** (C37). **Owner approval pending; adoption is not applied.** Normal source and baseline remain unchanged. Candidate **1.33.0** remains unpublished/public **1.26.3**.

The normal and copied isolated harnesses independently inventory all **31,821 cases / 428 sets**, including actual expression reading, lexical normalization and dependency-filter observations. Their inventories exactly reproduce C37: **57 pragma routing predicates** change across four sets. The direct cohort contains 57 prod-ExtensionExpr cases plus three external originals; every expression, assertion, module, environment and dependency record remains unchanged.

The isolated harness removes the last pragma-only refusal and its regex field. Its routing eligibility helper then permits candidates to reach the existing dependency/routing/parser checks; its obsolete summary is updated accordingly. This empties the bounded heuristic refusal set. It does not certify unrestricted grammar completeness or change parser semantics. Other lexical helpers and independent dependency guards remain intact. Engine DLLs match the delivered C37 normal binaries byte-for-byte.

| Fresh qualification | Passed / failed / skipped |
|---|---|
| Complete normal QT3 | **31,340 / 0 / 481** |
| Complete isolated QT3 | **31,395 / 0 / 426** |
| Complete normal qt4 gate | **8,939 / 0 / 1,920** |
| 60 originals, direct normal executor | **11 / 9 / 40** |
| Same originals, direct isolated executor | **59 / 1 / 0** |
| Public API controls | **24 / 0 / 0** |
| Previous copy originals, each executor | **49 / 0 / 0** |
| Previous context originals, each executor | **43 / 0 / 0** |
| Previous construction originals, each executor | **87 / 0 / 0** |

Exactly **55 eligible skips** leave the isolated gate, with no added skips and all **426** retained names/reasons unchanged. The independent exact-XQ10 exclusions **K-ExtensionExpression-4** and **XQST0079** remain. The former's direct assertion expects the older no-namespace error; C37's 3.1 behavior ignores that pragma, so its direct failure remains explicitly preserved. Direct diagnostics bypass runner dependency admission; this also explains normal direct failures despite a green complete normal gate. All 55 eligible original assertions pass.

Both normal skip maps match the approved C35 adoption and C37 evidence exactly. qt4 retains 230 gated sets and 11 experimental passes. All 16 lexical controls under each real harness reproduce C37. The public control results and original/resource results match C37 exactly; the complete inventory and direct replays also preserve all 49 copy, 43 context and 87 construction admissions. All **567 diagnostic processes** complete within external watchdogs with exit zero and no timeout; assertion outcomes are verified independently of process exits.

Full solution evidence is explicitly inherited from C37: **24,164 passing tests**, including **82 pragma regressions**. No implementation or normal binary changed in this qualification. The [exact two-file proposal](BASELINE-UPDATE-PROPOSAL.md) is ready for owner disposition; its routing predicate matches the qualified isolated harness, differing only in history wording.

No engine, dependency filter, KnownSkips, GatedSets, fixture, assertion, schema backend, qt4 runner, version, release, upstream or Braid change is applied. All six existing schema harness errors remain in the retained skip map. Bosak.Schema retains XSD 1.1 ownership; ADR-006/REQ-139 remains separately pending. REQ-126 remains Complete under bounded acceptance with all 48 historical unmet observations preserved.

Run `python docs/conformance/k04-pragma-admission/evidence/validate-evidence.py` to check immutable inputs, the full inventory, exact skip delta, compatibility cohorts and proposal identities. The manifest seals raw evidence and normalized documentation identities. Ten HTML renders use unchanged Fytala CSS; no screenshot/PDF acceptance is claimed.
