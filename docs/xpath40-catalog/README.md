<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak XPath 4.0 feature catalog">
  <br><br>
  <h1>The Bosak XPath/XSLT 4.0 Feature Catalog</h1>
  <p>Structured reference for every XPath/XSLT 4.0 syntax decision in Bosak — where each feature comes from, why it exists, what it replaces, and what it costs</p>
</div>

<!-- The Bosak XPath/XSLT 4.0 Feature Catalog -->
<!-- Living document: one file per feature; index updated in the same PR as any entry. -->

> **Purpose:** A catalog, in the spirit of the Design Patterns template (Gamma, Helm, Johnson, Vlissides, pp. 6–7), of every XPath/XSLT 4.0 feature Bosak ships, defers, or rejects. **Audience:** engine users choosing which 4.x idioms to adopt, Bosak.Braid (which syntax it may generate per compatibility level), and contributors extending the engine. **Living document:** last updated 2026-10-12. **Governing REQ:** [REQ-126](../FEATURE_REQUESTS.md); adoption plan and stability tiers: [REQ-118 dossier](../REQ-118-xpath-xslt-40.md); experimental track: [REQ-123](../FEATURE_REQUESTS.md).

## Direction: a book and a working reference

This catalog is intended to develop into a book or reference guide. Readers should be able to follow chapters to learn how features solve document-processing problems, or look up a single entry while implementing a transformation. The catalog now contains 67 entries and four worked chapters. Further coverage of shipped functions and deferred draft decisions remains REQ-126 work.

Examples must reflect real work. Use synthetic but plausible fixtures: supplier invoices mapped into an ERP format, order lines grouped into a reconciliation report, XML manuals published as HTML, or transaction amounts accumulated into running balances. State the business problem before introducing the syntax, and explain why the feature helps. Publishing examples should preserve mixed content and cross-references; mapping examples should handle namespaces, repeated records, optional fields, and conversions.

A worked example must include representative input, a complete expression or stylesheet with its required context, expected output, the compatibility level, and at least one meaningful edge case. Declare simplifications, such as a reduced invoice vocabulary; a small fixture must not imply compliance with an industry standard. Pair new syntax with the earlier idiom where a migration comparison helps. Short syntax probes can supplement the worked example.

Worked-example stylesheets must be understandable independently of the chapter:
include a file contract and change history, helper purpose/parameter/return/edge
blocks, and template context/output/precondition blocks. Explain business choices
and non-obvious invariants rather than narrating each instruction. See the
[XSLT style-guide applicability note](../XSLT_STYLE_GUIDE.md) and the annotated
[invoice stylesheet](worked-examples/invoice-to-erp/invoice-to-erp.xsl).

Keep reader-facing explanations before engine implementation details. Chapter introductions and cross-links will connect the individual entries into a learning path; the feature index remains available for quick lookup. Runnable examples must meet REQ-126's engine-verification requirement before their results are presented as verified.

## Start with a worked chapter

[Supplier invoices to ERP XML](worked-examples/invoice-to-erp/README.md) is the first complete case: a 3.0 stylesheet, realistic inputs, expected principal/exception outputs, and seven executable checks. It includes a separate verified 3.1/4.0 map-destructuring comparison. Continue with [XML manual publishing](worked-examples/manual-to-html/README.md) and [order reconciliation](worked-examples/order-reconciliation/README.md). The [example runner](examples/README.md) verifies 778 focused catalog cases and migration baselines.

## External specifications and source documentation

These primary sources provide language definitions, migration baselines, draft
history, and upstream tests. Links and section anchors were checked on
2026-10-11. The 4.0 documents are evolving QT4CG drafts, not final W3C
Recommendations. Current links are navigation references; they do not prove that
Bosak adopted every feature or the latest semantics. Record the adopted draft
revision/date and Bosak delivery revision alongside feature-specific references.
When wording or numbering changes, retain historical disposition evidence and
compare the current draft explicitly.

