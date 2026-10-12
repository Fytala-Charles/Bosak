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

The [case manifest](cases.json) contains 1,286 named checks. Each points to a complete
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
code in the exception message. For XSLT, `error` verifies either a compilation
exception or a failed capture message; it does not assert that detection occurs
before transformation. Successful XSLT cases still require successful capture,
no secondary outputs and exact principal bytes.

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

[Ordered queue prefix](../functions/ordered-queue-prefix.md) supplies take-while/drop-while with complete input/output, an XPath 3.1 comparison and 18 boundary/callback cases. Current coverage: 67 entries, 778 feature checks plus 24 chapter checks (802 total), and 117 assigned / 0 unassigned registered names. REQ-126 remains In Progress; source/public version remains 1.1.2. Bounded loop processing is now supplied by the reference below; dependency traversal and partial application are now supplied below.

[Bounded retry state](../functions/bounded-retries.md) documents while-do/do-until using invoice-submission response fixtures, complete input/output, a bounded XPath 3.1 fold, and 25 executable normal/edge/error cases. It explains precondition versus postcondition evaluation, explicit attempt limits, zero-budget guarding, and whole-state callback values. Current coverage is 67 entries and 802 documentation checks (778 feature checks plus 24 chapter checks); 117 of 117 registered names have entries, leaving 0. REQ-126 remains In Progress; source/public version stays 1.1.2. Dependency traversal and partial application are supplied by this delivery.

[Product dependency closure](../functions/product-dependency-closure.md) and [configured label callbacks](../functions/configured-label-callbacks.md) add two complete references for transitive-closure, partial-apply and identity, with full input/output, XPath 3.1 comparisons and 37 executable cases. Shared dependencies, cycles, node identity/document order, unresolved references, argument order and callback binding limits are explicit. Current coverage: 67 entries, 778 feature checks plus 24 chapter checks (802 total), and 117 assigned / 0 unassigned registered names. REQ-126 remains In Progress; source/public version stays 1.1.2.

[Month-end cutoffs](../functions/month-end-cutoffs.md) and [timestamp deadlines](../functions/timestamp-deadlines.md) add full scheduling references for build-dateTime, days-in-month, unix-dateTime, seconds and duration-to-seconds. Complete inputs/reports, XPath 3.1 comparisons and 36 executable cases explain Gregorian leap rules, numeric timezone offsets, stamp types, epoch milliseconds, signed/fractional durations and explicit budget boundaries. Coverage is 67 entries, 778 feature checks plus 24 chapter checks (802 total), and 117 assigned / 0 unassigned registered names. REQ-126 remains In Progress; source/public stays 1.1.2. URI parsing/construction and encoding are supplied by the reference below.

[Invoice API URIs](../functions/invoice-api-uris.md) supplies parse-uri, build-uri and decode-from-uri with full request input/output, a scoped XPath 3.1 construction comparison and 28 executable cases. It explains segment boundaries, query form decoding versus literal plus, repeated keys, permissive malformed-input decoding, component precedence and textual roundtrip limits. Coverage: 67 entries, 778 feature checks plus 24 chapter checks (802 total), and 117 assigned / 0 unassigned names. REQ-126 remains In Progress; source/public stays 1.1.2. Unicode/text cleanup and substring references are supplied below.

[Customer text cleanup](../functions/customer-text-cleanup.md) and [import file names](../functions/import-file-names.md) add eight function names with full input/output, scoped XPath 3.1 comparisons and 35 executable cases. They distinguish codepoints from grapheme clusters and display width, preserve interior whitespace, and explain final-delimiter/overlap rules. Bosak grapheme segmentation is approximate; complete Unicode segmentation remains outside the verified coverage. Current coverage: 67 entries, 778 feature checks plus 24 chapter checks (802 total), and 117 assigned / 0 unassigned registered names. REQ-126 remains In Progress; source/public stays 1.1.2. Ranking, numeric diagnostics and utility references are supplied below.

[Supplier offer ranking](../functions/supplier-offer-ranking.md), [numeric import diagnostics](../functions/numeric-import-diagnostics.md) and [payload fingerprints](../functions/payload-fingerprints.md) add the final eight registered function names with 41 executable cases. Current coverage: 67 entries, 778 feature checks plus 24 chapter checks (802 total); all 117 registered names have family entries. This completes name assignment, not REQ-126 acceptance: signatures/options, grammar/disposition provenance and conformance limits remain. Computed-key boundary ties require explicit filtering in the business example and follow-up specification/engine reconciliation. Source/public stays 1.1.2; REQ-126 remains In Progress.

