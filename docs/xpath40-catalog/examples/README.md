<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak executable catalog examples">
  <br><br><h1>Executable Catalog Examples</h1>
  <p>Real tasks, earlier-version baselines, edge cases, and explicit compatibility</p>
</div>

**22 October 2026 — source 1.23.3:** 4.0 node unions now use the minimum alternative default priority, aligned with the pinned current draft. The two catalog reproducers now expect `competitor`; their prior 1.23.2 observations remain recorded. 3.0 dispatch, explicit priorities and single-rule 4.0 `next-match` are preserved. [Correction and validation](../UNION-MINIMUM-PRIORITY.md). Public release remains 1.23.2; REQ-126 stays In Progress. Earlier delivery snapshots below are historical.

**REQ-126 compound-pattern review — 21 October 2026:** [Grammar mapping](../COMPOUND-PATTERN-GRAMMAR.md) adds30 manifest checks, including six complete3.0/4.0 manual reports. Two rows are explicit implementation observations: source1.23.2 uses maximum union alternative priority, while the pinned draft requires minimum; [separate finding](../UNION-PRIORITY-ENGINE-FINDING.md) records the normative outcome and correction scope. Catalog71 entries /2,767 checks. REQ-126 remains In Progress; D2/D3/D4 and AC-03 partial. No engine/API/harness/version/gate/release change; prior count snapshots are historical.

**REQ-126 type/pattern review — 21 October 2026:** [Current grammar mapping](../TYPE-PATTERN-GRAMMAR.md) and [intake reports](intake-types/README.md) add40 executable checks and one16-field structured-value pattern entry. Current catalog: **70 entries;2,713 feature +24 chapter checks =2,737**. Existing alias/record pages now describe bounded published1.23.2 XSLT support; older source/deferral logs are preserved separately. D2/D3/D4 and AC-03 remain partial; REQ-126 stays In Progress. No engine/API/harness/version/profile/gate/release change. Earlier count/status snapshots are historical.

**REQ-126 D3 grammar review — 21 October 2026:** [Switch/array/select mapping](../SWITCH-ARRAY-SELECT-GRAMMAR.md) adds a16-field [xsl:select entry](../xslt-instructions/xsl-select.md) and37 executable checks, including six full dispatch reports. Current catalog: **69 entries;2,673 feature +24 chapter checks =2,697**; four chapters and117 assigned function names unchanged. Public/source1.23.2 is verified. Instruction recognition, effective XPath version and host profile are distinct. REQ-126 remains In Progress; D2/D3/D4 and AC-03 remain partial. No engine/API/harness/version/gate change or release; earlier status/count snapshots are historical.

**REQ-126 equality/collation review - 21 October 2026:** [Five-family review](../EQUALITY-COLLATION-CONTRACT-REVIEW.md) and [customer-reference matching](../functions/customer-reference-matching.md) add58 executable checks, including six complete3.1/4.0 reports. Current catalog: **68 entries;2,636 feature +24 chapter checks =2,660**; four worked chapters and117 assigned names unchanged. Source1.23.1 remains unpublished; public1.23.0 unchanged. Two engine boundaries and compare keyword drift are explicitly separate observations, not supported draft contracts. REQ-126 remains In Progress; earlier counts/version statements below are historical.


## Run the examples

From the repository root:

```powershell
dotnet test tests/Bosak.Xslt.Tests/Bosak.Xslt.Tests.csproj -c Release --filter FullyQualifiedName~CatalogExampleTests
```

The [case manifest](cases.json) contains 2,743 named checks. Each points to a complete
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


**Signature contract review — 2026-10-12:** [Metadata matrix and consumer boundaries](../SIGNATURE-CONTRACT-AUDIT.md) supplies 68 new-name families plus 20 selected existing functions, 148 exact registration checks and 39 behavior cases (187 new checks). 37 families lack keyword metadata; four keyword-name differences and three default-expression differences are recorded. Replace arity/default and two-let hash keyword failures have reproducible cases. Hash extension options are implementation-defined; missing mandatory algorithms remain distinct. Catalog: 67 entries, 1,473 feature + 24 chapter checks = 1,497; version 1.1.2, REQ-126 In Progress. D2 remains partial. This snapshot supersedes earlier delivery counts; next review missing metadata/defaults/options family by family.


