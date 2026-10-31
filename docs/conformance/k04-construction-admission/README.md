# K04-C35 — Construction routing admission qualification

28 October 2026. Qualification base `994b4e33` (C34); **owner approval pending**. Production source and the normal baseline are unchanged. Candidate **1.32.0** remains unpublished; public **1.26.3**.

The complete QT3 inventory independently loads the normal and copied harnesses and checks their actual routing predicates for all **31,821 cases / 428 sets**. Exactly **87 construction or mixed-mode predicates** change across ten sets, retaining C33's original expressions, assertions, modules, environments and dependencies byte-for-byte in the decoded records. The isolated harness removes only the construction refusal and its now-empty regex field, with a diagnostic history row. The pragma and independent dependency guards remain unchanged. Its engine DLLs match the delivered C34 normal DLLs byte-for-byte.

| Fresh qualification | Passed / failed / skipped |
|---|---|
| Complete normal QT3 | **31,257 / 0 / 564** |
| Complete isolated QT3 | **31,340 / 0 / 481** |
| Complete normal qt4 gate | **8,939 / 0 / 1,920** |
| 87 originals, direct normal executor | **3 / 0 / 84** |
| Same 87 originals, direct isolated executor | **87 / 0 / 0** |
| Previously admitted copy cases, each executor | **49 / 0 / 0** |
| Previously admitted context cases, each executor | **43 / 0 / 0** |

Exactly **83** eligible skips leave the isolated gate. All **481** retained names/reasons are identical, including the four independent exact-XQ10 exclusions: `K-VersionProlog-5`, `XQTY0086_1`, `XQTY0086_2`, `XQTY0086_3`. The direct executor deliberately bypasses runner dependency admission, explaining its 87 passes versus 83 gate admissions. Both normal skip maps exactly match the owner-approved C32 baseline. qt4 retains 230 gated sets and 11 experimental passes.

All 19 lexical controls pass against each real harness, including nested comments, literal masking, mixed declarations and retained pragma refusal. The complete inventory also confirms the 49 copy and 43 context admissions remain eligible; their original assertions pass under both executors. Original/resource results exactly replay C34. All **450 diagnostic processes** complete within external watchdogs with exit zero and no timeout. Full solution evidence is explicitly inherited from C34: **24,082 passes**, including 73 construction regressions. This review changes no implementation and does not rerun unit tests.

The [exact two-file proposal](BASELINE-UPDATE-PROPOSAL.md) is ready for owner disposition. No adoption, dependency filter, KnownSkips, GatedSets, fixture, assertion, schema backend, package version, release or upstream submission change is applied. Bosak.Schema retains XSD 1.1 ownership; ADR-006/REQ-139 remains separately pending. REQ-126 remains Complete under bounded acceptance and its 48 historical unmet observations remain preserved.

Raw evidence is sealed by `evidence/MANIFEST.json`; run `python docs/conformance/k04-construction-admission/evidence/validate-evidence.py` to verify it. HTML checks cover ten updated pages using unchanged Fytala CSS; no screenshot/PDF acceptance is claimed.