| Area | Primary source | Use |
|---|---|---|
| XPath syntax and types | [XPath 4.0 draft](https://qt4cg.org/specifications/xquery-40/xpath-40.html) | Operators, bindings, grammar, coercion, and compatibility. |
| Functions and operators | [F&O 4.0 draft](https://qt4cg.org/specifications/xpath-functions-40/Overview.html) | Signatures, arguments, results, errors, and function examples. |
| Stylesheet language | [XSLT 4.0 draft](https://qt4cg.org/specifications/xslt-40/Overview.html), [changes since 3.0](https://qt4cg.org/specifications/xslt-40/Overview.html#changes-since-3.0), [incompatibilities](https://qt4cg.org/specifications/xslt-40/Overview.html#incompatibilities) | Instruction behavior and migration differences. |
| Data model | [XDM 4.0 draft](https://qt4cg.org/specifications/xpath-datamodel-40/Overview.html) | Atomic items, nodes, maps, arrays, and JNodes. |
| Serialization | [Serialization 4.0 draft](https://qt4cg.org/specifications/xslt-xquery-serialization-40/Overview.html) | Output methods, parameters, and serialization errors. |
| Stable expression baseline | [XPath 3.1](https://www.w3.org/TR/xpath-31/), [F&O 3.1](https://www.w3.org/TR/xpath-functions-31/) | Earlier-version syntax and migration examples. |
| Stable stylesheet/model/output baseline | [XSLT 3.0](https://www.w3.org/TR/xslt-30/), [XDM 3.1](https://www.w3.org/TR/xpath-datamodel-31/), [Serialization 3.1](https://www.w3.org/TR/xslt-xquery-serialization-31/) | Portable worked chapters and baseline contracts. |
| Draft source and discussions | [QT4CG specification repository](https://github.com/qt4cg/qtspecs) | Source history, issues, pull requests, and adoption decisions. |
| Upstream 4.0 tests | [qt4tests](https://github.com/qt4cg/qt4tests), [xslt40-test](https://github.com/qt4cg/xslt40-test) | XPath/XQuery versus XSLT evidence; distinguish upstream tests from Bosak gate membership. |
| Upstream 3.x tests | [QT3](https://github.com/w3c/qt3tests), [xslt30-test](https://github.com/w3c/xslt30-test) | Regression baselines and XSLT 3.0 conformance evidence. |

Specification links establish language meaning; Bosak source, named tests, and
conformance runners establish implemented coverage. Deferred/rejected entries
link current alternatives, while their historical disposition must cite the
relevant issue/PR or archived draft when that evidence is supplied.

## 1. How to read this catalog

Each feature is one file, written against the template below. Fields may not be omitted, but may be *empty by declaration* — see the field contracts.

### 1.1 Entry template

1. **Name and Classification** — feature name + tier (**Frozen 4.0** / **Experimental** / **Deferred** / **Rejected–Superseded** / **Rejected–Never Existed**), the spec section, and the REQ slice/PR that landed it (traceability — every entry links back to its delivery).
2. **Intent** — one paragraph: what the construct does.
3. **Also Known As** — spec PR/issue numbers and former names. The 4.x drafts rename things; this field is the antidote.
4. **History / Supersedes** — which 1.x–3.x idiom this replaces, with a migration pair. If the feature is genuinely new, declare: *"None — new capability; 3.1 workaround: …"* — emptiness must be informative, not accidental.
5. **Motivation** — the problem in 4.x terms: why the pre-existing idioms were not enough.
6. **Language Alignment** — prior art in other languages (F#, Haskell, JS, Python…), **including semantic-drift warnings** where XPath deliberately differs from the language it borrowed from (argument position, null-vs-empty, renames).
7. **Applicability** — when to use it; when *not* (library code targeting 3.1, hot paths, …).
8. **Structure** — the grammar fragment or syntax shape.
9. **Participants** — how Bosak models it: parser flag, AST node(s), opcodes, version-gate entry.
10. **Collaborations** — interactions with other features (e.g., destructuring × typed-let coercion × lexical scoping).
11. **Consequences** — benefits *and* liabilities, including any deliberate breaking change.
12. **Implementation** — files, versions, and where the version gate rejects (XPST0003 at 3.1, XPST0017, …).
13. **Sample Code** — a worked real-world case with input, complete code, required context, expected output, compatibility level, and a meaningful edge case; syntax probes supplement the case and cover each form.
14. **Performance Notes** — cost class (zero-cost desugar / small constant overhead / hot-path relevant), plus measured deltas **only** when they carry revision + date from `benchmarks/Bosak.Benchmarks` (BenchmarkDotNet). Ad-hoc timings do not belong here; the harness owns numbers.
15. **Known Uses** — the qt4tests sets that pin the behavior and their gate status.
16. **Related Features** — cross-links to other catalog entries.

For book-style reading, begin with Intent, Motivation, Applicability, and Sample Code, then use History, Consequences, and Related Features to evaluate alternatives. Participants, Collaborations, and Implementation provide a separate contributor reference. Authors should open with a practical scenario and link its runnable case before presenting engine details. The numbered fields remain stable for traceability; a chapter can introduce them in the order that best teaches the task.

### 1.2 Compatibility tiers

| Tier | `XPathCompatibility` | Meaning |
|------|----------------------|---------|
| **Frozen 4.0** | `XPath40` (40) | Stabilized draft features. Static XPST0003/XPST0017 at `XPath31`; legal at `XPath40`. |
| **Experimental** | `XPath40Experimental` (50) | Not-yet-stabilized additions (post-June-2026 churn-risk sections). XPST0017 at *both* 3.1 and frozen 4.0. Semantics may change as the draft moves. |
| **Deferred** | — | Spec sections still moving; deliberately not implemented. Entries exist so the decision is recorded. |
| **Rejected** | — | Superseded renames and shapes that never existed; implemented old shapes are a defect. |

The 3.1 default is untouched by every tier: consumers on `XPath31` see zero change.

### 1.3 Structural rules

1. One feature = one file, named short-kebab (no numbering — the index owns ordering).
2. Every entry links back to its REQ slice/PR (field 1).
3. The index table (§2) is updated **in the same PR** as any entry add, move, or tier change. No exceptions.
4. Entries are never deleted when a feature is rejected or superseded — the file moves to `rejected/` and the index records the disposition.

## 2. Index

The [completeness audit](COMPLETENESS-AUDIT.md) records acceptance gaps and the
prioritized completion plan, originally against the published 1.1.1 engine and
now reviewed against 1.1.2. Its
[function inventory](audit-function-inventory.json) distinguishes registered names
from fully documented signatures; the catalog remains In Progress.

67 entries now include practical cases or stable alternatives. The [1,286-case manifest](examples/cases.json) records exact files, modes, results, and errors. The catalog is not a claim of complete coverage of every shipped 4.0 function or every deferred draft shape.

### Operators and syntax

| Feature | Tier | Delivery / evidence |
|---|---|---|
| [Destructuring let](operators-and-syntax/destructuring-let.md) | Frozen 4.0 | REQ-123 / PR #129 |
| [Method-call operator](operators-and-syntax/method-call-operator.md) | Deferred | REQ-118 deferred surface |
| [Otherwise operator](operators-and-syntax/otherwise-operator.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Pipeline operator](operators-and-syntax/pipeline-operator.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Mapping arrow](operators-and-syntax/mapping-arrow.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Keyword arguments](operators-and-syntax/keyword-arguments.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [String templates](operators-and-syntax/string-templates.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Array member iteration](operators-and-syntax/array-member-iteration.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Map entry iteration](operators-and-syntax/map-entry-iteration.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Focus-based constructors](operators-and-syntax/focus-functions.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |

### Functions

| Feature | Tier | Delivery / evidence |
|---|---|---|
| [CSV parsing](functions/csv-parsing.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Element-to-map conversion](functions/element-to-map.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Map-to-element conversion](functions/map-to-element.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Atomic equality](functions/atomic-equal.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Comparator-based sorting](functions/sort-with.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Typed measurement comparison](functions/measurement-comparison.md) | Frozen 4.0 changed contract | Mixed numeric boundaries, 3.1 policy and keyword drift; 27 cases |
| [Batch extrema and component codes](functions/batch-extrema.md) | Frozen 4.0 changed contracts | Decimal batch range, QName endpoints, 3.1 folds and observed limits; 29 cases |
| [Warehouse pick sorting](functions/warehouse-pick-sorting.md) | Retained 3.1 / changed 4.0 contracts | Stable composite keys, array member payloads and observed limits; 28 cases |

### Map/array and sequence-selection families

| Feature | Tier | Delivery / evidence |
|---|---|---|
| [Product indexing with maps](functions/product-map-index.md) | Frozen 4.0; source-1.1.2 result-shape correction | [#84](https://github.com/Fytala-Charles/Bosak/pull/84); 15 exact examples and named unit tests |
| [Import batches with arrays](functions/import-array-batches.md) | Frozen 4.0 | [#84](https://github.com/Fytala-Charles/Bosak/pull/84); 11 exact examples and named unit tests |
| [Order-export sequence selection](functions/order-sequence-selection.md) | Frozen 4.0 | [#82](https://github.com/Fytala-Charles/Bosak/pull/82); 14 exact examples and named unit tests |

These entries include complete synthetic inputs, exact output text, 3.1
comparisons and edge contracts. The map entry distinguishes published 1.1.1 behavior from the corrected
source-1.1.2 sequence contracts and the pinned draft. Current coverage is **778 feature checks plus
24 worked-chapter checks (802 total)**. See the [audit](COMPLETENESS-AUDIT.md)
for remaining signature/grammar coverage and engine follow-up findings.

### Shipment history and sequence contracts

| Feature | Tier | Delivery / evidence |
|---|---|---|
| [Shipment-event sequence checks](functions/shipment-event-sequences.md) | Frozen 4.0 | [#83](https://github.com/Fytala-Charles/Bosak/pull/83); 27 exact examples, named tests and pinned specification source |

This entry explains identity consistency, contiguous required steps and event
boundaries with complete carrier-history inputs and report outputs. The empty
history, missing identifiers/codes, unknown events, record comparators and
XPath 3.1 comparison make the acceptance contract explicit.

### Type system

| Feature | Tier | Delivery / evidence |
|---|---|---|
| [Enumeration types](type-system/enum-types.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Choice item types](type-system/choice-types.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Structural record types](type-system/structural-records.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Typed let coercion](type-system/typed-let-coercion.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |

### Data model

| Feature | Tier | Delivery / evidence |
|---|---|---|
| [JNode navigation](data-model/jnode-navigation.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |

### XSLT instructions

| Feature | Tier | Delivery / evidence |
|---|---|---|
| [xsl:note](xslt-instructions/xsl-note.md) | Bosak XSLT 4.0 surface | REQ-118 / REQ-123; linked primary tests and executable cases |
| [xsl:if then/else](xslt-instructions/xsl-if.md) | Bosak XSLT 4.0 surface | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Instruction separators](xslt-instructions/instruction-separators.md) | Bosak XSLT 4.0 surface | REQ-118 / REQ-123; linked primary tests and executable cases |
| [xsl:map selection and duplicate policy](xslt-instructions/xsl-map.md) | Bosak XSLT 4.0 surface | REQ-118 / REQ-123; linked primary tests and executable cases |
| [xsl:array and xsl:array-member](xslt-instructions/xsl-array.md) | Bosak XSLT 4.0 surface | REQ-118 / REQ-123; linked primary tests and executable cases |
| [xsl:switch](xslt-instructions/xsl-switch.md) | Bosak XSLT 4.0 surface | REQ-118 / REQ-123; linked primary tests and executable cases |

### Experimental

| Feature | Tier | Delivery / evidence |
|---|---|---|
| [fn:scan](experimental/fn-scan.md) | Experimental | REQ-123 S1 |

### Deferred

| Feature | Tier | Evidence |
|---|---|---|
| [XQuery and streaming scope](deferred/language-streaming-scope.md) | Separate 4.0 deltas | Existing query/streaming APIs; 28 scope and invoice checks |
| [External-input function boundaries](deferred/external-input-functions.md) | Out of scope; absent | Five contracts/ownership decisions; 55 refusal and narrower alternative checks |
| [Serialization 4.0 parameters](deferred/serialization-parameters.md) | Deferred, scoped XSLT solidus support | Nine parameter additions; 52 boundary and portable invoice checks |
| [fn:apply-templates](deferred/fn-apply-templates.md) | Deferred | REQ-118 tier 4; 28 refusal and invoice alternative checks |
| [Capturing accumulators](deferred/capturing-accumulators.md) | Deferred | REQ-118 tier 4; 16 capture identity and supported alternative checks |
| [Enclosing modes](deferred/enclosing-modes.md) | Deferred | REQ-118 tier 4; 20 refusal and named-mode dispatch checks |
| [xsl:item-type](deferred/xslt-item-type.md) | Deferred | REQ-118 tier 4; named alias use and portable alternatives |
| [xsl:record-type](deferred/xslt-record-type.md) | Deferred | REQ-118 tier 4; named constructor and structural binding boundaries |
| [Computed XPath node constructors](deferred/computed-node-constructors.md) | Deferred in XPath | REQ-118 tier 2; 30 refusal/alternative checks |

### Rejected / superseded

| Feature | Tier | Delivery / evidence |
|---|---|---|
| [Mapping arrow =>>](rejected/mapping-arrow-gtgt.md) | Rejected / renamed | REQ-118 syntax audit |

### Added routing families and disposition records

| Feature | Tier | Delivery / evidence |
|---|---|---|
| [Higher-order selection](functions/higher-order-selection.md) | Frozen 4.0 | [#88](https://github.com/Fytala-Charles/Bosak/pull/88); named evidence and executable cases |
| [Consecutive partitioning](functions/consecutive-partition.md) | Frozen 4.0 | [#88](https://github.com/Fytala-Charles/Bosak/pull/88); named evidence and executable cases |
| [Ordered queue prefix](functions/ordered-queue-prefix.md) | Frozen 4.0 | [#88](https://github.com/Fytala-Charles/Bosak/pull/88); 18 exact queue and callback examples |
| [Bounded retry state](functions/bounded-retries.md) | Frozen 4.0 | [#88](https://github.com/Fytala-Charles/Bosak/pull/88); 25 exact state, budget and callback examples |
| [Product dependency closure](functions/product-dependency-closure.md) | Frozen 4.0 | [#88](https://github.com/Fytala-Charles/Bosak/pull/88); 15 exact graph and validation examples |
| [Configured label callbacks](functions/configured-label-callbacks.md) | Frozen 4.0 | [#88](https://github.com/Fytala-Charles/Bosak/pull/88); 22 exact binding and identity examples |
| [Month-end cutoffs](functions/month-end-cutoffs.md) | Frozen 4.0 | [#84](https://github.com/Fytala-Charles/Bosak/pull/84); 19 calendar, timezone and component examples |
| [Timestamp deadlines](functions/timestamp-deadlines.md) | Frozen 4.0 | [#84](https://github.com/Fytala-Charles/Bosak/pull/84); 17 unit, duration and budget examples |
| [Archive and partner URI policy](functions/uri-option-policy.md) | Frozen 4.0 | #84; 28 option and precedence cases |
| [Invoice API URIs](functions/invoice-api-uris.md) | Frozen 4.0 | [#84](https://github.com/Fytala-Charles/Bosak/pull/84); 28 component, encoding and integration examples |
| [Customer text cleanup](functions/customer-text-cleanup.md) | Frozen 4.0 | [#82](https://github.com/Fytala-Charles/Bosak/pull/82), [#83](https://github.com/Fytala-Charles/Bosak/pull/83); 22 cleanup/Unicode/padding cases and segmentation limits |
| [Import file names](functions/import-file-names.md) | Frozen 4.0 | [#83](https://github.com/Fytala-Charles/Bosak/pull/83); 13 delimiter/position cases |
| [Supplier offer ranking](functions/supplier-offer-ranking.md) | Frozen 4.0 | Boundary selection, stable multi-key sorting and collation preflight; 16 cases |
| [Numeric import diagnostics](functions/numeric-import-diagnostics.md) | Frozen 4.0 | NaN, atomic reflection and operator callbacks; 15 cases |
| [Payload fingerprints](functions/payload-fingerprints.md) | Frozen 4.0 | Explicit hash input and algorithm contracts; 10 cases |
| [Sequence identity checks](functions/sequence-distinctness.md) | Frozen 4.0 | [#83](https://github.com/Fytala-Charles/Bosak/pull/83); named evidence and executable cases |
| [Nominative record types](type-system/nominative-records.md) | Deferred | [#77](https://github.com/Fytala-Charles/Bosak/pull/77); named evidence and executable cases |
| [Iteration across collection sequences](operators-and-syntax/sequence-collection-iteration.md) | Frozen 4.0 | [#87](https://github.com/Fytala-Charles/Bosak/pull/87); named evidence and executable cases |
| [JNode model and template boundary](data-model/jnode-template-boundary.md) | Deferred remainder; partial Frozen 4.0 | [#116](https://github.com/Fytala-Charles/Bosak/pull/116); named evidence and executable cases |
| [Never-adopted quantifier names](rejected/obsolete-quantifier-functions.md) | Rejected | [#77](https://github.com/Fytala-Charles/Bosak/pull/77); named evidence and executable cases |
| [Directional scan names](rejected/directional-scan.md) | Rejected / superseded | [#77](https://github.com/Fytala-Charles/Bosak/pull/77); named evidence and executable cases |
| [XPath switch expression](rejected/xpath-switch-expression.md) | Rejected in XPath | [#77](https://github.com/Fytala-Charles/Bosak/pull/77); named evidence and executable cases |
| [Extensible map type proposal](rejected/extensible-map-types.md) | Rejected proposal; record alternatives shipped | [#77](https://github.com/Fytala-Charles/Bosak/pull/77); named evidence and executable cases |

The [provenance ledger](PROVENANCE.md) supplies exact Bosak delivery revisions and named tests for the 28 baseline entries; historical immutable adopted-specification snapshots remain unavailable.

## 3. Benchmark provenance

Measured performance claims in this catalog must cite `benchmarks/Bosak.Benchmarks` (BenchmarkDotNet) output and carry the git revision + measurement date. Qualitative cost classes (zero-cost desugar / constant overhead / hot-path relevant) need no citation but must be stated as expectations, not measurements. See field 14 contract in §1.1.

*Last updated: 2026-10-11: four complete worked chapters; 38 catalog entries; 104 catalog checks plus 24 chapter checks. Remaining feature inventory is still in progress.*

[Ordered queue prefix](functions/ordered-queue-prefix.md) supplies take-while/drop-while with complete input/output, an XPath 3.1 comparison and 18 boundary/callback cases. Current coverage: 67 entries, 778 feature checks plus 24 chapter checks (802 total), and 117 assigned / 0 unassigned registered names. REQ-126 remains In Progress; source/public version remains 1.1.2. Bounded loop processing is now supplied by the reference below; dependency traversal and partial application are now supplied below.

[Bounded retry state](functions/bounded-retries.md) documents while-do/do-until using invoice-submission response fixtures, complete input/output, a bounded XPath 3.1 fold, and 25 executable normal/edge/error cases. It explains precondition versus postcondition evaluation, explicit attempt limits, zero-budget guarding, and whole-state callback values. Current coverage is 67 entries and 802 documentation checks (778 feature checks plus 24 chapter checks); 117 of 117 registered names have entries, leaving 0. REQ-126 remains In Progress; source/public version stays 1.1.2. Dependency traversal and partial application are supplied by this delivery.

[Product dependency closure](functions/product-dependency-closure.md) and [configured label callbacks](functions/configured-label-callbacks.md) add two complete references for transitive-closure, partial-apply and identity, with full input/output, XPath 3.1 comparisons and 37 executable cases. Shared dependencies, cycles, node identity/document order, unresolved references, argument order and callback binding limits are explicit. Current coverage: 67 entries, 778 feature checks plus 24 chapter checks (802 total), and 117 assigned / 0 unassigned registered names. REQ-126 remains In Progress; source/public version stays 1.1.2.

[Month-end cutoffs](functions/month-end-cutoffs.md) and [timestamp deadlines](functions/timestamp-deadlines.md) add full scheduling references for build-dateTime, days-in-month, unix-dateTime, seconds and duration-to-seconds. Complete inputs/reports, XPath 3.1 comparisons and 36 executable cases explain Gregorian leap rules, numeric timezone offsets, stamp types, epoch milliseconds, signed/fractional durations and explicit budget boundaries. Coverage is 67 entries, 778 feature checks plus 24 chapter checks (802 total), and 117 assigned / 0 unassigned registered names. REQ-126 remains In Progress; source/public stays 1.1.2. URI parsing/construction and encoding are supplied by the reference below.

[Invoice API URIs](functions/invoice-api-uris.md) supplies parse-uri, build-uri and decode-from-uri with full request input/output, a scoped XPath 3.1 construction comparison and 28 executable cases. It explains segment boundaries, query form decoding versus literal plus, repeated keys, permissive malformed-input decoding, component precedence and textual roundtrip limits. Coverage: 67 entries, 778 feature checks plus 24 chapter checks (802 total), and 117 assigned / 0 unassigned names. REQ-126 remains In Progress; source/public stays 1.1.2. Unicode/text cleanup and substring references are supplied below.

[Customer text cleanup](functions/customer-text-cleanup.md) and [import file names](functions/import-file-names.md) add eight function names with full input/output, scoped XPath 3.1 comparisons and 35 executable cases. They distinguish codepoints from grapheme clusters and display width, preserve interior whitespace, and explain final-delimiter/overlap rules. Bosak grapheme segmentation is approximate; complete Unicode segmentation remains outside the verified coverage. Current coverage: 67 entries, 778 feature checks plus 24 chapter checks (802 total), and 117 assigned / 0 unassigned registered names. REQ-126 remains In Progress; source/public stays 1.1.2. Ranking, numeric diagnostics and utility references are supplied below.

[Supplier offer ranking](functions/supplier-offer-ranking.md), [numeric import diagnostics](functions/numeric-import-diagnostics.md) and [payload fingerprints](functions/payload-fingerprints.md) add the final eight registered function names with 41 executable cases. Current coverage: 67 entries, 778 feature checks plus 24 chapter checks (802 total); all 117 registered names have family entries. This completes name assignment, not REQ-126 acceptance: signatures/options, grammar/disposition provenance and conformance limits remain. Computed-key boundary ties require explicit filtering in the business example and follow-up specification/engine reconciliation. Source/public stays 1.1.2; REQ-126 remains In Progress.

[Supplementary coverage audit](SUPPLEMENTARY-COVERAGE-AUDIT.md) (2026-10-12) records registered arities, changed existing-function contracts and wider grammar/disposition gaps. REQ-126 remains In Progress; current coverage is 67 entries and 802 checks. Typed measurement comparison with `fn:compare` is now supplied; min/max references are now supplied; sorting references are now supplied; URI option contracts and component precedence are now supplied; next: catalog close-out and prioritized follow-up review. Source/public version remains 1.1.2.

[Typed measurement comparison](functions/measurement-comparison.md) adds 27 executable cases for changed `fn:compare` behavior, with complete input/output and an explicit 3.1 double-normalization policy that differs at a decimal boundary. The runner verifies declared default-mode results or evaluation errors for existing functions. Keyword drift (`comparand1`/`comparand2` versus reviewed `value1`/`value2`) and legacy binary/empty-collation behavior are explicit. Current coverage: 67 entries, 778 feature plus 24 chapter checks (802 total); the 117-name assignment is unchanged. REQ-126 remains In Progress; source/public stays 1.1.2. Min/max references are now supplied; sorting references are now supplied; URI option contracts and component precedence are now supplied; next: catalog close-out and prioritized follow-up review.

[Batch extrema and component codes](functions/batch-extrema.md) adds 29 executable cases for `fn:min`/`fn:max`: complete batch input/output, explicit decimal normalization, namespace/local-name QName boundaries and a 3.1 fold comparison. Mixed numeric promotion/result-type and plain-duration limits remain explicit observed gaps against the pinned draft; these fixtures do not claim conformance. Current coverage: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 registered names remain assigned. REQ-126 remains In Progress; source/public stays 1.1.2. Sorting references are now supplied; URI option contracts and component precedence are now supplied; next: catalog close-out and prioritized follow-up review.

[Warehouse pick sorting](functions/warehouse-pick-sorting.md) adds 28 cases for retained `fn:sort`/`array:sort`: complete input/output, a portable 3.1 comparison, stable composite location keys and preserved multi-item array members. Mixed numeric promotion, input-dependent NaN order and missing empty-key defaults and unknown-collation acceptance are explicit observed boundaries. Exact fn-sort/array-sort gate membership is recorded without claiming full conformance. Coverage: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names remain assigned. REQ-126 stays In Progress; source/public remains 1.1.2. URI option policy is now supplied; next: catalog close-out and prioritized follow-up review.

**Historical session close (2026-10-12, before URI review):** PR #173 merged after CI passed. Current reference coverage is 57 entries and 499 documentation checks (475 feature plus 24 chapter); all 117 registered names are assigned. Latest implementation validation: 4,497 tests passed across ten assemblies. REQ-126 remains In Progress; released/source version remains 1.1.2. Resume with URI options and component precedence; see [the handover](../AGENT_HANDOVER.md).


**URI option review — 2026-10-12:** [Archive and partner URI policy](functions/uri-option-policy.md) adds complete input/output, a scoped 3.1 comparison and 28 executable cases. Boolean options, UNC spelling, raw/structured authority and path/query precedence are explicit. Non-Boolean option truthiness and empty-query-map fallback remain observed draft gaps. Coverage: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names remain assigned. REQ-126 stays In Progress; source/public version remains 1.1.2. Next: catalog close-out and prioritized follow-up review.

**Computed XPath constructor disposition — 2026-10-12:** [Deferred node construction](deferred/computed-node-constructors.md) separates the draft XPath feature from Bosak XQuery 3.1 support. Complete invoice input, refused XPath, 3.1 report and XSLT receipt output accompany 30 executable checks: 25 refusals across compatibility modes, three reports and two XML outputs. Current coverage: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names remain assigned. Version stays 1.1.2; REQ-126 remains In Progress. Named XSLT declaration dispositions are now supplied; enclosing modes are now supplied; capturing accumulator disposition is now supplied; fn:apply-templates disposition is now supplied; next: catalog close-out and prioritized follow-up review.

**Named XSLT declaration review — 2026-10-12:** [xsl:item-type](deferred/xslt-item-type.md) and [xsl:record-type](deferred/xslt-record-type.md) add full invoice cases, pinned disposition evidence and 28 executable boundary/alternative checks. Unused 4.0 declarations are ignored, not implemented; named bindings and constructors fail. Portable map validation and inline enums are verified. Structural-record XSLT `@as` raises XTTE0570; explicit XPath cast in `select` plus `as="map(*)"` is the verified workaround. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names assigned. REQ-126 remains In Progress, version 1.1.2. Next: catalog close-out and prioritized follow-up review.

**Enclosing-mode review — 2026-10-12:** [Deferred enclosing modes](deferred/enclosing-modes.md) supplies a full invoice export/audit case, pinned draft/source evidence and 20 executable checks. Nested mode templates refuse with XTSE0260; top-level named rules provide supported dispatch, without enclosing-rule isolation. Explicit mode, #current and lexical defaults, empty/reordered input and namespace identity are verified. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names remain assigned. Version remains 1.1.2; REQ-126 In Progress. Next: catalog close-out and prioritized follow-up review.

**Capturing-accumulator review — 2026-10-12:** [Deferred capturing accumulators](deferred/capturing-accumulators.md) adds full invoice input/output and 16 executable cases. Accepted capture=yes retains source identity rather than a draft-required snapshot; invalid/start-phase capture is tolerated in tested 4.0 forms. Ordinary accumulators, explicit snapshots and scalar projection are verified separately in memory, without streaming or special annotation-copy claims. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names assigned. REQ-126 remains In Progress, version 1.1.2. Next: catalog close-out and prioritized follow-up review.

**fn:apply-templates review — 2026-10-12:** [Deferred function and invoice alternatives](deferred/fn-apply-templates.md) adds 28 executable checks. Calls/references refuse with XPST0017; instruction availability is separate from function availability. Full invoice input/output verifies named-mode dispatch and a stylesheet-function wrapper with ordinary/tunnel parameters, empty/reordered input and explicit joining. Draft dynamic options/public-mode validation remain deferred. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names assigned. Version 1.1.2, REQ-126 In Progress. Next: catalog close-out and prioritized follow-up review.

**Serialization parameter review — 2026-10-12:** [Nine draft additions and invoice export](deferred/serialization-parameters.md) adds 52 executable cases. XSLT JSON solidus control works; fn:serialize ignores the new keys. JSON Lines, canonicalization, layout controls and CSV serialization remain deferred; fn CSV fallback and XSLT sequence-as-array are explicit boundaries. Complete portable JSON Lines input/output includes empty/reordered cases. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names assigned. Version 1.1.2, REQ-126 In Progress. Next: catalog close-out and prioritized follow-up review.

**External-input function review — 2026-10-12:** [Five function boundaries and partner invoices](deferred/external-input-functions.md) adds 55 checks. parse-html, html-doc, invisible-xml, unparsed-binary and xsd-validator remain absent/out of the original implementation scope. Pinned contracts and per-function dependency/ownership rationale accompany complete normalized XML, fixed-record, binary-value and business-guard alternatives. These do not implement HTML recovery, arbitrary grammar parsing, binary acquisition or XSD factory validation. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 registered names assigned. Version 1.1.2, REQ-126 In Progress. Next: catalog close-out and prioritized follow-up review.

**GNode/JNode scope review — 2026-10-12:** [Partial model and invoice navigation](data-model/jnode-template-boundary.md) replaces blanket exclusion with shipped JNode support and a deferred remainder. Eighteen added checks verify full invoice navigation/portable reports and broader node()/xnode()/identity gaps. Full generic-model and stylesheet built-in behavior remain uncertified. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 registered names assigned. Version 1.1.2, REQ-126 In Progress. Next: catalog close-out and prioritized follow-up review.

**Language/streaming scope review — 2026-10-12:** [Existing APIs and separate 4.0 deltas](deferred/language-streaming-scope.md) adds 28 cases through XQuery, XPath, ordinary XSLT and actual streaming APIs. Corrects stale XPath where exclusion and distinguishes REQ-087 Done from historical residual reports. Full XQuery/Streaming 4.0 remains unclaimed. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names assigned. Version 1.1.2, REQ-126 In Progress. Next: catalog close-out and prioritized follow-up review.


Catalog constructor audit (2026-10-12): **67 entries, 1,286 feature + 24 chapter checks = 1,310**; four worked chapters and all 117 registered names assigned. REQ-126 remains In Progress, version 1.1.2. The [prioritized review](CLOSEOUT-REVIEW.md) identifies constructor/signature/grammar and acceptance work, with engine follow-ups tracked separately. All 49 focus constructors now have representative lexical/type/context/error checks and paired delivery-import examples. Next: D2, supplementary signatures, keywords, defaults and options.