[Typed measurement comparison](../functions/measurement-comparison.md) adds 27 executable cases for changed `fn:compare` behavior, with complete input/output and an explicit 3.1 double-normalization policy that differs at a decimal boundary. The runner verifies declared default-mode results or evaluation errors for existing functions. Keyword drift (`comparand1`/`comparand2` versus reviewed `value1`/`value2`) and legacy binary/empty-collation behavior are explicit. Current coverage: 67 entries, 778 feature plus 24 chapter checks (802 total); the 117-name assignment is unchanged. REQ-126 remains In Progress; source/public stays 1.1.2. Min/max references are now supplied; sorting references are now supplied; URI option contracts and component precedence are now supplied; next: catalog close-out and prioritized follow-up review.

For a changed existing function, `baselineError` verifies the default-3.1 compile/evaluation error marker; `baselineExpected` verifies its exact default-3.1 result. These fields apply to successful XPath cases and are mutually exclusive. Existing 4.0-only cases retain compile-refusal checks when neither is supplied. Experimental cases still verify refusal in frozen 4.0.

[Batch extrema and component codes](../functions/batch-extrema.md) adds 29 executable cases for `fn:min`/`fn:max`: complete batch input/output, explicit decimal normalization, namespace/local-name QName boundaries and a 3.1 fold comparison. Mixed numeric promotion/result-type and plain-duration limits remain explicit observed gaps against the pinned draft; these fixtures do not claim conformance. Current coverage: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 registered names remain assigned. REQ-126 remains In Progress; source/public stays 1.1.2. Sorting references are now supplied; URI option contracts and component precedence are now supplied; next: catalog close-out and prioritized follow-up review.

[Warehouse pick sorting](../functions/warehouse-pick-sorting.md) adds 28 cases for retained `fn:sort`/`array:sort`: complete input/output, a portable 3.1 comparison, stable composite location keys and preserved multi-item array members. Mixed numeric promotion, input-dependent NaN order and missing empty-key defaults and unknown-collation acceptance are explicit observed boundaries. Exact fn-sort/array-sort gate membership is recorded without claiming full conformance. Coverage: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names remain assigned. REQ-126 stays In Progress; source/public remains 1.1.2. URI option policy is now supplied; next: catalog close-out and prioritized follow-up review.

**Historical session close (2026-10-12, before URI review):** PR #173 merged after CI passed. Current reference coverage is 57 entries and 499 documentation checks (475 feature plus 24 chapter); all 117 registered names are assigned. Latest implementation validation: 4,497 tests passed across ten assemblies. REQ-126 remains In Progress; released/source version remains 1.1.2. Resume with URI options and component precedence; see [the handover](../../AGENT_HANDOVER.md).


**URI option review — 2026-10-12:** [Archive and partner URI policy](../functions/uri-option-policy.md) adds complete input/output, a scoped 3.1 comparison and 28 executable cases. Boolean options, UNC spelling, raw/structured authority and path/query precedence are explicit. Non-Boolean option truthiness and empty-query-map fallback remain observed draft gaps. Coverage: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names remain assigned. REQ-126 stays In Progress; source/public version remains 1.1.2. Next: catalog close-out and prioritized follow-up review.

**Computed XPath constructor disposition — 2026-10-12:** [Deferred node construction](../deferred/computed-node-constructors.md) separates the draft XPath feature from Bosak XQuery 3.1 support. Complete invoice input, refused XPath, 3.1 report and XSLT receipt output accompany 30 executable checks: 25 refusals across compatibility modes, three reports and two XML outputs. Current coverage: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names remain assigned. Version stays 1.1.2; REQ-126 remains In Progress. Named XSLT declaration dispositions are now supplied; enclosing modes are now supplied; capturing accumulator disposition is now supplied; fn:apply-templates disposition is now supplied; next: catalog close-out and prioritized follow-up review.

**Named XSLT declaration review — 2026-10-12:** [xsl:item-type](../deferred/xslt-item-type.md) and [xsl:record-type](../deferred/xslt-record-type.md) add full invoice cases, pinned disposition evidence and 28 executable boundary/alternative checks. Unused 4.0 declarations are ignored, not implemented; named bindings and constructors fail. Portable map validation and inline enums are verified. Structural-record XSLT `@as` raises XTTE0570; explicit XPath cast in `select` plus `as="map(*)"` is the verified workaround. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names assigned. REQ-126 remains In Progress, version 1.1.2. Next: catalog close-out and prioritized follow-up review.

