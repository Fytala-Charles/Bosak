# K04-C22 - Normalization form names, defaults and keywords

Completed 2026-10-28 against source base `aa56978e`. Candidate **1.29.2** remains unpublished; public version is **1.26.3**. The immutable [pre-implementation adoption record](evidence/IMPLEMENTATION-PIN.json) pins F&O 3.1 REC-20170321 and the F&O 4.0 function catalog at `e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e`.

The effective normalization form now uses the existing XML-whitespace normalizer before invariant uppercase, matching `upper-case(normalize-space(form))`. XML whitespace is U+0020, U+0009, U+000D and U+000A; NBSP and other Unicode separators do not disappear. Form names remain case-insensitive. An empty string (including XML-whitespace-only strings) still means no normalization.

XPath 4.0 and Experimental now default an explicit empty-sequence form argument to NFC, including named function references, function lookup and partial application. XPath 3.1 retains XPTY0004 for this argument. The 4.0 keyword metadata declares required `value` and optional `form` with an NFC default; reordered and omitted keyword calls work, while 3.1 still refuses keyword syntax.

The existing FULLY-NORMALIZED Unicode 17 backend, its data/license, other platform normalization algorithms, and serialization are unchanged. There are no fixture, dependency-filter, KnownSkips, GatedSets, AGENTS baseline or schema changes.

## Verification

- **246 new API regression rows pass** across 3.1, 4.0 and Experimental, each with and without shared XSLT mode. Before implementation: 80 failures/166 passes. Checks assert exact scalar results and singleton string cardinality, XML versus non-XML whitespace, form variants, omission versus empty sequence versus empty string, static/dynamic/lookup/partial calls, keyword order/defaults, atomization and retained type/cardinality errors.
- The complete solution build and one complete solution test run pass: **23,746 tests**, including **13,797 Standard** and **8,196 XSLT** tests. An initial compile typo used an incorrect existing helper name; its failed output is retained separately, followed by the corrected focused run and complete build. No test expectation was weakened.
- All **62 C20 review controls pass**. Exactly the five C21 remaining controls change from failure to pass; every other result/message is preserved. The eight direct originals remain 4 passes/4 failures. Those four failures are conditional negative assertions for unsupported optional forms; supporting FULLY-NORMALIZED makes them inapplicable, and they remain excluded.
- Complete normal normalization sets: QT3 **42/0/6**, qt4 **44/0/6**. Both 4.0 default-on-empty original failures are repaired. All six retained skip names/reasons in each suite are unchanged.
- Fresh complete frozen gates: QT3 **31,164/0/657**, qt4 **8,895/0/1,914**, **229 gated sets**, 11 experimental passes. Exact retained skip mappings match C21. No admission changed.
- Every C20/C21 artifact is hash-preserved. Source/binary identities, original commands, bounded diagnostic receipts, raw logs and rendered HTML receipts are included in the [evidence manifest](evidence/manifest.json). HTML rendering uses unchanged Fytala CSS; it is not screenshot/PDF acceptance.

## Next bounded slice

Review exact admission of the repaired positive QT3 `cbcl-fn-normalize-unicode-006` and qualification of the now-green qt4 normalization set. The QT3 KnownSkip entry still incorrectly describes the required leading space as an upstream defect; it is retained pending a concrete owner-approved exact-baseline update. Do not admit the conditional negative tests or rewrite their original assertions. No upstream fixture correction is justified.

REQ-126 stays Complete under bounded owner acceptance. All historical dispositions and 48 unmet batch observations are preserved. Bosak.Schema owns staged XSD 1.1 work. No release, publication, merge or baseline adoption is included.
