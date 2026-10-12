<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak executable catalog examples">
  <br><br><h1>Executable Catalog Examples</h1>
  <p>Real tasks, earlier-version baselines, edge cases, and explicit compatibility</p>
</div>

## Run the examples

From the repository root:

```powershell
dotnet test tests/Bosak.Xslt.Tests/Bosak.Xslt.Tests.csproj -c Release --filter FullyQualifiedName~CatalogExampleTests
```

The [case manifest](cases.json) contains 475 named checks. Each points to a complete
published expression or stylesheet and declares its expected text or error. The
test project copies those exact files; [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs)
uses public compiler/evaluation APIs and verifies the declared outcomes.

| Mode | How it runs |
|---|---|
| `31` | `XPath31Expression.Compile` with `XPathCompatibility.XPath31`. |
| `40` | Explicit `XPathCompatibility.XPath40`; successful cases verify default compile refusal unless `baselineError` or `baselineExpected` declares the default evaluation outcome. |
| `experimental` | Explicit `XPathCompatibility.XPath40Experimental`; successful cases verify refusal at both lower modes. |
| `xslt31` | Complete standard XSLT 3.0 baseline, paired with its declared XML input. |
| `xslt40` | Complete version="4.0" stylesheet demonstrating a Bosak-supported draft instruction; effective-version expressions use frozen XPath40 in 1.1.1; local earlier-version overrides retain XPath31. |

XPath files declare their synthetic input inline and require no external variables,
namespaces, or document focus. XSLT files declare their namespaces and use the
manifest's `input` file. Text-method capture returns the exact expected text
without file output. Error cases intentionally fail and verify the recorded error
code in the exception message.

## Read the comparisons honestly

Before/after cases agree on the illustrated result. They do not claim universal
semantic equivalence. For example, the CSV baseline begins with a host-parsed
row array: it demonstrates using the row, not implementing a CSV parser in 3.1.
The record baseline checks relevant map fields; it does not create a record
annotation. The coercion baseline uses an explicit constructor; its behavior on
fractional values differs from integer relabeling.

The stock-balance scan returns a **sequence of single-member arrays**, including
the initial state. Mapping arrows process individual items. The rejected `=>>`
case cannot compile; the deferred method-call entry instead demonstrates the
stable `array:size` alternative. Neither example introduces support for absent
syntax.

The generic fixture vocabulary is a dispatch task with priority and product
codes. Larger document cases live in the [worked chapters](../worked-examples/README.md).
Both types of evidence complement engine unit/conformance tests; these examples
do not change GatedSets or imply standards-level compatibility for every draft
surface. See the [catalog index](../README.md) for feature entries.

The new sequence/HOF and decision examples complement the [order-routing chapter](../worked-examples/order-routing/README.md), which uses an XML document context and a host-supplied plan rather than the context-free manifest runner.

## Indexing, batches and export selections

Read [product indexing](../functions/product-map-index.md),
[array batches](../functions/import-array-batches.md) and
[sequence selection](../functions/order-sequence-selection.md) for 40 additional
checks. Their complete expressions declare all synthetic input inline.
Comparisons at mode 31 explain exactly which behavior agrees and which differs.
The map result-shape probes now verify corrected source-1.1.2 contracts:
`map:entries` returns a sequence and `map:items` concatenates value items. The
[entry](../functions/product-map-index.md) retains the published-1.1.1 differences
and migration guidance. These checks verify the illustrated contracts, not
complete draft conformance.

## Shipment-event sequence checks

[Shipment histories](../functions/shipment-event-sequences.md) add 27 checks for
all-equal and contiguous subsequence/prefix/suffix contracts. Complete expressions
parse synthetic XML inline; the entry shows full normal/empty/unexpected-event
input/output, the earlier-version report and every exact result/error.
Missing IDs/codes are checked before acceptance, and an empty history cannot
pass merely because all-equal and universal predicates are vacuously true.

[Ordered queue prefix](../functions/ordered-queue-prefix.md) supplies take-while/drop-while with complete input/output, an XPath 3.1 comparison and 18 boundary/callback cases. Current coverage: 57 entries, 475 feature checks plus 24 chapter checks (499 total), and 117 assigned / 0 unassigned registered names. REQ-126 remains In Progress; source/public version remains 1.1.2. Bounded loop processing is now supplied by the reference below; dependency traversal and partial application are now supplied below.

[Bounded retry state](../functions/bounded-retries.md) documents while-do/do-until using invoice-submission response fixtures, complete input/output, a bounded XPath 3.1 fold, and 25 executable normal/edge/error cases. It explains precondition versus postcondition evaluation, explicit attempt limits, zero-budget guarding, and whole-state callback values. Current coverage is 57 entries and 499 documentation checks (475 feature checks plus 24 chapter checks); 117 of 117 registered names have entries, leaving 0. REQ-126 remains In Progress; source/public version stays 1.1.2. Dependency traversal and partial application are supplied by this delivery.

