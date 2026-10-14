# Key indexing declaration contexts

**Submitted 2026-10-14:** [Upstream PR #30](https://github.com/qt4cg/xslt40-test/pull/30) adds two cases. Review/acceptance and independent processor confirmation are pending. Published Bosak remains 1.2.0; these checks pass in unpublished source candidate 1.2.1 at integrated revision `836ec5a8` (index-context PR #272 merged after CI).

## Coverage and specification basis

The upstream fixed-namespaces family covers component identities and ordinary template patterns. The key family includes predicates and namespace-sensitive key arguments. Neither combines fixed key indexing namespaces, native rebinding, locally earlier declarations and an included module whose bindings differ from the caller. This batch addresses those specific combinations without changing any existing test or the pending assertion correction PR #29.

The [fixed-namespaces rules](https://qt4cg.org/specifications/xslt-40/Overview.html#fixed-namespaces) apply to XPath expressions and patterns throughout the module. The [key declaration rules](https://qt4cg.org/specifications/xslt-40/Overview.html#element-key) make match/use belong to the key declaration's context. The local 3.0 cases change expression grammar while retaining the 4.0 module's fixed bindings.

| Case | Boundary | Expected result |
|---|---|---|
| fixed-namespaces-016 | Current and local 3.0 key match names, wildcard/predicate and fixed-only alias despite native rebinding; a native node deliberately shares the lookup value | `<out current="1" earlier="1" current-ns="urn:fixed" earlier-ns="urn:fixed"/>` |
| fixed-namespaces-017 | Included key module's @use bindings differ from the caller; current and local 3.0 keys, including a prefix absent from caller context | `<out current="1" earlier="1"/>` |

Both result assertions combine assert-xml with explicit absence checks for native `p` and fixed-only `q` result namespace bindings. Case 016 checks selected-node namespace URIs as ordinary result attributes as well as counts, avoiding a wrong-node false positive.

## Verification

- Exact submitted cases: **2 pass / 0 fail**. Each filtered receipt has one pass and seventeen unselected skips.
- Expanded family: **18 pass / 0 fail / 0 skip** at submitted native commit `586d75d7`.
- Pre-index-fix execution: both cases fail using retained Bosak binaries from the verified local-key test copy built before the index-context fix. Case 016 selects a native node and misses the earlier key; case 017 reports XPST0081 for the declaration-only prefix. The current conformance harness was reused with only Bosak.Xslt.dll replaced; assembly hashes are recorded. This is a retained-binary regression check, not a fresh source checkout baseline.
- Namespace negative control: add a non-excluded `p` binding to each new literal result. Both new cases fail while the other sixteen pass; originals are restored byte-for-byte afterwards.
- Catalog XML validates. Integrated engine CI passes **8,170 unit tests**, including **5,284 XSLT tests**. Engine source and frozen gate membership are unchanged by this test submission; prior index-context gate receipts remain authoritative.

This verifies these cases in Bosak; it does not establish complete XSLT 4.0 conformance. The original seventeen accepted contributions remain accepted. These two cases are additional submissions, and PR #29 separately corrects two existing assertions. Original contribution/acceptance/execution receipts remain unchanged.

## Current upstream base and reproduction

Upstream default branch `master` was fetched before branching and again immediately before submission. It remained `91328fbf0fed4a5d2813eb3661808ee53cb961ba`. The final four-file diff contains only the catalog additions and three new stylesheets; it is prepared against that current base, avoiding stale contribution-branch application conflicts.

Apply the portable patch in this directory to that upstream base, or use branch `bosak/key-index-contexts` at `586d75d7`. Run:

```powershell
powershell -ExecutionPolicy Bypass -File ./validate-xslt40-catalog.ps1 -Files tests/attr/fixed-namespaces/_fixed-namespaces-test-set.xml
powershell -ExecutionPolicy Bypass -File ./run-xslt40-tests.ps1 -TestSet fixed-namespaces -TestCase fixed-namespaces-016 -NoBuild
powershell -ExecutionPolicy Bypass -File ./run-xslt40-tests.ps1 -TestSet fixed-namespaces -TestCase fixed-namespaces-017 -NoBuild
```

The original family cases 013/015 remain subject to pending correction PR #29; their assert-xml-only expectations do not independently check namespace nodes. This batch leaves those assertions untouched. New result predicates independently check the relevant namespace requirement.