**Text/Unicode contract review — 2026-10-12:** [Eight-family review and full label/filename reports](../TEXT-CONTRACT-REVIEW.md) adds 83 checks: 24 keyword/mode refusals, 55 positional/default/empty/invalid/option cases and four normal/empty 3.1/4.0 reports. Metadata remains absent; eight of 37 such families are reviewed and 29 remain. Empty-source/empty-needle boundary wording corrected; coercion/option-validation follow-ups remain explicit. Current catalog: 67 entries, 1,556 feature + 24 chapter checks = 1,580; version 1.1.2, REQ-126 In Progress. This snapshot supersedes earlier delivery counts. Next: sequence selection/repetition contracts, keeping D2 partial.


**Sequence contract review — 2026-10-12:** [Six-family review and complete dispatch batch reports](../SEQUENCE-CONTRACT-REVIEW.md) adds 65 checks: 18 keyword/mode cases, 41 focused defaults/types/identity/keyword-validation cases and six normal/empty/reordered 3.1/4.0 reports. Five more absent-keyword families reviewed; 13 of 37 total, 24 awaiting targeted review. slice keywords/defaults work; items-at accepts the tested integral decimal, while replicate/slice reject it. Untyped-node position/coercion limits remain explicit. Catalog: 67 entries, 1,621 feature + 24 chapter checks = 1,645; version 1.1.2, REQ-126 In Progress, D2 partial. This snapshot supersedes earlier delivery counts. Next: equality/distinctness/subsequence predicate contracts.


**Predicate contract review — 2026-10-12:** [Equality/distinctness/subsequence contracts and complete shipment reports](../PREDICATE-CONTRACT-REVIEW.md) add 102 checks: 15 keyword/mode refusals, 79 positional/default/type/callback checks and eight normal/empty/reordered/duplicate-event reports in 3.1/4.0. Targeted absent-keyword review now covers 18 of 37 families, with 19 remaining; metadata remains absent. Map acceptance in atomic-signature equality functions and numeric/empty-string collation processing remain engine follow-ups. Current catalog: 67 entries, 1,723 feature + 24 chapter checks = 1,747. Version 1.1.2, REQ-126 In Progress, D2 partial. This snapshot supersedes earlier counts and next-step queues. Next: duplicate-values/highest/lowest/index-where comparison and selection contracts.


**Comparison/selection contract review — 2026-10-12:** [Four-family review and detailed warehouse reports](../SELECTION-CONTRACT-REVIEW.md) add 68 checks: 12 keyword/mode cases, 50 focused default/type/callback/keyword-validation checks and six complete 3.1/4.0 warehouse reports. Three more absent-keyword families reviewed: 21 of 37 supplied, 16 remaining, metadata unchanged. index-where keywords succeed. Map atomization and extrema NaN ordering remain explicit follow-ups. Current catalog: 67 entries, 1,791 feature + 24 chapter checks = 1,815; version 1.1.2, REQ-126 In Progress, D2 partial. This snapshot supersedes earlier counts and queues. Next: map:entries/map:items/map:filter and array:build/array:items/array:empty container contracts.


**Container contract review — 2026-10-12:** [Six-family review and detailed stock-feed reports](../CONTAINER-CONTRACT-REVIEW.md) 89 new checks: 18 keyword/mode refusals, 65 focused member/item/default/type/callback checks and six complete XPath 3.1/4.0 stock-feed reports. Targeted absent-keyword review: 27 of 37 families supplied, 10 remaining; metadata unchanged. map:filter accepts tested string/numeric predicate results despite its boolean result signature; dedicated conversion/enforcement review remains. Current catalog: 67 entries, 1,880 feature + 24 chapter checks = 1,904; four worked chapters, 117 names assigned. Version 1.1.3, REQ-126 In Progress, D2 partial; no engine/API/harness change or release. This snapshot supersedes earlier counts and queues. Next: build-dateTime, days-in-month, unix-dateTime, seconds and duration-to-seconds contracts.