[Product dependency closure](../functions/product-dependency-closure.md) and [configured label callbacks](../functions/configured-label-callbacks.md) add two complete references for transitive-closure, partial-apply and identity, with full input/output, XPath 3.1 comparisons and 37 executable cases. Shared dependencies, cycles, node identity/document order, unresolved references, argument order and callback binding limits are explicit. Current coverage: 57 entries, 475 feature checks plus 24 chapter checks (499 total), and 117 assigned / 0 unassigned registered names. REQ-126 remains In Progress; source/public version stays 1.1.2.

[Month-end cutoffs](../functions/month-end-cutoffs.md) and [timestamp deadlines](../functions/timestamp-deadlines.md) add full scheduling references for build-dateTime, days-in-month, unix-dateTime, seconds and duration-to-seconds. Complete inputs/reports, XPath 3.1 comparisons and 36 executable cases explain Gregorian leap rules, numeric timezone offsets, stamp types, epoch milliseconds, signed/fractional durations and explicit budget boundaries. Coverage is 57 entries, 475 feature checks plus 24 chapter checks (499 total), and 117 assigned / 0 unassigned registered names. REQ-126 remains In Progress; source/public stays 1.1.2. URI parsing/construction and encoding are supplied by the reference below.

[Invoice API URIs](../functions/invoice-api-uris.md) supplies parse-uri, build-uri and decode-from-uri with full request input/output, a scoped XPath 3.1 construction comparison and 28 executable cases. It explains segment boundaries, query form decoding versus literal plus, repeated keys, permissive malformed-input decoding, component precedence and textual roundtrip limits. Coverage: 57 entries, 475 feature checks plus 24 chapter checks (499 total), and 117 assigned / 0 unassigned names. REQ-126 remains In Progress; source/public stays 1.1.2. Unicode/text cleanup and substring references are supplied below.

[Customer text cleanup](../functions/customer-text-cleanup.md) and [import file names](../functions/import-file-names.md) add eight function names with full input/output, scoped XPath 3.1 comparisons and 35 executable cases. They distinguish codepoints from grapheme clusters and display width, preserve interior whitespace, and explain final-delimiter/overlap rules. Bosak grapheme segmentation is approximate; complete Unicode segmentation remains outside the verified coverage. Current coverage: 57 entries, 475 feature checks plus 24 chapter checks (499 total), and 117 assigned / 0 unassigned registered names. REQ-126 remains In Progress; source/public stays 1.1.2. Ranking, numeric diagnostics and utility references are supplied below.

[Supplier offer ranking](../functions/supplier-offer-ranking.md), [numeric import diagnostics](../functions/numeric-import-diagnostics.md) and [payload fingerprints](../functions/payload-fingerprints.md) add the final eight registered function names with 41 executable cases. Current coverage: 57 entries, 475 feature checks plus 24 chapter checks (499 total); all 117 registered names have family entries. This completes name assignment, not REQ-126 acceptance: signatures/options, grammar/disposition provenance and conformance limits remain. Computed-key boundary ties require explicit filtering in the business example and follow-up specification/engine reconciliation. Source/public stays 1.1.2; REQ-126 remains In Progress.

[Typed measurement comparison](../functions/measurement-comparison.md) adds 27 executable cases for changed `fn:compare` behavior, with complete input/output and an explicit 3.1 double-normalization policy that differs at a decimal boundary. The runner verifies declared default-mode results or evaluation errors for existing functions. Keyword drift (`comparand1`/`comparand2` versus reviewed `value1`/`value2`) and legacy binary/empty-collation behavior are explicit. Current coverage: 57 entries, 475 feature plus 24 chapter checks (499 total); the 117-name assignment is unchanged. REQ-126 remains In Progress; source/public stays 1.1.2. Min/max references are now supplied; sorting references are now supplied; next: URI option contracts and component precedence.

For a changed existing function, `baselineError` verifies the default-3.1 compile/evaluation error marker; `baselineExpected` verifies its exact default-3.1 result. These fields apply to successful XPath cases and are mutually exclusive. Existing 4.0-only cases retain compile-refusal checks when neither is supplied. Experimental cases still verify refusal in frozen 4.0.

[Batch extrema and component codes](../functions/batch-extrema.md) adds 29 executable cases for `fn:min`/`fn:max`: complete batch input/output, explicit decimal normalization, namespace/local-name QName boundaries and a 3.1 fold comparison. Mixed numeric promotion/result-type and plain-duration limits remain explicit observed gaps against the pinned draft; these fixtures do not claim conformance. Current coverage: 57 entries, 475 feature plus 24 chapter checks (499 total); all 117 registered names remain assigned. REQ-126 remains In Progress; source/public stays 1.1.2. Sorting references are now supplied; next: URI option contracts and component precedence.

[Warehouse pick sorting](../functions/warehouse-pick-sorting.md) adds 28 cases for retained `fn:sort`/`array:sort`: complete input/output, a portable 3.1 comparison, stable composite location keys and preserved multi-item array members. Mixed numeric promotion, input-dependent NaN order and missing empty-key defaults and unknown-collation acceptance are explicit observed boundaries. Exact fn-sort/array-sort gate membership is recorded without claiming full conformance. Coverage: 57 entries, 475 feature plus 24 chapter checks (499 total); all 117 names remain assigned. REQ-126 stays In Progress; source/public remains 1.1.2. Next: URI option contracts and component precedence.
