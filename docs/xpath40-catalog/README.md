<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak XPath 4.0 feature catalog">
  <br><br>
  <h1>The Bosak XPath/XSLT 4.0 Feature Catalog</h1>
  <p>Structured reference for every XPath/XSLT 4.0 syntax decision in Bosak — where each feature comes from, why it exists, what it replaces, and what it costs</p>
</div>

<!-- The Bosak XPath/XSLT 4.0 Feature Catalog -->
<!-- Living document: one file per feature; index updated in the same PR as any entry. -->

> **Purpose:** A catalog, in the spirit of the Design Patterns template (Gamma, Helm, Johnson, Vlissides, pp. 6–7), of every XPath/XSLT 4.0 feature Bosak ships, defers, or rejects. **Audience:** engine users choosing which 4.x idioms to adopt, Bosak.Braid (which syntax it may generate per compatibility level), and contributors extending the engine. **Living document:** last updated 2026-10-11. **Governing REQ:** [REQ-126](../FEATURE_REQUESTS.md); adoption plan and stability tiers: [REQ-118 dossier](../REQ-118-xpath-xslt-40.md); experimental track: [REQ-123](../FEATURE_REQUESTS.md).

## Direction: a book and a working reference

This catalog is intended to develop into a book or reference guide. Readers should be able to follow chapters to learn how features solve document-processing problems, or look up a single entry while implementing a transformation. The catalog now contains 28 entries and three worked chapters. Further coverage of shipped functions and deferred draft decisions remains REQ-126 work.

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

[Supplier invoices to ERP XML](worked-examples/invoice-to-erp/README.md) is the first complete case: a 3.0 stylesheet, realistic inputs, expected principal/exception outputs, and seven executable checks. It includes a separate verified 3.1/4.0 map-destructuring comparison. Continue with [XML manual publishing](worked-examples/manual-to-html/README.md) and [order reconciliation](worked-examples/order-reconciliation/README.md). The [example runner](examples/README.md) verifies 81 focused catalog cases and migration baselines.

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

28 entries now include practical cases or stable alternatives. The [81-case manifest](examples/cases.json) records exact files, modes, results, and errors. The catalog is not a claim of complete coverage of every shipped 4.0 function or every deferred draft shape.

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

### Rejected / superseded

| Feature | Tier | Delivery / evidence |
|---|---|---|
| [Mapping arrow =>>](rejected/mapping-arrow-gtgt.md) | Rejected / renamed | REQ-118 syntax audit |

## 3. Benchmark provenance

Measured performance claims in this catalog must cite `benchmarks/Bosak.Benchmarks` (BenchmarkDotNet) output and carry the git revision + measurement date. Qualitative cost classes (zero-cost desugar / constant overhead / hot-path relevant) need no citation but must be stated as expectations, not measurements. See field 14 contract in §1.1.

*Last updated: 2026-10-11: three complete worked chapters; 28 catalog entries; 81 catalog checks plus 13 chapter checks. Remaining feature inventory is still in progress.*