**Enclosing-mode review — 2026-10-12:** [Deferred enclosing modes](../deferred/enclosing-modes.md) supplies a full invoice export/audit case, pinned draft/source evidence and 20 executable checks. Nested mode templates refuse with XTSE0260; top-level named rules provide supported dispatch, without enclosing-rule isolation. Explicit mode, #current and lexical defaults, empty/reordered input and namespace identity are verified. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names remain assigned. Version remains 1.1.2; REQ-126 In Progress. Next: catalog close-out and prioritized follow-up review.

**Capturing-accumulator review — 2026-10-12:** [Deferred capturing accumulators](../deferred/capturing-accumulators.md) adds full invoice input/output and 16 executable cases. Accepted capture=yes retains source identity rather than a draft-required snapshot; invalid/start-phase capture is tolerated in tested 4.0 forms. Ordinary accumulators, explicit snapshots and scalar projection are verified separately in memory, without streaming or special annotation-copy claims. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names assigned. REQ-126 remains In Progress, version 1.1.2. Next: catalog close-out and prioritized follow-up review.

**fn:apply-templates review — 2026-10-12:** [Deferred function and invoice alternatives](../deferred/fn-apply-templates.md) adds 28 executable checks. Calls/references refuse with XPST0017; instruction availability is separate from function availability. Full invoice input/output verifies named-mode dispatch and a stylesheet-function wrapper with ordinary/tunnel parameters, empty/reordered input and explicit joining. Draft dynamic options/public-mode validation remain deferred. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names assigned. Version 1.1.2, REQ-126 In Progress. Next: catalog close-out and prioritized follow-up review.

**Serialization parameter review — 2026-10-12:** [Nine draft additions and invoice export](../deferred/serialization-parameters.md) adds 52 executable cases. XSLT JSON solidus control works; fn:serialize ignores the new keys. JSON Lines, canonicalization, layout controls and CSV serialization remain deferred; fn CSV fallback and XSLT sequence-as-array are explicit boundaries. Complete portable JSON Lines input/output includes empty/reordered cases. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names assigned. Version 1.1.2, REQ-126 In Progress. Next: catalog close-out and prioritized follow-up review.

**External-input function review — 2026-10-12:** [Five function boundaries and partner invoices](../deferred/external-input-functions.md) adds 55 checks. parse-html, html-doc, invisible-xml, unparsed-binary and xsd-validator remain absent/out of the original implementation scope. Pinned contracts and per-function dependency/ownership rationale accompany complete normalized XML, fixed-record, binary-value and business-guard alternatives. These do not implement HTML recovery, arbitrary grammar parsing, binary acquisition or XSD factory validation. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 registered names assigned. Version 1.1.2, REQ-126 In Progress. Next: catalog close-out and prioritized follow-up review.

**GNode/JNode scope review — 2026-10-12:** [Partial model and invoice navigation](../data-model/jnode-template-boundary.md) replaces blanket exclusion with shipped JNode support and a deferred remainder. Eighteen added checks verify full invoice navigation/portable reports and broader node()/xnode()/identity gaps. Full generic-model and stylesheet built-in behavior remain uncertified. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 registered names assigned. Version 1.1.2, REQ-126 In Progress. Next: catalog close-out and prioritized follow-up review.

**Language/streaming scope review — 2026-10-12:** [Existing APIs and separate 4.0 deltas](../deferred/language-streaming-scope.md) adds 28 cases through XQuery, XPath, ordinary XSLT and actual streaming APIs. Corrects stale XPath where exclusion and distinguishes REQ-087 Done from historical residual reports. Full XQuery/Streaming 4.0 remains unclaimed. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names assigned. Version 1.1.2, REQ-126 In Progress. Next: catalog close-out and prioritized follow-up review.

The harness also executes `kind: xquery` with XQueryCompiler/XQueryContext. For `kind: xslt`, modes `stream31`/`stream40` use TransformStreamingToString with a disposed file stream; other XSLT modes use capture. Explicit `frozenExpected` permits an experimental case to declare an existing frozen-4.0 outcome instead of asserting refusal. These metadata fields do not configure a new engine version.


**Focus-constructor audit — 2026-10-12:** [Typed supplier delivery import and all 49 names](../operators-and-syntax/focus-functions.md) adds 508 executable checks, representative result types, lexical/source errors, missing context, empty/multiple input, default 3.1 refusal, experimental parity and portable 3.1 reports. List types and xs:error are explicit. Current coverage: 67 entries, 1,286 feature + 24 chapter checks = 1,310; four worked chapters; all 117 names assigned. Version remains 1.1.2, REQ-126 In Progress. This current snapshot supersedes earlier delivery counts. Next: D2, supplementary signatures/keywords/defaults/options.