**Date/time contract review — 2026-10-12:** [Five-family review and detailed receipt-retention reports](../DATE-TIME-CONTRACT-REVIEW.md) 108 new checks: 15 keyword/mode refusals, 87 focused Gregorian/timezone/unit/type/cardinality checks and six full XPath 3.1/4.0 receipt-retention reports. Targeted absent-keyword review: 32 of 37 supplied, five remaining; metadata unchanged. Malformed string/multi-item tolerance, record conversion and fractional-component precision remain explicit follow-ups. Current catalog: 67 entries, 1,988 feature + 24 chapter checks = 2,012; four worked chapters, 117 names assigned. Version 1.1.3, REQ-126 In Progress, D2 partial; no engine/API/harness change or release. This snapshot supersedes earlier counts and queues. Next: build-uri, decode-from-uri, collation-available, identity and is-NaN.


**URI/import utility contract review — 2026-10-12:** [Five-family review and detailed import/link reports](../URI-UTILITY-CONTRACT-REVIEW.md) 105 new checks: 15 keyword/mode refusals, 82 focused URI/option/decoding/collation/identity/NaN checks and eight complete XPath 3.1/4.0 import/link reports. All 37 originally absent-keyword families now have targeted review; zero await that first review, but all 37 still lack metadata. Boolean-option and record-field tolerance, empty query-map precedence and context-dependent collation boundaries remain follow-ups. Current catalog: 67 entries, 2,093 feature + 24 chapter checks = 2,117; four worked chapters, 117 names assigned. Version 1.1.3, REQ-126 In Progress, D2 partial; no engine/API/harness change or release. This snapshot supersedes earlier counts and queues. Next: exhaustively inventory changed existing 3.1 function contracts; the current matrix samples only twenty.


**Complete baseline-name inventory — 2026-10-12:** [Existing 3.1 contract inventory](../CHANGED-31-CONTRACT-INVENTORY.md) The complete 197-name fn/math/map/array baseline inventory is now supplied, with dated W3C 3.1 declarations, pinned F&O e7ee4ab8 declarations and fresh 3.1/4.0 evidence. Added 353 checks: 320 exact-arity lookups, 17 named-reference refusals, ten focused defaults/behavior checks and six complete stock reports. Thirteen families have positional arity declaration differences; seventeen draft arities are unavailable in the fresh standard context. Type/prose differences remain candidates for semantic review, not certified behavioral changes. Empty substring/subsequence lengths have distinct observed 3.1 and 4.0 results. Empty sort keys and map:merge options refuse XPTY0004. Catalog: 67 entries, 2,446 feature + 24 chapter checks = 2,470; four chapters, 117 assigned names. All 37 reviewed keyword families still lack metadata. REQ-126 stays In Progress, D2 partial; version 1.1.3, no engine/API/harness change or release. Next: fn:sort, array:sort, fn:min, fn:max and map:merge defaults/conversion contracts. Prose classification, D3/D4, historical adoption pins and AC-03 remain open.


**Ordering/default review — 2026-10-12:** [Five-family review and supplier reports](../ORDER-DEFAULT-CONTRACT-REVIEW.md) Targeted review of fn:sort, array:sort, fn:min, fn:max and map:merge adds 132 checks (126 focused, six complete supplier reports), preserving source declarations and exact 3.1/4.0/experimental keyword boundaries. min/max have no key callback parameter. Explicit empty sort keys and merge options, unknown sort collations, NaN ordering and malformed merge input/option cardinality remain engine follow-ups. Supplier rankings use numeric price plus stable ID and explicit duplicate combine; normal/reordered results match. Current catalog: 67 entries, 2,578 feature + 24 chapter checks = 2,602; four worked chapters, 117 assigned names. Version 1.1.3, REQ-126 In Progress, D2 partial. All 37 reviewed keyword families still lack metadata; no engine/API/harness change or release. Next: distinct-values, index-of, deep-equal, compare and contains-token equality/collation contracts. Earlier counts and queues are historical; normative prose impact, D3/D4, historical pins and AC-03 remain open.
