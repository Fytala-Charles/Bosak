<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak existing XPath function contract inventory">
  <br><br><h1>Existing XPath 3.1 Function Contract Inventory</h1>
  <p>Dated 3.1 declarations, pinned 4.0 differences and consumer boundaries</p>
</div>

## Scope and outcome

Reviewed 2026-10-12 against Bosak `21ca1497df93b6c2d7186406be7189ef28b32618`, published version 1.1.3. The [raw inventory](audit-changed31-contracts.json) covers **all 197 fn/math/map/array names** in the dated [W3C F&O 3.1 Recommendation catalog](https://www.w3.org/TR/2017/REC-xpath-functions-31-20170321/function-catalog.xml), comparing signatures and contract-section fingerprints with [QT4CG F&O e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml). Namespace counts: array: 18, fn: 155, map: 10, math: 14. SHA-256 hashes record both exact downloaded sources.

353 new checks: 320 exact-arity lookup cases, 17 named-reference refusals, ten focused behavior cases and six complete normal/empty/reordered stock-lookup reports. Each successful 4.0 case records its independently probed 3.1 result. Catalog: 67 entries, 2,446 feature + 24 chapter checks = 2,470; four worked chapters. All 37 formerly unreviewed absent-keyword families retain targeted review and missing metadata. REQ-126 stays In Progress, D2 partial; no engine/API/harness change, gate promotion or release.

The earlier [88-row signature matrix](SIGNATURE-CONTRACT-AUDIT.md) remains 68 new names plus twenty selected existing functions and 148 literal registrations. This complementary inventory replaces the missing all-name declaration scan; it does not certify exhaustive semantic differences, conversion or options. op:* operators, xs:* constructors, new draft names and XSLT-only functions are outside this baseline-name inventory. Their existing catalog audits remain separate. Review pins do not reconstruct historical adoption.

## Method and precise limits

Enumerate every baseline function by namespace and local name, then retain every baseline/pinned prototype, raw parameter/return type, default/note, type-reference record fields, change-annotation attributes, live literal registration and lookup evidence. No baseline name is discarded because its signatures appear unchanged. All 197 names have a corresponding pinned draft declaration. Runtime evidence comes from fresh standard-only EvaluationContexts at XPath31 and Frozen XPath40.

Comparison normalizes whitespace and function( versus fn( spelling for type text only. Named function parameters, unions, optionality and record references can still produce syntactic differences without proving a different accepted value set. Parameter labels are declaration facts; 3.1 does not thereby support keyword calls. Default expressions are raw declarations, not proof of evaluation timing, omission behavior or default-on-empty enforcement. A post-2017 or undated change annotation is review evidence, not proof that a normative 4.0 contract changed. Every contract-section fingerprint differs in this structural comparison, including edited cross-references; that broad flag must not be described as 197 proven behavioral changes. Full prose/semantic impact classification remains open.

concat is variadic: baseline minimum two, pinned minimum zero, both unbounded. The baseline ellipsis is not a third required parameter. The pinned variadic property is read alongside its single defaulted sequence parameter. Finite arity representatives and a null maximum preserve this distinction; no finite lookup sample certifies every possible variadic arity.

## Declaration differences and availability

Overlapping counts: **13** arity-declaration families, **79** parameter/result type-text families, **145** parameter-label families, **77** families with pinned default expressions, and **73** with post-3.1 or undated change annotations. These are candidate review categories, not separate disjoint feature totals. Twelve arity families were outside the original twenty-function sample; replace was already recorded.

| Family | 3.1 arities | Pinned 4.0 arities | Fresh 4.0 lookup boundary |
|---|---|---|---|
| `array:get` | 2 | 2, 3 | Zero for 3 |
| `fn:concat` | 2 and higher | 0 and higher | Zero for 0, 1 |
| `fn:doc` | 1 | 1, 2 | Zero for 2 |
| `fn:doc-available` | 1 | 1, 2 | Zero for 2 |
| `fn:format-date` | 2, 5 | 2, 3, 4, 5 | Zero for 3, 4 |
| `fn:format-dateTime` | 2, 5 | 2, 3, 4, 5 | Zero for 3, 4 |
| `fn:format-time` | 2, 5 | 2, 3, 4, 5 | Zero for 3, 4 |
| `fn:parse-xml` | 1 | 1, 2 | Zero for 2 |
| `fn:parse-xml-fragment` | 1 | 1, 2 | Zero for 2 |
| `fn:path` | 0, 1 | 0, 1, 2 | Zero for 2 |
| `fn:replace` | 3, 4 | 2, 3, 4 | Zero for 2 |
| `fn:round` | 1, 2 | 1, 2, 3 | Zero for 3 |
| `map:get` | 2 | 2, 3 | Zero for 3 |

The 17 missing draft arities belong to those thirteen families: array:get#3, concat#0/#1, doc#2, doc-available#2, format-date/format-dateTime/format-time#3/#4, parse-xml#2, parse-xml-fragment#2, path#2, replace#2, round#3 and map:get#3. Named references refuse XPST0017 for all seventeen tested forms. Available legacy arities return lookup count one. The runtime check is lookup availability, not invocation of every valid overload. Resource-related named-reference diagnostics compile only; they do not retrieve resources.

fn:transform#1 also returns zero in the standard-only probe, in both modes. It has matching 3.1/4.0 signatures and no literal standard registration. This is a **host boundary**, not a new unsupported-engine finding: [XsltFunctionLibrary.PopulateTransformOnly](../../src/Bosak.Xslt/Api/XsltFunctionLibrary.cs) registers it for an XSLT-capable context. The inventory does not invoke it or certify that host's options.

## Focused defaults and portable business workflow

concat() and concat(('A','B')) refuse XPST0017; replace with two positional arguments still refuses XPST0017. At Frozen 4.0, substring('SKU-A',5,()) returns A and subsequence(('A','B','C'),2,()) returns B,C. Their independently probed 3.1 results differ: substring returns an empty string and subsequence refuses XPTY0004. The fixtures assert both observed profiles. This is explicit empty-length evidence for those examples, not full numeric coercion coverage. sort((3,1,2),(),()) refuses XPTY0004 despite the pinned empty-key default data#1, and map:merge(...,()) refuses XPTY0004 despite the pinned empty-options default. Existing helper/default/keyword findings remain separate; do not infer that all empty/default arguments fail.

The portable map fallback uses map:contains then map:get#2; the array fallback checks one-based bounds before array:get#2. Missing stock codes default to zero for this report, and absent requested rows are labelled missing-row. Explicit zero stock stays present. Existing format-date#5 produces the expected date with empty locale/calendar/place parameters. The [detailed stock-lookup README](examples/changed31-contracts/README.md) publishes complete XML, paired 3.1/4.0 portable code, exact outputs, all availability evidence and the focused diagnostics. It does not present unsupported three-argument getters as runnable 4.0 alternatives.

## Next work

Prioritize existing higher-order/default contracts: fn:sort, array:sort, fn:min, fn:max and map:merge. Compare their precise key/comparator/options declarations, default-on-empty forms, conversion/error behavior and keyword versus reference/lookup paths, using the complete inventory to prevent omitted overloads. The all-name inventory is supplied; normative prose/semantic impact and full behavioral contracts remain partial. D3 grammar/pattern, D4 acceptance/provenance, historical adopted pins and AC-03 applicability remain unresolved. No release or closure follows from declaration coverage.


## Delivery validation

Release solution build passed (zero warnings/errors); **6,674 tests / 0 failures / 0 skips**, ten assemblies including all 2,446 feature checks. Renderer verified 101 catalog pages plus six canonical/implementation pages. All 197 baseline names and both exact source prototype sets/hashes, variadic bounds, 353 new checks, six independently calculated reports, profile differences, 67 sixteen-field entries, 241 cached primary links/anchors, 28 delivery/test records and 117 assigned names verified. All 37 reviewed keyword rows still lack metadata. Manual engine gates were not rerun for this documentation/example-only slice.
